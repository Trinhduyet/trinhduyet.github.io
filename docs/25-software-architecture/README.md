# Software Architecture — Lộ trình .NET Solution Architect

> [← System Design](../24-system-design/README.md) · [Master Roadmap](../00-roadmap/master-roadmap.md)

<div class="arch-landing" markdown>

<div class="arch-eyebrow">CHƯƠNG 25 · LỘ TRÌNH TỪ SENIOR BACKEND ĐẾN SOLUTION ARCHITECT</div>

## Học thiết kế hệ thống có thể kiểm chứng

Từ **C# / SQL Server** đến **Distributed Systems, HLD, LLD, Cloud và vận hành production**. Học bằng tiếng Việt, có ví dụ **Financial, Loyalty, Booking và Securities OMS**; không học thuộc tên pattern.

<div class="arch-metrics" markdown>
<div><strong>16</strong><span>Nhóm kiến thức</span></div>
<div><strong>12</strong><span>Lab nền tảng bắt buộc</span></div>
<div><strong>4</strong><span>Case study nghiệp vụ</span></div>
<div><strong>3</strong><span>Cấp độ LLD · HLD · Architect</span></div>
</div>

<div class="arch-actions" markdown>
<a class="arch-action arch-action--primary" href="solution-architect-technical-catalog/">Xem danh mục kiến thức →</a>
<a class="arch-action" href="solution-architect-mandatory-labs/">Bài thực hành bắt buộc →</a>
</div>

</div>

## Bắt đầu từ đâu?

<div class="arch-card-grid">
<a class="arch-path-card" href="solution-architect-dotnet-roadmap/"><span class="arch-card-index">01 · LỘ TRÌNH</span><strong>Bản đồ System Design</strong><span>LLD → HLD → Solution Architecture; thứ tự học và tiêu chuẩn đầu ra.</span><em>Đọc lộ trình →</em></a>
<a class="arch-path-card" href="solution-architect-technical-catalog/"><span class="arch-card-index">02 · KIẾN THỨC</span><strong>Danh mục 16 nhóm kỹ thuật</strong><span>Từ networking, SQL, DDD, concurrency đến observability và DR.</span><em>Tra cứu kỹ thuật →</em></a>
<a class="arch-path-card" href="solution-architect-mandatory-labs/"><span class="arch-card-index">03 · THỰC HÀNH</span><strong>12 lab bắt buộc + 4 mở rộng</strong><span>Đề bài, stack, pass/fail và bằng chứng cần nộp.</span><em>Xem bài lab →</em></a>
<a class="arch-path-card" href="solution-architect-dotnet-case-studies/"><span class="arch-card-index">04 · NGHIỆP VỤ</span><strong>Financial · Loyalty · Booking · OMS</strong><span>Transaction, idempotency, outbox/inbox, recovery và reconciliation.</span><em>Xem case study →</em></a>
</div>

## Ba cấp độ năng lực

<div class="arch-stage-grid">
<div><b>LLD — Thiết kế chi tiết</b><p>Code C#, SQL, API, isolation, locks, unique constraints, state machine và integration tests.</p></div>
<div><b>HLD — Thiết kế hệ thống</b><p>Bounded contexts, data ownership, messaging, caching, scaling, availability và failure flows.</p></div>
<div><b>Solution Architect — Quyết định kiến trúc</b><p>Business/NFR, C4, ADR, capacity, security, cost, SLO, RTO/RPO và governance.</p></div>
</div>

## Kiểm tra độ bao phủ và học sâu từ nền tảng

- [Kiểm tra độ bao phủ kiến thức](solution-architect-completeness-review.md) — phân biệt nội dung đã có nền tảng với nội dung cần đào sâu và evidence cần có.
- [Computer Science & Networking cho .NET Solution Architect](computer-science-networking-dotnet-architect.md) — process/thread/Task, ThreadPool, TCP/DNS/TLS/HTTP, latency, connection pooling, lỗi production và lab L01.
- [System Design Case Study Template](system-design-case-study-template.md) — template chuẩn cho FR/NFR, capacity, APIs, entities, HLD, failure modes, security, ADR và verification evidence.

## Nội dung Software Architecture chuyên sâu đang có

Không viết trùng kiến thức. Sau khi nắm bản đồ và làm lab, đọc các chương hiện hữu:

| Chủ đề | Nội dung |
|---|---|
| [Quality Attributes, Boundaries & Styles](quality-attributes-boundaries-and-styles.md) | Chọn kiến trúc theo NFR |
| [DDD, Modular Monolith & Microservices](ddd-modular-monolith-and-microservices.md) | Ranh giới nghiệp vụ và sở hữu dữ liệu |
| [Clean, Hexagonal & Vertical Slice](clean-hexagonal-and-vertical-slice.md) | Cấu trúc code và khả năng kiểm thử |
| [Event-Driven, CQRS & Integration](event-driven-cqrs-and-integration.md) | Hợp đồng tích hợp, event, transaction phân tán |
| [ADR, Fitness Functions & Evolution](architecture-decisions-evolution-and-fitness-functions.md) | Quyết định, kiểm chứng, tiến hóa kiến trúc |
| [Architecture Review Playbook](architecture-review-playbook.md) | Review kiến trúc theo business, NFR và failure |
| [Nguồn tham khảo](references.md) | Tài liệu nền và nguồn chính thống |

---

## Kiến thức nền: tại sao không chọn kiến trúc theo trào lưu?

**Software Architecture** là cách phân chia trách nhiệm và đưa ra quyết định dưới các ràng buộc cụ thể. Microservices, CQRS hay Event Sourcing không tự động tốt hơn Modular Monolith. Hãy xác định **bất biến nghiệp vụ, yêu cầu chất lượng, ranh giới dữ liệu và bằng chứng** trước khi chọn công nghệ.

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
