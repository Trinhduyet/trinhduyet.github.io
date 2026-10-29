# Distributed Systems, Event-Driven & High-Throughput Architecture

> [← Module 25](README.md) · [Business → HLD/LLD](solution-architecture-from-requirements-to-design.md) · [SQL & Data](data-architecture-polyglot-persistence.md)

**Bài học chung** cho finance, retail, booking, messaging, logistics và các domain khác. Đầu ra là khả năng chọn ranh giới và semantics đúng, không phải thuộc tên Kafka, Saga hay CQRS.

## 1. Vì sao phân tán làm hệ thống khó hơn?

Một process có thể dùng local memory và một database transaction. Khi tách thành nhiều node, ta phải giải quyết:

- **Partial failure:** service A không nhận response, không biết B có xử lý hay chưa.
- **Time and ordering:** clocks lệch, event đến muộn/trùng/sai thứ tự, request cũ ghi đè state mới.
- **Consistency:** read từ replica/projection chưa thấy write; các aggregate không thể cùng transaction tùy ý.
- **Coordination:** leader failover, partition, retries, locks/leases, fencing.
- **Operations:** version skew, network policy, certificate, queue backlog, dependency graph, observability.

Nên bắt đầu bằng source-of-truth và business invariants, rồi chọn consistency per flow. Không dùng câu “eventually consistent” để che một invariant bắt buộc phải đúng ngay.

## 2. Service boundaries và data ownership

Tách theo **business capability + ownership + deploy/scale/failure needs**, không tách mỗi database table thành service. Việc nhiều service cùng ghi một bảng làm mất độc lập dù deployment riêng. Nếu bắt buộc dùng shared DB trong giai đoạn chuyển đổi, ghi rõ chủ sở hữu schema, access policy và migration plan.

| Quan hệ | Khi phù hợp | Rủi ro cần thiết kế |
|---|---|---|
| Synchronous HTTP/gRPC | Người dùng cần kết quả ngay, dependency latency ổn định | Timeout budget, retry storm, circuit breaker, versioning |
| Async command/message | Công việc có thể chờ, cần durable dispatch | Duplicate, poison, ordering, backlog, status query |
| Event broadcast | Nhiều consumer phản ứng độc lập | Schema evolution, replay, consumer side effects |
| Batch/stream processing | Khối lượng lớn và data latency mục tiêu rõ | Watermark, late arrival, checkpoint, backfill |

## 3. Consistency được định nghĩa theo dữ liệu

| Consistency / guarantee | Ví dụ | Mechanism / caveat |
|---|---|---|
| Strong inside transaction | Chỉ giữ một slot cuối cùng | Atomic conditional update + constraint + transaction |
| Read-your-writes | UI thấy đơn vừa tạo | Read primary, session token, await projection/version |
| Eventual consistency | Dashboard tổng hợp số đơn | Outbox → projection, lag SLI + replay |
| Per-key ordering | Events của một order theo thứ tự | Partition/session key; sequence check và gap detection |
| Idempotent effect | Retry không trừ tiền hai lần | Durable key/hash/result + unique business constraint |

Không thể suy diễn exactly-once end-to-end từ broker exactly-once trong một phạm vi hẹp. Producer, DB, consumer và external provider có transaction boundaries khác nhau.

## 4. Idempotency và concurrency

Một idempotency record nên gồm `(scope, key, request_hash, state, result, expires_at)` với unique index. Trình tự an toàn:

1. Xác thực user/account scope, chuẩn hóa payload, tính hash.
2. Claim key trong transaction với unique constraint; cùng key + hash thì trả existing result/operation, khác hash thì conflict.
3. Áp dụng business invariant bằng conditional update/locking trong authoritative store.
4. Ghi business record và Outbox cùng transaction nếu có async integration.
5. Commit; retry đọc trạng thái đã lưu, không thực thi lại effect tùy tiện.

Key TTL phải theo business lifecycle và retry window. Nếu xóa key trước khi side effect có thể lặp, unique business ID và reconciliation vẫn phải bảo vệ. Cache/Redis lock không thay unique/transaction của source-of-truth. Distributed lock khi có lease cần **fencing token** để worker hết lease không ghi đè worker mới.

### SQL illustrative: atomic reserve

```sql
BEGIN TRANSACTION;

UPDATE dbo.Inventory
SET Available = Available - @Quantity
WHERE SkuId = @SkuId
  AND Available >= @Quantity;

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50001, 'Insufficient inventory', 1;
END;

-- Insert reservation + outbox in this same transaction.
COMMIT TRANSACTION;
```

Đây là pattern **SQL Server**, không phải đoạn transaction hoàn chỉnh. Trong lab cần validation, decimal precision, unique keys, isolation, error handling và test trên DB thật.

## 5. Transactional Outbox & Inbox

**Dual-write failure:** DB commit thành công, process chết trước khi publish event. Nếu publish trước DB commit, event có thể được consumer thấy dù business transaction rollback.

| Step | Durable data | Failure behavior |
|---|---|---|
| Commit domain + outbox | Domain row và outbox row cùng transaction | Crash trước commit: cả hai không có; sau commit: cả hai còn |
| Claim pending outbox | Lease/owner, retry count, next attempt | Worker restart có thể gửi lại |
| Broker publish | Message ID, key, schema version | Publisher ACK mất: producer không biết đã gửi chưa |
| Consumer inbox | Unique message/business key cùng local business effect | Redelivery không tạo effect thứ hai |
| Cleanup/archive | Retention, replay policy | Không xóa sớm message chưa xử lý hoặc audit cần giữ |

