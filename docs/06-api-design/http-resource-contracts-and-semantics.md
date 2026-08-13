# HTTP Resource Contracts và Semantics

> [← Module 06](README.md) · [Security](security-auth-oauth-and-cors.md) · [References](references.md)

## Hiểu trong 5 phút

HTTP API contract không chỉ là URL + JSON.

```text
Endpoint
= method
+ route / target resource
+ request headers/body
+ response status/headers/body
+ security policy
+ retry/idempotency semantics
```

Ví dụ:

```http
PUT /v1/customers/cus-123/profile HTTP/1.1
If-Match: "v7"
Content-Type: application/json
Accept: application/json

{
  "displayName": "Alice"
}
```

Contract phải nói được:

```text
PUT có nghĩa gì?
Nếu resource chưa tồn tại?
Nếu ETag cũ?
Retry có an toàn?
Response status nào?
Representation nào trả về?
```

---

# 1. Endpoint là gì?

Cách nói “endpoint = unique URL naming one resource” hữu ích cho beginner nhưng chưa đủ.

Trong production, một endpoint thường được hiểu là:

```text
HTTP method + route template + observable contract
```

Ví dụ hai endpoint khác nhau dù cùng route:

```text
GET  /orders/{id}
PUT  /orders/{id}
```

Route có thể identify:

```text
single resource      /orders/123
collection           /orders
sub-resource         /orders/123/items
business resource    /orders/123/cancellation
```

Không phải mọi endpoint đều map 1:1 tới DB row.

---

# 2. Resource design

Resource nên phản ánh business vocabulary.

Bad:

```text
/api/OrderTableRows
/api/ExecuteCheckoutProcedure
/api/GetAllOrderDtos
```

Better:

```text
/orders
/orders/{orderId}
/orders/{orderId}/items
/checkouts
/payment-attempts
```

Ẩn implementation details giúp thay DB/schema/service internals mà không phá client.

---

# 3. HTTP Methods

RFC HTTP semantics phân biệt **safe** và **idempotent**.

| Method | Typical meaning | Safe? | Idempotent by method semantics? |
|---|---|---:|---:|
| `GET` | retrieve representation | yes | yes |
| `HEAD` | GET metadata without response content | yes | yes |
| `POST` | process representation / create subordinate resource / command | no | no by default |
| `PUT` | create/replace target resource representation | no | yes |
| `PATCH` | partial modification according to patch media type/contract | no | not guaranteed |
| `DELETE` | remove association/resource according to API semantics | no | yes |

**Safe** không có nghĩa server không log/metric; nghĩa user-requested semantics không yêu cầu state-changing effect.

**Idempotent** không có nghĩa response luôn giống nhau. Nó nghĩa intended effect của multiple identical requests giống effect của một request.

---

# 4. GET

```http
GET /v1/orders/123 HTTP/1.1
Accept: application/json
```

Success:

```http
HTTP/1.1 200 OK
Content-Type: application/json
ETag: "order-123-v9"
Cache-Control: private, max-age=30
```

```json
{
  "id": "123",
  "status": "PAID",
  "total": 120.50
}
```

Not found:

```http
HTTP/1.1 404 Not Found
```

GET không nên trigger destructive business action:

Bad:

```text
GET /orders/123?do=cancel
```

Crawler/prefetcher có thể issue GET. Unsafe action phải dùng method/resource contract phù hợp.

---

# 5. POST

POST phù hợp khi server/process quyết định resource identity hoặc operation không mang semantics replace target như PUT.

```http
POST /v1/orders HTTP/1.1
Content-Type: application/json
Idempotency-Key: create-order-abc123
```

```json
{
  "cartId": "cart-1"
}
```

Created:

```http
HTTP/1.1 201 Created
Location: /v1/orders/ord-900
```

```json
{
  "id": "ord-900",
  "status": "CREATED"
}
```

`Location` giúp consumer biết URI của created resource.

---

# 6. PUT

PUT target URI thường đã xác định resource:

```http
PUT /v1/user-preferences/user-42 HTTP/1.1
Content-Type: application/json
```

Repeated identical PUT có same intended effect.

Nhưng concurrency vẫn cần policy. Hai clients có thể overwrite nhau nếu không dùng precondition/version.

---

# 7. PATCH

PATCH không chỉ có nghĩa “PUT nhưng gửi ít field hơn”. Contract phải nói patch document semantics.

Ví dụ custom partial update:

```http
PATCH /v1/orders/123 HTTP/1.1
Content-Type: application/merge-patch+json
```

```json
{
  "shippingAddress": {
    "city": "Hanoi"
  }
}
```

Bạn cần quyết định:

```text
missing field = unchanged?
null = clear value hay unchanged?
array replace hay merge?
operation idempotent không?
```

Nếu semantics mơ hồ, client/server drift rất nhanh.

---

# 8. DELETE

```http
DELETE /v1/api-keys/key-123 HTTP/1.1
```

