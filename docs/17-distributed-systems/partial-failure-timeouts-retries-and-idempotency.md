# Partial Failure, Timeouts, Retries và Idempotency

> [← Distributed Systems](README.md)

## Hiểu trong 5 phút

Trong local function call:

```text
method returned
→ biết result

method threw
→ biết failed
```

Qua network:

```text
request sent
↓
remote may execute
↓
response may be lost
↓
caller sees timeout
```

Timeout thường có nghĩa:

> **Caller không biết outcome**, không phải chắc chắn operation chưa chạy.

Đây là lý do retry và idempotency phải học cùng nhau.

---

# 1. Failure classification

| Failure | Example | Typical thinking |
| --- | --- | --- |
| validation/permanent | invalid payload | fail fast, no retry |
| authorization | 403 | no retry until identity/policy changes |
| not found | 404 | usually business result, not transient |
| rate limit | 429 | retry only respecting policy/backoff |
| transient dependency | temporary 503/network reset | bounded retry may help |
| timeout | unknown remote outcome | retry only if idempotent/dedup safe |
| overload | queue/CPU saturated | retry can make it worse |

Do not write:

```csharp
catch
{
    await RetryAsync();
}
```

---

# 2. Deadline before retry

Request budget:

```text
Total deadline = 2 seconds
```

If first attempt takes 1.8s, three retries each with 2s timeout violate caller budget.

Mental model:

```text
caller deadline
   ↓ remaining budget
attempt 1
   ↓ remaining budget
backoff
   ↓ remaining budget
attempt 2 only if useful
```

Code shape:

```csharp
public static async Task<T> WithDeadlineAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    TimeSpan timeout,
    CancellationToken cancellationToken)
{
    using var timeoutCts = CancellationTokenSource
        .CreateLinkedTokenSource(cancellationToken);

    timeoutCts.CancelAfter(timeout);

    return await operation(timeoutCts.Token);
}
```

---

# 3. Bounded retry with exponential backoff + jitter

Simplified learning code:

```csharp
public static async Task<T> RetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxAttempts,
    Func<Exception, bool> isTransient,
    CancellationToken cancellationToken)
{
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            return await operation(cancellationToken);
        }
        catch (Exception ex)
            when (attempt < maxAttempts && isTransient(ex))
        {
            var exponentialMs = 100 * Math.Pow(2, attempt - 1);
            var jitterMs = Random.Shared.Next(0, 100);
            var delay = TimeSpan.FromMilliseconds(
                Math.Min(exponentialMs + jitterMs, 2000));

            await Task.Delay(delay, cancellationToken);
        }
    }
}
```

Production có thể dùng resilience library, nhưng phải hiểu semantics trước library.

---

# 4. Retry amplification

Giả sử:

```text
API retries Service B 3 times
Service B retries DB 3 times
Gateway retries API 3 times
```

Một user request có thể tạo rất nhiều downstream attempts.

Conceptual worst case:

```text
3 × 3 × 3 = 27 attempts
```

Khi dependency đang overload, retry layers có thể tạo retry storm.

Architectural rule:

```text
Choose retry ownership deliberately.
Do not stack automatic retries everywhere.
```

---

# 5. Circuit breaker mental model

Retry handles occasional transient failure.

Circuit breaker handles a dependency that is failing enough that continued calls are wasteful/harmful.

```text
Closed
  calls flow
  failures accumulate
     ↓ threshold
Open
  fail fast
     ↓ recovery interval
Half-open
  limited trial calls
     ↓
success → Closed
failure → Open
```

Circuit breaker is not a recovery mechanism by itself. It reduces repeated pressure while dependency is unhealthy.

---

# 6. Timeout ≠ rollback

Scenario:

```text
API → payment service
payment service commits charge
response delayed
API times out
```

Caller cannot infer:

```text
"timeout → payment not charged"
```

Correct API design needs operation identity/idempotency or a status lookup/reconciliation path.

---

# 7. Idempotency key

Request:

```http
POST /orders
Idempotency-Key: 74e9ecce-2d72-43b3-8bc8-92a612778b92
```

Storage table:

```sql
CREATE TABLE dbo.IdempotencyRequests
(
    TenantId        int             NOT NULL,
    IdempotencyKey  varchar(100)    NOT NULL,
    RequestHash     varbinary(32)   NOT NULL,
    Status          varchar(20)     NOT NULL,
    ResponseCode    int             NULL,
    ResponseBody    nvarchar(max)   NULL,
    CreatedAt       datetime2       NOT NULL,
    CompletedAt     datetime2       NULL,

    CONSTRAINT PK_IdempotencyRequests
        PRIMARY KEY (TenantId, IdempotencyKey)
);
```

Primary key protects race on same tenant/key.

---

# 8. Request hash prevents key misuse

If client reuses same key with different body:

```text
Key: abc
Request 1: Amount = 10
Request 2: Amount = 10000
```

Silently returning first response is dangerous.

Store a canonical request hash and reject mismatched reuse.

Pseudo-code:

```csharp
if (existing.RequestHash != requestHash)
{
    throw new IdempotencyConflictException();
}
```

---

# 9. Idempotency transaction boundary

Naive sequence:

