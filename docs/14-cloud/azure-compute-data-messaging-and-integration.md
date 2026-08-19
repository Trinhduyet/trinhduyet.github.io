# Azure Compute, Data, Messaging & Integration

> [← Identity & Networking](azure-identity-networking-and-zero-trust.md) · [Cloud & Azure](README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;Compute · Data · Messaging</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Question</strong>&nbsp;service nào phù hợp với workload nào?</span>
</div>

## 1. Đừng chọn service theo logo

Một architecture review nên bắt đầu bằng workload characteristics:

```text
Protocol
Traffic shape
Stateful / stateless
Latency budget
Scaling unit
Deployment model
Operational control needed
Data consistency
Message semantics
Team capability
Cost model
```

Sau đó mới map sang Azure.

## 2. Compute decision — App Service vs Functions vs Container Apps vs AKS

### Azure App Service

Phù hợp khi workload chủ yếu là web/API và team muốn PaaS đơn giản.

```text
HTTP API
+ managed hosting
+ deployment slots / scale features
+ minimal cluster ownership
→ App Service is a strong default candidate
```

Không có nghĩa App Service luôn đúng; nếu app cần Kubernetes primitives, custom scheduling hoặc platform-level cluster control, requirement đã khác.

### Azure Functions

Phù hợp với event-driven/trigger-based workload, đặc biệt traffic bursty/sporadic.

Ví dụ:

```text
Blob uploaded
→ Function
→ extract metadata
→ publish result
```

Cần nghĩ tới:

- execution duration/runtime model;
- concurrency;
- cold/start behavior depending on plan;
- retry semantics;
- poison messages;
- idempotency.

### Azure Container Apps

Container Apps phù hợp khi muốn deploy containers, autoscale/revisions và microservice/event-driven primitives mà không cần direct Kubernetes API/control plane.

Mental model:

```text
Need containers
+ per-app scaling
+ revisions / ingress
+ less cluster management
→ Container Apps candidate
```

### Azure Kubernetes Service (AKS)

AKS phù hợp khi Kubernetes API/ecosystem/control là requirement thật:

```text
custom operators
special scheduling/node pools
cluster-level networking policy/control
service mesh/platform standardization
Kubernetes-native deployment ecosystem
```

Nếu team không cần những capability này, AKS có thể chỉ thêm operating surface.

### Decision table

| Need | First candidate |
|---|---|
| simple HTTP/API PaaS | App Service |
| trigger/event function | Functions |
| container PaaS, per-app scale, revisions | Container Apps |
| Kubernetes API/control required | AKS |
| legacy/custom OS control | VM/VMSS class |

Đây là **starting heuristic**, không phải hard rule.

## 3. Data — source of truth trước service name

Trước database choice, viết:

```text
Entities
Access patterns
Transaction boundaries
Read/write ratio
Consistency requirements
Data size/growth
Partition key candidates
Retention
Recovery requirement
```

### Azure SQL

Phù hợp khi relational model + transactions + SQL query ecosystem là core requirement.

Example Order source of truth:

```text
Order
OrderLine
PaymentAttempt
OutboxMessage
```

Invariant:

```text
UNIQUE(OrderId, PaymentAttempt.BusinessKey)
```

hoặc transaction boundary cụ thể.

Azure SQL là managed database service; nó không tự thiết kế schema/index/transaction correctness cho bạn.

### Cosmos DB

Cosmos DB là candidate khi workload cần distributed NoSQL characteristics và partitioning model phù hợp.

Trước khi chọn, phải trả lời:

```text
partition key = ?
request units / workload = ?
query cross-partition bao nhiêu?
consistency level nào?
hot partition risk?
```

Sai partition key có thể biến “globally distributed database” thành bottleneck đắt tiền.

### Blob Storage

Object storage phù hợp với:

- files;
- images;
- receipts;
- export/import objects;
- large immutable/semi-immutable payloads.

Không lưu binary lớn trong relational DB chỉ vì “mọi data nên ở một DB” nếu access/lifecycle không phù hợp.

### Azure Managed Redis

Redis thường là **derived/temporary fast state**, không phải mặc định source of truth.

Các use case:

```text
cache
rate limit counters
short-lived coordination
session/projection depending on design
```

Phải xác định:

```text
TTL
invalidation
staleness budget
cache outage behavior
hot key
memory pressure
fallback
```

> Azure Cache for Redis đang trong lộ trình retirement; tài liệu mới nên ưu tiên Azure Managed Redis cho kiến trúc mới và kiểm tra migration guidance cho workload cũ.

## 4. Messaging — Event Grid vs Event Hubs vs Service Bus

Ba service giải quyết ba problem class khác nhau.

### Service Bus

Mental model:

```text
Business message / command / work item
→ durable enterprise messaging
→ queue/topic
→ consumer processing
```

Phù hợp với order processing, commands, work queues, pub/sub business integration khi cần delivery features như dead-lettering, duplicate detection/session/transactional semantics tùy feature/tier.

### Event Grid

Mental model:

```text
Something happened
→ lightweight discrete event notification
→ interested subscribers react
```

Ví dụ:

```text
BlobCreated
SubscriptionChanged
ResourceEvent
```

Publisher thường không dictate business workflow của subscriber.

### Event Hubs

Mental model:

```text
High-throughput ordered-per-partition event stream
→ ingest
→ multiple processors / replay-like stream consumption
```

Phù hợp telemetry, logs, clickstream, IoT/event streaming class hơn business command queue.

### Quick comparison

| Problem | Service class |
|---|---|
| durable enterprise command/work queue | Service Bus |
| discrete event routing | Event Grid |
| high-throughput event stream | Event Hubs |

Không chọn bằng rule:

```text
"Event-driven" → Event Grid
```

vì event-driven system có thể cần Service Bus, Event Hubs, Event Grid hoặc phối hợp nhiều service theo semantics khác nhau.

## 5. Queue không giải quyết duplicate

Giả sử:

```text
Service Bus message delivered
consumer commits DB
ACK lost / lock expires
message redelivered
```

Nếu consumer không idempotent:

```text
one logical business action
→ two side effects
```

Reference pattern:

```text
MessageId / BusinessId
        ↓
Inbox / dedup invariant
        ↓
Apply business transaction once
        ↓
ACK
```

“At least once” phải được thiết kế với duplicate như behavior bình thường, không phải exception hiếm.

## 6. Outbox on Azure

Business transaction:

```text
BEGIN TRANSACTION
  INSERT Order
  INSERT OutboxMessage
COMMIT
```

Publisher:

```text
read unpublished outbox
→ send Service Bus
→ mark published
```

Consumer:

```text
receive
→ dedup/inbox
→ apply local transaction
→ ACK
```

Outbox giảm dual-write gap giữa DB và broker; nó không tạo distributed exactly-once magic.

## 7. API integration

### API Management

API Management có thể cung cấp policy/auth/quota/routing/versioning facade, nhưng application vẫn phải chịu trách nhiệm:

```text
business validation
idempotency
transaction correctness
authorization where domain-specific
state machine
```

Gateway không nên trở thành nơi nhét business logic khó test/version.

### External API

Ví dụ payment provider:

```text
Checkout API
→ Payment Provider
→ timeout
```

Không map timeout thành failure ngay.

```text
HTTP timeout
= response unknown locally
```

Business state có thể cần:

```text
PENDING
UNKNOWN
SUCCEEDED
FAILED
```

và reconciliation job/webhook/query provider để resolve.

## 8. Example — Notification service

Requirements:

```text
50k notifications/min peak
email + push + SMS
provider quotas
retry temporary failures
avoid duplicate business notification when possible
user-visible status
```

Possible Azure design:

```text
API
 ↓
Azure SQL
(notification intent + idempotency)
 ↓ outbox
Service Bus Topic
 ├─ Email subscription → workers
 ├─ Push subscription  → workers
 └─ SMS subscription   → workers

Provider callbacks
→ status updates

Application Insights
→ latency / failure / backlog
```

Why not Event Hubs for the primary work queue?
Because the core requirement is **business work processing with delivery/workflow semantics**, not telemetry stream ingestion.

## 9. Failure matrix

| Failure | Danger | Design response |
|---|---|---|
| compute instance dies | in-flight request lost | stateless/retry + durable state |
| DB throttles | cascade/latency | bounded concurrency/backoff/capacity |
| cache unavailable | thundering herd | bounded fallback / degrade |
| queue backlog | stale work/SLO miss | autoscale + backlog SLI + shedding |
| duplicate message | duplicate side effect | idempotency/dedup |
| poison message | retry forever | DLQ + diagnostics/replay policy |
| external API timeout | UNKNOWN outcome | idempotency + reconcile |
| hot partition | uneven capacity | partition redesign/distribution |

## 10. Architect checklist

- [ ] Compute choice has workload reason.
- [ ] AKS is not default just because workload is containerized.
- [ ] Data source of truth and transaction boundary are explicit.
- [ ] Redis state is classified as authoritative or derived.
- [ ] Cosmos partition key has workload math.
- [ ] Messaging service chosen by semantics, not naming.
- [ ] Duplicate/redelivery behavior tested.
- [ ] DLQ/replay is operationally defined.
- [ ] External timeout maps to business UNKNOWN where appropriate.
- [ ] Quota/throttling limits are part of capacity planning.

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Azure gives nhiều managed building blocks, nhưng architecture vẫn là bài toán **semantic fit**: compute model, source of truth, consistency, message semantics và failure behavior phải khớp workload.
</div>

## Tiếp theo

→ [Azure Reliability, Observability, Governance & Cost](azure-reliability-observability-governance-and-cost.md)
