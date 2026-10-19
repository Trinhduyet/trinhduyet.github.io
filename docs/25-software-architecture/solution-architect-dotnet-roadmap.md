# .NET Solution Architect — System Design Competency Map

> [Software Architecture](README.md) · [System Design](../24-system-design/README.md) · [Distributed Systems](../17-distributed-systems/README.md)

<div class="lesson-meta">
  <span><strong>Priority</strong> P0 → P2</span>
  <span><strong>Audience</strong> Senior .NET Backend → Tech Lead → Solution Architect</span>
  <span><strong>Method</strong> decision-first · failure-first · evidence-driven</span>
</div>

## 1. Tổng quan: LLD → HLD → Solution Architecture

```text
Business goals / domain / constraints
                |
       NFR / quality attributes
                |
    HLD: context, boundaries, data ownership
                |
    LLD: transaction, API, schema, state machine
                |
    Production: security, scaling, HA/DR, telemetry
                |
    Evidence: tests, load, failure drills, ADR
```

**LLD** quan tâm cách thay đổi trạng thái đúng trong một module: C# concurrency, EF Core, SQL isolation, constraint, retry và tests.

**HLD** quan tâm service boundary, dữ liệu có thẩm quyền, integration contracts, request flows, bottlenecks, failure isolation và deployment.

**Solution Architecture** đặt hai lớp trên trong business/NFR, chi phí, timeline, vận hành, review governance và lộ trình tiến hóa. Không đồng nhất architect với người áp dụng nhiều patterns nhất.

## 2. Ma trận 16 nhóm năng lực

| # | Knowledge area | Priority | Evidence / bài thực hành |
|---|---|---|---|
| 01 | CS, OS, Networking, DNS, TCP/TLS, HTTP, latency vs throughput, Little's Law | P0 | Truy vết một request end-to-end |
| 02 | SOLID, design patterns, DI, Clean/Hexagonal, Vertical Slice | P0 | Refactor feature .NET có test |
| 03 | DDD: bounded context, aggregate, domain event, ownership | P0 | Mapping Trading/Cash/Margin |
| 04 | SQL/NoSQL: schema, indexes, plans, ACID, MVCC, replication | P0 | Query plan và transaction lab |
| 05 | Concurrency: race, lost update, write skew, atomic update, version, locks | P0 | 100 concurrent cash reservations |
| 06 | Distributed: CAP/PACELC, saga, outbox/inbox, dedup, consistency, reconciliation | P0 | Failure injection across services |
| 07 | REST/gRPC/GraphQL/WebSocket, API Gateway, contracts, versioning | P0 | Trading API + integration contract |
| 08 | Service Bus/Kafka/Event Hubs, ordering, partitions, DLQ, backpressure | P0 | Replay and duplicate consumer tests |
| 09 | Redis, cache-aside, TTL, invalidation, stampede, hot keys | P0 | Stale-cache and hot-key test |
| 10 | Horizontal scaling, HA, bulkhead, circuit breaker, load shedding | P0 | Load test + resilience scenario |
| 11 | OAuth2/OIDC, RBAC/ABAC, threat model, TLS/mTLS, audit, secrets | P0 | Tenant/ClientAdmin isolation test |
| 12 | Docker, Kubernetes, Azure, Terraform, CI/CD and rollback | P1 | Deployment + rollback proof |
| 13 | OpenTelemetry, logs, traces, RED/USE, SLI/SLO, alerts | P1 | Trace a failed order |
| 14 | RTO/RPO, backup restore, DR drill, failover, archival | P1 | Restore and recovery drill |
| 15 | C4, HLD/LLD, NFR, ADR, capacity and cost, review/deviation | P0 | Architecture review pack |
| 16 | Event sourcing, advanced CQRS, multi-region, workflow engine, service mesh | P2 | ADR proving why needed |

P0 không có nghĩa mọi pattern phải có mặt trong production. Một quyết định *không sử dụng* pattern với evidence rõ ràng cũng là kết quả kiến trúc tốt.

## 3. Roadmap học theo dependency, không theo buzzword

```text
.NET runtime + networking + SQL
        ↓
Transaction / race conditions / idempotency
        ↓
DDD ownership + REST contracts
        ↓
Messaging / outbox / inbox / Saga / recovery
        ↓
Caching / scaling / security / HA / observability
        ↓
HLD / LLD / NFR / ADR / governance
        ↓
Financial / Loyalty / Booking / Securities case studies
```

Học theo nhóm trước khi học công cụ. Ví dụ Redis Redlock không thay DB transaction; Kafka exactly-once semantics không đồng nghĩa business effect end-to-end exactly-once.

## 4. Cross-reference theo chuyên đề đang có

| Cần học | Đọc trước |
|---|---|
| .NET async, cancellation, ThreadPool | [C# / .NET Runtime](../03-dotnet/README.md) |
| Isolation, locking, SQL plan | [SQL](../05-sql/README.md) |
| API contracts, idempotency | [API Design](../06-api-design/README.md) |
| Redis lease và invalidation | [Redis & Caching](../11-redis-caching/README.md) |
| Distributed failure, Outbox/Inbox | [Distributed Systems](../17-distributed-systems/README.md) |
| Scaling, estimates and case studies | [System Design](../24-system-design/README.md) |
| DDD, CQRS, styles and ADR | [Software Architecture](README.md) |
| Reliability and failure under load | [Performance](../10-performance/README.md) |
| CI/CD and cloud | [DevOps](../13-devops-iac/README.md) · [Cloud](../14-cloud/README.md) |
| Kubernetes production | [Kubernetes](../15-kubernetes/README.md) |

## 5. Checklist review một distributed transaction

- [ ] Business invariant viết thành điều kiện kiểm thử (ví dụ available balance không âm).
- [ ] Source of truth và owner của balance/points/seat xác định.
- [ ] Transaction boundary giải thích được và không ôm external HTTP call.
- [ ] Atomic conditional update / locking / unique constraints bảo vệ concurrent writes.
- [ ] Client retry có idempotency key, request hash và response/status replay.
- [ ] Outbox record được commit cùng business record.
- [ ] Consumer sử dụng inbox / uniqueness để không lặp business effect.
- [ ] Broker redelivery, out-of-order, timeout, DLQ có policy.
- [ ] Unknown external outcome có trạng thái riêng; không suy diễn timeout = failed.
- [ ] Recovery worker + reconciliation có source of truth, metrics và audit.
- [ ] Integration tests dùng SQL thật, kịch bản race và injected failures.
- [ ] ADR mô tả alternatives, cost, latency, operations và revisit trigger.

## 6. Đầu ra của người học

Một case study đạt yêu cầu phải có: business flow; NFR + giả định số tải; C4 Context/Container; sequence success/failure; data owner/schema; transaction/idempotency; outage/DR; security trust boundaries; load/fault test; SLI/SLO; ADR và runbook.

Tiếp theo: [Financial, Loyalty, Booking & Securities Cases](solution-architect-dotnet-case-studies.md).
