# Performance, Scalability, HA/DR, Observability & Operational Readiness

> [← Module 25](README.md) · [Distributed Systems](distributed-systems-architecture-deep-dive.md) · [Integration/Security/Platform](integration-security-platform-architecture.md)

Solution Architect cần chứng minh thiết kế hoạt động khi **tải cao, dependency lỗi, deploy thay đổi, region/DB failover và khôi phục dữ liệu**. Không thể chứng minh bằng sơ đồ nhiều pod.

## 1. Định lượng SLI, SLO và error budget

| User journey | SLI khả thi | SLO ví dụ (giả định, phải được business chấp thuận) |
|---|---|---|
| Đọc dữ liệu | % successful requests và p95/p99 latency | 99.9% success/tháng; p95 ≤ 250 ms tại documented workload |
| Xử lý command | % accepted được chuyển terminal state trong deadline | 99.95% trong 5 phút, loại trừ rejected hợp lệ |
| Async processing | Oldest queue age / event completion lag | p99 lag ≤ 30s tại peak profile |
| Data recovery | Actual lost writes và elapsed restore | RPO ≤ 5m, RTO ≤ 30m trong scenario defined |

**Lưu ý:** các con số chỉ để minh họa định dạng SLO, không phải chuẩn áp dụng mọi hệ thống. Availability SLO phải xác định numerator/denominator, window, planned maintenance, dependency boundaries, criticality.

Error budget của SLO 99.9% trong tháng 30 ngày xấp xỉ 43.2 phút unavailable theo phép tính đơn giản, nhưng SLI request-based không đồng nghĩa số phút downtime và cần diễn giải nhất quán.

## 2. Capacity: throughput, concurrency và saturation

1. Xác định workload: active users, operations/user, daily distribution, peak factor, payload sizes, read/write mix, hot tenants/keys.
2. Tính arrival rate λ, service time W và concurrency L; dùng `L ≈ λW` khi assumptions ổn định.
3. Vẽ dependency budget: edge/network → application → DB/broker/cache → provider. p99 tổng không bằng cộng tùy tiện p99 từng thành phần độc lập.
4. Tìm bottleneck: CPU, GC, ThreadPool, sockets, DB pool/locks/IO, broker partitions, provider quotas, disk/network.
5. Chọn queue/circuit/backpressure/admission policy; đo performance khi retry/dependency chậm.
6. Làm sensitivity analysis 2×/5×/10× workload và chi phí mỗi business operation.

### Load testing phải có profile

| Loại | Kiểm tra | Evidence |
|---|---|---|
| Baseline | Normal load, correctness, latency | p50/p95/p99, CPU/DB plans/traces |
| Stress | Saturation point, load shedding | Throughput knee, error shape, recovery |
| Soak | Leaks, GC, connection pool, disk/log growth | 1–24h trends theo khả năng lab |
| Spike | Burst mở phiên/sale campaign | Queue age, autoscale delay, throttling |
| Failure-under-load | DB/broker/provider down và phục hồi | Retry amplification, data invariants, recovery time |

Mỗi benchmark lưu commit SHA, hardware/cluster, dataset, concurrency, ramp-up, duration, tool/config, random seed khi cần, raw results và limitations. Không chỉ chụp một con số RPS.

## 3. Resilience patterns: chọn theo failure mode

| Pattern | Giúp gì | Trade-off / anti-pattern |
|---|---|---|
| Timeout/deadline | Ngăn request treo vô hạn | Timeout có thể để lại UNKNOWN state; phải reconcile |
| Retry with jitter | Hấp thụ transient fault | Non-idempotent effect/DB contention có thể tệ hơn |
| Circuit breaker | Giảm áp lực lên dependency hỏng | Misconfiguration làm reject sớm hoặc chặn phục hồi |
| Bulkhead | Giới hạn blast radius | Resource partition có thể lãng phí khi load skew |
| Rate limit/load shed | Bảo vệ source of truth | Phải ưu tiên critical journey và trả error rõ |
| Cache/degraded mode | Duy trì read khi dependency chậm | Stale data không được quyết định financial/booking invariants |
| Outbox/replay | Durable async processing | Duplicate, replay safety và backlog operations |

### Timeout budget

Nếu end-to-end deadline 800 ms, phải chia budget cho auth/DB/provider/network, giới hạn attempts và đo p99. Retry 3 lần mỗi lần 800 ms **không** nằm trong deadline 800 ms. Cancellation cần truyền xuống dependencies khi khả thi, nhưng không giả định hủy network call là rollback external effect.

## 4. High Availability: failure domains và consistency

| Layer | HA strategy | What can still fail |
|---|---|---|
| Stateless API | Multiple replicas, health-based LB, rolling update | Shared DB exhaustion, correlated bad release |
| Database | Replication/failover/cluster, backup | Data loss per async replication, failover split, ambiguous commits |
| Messaging | Durable queues/replicas, consumer groups | Poison message, duplicate, partition skew, producer ACK ambiguity |
| Region/site | Multi-zone/region, DNS/routing, dependency redundancy | Network partition, replication conflict, legal residency, cost |

