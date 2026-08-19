# Module 25 — Software Architecture

> [← System Design](../24-system-design/README.md) · [Master Roadmap](../00-roadmap/master-roadmap.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;boundaries · quality attributes · evolution</span>
  <span><strong>Mode</strong>&nbsp;decision-first</span>
</div>

Software Architecture không phải bộ sưu tập pattern names. Nó là cách **cấu trúc hệ thống để các quality attributes quan trọng được bảo vệ trong khi hệ thống vẫn có thể thay đổi**.

Mental model:

```text
Business domain
      ↓
Quality attributes / constraints
      ↓
Boundaries + ownership
      ↓
Architecture style
      ↓
Integration + data ownership
      ↓
Deployment / operations
      ↓
Evolution / migration
      ↓
Evidence + ADR + fitness functions
```

<div class="key-takeaway" markdown>
<strong>Architecture = decisions under constraints.</strong>

Clean Architecture, DDD, CQRS, Event-Driven Architecture hay Microservices chỉ là tools. Chúng không tốt tự thân; chúng chỉ tốt khi giải quyết đúng coupling, ownership, quality-attribute hoặc evolution problem.
</div>

## Learning path

| Guide | Bạn sẽ học |
|---|---|
| [Quality Attributes, Boundaries & Architecture Styles](quality-attributes-boundaries-and-styles.md) | reliability/performance/security/modifiability → boundary/style decision |
| [DDD, Modular Monolith & Microservices](ddd-modular-monolith-and-microservices.md) | domain ownership, bounded contexts, modularity, decomposition triggers |
| [Clean, Hexagonal, Vertical Slice & Application Structure](clean-hexagonal-and-vertical-slice.md) | dependency direction, ports/adapters, feature slices và testability |
| [Event-Driven, CQRS & Integration Architecture](event-driven-cqrs-and-integration.md) | sync/async, domain/integration events, CQRS, outbox, contract evolution |
| [Architecture Decisions, Fitness Functions & Evolution](architecture-decisions-evolution-and-fitness-functions.md) | ADR, governance, architecture tests, strangler/migration/evolution |
| [Architecture Review Playbook](architecture-review-playbook.md) | cách review một hệ thống thật từ business tới failure/cost |
| [References](references.md) | user-supplied architecture scope + primary guidance |

## 1. Quality attributes trước style

Đừng hỏi:

```text
Should we use microservices?
```

Hỏi:

```text
What must be independently deployable?
What must be strongly consistent?
What failure blast radius is acceptable?
How fast must change ship?
Which team owns which data/capability?
What latency/cost/operational burden can we afford?
```

Một style là response tới forces này.

## 2. Architecture styles — mỗi cái tối ưu khác nhau

### Layered / N-tier

```text
Presentation
↓
Application
↓
Domain / Business
↓
Data / Infrastructure
```

Useful khi application đơn giản và team cần structure dễ hiểu.

Failure mode:

```text
all features cross every layer
→ giant services
→ change impact broad
```

### Clean / Onion / Hexagonal

Tập trung dependency direction và bảo vệ core/application logic khỏi framework/external details.

```text
External adapters
      ↓
Ports
      ↓
Application / Domain
```

Useful khi testability/domain logic/external integration boundary quan trọng.

Failure mode: abstraction ceremony quá mức cho CRUD đơn giản.

### Vertical Slice

Organize theo feature/use case thay vì technical layer xuyên toàn app.

```text
CreateOrder
  command
  validation
  handler
  data access

GetOrder
  query
  projection
```

Useful khi feature autonomy/readability quan trọng.

### Modular Monolith

Một deployable process nhưng domain modules có boundary/data ownership/contract rõ.

```text
Application
├── Orders
├── Payments
├── Inventory
└── Notifications
```

Đây thường là bước tốt trước microservices vì giữ local transaction/debug/deploy simplicity.

### Microservices

Independent services với ownership/data/deployment lifecycle riêng.

Gain:
- independent deployment/scaling/ownership;
- fault/isolation opportunities.

Cost:
- network failure;
- distributed data consistency;
- contracts/versioning;
- observability;
- platform/operations.

### Event-Driven Architecture

Components react/publish events asynchronously.

Gain:
- temporal decoupling/fanout.

Cost:
- eventual state;
- duplicate/ordering/schema/replay complexity.

## 3. Domain boundary ≠ service boundary automatically

DDD Bounded Context giúp định nghĩa language/model ownership.

```text
Bounded Context
!=
always one microservice
```

Một modular monolith có thể chứa nhiều bounded contexts trước khi runtime/deployment separation justified.

## 4. Data ownership là architecture boundary mạnh

Nếu hai modules/services cùng sửa trực tiếp một table:

```text
Service A ─┐
           ├→ shared_orders table
Service B ─┘
```

thì deployment boundary có thể chỉ là hình thức.

Better ownership:

```text
Orders owns Orders data
Payments owns Payment data
Integration through explicit contract/event/query boundary
```

Không có nghĩa tuyệt đối không được shared DB trong mọi phase; nhưng nếu shared, phải ghi rõ coupling và migration plan.

## 5. Sync vs Async là architecture decision

### Sync

```text
Order API
→ Inventory API
→ Payment API
```

Simple immediate outcome, nhưng latency/failure chain dài.

### Async

```text
Order committed
→ event/message
→ downstream work
```

Decouple thời gian nhưng tạo state machine/eventual result/replay concerns.

Không dùng async chỉ vì “microservices should use events”.

## 6. CQRS

CQRS = Command Query Responsibility Segregation.

Tách write model và read model khi requirements thật sự khác:

```text
Command path
→ invariants / transaction

Query path
→ optimized projection
```

Không phải:

```text
CQRS = every app needs two databases + event sourcing
```

## 7. Event Sourcing

Persist state transitions/events as primary history model thay vì chỉ current row state.

Useful khi audit/history/rebuild/time-travel semantics justify.

Cost rất cao:

```text
schema/event evolution
replay
projection rebuild
privacy/deletion
operational tooling
mental model
```

Đừng dùng chỉ vì “event-driven”.

## 8. Architecture evolution

Typical progression:

```text
Simple Monolith
→ Structured Monolith
→ Modular Monolith
→ Async integration where needed
→ Split specific services when evidence justifies
```

Không cần:

```text
CRUD MVP
→ 30 microservices
```

Migration trigger examples:

- team ownership collision;
- independent scaling pressure;
- deployment cadence conflict;
- fault isolation requirement;
- regulatory/data boundary;
- runtime/technology specialization.

## 9. Architecture documentation

Mỗi important decision nên có:

### ADR

```text
Context
Decision
Options considered
Consequences
Evidence
Revisit trigger
```

### C4 / diagrams

Diagram trả lời audience-specific question:

```text
Context
Container
Component where useful
Deployment
```

Diagram không thay ADR/failure model.

### Runbook / threat / failure model

Architecture tồn tại trong production operations, không chỉ design time.

## 10. Fitness functions

Một architecture rule có thể encode/test tự động.

Ví dụ:

```text
Orders module cannot reference Payments.Infrastructure
```

hoặc:

```text
public API contracts cannot depend on internal persistence types
```

Fitness function biến “architecture convention” thành evidence thay vì wiki text.

## 11. Example — Checkout architecture evolution

### Stage 1 — Monolith

```text
Checkout App
├─ Orders
├─ Payment
├─ Inventory
└─ Notification
        ↓
       SQL
```

Good khi team nhỏ/workload vừa.

### Stage 2 — Modular Monolith

```text
Checkout Host
├─ Orders module owns order tables
├─ Payment module owns payment tables
├─ Inventory module owns reservation tables
└─ Notification module
```

Explicit module APIs/events; direct cross-table writes prohibited.

### Stage 3 — Split Payment if justified

Trigger:

```text
Payment has separate compliance/ownership/deploy lifecycle
+ independent failure/reconciliation requirements
```

Then:

```text
Orders
→ Payment contract
→ Payment service
→ provider
```

Now must handle:

```text
timeout = UNKNOWN
duplicate requests
contract versioning
observability
reconciliation
```

Microservice split bought isolation/ownership at cost of distributed correctness complexity.

## 12. Architecture review checklist

- [ ] business capabilities identified;
- [ ] critical quality attributes prioritized;
- [ ] source of truth/data ownership explicit;
- [ ] module/service boundaries have ownership reason;
- [ ] sync/async choices explain user/failure semantics;
- [ ] transaction boundaries explicit;
- [ ] duplicate/timeout/partial failure handled;
- [ ] architecture style justified, not fashion-driven;
- [ ] deployment/operations ownership defined;
- [ ] security/trust boundaries visible;
- [ ] performance/cost constraints measured;
- [ ] ADR documents important decisions;
- [ ] migration/revisit triggers written;
- [ ] architecture rules automated where useful.

## Nguồn tham khảo

User-provided `awesome-software-architecture` có breadth lớn: Clean/Onion/Hexagonal/Vertical Slice, EDA, SOA, DDD, CQRS, Microservices, Modular Monolith, design principles/patterns, cloud patterns, backpressure, eventual consistency, messaging, distributed transactions/locking, API/gRPC/caching/sharding/database và Azure. Module này tổ chức lại breadth đó theo **decision/trade-off/evolution**, không sao chép catalog.
