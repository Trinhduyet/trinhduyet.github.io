# System Design Case Study Template — .NET Solution Architect

> Mục tiêu: biến đề bài mơ hồ thành yêu cầu có thể kiểm chứng, API/data contract rõ ràng, kiến trúc có trade-off, và kế hoạch triển khai/test có evidence. Đây là template dùng lại cho các case trong chương System Design.

## 1. Problem statement & scope

**Bài toán:** [mô tả trong 2–4 câu]

**Actors:** [end user, admin, external provider, operator]

**In scope**
- ...

**Out of scope**
- ...

**Assumptions / open questions**
- Mọi con số chưa được đề bài xác nhận phải ghi là giả định.
- Phân biệt yêu cầu bắt buộc, mong muốn và câu hỏi cần hỏi Product/Business.

## 2. Functional requirements (FR)

Mỗi yêu cầu phải có ID, actor, hành vi và tiêu chí nghiệm thu.

| ID | Requirement | Acceptance criteria | Priority |
|---|---|---|---|
| FR-01 | Người dùng có thể ... | Given ... When ... Then ... | Must |
| FR-02 | Hệ thống xử lý ... | ... | Should |

Bao gồm cả luồng lỗi: trùng request, dữ liệu không hợp lệ, người dùng không có quyền, provider timeout, retry, hủy thao tác và xử lý trạng thái chưa xác định.

## 3. Non-functional requirements (NFR)

Không dùng các từ “fast”, “scalable”, “high availability” mà không định lượng hoặc nêu cách đo.

| ID | Attribute | Target / constraint | How to verify |
|---|---|---|---|
| NFR-01 | Latency | API read p95 < [x] ms tại [load] | k6/NBomber + tracing |
| NFR-02 | Availability | SLO [x]% trong [window] | SLI dashboard / failure test |
| NFR-03 | Durability | Không mất [loại dữ liệu] sau [failure] | restart/recovery test |
| NFR-04 | Consistency | [strong/eventual] và độ trễ tối đa [x] | integration test |
| NFR-05 | Security | [authn/authz/data protection] | negative tests / threat review |
| NFR-06 | Cost | ngân sách hoặc đơn vị chi phí mục tiêu | estimate + actual usage |

Nêu rõ workload assumptions: DAU/MAU, requests/user, peak factor, read/write ratio, payload size, retention, growth, concurrency, geography và burst pattern.

## 4. Capacity estimation

Ghi công thức và giả định, không chỉ ghi kết quả.

- Average RPS = requests per day / 86,400.
- Peak RPS = average RPS × peak factor.
- Storage = records/day × average record size × retention × replication/index overhead.
- Network = requests/second × average payload size (tính riêng ingress/egress).
- Ước lượng queue backlog và thời gian drain khi consumer xử lý chậm.

Tách read/write workload và hot-key/hot-partition risks. Không chọn shard count hoặc số server chỉ dựa vào DAU.

## 5. API design

Mỗi endpoint cần có method/path, auth, request, response, status codes, validation, pagination, rate limit và idempotency behavior.

| Method & path | Purpose | Request / response | Important behavior |
|---|---|---|---|
| POST /api/v1/resources | Create | JSON schema | Idempotency-Key nếu retry có thể tạo trùng |
| GET /api/v1/resources/{id} | Read | Resource DTO | Authorization theo resource |
| GET /api/v1/resources | List | cursor/page token | Sort ổn định, giới hạn page size |

Quy ước:
- Dùng DTO, không expose trực tiếp database entity.
- Xác định semantics của 400/401/403/404/409/422/429/5xx.
- Với asynchronous workflow, trả operation/resource ID và endpoint truy vấn trạng thái khi phù hợp.
- Webhook cần signature verification, deduplication, replay protection và retry contract.
- Ghi rõ versioning, correlation ID, timeout và cancellation.

## 6. Entities & data model

| Entity | Purpose | Key | Invariants / constraints |
|---|---|---|---|
| ... | ... | ... | ... |

Với từng entity, xác định:
- Primary key, unique constraints, foreign-key strategy nếu được phép, indexes và access patterns.
- Ownership và tenant boundary.
- Trạng thái và state transitions hợp lệ.
- Retention, PII/secrets, audit requirements.
- Transaction boundary: những thay đổi nào phải commit cùng nhau?
- Những read model/cache nào là derived data và cách rebuild chúng.

Đừng mặc định mọi entity cần CRUD. Model theo invariant và hành vi nghiệp vụ.

## 7. High-level design (HLD)

Vẽ diagram thể hiện **request path**, **data ownership**, **async/event path**, **trust boundary** và các dependency ngoài hệ thống. Ghi protocol và hướng dữ liệu trên các mũi tên.

Mỗi component cần trả lời:
- Trách nhiệm duy nhất là gì?
- Ai sở hữu dữ liệu?
- Giao tiếp sync hay async, vì sao?
- Nếu component chậm hoặc unavailable thì hành vi nào xảy ra?
- Có thể bỏ component này ở phiên bản đầu không?

Bắt đầu với kiến trúc nhỏ nhất đáp ứng FR/NFR. Chỉ thêm broker, cache, search engine, microservice hoặc database riêng khi có lý do và trade-off cụ thể.

## 8. Critical flows & sequence diagrams

Ít nhất mô tả:
1. Happy path.
2. Validation/authorization failure.
3. Timeout và retry.
4. Duplicate request/message.
5. Partial failure giữa database và external system.
6. Recovery/reconciliation sau khi hệ thống hoạt động trở lại.

