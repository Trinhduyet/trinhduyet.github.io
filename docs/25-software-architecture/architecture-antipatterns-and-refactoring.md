# Architecture Anti-patterns, Code Smells & Safe Refactoring

> [← Module 25](README.md) · [Design Principles](architecture-principles-patterns-and-12-factor.md) · [Technical Leadership](architecture-leadership-decisions-and-governance.md) · [Architecture Decisions](architecture-decisions-evolution-and-fitness-functions.md)

**Mục tiêu:** nhận diện design debt bằng evidence, chọn remediation có tỷ lệ lợi ích/rủi ro tốt và refactor khi hệ thống vẫn phục vụ người dùng. Một kỹ thuật không trở thành “anti-pattern” chỉ vì không hợp sở thích kiến trúc sư.

## 1. Code smell, anti-pattern và technical debt khác nhau

- **Code smell:** tín hiệu cục bộ (`long method`, `primitive obsession`, `feature envy`), cần đọc context trước khi sửa.
- **Architecture anti-pattern:** cấu trúc gây coupling, availability, change-rate hoặc security failures có tính hệ thống.
- **Technical debt:** trade-off đã biết hoặc vô tình tạo ra, có hậu quả và chi phí lãi theo thời gian. Một món nợ kỹ thuật có thể chấp nhận **nếu chủ động ghi nhận và có revisit trigger**.

Không tự động dùng LOC, số microservices hoặc số class làm quality score. Đánh giá bằng change amplification, defect rate, cycle time, deployment coupling, on-call toil và độ khó kiểm thử.

## 2. Catalog anti-patterns có triệu chứng và cách xử lý

| Anti-pattern | Dấu hiệu thực tế | Hậu quả | Remediation có kiểm chứng |
|---|---|---|---|
| **Big Ball of Mud** | Business rules xuyên tầng; không rõ module owner | Thay một rule chạm 8 project; regression liên tục | Map dependencies, extract bounded modules từng lát cắt |
| **God Object / Service** | `OrderService` 4.000 dòng, IO và domain lẫn | Dễ violate SRP, unit test rất đắt | Tách domain policy, workflow, adapter theo change reasons |
| **Anemic Domain Model** | Entity chỉ get/set; service procedural chứa rules | Invariants không có nơi bảo vệ | Đưa behavior vào aggregate khi domain thực sự phức tạp |
| **Leaky Abstraction** | Business layer phụ thuộc `DbContext`, provider error codes | Vendor detail lan ra nhiều nơi | Stable ports/adapters + provider contract tests |
| **Shared Database Microservices** | Các service sửa bảng của nhau | Deploy/transaction coupling, ownership mờ | Define owners, published APIs/events, strangler migration |
| **Distributed Monolith** | 10 services phải deploy một lượt, sync call chain dài | Tăng latency/failure surface mà không được autonomy | Analyze boundaries; merge, decouple hoặc isolate deploy |
| **Chatty Remote Calls** | N+1 HTTP requests, synchronous fan-out | Tail latency và exhaustion tăng | Batch, read model, async where appropriate |
| **Cache as Source of Truth** | Redis TTL quyết định số dư, inventory | Eviction/stale gây vi phạm invariant | Durable transactional DB; cache derived state/rebuild |
| **Retry Storm** | Mỗi tầng tự retry ×3 khi provider chậm | Traffic amplification, outage lan rộng | End-to-end deadline, bounded retry + jitter, backpressure |
| **Premature Microservices** | Small team quản 15 services, nhiều cross-DB calls | Ops/cost overhead, inconsistent data | Modular monolith trước khi có scaling/team boundary thật |
| **Premature Generalization** | 20 plugin interfaces cho một use case | Review/maintain khó hơn nghiệp vụ | Delete speculative layers, YAGNI, extract khi có variation thật |
| **Vendor/Framework Lock-in** | Domain API chứa SDK types everywhere | Migration/hybrid cloud khó | Encapsulate high-change integration, ADR đánh đổi |

**Lưu ý:** một CRUD-centric application không nhất thiết cần rich domain model; shared database trong transitional period không phải lỗi nghiêm trọng nếu ownership/cutover được kiểm soát.

## 3. Ví dụ: từ OrderService mọi thứ đến module có trách nhiệm

### Trước refactor

```text
CheckoutController
   → OrderService
        ├─ validate customer + inventory + promotion
        ├─ write 5 SQL tables
        ├─ charge payment provider synchronously
        ├─ update search index
        └─ send email
```

