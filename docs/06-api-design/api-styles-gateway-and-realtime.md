# API Styles — REST, GraphQL, gRPC, API Gateway, Microservices, WebSockets và SSE

> [← Module 06](README.md) · [Module 18 — Microservices](../18-microservices-architecture/README.md) · [References](references.md)

## Hiểu trong 5 phút

Đừng hỏi:

> “REST, GraphQL hay gRPC cái nào tốt nhất?”

Hãy hỏi:

```text
consumer là ai?
data shape như thế nào?
latency/bandwidth budget?
streaming hay request-response?
browser/public ecosystem hay internal services?
contract evolution/tooling?
caching/observability/security constraints?
```

Sau đó chọn protocol/style.

| Style | Strength | Typical fit |
|---|---|---|
| REST/HTTP resource API | Web semantics, caching, broad interoperability | public APIs, CRUD/resource workflows, browser/mobile/partners |
| GraphQL | Client-shaped field selection qua typed schema | frontend aggregation, graph-like data, many client views |
| gRPC | Strong contract + codegen + efficient RPC/streaming | internal service-to-service, low-latency typed RPC |
| WebSocket | Full-duplex persistent connection | collaborative/chat/game/control-plane realtime |
| SSE | One-way server→client event stream over HTTP | notifications, progress, feeds, LLM/token streaming-like updates |
| Webhook | Server→server event callback | external integrations, async notifications |

---

# 1. REST mental model

REST-style HTTP API nên bắt đầu từ **resources + standard HTTP semantics**.

Example:

```http
GET    /v1/orders/123
POST   /v1/orders
PUT    /v1/orders/123
PATCH  /v1/orders/123
DELETE /v1/orders/123
```

Không biến route thành RPC verbs nếu resource model diễn đạt được rõ hơn:

Less resource-oriented:

```text
POST /getOrder
POST /updateOrder
POST /deleteOrder
```

Resource-oriented:

```text
GET    /orders/{id}
PATCH  /orders/{id}
DELETE /orders/{id}
```

Nhưng không dogmatic. Business command có thể hợp lý:

```http
POST /orders/{id}/cancellation
POST /invoices/{id}/payment-attempts
```

vì `cancellation`/`payment-attempt` có thể là resource/business operation thực sự có identity/state riêng.

---

# 2. REST strengths

```text
standard HTTP methods/status/headers
HTTP caching / conditional requests
CDN/proxy interoperability
simple debugging with curl/browser tools
broad client/tool support
resource-oriented contract dễ audit
```

Costs:

```text
over-fetch / under-fetch trong complex client screens
multiple round trips nếu client cần graph sâu
version/evolution pressure nếu representations cứng
aggregation có thể đẩy sang BFF/gateway/client
```

---

# 3. GraphQL mental model

GraphQL expose **typed schema**. Client chọn fields cần lấy.

Query example:

```graphql
query OrderScreen($id: ID!) {
  order(id: $id) {
    id
    status
    customer {
      displayName
    }
    items {
      productName
      quantity
    }
  }
}
```

Thay vì client gọi:

```text
GET /orders/123
GET /customers/45
GET /orders/123/items
GET /products/...
```

GraphQL server có thể aggregate theo schema/resolvers.

---

# 4. GraphQL production concerns

Client-shaped query không có nghĩa “client muốn gì cũng được”.

Phải kiểm soát:

```text
query depth
query complexity/cost
field-level authorization
N+1 resolver problem
batching/DataLoader
pagination
timeouts
persisted queries / allowlists when appropriate
schema deprecation
observability by operation name/hash
```

## N+1 example

Query 100 orders rồi mỗi order resolver query customer:

```text
1 query orders
+ 100 customer queries
= 101 DB calls
```

Mitigation:

```text
batch by customer IDs
DataLoader pattern
projection/join/read model
```

GraphQL không tự làm backend efficient.

---

# 5. GraphQL authorization

Bad:

```text
Top-level query authorized
→ every nested field assumed safe
```

Better:

```text
operation authorization
+
field/resource authorization
+
data source tenant filtering
```

Sensitive fields:

```graphql
type Customer {
  id: ID!
  displayName: String!
  taxId: String # requires separate permission
}
```

Schema flexibility tăng attack/cost surface nên query cost/rate policy quan trọng.

---

# 6. gRPC mental model

gRPC thường dùng Protocol Buffers để define contract:

```proto
syntax = "proto3";

package orders.v1;

service OrderService {
  rpc GetOrder(GetOrderRequest) returns (OrderReply);
  rpc WatchOrder(WatchOrderRequest) returns (stream OrderEvent);
}

message GetOrderRequest {
  string order_id = 1;
}

message OrderReply {
  string order_id = 1;
  string status = 2;
}
```