Nếu consumer gọi external provider, local inbox **không tự đảm bảo provider effect exactly once**. Cần provider idempotency key/status API, UNKNOWN state và reconciliation. Với poison message, DLQ giữ payload/metadata có giới hạn bảo mật; replay phải có quyền, version compatibility và dry-run.

## 6. Saga: quy trình liên service

**Saga không phải distributed rollback SQL.** Một bước đã phát sinh external effect có thể chỉ bù trừ bằng một business action khác (refund, release, reverse posting), đôi khi bù trừ không hoàn hảo.

| Orchestration | Choreography |
|---|---|
| Coordinator biết flow/state, có thể quan sát timeout/unknown | Các participant phản ứng event, coupling về workflow ít rõ hơn |
| Phù hợp flow dài, nhiều nhánh/compensation, audit cao | Phù hợp chuỗi nhỏ, event ownership rõ |
| Rủi ro central coordinator complexity | Rủi ro hidden cycles, event storm, khó truy vết end-to-end |

Một saga step nên lưu `operation_id`, state, attempt, deadline, external reference, result, compensation state. Những trạng thái **UNKNOWN/PENDING_RECONCILIATION** quan trọng ngang SUCCESS/FAILED. Retry phải bounded + exponential backoff/jitter; không retry bừa non-idempotent action.

## 7. Messaging & event streaming decision table

| Nhu cầu | Message queue/command broker | Log/event stream |
|---|---|---|
| Work distribution và delayed retry/DLQ | Thường tự nhiên hơn | Cần thiết kế thêm workflow/retry topic |
| Replay lịch sử, multiple independent consumers | Có thể hạn chế theo broker/retention | Thường là đặc tính cốt lõi |
| Per-key ordering | Session/group hoặc queue semantics tùy nền tảng | Partition key + single ordered partition |
| Throughput lớn, analytics/CDC | Có giới hạn theo partition/entity/operation | Phù hợp append stream, chịu chi phí vận hành/retention |
| Request-response synchronous | Không phải default | Không phải default |

Khi chọn Azure Service Bus, Event Hubs, Kafka, RabbitMQ hay storage queue, hãy lập ADR theo throughput, partition/order, replay, retention, quota, operational skill, region/HA, security và TCO. Không coi các broker có cùng semantics. **Kiểm chứng trên phiên bản dịch vụ cụ thể** trước khi khẳng định guarantees.

## 8. Backpressure, load shedding và high throughput

- **Bounded queue:** giới hạn memory; khi đầy phải block/admit/reject theo ưu tiên, không tăng RAM vô hạn.
- **Queue-age SLI:** backlog count không đủ; một message 2 giờ tuổi quan trọng hơn 10.000 message mới.
- **Retry budget:** retry traffic có thể nhân tải của dependency đã quá tải; giới hạn theo deadline và priority.
- **Bulkhead:** tách thread/connection pool cho dependency critical vs best-effort.
- **Hot partition:** phân phối theo customer/order key có thể skew; đo top keys và partition occupancy.
- **Batch/checkpoint:** lưu offset cùng effect có idempotency, checkpoint trước effect có thể mất dữ liệu, checkpoint sau effect có thể lặp.

### Capacity exercise

Input peak 8.000 events/s trong 10 phút; consumers xử lý 6.000/s. Backlog tăng 1,2 triệu events. Sau peak, nếu arrival 1.000/s và consumers giữ 6.000/s, drain tối thiểu `1.2M / 5k = 240s` trước overhead/retry. Nếu business yêu cầu 1 phút, phải scale, ưu tiên hoặc giới hạn input; chỉ tính average RPS là sai.

## 9. Failure matrix dùng cho mọi domain

| Failure | Detection | Safe response | Test |
|---|---|---|---|
| Provider xử lý rồi timeout | UNKNOWN, correlation ID | Query/reconcile; không tự tạo business ID mới | Drop response after commit |
| Producer crash after DB commit | Outbox pending age | Relaunch, at-least-once publish | Kill worker after commit |
| Duplicate/reordered event | Inbox unique + sequence | Dedup, buffer/re-fetch gap | Replay duplicate/out-of-order |
| Broker unavailable | Publish backlog/queue age | Retry bounded, degrade noncritical writes | Disconnect broker |
| DB failover | Error/latency, lock waits | Retry only safe operations, reconcile ambiguous commits | Failover while writing |
| Cache inconsistent | Version/TTL lag | Fallback authoritative read or mark stale | Invalidate/delay cache |
| Consumer poison | Retry count/DLQ age | Quarantine, alert, safe replay after fix | Bad schema/version |

## 10. POC / lab bắt buộc (domain-neutral)

1. **L05:** 100 concurrent claims chống double-spend/oversell trên SQL Server hoặc PostgreSQL thật.
2. **L06:** transaction + Outbox/Inbox, kill publisher after commit, restart, replay duplicate, chứng minh một logical effect.
3. **L08:** broker thật, partition/order scope, poison message/DLQ, queue-age/backpressure và replay.
4. **Integration:** external provider simulator trả timeout sau khi thực thi; hệ thống phải xác định UNKNOWN và reconcile.
5. **Architecture review:** viết ADR chọn modular monolith vs service split cho Banking/Booking/Commerce; chỉ tách khi đo được lợi ích.

**Evidence:** source, schema/migrations, compose, run commands, test logs, tracing, benchmark environment, failures observed và known limitations. Một snippet lý thuyết không đạt tiêu chí hoàn thành lab.

## Nguồn chính thống

- [RabbitMQ .NET Client Guide](https://www.rabbitmq.com/client-libraries/dotnet-api-guide)
- [Apache Kafka documentation](https://kafka.apache.org/documentation/)
- [Azure Service Bus documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Event Hubs documentation](https://learn.microsoft.com/en-us/azure/event-hubs/)
