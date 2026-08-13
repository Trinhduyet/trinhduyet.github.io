# Module 06 — API Design

> [← Module 05 — SQL](../05-sql/README.md) · [Roadmap](../00-roadmap/README.md) · [→ Module 07 — ASP.NET Core](../07-aspnet-core/README.md)

API Design không phải chỉ là đặt tên URL đẹp. Đây là **public contract** giữa producer và consumer: resource, HTTP semantics, identity, authorization, traffic policy, compatibility, failure semantics và operational behavior.

Module này dùng format **code-first + failure-first**:

```text
Business requirement
        ↓
Public contract
        ↓
HTTP / protocol semantics
        ↓
Security + reliability policy
        ↓
Executable examples/tests
        ↓
Observability + operational evidence
        ↓
Architecture trade-off
```

## Hiểu trong 5 phút

Một API production phải trả lời được ít nhất 8 câu hỏi:

1. **What** — resource/capability nào đang được expose?
2. **How** — method/protocol nào biểu diễn operation?
3. **Who** — caller là ai?
4. **May** — caller được phép làm gì với resource cụ thể?
5. **How much** — caller được dùng bao nhiêu capacity?
6. **What if duplicated** — retry/duplicate có tạo side effect lần hai không?
7. **What if changed** — contract mới có phá consumer cũ không?
8. **What if dependency fails** — timeout/retry/circuit breaker/fallback được xử lý thế nào?

Không trả lời được các câu hỏi này thì API mới chỉ là transport wrapper quanh business code.

---

# 1. Coverage matrix — 25 chủ đề bắt buộc

