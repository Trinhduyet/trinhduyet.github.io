# API Security — Authentication, Authorization, Access Tokens, OAuth 2.0 và CORS

> [← Module 06](README.md) · [References](references.md)

## Hiểu trong 5 phút

Đừng trộn 5 khái niệm này:

```text
Authentication
  "Caller là ai / principal nào?"
        ↓
Authorization
  "Principal này được làm action gì trên resource nào?"
        ↓
Access token
  "Credential mang/đại diện quyền truy cập trong một thời gian/phạm vi"
        ↓
OAuth 2.0
  "Framework để client nhận delegated authorization"
        ↓
OpenID Connect
  "Identity layer trên OAuth để login / user authentication"
```

CORS nằm ở một trục khác:

```text
CORS
= browser policy cho cross-origin HTTP
≠ authentication
≠ authorization
≠ CSRF protection
```

---

# 1. Authentication vs Authorization

## Authentication — AuthN

Authentication tạo ra một `ClaimsPrincipal` hoặc equivalent identity context.

Ví dụ API nhận:

```http
GET /v1/orders/ord-123 HTTP/1.1
Authorization: Bearer eyJ...
```

Resource server cần verify token theo policy:

```text
issuer
signature / introspection
expiry
not-before
intended audience/resource
required token type
```

Sau khi token hợp lệ:

```text
HTTP request
   ↓
Authentication handler
   ↓
ClaimsPrincipal
   ↓
Authorization policy
   ↓
Endpoint/business capability
```

Authentication thành công **không có nghĩa** caller được phép đọc mọi order.

## Authorization — AuthZ

Authorization nên nói bằng business capability:

```text
orders:read
orders:write
refunds:create
admin:tenant-manage
```

Không chỉ:

```text
role == "Admin"
```

Role có thể là input cho policy, nhưng resource authorization thường còn cần ownership/tenant/state.

Ví dụ:

```text
User A authenticated
      ↓
GET /orders/order-of-user-B
      ↓
valid token
      ↓
but NOT owner and lacks support permission
      ↓
403 Forbidden
```

---

# 2. `401` và `403` không giống nhau

Mental model thực dụng:

| Status | Ý nghĩa API thường cần |
|---|---|
| `401 Unauthorized` | request chưa có/không có authentication credentials hợp lệ cho resource; response thường có `WWW-Authenticate` |
| `403 Forbidden` | caller được nhận diện nhưng không đủ quyền theo policy hiện tại |
| `404 Not Found` | resource không tồn tại **hoặc** API cố tình không reveal existence tùy threat model |

Không map mọi security failure thành `401`.

---

# 3. Access Token là gì?

Access token là credential để client gọi protected resource.

Bearer token có đặc tính quan trọng:

```text
ai cầm token
→ có thể dùng token
```

nếu token không sender-constrained bằng mechanism khác.

Vì vậy token phải được bảo vệ khỏi disclosure trong:

```text
logs
URLs
browser storage
telemetry
crash dump
support ticket
proxy traces
```

## Đừng log token

Bad:

```csharp
logger.LogInformation(
    "Authorization={Authorization}",
    http.Request.Headers.Authorization.ToString());
```

Better:

```csharp
logger.LogInformation(
    "Authenticated request subject={Subject} trace={TraceId}",
    http.User.FindFirst("sub")?.Value,
    Activity.Current?.TraceId.ToString());
```

Log identity metadata cần thiết, không log bearer credential.

---

# 4. JWT access token vs opaque access token

Access token **không bắt buộc là JWT**.

## JWT/self-contained

Resource server có thể validate locally nếu có signing key/metadata.

Pros:

```text
low per-request auth latency
no introspection network hop
claims available locally
```

Costs:

```text
revocation harder before expiry
claim staleness
larger credential
key rotation/validation complexity
risk of consumers coupling to token internals
```

## Opaque/reference token

Resource server cần introspection/gateway/token service tùy architecture.

