# DDD, Modular Monolith & Microservices

> [← Software Architecture](README.md) · [Microservices module](../18-microservices-architecture/README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;domain boundaries</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Rule</strong>&nbsp;bounded context != microservice automatically</span>
</div>

## 1. DDD giải quyết vấn đề gì?

Domain-Driven Design hữu ích khi business rules/language/ownership phức tạp tới mức data-table-centric design bắt đầu gây confusion.

Không phải:

```text
DDD = Entity + Repository + DomainService folders
```

Mà là:

```text
Business language
→ domain model
→ boundaries where that model is consistent
→ explicit integration between models
```

## 2. Ubiquitous Language

Team kỹ thuật và domain experts dùng cùng từ cho cùng concept.

Ví dụ commerce:

```text
Cart
Order
Payment Attempt
Inventory Reservation
Fulfillment
Refund
```

Nếu `OrderStatus = Done` nhưng mỗi team hiểu `Done` khác nhau, architecture đã có semantic coupling/ambiguity.

## 3. Bounded Context

Bounded Context là ranh giới nơi một model/language có nghĩa nhất quán.

Ví dụ:

```text
Sales Context
Order = customer purchase commitment

Fulfillment Context
Order/Shipment = work to pick/pack/ship

Accounting Context
Invoice/Receivable = financial obligation
```

Một entity name có thể giống nhưng model khác nhau hợp lý.

## 4. Entity vs Value Object

### Entity

Có identity xuyên thời gian.

```text
OrderId = O123
```

### Value Object

Định nghĩa bằng value, thường immutable.

```text
Money(Amount=100, Currency="USD")
Address(...)
```

Value Object giúp gom invariants gần data:

```csharp
public readonly record struct Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Currency mismatch");

        return new Money(Amount + other.Amount, Currency);
    }
}
```

## 5. Aggregate — transaction consistency boundary

Aggregate không phải “object graph lớn”.

Mental model:

```text
Command
→ Aggregate Root
→ enforce invariants
→ one transactional consistency boundary
```

Ví dụ Order aggregate có thể enforce:

```text
cannot add line after order submitted
cannot capture more than authorized amount in local model
```

Nhưng đừng nhét Inventory, Payment Provider, Shipment vào một giant aggregate nếu chúng là independent systems/boundaries.

## 6. Domain Event vs Integration Event

### Domain Event

Fact có ý nghĩa trong domain boundary:

```text
OrderSubmitted
```

Có thể dùng nội bộ trong same process/transactional workflow.

### Integration Event

Contract publish ra boundary khác:

```text
OrderSubmittedV1
{
  OrderId,
  CustomerId,
  TotalAmount
}
```

Integration event cần versioning/schema/consumer compatibility và không nên leak full internal entity.

## 7. Context Mapping

Các contexts tương tác theo relationship khác nhau:

```text
Customer/Supplier
Conformist
Anti-Corruption Layer
Published Language
Shared Kernel (high coupling)
```

Anti-Corruption Layer hữu ích khi external model không nên tràn vào core.

Ví dụ payment provider:

```text
Provider SDK response
      ↓ adapter
PaymentGatewayPort
      ↓
Domain PaymentResult
```

Core không nên biết provider-specific status code ở mọi nơi.

## 8. Modular Monolith trước khi Microservices

Một architecture rất mạnh cho many systems:

```text
CheckoutHost
├── Orders
│   ├── Domain
│   ├── Application
│   ├── Infrastructure
│   └── Public contract
├── Payments
├── Inventory
└── Notifications
```

Rules:

```text
module owns tables/model
other module cannot reference internals
integration through explicit API/event interface
architecture tests enforce dependencies
```

### Why useful

Bạn nhận được:

- clear domain boundaries;
- local transactions/debugging;
- one deployment;
- lower operational complexity;
- future extraction path.

## 9. Khi nào split microservice?

Không split vì file count lớn.

Signals tốt hơn:

### Independent team ownership

Payment team release/control khác Orders.

### Independent scaling

Image processing 100× CPU profile của rest app.

### Fault isolation

Notification provider outage không được ảnh hưởng checkout core.

### Compliance/security boundary

Payment/customer-data processing có control/credential/on-call riêng.

### Deployment cadence

Module cần release nhiều lần/ngày độc lập mà monolith coordination gây bottleneck thật.

### Technology specialization

Workload cần runtime/storage đặc thù có reason rõ.

## 10. Split tạo ra distributed transaction problem

Before:

```text
BEGIN TX
Order + Reservation
COMMIT
```

After service split:

```text
Order Service
→ Inventory Service
→ Payment Service
```

Không còn một local transaction across services.

Giờ phải thiết kế:

```text
state machine
idempotency
partial failure
compensation
reconciliation
```

Đó là cost thật của autonomy.

## 11. Saga

Saga quản lý multi-step business process qua local transactions + messages/commands/events/compensations.

Example:

```text
Create Order
→ Reserve Inventory
→ Authorize Payment
→ Confirm Order
```

Failure payment:

```text
Payment FAILED
→ release reservation
→ mark Order PAYMENT_FAILED
```

Timeout payment:

```text
Payment UNKNOWN
→ do NOT blindly compensate/retry
→ reconcile provider outcome
```

Saga không giải quyết unknown external side effect nếu integration semantics chưa thiết kế.

## 12. Orchestration vs Choreography

### Orchestration

Central process manager knows workflow:

```text
Checkout Saga
├→ Inventory
├→ Payment
└→ Order
```

+ explicit workflow state
- orchestrator coupling/responsibility

### Choreography

Services react to events:

```text
OrderCreated
→ Inventory reacts
→ InventoryReserved
→ Payment reacts
```

+ decentralized
- emergent workflow khó thấy/debug nếu quá dài

Long business workflow thường benefit từ explicit process state hơn “event chain vô hình”.

## 13. Shared Database — nuanced view

Shared DB không phải luôn forbidden trong transition/monolith, nhưng phải hiểu coupling.

### Bad distributed shared DB

```text
Orders Service writes Orders table
Payment Service also updates Orders.Status directly
```

Ownership mơ hồ, release coupling cao.

### Transitional strategy

Có thể giữ same physical SQL instance nhưng logical schema ownership:

```text
orders.* owned by Orders
payments.* owned by Payments
```

No cross-write. Sau đó split physical DB khi requirement justified.

## 14. Extracting a service — Strangler approach

Không rewrite big-bang.

```text
Identify capability
→ create explicit module boundary
→ stop cross-writes
→ define contract
→ move data ownership
→ route new traffic
→ observe
→ retire old path
```

Extraction dễ hơn nếu modular monolith đã có clean seam.

## 15. Example — Investor platform

Một unified UI:

```text
Investor App
├── Stocks
├── Bonds
├── Funds
└── Cash
```

Không có nghĩa backend nên có:

```text
UniversalInvestmentEntity
```

Domains có lifecycle khác:

```text
Stock Order → Execution → Trade → Settlement
Bond → Coupon / Yield / Maturity
Fund → Subscription / NAV / Allocation / Redemption
Cash → Ledger / Reservation / Settlement
```

UI unified ≠ domain unified.

## 16. Architecture tests

Ví dụ .NET dependency rule conceptually:

```text
Orders.Domain must not reference Payments.Infrastructure
Payments cannot write Orders persistence types
Public contracts live in explicit contract assembly/module
```

Có thể implement bằng architecture testing library hoặc custom reflection tests.

## 17. Common DDD mistakes

### Folder-driven DDD

```text
Entities/
Repositories/
Services/
```

nhưng business boundary không rõ.

### Aggregate too large

Load whole object graph cho mọi command → contention/performance issue.

### Repository per table

Repository trở thành CRUD wrapper, aggregate semantics mất.

### Events everywhere

Mọi setter phát event → noise/complexity.

### One bounded context = one service immediately

Tạo distributed system trước khi cần.

## 18. Decision checklist

- [ ] language/model conflicts identified;
- [ ] bounded contexts based on domain meaning/ownership;
- [ ] aggregates sized by invariants/transaction needs;
- [ ] integration contracts do not leak internals;
- [ ] module data ownership explicit;
- [ ] modular monolith considered before network split;
- [ ] service extraction has measurable trigger;
- [ ] distributed failure/idempotency/reconciliation accounted;
- [ ] architecture tests enforce key boundaries;
- [ ] migration path avoids big-bang rewrite.

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

DDD giúp tìm **semantic boundary**. Modular Monolith giúp enforce boundary với low operational cost. Microservices chỉ thêm runtime/deployment separation khi business/organization/workload evidence đủ mạnh để trả giá distributed complexity.
</div>