| # | Topic | Cách hiểu chính xác hơn | Chapter |
|---:|---|---|---|
| 1 | **Endpoint** | Không chỉ là “một URL cho một resource”. Trong thực tế endpoint thường là **HTTP method + route + request/response contract**; route có thể trỏ tới resource đơn, collection hoặc command/action có business meaning. | [HTTP Contracts](http-resource-contracts-and-semantics.md) |
| 2 | **HTTP Methods** | GET/HEAD/POST/PUT/PATCH/DELETE + semantics `safe`/`idempotent`; method không chỉ là CRUD alias. | [HTTP Contracts](http-resource-contracts-and-semantics.md) |
| 3 | **Request–Response** | Request target + method + headers + optional content; response = status + headers + optional content. | [HTTP Contracts](http-resource-contracts-and-semantics.md) |
| 4 | **Status Codes** | 2xx/3xx/4xx/5xx là classes; không nên hiểu đơn giản “4xx = client broke it, 5xx = server broke it”. `409`, `412`, `429`, `503` cần semantics cụ thể. | [HTTP Contracts](http-resource-contracts-and-semantics.md) |
| 5 | **Authentication** | Xác lập identity/principal của caller. Không phải business authorization. | [Security, OAuth & CORS](security-auth-oauth-and-cors.md) |
| 6 | **Authorization** | Kiểm tra caller có permission trên action/resource cụ thể hay không; thường cần policy + resource-based authorization. | [Security, OAuth & CORS](security-auth-oauth-and-cors.md) |
| 7 | **Access Tokens** | Credential đại diện authorization grant/scope. Bearer token phải được bảo vệ như secret; “short-lived” là best practice chứ không phải định nghĩa duy nhất. | [Security, OAuth & CORS](security-auth-oauth-and-cors.md) |
| 8 | **OAuth 2.0** | Delegated **authorization** framework. OAuth không tự định nghĩa user authentication; OpenID Connect bổ sung identity layer. | [Security, OAuth & CORS](security-auth-oauth-and-cors.md) |
| 9 | **Rate Limiting** | Giới hạn request/action theo key + window/capacity; thường trả `429` + retry metadata. | [Traffic, Cache & Resilience](traffic-caching-rate-limits-and-resilience.md) |
| 10 | **Throttling** | Từ này thường được dùng đồng nghĩa với rate limiting trong sản phẩm/vendor docs. “Làm chậm thay vì reject” chính xác hơn là **traffic shaping / queueing / admission control**. | [Traffic, Cache & Resilience](traffic-caching-rate-limits-and-resilience.md) |
| 11 | **Pagination** | Offset/page-number hoặc cursor/keyset; phải có deterministic order và bounded page size. | [Evolution, Errors & Pagination](api-evolution-errors-and-pagination.md) |
| 12 | **Caching** | HTTP freshness + validators (`Cache-Control`, `ETag`, `Last-Modified`, `304`) và application/cache layer. Không chỉ “copy response”. | [Traffic, Cache & Resilience](traffic-caching-rate-limits-and-resilience.md) |
| 13 | **Idempotency** | RFC nghĩa là repeated identical request có **same intended effect**; response có thể khác. POST/payment thường cần application-level idempotency key. | [HTTP Contracts](http-resource-contracts-and-semantics.md) |
| 14 | **Webhooks** | Server-to-server callback/event delivery; phải có signature, delivery ID, retry, dedup, replay protection và async processing. | [Events, gRPC & Webhooks](events-grpc-webhooks-and-contracts.md) |
| 15 | **API Versioning** | Ưu tiên backward-compatible evolution trước; version chỉ dùng khi breaking change thực sự cần. | [Evolution, Errors & Pagination](api-evolution-errors-and-pagination.md) |
| 16 | **OpenAPI** | Machine-readable HTTP API contract cho docs, generation, validation, test/mocks và contract diff; spec hiện có dòng 3.2.x nhưng tooling support phải verify riêng. | [Evolution, Errors & Pagination](api-evolution-errors-and-pagination.md) |
| 17 | **REST vs GraphQL** | Resource-oriented HTTP vs schema/query language cho client-shaped selection. Cần so thêm gRPC theo workload. | [API Styles & Realtime](api-styles-gateway-and-realtime.md) |
| 18 | **API Gateway** | Edge/front-door cho routing, authn, rate limit, TLS, observability, transformation; tránh nhét domain logic vào gateway. | [API Styles & Realtime](api-styles-gateway-and-realtime.md) |
| 19 | **Microservices** | Independent business/data/deployment boundaries; giao tiếp có thể là sync API **hoặc async messages/events**, không phải “chỉ qua APIs”. | [API Styles & Realtime](api-styles-gateway-and-realtime.md) + [Module 18](../18-microservices-architecture/README.md) |
| 20 | **Error Handling** | Stable machine-readable error contract; HTTP status + problem detail + domain code + correlation/trace context. | [Evolution, Errors & Pagination](api-evolution-errors-and-pagination.md) |
| 21 | **gRPC** | Contract-first RPC, thường Protobuf + HTTP/2, unary/streaming; rất hợp service-to-service nhưng browser/public API có trade-off. | [API Styles & Realtime](api-styles-gateway-and-realtime.md) |
| 22 | **WebSockets & SSE** | WebSocket = bidirectional full-duplex; SSE = server→client text event stream với reconnect semantics. | [API Styles & Realtime](api-styles-gateway-and-realtime.md) |
| 23 | **CORS** | Browser mechanism để relax same-origin policy. CORS **không thay authentication/authorization** và non-browser clients không bị CORS enforce. | [Security, OAuth & CORS](security-auth-oauth-and-cors.md) |
| 24 | **Retries & Backoff** | Chỉ retry transient failures khi operation an toàn/idempotent; exponential backoff + jitter + bounded attempts + respect retry signals. | [Traffic, Cache & Resilience](traffic-caching-rate-limits-and-resilience.md) |
| 25 | **Circuit Breaker** | Tạm fail-fast khi dependency có tỷ lệ failure/timeout cao để tránh cascading failure; không phải replacement cho timeout/retry. | [Traffic, Cache & Resilience](traffic-caching-rate-limits-and-resilience.md) |