```text
check key missing
↓
perform side effect
↓
insert key
```

Two concurrent requests can both pass the check.

Need atomic ownership/reservation.

Example SQL shape:

```sql
BEGIN TRANSACTION;

INSERT INTO dbo.IdempotencyRequests
(
    TenantId,
    IdempotencyKey,
    RequestHash,
    Status,
    CreatedAt
)
VALUES
(
    @tenantId,
    @key,
    @hash,
    'Processing',
    SYSUTCDATETIME()
);

-- duplicate PK means another request owns this key

COMMIT;
```

Then business implementation must decide how `Processing` recovery works if process crashes.

---

# 10. Status endpoint / reconciliation

For long/uncertain operation, expose operation identity:

```http
POST /exports
→ 202 Accepted
Location: /operations/op-123
```

Then:

```http
GET /operations/op-123
```

Response:

```json
{
  "id": "op-123",
  "status": "Completed",
  "resultUrl": "/exports/file-456"
}
```

This gives caller a way to reconcile unknown outcome instead of blind retry.

---

# 11. Safe retry example: read

```csharp
var result = await RetryAsync(
    ct => catalogClient.GetProductAsync(productId, ct),
    maxAttempts: 3,
    isTransient: IsTransientHttpFailure,
    cancellationToken);
```

Still respect:

```text
caller deadline
429 Retry-After
circuit state
capacity
```

---

# 12. Unsafe retry example: side effect

```csharp
await RetryAsync(
    ct => paymentClient.ChargeAsync(request, ct),
    maxAttempts: 3,
    isTransient: _ => true,
    cancellationToken);
```

Danger if provider has no idempotency contract.

Safer design:

```csharp
await paymentClient.ChargeAsync(
    new ChargeRequest(
        IdempotencyKey: paymentAttemptId,
        Amount: amount),
    cancellationToken);
```

Then provider/application must guarantee semantics for duplicate key.

---

# 13. Bulkhead / concurrency limit

Even with timeout/retry, one slow dependency can consume all request concurrency.

Simple concurrency gate:

```csharp
public sealed class BoundedDependencyClient
{
    private readonly SemaphoreSlim _gate = new(initialCount: 20);

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> call,
        CancellationToken cancellationToken)
    {
        if (!await _gate.WaitAsync(
                TimeSpan.FromMilliseconds(100),
                cancellationToken))
        {
            throw new DependencyCapacityExceededException();
        }

        try
        {
            return await call(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
```

This is a learning example; libraries/platform policies may provide richer bulkhead/rate limiting.

---

# 14. Retry metrics

Track:

```text
requests
attempts
retry_count
retry_exhausted
circuit_open
latency by attempt
final outcome
```

If successful requests average 2.8 attempts, system is unhealthy even if final success rate looks good.

---

# 15. Failure drill — timeout after commit

Server endpoint:

```csharp
app.MapPost("/demo-orders", async (
    CreateOrderRequest request,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var order = new Order { /* ... */ };
    db.Orders.Add(order);
    await db.SaveChangesAsync(cancellationToken);

    // Simulate response lost/delayed AFTER durable side effect.
    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

    return Results.Ok(order.Id);
});
```

Client timeout 500ms and retry.

Without idempotency: duplicate Orders may appear.

Then add idempotency key and prove only one business Order exists.

---

# 16. Failure drill — retry storm

Fake dependency returns 503 for 30 seconds.

Run 100 concurrent requests with:

```text
no retry
3 retries fixed 10ms
3 retries exponential+jitter
```

Measure:

```text
total downstream attempts
P95 latency
CPU
error rate
recovery behavior
```

Goal: see retry load, not merely code backoff.

---

# 17. Failure drill — slow dependency and bulkhead

Dependency sleeps 10 seconds.

Without concurrency bound:

```text
request count grows
inflight calls grow
memory/thread/task pressure grows
```

With bound 20 + fail-fast/queue policy:

```text
blast radius bounded
```

Measure user-facing trade-off.

---

# 18. Review checklist

```text
[ ] Timeout means unknown outcome?
[ ] Operation idempotent/deduplicated?
[ ] Retry only transient failures?
[ ] Retry count and total deadline bounded?
[ ] Jitter used for many concurrent clients?
[ ] Retry ownership only at intended layer?
[ ] 429/Retry-After respected?
[ ] Circuit/bulkhead needed by failure data?
[ ] Idempotency key scoped to tenant/principal?
[ ] Same key + different payload rejected?
[ ] Processing/crash recovery defined?
```

---

# 19. Exit criteria

Bạn hoàn thành khi có thể:

- explain timeout as unknown outcome;
- classify permanent vs transient vs overload;
- implement bounded retry + jitter;
- explain retry amplification;
- implement idempotency key table/race protection;
- handle same-key different-request conflict;
- expose operation reconciliation when appropriate;
- bound dependency concurrency;
- run timeout/retry-storm/bulkhead failure drills.

## Official English Sources

- [Retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Circuit Breaker pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- [Transient fault handling](https://learn.microsoft.com/en-us/azure/architecture/best-practices/transient-faults)

## Verification metadata

- Verified: 2026-08-12.
- Concepts broker/framework-neutral; .NET examples are illustrative.
- Status: code-first v1.
