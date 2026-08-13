# API Evolution, Error Handling, Pagination và OpenAPI

> [← Module 06](README.md) · [HTTP Contracts](http-resource-contracts-and-semantics.md) · [References](references.md)

## Hiểu trong 5 phút

Một API tồn tại lâu hơn version code đầu tiên.

Bạn phải thiết kế cho:

```text
more data
more consumers
new fields
new behavior
old mobile apps
partial deployments
client mistakes
server failures
```

Bốn trụ cột chapter này:

```text
Pagination
→ bound data/latency

Error Contract
→ predictable recovery

Compatibility / Versioning
→ independent evolution

OpenAPI
→ executable contract artifact
```

---

# 1. Pagination là correctness + performance contract

Không trả collection vô hạn:

```http
GET /v1/orders
```

nếu tenant có thể có hàng triệu rows.

API phải bound:

```text
page size
sort order
filter domain
continuation state
```

Example:

```http
GET /v1/orders?limit=50&status=paid
```

---

# 2. Offset pagination

Request:

```http
GET /v1/orders?offset=100&limit=50
```

SQL shape:

```sql
SELECT Id, CreatedAt, Status, Total
FROM Orders
WHERE TenantId = @tenantId
ORDER BY CreatedAt DESC, Id DESC
OFFSET @offset ROWS
FETCH NEXT @limit ROWS ONLY;
```

Pros:

```text
simple
supports page-number UX
jump to approximate page
```

Costs:

```text
deep offsets can be expensive
concurrent inserts/deletes can shift pages
duplicate/missing items between page requests
```

Use when data size/deep-navigation requirements allow it.

---

# 3. Cursor / keyset pagination

Use stable ordering key:

```text
ORDER BY CreatedAt DESC, Id DESC
```

First page:

```sql
SELECT TOP (@limit)
    Id, CreatedAt, Status, Total
FROM Orders
WHERE TenantId = @tenantId
ORDER BY CreatedAt DESC, Id DESC;
```

Next page cursor contains last seen key:

```text
createdAt=2026-08-13T02:00:00Z
id=ord-500
```

Query:

```sql
SELECT TOP (@limit)
    Id, CreatedAt, Status, Total
FROM Orders
WHERE TenantId = @tenantId
  AND (
      CreatedAt < @createdAt
      OR (CreatedAt = @createdAt AND Id < @id)
  )
ORDER BY CreatedAt DESC, Id DESC;
```

Pros:

```text
stable forward traversal
avoids large OFFSET scan pattern
better under inserts for feed/timeline-like access
```

Costs:

```text
no arbitrary page jump
cursor encoding/versioning needed
sort fields must support deterministic seek
```

---

# 4. Cursor phải opaque với client

Public response:

```json
{
  "items": [
    { "id": "ord-501", "createdAt": "2026-08-13T02:01:00Z" },
    { "id": "ord-500", "createdAt": "2026-08-13T02:00:00Z" }
  ],
  "nextCursor": "eyJ2IjoxLCJjcmVhdGVkQXQiOiIyMDI2LTA4LTEzVDAyOjAwOjAwWiIsImlkIjoib3JkLTUwMCJ9"
}
```

Client không nên parse/rely on internal cursor fields.

Server có thể encode:

```csharp
public sealed record OrderCursor(
    int Version,
    DateTimeOffset CreatedAt,
    string Id);
```

Version cursor để migration sau này khả thi.

---

# 5. Pagination invariants

Phải có:

```text
limit >= 1
limit <= MaxPageSize
deterministic ORDER BY
stable tie-breaker (often ID)
filter is part of cursor semantics
cursor belongs to same tenant/query shape
```

Bad:

```sql
ORDER BY CreatedAt DESC
```

nếu nhiều rows có same timestamp và không có tie-breaker.

Better:

```sql
ORDER BY CreatedAt DESC, Id DESC
```

---

# 6. Response metadata

Cursor response thường cần:

```json
{
  "items": [],
  "nextCursor": "...",
  "hasMore": true
}
```

Đừng luôn trả `totalCount` nếu count rất expensive và UI không thật sự cần.

`COUNT(*)` trên large filtered dataset cũng là workload phải budget.

---

# 7. Error handling — status + machine-readable body

Error contract phải giúp client quyết định:

```text
fix input?
authenticate?
ask permission?
retry later?
refresh state?
show user message?
contact support?
```

Một predictable shape tốt hơn random strings.

Use Problem Details media type:

```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json
```

```json
{
  "type": "https://api.example.com/problems/order-state-conflict",
  "title": "Order state conflict",
  "status": 409,
  "detail": "The order can no longer be cancelled.",
  "instance": "/v1/orders/ord-123/cancellation",
  "code": "ORDER_NOT_CANCELLABLE",
  "traceId": "00-a1b2..."
}
```

