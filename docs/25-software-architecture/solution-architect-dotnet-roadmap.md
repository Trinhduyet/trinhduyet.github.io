# Lộ trình .NET Solution Architect — Bản đồ System Design từ cơ bản đến Production

> [← Software Architecture](README.md) · [System Design](../24-system-design/README.md) · [Distributed Systems](../17-distributed-systems/README.md)

**Đối tượng:** Senior .NET Backend, Tech Lead đang phát triển thành Solution Architect. **Ngôn ngữ:** Giải thích tiếng Việt; giữ thuật ngữ tiếng Anh trong ngoặc để tra cứu tài liệu chính thống.

## 1. Sau khi học xong, bạn làm được gì?

Bạn không chỉ kể tên DDD, CQRS hay Microservices. Bạn phải có khả năng:

1. Chuyển yêu cầu nghiệp vụ thành các **bất biến** (invariant) và **yêu cầu phi chức năng** (NFR).
2. Chọn ranh giới module/service, chủ sở hữu dữ liệu và transaction boundary.
3. Thiết kế chống race condition, double-spend, duplicate event, timeout và partial failure.
4. Tính toán tải, đo p95/p99, xác định điểm nghẽn và chi phí.
5. Trình bày HLD, LLD, sequence diagram, ADR; chứng minh thiết kế bằng test và số liệu.

### Bản đồ kiến trúc

```text
Mục tiêu kinh doanh (Business Goal)
             |
             v
Nghiệp vụ + Bất biến + NFR
             |
             v
HLD: Bounded Context, ranh giới hệ thống, luồng dữ liệu
             |
             v
LLD: API, state machine, SQL, transaction, concurrency
             |
             v
Vận hành: Security, Scaling, HA/DR, Observability
             |
             v
Kiểm chứng: Integration Test, Load Test, Failure Injection, ADR
```

**LLD (thiết kế chi tiết)** trả lời: một request thay đổi dữ liệu như thế nào để luôn đúng? **HLD (thiết kế tổng thể)** trả lời: các thành phần phối hợp ra sao, ai sở hữu dữ liệu và hệ thống phục hồi thế nào? **Solution Architecture** trả lời: lựa chọn nào đáp ứng mục tiêu kinh doanh, NFR, chi phí và năng lực đội ngũ?

## 2. Danh mục 16 nhóm kiến thức cần nắm

**P0:** bắt buộc nắm chắc; **P1:** phải triển khai/vận hành được; **P2:** chuyên sâu khi có nhu cầu.

| # | Nhóm kiến thức | Cần hiểu và áp dụng | Ưu tiên |
|---|---|---|---|
| 01 | Khoa học máy tính và mạng | TCP, DNS, TLS, HTTP, thread, async/await, latency, throughput | P0 |
| 02 | Thiết kế phần mềm | SOLID, DI, Strategy, Factory, Clean/Hexagonal, Vertical Slice | P0 |
| 03 | Thiết kế theo miền nghiệp vụ (DDD) | Bounded Context, Aggregate, Entity, Value Object, Domain Event | P0 |
| 04 | Cơ sở dữ liệu | Index, query plan, ACID, isolation, replication, partitioning | P0 |
| 05 | Xử lý đồng thời và giao dịch | Lost Update, RowVersion, lock, atomic update, unique constraint | P0 |
| 06 | Hệ thống phân tán | Saga, Outbox/Inbox, idempotency, consistency, reconciliation | P0 |
| 07 | API và tích hợp | REST, gRPC, WebSocket, API Gateway, versioning, rate limiting | P0 |
| 08 | Message và event streaming | Service Bus, Kafka, Event Hubs, ordering, retry, DLQ, backpressure | P0 |
| 09 | Cache và tối ưu hiệu năng | Redis, cache-aside, TTL, invalidation, hot key, stampede | P0 |
| 10 | Mở rộng và sẵn sàng cao | Load balancing, horizontal scaling, timeout, circuit breaker, HA | P0 |
| 11 | Kiến trúc bảo mật | OAuth2, OIDC, RBAC/ABAC, TLS, audit, secret, threat modeling | P0 |
| 12 | Cloud và triển khai | Docker, Kubernetes, Azure, Terraform, CI/CD, rollback | P1 |
| 13 | Giám sát và độ tin cậy | OpenTelemetry, trace, metrics, SLI/SLO, alert, runbook | P1 |
| 14 | Khôi phục thảm họa | RTO, RPO, backup/restore, failover, DR drill | P1 |
| 15 | Tài liệu và quản trị kiến trúc | C4, HLD/LLD, ADR, review, capacity, trade-off, technical debt | P0 |
| 16 | Kiến trúc nâng cao | Event Sourcing, multi-region, workflow engine, service mesh | P2 |

### Hiểu sâu một kỹ thuật là như thế nào?

Ví dụ **Optimistic Concurrency**: không chỉ biết EF Core có `RowVersion`. Bạn cần giải thích được tại sao hai request đọc cùng số dư có thể tạo **Lost Update**; `UPDATE ... WHERE RowVersion = @original` phát hiện xung đột thế nào; khi nào phải đọc lại dữ liệu; khi nào atomic conditional update phù hợp hơn; và integration test nào chứng minh không chi vượt tiền.

## 3. Thứ tự học khuyến nghị

### Giai đoạn 1 — Nền tảng .NET và SQL (4–6 tuần tham khảo)

Học C# async/await, ThreadPool, cancellation, HTTP/TCP, SQL indexes, isolation và EF Core.