Với mọi flow ghi dữ liệu, đánh dấu điểm commit, nơi tạo idempotency key, nơi phát event và trạng thái mà client nhìn thấy.

## 9. Deep dives — chọn theo rủi ro của bài toán

- **Concurrency:** optimistic/pessimistic locking, unique constraint, atomic update, race conditions.
- **Consistency:** source of truth, stale reads, read-your-writes, eventual consistency và convergence.
- **Messaging:** at-least-once, duplicate handling, ordering scope, outbox/inbox, poison messages, replay.
- **Caching:** cache-aside, TTL, invalidation, stampede, stale data và cache miss storm.
- **Scaling:** stateless compute, partition key, hot partition, backpressure, queue lag.
- **Resilience:** timeout, bounded retry with jitter, circuit breaker, bulkhead, graceful degradation.
- **Security:** authentication, resource-level authorization, secrets, encryption, abuse controls, audit.
- **Operations:** logs/metrics/traces, SLI/SLO, alerts, backup/restore, RTO/RPO, runbook.

Chỉ đào sâu các mục liên quan đến yêu cầu và failure mode thực tế; giải thích vì sao các mục khác chưa cần.

## 10. Failure mode & recovery table

| Failure | User/system impact | Detection | Mitigation | Recovery / evidence |
|---|---|---|---|---|
| DB unavailable | ... | ... | bounded timeout, controlled degradation | restart/failover test |
| Provider timeout | ... | ... | timeout, retry policy, unknown state handling | reconciliation test |
| Duplicate event | ... | ... | idempotent consumer / unique key | replay test |
| Queue backlog | ... | ... | backpressure, scaling, admission control | drain-time measurement |

Không tuyên bố exactly-once end-to-end chỉ vì broker có tính năng exactly-once. Mô tả semantics thực sự của toàn flow và cách consumer xử lý duplicate.

## 11. Security, privacy & compliance

- Threat model: tài sản, trust boundaries, attacker capabilities, abuse cases.
- Authentication vs authorization; tenant/resource-level access.
- Encryption in transit/at rest; secrets/token rotation.
- PII minimization, retention, deletion, auditability.
- Rate limiting, replay protection, input validation, SSRF/injection risks khi phù hợp.
- Với financial/trading: audit trail, reconciliation và không để cache/eventual read trở thành nguồn quyết định số dư/lệnh.

## 12. Architecture decisions (ADR)

Mỗi quyết định quan trọng ghi:

- **Context:** constraint hoặc vấn đề cần giải quyết.
- **Decision:** lựa chọn cụ thể.
- **Alternatives:** ít nhất một phương án khác.
- **Consequences:** latency, consistency, complexity, cost, operations và migration.
- **Revisit trigger:** điều kiện nào khiến cần xem lại quyết định.

Ví dụ: SQL Server transaction làm source of truth, broker dùng để phân phối thay đổi qua outbox; chỉ thêm read model khi đo được nhu cầu đọc riêng biệt.

## 13. Implementation plan — .NET & SQL Server

Chia theo lát cắt chạy được, không bắt đầu bằng cách tạo hàng loạt microservices:

1. Domain invariants, state machine và API contract.
2. SQL schema, indexes, constraints, transaction và repository/query path.
3. ASP.NET Core endpoints, authentication/authorization, validation.
4. Background processing / outbox / integration adapter nếu có nhu cầu.
5. Telemetry, configuration, secrets, health checks và graceful shutdown.
6. Container/deployment, migration strategy, rollback và runbook.

Chỉ chọn công nghệ thực sự cần. Ví dụ stack có thể là .NET 8+, ASP.NET Core, EF Core hoặc SQL thuần, SQL Server, OpenTelemetry và Docker; Redis/broker/cloud service là lựa chọn theo yêu cầu, không phải checklist bắt buộc.

## 14. Verification & evidence

| Level | Required evidence |
|---|---|
| Unit | Domain invariants, validation, state transitions |
| Integration | API + real database behavior, unique constraints, transaction rollback |
| Contract | Provider/webhook/event schema and compatibility |
| Concurrency | Concurrent requests, duplicate idempotency key, race conditions |
| Resilience | Timeout, retry bounds, dependency unavailable, poison event |
| Load | Throughput, p50/p95/p99, error rate, CPU/memory, DB waits/pool |
| Recovery | Restart, replay, reconciliation, backup/restore where applicable |

Mỗi kết luận cần được gắn nhãn:
- **Verified:** đã chạy và lưu kết quả.
- **Simulated:** mô phỏng có ghi rõ giới hạn.
- **Reasoned:** lập luận thiết kế, chưa được kiểm chứng.
- **Unknown:** còn thiếu dữ liệu hoặc câu trả lời.

Không ghi “production-proven”, “exactly once”, “zero data loss” hoặc “supports 10M users” nếu không có evidence phù hợp.

## 15. Trade-offs & final recommendation

Kết thúc bằng:
- Kiến trúc đề xuất và sơ đồ cuối.
- Lý do chọn, alternatives đã loại và trade-offs.
- Bottleneck dự kiến đầu tiên.
- Giới hạn đã biết và assumptions cần xác nhận.
- Kế hoạch rollout, rollback và các bước mở rộng theo ngưỡng đo được.

## Definition of Done

- [ ] FR có acceptance criteria.
- [ ] NFR có target và cách đo.
- [ ] Capacity estimates có công thức/assumptions.
- [ ] API và entities có contract/invariants.
- [ ] Diagram thể hiện data flow và trust boundaries.
- [ ] Failure/recovery và security được phân tích.
- [ ] ADR ghi trade-offs.
- [ ] Có test plan và evidence, không tuyên bố quá mức.