Generated client/server types giúp contract strongly typed.

---

# 7. gRPC strengths và costs

Strengths:

```text
compact binary serialization
strong schema
client/server code generation
HTTP/2 multiplexing
unary + streaming RPC
excellent internal polyglot contracts
```

Costs/trade-offs:

```text
less human-readable on wire
browser/public ecosystem not as universal as JSON/HTTP
schema field-number evolution rules must be respected
debugging requires gRPC-aware tooling
HTTP intermediaries/caching semantics khác REST representations
```

Không chọn gRPC chỉ vì “nhanh hơn”. Đo payload/latency/CPU và xem consumer ecosystem.

---

# 8. REST vs GraphQL vs gRPC decision table

| Question | REST | GraphQL | gRPC |
|---|---:|---:|---:|
| Public/browser interoperability | **Strong** | Strong | Medium / needs browser strategy |
| HTTP caching/CDN | **Strong** | Requires more deliberate strategy | Not primary model |
| Client-shaped data | Medium | **Strong** | Low/contract-fixed |
| Contract/code generation | OpenAPI | Schema tooling | **Strong** |
| Internal low-latency RPC | Good | Situational | **Strong** |
| Bidirectional/service streaming | Limited by separate mechanisms | Subscription ecosystem | **Strong** |
| Easy curl/debug | **Strong** | Strong | Medium |
| Fine-grained query abuse surface | Lower | **Higher** | Lower |

Không có winner tuyệt đối.

DesignGurus cũng nhấn mạnh rằng trong system design, lựa chọn REST/GraphQL/gRPC quan trọng ít hơn việc **defend choice theo requirement, data shape, latency và operational trade-off**.

---

# 9. API Gateway

Gateway là “front door” cho một tập backend services.

```text
Internet / Mobile / Partner
          ↓
      API Gateway
    ┌─────┼──────────────┐
    ↓     ↓              ↓
 Orders  Catalog       Identity-facing adapters
    ↓     ↓
  own DB own DB
```

Gateway responsibilities phù hợp:

```text
TLS termination
routing
host/path rewrite
authentication integration
token validation / coarse authorization
rate limiting / quotas
request size limits
WAF/security policy
observability/correlation
protocol transformation when justified
canary/traffic routing
```

---

# 10. Không nhét domain vào gateway

Bad:

```text
Gateway
  ├─ calculate discount
  ├─ decide order state
  ├─ reserve inventory business rule
  ├─ write shared DB
  └─ orchestrate every domain workflow
```

Gateway trở thành distributed monolith control center.

Better:

```text
Gateway owns edge policy
Service owns business capability
```

API composition/BFF có thể aggregate data, nhưng ownership phải rõ và bounded.

---

# 11. Gateway vs BFF

Generic gateway:

```text
cross-cutting edge concerns for many consumers
```

Backend for Frontend (BFF):

```text
consumer-specific API composition
Mobile BFF
Web BFF
Partner BFF
```

BFF hữu ích khi consumer data shape khác nhau rõ rệt, nhưng tránh duplicate business logic giữa BFFs.

---

# 12. Microservices communication

Câu:

> “Microservices = independent services talking only over APIs”

chưa đủ chính xác.

Microservices có thể giao tiếp:

```text
synchronous HTTP/REST
gRPC
async message/event
stream
webhook (external integration)
```

Quan trọng hơn transport là:

```text
business boundary
data ownership
contract ownership
independent deployment
failure isolation
team ownership
```

Xem [Module 18 — Microservices Architecture](../18-microservices-architecture/README.md).

---

# 13. Sync vs async

Sync request-response:

```text
Order API → Payment API
caller waits
```

Good when:

```text
caller needs immediate answer
operation is short
availability coupling acceptable
failure semantics understood
```

Async:

```text
Order Service
  ↓ event/command
Broker
  ↓
Payment consumer
```

Good when:

```text
work can complete later
buffering/backpressure helps
producer/consumer cadence differs
replay/durability useful
```

Async không miễn phí: ordering, duplication, lag, schema evolution và operations xuất hiện.

---

# 14. WebSocket

WebSocket cho persistent full-duplex communication:

```text
Client ⇄ Server
```

Fit:

```text
chat
collaborative editing
realtime dashboards with client commands
multiplayer/game state
interactive control plane
```

Production concerns:

```text
connection lifecycle
heartbeat/ping
reconnect
session/state routing
fan-out
backpressure
message ordering
per-connection memory
load balancer timeout
horizontal scale
```

Không coi “connection open” là delivery guarantee.

---

# 15. SSE — Server-Sent Events

