# Data Architecture — SQL Server, PostgreSQL, Oracle, MongoDB, Cache & Governance

> [← Module 25](README.md) · [Distributed Systems](distributed-systems-architecture-deep-dive.md) · [SQL nền tảng](../05-sql/README.md)

Solution Architect chọn **mô hình dữ liệu, consistency, ownership, lifecycle và cách vận hành** trước khi chọn nhãn database. Bài này là kiến thức cross-vendor; cú pháp/semantics cụ thể phải được xác minh trên version và isolation mode đang dùng.

## 1. Workload-first data design

Trước khi tạo bảng/collection, phải trả lời:

- OLTP hay OLAP? Point lookup, range scan, join, full-text, aggregation, time-series hay graph?
- Read/write ratio, peak TPS, top queries, hot key, dữ liệu tăng mỗi ngày, retention và archive?
- Bất biến nào cần transaction cùng lúc? Những trạng thái nào là projection có thể dựng lại?
- Multi-tenant isolation, PII classification, encryption, data residency, audit/legal hold?
- RPO/RTO, backup/restore/PITR, replication lag, failover semantics, licensing/operations skill?

### Định nghĩa ownership

Mỗi business entity có một authoritative writer, hợp đồng đọc/ghi, chính sách schema evolution. Derived cache/index/search/analytics không được âm thầm trở thành nơi quyết định invariant (ví dụ balance, last stock, booking capacity).

## 2. So sánh hệ quản trị — không có một lựa chọn thắng mọi tình huống

| Góc nhìn | SQL Server | PostgreSQL | Oracle Database | MongoDB |
|---|---|---|---|---|
| Mô hình chính | Relational OLTP, T-SQL | Relational/extensible, SQL | Enterprise relational, PL/SQL | Document, aggregation |
| Transaction | ACID; lock/row versioning theo isolation | ACID; MVCC snapshot per statement/transaction theo isolation | ACID; multiversion read consistency | Atomic single-document; multi-document transaction hỗ trợ nhưng thêm chi phí |
| Concurrency | Lock, rowversion, RCSI/Snapshot tùy cấu hình | MVCC, row-level locks, serializable SSI | Read consistency, locks, isolation/serialization behavior theo mode | Atomic document update, unique indexes, retryable writes/transaction semantics theo deployment |
| Typical strength | .NET/SQL ecosystem, operational tooling | Open ecosystem, extensions, rich SQL | Enterprise features/operations, established estates | Flexible aggregate-shaped documents, document access patterns |
| Attention | Query Store/plan regression, tempdb/version store, license | Vacuum/autovacuum, bloat, replica lag, connection scaling | Licensing/DBA skills, RAC/Data Guard operational complexity | Document growth, indexes, shard key, cross-document invariant, schema governance |

**Không kết luận:** MongoDB “không có transaction”, PostgreSQL “không có lock”, hay RCSI = Snapshot isolation. Các khả năng phụ thuộc version, topology và configuration.

### Concurrency nuance cần biết

- **SQL Server:** default READ COMMITTED có thể lock-based; khi bật `READ_COMMITTED_SNAPSHOT`, statement read thường dùng row version. `SNAPSHOT` là transaction-level snapshot riêng và update conflict có thể xảy ra. `UPDLOCK`/`HOLDLOCK` và index shape ảnh hưởng contention.
- **PostgreSQL:** READ COMMITTED tạo snapshot mỗi statement; REPEATABLE READ giữ transaction snapshot; SERIALIZABLE dùng Serializable Snapshot Isolation và có thể yêu cầu retry khi serialization failure. `SELECT ... FOR UPDATE` khi cần row locking.
- **Oracle:** READ COMMITTED có statement-level read consistency; SERIALIZABLE có thể báo lỗi serialization. Không giả định behavior giống PostgreSQL chỉ vì cùng tên isolation.
- **MongoDB:** thiết kế aggregate/document để tối đa hóa atomic single-document update; unique indexes, update filters, transaction retry và write concern/read concern phải được thiết kế rõ.

## 3. Transaction boundary, locking và isolation

Ví dụ credit transfer nội bộ cần kiểm soát debit/credit cùng posting group, unique transfer ID, validation account/currency, ledger balanced và reconciliation. Một `UPDATE balance` không đủ để chứng minh money correctness.

| Vấn đề | Triệu chứng | Biện pháp / evidence |
|---|---|---|
| Lost update | Hai writer ghi đè | Version check/conditional update; parallel test |
| Write skew | Hai transaction đọc chung invariant rồi ghi hai hàng khác | Serializable/explicit lock/re-model constraint; interleaving test |
| Deadlock | Circular wait, transaction victim | Stable lock order, short transaction, bounded safe retry; deadlock graph |
| Hot row | p99 lock wait tăng theo traffic | Partition by legitimate ownership, queue/serialize per key, workload redesign |
| Long transaction | Log/version store/locks tăng | Bound transaction, chunk batch, no HTTP call inside SQL transaction |
| Connection exhaustion | Pool wait/timeout | Fix leak/long queries, right-size pool, pool metrics; không chỉ tăng max pool |

### Query & index design

1. Capture representative query shape/parameter distribution và actual plan.
2. Kiểm tra cardinality estimate, scans/seeks, joins, sort/spill, key lookup, missing index suggestion như **tín hiệu**, không auto-apply.
3. Chọn composite index theo predicates, sort, selectivity, covering và write amplification.
4. Đo p50/p95/p99 và CPU/IO/locks trên production-like data.
5. Regressions: parameter sniffing/plan caching, stats freshness, plan changes, schema migration.