Pros:

```text
central control/revocation easier
client cannot infer internal claims
smaller wire credential
```

Costs:

```text
introspection dependency
latency/cache strategy
availability coupling
```

Architectural rule:

> API business contract không nên phụ thuộc vào việc access token là JWT hay opaque nếu requirement không bắt buộc.

---

# 5. OAuth 2.0 mental model

OAuth có các role chính:

```text
Resource Owner / User
        │
        │ grants authorization
        ▼
Client Application
        │
        │ obtains access token
        ▼
Authorization Server
        │
        │ access token
        ▼
Client Application
        │
        │ Authorization: Bearer ...
        ▼
Resource Server / API
```

Điểm load-bearing:

```text
OAuth 2.0 = authorization framework
```

Nếu requirement là:

> "User login vào application và application cần identity claims"

thì thường cần **OpenID Connect** trên OAuth, không tự suy diễn identity từ access token.

---

# 6. Authorization Code + PKCE

Modern browser/native/web OAuth flow nên hiểu Authorization Code + PKCE.

Simplified:

```text
Client creates:
code_verifier
      ↓ hash
code_challenge

Client → Authorization Server
authorization request + code_challenge

User authenticates/consents

Authorization Server → Client
authorization code

Client → Token endpoint
authorization code + code_verifier

Server checks verifier matches original challenge
      ↓
access token
```

OAuth Security BCP RFC 9700 khuyến nghị/đòi hỏi PKCE behavior mạnh hơn legacy OAuth guidance; `S256` là challenge method phù hợp hiện tại.

Không xây flow mới dựa vào:

```text
Implicit Grant
Resource Owner Password Credentials Grant
```

chỉ vì tutorial cũ còn dùng chúng.

---

# 7. Scope, role, permission và resource ownership

Token có thể chứa scope:

```text
scope = "orders.read orders.write"
```

Nhưng API vẫn phải kiểm tra resource:

```text
scope orders.read
        +
order.TenantId == caller.TenantId
        +
order visible under business policy
        ↓
ALLOW
```

Scope không tự giải quyết multi-tenancy.

Bad:

```csharp
if (User.Identity?.IsAuthenticated == true)
{
    return await db.Orders.FindAsync(id);
}
```

Better direction:

```csharp
Order? order = await db.Orders
    .SingleOrDefaultAsync(
        x => x.Id == id && x.TenantId == tenantContext.TenantId,
        cancellationToken);

if (order is null)
    return Results.NotFound();

AuthorizationResult authz = await authorization.AuthorizeAsync(
    http.User,
    order,
    "CanReadOrder");

if (!authz.Succeeded)
    return Results.Forbid();

return Results.Ok(OrderResponse.From(order));
```

Defense in depth:

```text
query/data isolation
+
resource-based authorization
```

---

# 8. ASP.NET Core JWT bearer example

Conceptual configuration:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];

        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("orders:read", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "orders.read");
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/v1/orders/{id}", GetOrder)
    .RequireAuthorization("orders:read");
