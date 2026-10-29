# Danh mục kiến thức kỹ thuật — .NET Solution Architect

> [← Tổng quan kiến trúc](README.md) · [Lộ trình học](solution-architect-dotnet-roadmap.md) · [Bài thực hành bắt buộc](solution-architect-mandatory-labs.md)

Đây là **bản đồ chương trình học**, không phải 16 giáo trình đã hoàn thiện. Mỗi nhóm nêu kiến thức cần nắm, tình huống production, tài liệu nền và lab cần làm. Dùng [bảng kiểm độ bao phủ](solution-architect-completeness-review.md) để phân biệt nội dung đã được đề cập với nội dung đã giải thích sâu hoặc có bằng chứng chạy thật. Các lab ở đây là đề bài; không coi là đã thực thi nếu chưa có source và evidence. **P0** là kiến thức cốt lõi; **P1** là năng lực vận hành; **P2** dành cho yêu cầu chuyên sâu.

<div class="arch-metrics" markdown>
<div><strong>16</strong><span>Nhóm kiến thức</span></div><div><strong>12</strong><span>Lab cốt lõi</span></div><div><strong>4</strong><span>Case study nghiệp vụ</span></div><div><strong>7</strong><span>Câu hỏi kiểm chứng / chủ đề</span></div>
</div>

## Bảng tra cứu nhanh