**Failure thực tế:** DB commit `OrderCreated`, PSP charge thành công nhưng HTTP timeout; retry endpoint tạo charge thứ hai và gửi email trùng. Tách class đơn thuần không xử lý được vấn đề.

### Sau refactor có transaction boundary rõ

```text
Checkout API
  → Order Application Service
      → Aggregate guards local invariant
      → DB transaction: Order + Idempotency + Outbox
  → Async workflow: Payment Attempt → PSP Adapter
       ├─ confirmed success → mark paid + next step
       └─ UNKNOWN → status reconciliation
  → Notification consumer (dedup)
```

**Mấu chốt:** refactor vừa giảm coupling vừa **thay semantics**; phải thống nhất Product về status `PENDING/UNKNOWN`, SLA và refund policy trước rollout.

## 4. Safe refactoring và migration từng bước

| Step | Việc thực hiện | Evidence |
|---|---|---|
| 1. Baseline | Vẽ dependency graph, query/rate trace, incident history | Request latency, error budget, defect data |
| 2. Characterization tests | Đóng băng behavior hiện tại, kể cả edge cases | Contract/golden tests với payload thực tế đã ẩn PII |
| 3. Identify seam | Chọn boundary có high-change/high-risk | API/adapter contract, owner, risk |
| 4. Extract policy | Tách pure domain logic khỏi IO | Unit tests; dependency rules pass |
| 5. Introduce strangler/ACL | Route một phần traffic sang implementation mới | Feature flag, canary metrics, rollback procedure |
| 6. Data migration | Expand-contract, backfill, validation, read switch | Reconciliation counts, data correctness, dual-write risks |
| 7. Remove old path | Delete dead code sau confidence window | No old consumers, link/rollback decision |

**Dual write caution:** ghi old DB và new DB trong hai transactions riêng không atomic; phải có rõ owner, outbox/CDC, reconciliation và conflict policy. “Zero downtime” cần test cho schema compatibility khi old/new deployments coexist.

## 5. Architecture fitness functions

Mục tiêu là **guardrail có thể tự động kiểm tra**, không phải cấm hoàn toàn kỹ thuật cụ thể.

| Fitness function | Check |
|---|---|
| Domain independence | Domain assemblies không reference ASP.NET, persistence/provider SDK |
| API contract compatibility | Consumer-driven/contract tests khi đổi schema |
| Data ownership | Truy vấn ghi chỉ từ owner module/service hoặc migration được cho phép |
| Performance | p95/p99, DB waits và allocations trong budget tại workload đại diện |
| Reliability | Replay duplicate, kill worker, timeout UNKNOWN và verify recovery |
| Security | Resource-level authorization, secret scan, SBOM/vulnerability policy |
| Architecture observability | Trace correlation across API, SQL, broker, providers |

**Phạm vi cảnh báo:** architecture test có thể bắt dependency rule nhưng không chứng minh runtime invariant; cần SQL integration/concurrency và load/fault evidence.

## 6. Risk register & ADR mẫu

```text
Risk: Provider retry causes duplicate charges.
Likelihood: Medium; Impact: Critical.
Current evidence: one incident, no durable idempotency record.
Decision: use stable business key + PSP idempotency + UNKNOWN reconciliation.
Owner: Payments Tech Lead; Due date: release gate.
Verification: 100 concurrent duplicate requests + PSP timeout simulator.
Revisit: new PSP with different idempotency retention contract.
```

Prioritize theo expected harm, business criticality, blast radius và migration cost; không hứa “clean architecture” là giải pháp duy nhất.

## 7. Bài tập thực hành

1. Chọn một service có 3+ external dependencies; vẽ current dependency + failure chain.
2. Dùng test characterization trước khi tách Adapter/Strategy/Domain Policy.
3. Ghi ít nhất 4 anti-pattern findings, phân loại `must fix / tolerate / monitor` cùng business impact.
4. Đo before/after: coupling modules, test time, query latency và số lần network gọi.
5. Thực hiện canary + rollback drill, mô phỏng provider timeout và duplicate event.
6. Viết ADR tại sao **không** tách microservices ở quy mô/team hiện tại.

**Exit criteria:** mỗi remediation gắn với failure, requirement và evidence đo được; không chấm điểm theo số lượng pattern đã “áp dụng”.

**Tham khảo:** [Awesome — Anti-patterns](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/software-architecture/anti-patterns) · [Microsoft — Strangler Fig Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/strangler-fig) · [Architecture Decisions & Fitness Functions](architecture-decisions-evolution-and-fitness-functions.md).