**Kết luận coverage:** sau deep rewrite này, 25/25 chủ đề đều có nơi học cụ thể; các chủ đề P0 có code/config, failure experiment và exit criteria thay vì chỉ xuất hiện trong glossary.

---

# 2. Learning path

Học theo dependency sau:

```text
HTTP resource + method + status
        ↓
Authentication / Authorization / OAuth / CORS
        ↓
Pagination / Error / Versioning / OpenAPI
        ↓
Rate limit / Cache / Idempotency / Retry / Circuit Breaker
        ↓
Webhooks / Events / gRPC
        ↓
REST vs GraphQL vs gRPC / Gateway / Realtime
        ↓
ASP.NET Core implementation
        ↓
Distributed Systems + Microservices
```

## Chapter map

| Chapter | Priority | Nội dung chính |
|---|---|---|
| [HTTP Resource Contracts & Semantics](http-resource-contracts-and-semantics.md) | P0 | endpoint, method, request/response, status, safe/idempotent, preconditions |
| [Security, OAuth & CORS](security-auth-oauth-and-cors.md) | P0 | authn/authz, bearer tokens, OAuth 2.0/OIDC boundary, CORS |
| [API Evolution, Errors & Pagination](api-evolution-errors-and-pagination.md) | P0 | Problem Details, cursor pagination, compatibility, versioning, OpenAPI |
| [Traffic, Caching, Rate Limits & Resilience](traffic-caching-rate-limits-and-resilience.md) | P0 | rate limiting, throttling nuance, caching, retries/backoff, circuit breaker |
| [Events, gRPC, Webhooks & Contracts](events-grpc-webhooks-and-contracts.md) | P0/P1 | webhook delivery, dedup, event contract, gRPC interoperability |
| [API Styles, Gateway & Realtime](api-styles-gateway-and-realtime.md) | P1 | REST/GraphQL/gRPC, API Gateway, microservice boundary, WebSocket/SSE |
| [References](references.md) | source | RFC/IETF, Microsoft, OpenAPI, GraphQL/gRPC + supplementary reading |

---

# 3. Senior API Design checklist

Trước khi viết endpoint, đi qua checklist này:

```text
01 Business capability / actor / invariant
02 Consumer types: browser, mobile, internal service, partner
03 Expected QPS / payload / latency / availability
04 Resource model + operation semantics
05 Request/response/error contract
06 Authentication mechanism
07 Authorization policy + resource ownership
08 Pagination/filter/sort/search bounds
09 Idempotency + concurrency/precondition behavior
10 Rate limit / quota / admission behavior
11 Cacheability + staleness tolerance
12 Timeout / retry / circuit breaker policy
13 Versioning + deprecation strategy
14 OpenAPI/Proto/GraphQL schema as contract artifact
15 Observability: trace ID, metrics, logs, audit
16 Abuse/security: input size, CORS, token scope, secrets, SSRF
17 Failure drills + dependency outage behavior
18 Cost/capacity at 10×/100×
```

DesignGurus nhấn mạnh contract-first, backward compatibility, security-by-design, observability, pagination, idempotency, standardized errors, rate limiting và HTTP caching. Roadmap này lấy các ý đó làm **review checklist**, nhưng normative protocol behavior vẫn phải theo RFC/official docs.

Craft Better Software nhấn mạnh một điểm rất phù hợp với module này: **test public behavior thay vì test implementation detail**. Vì vậy API contract được coi là executable behavior boundary; controller/service có thể refactor nhưng observable HTTP behavior phải được regression-test.

---

# 4. Một API slice production tối thiểu

Ví dụ checkout:

```http
POST /v1/checkouts HTTP/1.1
Authorization: Bearer <access-token>
Idempotency-Key: checkout-7f2d
Content-Type: application/json
Accept: application/json

{
  "cartId": "cart-123",
  "paymentMethodId": "pm-456"
}
```