**Ví dụ:** API đặt lệnh gọi SQL Server chậm; phân biệt thread starvation với connection pool exhaustion. Đo p95 và query duration, không đoán bằng cảm giác.

**Đầu ra:** một API có tracing, SQL query plan và bài test concurrent update.

### Giai đoạn 2 — Transaction và Distributed Systems (6–8 tuần)

Học atomic update, optimistic/pessimistic locking, idempotency, outbox/inbox, Saga, consistency, timeout, retry, reconciliation.

**Ví dụ:** 100 request cùng trừ 80 triệu từ tài khoản chỉ có 100 triệu. Chỉ một request được thành công; retry cùng idempotency key không trừ lần hai.

**Đầu ra:** Wallet + Loyalty chạy với SQL Server thực, integration test mô phỏng crash và message trùng.

### Giai đoạn 3 — HLD và Production (6–8 tuần)

Học DDD, API contracts, caching, messaging, capacity planning, HA/DR, security, Kubernetes, observability.

**Ví dụ:** bảng giá chứng khoán nhận snapshot/trades/bidask qua Event Hubs, đẩy qua WebSocket. Phải xử lý dữ liệu trễ, reconnect, event ordering và backpressure.

**Đầu ra:** C4 diagram, capacity estimate, load test, dashboard và runbook.

### Giai đoạn 4 — Solution Architecture (6–8 tuần)

Học cách review kiến trúc dựa trên business requirement, NFR, ADR, security, cost, RTO/RPO và kế hoạch tiến hóa.

**Đầu ra:** tài liệu thiết kế hệ thống OMS gồm HLD, LLD, data ownership, sequence thành công/thất bại, ADR và bằng chứng kiểm thử.

> Các khoảng thời gian là gợi ý thực hành, không phải tiêu chuẩn bắt buộc.

## 4. Ví dụ xuyên suốt: Đặt lệnh chứng khoán

**Yêu cầu:** Khách hàng có 100 triệu sức mua, gửi hai lệnh mua cùng lúc, mỗi lệnh cần phong tỏa 80 triệu.

**Sai lầm thường gặp:** hai API instance cùng `SELECT AvailableBalance = 100M`, cùng kiểm tra đủ tiền rồi ghi; hoặc chỉ dùng `lock` trong C# nên không bảo vệ được khi scale nhiều pod.

**Thiết kế đúng:** SQL Server là nguồn sự thật; dùng atomic conditional update trong transaction, ghi reservation và outbox; idempotency key chống retry trùng; consumer dùng inbox; trạng thái gateway timeout được đối soát, không tự coi là lệnh thất bại.

```sql
UPDATE dbo.CashAccounts
SET AvailableBalance = AvailableBalance - @Amount,
    ReservedBalance = ReservedBalance + @Amount
WHERE AccountId = @AccountId
  AND AvailableBalance >= @Amount;
```

Cần kiểm tra `@@ROWCOUNT = 1`, `@Amount > 0`, tạo reservation/ledger trong cùng transaction và có unique business key. Một câu UPDATE không tự giải quyết tất cả vấn đề distributed transaction.

**So sánh lựa chọn:**

| Kỹ thuật | Phù hợp khi | Không nên dùng riêng khi |
|---|---|---|
| Optimistic Concurrency | Xung đột thấp, cần phát hiện ghi đè | Hot account bị tranh chấp cao |
| Pessimistic Locking | Nhiều bước đọc/ghi phải bảo vệ cùng transaction | Phải chờ external API trong lúc giữ lock |
| Atomic Conditional Update | Điều kiện và cập nhật gói trong một câu SQL | Workflow nhiều hệ thống độc lập |
| Redis distributed lock | Điều phối job/worker | Bảo vệ tính đúng đắn tiền mà không có DB invariant |
| Saga | Quy trình nhiều service có commit riêng | Có thể giải quyết đơn giản bằng local transaction |

## 5. Lộ trình đọc tài liệu đang có — tránh học trùng

| Bạn đang thiếu gì? | Đọc tại đây |
|---|---|
| Process, thread, bộ nhớ | [Computer Science](../01-computer-science/README.md) |
| DNS, TCP, TLS, HTTP | [Linux, Git & Networking](../02-linux-git-networking/README.md) |
| C# async, ThreadPool, GC | [.NET Runtime](../03-dotnet/README.md) |
| Transaction, isolation, index | [SQL](../05-sql/README.md) |
| API security, contract | [API Design](../06-api-design/README.md) |
| Redis, invalidation | [Redis & Caching](../11-redis-caching/README.md) |
| Outbox/Inbox, failure, Saga | [Distributed Systems](../17-distributed-systems/README.md) |
| Capacity, scaling, case study | [System Design](../24-system-design/README.md) |
| DDD, CQRS, ADR, review | [Software Architecture](README.md) |
| Azure và Kubernetes | [Azure](../14-cloud/README.md) · [Kubernetes](../15-kubernetes/README.md) |

## 6. Tiêu chuẩn học và kiểm chứng

Với **mỗi** chủ đề, phải trả lời: (1) là gì, (2) giải quyết lỗi nào, (3) khi nào dùng, (4) khi nào không dùng, (5) triển khai .NET/SQL thế nào, (6) trade-off gì, (7) test lỗi production ra sao.

Một case study hoàn chỉnh cần có **Business Flow → NFR → HLD → LLD → Sequence → Failure Matrix → Test Evidence → ADR → Runbook**.

Đọc tiếp: [Bốn bài toán thực tế Financial, Loyalty, Booking và Securities](solution-architect-dotnet-case-studies.md).
