# SQL, EF Core & Distributed Systems

<div class="lesson-meta"><span>Data correctness</span><span>Concurrency</span><span>Messaging</span></div>

<div class="course-grid" markdown>

[**SQL / EF Core — 9 câu có lời giải** · indexes, isolation, keyset, N+1, plan, outbox.](../05-interview-preparation/sql-data.md){ .course-card }

[**Distributed / Azure — 9 câu có lời giải** · Service Bus, Event Hubs, ACK, retry, Redis, deployment.](../05-interview-preparation/distributed-cloud.md){ .course-card }

[**SQL Performance Lab** · dựng synthetic data, đo plan, so index, keyset.](../06-practical-labs/sql-performance-lab.md){ .course-card }

</div>

## Hai quyết định cần giải thích rõ

### Query nhanh có đủ chưa?

Không. Query phải **đúng dữ liệu, đúng tenant, đúng isolation và đúng workload**. Composite index phù hợp có thể giúp hàng trăm triệu rows nhưng index không chữa truy vấn unbounded hoặc điều kiện lọc sai. Benchmark cần IO, p95, locking và data semantics.

[SQL Indexes](../../05-sql/indexes-execution-plans-and-operations.md) · [EF Core query shape](../../05-sql/ef-core-query-shape-and-sql.md).

### Message đã ACK có nghĩa không xử lý trùng?

Không. Worker có thể commit SQL trước rồi chết trước ACK; redelivery là bình thường. Cần stable operation identity và unique constraint/inbox khi business effect phải idempotent. Outbox giúp atomic commit data+intent, không đảm bảo provider ngoài hệ thống exactly-once.

[Outbox/Inbox](../../17-distributed-systems/messaging-outbox-inbox-and-dedup.md) · [Partial failures](../../17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md).

## Nên luyện theo thứ tự

1. DB transactions & concurrency.
2. Execution plans, index design và EF Core N+1.
3. Redis cache consistency, stale reads và cache tenant scope.
4. Broker semantics (EH, ASB, RabbitMQ), backpressure và dead letter.
5. Outbox/inbox, retries, unknown outcome và reconciliation.
6. System Design: [Notifications](../09-solution-architecture-leadership/design-exercises.md).

**Chốt buổi:** tự giải thích failure giữa SQL COMMIT và Message ACK bằng sequence 4 bước.
