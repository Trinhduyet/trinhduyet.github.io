# Relational Model, Schema và SQL

> [← SQL overview](README.md) · [SQL references](references.md)

## Hiểu trong 5 phút

Relational database không chỉ là nơi EF Core lưu object.

Schema là nơi bạn mô hình hóa:

```text
Entity identity
Relationships
Invariants
Uniqueness
Nullability
Data types
```

Một schema tốt giúp database từ chối state sai **kể cả khi application có bug hoặc nhiều requests chạy đồng thời**.

```mermaid
flowchart LR
    A[Business invariant] --> B[Schema]
    B --> C[PK / FK / UNIQUE / CHECK]
    C --> D[Queries]
    D --> E[Indexes / Plans]
    E --> F[Application behavior]
```

---

# 1. Từ business rule tới schema

Business rules:

```text
Customer belongs to one Tenant.
Order belongs to one Customer.
External order id unique inside a Tenant.
Order amount cannot be negative.
```

Schema:

```sql
CREATE TABLE dbo.Customers
(
    Id          bigint        NOT NULL,
    TenantId    int           NOT NULL,
    Name        nvarchar(200) NOT NULL,

    CONSTRAINT PK_Customers
        PRIMARY KEY (Id),

    CONSTRAINT UX_Customers_Tenant_Id
        UNIQUE (TenantId, Id)
);

CREATE TABLE dbo.Orders
(
    Id              bigint          NOT NULL,
    TenantId        int             NOT NULL,
    CustomerId      bigint          NOT NULL,
    ExternalOrderId varchar(100)     NOT NULL,
    Status          varchar(30)      NOT NULL,
    TotalAmount     decimal(18, 2)  NOT NULL,
    CreatedAt       datetime2       NOT NULL,

    CONSTRAINT PK_Orders
        PRIMARY KEY (Id),

    CONSTRAINT FK_Orders_Customers
        FOREIGN KEY (TenantId, CustomerId)
        REFERENCES dbo.Customers(TenantId, Id),

    CONSTRAINT UX_Orders_Tenant_ExternalOrderId
        UNIQUE (TenantId, ExternalOrderId),

    CONSTRAINT CK_Orders_TotalAmount_NonNegative
        CHECK (TotalAmount >= 0)
);
```

Composite FK `(TenantId, CustomerId)` helps enforce tenant/customer relationship instead of trusting application filter only.

---

# 2. Primary Key không chỉ để EF tracking

Primary Key defines row identity.

```sql
CONSTRAINT PK_Orders PRIMARY KEY (Id)
```

Questions:

```text
Identity global hay tenant-scoped?
Natural key stable không?
Surrogate key cần không?
Key size ảnh hưởng indexes/FKs thế nào?
```

Không có rule “mọi PK phải GUID” hoặc “mọi PK phải bigint”. Chọn theo identity, distribution, write pattern và operational constraints.

---

# 3. Surrogate Key + Business Unique Key

Common design:

```text
Id = internal surrogate key
ExternalOrderId = business/external identity
```

Enforce uniqueness:

```sql
CREATE UNIQUE INDEX UX_Orders_Tenant_ExternalOrderId
ON dbo.Orders(TenantId, ExternalOrderId);
```

Application lookup:

```sql
SELECT Id, Status, TotalAmount
FROM dbo.Orders
WHERE TenantId = @tenantId
  AND ExternalOrderId = @externalOrderId;
```

Unique constraint/index is concurrency-safe invariant; `SELECT then INSERT` check alone is not.

---

# 4. Foreign Key protects referential integrity

Without FK:

```text
Order.CustomerId = 999999
but customer does not exist
```

FK rejects invalid relationship.

```sql
ALTER TABLE dbo.OrderItems
ADD CONSTRAINT FK_OrderItems_Orders
FOREIGN KEY (OrderId)
REFERENCES dbo.Orders(Id);
```

Delete behavior needs explicit business decision:

```text
RESTRICT / NO ACTION
CASCADE
soft delete
archive
```

Don't enable cascade delete on large/deep graph without understanding lock/log/operational impact.

---

# 5. NULL is a domain decision

Bad habit:

```text
Make every column nullable because migration is easier.
```

Ask:

```text
Can this value genuinely be unknown/not-applicable?
Or is NULL hiding incomplete state?
```

Example:

```sql
PaidAt datetime2 NULL
```

may be valid because unpaid order has no payment time.

But:

```sql
TenantId int NULL
```

may be invalid if every Order must belong to tenant.

---

# 6. `CHECK` constraint

```sql
ALTER TABLE dbo.OrderItems
ADD CONSTRAINT CK_OrderItems_Quantity_Positive
CHECK (Quantity > 0);
```

Application still validates for friendly response:

```csharp
if (request.Quantity <= 0)
{
    return Results.ValidationProblem(new Dictionary<string, string[]>
    {
        ["quantity"] = ["Quantity must be greater than zero."]
    });
}
```

Two layers solve different concerns:

```text
Application validation → UX / early rejection
DB constraint           → invariant under all writers/concurrency
```

---

# 7. Data type choices matter

Money:

```sql
TotalAmount decimal(18, 2)
```

Avoid floating-point for exact monetary values.

Text:

```sql
nvarchar(200)
```

Bound length if domain has meaningful limit; unlimited columns affect storage/index/options.

Time:

```sql
CreatedAt datetime2
```

For distributed systems, be explicit about UTC/offset semantics at application boundary.

EF model:

```csharp
public sealed class Order
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Do not assume .NET type mapping automatically reflects desired precision/length.

---

# 8. EF Core schema configuration

```csharp
modelBuilder.Entity<Order>(entity =>
{
    entity.ToTable("Orders", "dbo");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.ExternalOrderId)
        .HasMaxLength(100)
        .IsRequired();

    entity.Property(x => x.Status)
        .HasMaxLength(30)
        .IsRequired();

    entity.Property(x => x.TotalAmount)
        .HasPrecision(18, 2);

    entity.HasIndex(x => new
        {
            x.TenantId,
            x.ExternalOrderId
        })
        .IsUnique();
});
```

Migration output must still be reviewed as SQL/schema change, not accepted blindly because EF generated it.

---

# 9. Basic query shape

```sql
SELECT
    o.Id,
    o.Status,
    o.TotalAmount,
    c.Name AS CustomerName
FROM dbo.Orders AS o
JOIN dbo.Customers AS c
  ON c.TenantId = o.TenantId
 AND c.Id = o.CustomerId
WHERE o.TenantId = @tenantId
  AND o.CreatedAt >= @from
ORDER BY o.CreatedAt DESC;
```

Review:

```text
Filter selective không?
Join relationship/index hỗ trợ không?
Rows bounded không?
Sort có index support không?
Projection có thừa columns không?
```

---

# 10. INNER JOIN vs LEFT JOIN

INNER JOIN:

```sql
SELECT o.Id, p.PaymentId
FROM dbo.Orders o
JOIN dbo.Payments p
  ON p.OrderId = o.Id;
```

Only Orders having matching Payment.

LEFT JOIN:

```sql
SELECT o.Id, p.PaymentId
FROM dbo.Orders o
LEFT JOIN dbo.Payments p
  ON p.OrderId = o.Id;
```

Returns Orders even without Payment; payment columns become NULL.

Choice is business semantics, not style preference.

---

# 11. Aggregation

```sql
SELECT
    CustomerId,
    COUNT(*) AS OrderCount,
    SUM(TotalAmount) AS Revenue
FROM dbo.Orders
WHERE TenantId = @tenantId
GROUP BY CustomerId
HAVING SUM(TotalAmount) > 10000
ORDER BY Revenue DESC;
```

Distinguish:

```text
WHERE  → filters rows before grouping
HAVING → filters groups after aggregation
```

---

# 12. CTE

```sql
WITH PaidOrders AS
(
    SELECT
        Id,
        CustomerId,
        TotalAmount,
        CreatedAt
    FROM dbo.Orders
    WHERE TenantId = @tenantId
      AND Status = 'Paid'
)
SELECT
    CustomerId,
    SUM(TotalAmount) AS Revenue
FROM PaidOrders
GROUP BY CustomerId;
```

CTE improves query expression/readability in many cases. It is not automatically materialized cache/table.

Execution behavior still comes from optimizer/plan.

---

# 13. Window function

```sql
SELECT
    CustomerId,
    CreatedAt,
    TotalAmount,
    ROW_NUMBER() OVER
    (
        PARTITION BY CustomerId
        ORDER BY CreatedAt DESC
    ) AS OrderNumber,
    SUM(TotalAmount) OVER
    (
        PARTITION BY CustomerId
        ORDER BY CreatedAt
    ) AS RunningRevenue
FROM dbo.Orders
WHERE TenantId = @tenantId;
```

Useful for ranking/running aggregates without collapsing rows like `GROUP BY`.

Still inspect Sort/Window operators and indexes on large workloads.

---

# 14. Normalization first

Order data:

Bad denormalized repetition:

```text
Orders
- CustomerName
- CustomerEmail
- CustomerAddress
- Product1Name
- Product2Name
...
```

Better normalized model:

```text
Customers
Orders
OrderItems
Products
```

Benefits:

```text
less update anomaly
clear ownership/invariants
reusable relationships
```

But read path may require joins/projections. Denormalization is later trade-off, not default beginner shortcut.

---

# 15. Denormalization when justified

Example immutable snapshot field:

```text
Order.ShippingAddressJson
```

May be valid because business needs address-at-order-time, not current Customer address.

This is **business snapshot semantics**, not just performance hack.

Other denormalization/read model cases:

```text
reporting tables
materialized projections
search index
cached aggregates
```

Need source-of-truth + rebuild/update semantics.

---

# 16. Multi-tenant query safety

Danger:

```sql
SELECT *
FROM dbo.Orders
WHERE Id = @id;
```

If ID can be guessed across tenants, app-level authorization must prevent cross-tenant access.

Safer query boundary when tenant identity part of ownership:

```sql
SELECT Id, Status, TotalAmount
FROM dbo.Orders
WHERE TenantId = @tenantId
  AND Id = @id;