Success:

```http
HTTP/1.1 201 Created
Location: /v1/orders/ord-789
Content-Type: application/json

{
  "orderId": "ord-789",
  "status": "PENDING_PAYMENT"
}
```

Conflict/duplicate semantics phải được thiết kế trước, không để framework quyết định ngẫu nhiên.

ASP.NET Core minimal example:

```csharp
app.MapPost("/v1/checkouts", async Task<Results<Created<OrderResponse>, ProblemHttpResult>> (
    CheckoutRequest request,
    HttpContext http,
    ICheckoutService checkout,
    CancellationToken cancellationToken) =>
{
    string? idempotencyKey = http.Request.Headers["Idempotency-Key"].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(idempotencyKey))
    {
        return TypedResults.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Missing idempotency key",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "IDEMPOTENCY_KEY_REQUIRED"
            });
    }

    OrderResponse result = await checkout.CheckoutAsync(
        request,
        idempotencyKey,
        cancellationToken);

    return TypedResults.Created($"/v1/orders/{result.OrderId}", result);
})
.RequireAuthorization("checkout:write");
```

API design review không dừng ở việc code compile. Phải hỏi:

```text
retry POST này có charge hai lần không?
caller bị 429 thì retry lúc nào?
payment timeout là FAILED hay UNKNOWN?
access token scope nào cần?
CORS có liên quan không nếu caller là mobile/native app?
contract thay field sẽ ảnh hưởng SDK/client nào?
```

---

# 5. Failure experiments bắt buộc

Mỗi người học nên chạy/tự thiết kế ít nhất 8 failure drills:

1. gửi cùng `Idempotency-Key` đồng thời 10 lần;
2. dependency trả `429 Retry-After`;
3. dependency timeout sau khi remote side effect có thể đã commit;
4. cache trả stale representation;
5. access token hết hạn / scope thiếu;
6. CORS preflight fail dù API endpoint vẫn hoạt động với `curl`;
7. webhook delivery lặp lại và out-of-order;
8. circuit breaker open rồi dependency recover.

Evidence lưu lại:

```text
request
response
trace/correlation ID
expected invariant
actual side effect count
retry/circuit state
metrics before/after
```

---

# 6. Exit criteria

Bạn hoàn thành Module 06 khi có thể:

- [ ] giải thích đúng cả 25 chủ đề trong coverage matrix;
- [ ] thiết kế endpoint bằng resource/method semantics thay vì CRUD naming máy móc;
- [ ] phân biệt authentication, authorization, access token, OAuth 2.0 và OIDC;
- [ ] thiết kế POST idempotency có race-condition protection;
- [ ] chọn offset vs cursor pagination dựa trên workload;
- [ ] thiết kế `ProblemDetails` + stable domain error code;
- [ ] mô tả HTTP caching bằng freshness + validators;
- [ ] implement rate limiting và giải thích algorithm/partition key;
- [ ] retry có exponential backoff + jitter và không retry unsafe operation mù quáng;
- [ ] giải thích circuit breaker state/threshold/recovery;
- [ ] chọn REST/GraphQL/gRPC theo consumer/data/workload thay vì trend;
- [ ] phân biệt WebSocket và SSE;
- [ ] thiết kế webhook signing + dedup + retry;
- [ ] tạo/read OpenAPI contract và biết backward-compatible change là gì;
- [ ] giải thích gateway responsibilities và điều gì **không nên** đặt ở gateway;
- [ ] có contract/integration tests ở public API boundary;
- [ ] có ít nhất một failure experiment + decision note.

## Verification metadata

- Verified: **2026-08-13**.
- Baseline: .NET / ASP.NET Core 10-oriented examples.
- Normative protocol sources: RFC/IETF/OpenAPI/GraphQL/gRPC official sources in [references](references.md).
- Supplementary perspectives reviewed: Craft Better Software archive and DesignGurus system-design/API articles.