Possible response:

```http
HTTP/1.1 204 No Content
```

DELETE idempotent theo intended effect:

```text
resource absent after request
```

Lần hai có thể trả `404` hoặc `204` tùy contract; response khác không phá idempotency definition.

---

# 9. Request anatomy

Một request gồm:

```text
method
request target
headers
optional content/body
```

Example:

```http
POST /v1/payments HTTP/1.1
Authorization: Bearer <token>
Idempotency-Key: pay-123
Content-Type: application/json
Accept: application/json
Traceparent: 00-...

{
  "orderId": "ord-1",
  "amount": 100.00,
  "currency": "USD"
}
```

Các headers không phải decoration; chúng có contract semantics.

---

# 10. Response anatomy

Response gồm:

```text
status code
headers
optional content/body
```

Example:

```http
HTTP/1.1 202 Accepted
Location: /v1/payment-attempts/pay-123
Retry-After: 2
Content-Type: application/json
```

```json
{
  "id": "pay-123",
  "status": "PENDING"
}
```

`202 Accepted` không có nghĩa business operation đã hoàn tất.

---

# 11. Status Code classes

Beginner mnemonic:

```text
2xx success
3xx redirection
4xx client/request-side condition
5xx server-side failure condition
```

Nhưng production phải hiểu từng code.

## Frequently used

| Code | Typical API semantics |
|---:|---|
| `200 OK` | successful response with representation/result |
| `201 Created` | resource created; often include `Location` |
| `202 Accepted` | accepted for async processing, not completed yet |
| `204 No Content` | successful, no response content |
| `304 Not Modified` | conditional GET validator matched; use cached representation |
| `400 Bad Request` | malformed/invalid request contract |
| `401 Unauthorized` | authentication credentials missing/invalid for resource |
| `403 Forbidden` | authenticated/known caller not allowed |
| `404 Not Found` | target resource not found / hidden according to policy |
| `409 Conflict` | conflict with current resource/business state |
| `412 Precondition Failed` | `If-Match`/other precondition failed |
| `415 Unsupported Media Type` | request content type unsupported |
| `422 Unprocessable Content` | syntactically understood but semantic validation failed when chosen by API policy |
| `429 Too Many Requests` | rate policy exceeded |
| `500 Internal Server Error` | unexpected server error |
| `502 Bad Gateway` | intermediary got invalid/failed upstream response |
| `503 Service Unavailable` | temporary unavailability/overload/maintenance |
| `504 Gateway Timeout` | gateway/proxy timed out waiting upstream |

Không trả `200` rồi đặt `{ "success": false }` cho mọi error.

---

# 12. 400 vs 409 vs 422

Ví dụ create booking.

Malformed JSON:

```text
400
```

Request fields parse được nhưng amount negative:

```text
400 or 422 according to your API convention
```

Seat đã được booking bởi transaction khác:

```text
409 Conflict
```

Điều quan trọng là **consistent documented contract**.

---

# 13. Content-Type và Accept

Request:

```http
Content-Type: application/json
```

nói body đang encoded dạng gì.

Client:

```http
Accept: application/json
```

nói representation muốn nhận.

Nếu request body media type không hỗ trợ:

```http
415 Unsupported Media Type
```

Nếu server không thể produce acceptable representation, có thể dùng `406 Not Acceptable` tùy design.

---

# 14. Idempotency — HTTP vs application

HTTP method idempotency:

```text
PUT / DELETE / safe methods
```

Application idempotency cho POST:

```http
POST /v1/checkouts
Idempotency-Key: checkout-123
```

Server cần atomic ownership, không chỉ query-before-insert.

Bad race:

```text
Request A: key absent
Request B: key absent
A charges
B charges
```

Better:

```text
unique constraint / atomic insert on idempotency key
        ↓
one request wins ownership
        ↓
others read/replay stable operation result
```

Pseudo SQL:

```sql
CREATE UNIQUE INDEX UX_Checkout_IdempotencyKey
ON CheckoutOperation(IdempotencyKey);
```

---

# 15. Idempotency response storage

Một design thực dụng:

```text
IdempotencyKey
RequestHash
OperationId
Status
ResponseStatusCode
ResponseBody/reference
CreatedAt
ExpiresAt
```

Nếu cùng key nhưng payload khác:

```text
409 Conflict
```

hoặc explicit domain error:

```json
{
  "code": "IDEMPOTENCY_KEY_REUSED_WITH_DIFFERENT_REQUEST"
}
```

Không silently accept different operation under same key.

---

# 16. Conditional requests và lost update

GET:

```http
HTTP/1.1 200 OK
ETag: "v10"
```

Update:

```http
PUT /v1/orders/123
If-Match: "v10"
```

Nếu resource đã `v11`:

```http
HTTP/1.1 412 Precondition Failed
```

Flow:

```text
Client A reads v10
Client B reads v10
Client A writes → v11
Client B writes If-Match v10
              ↓
             412
```

