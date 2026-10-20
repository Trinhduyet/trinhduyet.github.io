# Kiểm tra độ bao phủ kiến thức — .NET Solution Architect

> [← Tổng quan](README.md) · [Danh mục 16 nhóm](solution-architect-technical-catalog.md) · [Lộ trình](solution-architect-dotnet-roadmap.md) · [Lab bắt buộc](solution-architect-mandatory-labs.md)

## Mục đích và giới hạn đánh giá

Trang này kiểm tra **độ bao phủ của chương trình học**, không khẳng định mọi nội dung đã được dạy sâu hay mọi lab đã chạy. Trạng thái phản ánh việc đối chiếu tài liệu trong repository; không thay cho build, test, security review hoặc production evidence.

- **Có nền tảng:** đã có tài liệu tham chiếu, cần kiểm tra độ sâu theo mục tiêu cụ thể.
- **Cần đào sâu:** có tên chủ đề nhưng thiếu giải thích, trade-off hoặc failure analysis xuyên suốt.
- **Cần lab/evidence:** phải có source, lệnh chạy, test/metrics và kết quả lưu được.
- **Theo yêu cầu hệ thống:** chỉ cần triển khai khi business/NFR thực sự yêu cầu; không phải kiến trúc mặc định.

## Ma trận độ bao phủ

| Lĩnh vực | Kiến thức tối thiểu phải giải thích | Bằng chứng thực hành cần có | Tình trạng cần theo dõi |
|---|---|---|---|
| Computer Science & OS | Process/thread/task, scheduling, memory, GC, allocation, blocking | Đo CPU/GC/ThreadPool và phân biệt CPU-bound với I/O-bound | Cần đào sâu production |
| Networking | DNS, TCP, TLS, HTTP/1.1–3, proxy, NAT/SNAT, connection reuse | Trace request, mô phỏng timeout và đo tail latency | Lab L01 cần hoàn thiện |
| .NET Runtime | async/await, ThreadPool, GC/LOH, HttpClient, cancellation, hosted services | Counters/trace và kiểm tra starvation/resource leaks | Cần lab chuyên biệt |
| Design principles | SOLID, cohesion/coupling, patterns, dependency direction | Refactor có test, so sánh trước/sau | Lab L02 |
| Domain modeling | Capability map, subdomain, bounded context, aggregate/invariant, ACL | Domain model + quyết định ownership và contract | Lab L03 |
| SQL Server | Data modeling, indexes, execution plans, isolation, locks, Query Store, log/backup | Integration test, plan, deadlock graph, benchmark | Lab L04 cần mở rộng |
| Concurrency | Lost update, write skew, optimistic/pessimistic locking, unique constraints, idempotency | Test đồng thời trên SQL Server thật | Lab L05 |
| Distributed consistency | Partial failure, timeout/unknown, retries, saga, reconciliation | Kill/restart, duplicate, delayed response và recovery | Lab L06 |
| API design | Versioning, contract compatibility, pagination, rate limit, auth, webhook | Contract/integration tests | Lab L07 |
| Messaging | Delivery semantics, ordering, partition/session, retry, DLQ, replay, backpressure | Broker thật hoặc tương thích; chứng minh duplicate và recovery | Lab L08 |
| Caching/performance | Cache-aside, invalidation, stampede, hot key, cache failure | Benchmark DB QPS, hit rate, stale-data behavior | Lab L09 |
| Availability/resilience | Timeout budget, bulkhead, circuit breaker, graceful shutdown, load shedding | Fault injection và tải có giới hạn | Lab L10 |
| Security architecture | Threat modeling, identity, authorization, tenant isolation, secrets, audit | Negative security tests, dependency/secret scan | Lab L11 |
| Cloud/platform/IaC | Deployment topology, network/identity, config/secrets, rollout/rollback, cost | Deploy/fail/rollback test và pipeline evidence | Lab L12 |
| Observability/SRE | SLI/SLO, error budget, RED/USE, trace/log/metric, alerting | Trace end-to-end, dashboard, alert test | Lab L13 mở rộng |
| Backup/DR | Backup strategy, PITR, RTO/RPO, restore, failover, reconciliation | Restore drill và số đo thực tế | Lab L14 mở rộng |
| Architecture governance | Capability map, NFR, C4, ADR, risk register, fitness functions | Một bộ hồ sơ kiến trúc có thể review và cập nhật | Lab L15 mở rộng |
| Advanced architecture | CQRS/Event Sourcing, multi-region, workflow, service mesh | ADR chứng minh lợi ích, chi phí và trigger áp dụng | Lab L16 mở rộng |
| Data governance | Classification, retention, lineage, privacy, ownership, deletion | Data inventory + retention/deletion scenario | Cần nội dung rõ hơn |
| Capacity/cost | Arrival rate, peak factor, queue drain, storage growth, egress, cost drivers | Workbook giả định + load test đối chiếu | Cần bài mẫu định lượng |
| Migration/evolution | Expand-contract, backward compatibility, strangler, rollback, dual-read/write risks | Migration rehearsal trên dữ liệu đại diện | Cần runbook riêng |
| Architecture risk | Risk register, blast radius, dependency criticality, reversibility | Risk workshop + ADR có revisit trigger | Cần chuẩn hóa |
| Operational readiness | On-call ownership, runbook, SLO, escalation, maintenance, support model | Game day và checklist go-live | Cần checklist riêng |

## Các điểm không được đánh dấu “đã hoàn thành” nếu chưa có evidence

1. Lab chỉ có đề bài hoặc snippet không đồng nghĩa đã chạy.
2. Unit test dùng mock không chứng minh transaction/concurrency của SQL Server.
3. Broker giả lập không chứng minh được mọi semantics của Azure Service Bus, Event Hubs hoặc Kafka.
4. Sơ đồ kiến trúc không chứng minh capacity, security, availability hay recovery.
5. Một lần restore thành công không chứng minh đạt RTO/RPO cho mọi tình huống.
6. “Exactly once” phải nêu rõ phạm vi; side effect ở external provider vẫn cần idempotency/reconciliation.
7. Số đo benchmark phải ghi môi trường, dữ liệu, concurrency, warm-up, duration và giới hạn; không ngoại suy tùy tiện thành production capacity.

## Chuẩn tối thiểu để gọi một chủ đề là “học xong”

- [ ] Giải thích nguyên lý và giới hạn bằng lời của mình.
- [ ] So sánh ít nhất hai phương án và nói rõ khi nào không chọn mỗi phương án.
- [ ] Nêu failure mode, dấu hiệu quan sát và quy trình phục hồi.
- [ ] Có bài thực hành phù hợp: code/test hoặc architecture artifact.
- [ ] Có lệnh chạy, assumptions và evidence thật; ghi rõ phần nào mô phỏng.
- [ ] Liên kết được quyết định kỹ thuật với business invariant, NFR, security và cost.

## Thứ tự hoàn thiện đề xuất

1. Computer Science/Networking + .NET Runtime.
2. SQL Server concurrency + idempotency.
3. Outbox/Inbox + messaging + recovery.
4. Security/tenant isolation.
5. Capacity, SLO, resilience và deployment.
6. Data governance, migration và architecture risk.
7. Case study Securities OMS tổng hợp.

Không mở thêm nhóm kiến thức để tăng số lượng trước khi các P0 có đủ chiều sâu và evidence.