| Nhóm | Ưu tiên | Bài thực hành |
|---|---|---|
| [01. Khoa học máy tính và Networking](#nhom-01) | P0 | [L01](solution-architect-mandatory-labs.md#l01) |
| [02. Nguyên lý thiết kế và Design Patterns](#nhom-02) | P0 | [L02](solution-architect-mandatory-labs.md#l02) |
| [03. Domain-Driven Design](#nhom-03) | P0 | [L03](solution-architect-mandatory-labs.md#l03) |
| [04. Thiết kế dữ liệu và SQL](#nhom-04) | P0 | [L04](solution-architect-mandatory-labs.md#l04) |
| [05. Concurrency và giao dịch](#nhom-05) | P0 | [L05](solution-architect-mandatory-labs.md#l05) |
| [06. Hệ thống phân tán](#nhom-06) | P0 | [L06](solution-architect-mandatory-labs.md#l06) |
| [07. Thiết kế API và tích hợp](#nhom-07) | P0 | [L07](solution-architect-mandatory-labs.md#l07) |
| [08. Messaging và Event Streaming](#nhom-08) | P0 | [L08](solution-architect-mandatory-labs.md#l08) |
| [09. Caching và hiệu năng](#nhom-09) | P0 | [L09](solution-architect-mandatory-labs.md#l09) |
| [10. Scalability và High Availability](#nhom-10) | P0 | [L10](solution-architect-mandatory-labs.md#l10) |
| [11. Security Architecture](#nhom-11) | P0 | [L11](solution-architect-mandatory-labs.md#l11) |
| [12. Cloud, Docker, Kubernetes và IaC](#nhom-12) | P1 | [L12](solution-architect-mandatory-labs.md#l12) |
| [13. Observability và SRE](#nhom-13) | P1 | [L13](solution-architect-mandatory-labs.md#l13) |
| [14. Backup, Disaster Recovery và dữ liệu](#nhom-14) | P1 | [L14](solution-architect-mandatory-labs.md#l14) |
| [15. Tài liệu, quyết định và governance](#nhom-15) | P0 | [L15](solution-architect-mandatory-labs.md#l15) |
| [16. Kiến trúc nâng cao](#nhom-16) | P2 | [L16](solution-architect-mandatory-labs.md#l16) |

## Học chuyên sâu theo từng nhóm

Các chương mới là **kiến thức kiến trúc tổng quát**, áp dụng cho nhiều ngành, được dùng bổ sung cho 16 nhóm thay vì biến OMS thành chương trình bắt buộc:

| Nhóm | Deep dive tích hợp | Kỹ năng đầu ra |
|---|---|---|
| 01–03 Foundations, design, DDD | [Business → HLD/LLD](solution-architecture-from-requirements-to-design.md), [Leadership](architecture-leadership-decisions-and-governance.md) | Capability map, boundaries, design review |
| 04–05 Database, concurrency | [Polyglot Data](data-architecture-polyglot-persistence.md), [Distributed Systems](distributed-systems-architecture-deep-dive.md) | Transaction, isolation, invariant, SQL/NoSQL trade-off |
| 06, 08 Distributed & messaging | [Distributed Systems, EDA](distributed-systems-architecture-deep-dive.md) | Outbox/Inbox, Saga, backpressure, ordering, failure recovery |
| 07, 11 API & security | [Integration/Security/Platform](integration-security-platform-architecture.md) | OAuth/OIDC, API Gateway, tenant/resource authz, partner contract |
| 09–10 Cache, scale, HA | [Data/Cache](data-architecture-polyglot-persistence.md), [Performance/Operations](performance-reliability-operational-architecture.md) | p99, capacity, load shedding, HA/DR |
| 12–14 Cloud/platform, observability, DR | [Integration/Hybrid](integration-security-platform-architecture.md), [Performance/Operations](performance-reliability-operational-architecture.md) | CI/CD, IaC, SLO, tracing, restore drill |
| 15–16 Governance & advanced | [Leadership/Governance](architecture-leadership-decisions-and-governance.md), [Practice/Assessment](solution-architect-case-practice-and-assessment.md) | ADR, POC, deviations, mentoring, capstone |

**Chú ý:** source lab [Distributed Workflow](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/25-solution-architect/distributed-workflow) chỉ là một implementation chung cho một phần L05/L06/L07/L08. Không suy ra 16 nhóm hoặc 12 lab đã được kiểm chứng đầy đủ.

<h3 id="nhom-01">01. Khoa học máy tính và Networking — P0</h3>

**Phải nắm:** Process/Thread/Task; scheduling, ThreadPool starvation, CPU-bound vs I/O-bound; TCP handshake/retransmission, DNS TTL/cache, TLS trust chain, HTTP/1.1/2/3, keep-alive, proxy/load balancer, NAT/SNAT, connection reuse, timeout/cancellation, p95/p99, queueing và Little’s Law.

**Bài học chi tiết:** [Computer Science & Networking cho .NET Architect](computer-science-networking-dotnet-architect.md).

**Tình huống thực tế:** API timeout do DNS/TLS hay SQL? Phân biệt ThreadPool starvation với cạn connection pool.

**Tài liệu nền:** [Networking](../02-linux-git-networking/README.md) · [.NET Runtime](../03-dotnet/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L01](solution-architect-mandatory-labs.md#l01) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-02">02. Nguyên lý thiết kế và Design Patterns — P0</h3>

**Phải nắm:** SOLID, DRY, KISS, YAGNI; Strategy, Factory, Adapter, Decorator; DI; Clean, Hexagonal, Vertical Slice.

**Tình huống thực tế:** Đổi nhà cung cấp thanh toán không sửa nghiệp vụ; test được adapter giả lập.

**Tài liệu nền:** [Clean/Hexagonal](clean-hexagonal-and-vertical-slice.md).

**Bằng chứng cần nộp:** [Bài thực hành L02](solution-architect-mandatory-labs.md#l02) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-03">03. Domain-Driven Design — P0</h3>

**Phải nắm:** Ubiquitous Language, Subdomain, Bounded Context, Entity, Value Object, Aggregate, Domain Event, ACL.

**Tình huống thực tế:** Tách Cash, OMS, Exchange Gateway theo ownership; không dùng chung bảng để sửa dữ liệu.

**Tài liệu nền:** [DDD](ddd-modular-monolith-and-microservices.md).

**Bằng chứng cần nộp:** [Bài thực hành L03](solution-architect-mandatory-labs.md#l03) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-04">04. Thiết kế dữ liệu và SQL — P0</h3>

**Phải nắm:** ACID, isolation, MVCC, index, query plan, deadlock, normalization, partition, replication, SQL/NoSQL.

**Tình huống thực tế:** Đo plan và lock wait; xác định query gây p99 tăng.

**Tài liệu nền:** [SQL](../05-sql/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L04](solution-architect-mandatory-labs.md#l04) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-05">05. Concurrency và giao dịch — P0</h3>

**Phải nắm:** Race condition, lost update, write skew; RowVersion, UPDLOCK, conditional update, unique constraint, fencing.

**Tình huống thực tế:** 100 request cùng mua chỉ một request được reserve khi tiền không đủ cho hai.

**Tài liệu nền:** [SQL Concurrency](../05-sql/transactions-isolation-and-concurrency.md).

**Bằng chứng cần nộp:** [Bài thực hành L05](solution-architect-mandatory-labs.md#l05) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-06">06. Hệ thống phân tán — P0</h3>

**Phải nắm:** CAP/PACELC, strong/eventual consistency, saga, idempotency, outbox/inbox, reconciliation.

**Tình huống thực tế:** Crash sau commit trước publish không mất event; timeout bên ngoài không coi là failed.

**Tài liệu nền:** [Distributed Systems](../17-distributed-systems/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L06](solution-architect-mandatory-labs.md#l06) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-07">07. Thiết kế API và tích hợp — P0</h3>

**Phải nắm:** REST, gRPC, WebSocket, SSE, Gateway/BFF, contract version, pagination, rate limit, webhook.

**Tình huống thực tế:** Cùng idempotency key khác payload phải conflict; version cũ vẫn tương thích.

**Tài liệu nền:** [API Design](../06-api-design/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L07](solution-architect-mandatory-labs.md#l07) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-08">08. Messaging và Event Streaming — P0</h3>

**Phải nắm:** Service Bus, RabbitMQ, Kafka, Event Hubs; partition, consumer group, ordering, retry, DLQ, replay, backpressure.

**Tình huống thực tế:** Message trùng, out-of-order, poison message đều có kết quả xác định.

**Tài liệu nền:** [Messaging](../17-distributed-systems/messaging-outbox-inbox-and-dedup.md).

**Bằng chứng cần nộp:** [Bài thực hành L08](solution-architect-mandatory-labs.md#l08) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-09">09. Caching và hiệu năng — P0</h3>

**Phải nắm:** Redis, cache-aside, TTL, invalidation, cache stampede, hot key, connection pooling, batching.

**Tình huống thực tế:** Cache stale không làm bán vượt ghế; đo hit rate và tải SQL khi cache miss đồng loạt.

**Tài liệu nền:** [Redis](../11-redis-caching/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L09](solution-architect-mandatory-labs.md#l09) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-10">10. Scalability và High Availability — P0</h3>

**Phải nắm:** Scale up/out, load balancing, stateless, circuit breaker, bulkhead, timeout budget, graceful shutdown.

**Tình huống thực tế:** Tắt một pod giữa request, dịch vụ vẫn phục hồi; không retry storm.

**Tài liệu nền:** [System Design](../24-system-design/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L10](solution-architect-mandatory-labs.md#l10) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-11">11. Security Architecture — P0</h3>

**Phải nắm:** OAuth2/OIDC, JWT, RBAC/ABAC, mTLS, secrets, threat modeling, audit, tenant isolation.

**Tình huống thực tế:** ClientAdmin không đọc được tài khoản tenant khác kể cả sửa request ID.

**Tài liệu nền:** [Security](../09-security-devsecops/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L11](solution-architect-mandatory-labs.md#l11) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-12">12. Cloud, Docker, Kubernetes và IaC — P1</h3>

**Phải nắm:** Container image, health probe, requests/limits, HPA, Azure, Terraform, CI/CD, rollback.

**Tình huống thực tế:** Deploy lỗi revision mới không thay bản đang Ready; pipeline báo fail rõ ràng.

**Tài liệu nền:** [Kubernetes](../15-kubernetes/README.md) · [DevOps](../13-devops-iac/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L12](solution-architect-mandatory-labs.md#l12) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-13">13. Observability và SRE — P1</h3>

**Phải nắm:** Structured logs, correlation ID, OpenTelemetry traces/metrics, RED/USE, SLI/SLO, error budget.

**Tình huống thực tế:** Theo trace được từ HTTP → SQL → Service Bus → consumer và cảnh báo backlog.

**Tài liệu nền:** [Performance](../10-performance/README.md).

**Bằng chứng cần nộp:** [Bài thực hành L13](solution-architect-mandatory-labs.md#l13) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-14">14. Backup, Disaster Recovery và dữ liệu — P1</h3>

**Phải nắm:** RTO, RPO, backup, PITR, restore, multi-zone, failover, DR drill, retention.

**Tình huống thực tế:** Khôi phục bản backup thật và đo RTO/RPO; đối soát in-flight transactions.

**Tài liệu nền:** [HA/DR](../24-system-design/availability-multiregion-dr-security-and-cost.md).

**Bằng chứng cần nộp:** [Bài thực hành L14](solution-architect-mandatory-labs.md#l14) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-15">15. Tài liệu, quyết định và governance — P0</h3>

**Phải nắm:** Business capability, NFR, C4, HLD/LLD, ADR, threat model, capacity estimate, cost, architecture review.

**Tình huống thực tế:** Nộp C4 + sequence failure + ADR + số đo thay vì chỉ sơ đồ đẹp.

**Tài liệu nền:** [Architecture Review](architecture-review-playbook.md).

**Bằng chứng cần nộp:** [Bài thực hành L15](solution-architect-mandatory-labs.md#l15) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


<h3 id="nhom-16">16. Kiến trúc nâng cao — P2</h3>

**Phải nắm:** CQRS, Event Sourcing, multi-region, multi-tenancy, workflow engine, service mesh, fitness functions.

**Tình huống thực tế:** Chứng minh tại sao cần hoặc không cần Event Sourcing bằng trade-off cụ thể.

**Tài liệu nền:** [CQRS](event-driven-cqrs-and-integration.md).

**Bằng chứng cần nộp:** [Bài thực hành L16](solution-architect-mandatory-labs.md#l16) (hoặc phần đối chiếu tương ứng trong lab tích hợp). Không đánh dấu hoàn thành nếu chỉ đọc lý thuyết.


## Bảy câu hỏi bắt buộc cho mỗi kỹ thuật

1. **Là gì?** Giải thích bằng tiếng Việt, giữ thuật ngữ tiếng Anh để tra cứu.
2. **Tại sao cần?** Nêu lỗi xảy ra nếu không dùng.
3. **Khi nào dùng?** Chỉ rõ quy mô, invariant, yêu cầu vận hành.
4. **Khi nào không dùng?** Ví dụ không dùng Redis lock để thay invariant trong SQL.
5. **Triển khai thế nào?** Viết SQL/C# hoặc mô tả cấu hình thật.
6. **Trade-off?** Đo latency, contention, cost, consistency, complexity.
7. **Kiểm chứng?** Có integration test, load test hoặc failure drill và kết quả thực tế.

## Tiêu chí hoàn thành

- [ ] Giải thích được ít nhất một failure mode cho từng nhóm P0.
- [ ] Có bằng chứng test/trace cho các lab cốt lõi.
- [ ] Trình bày được trade-off của hai phương án kiến trúc khác nhau.
- [ ] Có C4, ADR, runbook và báo cáo NFR cho case cuối.

**Đọc tiếp:** [Danh sách lab và tiêu chí pass/fail](solution-architect-mandatory-labs.md).
