# Lab — SQL Server index, keyset pagination và query plan

<div class="lesson-meta"><span>Runnable T-SQL in sandbox DB</span><span>Không yêu cầu dữ liệu thực</span><span>Estimated 30–45 phút</span></div>

!!! warning "Chỉ chạy trong database lab riêng"
    Script tạo bảng `dbo.InterviewTransactionsLab` trong database đang chọn. Không chạy trên shared Development/Production database. Nếu bạn không có database lab riêng, **chưa chạy**. Không sử dụng dữ liệu người dùng thật.

## 1. Setup — SQL Server sandbox

```sql
CREATE TABLE dbo.InterviewTransactionsLab
(
    Id int IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_InterviewTransactionsLab PRIMARY KEY,
    WalletId int NOT NULL,
    CreatedAt datetime2(3) NOT NULL,
    Amount decimal(18,2) NOT NULL
);
GO

;WITH n AS
(
    SELECT TOP (10000)
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT dbo.InterviewTransactionsLab(WalletId, CreatedAt, Amount)
SELECT 1 + (rn % 20),
       DATEADD(minute, rn % 500, CONVERT(datetime2(3), '2026-01-01T00:00:00')),
       CONVERT(decimal(18,2), rn % 5000)
FROM n;
GO
```

Đây là synthetic data; không tạo kết luận throughput production từ số liệu lab.

## 2. Baseline

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

SELECT TOP (50) Id, CreatedAt, Amount
FROM dbo.InterviewTransactionsLab
WHERE WalletId = 5
ORDER BY CreatedAt, Id;
```

Bật Actual Execution Plan trong SSMS. Ghi logical reads, plan nodes và elapsed time qua vài runs; first run có thể khác vì plan/data cache.

## 3. Thêm composite index

```sql
CREATE INDEX IX_InterviewTransactionsLab_Wallet_Time_Id
ON dbo.InterviewTransactionsLab (WalletId, CreatedAt, Id)
INCLUDE (Amount);
GO
```

Lặp query. Seek/scan được chọn theo optimizer và dataset nhỏ; không giả định index chắc chắn nhanh hơn.

## 4. Keyset với timestamp trùng

Lấy (`CreatedAt`, `Id`) từ row cuối trang 1 làm cursor:

```sql
DECLARE @lastTime datetime2(3) = '2026-01-01T00:00:00';
DECLARE @lastId int = 0;

SELECT TOP (50) Id, CreatedAt, Amount
FROM dbo.InterviewTransactionsLab
WHERE WalletId = 5
  AND (CreatedAt > @lastTime
       OR (CreatedAt = @lastTime AND Id > @lastId))
ORDER BY CreatedAt, Id;
```

Kiểm thử nhiều cursor lấy thật từ trang trước để không trùng/thiếu rows. Sau đó thêm inserts đồng thời và thảo luận isolation/sort-key mutation.

## 5. Negative experiments

- Thay `WHERE WalletId = 5` bằng không filter → compare plan.
- Dùng `WHERE CONVERT(date, CreatedAt) = '2026-01-01'` và bounded range; so SARGability.
- OFFSET 5000 vs keyset theo cursor gần cuối.
- Trình bày khi nào index thêm làm chậm INSERT/UPDATE.

## 6. Cleanup (trên lab DB)

```sql
DROP TABLE dbo.InterviewTransactionsLab;
```

## Exit criteria

Cần có ảnh/plan hoặc text metrics, correct paging order, SQL script + giải thích trade-off. Lab chưa được thực thi tại đây; kết quả phải do người học tự chạy và ghi.

[SQL Interview](../05-interview-preparation/sql-data.md) · [Indexes & Plans](../../05-sql/indexes-execution-plans-and-operations.md).
