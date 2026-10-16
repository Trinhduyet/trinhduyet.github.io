# SQL Server / EF Core — giải thích bằng truy vấn, plan và concurrency

<div class="lesson-meta"><span>Track: Database</span><span>9 câu thực hành</span><span>SQL Server syntax</span></div>

## DB-01. Index Seek luôn tốt hơn Index Scan?

**Không.** Seek tìm phạm vi key phù hợp nhưng có thể trả rất nhiều rows; Scan hợp lý nếu trả phần lớn bảng. Đánh giá **logical reads, CPU, elapsed time, estimates vs actual, lookup count** thay vì chỉ nhìn icon kế hoạch.

**Ví dụ:** 50 triệu trade rows nhưng query có `WHERE InstrumentId = @id AND TradeTime >= @from AND TradeTime < @to` có thể dùng index (`InstrumentId`, `TradeTime`). Quy mô bảng không phải latency.

**Follow-up:** Vì sao key lookup hàng trăm nghìn lần khiến query chậm? Khi nào covering index tốt hơn, write overhead là gì?

## DB-02. SARGability là gì?

**Trả lời:** Giữ indexed column ở dạng có thể so phạm vi thay vì áp hàm lên cột.

```sql
-- Khó tận dụng seek theo TradeTime:
WHERE CAST(TradeTime AS date) = @day

-- Predicate kiểu half-open range:
WHERE TradeTime >= @day
  AND TradeTime < DATEADD(day, 1, @day)
```

**Follow-up:** Implicit conversion do khác datatype/collation gây lỗi plan ra sao? Có nên chuyển mọi index thành INCLUDE tất cả columns không?

## DB-03. Keyset pagination đúng trong T-SQL?

Giả sử (`CreatedAt`, `Id`) là thứ tự tăng với `Id` unique.

```sql
SELECT TOP (50) Id, CreatedAt, Amount
FROM dbo.Transactions
WHERE WalletId = @walletId
  AND (
    CreatedAt > @lastCreatedAt
    OR (CreatedAt = @lastCreatedAt AND Id > @lastId)
  )
ORDER BY CreatedAt, Id;
```

Cần composite index tương thích, tie-breaker unique và cursor chứa cả hai giá trị. **T-SQL không hỗ trợ** cú pháp (`CreatedAt`, `Id`) > (...).

**Trade-off:** OFFSET phù hợp page nhỏ nhưng skip sâu tốn IO; keyset không nhảy tùy ý tới page 10.000, và concurrent updates của sort key vẫn có thể làm thứ tự biến động.

## DB-04. `NOLOCK` có phải cách sửa blocking tốt?

**Không mặc định.** READ UNCOMMITTED/NOLOCK có thể đọc dirty, thiếu, lặp rows; không phù hợp ledger hoặc báo cáo đòi correctness. Cân nhắc RCSI/SNAPSHOT sau khi đánh giá version store, consistency semantics và workload.

**Follow-up:** RCSI cho snapshot ở phạm vi statement; SNAPSHOT isolation có view transaction-level — vì sao cần hiểu write conflicts?

## DB-05. Hai request trừ cùng số dư xử lý thế nào?

Với 100.000 điểm, hai request đồng thời đòi 80.000:

```sql
BEGIN TRAN;
UPDATE dbo.Wallet
SET Balance = Balance - @amount
WHERE WalletId = @walletId
  AND Balance >= @amount;

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK;
    THROW 50001, 'Insufficient balance or missing wallet', 1;
END;

-- Trong cùng transaction: insert ledger/redemption/inbox/outbox
COMMIT;
```

Bước UPDATE phải gắn với unique business idempotency key và ledger trong cùng transaction; snippet chỉ mô tả invariant balance, **chưa phải API payment production-ready**.

**Follow-up:** Deadlocks, timeout sau COMMIT nhưng trước response, key reuse với payload khác?

## DB-06. EF Core `AsNoTracking` và N+1?

`AsNoTracking` giảm tracking overhead cho read-only projection; không khiến SQL query được tối ưu tự động. N+1 có thể xuất hiện khi iterate navigation/lazy load; dùng projection/explicit loading hoặc split query theo shape, đo câu SQL phát ra.

```csharp
var page = await db.Orders.AsNoTracking()
    .Where(x => x.TenantId == tenantId)
    .OrderByDescending(x => x.CreatedAt)
    .Select(x => new OrderListDto(x.Id, x.CreatedAt, x.Status))
    .Take(50)
    .ToListAsync(ct);
```

**Follow-up:** Khi Include nhiều collections tạo cartesian explosion, khi nào dùng AsSplitQuery? Các queries trong split query có nhìn cùng snapshot không?

## DB-07. Parameter sniffing và plan regression?

SQL Server có thể cache plan dựa parameter lúc compile; phân bố dữ liệu lệch (hot/cold tenant, instrument) khiến plan phù hợp giá trị này nhưng tệ ở giá trị khác. Đo actual vs estimated rows, Query Store, index/filters; `OPTION (RECOMPILE)` hay `OPTIMIZE FOR` có chi phí, không chữa mù.

## DB-08. Transaction vs Outbox?

Một transaction SQL đảm bảo atomicity cho các thay đổi cùng DB. Không thể atomically commit SQL và gửi SMTP/third-party HTTP chỉ bằng `TransactionScope` thông thường. Outbox lưu intent trong cùng DB transaction; worker publish sau; consumer cần idempotency vì publish lại là bình thường.

**Follow-up:** Crash sau broker send trước đánh dấu published? Duplicate event; unique message operation key ở consumer.

## DB-09. Làm sao chứng minh query tối ưu?

Lưu SQL+parameters an toàn, execution plan, STATISTICS IO/TIME, số rows trả về, workload và index state. Test lại business semantics (đúng số lượng/timestamp/tenant), compare p95 under representative load; ghi môi trường và rollback.

**Lab:** [SQL keyset + isolation](../06-practical-labs/sql-performance-lab.md). **Đọc sâu:** [SQL plans](../../05-sql/indexes-execution-plans-and-operations.md) · [EF shape](../../05-sql/ef-core-query-shape-and-sql.md).