No lost update.

---

# 17. Long-running operations

Không giữ HTTP request 10 phút nếu workflow phù hợp async resource.

```http
POST /v1/exports
```

```http
HTTP/1.1 202 Accepted
Location: /v1/exports/exp-123
```

Poll:

```http
GET /v1/exports/exp-123
```

```json
{
  "status": "RUNNING",
  "progress": 42
}
```

Later:

```json
{
  "status": "COMPLETED",
  "downloadUrl": "..."
}
```

Alternative realtime notification: webhook/SSE depending consumer.

---

# 18. Filtering, sorting, search và bounds

Collection endpoint:

```http
GET /v1/orders?status=paid&sort=-createdAt&limit=50
```

Design rules:

```text
allowlisted sort fields
bounded page size
validated filters
indexed access path
no arbitrary SQL expression from query string
```

Bad:

```text
?orderBy=<raw SQL>
```

API contract phải map tới safe application query model.

---

# 19. Bulk operations

1000 individual calls:

```text
1000 × HTTP overhead
1000 × auth/policy
1000 × retries
```

Bulk endpoint có thể hợp lý:

```http
POST /v1/orders:batch-status
```

hoặc resource-oriented batch request tùy API convention.

Phải define partial result semantics:

```json
{
  "results": [
    { "id": "1", "status": "updated" },
    { "id": "2", "status": "failed", "code": "ORDER_LOCKED" }
  ]
}
```

Không assume bulk = one distributed atomic transaction.

---

# 20. ASP.NET Core typed endpoints

```csharp
app.MapGet("/v1/orders/{id}", async Task<Results<Ok<OrderResponse>, NotFound>> (
    string id,
    OrderQueries queries,
    CancellationToken cancellationToken) =>
{
    OrderResponse? order = await queries.FindAsync(id, cancellationToken);

    return order is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(order);
});
```

Create:

```csharp
app.MapPost("/v1/orders", async Task<Results<Created<OrderResponse>, ValidationProblem>> (
    CreateOrderRequest request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    if (request.Items.Count == 0)
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["items"] = ["At least one item is required."]
        });
    }

    OrderResponse result = await service.CreateAsync(request, cancellationToken);

    return TypedResults.Created($"/v1/orders/{result.Id}", result);
});
```

Typed result giúp OpenAPI/test contract rõ hơn, nhưng framework type không thay business design.

---

# 21. Contract testing

Craft Better Software có một principle phù hợp: test **observable public behavior**, không couple test vào internal class graph.

API integration/contract test nên assert:

```text
method + route
status
headers
body schema
business side effect
negative cases
```

Example:

```csharp
[Fact]
public async Task Duplicate_checkout_key_creates_one_order()
{
    using HttpClient client = factory.CreateClient();

    var request1 = NewCheckoutRequest("checkout-123");
    var request2 = NewCheckoutRequest("checkout-123");

    HttpResponseMessage first = await client.SendAsync(request1);
    HttpResponseMessage second = await client.SendAsync(request2);

    Assert.True(first.IsSuccessStatusCode);
    Assert.True(second.IsSuccessStatusCode);

    int count = await db.Orders.CountAsync(x => x.IdempotencyKey == "checkout-123");
    Assert.Equal(1, count);
}
```

---

# 22. Common mistakes

- dùng GET cho action thay đổi state;
- coi POST luôn non-retryable nhưng lại không thiết kế idempotency;
- coi DELETE phải trả cùng status mọi lần mới idempotent;
- dùng `200 OK` cho mọi outcome;
- thiếu `Location`/operation resource cho async flow;
- expose DB column names trực tiếp thành permanent public contract;
- pagination không deterministic order;
- không bound filter/sort/page size;
- không có ETag/precondition cho concurrent update quan trọng;
- error body leak stack trace/internal exception.

---

# 23. Failure experiments

## A — duplicate POST race

Send same idempotency key concurrently. Verify one side effect.

## B — lost update

Two clients GET same ETag; both update. Verify second gets `412`.

## C — malformed vs conflict

Verify `400` and `409` remain distinguishable and documented.

## D — unsafe GET

Write a test/crawler simulation proving GET endpoints have no destructive state transition.

## E — content type mismatch

Send XML to JSON-only endpoint; verify explicit `415` instead of hidden parser error.

---

# 24. Exit criteria

- [ ] define endpoint as method + route + contract;
- [ ] explain safe vs idempotent;
- [ ] use GET/POST/PUT/PATCH/DELETE intentionally;
- [ ] choose status code from semantics, not habit;
- [ ] implement application idempotency for POST;
- [ ] use ETag/If-Match where lost update matters;
- [ ] design bounded filtering/sorting/bulk behavior;
- [ ] write public API behavior tests;
- [ ] explain request/response headers as contract.

## Verification metadata

- Verified: **2026-08-13**.
- Normative HTTP semantics: RFC 9110 and related HTTP RFCs in [references](references.md).