```

Đây chỉ là wiring. Production còn cần validate đúng issuer/audience/key metadata, TLS, resource policy, secrets và error handling.

---

# 9. API Keys khi nào hợp lý?

API key thường phù hợp cho:

```text
machine/client identification
partner integration đơn giản
metering/quota key
server-to-server use case có scope hạn chế
```

Không dùng API key như user login replacement.

Security baseline:

```text
store hash/secret securely
show once if possible
rotate
expire
scope
rate limit
never put in query string unless protocol forces it
```

Header example:

```http
X-Api-Key: <secret>
```

hoặc custom authentication scheme tùy contract.

---

# 10. CORS — đúng mental model

Browser same-origin policy kiểm soát script đọc cross-origin responses.

Same origin cần cùng:

```text
scheme + host + port
```

Ví dụ:

```text
https://app.example.com
https://api.example.com
```

khác origin vì host khác.

Browser có thể gửi preflight:

```http
OPTIONS /v1/orders HTTP/1.1
Origin: https://app.example.com
Access-Control-Request-Method: POST
Access-Control-Request-Headers: authorization,content-type
```

Server nếu cho phép sẽ trả headers tương ứng.

## CORS không bảo vệ API khỏi curl

Nếu CORS policy chặn browser app:

```bash
curl https://api.example.com/v1/orders
```

vẫn có thể gọi API nếu network/auth cho phép.

Do đó:

```text
CORS ≠ API authorization
```

---

# 11. ASP.NET Core CORS example

```csharp
const string FrontendPolicy = "frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendPolicy, policy =>
    {
        policy
            .WithOrigins("https://app.example.com")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("authorization", "content-type", "idempotency-key");
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors(FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
```

Không dùng:

```text
AllowAnyOrigin + credentials
```

như một shortcut.

---

# 12. CORS vs CSRF

Hai bài toán khác nhau.

CORS:

```text
browser script có được đọc cross-origin response không?
```

CSRF:

```text
attacker site có thể khiến browser gửi authenticated state-changing request ngoài ý muốn không?
```

Cookie-based authentication cần CSRF threat model riêng. Bearer token trong explicit `Authorization` header có threat model khác, nhưng XSS/token theft vẫn rất quan trọng.

---

# 13. Security response contract

Đừng leak chi tiết validation/token internals.

Bad:

```json
{
  "error": "JWT signature failed because key kid=abc was not found in cache"
}
```

Better public contract:

```http
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer
Content-Type: application/problem+json
```

```json
{
  "type": "https://api.example.com/problems/authentication-required",
  "title": "Authentication required",
  "status": 401,
  "code": "AUTHENTICATION_REQUIRED",
  "traceId": "00-..."
}
```

Internal logs giữ diagnostic details với access control phù hợp.

---

# 14. Security failure experiments

## A — expired token

Expected:

```text
401
no business side effect
no sensitive token logged
```

## B — valid token, wrong scope

Expected:

```text
403
```

## C — valid scope, wrong tenant/resource owner

Expected:

```text
403 or 404 according to threat model
no data leakage
```

## D — CORS denied

Browser:

```text
response unavailable to script
```

`curl` with valid auth:

```text
API may still return normal response
```

Điều này chứng minh CORS là browser control, không phải API authentication.

## E — token accidentally logged

Add automated test/log sanitizer proving `Authorization` header never appears in request logs.

---

# 15. Review checklist

- [ ] authentication mechanism documented;
- [ ] authorization is policy/resource based, not only `IsAuthenticated`;
- [ ] tenant boundary enforced before returning data;
- [ ] access token never placed in URL/log;
- [ ] issuer/audience/expiry validated;
- [ ] OAuth flow follows current BCP + PKCE;
- [ ] OIDC used when user identity/login is required;
- [ ] scopes are least privilege;
- [ ] CORS origins are explicit;
- [ ] CORS is not treated as AuthZ;
- [ ] API key has rotation/expiry/scope policy;
- [ ] 401/403/404 semantics are intentional;
- [ ] negative tests exist.

## Exit criteria

Bạn hoàn thành chapter khi có thể:

- giải thích AuthN vs AuthZ bằng một request cụ thể;
- phân biệt access token, ID token và API key;
- vẽ được OAuth Authorization Code + PKCE flow;
- giải thích tại sao OAuth 2.0 không đồng nghĩa login;
- implement policy authorization trong ASP.NET Core;
- implement resource/tenant authorization;
- cấu hình CORS đúng origin;
- chứng minh bằng test rằng CORS không thay API authorization.

## Official sources

Xem [references](references.md): RFC 6750, RFC 9700, RFC 9449, ASP.NET Core authentication/authorization/CORS documentation.

## Verification metadata

- Verified: **2026-08-13**.
- Security behavior should follow current OAuth BCP, not legacy tutorials.
