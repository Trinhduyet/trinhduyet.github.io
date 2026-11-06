# Architecture Modeling — Event Storming, C4, UML và yêu cầu nghiệp vụ

> [← Module 25](README.md) · [Design Principles](architecture-principles-patterns-and-12-factor.md) · [Tactical DDD →](tactical-ddd-and-aggregate-consistency.md) · [Business → HLD/LLD](solution-architecture-from-requirements-to-design.md)

**Mục tiêu:** tạo mô hình kiến trúc có thể dùng để quyết định, triển khai, review và vận hành. Biết **chọn đúng loại sơ đồ theo câu hỏi**, không vẽ đủ 10 hình vì checklist.

## 1. Từ business discovery đến bộ tài liệu thiết kế

| Câu hỏi cần trả lời | Artifact tốt nhất | Người review |
|---|---|---|
| Người dùng/đối tác làm gì, ranh giới sản phẩm? | Context diagram, use-case map, scope & assumptions | Product/Business, Security |
| Business event/state/exception nào tồn tại? | Event Storming, state machine, domain glossary | Domain expert, Tech Lead |
| Năng lực nào cần tách sở hữu? | Capability map, bounded-context map | EDA/Architect, các team |
| Hệ thống tương tác qua đâu? | C4 Context + Container; integration contract | Architect, Security, DevOps |
| Một operation chạy theo thứ tự nào? | Sequence / activity diagram | Backend/QA/DevOps |
| Transaction boundary/invariant ở đâu? | Aggregate model + ERD + state transitions | DBA, Tech Lead |
| Đáp ứng availability, security và cost thế nào? | Deployment view, NFR quality scenarios, ADR | SRE, Security, Finance |

**Không đồng nhất các biểu đồ:** C4 mô tả cấu trúc, sequence mô tả behavior theo thời gian, state machine mô tả transitions, ERD mô tả persistence. C4 `Component` không tự động tương đương microservice hay .NET class.

## 2. Event Storming — khai phá nghiệp vụ, không phải kiến trúc triển khai

### Thành phần của workshop

| Thành phần | Ví dụ trong ứng dụng đặt chỗ | Hỏi thêm |
|---|---|---|
| Domain Event (past tense) | `SeatHeld`, `PaymentTimedOut`, `BookingConfirmed` | Fact được Business công nhận? |
| Command | `HoldSeat`, `ConfirmPayment` | Ai được quyền kích hoạt? |
| Actor | Customer, Operator, Payment Provider | Human hay automated? |
| Policy/Process | Khi payment thành công, confirm hold | Nếu callback đến trễ thì sao? |
| Aggregate | Reservation, PaymentAttempt | Invariants nào phải atomic? |
| External System | PSP, ticket inventory | Ai sở hữu source of truth? |
| Hotspot/Open question | “Payment timeout nhưng provider đã charge?” | Ai quyết định trạng thái và SLA? |

### Workshop 90 phút gợi ý

1. **0–15 phút — Scope:** business goal, actors, success KPI, preconditions, boundaries.
2. **15–35 phút — Big Picture:** domain experts sắp xếp event theo thời gian; không tranh cãi chọn Kafka/SQL.
3. **35–55 phút — Exceptions:** duplicate command, cancellation, timeout, late callback, reconciliation, security refusal.
4. **55–70 phút — Boundaries:** phân nhóm event/policy/language thành candidate subdomains và bounded contexts.
5. **70–85 phút — Design Level:** chọn một hotspot, thêm command, aggregate, policy và transaction boundary.
6. **85–90 phút — Decisions:** ghi assumptions, unresolved questions, owners, deadline và next POC.

**Ví dụ business flow:**

```text
HoldRequested → SeatHeld → PaymentRequested
                            ├─ PaymentConfirmed → BookingConfirmed
                            ├─ PaymentFailed → HoldReleased
                            └─ PaymentUnknown → ReconciliationOpened
                                                    ├─ Confirmed → BookingConfirmed / RefundPolicy
                                                    └─ Failed → HoldReleased
```

Đường `UNKNOWN` quan trọng hơn lựa chọn broker: hold expiry và PSP callback có thể xảy ra đồng thời. Design phải quy định owner/invariant của tài nguyên và xử lý late confirmation.

**Output:** event timeline, business glossary, context candidates, hotspot register, open questions và acceptance scenarios. Đừng coi sticky notes là hợp đồng dữ liệu chính thức; cần chuyển thành API/event schema và ADR.

## 3. Domain Storytelling và use-case modeling

Domain Storytelling hữu ích để Business kể **actor nào làm hành động gì với đối tượng nào**. Ví dụ: “Khách tạo yêu cầu; hệ thống giữ chỗ; PSP xác nhận; hệ thống phát vé”. Nó giúp xác định sai biệt trách nhiệm trước khi định nghĩa endpoint.

