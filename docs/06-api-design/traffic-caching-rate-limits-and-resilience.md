# API Traffic, Caching, Rate Limits và Resilience

> [← Module 06](README.md) · [References](references.md)

## Hiểu trong 5 phút

Một API production không chỉ cần đúng business logic. Nó cần **bounded behavior** khi traffic/dependency xấu đi.

```text
Client traffic
    ↓
Admission / rate policy
    ↓
API capacity
    ↓
Cache / application
    ↓
Dependency call
    ↓
Timeout + retry policy
    ↓
Circuit breaker
```

Năm khái niệm dễ bị trộn:

| Concept | Câu hỏi |
|---|---|
| Rate limiting | client/action được phép bao nhiêu request trong một khoảng/capacity budget? |
| Concurrency limiting | cùng lúc cho phép bao nhiêu request đang chạy? |
| Throttling | thuật ngữ vendor thường dùng gần nghĩa rate limiting; cần đọc semantics cụ thể |
| Queueing/traffic shaping | thay vì reject ngay, giữ/chậm work theo capacity hữu hạn |
| Load shedding | từ chối work sớm để bảo vệ core system |

---

# 1. Rate Limiting

Rate limit policy cần ít nhất:

```text
partition key
+ algorithm
+ limit
+ time/capacity model
+ rejection behavior
+ observability
```

Ví dụ:

```text
key     = tenantId + endpoint
policy  = token bucket
rate    = 20 requests / second
burst   = 40
reject  = HTTP 429
```

Không chọn key chỉ vì dễ code.

Possible keys:

```text
IP
user ID
API key
tenant ID
endpoint
resource ID
composite key
```

IP có thể không đại diện user thật vì NAT/proxy/mobile carrier; đồng thời partition theo untrusted arbitrary input có thể làm nổ state cardinality.

---