SSE là one-way server→client event stream:

```text
Browser / client
     ↑
     │ text/event-stream
Server
```

HTTP response:

```http
HTTP/1.1 200 OK
Content-Type: text/event-stream
Cache-Control: no-cache
```

Stream body:

```text
id: 101
event: order-status
data: {"orderId":"123","status":"PAID"}

id: 102
event: order-status
data: {"orderId":"123","status":"SHIPPED"}

```

SSE supports event IDs and reconnect behavior through `EventSource` semantics.

---

# 16. WebSocket vs SSE

| Requirement | WebSocket | SSE |
|---|---:|---:|
| Server → client only | Works | **Excellent** |
| Client → server frequent realtime messages | **Excellent** | Use normal HTTP separately |
| Text/event semantics | Custom framing/app protocol | **Built-in** |
| Auto reconnect browser API | Implement carefully | **EventSource supports reconnect** |
| Existing HTTP infrastructure friendliness | Medium | **High** |
| Binary messages | **Yes** | No, text stream |

Nếu requirement chỉ là:

```text
server push progress/status/feed
```

SSE thường đơn giản hơn WebSocket.

---

# 17. ASP.NET Core SSE-style example

Conceptual streaming endpoint:

```csharp
app.MapGet("/v1/orders/{id}/events", async (
    string id,
    HttpResponse response,
    IOrderEventStream stream,
    CancellationToken cancellationToken) =>
{
    response.Headers.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";

    await foreach (OrderEvent evt in stream.ReadAsync(id, cancellationToken))
    {
        await response.WriteAsync(
            $"id: {evt.Sequence}\n" +
            $"event: order-status\n" +
            $"data: {JsonSerializer.Serialize(evt)}\n\n",
            cancellationToken);

        await response.Body.FlushAsync(cancellationToken);
    }
});
```

Production cần authorization, connection limits, heartbeat/reconnect semantics và backpressure.

---

# 18. WebSocket ASP.NET Core sketch

```csharp
app.UseWebSockets();

app.Map("/ws/orders", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    using WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

    // receive/send loop with bounded buffers, cancellation and close handling
});
```

Không deploy loop vô hạn không có max message size / cancellation / metrics.

---

# 19. Gateway failure modes

| Failure | Risk | Response |
|---|---|---|
| Gateway down | broad outage | HA/multi-instance + health routing |
| Auth metadata provider slow | all protected routes slow | bounded cache + timeout + operational policy |
| Rate-limit store down | fail-open/closed ambiguity | per-route explicit decision |
| Gateway config error | huge blast radius | config validation + canary + rollback |
| Aggregation fan-out slow | tail latency amplification | deadlines + parallelism bound + partial response policy |
| Request transformation drift | hidden contract break | contract tests + versioned config |

Gateway is high blast radius. Treat config like production code.

---

# 20. Failure experiments

## A — GraphQL expensive query

Create deep/wide query; verify query complexity/depth/cost controls reject before DB meltdown.

## B — Gateway rate-limit store outage

Record whether sensitive endpoint fails open or closed and why.

## C — gRPC incompatible schema change

Remove/reuse a field number in a test branch; demonstrate compatibility risk. Learn Protobuf evolution rules.

## D — WebSocket reconnect storm

Restart server with many clients. Observe connection QPS, CPU and retry jitter.

## E — SSE disconnect/reconnect

Drop connection after event ID 100; verify client can reason about resume/replay policy.

---

# 21. Architect review questions

1. Consumer thật sự cần REST, GraphQL hay gRPC?
2. API Gateway đang làm edge policy hay đã chứa domain logic?
3. Gateway outage blast radius là gì?
4. Microservice contract sync nào tạo availability chain?
5. GraphQL query cost bound ở đâu?
6. Realtime cần bidirectional hay chỉ server push?
7. Connection count × memory per connection = bao nhiêu?
8. Schema evolution strategy của OpenAPI/GraphQL/Proto là gì?
9. Có thể dùng simpler HTTP request/response thay persistent connection không?
10. Failure/reconnect/retry behavior có test chưa?

## Exit criteria

- phân biệt REST/GraphQL/gRPC bằng workload;
- giải thích GraphQL N+1/query-cost/security concerns;
- đọc/viết `.proto` basic service contract;
- thiết kế API Gateway responsibilities mà không nhét domain logic;
- nối API contract với Module 18 microservice boundaries;
- chọn WebSocket vs SSE đúng use case;
- có failure experiment cho gateway/realtime/protocol evolution.

## Verification metadata

- Verified: **2026-08-13**.
- GraphQL/gRPC/WebSocket/SSE behavior should be verified against current official specifications/docs in [references](references.md).