```

Also enforce tenant relationship constraints where possible.

Security is not solved by adding `TenantId` column alone. Every access path, background job, export/admin tool must respect policy.

---

# 17. Pagination needs deterministic ordering

Bad:

```sql
SELECT Id, CreatedAt
FROM dbo.Orders
ORDER BY CreatedAt DESC
OFFSET @skip ROWS
FETCH NEXT @take ROWS ONLY;
```

If many rows share same `CreatedAt`, ordering may be non-deterministic across pages.

Add tie-breaker:

```sql
ORDER BY CreatedAt DESC, Id DESC
```

Then index/query design can align.

---

# 18. Avoid `SELECT *`

Instead:

```sql
SELECT
    Id,
    Status,
    TotalAmount,
    CreatedAt
FROM dbo.Orders
WHERE TenantId = @tenantId;
```

Reasons:

```text
network payload
materialization cost
contract coupling to schema
covering-index possibilities
future large columns
```

---

# 19. Migration review

Generated migration:

```csharp
migrationBuilder.AddColumn<string>(
    name: "ExternalOrderId",
    table: "Orders",
    type: "varchar(100)",
    nullable: false,
    defaultValue: "");
```

Questions:

```text
Existing millions of rows get what value?
Unique constraint can be added immediately?
Table lock/log impact?
Can old app version work with new schema during rolling deploy?
Need expand/backfill/contract rollout?
```

Migration code compiling is not production migration proof.

---

# 20. Failure experiment — constraint under concurrency

Without unique constraint:

```text
Request A checks ExternalOrderId absent
Request B checks ExternalOrderId absent
A inserts
B inserts
```

Both may succeed.

Add unique index:

```sql
CREATE UNIQUE INDEX UX_Orders_Tenant_ExternalOrderId
ON dbo.Orders(TenantId, ExternalOrderId);
```

Run concurrent insert test and verify database enforces invariant.

---

# 21. Failure experiment — orphan data

In a disposable DB without FK, insert `OrderItem.OrderId` nonexistent and observe success.

Add FK, repeat, observe rejection.

Goal: understand why application code alone is insufficient if multiple writers/concurrency exist.

---

# 22. Failure experiment — nullable misuse

Create rows with NULL in field that business actually requires. Follow downstream report/API logic and observe extra invalid-state branches.

Then migrate to explicit invariant after data cleanup.

---

# 23. Review checklist

```text
[ ] Table maps clear business entity/snapshot/read model?
[ ] PK identity semantics clear?
[ ] Business uniqueness enforced?
[ ] FK relationships enforce tenant/ownership where possible?
[ ] NULL has domain meaning?
[ ] decimal/string/time precision intentional?
[ ] CHECK constraints protect simple invariants?
[ ] Query projection bounded?
[ ] Join type matches semantics?
[ ] Pagination deterministic?
[ ] Multi-tenant filter/auth path reviewed?
[ ] Migration safe for existing data + rolling versions?
```

---

# 24. Exit criteria

Bạn hoàn thành khi có thể:

- translate business rules into PK/FK/UNIQUE/CHECK/nullability;
- design surrogate + business key intentionally;
- write JOIN/GROUP BY/CTE/window queries;
- explain normalization and valid denormalization;
- create deterministic pagination;
- configure EF Core precision/length/indexes;
- review migration impact on existing data;
- prove uniqueness/FK constraints under failure/concurrency.

Next:

- [Transactions, Isolation và Concurrency](transactions-isolation-and-concurrency.md)
- [Indexes, Execution Plans và SQL Operations](indexes-execution-plans-and-operations.md)

## Official English Sources

- [Primary and foreign key constraints](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver17)
- [Unique constraints and check constraints](https://learn.microsoft.com/en-us/sql/relational-databases/tables/unique-constraints-and-check-constraints?view=sql-server-ver17)
- [SELECT](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-transact-sql?view=sql-server-ver17)
- [EF Core modeling](https://learn.microsoft.com/en-us/ef/core/modeling/)

## Verification metadata

- Verified: 2026-08-12.
- Target: SQL Server + EF Core 10.
- Status: code-first deep rewrite.
