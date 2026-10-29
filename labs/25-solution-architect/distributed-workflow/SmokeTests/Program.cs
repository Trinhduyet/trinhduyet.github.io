using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:8088";
var apiKey = Environment.GetEnvironmentVariable("LAB_API_KEY")
    ?? throw new InvalidOperationException("Set LAB_API_KEY to match compose .env.");
using var http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(10) };
http.DefaultRequestHeaders.Add("X-Lab-Key", apiKey);

static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception("FAIL: " + message);
}

async Task<string> NewAccountAsync(int capacity)
{
    var name = Guid.NewGuid().ToString("N");
    var response = await http.PostAsJsonAsync("/lab/accounts", new { accountId = name, quantity = capacity });
    response.EnsureSuccessStatusCode();
    return name;
}

async Task<(HttpStatusCode status, JsonElement? body)> ReserveAsync(
    string account, int qty, string simulation, string key)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/reservations")
    {
        Content = JsonContent.Create(new { accountId = account, quantity = qty, simulation })
    };
    request.Headers.Add("Idempotency-Key", key);
    using var response = await http.SendAsync(request);
    if (!response.IsSuccessStatusCode) return (response.StatusCode, null);
    var body = await response.Content.ReadFromJsonAsync<JsonElement>();
    return (response.StatusCode, body);
}

async Task<string> WaitStatusAsync(Guid id, string wanted)
{
    for (var i = 0; i < 120; i++)
    {
        var view = await http.GetFromJsonAsync<JsonElement>($"/api/reservations/{id}");
        var status = view.GetProperty("status").GetString()!;
        if (status == wanted) return status;
        await Task.Delay(250);
    }
    throw new TimeoutException($"Reservation {id} never reached {wanted}.");
}

async Task AssertBalanceAsync(string account, int available, int reserved)
{
    var view = await http.GetFromJsonAsync<JsonElement>($"/lab/accounts/{account}");
    Assert(view.GetProperty("available").GetInt32() == available, "Available balance mismatch");
    Assert(view.GetProperty("reserved").GetInt32() == reserved, "Reserved balance mismatch");
}

// Every caller uses a distinct key but all compete for the same authoritative capacity.
var account = await NewAccountAsync(10);
var claims = await Task.WhenAll(Enumerable.Range(0, 100)
    .Select(i => ReserveAsync(account, 8, "none", "claim-" + i)));
var successful = claims.Where(x => x.status == HttpStatusCode.Accepted).ToArray();
Assert(successful.Length == 1, "Exactly one of 100 concurrent claims should reserve 8/10");
Assert(claims.Count(x => x.status == HttpStatusCode.Conflict) == 99,
    "Other concurrent claims should receive 409, not errors or unexpected acceptance");
var id = successful[0].body!.Value.GetProperty("id").GetGuid();
await WaitStatusAsync(id, "Completed");
await AssertBalanceAsync(account, 2, 0);

var replay = await ReserveAsync(account, 8, "none", "claim-" + Array.FindIndex(claims, x => x.body?.GetProperty("id").GetGuid() == id));
Assert(replay.status == HttpStatusCode.Accepted, "Same-key replay accepted");
Assert(replay.body!.Value.GetProperty("id").GetGuid() == id, "Same-key replay same ID");
Assert(replay.body!.Value.GetProperty("replayed").GetBoolean(), "Replay flag set");
var conflict = await ReserveAsync(account, 9, "none", "claim-" + Array.FindIndex(claims, x => x.body?.GetProperty("id").GetGuid() == id));
Assert(conflict.status == HttpStatusCode.Conflict, "Same key/different payload must conflict");
await AssertBalanceAsync(account, 2, 0);

// External-effect ambiguity: hold resource until an explicit reconciliation result.
var unknownAccount = await NewAccountAsync(10);
var unknown = await ReserveAsync(unknownAccount, 7, "timeout", "unknown");
Assert(unknown.status == HttpStatusCode.Accepted, "Timeout scenario accepted");
var unknownId = unknown.body!.Value.GetProperty("id").GetGuid();
await WaitStatusAsync(unknownId, "Unknown");
await AssertBalanceAsync(unknownAccount, 3, 7);
var reconciled = await http.PostAsJsonAsync($"/lab/reconcile/{unknownId}", new { outcome = "rejected" });
Assert(reconciled.StatusCode == HttpStatusCode.OK, "Unknown reconciled as rejected");
await AssertBalanceAsync(unknownAccount, 10, 0);

var rejectAccount = await NewAccountAsync(10);
var rejected = await ReserveAsync(rejectAccount, 6, "reject", "reject");
await WaitStatusAsync(rejected.body!.Value.GetProperty("id").GetGuid(), "Rejected");
await AssertBalanceAsync(rejectAccount, 10, 0);

using var noAuth = new HttpClient { BaseAddress = new Uri(baseUrl) };
var forbidden = await noAuth.GetAsync($"/lab/accounts/{account}");
Assert(forbidden.StatusCode == HttpStatusCode.Unauthorized, "Missing lab key denied");
Console.WriteLine("PASS: 100 concurrent claims, replay, conflict, unknown/reconcile, rejection and auth smoke tests.");