# 2. HTTP contract khi limit bị vượt

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 5
Content-Type: application/problem+json
```

```json
{
  "type": "https://api.example.com/problems/rate-limit",
  "title": "Too many requests",
  "status": 429,
  "code": "RATE_LIMITED",
  "retryAfterSeconds": 5
}
```

`Retry-After` giúp client có retry policy hợp tác hơn.

Không mặc định retry 429 ngay lập tức:

```text
429
↓ immediate retry
429
↓ immediate retry
...
```

đó là retry storm.

---

# 3. Các algorithm rate limit cần biết

## Fixed window

```text
10:00:00 ───────── 10:00:59
limit = 100
```

Simple, cheap nhưng boundary burst có thể xảy ra:

```text
99 requests at 10:00:59
+
99 requests at 10:01:00
```

→ gần 198 requests trong ~1 giây quanh window boundary.

## Sliding window

Theo dõi rolling window tốt hơn fixed boundary nhưng state/computation phức tạp hơn.

## Token bucket

Mental model:

```text
bucket capacity = burst
refill rate     = sustained throughput
request         = consumes token(s)
```

Ưu điểm: cho controlled burst.

## Concurrency limiter

Không đo request/time; đo work đang active.

Hợp cho operation expensive:

```text
PDF export
AI generation
large report
CPU-heavy transform
```

Ví dụ:

```text
max 10 concurrent exports
```

khác với:

```text
100 exports / minute
```

---

# 4. ASP.NET Core rate limiting

ASP.NET Core có middleware cho fixed window, sliding window, token bucket và concurrency limiter.

Example token bucket:

```csharp
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.AddTokenBucketLimiter("public-write", limiter =>
    {
        limiter.TokenLimit = 40;
        limiter.TokensPerPeriod = 20;
        limiter.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
        limiter.AutoReplenishment = true;
        limiter.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();
app.UseRateLimiter();

app.MapPost("/v1/orders", CreateOrder)
    .RequireRateLimiting("public-write");
```

Production phải load test; các số `40/20` chỉ là example.

---

# 5. Distributed rate limiting

Một process-local limiter:

```text
Instance A limit 100
Instance B limit 100
Instance C limit 100
```

không tự tạo global limit 100.

Nếu traffic round-robin, effective allowance có thể lớn hơn nhiều.

Distributed options:

```text
API Gateway centralized policy
shared Redis/state store
local approximate limiter + centralized reconciliation
service mesh / sidecar policy
```

Trade-off:

```text
accuracy
latency
availability
cost
state cardinality
```

Rate limiter chính nó có thể thành dependency critical. Phải quyết định:

```text
fail-open?
fail-closed?
local fallback?
```

theo risk của endpoint.

Login/brute-force protection và read-only public catalog có thể chọn khác nhau.

---

# 6. Throttling nuance

Câu “throttling = slow down instead of reject” không phải universal definition.

Nhiều cloud/API products dùng:

```text
throttling == rate limiting / quota enforcement
```

Nếu bạn muốn **làm chậm** thay vì reject, hãy gọi rõ mechanism:

```text
bounded queue
concurrency limiter
traffic shaping
backpressure
admission control
```

Ví dụ bounded queue:

```text
capacity = 100
active   = 20
queue    = 80
next     = reject 503/429
```

Không dùng unbounded queue vì nó đổi overload thành:

```text
memory growth
+
very high tail latency
+
stale work
```

---

# 7. HTTP Caching — freshness + validators

Caching không chỉ là Redis.

HTTP có native semantics:

```text
Cache-Control
Age
ETag
If-None-Match
Last-Modified
If-Modified-Since
Vary
```

## Freshness example

```http
HTTP/1.1 200 OK
Cache-Control: public, max-age=60
ETag: "orders-v42"
```

Client/cache có thể reuse response trong freshness window.

Sau khi stale:

```http
GET /v1/catalog HTTP/1.1
If-None-Match: "catalog-v42"
```

Nếu chưa đổi:

```http
HTTP/1.1 304 Not Modified
ETag: "catalog-v42"
```

Không cần gửi lại body lớn.

---

# 8. ETag còn giúp optimistic concurrency

GET:

```http
HTTP/1.1 200 OK
ETag: "order-v7"
```

Client update:

```http
PUT /v1/orders/123 HTTP/1.1
If-Match: "order-v7"
```

Nếu server đã ở `v8`:

```http
HTTP/1.1 412 Precondition Failed
```

Điều này giúp tránh lost update.

Mental model:

```text
read representation version v7
       ↓
client edits
       ↓
write only if server still v7
```

---

# 9. Cacheability phải xuất phát từ business staleness

Questions:

```text
Data được stale bao lâu?
Có user-specific/private data không?
Response phụ thuộc Authorization/Cookie/Accept-Language không?
Invalidation source là gì?
Có CDN/shared cache không?
```

Sensitive response có thể cần:

```http
Cache-Control: no-store
```

Đừng thêm `public, max-age=3600` chỉ để benchmark đẹp.

---

# 10. Application cache khác HTTP cache

HTTP cache:

```text
consumer/proxy/CDN ↔ HTTP representation
```

Application cache:

```text
API implementation ↔ DB/dependency data
```

Ví dụ Redis cache-aside:

```text
request
 ↓
cache hit? ─ yes → response
 ↓ no
DB
 ↓
populate cache
 ↓
response
```

Hai lớp có failure/invalidation semantics riêng.

---

# 11. Retry — chỉ retry khi biết mình đang làm gì

Retry phù hợp với **transient failure**:

```text
connection reset
short timeout
503 transient dependency outage
429 with retry guidance
```

Retry không sửa:

```text
400 validation
401 bad/expired credential without refresh path
403 forbidden
404 stable resource absence
business conflict
permanent schema mismatch
```

---

# 12. Idempotency quyết định retry safety

GET thường safe/idempotent theo HTTP semantics.

POST payment:

```text
POST /payments
↓ timeout
```

không biết payment đã commit chưa.

Blind retry:

```text
POST again
→ possible double charge
```

Safe direction:

```http
POST /payments
Idempotency-Key: pay-attempt-123
```

Server atomically claims key/operation identity.

Đây là lý do retry policy không thể tách rời API contract.

---

# 13. Exponential backoff + jitter

Without backoff:

```text
1000 clients fail at same time
↓
1000 retry immediately
↓
dependency remains overloaded
```

Exponential backoff:

```text
attempt 1: ~200ms
attempt 2: ~400ms
attempt 3: ~800ms
```

Jitter randomizes delay để giảm synchronization/thundering herd.

Pseudo:

```csharp
TimeSpan ComputeDelay(int attempt)
{
    double baseMs = 200 * Math.Pow(2, attempt - 1);
    double jitter = Random.Shared.NextDouble() * 100;

    return TimeSpan.FromMilliseconds(baseMs + jitter);
}
```

Production ưu tiên library/platform resilience implementation thay hand-roll policy.

---

# 14. .NET HTTP resilience

Modern .NET có `Microsoft.Extensions.Http.Resilience` trên Polly.

Example:

```csharp
builder.Services
    .AddHttpClient<PaymentClient>(client =>
    {
        client.BaseAddress = new Uri("https://payments.internal");
    })
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);

        options.Retry.MaxRetryAttempts = 2;
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
    });