Use-case diagram chỉ hiển thị mục tiêu/actor và ranh giới; không diễn đạt được timeout race, consistency hoặc SQL locks. Gắn use case với Given/When/Then và negative scenarios.

### Traceability example

| Requirement | Scenario | Architecture decision | Evidence |
|---|---|---|---|
| FR-04: một ghế không bị bán hai lần | 100 concurrent hold attempts | SQL atomic conditional update/unique slot lease | Integration concurrency test |
| NFR-02: trạng thái thanh toán có thể tạm không xác định | PSP commit rồi timeout | UNKNOWN + stable request ID + reconciliation | Fault-injection test |
| SEC-03: customer không đọc booking của người khác | Request guessed ID | Resource ownership authorization | Negative API test |

## 4. C4 Model — chọn level theo audience

| Level | Thành phần | Mục tiêu | Những thứ cần annotate |
|---|---|---|---|
| **L1 System Context** | Users, systems, external partners | Ranh giới và dependencies | Actors, trust boundaries, business flows |
| **L2 Container** | API, worker, DB, broker, UI | Deployable/runtime units và data ownership | Protocol, sync/async, security zone, HA assumptions |
| **L3 Component** | Application/domain/adapter modules trong một container | Trách nhiệm và dependencies | Module contract, error handling, transaction boundary |
| **L4 Code** | Classes/interfaces khi thật sự cần | Giải thích hotspot implementation | Invariants, extension points; thường source code tốt hơn vẽ tay |

**Ví dụ L1 → L2:**

```text
L1: Customer ↔ Booking System ↔ Payment Provider

L2: Mobile/Web → API → Booking DB
                       ↘ Payment Adapter → PSP
                        ↘ Outbox → Worker → Notification Provider
```

Với hệ thống quan trọng, tạo thêm **deployment view**: region/AZ, ingress/egress, DB primary/replica, secret boundary, RTO/RPO, runbook owner. C4 không tự thay thế network topology hay threat model.

## 5. UML, ERD, state diagram: khi nào chọn?

| Diagram | Dùng để quyết định | Sai lầm thường gặp |
|---|---|---|
| Sequence | Request, DB commit, provider call, webhook, retries, partial failure | Không chỉ ra điểm commit hoặc timeout UNKNOWN |
| State machine | Legal/illegal transitions, compensation, late event | Cho `FAILED → SUCCESS` tùy ý không có evidence |
| Class | Object relationships và contracts trong LLD | Dùng class diagram thay business domain model |
| Component | Module dependency và boundary | Vẽ mọi project/namespace như microservice |
| Deployment | Nodes, zones, networking, capacity, HA | Không thể hiện network trust/cost/blast radius |
| ERD | Relational keys, constraints, cardinality | Bỏ unique keys, transaction invariant, retention |
| Activity/BPMN | Human approval và workflow coordination | Dùng như thay thế state machine và concurrency design |

**Review một sequence:** đánh dấu `BEGIN/COMMIT`, `Idempotency-Key`, outbox insert, provider timeout, message ACK, replay và user-visible result. Nếu không có, diagram chưa đủ cho distributed system review.

## 6. Architecture Decision Record & documentation lifecycle

```text
ADR-007: Use SQL conditional claim for seat inventory

Context: no oversell under concurrent requests; moderate write throughput.
Options: SQL conditional update / Redis lease / single-writer queue.
Decision: SQL owns invariant; Redis is optional read cache.
Consequences: contention per popular show, DB capacity budget.
Evidence: 100 concurrent claims, exactly one winner per seat.
Revisit trigger: p99 hold latency > agreed SLO at peak load.
Owner/Status/Date: explicit.
```

Kiến trúc tài liệu sống trong Git: review cùng code, link đến PR/POC/evidence, cập nhật khi architecture deviation được phê duyệt. Không để HLD là ảnh cũ không gắn version thực tế.

## 7. Lab & review gate

1. Chọn **Payment, Twitter Feed hoặc Booking**; tổ chức Event Storming giả lập với ít nhất 5 edge cases.
2. Nộp C4 L1/L2, một sequence happy path, một sequence timeout, một state machine, một ERD.
3. Map 6 FR + 6 measurable NFR đến component, decision và test.
4. Security reviewer đánh dấu trust boundaries, sensitive data, authz và vendor contract.
5. Tech Lead phản biện source-of-truth, transaction boundaries và replay; EDA/DevOps phản biện capacity/deployment.
6. Ghi 2 ADR với options/cost/revisit triggers; tracking unresolved assumptions.

**Pass/fail:** diagram đẹp nhưng không xử lý timeout, concurrent operations, PII và operational ownership **không đạt**. Mỗi decision quan trọng phải trace về requirement và evidence.

**Tham khảo:** [C4 Model](https://c4model.com/) · [EventStorming](https://www.eventstorming.com/) · [Awesome — Modeling Resources](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/foundations/modeling) · [Architecture Review Playbook](architecture-review-playbook.md).
