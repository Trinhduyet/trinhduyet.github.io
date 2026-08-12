# Resilience, Security và Middleware Production

> [← ASP.NET Core overview](README.md) · [References](references.md)

## Hiểu trong 5 phút

Production API phải xử lý bốn loại pressure khác nhau:

```text
Bad input / unauthorized
        ↓
reject correctly

Too much traffic
        ↓
rate limit / shed load

Dependency slow / transient failure
        ↓
time budget + carefully-scoped resilience

Unexpected exception
        ↓
stable error contract + telemetry
```

Không có một `retry middleware` chung giải quyết tất cả.

---

# 1. Error handling bằng Problem Details

Setup:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
```

Endpoint:

```csharp
app.MapGet("/users/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.BadRequest();
    }

    return Results.Ok(new { Id = id });
});
```

Mục tiêu là giữ public error contract ổn định và không leak stack trace/database detail.

Custom business result nên explicit:

```csharp
return Results.Problem(
    statusCode: StatusCodes.Status409Conflict,
    title: "Order state conflict",
    detail: "The order has already been completed.");
```

---

# 2. Authentication != Authorization

```text
Authentication
"Bạn là ai?"

Authorization
"Identity này được phép làm gì với resource này?"
```

Setup JWT bearer:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("orders.write", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "orders.write");
    });
});
```

Pipeline:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Endpoint:

```csharp
app.MapPost("/orders/{id:long}/cancel", CancelOrderAsync)
    .RequireAuthorization("orders.write");
```

Nhưng policy `orders.write` vẫn chưa đủ nếu user chỉ được sửa Orders thuộc tenant/resource của họ.

Resource authorization phải nằm gần business operation:

```csharp
var order = await db.Orders
    .SingleOrDefaultAsync(
        x => x.Id == id && x.TenantId == tenantId,
        cancellationToken);

if (order is null)
{
    return Results.NotFound();
}
```

Không nhận `TenantId` từ request rồi tin trực tiếp nếu tenant identity đã có trong authenticated principal.

---

# 3. Rate limiting

Đăng ký service:

```csharp
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("writes", limiter =>
    {
        limiter.PermitLimit = 20;
        limiter.Window = TimeSpan.FromSeconds(1);
        limiter.QueueLimit = 0;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
```

Pipeline:

```csharp
app.UseRateLimiter();
```

Endpoint:

```csharp
app.MapPost("/orders", CreateOrderAsync)
    .RequireRateLimiting("writes");
```

Rate limit không chỉ để chống attack. Nó còn bảo vệ bounded capacity phía sau.

Nhưng policy phải load-test. `20 req/s` không có nghĩa backend chắc chắn chịu được hoặc business muốn limit theo global IP.

Bạn phải chọn partition key phù hợp:

```text
anonymous public API → IP/client key?
authenticated SaaS     → tenant/user/client app?
expensive report       → user + endpoint?
```

---

# 4. QueueLimit = 0 hay queue?

Nếu bạn cho rate limiter queue 10,000 requests, bạn đã biến overload thành latency explosion.

Mental model:

```text
incoming > capacity

Option A: reject early
→ 429 nhanh, caller retry/backoff

Option B: queue
→ có thể smooth burst nhỏ
→ nhưng queue tăng memory + tail latency
```

Queue phải bounded và có lý do.

---

# 5. Timeout budget

Một request có total budget 2 giây không nên để downstream call có timeout 10 giây.

```text
HTTP request budget: 2s
    ↓
application work: 200ms
    ↓
database: 500ms
    ↓
external API: 800ms
    ↓
remaining buffer
```

Không cần chia cứng như trên cho mọi request, nhưng phải có deadline thinking.

Typed client:

```csharp
builder.Services.AddHttpClient<InventoryClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Inventory:BaseUrl"]!);

    client.Timeout = TimeSpan.FromSeconds(2);
});
```

---

# 6. Resilient HttpClient

Với package `Microsoft.Extensions.Http.Resilience`, .NET cung cấp standard resilience handler:

```csharp
builder.Services
    .AddHttpClient<InventoryClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Inventory:BaseUrl"]!);
    })
    .AddStandardResilienceHandler();
```

Điều quan trọng hơn API call là semantics:

```text
GET inventory
→ retry transient failure có thể hợp lý

POST charge credit card
→ retry mù có thể duplicate side effect
```

Nếu operation không idempotent, thiết kế idempotency/dedup trước khi thêm retry.

---

# 7. Idempotency ở API write

Client có thể retry vì timeout mà server đã commit thành công.

Request:

```http
POST /payments
Idempotency-Key: 8cc8c723-4af4-4450-b4da-01b95ca1a896
```

Storage concept:

```text
(IdempotencyKey, Principal/Tenant, RequestHash)
        ↓
Processing / Completed
        ↓
Stored response/result reference
```

Pseudo-code:

```csharp
var existing = await idempotencyStore.GetAsync(
    tenantId,
    idempotencyKey,
    cancellationToken);

if (existing is not null)
{
    return existing.ToHttpResult();
}

var result = await paymentService.CreateAsync(
    request,
    cancellationToken);

await idempotencyStore.CompleteAsync(
    tenantId,
    idempotencyKey,
    result,
    cancellationToken);

return Results.Ok(result);
```

Thực tế cần transaction/race protection. Hai request cùng key tới đồng thời không được cùng execute side effect.

---

# 8. Validation boundary

Bad:

```csharp
app.MapPost("/orders", async (CreateOrderRequest request) =>
{
    // assumes everything is valid
});
```

Simple validation:

```csharp
app.MapPost("/orders", async (
    CreateOrderRequest request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var errors = new Dictionary<string, string[]>();

    if (request.Quantity <= 0)
    {
        errors["quantity"] = ["Quantity must be greater than zero."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var result = await service.CreateAsync(request, cancellationToken);
    return Results.Created($"/orders/{result.Id}", result);
});
```

Business invariant vẫn cần enforce ở application/database dưới concurrency. Input validation không thay transaction constraint.

---

# 9. Exception classification

Đừng biến mọi exception thành `500 Internal Server Error` rồi retry.

```text
Validation / client error
→ 400/422-like contract, no retry

Unauthorized / forbidden
→ 401/403, no retry

Not found
→ 404

Conflict / concurrency
→ 409 or domain-specific mapping

Transient dependency failure
→ maybe 503 / retry policy

Unexpected bug
→ 500 + trace/log
```

Mapping phải consistent và tránh leak internal detail.

---

# 10. Security headers không thay auth

Bạn có thể harden response headers, HTTPS, CORS, cookie settings, nhưng chúng không thay authorization.

CORS đặc biệt:

```text
CORS controls browser cross-origin behavior
≠
API authorization
```

Backend-to-backend caller không bị browser CORS policy bảo vệ.

---

# 11. Middleware order example

```csharp
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
```

Order chính xác phụ thuộc middleware/endpoint routing setup, nhưng bạn phải biết component nào cần chạy trước component nào và verify với integration tests.

---

# 12. Failure experiment — retry duplicate side effect

Tạo fake payment dependency:

```csharp
public sealed class FakePaymentGateway
{
    private int _charges;

    public int Charges => _charges;

    public async Task ChargeAsync(
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _charges);
        await Task.Delay(100, cancellationToken);
        throw new HttpRequestException("Response lost after side effect");
    }
}
```

Nếu caller retry mù, `Charges` có thể thành 2.

Mục tiêu lab:

1. reproduce duplicate;
2. thêm idempotency key/dedup;
3. verify retry không duplicate business effect.

---

# 13. Failure experiment — rate limit

Load endpoint:

```bash
seq 1 100 | xargs -n1 -P20 -I{} \
  curl -s -o /dev/null -w "%{http_code}\n" \
  http://localhost:5000/expensive
```

Evidence:

```text
200 count
429 count
P95 latency
CPU
DB calls
```

Goal: chứng minh limiter bảo vệ downstream, không chỉ chứng minh có response 429.

---

# 14. Integration test cho authorization

```csharp
[Fact]
public async Task CancelOrder_without_scope_returns_forbidden()
{
    using var client = factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        FakeJwt("orders.read");

    var response = await client.PostAsync(
        "/orders/42/cancel",
        content: null);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

Thêm test cross-tenant/resource access chứ không chỉ role/scope test.

---

# 15. Architect questions

- Nên reject overload ở gateway, API hay downstream?
- Retry ở layer nào để tránh retry amplification?
- Write endpoint có idempotency contract không?
- AuthZ rule thuộc endpoint, application service hay domain/resource boundary?
- Rate-limit partition key có công bằng giữa tenants không?
- Timeout budget của request đi qua 3 dependency được phân bổ thế nào?
- Khi dependency down, degrade/fail-closed/fail-open semantics là gì?

---

# 16. Exit criteria

Bạn hoàn thành chapter khi có thể:

- cấu hình và test rate limiting;
- phân biệt authentication/authorization/resource authorization;
- thiết kế stable error contract bằng Problem Details;
- giải thích retry có thể gây duplicate side effect;
- thiết kế idempotency key boundary;
- đặt timeout/deadline hợp lý;
- load-test limiter và measure downstream protection;
- viết negative tests cho auth/cross-tenant access.

## Official English Sources

- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)
- [Rate limiting middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)
- [ASP.NET Core authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-10.0)
- [ASP.NET Core authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0)
- [.NET HTTP resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)

## Verification metadata

- Verified: 2026-08-12.
- Current docs confirm `AddRateLimiter` + `UseRateLimiter` pattern for ASP.NET Core 10.
- `AddStandardResilienceHandler()` verified against current Microsoft.Extensions.Http.Resilience 10.x docs.
- Status: code-first deep rewrite.
