# Data & Distributed Systems — SQL, Redis, Messaging

## Chủ đề cốt lõi

- SQL Server/PostgreSQL: index design, SARGability, execution plan, isolation, transactions, keyset pagination.
- EF Core: projection, N+1, AsNoTracking, compiled queries (when justified), concurrency tokens.
- Redis: cache-aside, key scope, invalidation races, stampede, rate limits.
- Messaging: Event Hubs vs Service Bus vs RabbitMQ, partition, consumer group, queue, acknowledgement, retry, DLQ.
- Distributed correctness: outbox/inbox, idempotency, ordering, UNKNOWN result, reconciliation.

## Worked interview question

**Q: Bảng có 100 triệu rows có bắt buộc query chậm?** Không. Đo actual plan, selectivity, logical reads và predicate. Query theo `InstrumentID` + khoảng ngày có thể dùng composite index hiệu quả; query bỏ time boundary có thể scan lịch sử của một instrument.

**SQL Server keyset example:**

```sql
SELECT TOP (100) Id, CreatedAt, Amount
FROM dbo.Transactions
WHERE WalletId = @walletId
  AND (CreatedAt > @lastCreatedAt
       OR (CreatedAt = @lastCreatedAt AND Id > @lastId))
ORDER BY CreatedAt, Id;
```

Cần index phù hợp và stable tie-breaker; không dùng cú pháp tuple comparison của PostgreSQL trong T-SQL. **NOLOCK không phải mẹo tăng tốc an toàn mặc định**; có thể gây dữ liệu sai/lặp/thiếu.

**Follow-up:** Consumer crash giữa DB commit và broker ACK? Message có thể redeliver; unique operation key hoặc inbox transaction giúp tránh double effect.

[SQL](../../05-sql/README.md) · [EF Core](../../05-sql/ef-core-query-shape-and-sql.md) · [Redis](../../11-redis-caching/README.md) · [Distributed patterns](../../17-distributed-systems/README.md).
