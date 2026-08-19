# Quality Attributes, Boundaries & Architecture Styles

> [← Software Architecture](README.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Question</strong>&nbsp;architecture phải tối ưu cái gì?</span>
</div>

## 1. Functional requirement chưa đủ để chọn architecture

Hai hệ thống đều có feature:

```text
POST /orders
```

nhưng architecture có thể khác hoàn toàn nếu NFR khác:

```text
System A
- 20 RPS
- one team
- 99.5%
- simple reporting

System B
- 20k RPS
- multiple regulated teams
- 99.99%
- strict audit
- regional isolation
```

Quality attributes tạo **architectural forces**.

## 2. Các quality attributes quan trọng

### Reliability

Hệ thống tiếp tục phục vụ/recover đúng qua failure.

Questions:

```text
What can fail?
What must remain correct?
How quickly recover?
How much data loss acceptable?
```

### Performance

```text
latency
throughput
resource efficiency
```

Không tối ưu performance trước khi có workload/measurement.

### Scalability

Khả năng giữ acceptable behavior khi workload tăng.

### Security

```text
identity
authorization
confidentiality
integrity
audit
threat resistance
```

### Modifiability

Một change business nhỏ chạm bao nhiêu module/team/deployable?

### Deployability

Có thể release phần nào độc lập, rollback an toàn tới đâu?

### Observability / Operability

Có thể detect, diagnose, recover không?

### Cost Efficiency

Architecture đạt business outcome với cost chấp nhận được không?

### Compliance / Auditability

Có trace/evidence/segregation/retention cần thiết không?

## 3. Quality attribute scenarios

Đừng ghi:

```text
System should be scalable.
```

Ghi:

```text
When peak checkout traffic grows from 2k/min to 10k/min,
system must keep P95 intake latency < 500 ms
without violating payment idempotency.
```

Một scenario tốt có:

```text
stimulus
source
environment
artifact
response
measure
```

## 4. Boundary types

Architecture có nhiều loại boundary:

### Code/module boundary

```text
Orders namespace/module
Payments namespace/module
```

### Data ownership boundary

```text
Orders owns order state
Payments owns payment state
```

### Process/runtime boundary

```text
Orders process
Payments process
```

### Deployment boundary

Có thể deploy độc lập.

### Team ownership boundary

Ai on-call / approve / evolve?

### Security/trust boundary

Identity/data/network privilege thay đổi ở đâu?

### Transaction boundary

State nào commit atomically cùng nhau?

Không nên assume tất cả boundaries trùng nhau.

Ví dụ một modular monolith:

```text
module boundary       = separate
process boundary      = same
transaction boundary  = potentially same DB/local transaction
team boundary         = maybe separate
```

## 5. Coupling — phải biết mình đang trả giá gì

### Compile-time coupling

Module A reference type/package của B.

### Runtime coupling

A cần B alive để hoàn thành request.

### Data coupling

A/B share schema/table hoặc phụ thuộc field shape.

### Temporal coupling

A và B phải available cùng lúc.

### Semantic coupling

A phải biết business meanings/internal states của B.

### Deployment coupling

Change A bắt buộc release B.

Architecture tốt không loại bỏ coupling; nó **đặt coupling ở nơi chấp nhận được và làm nó explicit**.

## 6. Layered architecture

```text
UI/API
 ↓
Application
 ↓
Business/Domain
 ↓
Infrastructure/Data
```

### Good fit

- moderate CRUD/business app;
- team cần convention rõ;
- dependencies reasonably one-directional.

### Failure mode

Mọi request đi qua giant layers:

```text
Controller
→ Service
→ Manager
→ Repository
→ GenericRepository
```

nhưng business behavior vẫn scattered.

Layered architecture không đòi “mỗi layer phải có interface” nếu abstraction không có value.

## 7. Modular Monolith

```text
Single deployable
├── Orders
├── Payments
├── Inventory
└── Notifications
```

Module rules:

```text
owns its model/data
explicit public contract
no random cross-module table access
internal details hidden
```

### Gains

- local debugging;
- simpler deploy;
- local transactions where useful;
- fewer distributed failure modes.

### Costs

- one process blast radius;
- independent scaling/deploy limited;
- bad discipline can collapse modules into big ball of mud.

## 8. Microservices

Runtime/deployment/data/team boundaries stronger.

```text
Orders Service
Payments Service
Inventory Service
```

### Justification signals

- separate team ownership;
- independent deployment cadence;
- distinct scaling profile;
- compliance/fault isolation;
- runtime specialization.

### Costs

```text
network latency/failure
contract evolution
idempotency
observability
data consistency
platform operations
```

## 9. Event-Driven Architecture

```text
Producer
→ Event Broker
→ Consumers
```

### Fit

- multiple reactions;
- temporal decoupling;
- eventual workflows acceptable;
- async scale/load leveling.

### Costs

- duplicate;
- ordering;
- schema evolution;
- replay;
- eventual consistency;
- harder tracing/debugging.

## 10. Serverless architecture

Functions/event services reduce server management for suitable workload.

Good for trigger/event/bursty work.

Architectural concerns remain:

```text
execution model
cold/start behavior
state
retry
idempotency
vendor quotas
cost at scale
```

Serverless không có nghĩa “stateless business process”. Durable business state vẫn cần ownership.

## 11. SOA vs Microservices

SOA là broader service-oriented approach; microservices thường nhấn mạnh smaller autonomous services/independent deployment/team ownership.

Đừng biến distinction thành battle of labels. Review:

```text
service boundary
contract
ownership
data
integration
runtime/deployment
```

thay vì chỉ hỏi “đây có thật sự là microservice không?”.

## 12. Architecture choice matrix

| Context | Likely starting point |
|---|---|
| small/medium business app | structured/layered monolith |
| complex domain, one/few teams | modular monolith + DDD boundaries |
| feature-centric backend | vertical slices within modules |
| many external adapters | hexagonal/ports-adapters useful |
| independent teams/deployments | selective microservices |
| many async reactions | EDA selectively |
| trigger/bursty workloads | serverless candidate |

Không phải hard rules. Mục tiêu là **simplest structure satisfying prioritized qualities**.

## 13. Example — Trading notification system

Requirements:

```text
Order execution event
→ mobile push
→ email
→ audit

Notification can lag seconds
Trading order path must not wait on notification provider
```

Architecture force:

```text
low temporal coupling
failure isolation
fanout
```

EDA/queue có thể justified.

Nhưng order acceptance itself vẫn cần transactional source of truth; không biến mọi state thành event chỉ vì notification async.

## 14. Common architecture smells

### Pattern-first

```text
We use CQRS + MediatR + Kafka because best practice.
```

Không có force/evidence.

### Distributed monolith

Services deploy riêng nhưng:

```text
share DB
synchronous chain
release together
```

nhận cost distributed nhưng không nhận autonomy.

### Shared library coupling

All services depend on giant shared domain package → one change upgrades whole fleet.

### Layer explosion

Interfaces/DTO/mappers/handlers cho mọi one-line operation làm signal-to-noise thấp.

### Architecture by org chart blindly

Org/team structure quan trọng, nhưng không nên copy team boundaries nếu domain/data flow chống lại hoàn toàn.

## 15. Review template

```text
Top 3 quality attributes
↓
Critical business invariants
↓
Ownership boundaries
↓
Current coupling
↓
Simplest architecture option
↓
Alternative options
↓
Trade-offs
↓
Failure / operations / cost
↓
Evidence and revisit trigger
```

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Architecture style là **response to forces**, không phải identity của project. Hãy ưu tiên quality attributes và boundaries trước khi chọn tên pattern.
</div>