```

Nhưng **do not blindly retry unsafe methods**.

Ví dụ disable retry cho unsafe methods khi contract không đảm bảo idempotency:

```csharp
builder.Services
    .AddHttpClient<PaymentClient>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.DisableForUnsafeHttpMethods();
    });
```

Nếu payment endpoint có idempotency-key contract, bạn có thể thiết kế retry riêng có evidence.

---

# 15. Timeout vs cancellation vs deadline

## Timeout

Policy quyết định operation quá chậm và abort attempt.

## Cancellation

Caller không còn cần result:

```text
browser disconnected
request cancelled
job shutdown
```

## Deadline

Time budget end-to-end:

```text
API budget = 2s
DB used    = 400ms
remaining  = 1.6s
```

Downstream không nên mỗi layer tự cấp thêm 30 giây.

Propagate `CancellationToken`:

```csharp
app.MapGet("/v1/catalog", async (
    CatalogService service,
    CancellationToken cancellationToken) =>
{
    return await service.GetAsync(cancellationToken);
});
```

---

# 16. Circuit Breaker

Circuit breaker có mental state:

```text
CLOSED
  requests flow normally
  failures sampled
        ↓ threshold reached
OPEN
  fail fast, no remote call
        ↓ break duration
HALF-OPEN / trial
  allow probe(s)
        ↓ success
CLOSED
        ↓ failure
OPEN
```

Mục tiêu:

```text
reduce wasted calls
protect dependency
reduce thread/socket/queue pressure
limit cascading failure
```

Circuit breaker **không thay timeout**.

Nếu dependency treo 60s và breaker chưa nhận result, breaker không tự biến request thành fast timeout.

---

# 17. Retry + Circuit Breaker interaction

Sai:

```text
10 retries × 5 callers × 10 service hops
```

Retry amplification rất nhanh.

Better:

```text
bounded retries near dependency owner
+
timeout/deadline
+
circuit breaker
+
load shedding
+
observability
```

Architecture review phải hỏi:

```text
Which layer owns retry?
How many total attempts can one user request cause?
Is operation idempotent?
What is maximum total latency?
```

---

# 18. Cache stampede

Hot key expires:

```text
1000 requests
   ↓ cache miss simultaneously
1000 DB queries
```

Mitigations tùy workload:

```text
single-flight/request coalescing
stale-while-revalidate
jittered TTL
refresh-ahead
bounded fallback
```

Không mặc định distributed lock cho mọi key.

---

# 19. Failure experiments

## A — rate limit burst

Send 200 requests concurrently.

Record:

```text
allowed
rejected 429
P50/P95/P99
rate limiter metrics
DB QPS
```

## B — distributed limiter mismatch

Run 3 app instances với local-only limiter. Chứng minh global rate khác expected.

## C — stale ETag

1. GET resource `ETag=v1`.
2. Server updates resource to `v2`.
3. Client PUT with `If-Match:v1`.
4. Verify `412` and no lost update.

## D — retry storm

Remove jitter; start many clients during dependency outage. Compare with jittered backoff.

## E — unsafe retry

Simulate POST remote commit + lost response. Verify blind retry duplicates side effect; then add idempotency key and prove one effect.

## F — breaker recovery

Force dependency failures until breaker opens; restore dependency; observe half-open/probe/recovery.

---

# 20. Production checklist

- [ ] partition key chosen intentionally;
- [ ] rate-limit algorithm matches burst semantics;
- [ ] 429 response has usable retry metadata;
- [ ] queue is bounded;
- [ ] distributed limiter semantics documented;
- [ ] HTTP cache headers match data sensitivity/staleness;
- [ ] ETag/precondition used where lost update matters;
- [ ] retry only transient + safe/idempotent work;
- [ ] exponential backoff + jitter;
- [ ] Retry-After respected where applicable;
- [ ] total attempt count bounded;
- [ ] timeout/deadline/cancellation propagated;
- [ ] circuit breaker has measured thresholds;
- [ ] breaker state observable;
- [ ] dependency outage/load test exists.

## Exit criteria

Bạn hoàn thành chapter khi có thể:

- chọn fixed/sliding/token/concurrency limiter theo workload;
- giải thích throttling ambiguity;
- thiết kế 429 contract;
- dùng Cache-Control + ETag/304 và If-Match/412;
- thiết kế retry policy không double-write;
- tính maximum retry amplification;
- giải thích circuit breaker states;
- chạy failure drill và lưu metrics/traces.

## Verification metadata

- Verified: **2026-08-13**.
- ASP.NET Core rate limiting and .NET HTTP resilience examples target current .NET 10 documentation.
