using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace WorkflowLab;

public sealed class LabStore(string connectionString)
{
    private SqlConnection Connection() => new(connectionString);

    public async Task InitializeAsync(CancellationToken ct)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var database = builder.InitialCatalog;
        if (!Regex.IsMatch(database, "^[A-Za-z0-9_]+$"))
            throw new InvalidOperationException("Use a simple lab database name.");
        builder.InitialCatalog = "master";
        await using (var master = new SqlConnection(builder.ConnectionString))
        {
            await master.OpenAsync(ct);
            await using var create = new SqlCommand("IF DB_ID(@name) IS NULL EXEC('CREATE DATABASE [' + @name + ']')", master);
            create.Parameters.AddWithValue("@name", database);
            await create.ExecuteNonQueryAsync(ct);
        }
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var schema = new SqlCommand(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "schema.sql")), conn);
        await schema.ExecuteNonQueryAsync(ct);
    }

    public async Task SeedAsync(string account, int quantity, CancellationToken ct)
    {
        if (account.Length is < 1 or > 64 || quantity < 0) throw new ArgumentException("Invalid seed input.");
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
            UPDATE dbo.LabAccounts SET Available = @quantity, Reserved = 0 WHERE AccountId = @account;
            IF @@ROWCOUNT = 0 INSERT dbo.LabAccounts(AccountId, Available, Reserved) VALUES (@account, @quantity, 0);
            """, conn);
        cmd.Parameters.AddWithValue("@account", account);
        cmd.Parameters.AddWithValue("@quantity", quantity);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<BalanceView?> GetBalanceAsync(string account, CancellationToken ct)
    {
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT Available, Reserved FROM dbo.LabAccounts WHERE AccountId=@account", conn);
        cmd.Parameters.AddWithValue("@account", account);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? new BalanceView(account, reader.GetInt32(0), reader.GetInt32(1)) : null;
    }

    public async Task<ReservationView?> FindAsync(Guid id, CancellationToken ct)
    {
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT AccountId, Quantity, Status FROM dbo.LabReservations WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? new ReservationView(id, reader.GetString(0), reader.GetInt32(1), reader.GetString(2))
            : null;
    }

    public async Task<(ReservationView? Reservation, string? Error)> ReserveAsync(
        ReservationInput input, string key, CancellationToken ct)
    {
        var hash = ReservationRules.PayloadHash(input);
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        // Serialize same-key claims on SQL, including when API has many replicas.
        await using (var appLock = new SqlCommand("""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource=@resource, @LockMode='Exclusive',
                @LockOwner='Transaction', @LockTimeout=10000;
            SELECT @result;
            """, conn, tx))
        {
            appLock.Parameters.AddWithValue("@resource", "reserve:" + input.AccountId + ":" + key);
            var result = (int)(await appLock.ExecuteScalarAsync(ct) ?? -999);
            if (result < 0) return (null, "Idempotency lock unavailable.");
        }

        await using (var existing = new SqlCommand("""
            SELECT Id, PayloadHash, Quantity, Status FROM dbo.LabReservations WITH (UPDLOCK)
            WHERE AccountId=@account AND IdempotencyKey=@key;
            """, conn, tx))
        {
            existing.Parameters.AddWithValue("@account", input.AccountId);
            existing.Parameters.AddWithValue("@key", key);
            await using var reader = await existing.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var id = reader.GetGuid(0);
                var same = reader.GetString(1) == hash;
                var quantity = reader.GetInt32(2);
                var status = reader.GetString(3);
                await reader.DisposeAsync();
                await tx.CommitAsync(ct);
                return same
                    ? (new ReservationView(id, input.AccountId, quantity, status, true), null)
                    : (null, "Same idempotency key with different payload.");
            }
        }

        await using (var reserve = new SqlCommand("""
            UPDATE dbo.LabAccounts
            SET Available = Available - @quantity, Reserved = Reserved + @quantity
            WHERE AccountId=@account AND Available >= @quantity;
            """, conn, tx))
        {
            reserve.Parameters.AddWithValue("@account", input.AccountId);
            reserve.Parameters.AddWithValue("@quantity", input.Quantity);
            if (await reserve.ExecuteNonQueryAsync(ct) != 1)
                return (null, "Insufficient capacity or account missing.");
        }

        var idNew = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        await using (var create = new SqlCommand("""
            INSERT dbo.LabReservations(Id,AccountId,IdempotencyKey,PayloadHash,Quantity,Status)
            VALUES (@id,@account,@key,@hash,@quantity,'Pending');
            INSERT dbo.LabOutbox(EventId,ReservationId,Simulation)
            VALUES (@eventId,@id,@simulation);
            """, conn, tx))
        {
            create.Parameters.AddWithValue("@id", idNew);
            create.Parameters.AddWithValue("@account", input.AccountId);
            create.Parameters.AddWithValue("@key", key);
            create.Parameters.AddWithValue("@hash", hash);
            create.Parameters.AddWithValue("@quantity", input.Quantity);
            create.Parameters.AddWithValue("@eventId", eventId);
            create.Parameters.AddWithValue("@simulation", input.Simulation);
            await create.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct);
        return (new ReservationView(idNew, input.AccountId, input.Quantity, "Pending"), null);
    }

    public async Task<WorkflowEvent?> NextOutboxAsync(CancellationToken ct)
    {
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
            SELECT TOP(1) EventId, ReservationId, Simulation
            FROM dbo.LabOutbox WHERE PublishedAt IS NULL ORDER BY CreatedAt, EventId;
            """, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? new WorkflowEvent(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2)) : null;
    }

    public async Task MarkPublishedAsync(Guid eventId, CancellationToken ct)
    {
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("UPDATE dbo.LabOutbox SET PublishedAt=SYSUTCDATETIME() WHERE EventId=@id", conn);
        cmd.Parameters.AddWithValue("@id", eventId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task ProcessEventAsync(WorkflowEvent message, CancellationToken ct)
    {
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await using (var inbox = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.LabInbox WITH (UPDLOCK, HOLDLOCK) WHERE EventId=@eventId)
            BEGIN INSERT dbo.LabInbox(EventId) VALUES (@eventId); SELECT 1; END
            ELSE SELECT 0;
            """, conn, tx))
        {
            inbox.Parameters.AddWithValue("@eventId", message.EventId);
            if ((int)(await inbox.ExecuteScalarAsync(ct) ?? 0) == 0)
            {
                await tx.CommitAsync(ct);
                return;
            }
        }

        await using var cmd = new SqlCommand("""
            DECLARE @account nvarchar(64), @quantity int, @status nvarchar(24);
            SELECT @account=AccountId, @quantity=Quantity, @status=Status
            FROM dbo.LabReservations WITH (UPDLOCK) WHERE Id=@id;
            IF @status='Pending'
            BEGIN
                UPDATE dbo.LabReservations SET Status=@next WHERE Id=@id;
                IF @next='Completed'
                    UPDATE dbo.LabAccounts SET Reserved=Reserved-@quantity WHERE AccountId=@account;
                IF @next='Rejected'
                    UPDATE dbo.LabAccounts SET Reserved=Reserved-@quantity,
                        Available=Available+@quantity WHERE AccountId=@account;
            END;
            """, conn, tx);
        cmd.Parameters.AddWithValue("@id", message.ReservationId);
        cmd.Parameters.AddWithValue("@next", message.Simulation switch
        {
            "timeout" => "Unknown",
            "reject" => "Rejected",
            _ => "Completed"
        });
        await cmd.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<bool> ReconcileAsync(Guid id, bool confirmed, CancellationToken ct)
    {
        await using var conn = Connection();
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await using var cmd = new SqlCommand("""
            DECLARE @account nvarchar(64), @quantity int;
            SELECT @account=AccountId, @quantity=Quantity FROM dbo.LabReservations WITH (UPDLOCK)
            WHERE Id=@id AND Status='Unknown';
            IF @account IS NULL SELECT 0;
            ELSE
            BEGIN
                UPDATE dbo.LabReservations SET Status=@next WHERE Id=@id AND Status='Unknown';
                IF @confirmed=1 UPDATE dbo.LabAccounts SET Reserved=Reserved-@quantity WHERE AccountId=@account;
                ELSE UPDATE dbo.LabAccounts SET Reserved=Reserved-@quantity, Available=Available+@quantity WHERE AccountId=@account;
                SELECT 1;
            END;
            """, conn, tx);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@next", confirmed ? "Completed" : "Rejected");
        cmd.Parameters.AddWithValue("@confirmed", confirmed);
        var changed = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0) == 1;
        await tx.CommitAsync(ct);
        return changed;
    }
}
