using System.Security.Cryptography;
using System.Text;

namespace WorkflowLab;

public sealed record ReservationInput(string AccountId, int Quantity, string Simulation = "none");
public sealed record ReservationView(Guid Id, string AccountId, int Quantity, string Status, bool Replayed = false);
public sealed record BalanceView(string AccountId, int Available, int Reserved);
public sealed record WorkflowEvent(Guid EventId, Guid ReservationId, string Simulation);

public static class ReservationRules
{
    public static string? Validate(ReservationInput request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountId) || request.AccountId.Length > 64 ||
            request.AccountId != request.AccountId.Trim())
            return "AccountId must contain 1-64 characters.";
        if (request.Quantity is <= 0 or > 1_000_000)
            return "Quantity must be 1-1,000,000.";
        if (request.Simulation is not ("none" or "reject" or "timeout"))
            return "Simulation must be none, reject, or timeout.";
        return null;
    }

    public static string PayloadHash(ReservationInput request)
    {
        var canonical = $"{request.AccountId.Trim()}\n{request.Quantity}\n{request.Simulation}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
