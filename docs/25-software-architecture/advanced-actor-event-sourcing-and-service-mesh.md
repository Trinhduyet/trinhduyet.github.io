# Advanced Architecture — Actor Model, Event Sourcing, Service Mesh & Micro-frontends

> [← Module 25](README.md) · [Distributed Systems Deep Dive](distributed-systems-architecture-deep-dive.md) · [Messaging Platforms](../17-distributed-systems/messaging-platforms-asyncapi-and-workflows.md) · [Architecture Decisions](architecture-decisions-evolution-and-fitness-functions.md)

**Priority P1/P2:** các kỹ thuật này hữu ích cho những workload/boundary nhất định. Không là stack mặc định của Solution Architect. Muốn chọn phải chứng minh được profile tải, consistency, operational model, cost và engineering capabilities.

## 1. Actor Model là gì?

Actor là một đơn vị giữ state và xử lý messages theo semantics của actor framework. Actor có thể tạo/gửi messages và tự quản lifecycle. Trong .NET, **Microsoft Orleans** cung cấp *virtual actors* gọi là grains; Akka.NET có mô hình khác. **Actor Model không đồng nghĩa RabbitMQ/Kafka consumer hay distributed lock.**

### Khi có lợi

- Nhiều entity độc lập với state/lifecycle riêng: IoT device, game session, realtime collaboration, shopping cart ở mức activity.
- Một entity thường nhận nhiều commands cần serialization ở mức grain/actor.
- Framework hỗ trợ placement, activation/lifecycle, persistence/reminders/streams theo cấu hình; developer tránh tự xây scheduling/distribution tùy tiện.

### Khi không phù hợp

- CRUD API với DB workload vừa phải, SQL transaction là đủ.
- Invariant liên quan **nhiều actors** hoặc external PSP: actor isolation không tự tạo distributed ACID.
- Yêu cầu deterministic ordering qua nhiều partitions/regions nếu framework không cung cấp semantics tương ứng.

**Ví dụ:** một `AuctionGrain(auctionId)` nhận bid; cần validate bid/closing time. Nhưng phải thiết kế persistence, leader/failover, duplicate client command và race khi close. Grain method serialization **không thay thế** unique/version/checkpoint khi activation/retry/crash.

## 2. Actor vs database transaction vs queue worker

| Mechanism | Mạnh ở đâu | Không bảo đảm |
|---|---|---|
| SQL conditional update | Atomic invariant trên một DB boundary | Remote PSP/network result |
| Single-writer queue per key | Work ordering/backpressure theo key | Exactly-once side effects, durable DB logic |
| Actor/Orleans grain | State ownership/lifecycle per entity | Atomic cross-grain operations hoặc cross-region global ordering |
| Distributed lock/lease | Temporary resource coordination | Không có stale writer nếu thiếu fencing token |

**ADR:** dùng Actor chỉ khi per-entity stateful concurrency/placement mang lại lợi ích đo được so với stateless API + SQL + queue. Revisit theo hot-key distribution, activation churn, grain persistence latency và recovery SLO.

## 3. Event Sourcing ≠ Event-Driven Architecture

**Event-Driven Architecture (EDA):** hệ thống publish facts để những consumer khác xử lý. **Event Sourcing:** event log là **source of truth** để dựng lại state một aggregate; read models được project từ event stream.

```text
Command: OpenWallet
  → validate against current aggregate version
  → append WalletOpened (expectedVersion = 0)

Command: CreditWallet
  → load/rebuild events or snapshot + tail
  → validate invariant
  → append WalletCredited (expectedVersion = N)
  → projector consumes events → balance read model
```

### Những quyết định bắt buộc

| Area | Câu hỏi |
|---|---|
| Event schema | Event immutable nhưng version/upcasting thế nào? |
| Concurrency | Optimistic expected version khi append; duplicate command xử lý ra sao? |
| Projection | Ordering per aggregate, checkpoint, idempotent reprocess, rebuild duration? |
| Snapshot | Snapshot frequency, versioning, invalidate/rebuild policy? |
| Sensitive data | PII deletion/redaction/crypto-shredding khi event log immutable? |
| External effects | Replay event **không được** tự phát lại charge/email thực tế |
| Audit | Business event log có đủ yêu cầu audit/legal và integrity không? |
| Cost | Storage growth, schema migration, tooling, debugging và operator expertise |

**Ví dụ sai:** dựa vào Kafka topic retention 7 ngày rồi gọi đó là “event sourcing forever”. Nếu event log là source-of-truth, durability, ordering, append semantics và retention phải đáp ứng khả năng rebuild theo hợp đồng.