**Failure-domain mapping**: app node, availability zone, database leader, broker, provider, network link, region, identity provider và operators. Mỗi domain cần degrade/failover behavior và owner. Active-active cross-region writing là quyết định consistency phức tạp, không phải cờ bật HA.

## 5. DR, backup và business recovery

- **RTO:** thời gian tối đa chấp nhận để phục hồi chức năng trong scenario xác định.
- **RPO:** mất mát dữ liệu theo thời điểm chấp nhận được; phải xét acknowledged writes, replica lag và external side effects.
- **Backup:** full/differential/log/PITR theo engine, retention, encryption, immutable/offsite copy khi policy yêu cầu.
- **Restore rehearsal:** reset environment, restore, run migrations/validation, reconcile inbox/outbox/ledger/provider, measure actual elapsed time and lost data.
- **Failback:** cần design riêng; sau failover không tự động an toàn khi quay về primary cũ.

| Scenario | Drill | Success evidence |
|---|---|---|
| DB instance down | Kill/failover primary under write load | Client behavior, retries, data invariant, recovery duration |
| Region unavailable | Shift supported traffic to secondary | DNS/routing, secrets, dependencies, RTO/RPO |
| Accidental delete | Restore from PITR to clean environment | Record count/checksum, business reconciliation |
| Broker backlog after outage | Restart consumers with duplicate/replay | No duplicate logical effect, drain time |

Không sao chép production personal data vào lab nếu chưa được bảo vệ theo chính sách.

## 6. Observability: từ metrics đến quyết định vận hành

**RED** cho service: Rate, Errors, Duration. **USE** cho resources: Utilization, Saturation, Errors. Bổ sung **business metrics** như pending orders, aged payments, reconciliation mismatch, stale projection, queue age.

| Signal | Câu hỏi chẩn đoán |
|---|---|
| Logs | Điều gì xảy ra cho business ID/correlation ID? Secrets/PII có bị lộ? |
| Traces | Request chậm ở service, database, network, external provider nào? |
| Metrics | Lỗi tăng do tải, retries, saturation, GC hay backlog? |
| Audit trail | Ai thay đổi trạng thái nhạy cảm, với quyền/approval gì? |
| Synthetic probe | Journey thực tế còn hoạt động hay chỉ container health xanh? |

Thiết lập alerts theo **user-visible symptom + actionable condition**, không chỉ CPU 80%. Recording incident timeline cần deploy version, affected journey, blast radius, rollback/recovery, customer communications và follow-up ADR.

## 7. Operational readiness và SRE

Review trước go-live:

1. On-call owner, support hours, escalation matrix và partner contacts.
2. SLO/alerts/dashboards, log retention, privacy, trace sampling và cost.
3. Runbook: deploy, rollback, pause/resume consumers, replay safely, reconcile UNKNOWN states.
4. Backups/PITR/restore measured, secrets/cert rotation, capacity headroom.
5. Database migrations old/new compatibility, feature flags, release canary.
6. Chaos/game-day scenarios, post-incident review và debt owners.

### Runbook skeleton

```text
Symptom: queue oldest age > [threshold]
Impact: [user journeys]
Check: [dashboard, broker partition, consumer error, DB latency]
Contain: [rate-limit noncritical producers / stop unsafe retries]
Recover: [fix poison / resume / replay with stable IDs]
Verify: [drain age, business state, reconciliation mismatch]
Escalation: [owner, partner, business]
Evidence: [timeline, traces, metrics, incident ID]
```

## 8. Cost modeling và trade-offs

TCO gồm compute, managed DB/IO/storage, broker, cache, network egress, log/tracing retention, backup/DR standby, support/license, engineering operations. So sánh **cost per order/tenant/month** tại workload tương đương và ghi 2× growth sensitivity.

Ví dụ giải pháp A (managed database + simpler monolith) có infra bill cao hơn B nhưng tiết kiệm on-call/DBA đáng kể. Không chọn chi phí VM đơn lẻ khi team size và compliance khác nhau.

## 9. End-to-end exercise

Một service có 20k requests/s peak, p99 mục tiêu 500 ms, database phụ thuộc và một payment provider thỉnh thoảng timeout. Hãy thiết kế:

- Request/dependency budget, queue/backpressure và policy degrade.
- Topology 2 zone, DB failover/DR/backup, RTO/RPO theo từng capability.
- SLI/SLO/business KPI, dashboards, alerts, runbooks và on-call.
- Load/stress/soak/failure tests; evidence in raw result + trace.
- ADR giữa scale-up DB, cache/read model, async queue hoặc partitioning.

**Pass:** report giải thích bottleneck đo được, invariants vẫn đúng sau lỗi, recovery trong target hoặc minh bạch không đạt; không pass nếu chỉ nâng replica và đo average latency.

## Nguồn chính thống

- [Google SRE Workbook](https://sre.google/workbook/table-of-contents/)
- [OpenTelemetry documentation](https://opentelemetry.io/docs/)
- [Microsoft .NET diagnostics](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/)