Đừng dùng database benchmark nhỏ, mọi thứ nằm trong RAM để ngoại suy thành throughput production.

## 4. Cache architecture

| Pattern | Khi dùng | Failure cần giải quyết |
|---|---|---|
| Cache-aside | Read-heavy, có source of truth | Invalidation sau write, stampede, stale TTL |
| Write-through | Read consistency thuận tiện hơn ở một số workload | Dual-write atomicity với DB không tự có |
| Stale-while-revalidate | Cho phép stale trong thời gian có kiểm soát | Staleness budget, user-visible freshness |
| Near-cache | Giảm network hop | Distributed invalidation/versioning |
| Distributed lock/lease | Serialize side work, leader coordination | Lease expiry, fencing, split brain |

**Rule:** ghi rõ cache key schema, TTL, invalidation trigger, max staleness, cache miss fallback, cost và quyền truy cập tenant. Với money/inventory/risk, authoritative transaction không được phụ thuộc duy nhất vào Redis lock.

## 5. Large-scale processing, CDC và data pipelines

- **Batch:** pagination/keyset scan thay OFFSET lớn, chunk size, checkpoint, idempotent upsert, restartability.
- **CDC:** hiểu transaction log/oplog source, at-least-once delivery, snapshot + incremental boundary, schema changes, out-of-order/late events.
- **Streaming:** event-time vs processing-time, watermark, dedup window, partition key, state store retention, replay strategy.
- **OLAP:** tách transactional write path khỏi reports; data warehouse/lakehouse khi query patterns justify.
- **Backfill:** chạy song song với live ingest phải chống ghi đè version mới; ghi rõ source offsets/cutover point.

## 6. Multi-tenancy và data governance

| Tenant model | Lợi ích | Rủi ro / lựa chọn |
|---|---|---|
| Shared schema + tenant ID | Chi phí thấp, vận hành đơn giản | Broken object-level authorization, noisy neighbor, index/partition design |
| Schema per tenant | Isolation logic tốt hơn | Migrations và management tăng |
| Database per tenant | Isolation, backup/restore per tenant | Cost, migrations/fleet operations tăng |

Defense-in-depth: resource-level authz ở API/worker, tenant-aware query predicate/row-level security khi phù hợp, service identity, encryption, audit, negative tests. **Tenant ID từ request body không phải căn cứ xác thực.**

Data catalog tối thiểu: owner, classification, origin/lineage, retention, encryption, consent/purpose, deletion/legal hold, backup retention và consumers. Quy trình xóa theo chính sách phải tính cả cache, logs, analytics và restore backups.

## 7. Zero-downtime migration: expand–migrate–contract

1. **Expand:** add nullable column/table/new contract version, không phá old writer/reader.
2. **Deploy compatibility:** đọc được cả old/new, dual-write chỉ khi có reconciliation và failure plan.
3. **Backfill:** idempotent batches, cursor, row count/checksum, report drift.
4. **Cutover:** feature flag/traffic migration, monitor p99, error and data drift.
5. **Contract:** chỉ drop old schema khi mọi phiên bản cũ/người tiêu thụ đã migrate và rollback window kết thúc.

Không gọi mọi DDL là online. Lock duration, table rewrite, index build, replication/backup impact phụ thuộc engine/version. Rehearse trên data đại diện và có rollback/forward-fix plan.

## 8. HA/DR cho data layer

- Phân biệt backup (recovery history), replication (availability), archive (retention/cost) và reconciliation (business correctness).
- SQL Server Always On, PostgreSQL primary/replica, Oracle Data Guard, Mongo replica sets/sharding có failover/consistency trade-offs riêng.
- Read replicas có thể lag; không dùng để phê duyệt invariant cần current data nếu không có đảm bảo phù hợp.
- Restore drill phải đo **actual RTO/RPO**, verify constraints và reconcile external effects; backup job xanh không phải bằng chứng restore thành công.

## 9. Practice & vendor POC

Thiết kế cùng một workload: 10M ledger entries, 1M users, p95 history read target, 500 concurrent claims trên một hot account (các số là **giả định cho lab**).

| Deliverable | Acceptance |
|---|---|
| SQL Server / PostgreSQL comparison | Schema, query plan, index, concurrency test, isolation behavior và replication lag measurement |
| Oracle research/POC | Transaction/isolation, plan management, HA/DR, licensing/operations constraints có nguồn chính thức |
| MongoDB POC | Document model, unique key, cross-document consistency, shard key và index workload benchmark |
| Cache & CDC | Stale-read experiment, invalidation, replay/backfill, source-of-truth proof |
| Governance & migration | Data catalog, tenant authz negatives, expand–contract rehearsal + rollback |

**Pass:** giải thích vì sao chọn từng engine theo workload và NFR, nêu giới hạn; không pass nếu chỉ so tốc độ insert hay dựa trên “SQL vs NoSQL” khẩu hiệu.

## Nguồn chính thống để xác minh theo version

- [Microsoft SQL Server transaction isolation](https://learn.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql)
- [PostgreSQL transaction isolation](https://www.postgresql.org/docs/current/transaction-iso.html)
- [Oracle Database documentation](https://docs.oracle.com/en/database/)
- [MongoDB transactions](https://www.mongodb.com/docs/manual/core/transactions/)