**Khi nên chọn:** business thật sự cần temporal reconstruction/audit lịch sử thay đổi và team sẵn sàng vận hành projection/replay/versioning. Với simple payments, **relational ledger + outbox** thường đơn giản hơn; ledger entries immutable không tự khiến toàn bộ hệ thống thành Event Sourcing.

## 4. Service Mesh: data plane vs control plane

Service mesh thường đưa traffic policy/telemetry/mTLS xuống lớp proxy hoặc dataplane do platform quản lý. Các thành phần và khả năng tùy sản phẩm/vận hành (chẳng hạn Istio, Linkerd).

| Concern | Mesh giúp gì? | Ứng dụng vẫn chịu trách nhiệm |
|---|---|---|
| mTLS workload-to-workload | Identity/transport encryption theo mesh policy | Business-level authz, ownership, token/claim semantics |
| Traffic routing | Canary, retries, timeouts theo route | Idempotency, transaction consistency, unknown outcomes |
| Telemetry | Network/service request visibility | DB spans, business metrics, redaction, correlation |
| Policy | Một số network access policies | Application validation, PII protection, audit |
| Failure isolation | Circuit-breaking/load balancing (khả năng tùy mesh) | Backpressure/end-to-end deadline/queue ownership |

**Double retry storm:** nếu SDK retry 3 lần, Gateway 3 lần và mesh 3 lần, request có thể khuếch đại đến 27 calls. Không cấu hình retries ở tất cả các tầng mà không có tổng deadline/side-effect semantics.

### Costs and risks

- Control plane/data plane failure semantics, upgrade và certificate rotation.
- Proxy CPU/memory, tail latency, config drift và incident-debugging complexity.
- Sidecar/ambient topology, network policies, multi-cluster/multi-zone consistency; workload profile quyết định.
- Khi network nhỏ, managed ingress + direct service telemetry có thể đủ.

## 5. Micro-frontends & Backend for Frontend (BFF)

Micro-frontends tách UI theo team/domain để independent release; BFF gom API phù hợp với từng client (web/mobile). Chúng **không** tự động tăng performance hoặc security.

| Decision | Khi đáng dùng | Cost |
|---|---|---|
| Monolithic frontend | Ít team, UI shared design/system | Independent deploy kém hơn khi team lớn |
| Micro-frontends | Nhiều team ownership boundaries thật, release cycles độc lập | Duplicate dependencies, routing, UX consistency, auth/session risk |
| BFF | Mobile/web có aggregation/latency/auth needs khác nhau | Thêm deployable và API governance; tránh business duplication |

**Security:** BFF server-side token handling, CSRF/session policies, API authz; không đặt trust boundary dựa trên “đã có frontend route guard”.

## 6. Failure scenarios & operational review

| Failure | Detection | Test / mitigation |
|---|---|---|
| Actor crashes after external side effect | Duplicate provider reference | Stable business ID + recovery workflow; kill grain/worker |
| Event projection crash halfway | Lag + checkpoint mismatch | Idempotent replay + projector version, rebuild drill |
| Event schema changes | Old consumers reject new payload | Compatibility contract/upcaster tests |
| Mesh certificate/route invalid | Cross-service 5xx, mTLS errors | Staged rollout, rollback + workload identity diagnostics |
| Mesh and SDK both retry | Provider QPS spike, tail latency | End-to-end budget, disable duplicate retry layers |
| Microfrontend version skew | UI inconsistency, auth breaks | Consumer contract + canary + shared UX governance |

## 7. Lab & ADRs

1. Implement one actor-like stateful service (e.g. auction counter) bằng Orleans **hoặc** single-key queue + SQL. So sánh p95, recovery, correctness, ops.
2. Event-sourced sample với optimistic append, snapshot, projector, upcasting và 100-event replay. Tắt projector, sửa schema, rebuild read model.
3. Prove replay không gửi lại payment/email side effects; inbound consumers dedup.
4. Mesh POC: có/không mesh; đo latency, telemetry và behavior khi một service bị timeout.
5. ADR: chọn/không chọn Actor, Event Sourcing, Service Mesh và BFF dựa NFR, cost, team capacity.

**Definition of Done:** có bảng alternative + thực nghiệm + runbook. Không coi một local hello-world hoặc mock làm bằng chứng multi-region/production.

**Nguồn chính thống:** [Microsoft Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/overview) · [Istio Documentation](https://istio.io/latest/docs/) · [Linkerd Documentation](https://linkerd.io/2/overview/) · [Awesome — Actor Model](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/software-architecture/architecture-patterns/actor-model) · [Awesome — Event Sourcing](https://github.com/mehdihadeli/awesome-software-architecture/blob/main/docs/software-architecture/architecture-patterns/event-sourcing.md).
