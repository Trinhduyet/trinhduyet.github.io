using WorkflowLab;

var builder = WebApplication.CreateBuilder(args);
var dbConnection = builder.Configuration.GetConnectionString("LabDb")
    ?? throw new InvalidOperationException("ConnectionStrings:LabDb is required.");
var apiKey = builder.Configuration["LabApiKey"]
    ?? throw new InvalidOperationException("LabApiKey is required; use a local-only lab credential.");

builder.Services.AddSingleton(new LabStore(dbConnection));
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<InboxConsumer>();
var app = builder.Build();

// This is a deliberately scoped LAB key, not production OAuth2/OIDC/tenant auth.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path == "/health") { await next(); return; }
    if (ctx.Request.Headers["X-Lab-Key"] != apiKey)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }
    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "alive" }));
app.MapPost("/lab/accounts", async (SeedInput input, LabStore store, CancellationToken ct) =>
{
    if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("AllowLabSeeding"))
        return Results.Forbid();
    if (string.IsNullOrWhiteSpace(input.AccountId) || input.AccountId.Length > 64 || input.Quantity < 0)
        return Results.BadRequest("Invalid account or capacity.");
    await store.SeedAsync(input.AccountId, input.Quantity, ct);
    return Results.Ok(new { seeded = input.AccountId });
});
app.MapGet("/lab/accounts/{accountId}", async (string accountId, LabStore store, CancellationToken ct) =>
    await store.GetBalanceAsync(accountId, ct) is { } balance ? Results.Ok(balance) : Results.NotFound());

app.MapPost("/api/reservations", async (HttpRequest http, ReservationInput input, LabStore store, CancellationToken ct) =>
{
    var validation = ReservationRules.Validate(input);
    if (validation is not null) return Results.BadRequest(new { error = validation });
    var key = http.Headers["Idempotency-Key"].ToString();
    if (key.Length is < 1 or > 128) return Results.BadRequest(new { error = "Idempotency-Key required, 1-128 chars." });
    var (result, error) = await store.ReserveAsync(input, key, ct);
    if (error is not null) return Results.Conflict(new { error });
    return Results.Accepted($"/api/reservations/{result!.Id}", result);
});

app.MapGet("/api/reservations/{id:guid}", async (Guid id, LabStore store, CancellationToken ct) =>
    await store.FindAsync(id, ct) is { } item ? Results.Ok(item) : Results.NotFound());

app.MapPost("/lab/reconcile/{id:guid}", async (Guid id, ReconcileInput input, LabStore store, CancellationToken ct) =>
{
    if (input.Outcome is not ("confirmed" or "rejected")) return Results.BadRequest("Expected confirmed/rejected.");
    return await store.ReconcileAsync(id, input.Outcome == "confirmed", ct) ? Results.Ok() : Results.Conflict();
});

var storeAtStart = app.Services.GetRequiredService<LabStore>();
for (var attempt = 1; ; attempt++)
{
    try { await storeAtStart.InitializeAsync(CancellationToken.None); break; }
    catch (Exception ex) when (attempt < 30)
    {
        app.Logger.LogWarning(ex, "Waiting for SQL Server (attempt {Attempt})", attempt);
        await Task.Delay(2000);
    }
}

await app.RunAsync();

public sealed record SeedInput(string AccountId, int Quantity);
public sealed record ReconcileInput(string Outcome);