---

# 8. Stable domain error code

Human `title/detail` có thể đổi wording/localization.

Client logic nên dựa vào stable documented code:

```text
ORDER_NOT_CANCELLABLE
PAYMENT_ALREADY_CAPTURED
IDEMPOTENCY_KEY_CONFLICT
RATE_LIMITED
```

Không để client parse:

```text
"Order cannot be cancelled because..."
```

bằng substring.

---

# 9. Error taxonomy

| Category | Example | Typical status |
|---|---|---:|
| Syntax/shape invalid | malformed JSON | 400 |
| Validation | empty items, invalid date | 400/422 per API convention |
| Authentication | missing/expired access token | 401 |
| Authorization | caller lacks permission | 403 |
| Not found | unknown resource | 404 |
| State conflict | already completed/cancelled | 409 |
| Preconditions | stale ETag | 412 |
| Rate policy | quota exceeded | 429 |
| Unexpected server fault | null/ref bug, invariant leak | 500 |
| Temporary availability | overload/maintenance | 503 |

Không map domain conflict thành 500.

---

# 10. ASP.NET Core Problem Details

Minimal example:

```csharp
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
```

Endpoint-specific problem:

```csharp
return TypedResults.Problem(
    statusCode: StatusCodes.Status409Conflict,
    title: "Order state conflict",
    type: "https://api.example.com/problems/order-state-conflict",
    extensions: new Dictionary<string, object?>
    {
        ["code"] = "ORDER_NOT_CANCELLABLE",
        ["traceId"] = Activity.Current?.TraceId.ToString()
    });
```

Không expose stack trace/database SQL trong public `detail`.

---

# 11. Correlation / trace context

Client support ticket nói:

```text
"Request failed at 10:02"
```

không đủ.

Response có thể expose safe trace identifier:

```json
{
  "traceId": "4f3a..."
}
```

Internal telemetry nối:

```text
API request
→ DB query
→ payment HTTP call
→ message publish
```

DesignGurus API checklist nhấn mạnh observability/correlation ID như một phần API design, không phải việc làm sau release.

---

# 12. API evolution — compatible before versioned

Best default:

```text
prefer additive backward-compatible change
```

Usually compatible:

```text
add optional response field
add new endpoint
add optional request property with safe default
add new enum value ONLY if consumers are designed for unknown values
```

Potentially breaking:

```text
rename/remove field
change field type
make optional field required
change meaning of existing status/code
change units/timezone semantics
change pagination order
reuse enum meaning
```

Compatibility is behavioral, not just JSON-schema shape.

---

# 13. Tolerant readers — nhưng có giới hạn

Consumer nên ignore unknown response fields khi ecosystem supports it.

Producer không nên assume mọi generated client tolerates unknown enum value.

Example:

Old enum:

```text
PENDING
PAID
FAILED
```

New producer adds:

```text
REQUIRES_REVIEW
```

Old strict client có thể crash deserialize.

Safer local mapping:

```csharp
PaymentStatus Map(string wireStatus) => wireStatus switch
{
    "PENDING" => PaymentStatus.Pending,
    "PAID" => PaymentStatus.Paid,
    "FAILED" => PaymentStatus.Failed,
    _ => PaymentStatus.Unknown
};
```

Contract evolution must account for real client tooling.

---

# 14. API Versioning

Versioning is a cost, not a badge.

Possible strategies:

```text
URI      /v1/orders
header   custom/version media/header convention
host     v2.api.example.com
```

URI versioning is easy to see/debug but duplicates route surface.

Header/media versioning keeps URI stable but tooling/discovery can be less obvious.

No universal winner.

---

# 15. When to introduce v2

Do not create v2 for every added field.

Consider version boundary when:

```text
old behavior cannot coexist safely
business semantics changed materially
wire type incompatible
resource model changed fundamentally
security policy requires incompatible contract
```

Even with v2, migration needs:

```text
support window
consumer inventory
usage telemetry
deprecation notice
migration guide
shutdown criteria
```

---

# 16. Deprecation lifecycle

A mature deprecation flow:

```text
announce
↓
document replacement
↓
measure consumer usage
↓
warn in docs/headers/dashboard
↓
contact critical consumers
↓
stop new onboarding
↓
retire after policy window
```

Do not remove endpoint because “code search shows no internal references”. External clients are invisible to repo search.

---

# 17. OpenAPI

OpenAPI Specification is a language-agnostic contract format for HTTP APIs.

A minimal sketch:

```yaml
openapi: 3.1.0
info:
  title: Orders API
  version: 1.0.0
paths:
  /v1/orders/{id}:
    get:
      operationId: getOrder
      parameters:
        - in: path
          name: id
          required: true
          schema:
            type: string
      responses:
        '200':
          description: Order found
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Order'
        '404':
          description: Order not found
```

OpenAPI latest published spec line is currently 3.2.x, nhưng project tooling support có thể đi chậm hơn. **Pin version project thực sự support** thay vì nâng chỉ để “latest”.

---

# 18. OpenAPI dùng để làm gì?

```text
documentation
SDK/client generation
server stubs
mock servers
contract tests
request/response validation
breaking-change diff
security review
API catalog/discovery
```

OpenAPI không tự bảo đảm API tốt. Nó chỉ làm contract machine-readable.

Garbage contract → machine-readable garbage.

---

# 19. Contract-first workflow

```text
Business scenario
   ↓
API contract draft
   ↓
Consumer review
   ↓
OpenAPI / schema
   ↓
Mock / generated client
   ↓
Tests
   ↓
Implementation
```

DesignGurus gọi contract-first là một senior API design habit. Craft Better Software/TDD perspective cũng tương thích: public contract/test là nơi thiết kế observable behavior trước implementation detail.

---

# 20. Code-first vs contract-first

## Code-first

```text
C# endpoint/types
→ generate OpenAPI
```

Good:

```text
fast internal development
source and schema close together
```

Risk:

```text
implementation accident leaks into contract
consumer feedback comes late
```

## Contract-first

```text
OpenAPI/schema
→ review/generate
→ implementation
```

Good:

```text
consumer agreement early
polyglot teams
external API governance
```

Cost:

```text
schema workflow/tooling discipline
code/schema drift if automation weak
```

Many teams use hybrid: contract review + code-generated validation.

---

# 21. OpenAPI breaking-change gate

CI concept:

```text
base OpenAPI
   ↓ compare
new OpenAPI
   ↓
breaking change?
   ├─ no → continue
   └─ yes → require version/migration approval
```

Examples to flag:

```text
removed operation
removed response field
required request property added
response type changed
security requirement strengthened incompatibly
```

Exact checker depends on toolchain; policy is more important than brand.

---

# 22. Request/response examples are part of docs

Schema alone often insufficient.

Show:

```text
happy path
validation error
conflict
rate limited
async accepted
pagination continuation
```

Example docs should use realistic IDs/timestamps but never production secrets/data.

---

# 23. API testing strategy

Test from public boundary where practical:

```text
HTTP request
   ↓
route + auth + validation
   ↓
application behavior
   ↓
response + side effect
```

A contract test should fail when observable contract breaks, not whenever internal class names change.

Useful gates:

```text
OpenAPI snapshot/diff
integration tests
consumer contract tests
property-based API invariants
schema validation
load tests for page limits
security negative tests
```

---

# 24. Property/invariant examples

For paged API:

```text
items.Count <= limit
nextCursor from tenant A cannot be used to read tenant B
results preserve documented sort order
IDs do not duplicate across stable traversal under test model
```

For error API:

```text
every 4xx/5xx problem has traceId
stable machine code belongs to documented registry
no stack trace in public detail
```

Deterministic guardrails catch regression better than prose-only guidelines.

---

# 25. Failure experiments

## A — pagination drift

Run offset pages while inserting rows. Observe duplicates/missing items; compare cursor strategy.

## B — deep offset

Measure execution plan/latency at offset 0 vs 100k+.

## C — unknown enum

Deserialize new enum value with old client. Verify fallback or failure is understood.

## D — OpenAPI breaking diff

Remove a required response property or operation; ensure CI gate catches it.

## E — error leak

Throw DB exception; verify public Problem Details does not expose connection string/SQL/stack.

## F — deprecation visibility

Record consumer usage before retirement; prove which clients still call old version.

---

# 26. Exit criteria

- [ ] choose offset vs cursor/keyset based on workload;
- [ ] implement deterministic order + opaque cursor;
- [ ] design Problem Details + stable domain code;
- [ ] distinguish 400/401/403/404/409/412/429/500/503;
- [ ] classify compatible vs breaking changes;
- [ ] explain why versioning is not needed for every change;
- [ ] design deprecation lifecycle with usage evidence;
- [ ] read/write OpenAPI document;
- [ ] run an API contract/breaking-change diff;
- [ ] write public behavior tests independent of internal refactor.

## Verification metadata

- Verified: **2026-08-13**.
- Problem Details follows current HTTP API problem-detail standard family; see RFC references.
- OpenAPI spec currently publishes 3.2.x; project tooling compatibility must be verified before pinning.
