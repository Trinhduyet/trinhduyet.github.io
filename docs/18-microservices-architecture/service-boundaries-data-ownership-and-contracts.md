# Service Boundaries, Data Ownership và Contracts

> [← Microservices Architecture](README.md)

## Hiểu trong 5 phút

Microservice boundary tốt không bắt đầu từ table hay controller.

```text
Business capability
   ↓
Bounded context
   ↓
Owned model + rules
   ↓
Owned data
   ↓
External contract
```

Ví dụ e-commerce:

```text
Ordering
Payment
Inventory
Shipping
```

khác với tách theo technical layer:

```text
UserService
DatabaseService
ValidationService
EmailHelperService
```

Boundary tốt tối ưu **cohesion bên trong** và giảm **coupling bên ngoài**.

---

# 1. Bounded Context ≠ class/package

Một bounded context chứa ngôn ngữ và model riêng của business capability.

Cùng từ `Status` có thể có meaning khác nhau:

```text
Order Status
Payment Status
Shipment Status
```

Không nên tạo một `CommonStatus` dùng chung toàn hệ thống chỉ để reuse code.

---

# 2. Data ownership

Bad:

```text
Order Service ─┐
Payment Service├──> SharedDatabase.dbo.Orders
Inventory ─────┘
```

Một schema dùng chung tạo coupling:

```text
schema change
→ coordinated release
→ hidden business dependency
→ service autonomy giảm
```

Preferred:

```text
Order Service     → Order DB/schema
Payment Service   → Payment DB/schema
Inventory Service → Inventory DB/schema
```

Service khác truy cập thông qua:

```text
API
message/event
read model explicitly designed for integration
```

không query trực tiếp table nội bộ.

---

# 3. Database-per-service không nhất thiết là một physical server cho mỗi service

Learning rule:

```text
logical ownership matters first
```

Bạn có thể dùng cùng SQL Server instance trong giai đoạn đầu nhưng:

```text
different database/schema ownership
no cross-service table access
separate migration ownership
separate credentials where practical
```

Physical isolation tăng khi NFR/security/scale yêu cầu.

---

# 4. Không share EF entity/domain model

Bad shared package:

```csharp
// Shared.Domain.dll
public sealed class Payment
{
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string ProviderPayload { get; set; } = null!;
}
```

Order Service reference trực tiếp package này sẽ phụ thuộc internal model của Payment Service.

Preferred integration contract:

```csharp
public sealed record PaymentResultContract(
    string PaymentId,
    string Status,
    string Currency,
    decimal Amount);
```

Sau đó Order Service map sang local model:

```csharp
public enum CheckoutPaymentState
{
    Pending,
    Succeeded,
    Failed,
    Unknown
}

public static CheckoutPaymentState MapStatus(string status) =>
    status switch
    {
        "SUCCEEDED" => CheckoutPaymentState.Succeeded,
        "FAILED" => CheckoutPaymentState.Failed,
        "PENDING" => CheckoutPaymentState.Pending,
        _ => CheckoutPaymentState.Unknown
    };
```

Quan trọng không phải `string` hay `enum`; quan trọng là **không import internal type của service khác**.

---

# 5. Contract-owned DTO

HTTP example:

```http
GET /payments/pay-123
```

Response:

```json
{
  "paymentId": "pay-123",
  "status": "SUCCEEDED",
  "amount": 125.50,
  "currency": "USD"
}
```

Order Service cần tolerate field mới:

```json
{
  "paymentId": "pay-123",
  "status": "SUCCEEDED",
  "amount": 125.50,
  "currency": "USD",
  "providerReference": "stripe_abc"
}
```

Additive field không nên làm consumer cũ fail nếu contract/serializer policy cho phép.

---

# 6. Event contract != entity

Bad:

```csharp
await bus.PublishAsync(orderEntity);
```

Preferred:

```csharp
public sealed record OrderCreatedV1(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency);
```

Event contract nên chỉ chứa information consumer cần và có identity/version rõ.

---

# 7. Anti-Corruption Layer

Khi external service/provider có model khác domain của bạn:

```text
External Contract
       ↓
Adapter / Anti-Corruption Layer
       ↓
Local Domain Model
```

Ví dụ provider trả:

```json
{
  "state": "captured"
}
```

Local app không cần lan truyền string `captured` khắp domain.

```csharp
public PaymentOutcome MapProviderStatus(string state) =>
    state switch
    {
        "captured" => PaymentOutcome.Succeeded,
        "declined" => PaymentOutcome.Failed,
        "processing" => PaymentOutcome.Pending,
        _ => PaymentOutcome.Unknown
    };
```

---

# 8. Cross-service query problem

Monolith:

```sql
SELECT ...
FROM Orders o
JOIN Payments p ON ...
JOIN Shipments s ON ...;
```

Sau khi tách DB ownership, query này không còn là local join đơn giản.

Options:

```text
API composition
read model/projection
CQRS read store
analytics/data platform
client aggregation
```

Không giải bằng cách cấp read access trực tiếp vào DB của mọi service vì như vậy boundary bị xuyên thủng.

---

# 9. API composition example

Aggregator:

```csharp
public sealed record OrderDetails(
    OrderDto Order,
    PaymentSummary? Payment,
    ShipmentSummary? Shipment);

public async Task<OrderDetails> GetOrderDetailsAsync(
    Guid orderId,
    CancellationToken cancellationToken)
{
    var orderTask = orderClient.GetAsync(orderId, cancellationToken);
    var paymentTask = paymentClient.GetByOrderAsync(orderId, cancellationToken);
    var shipmentTask = shippingClient.GetByOrderAsync(orderId, cancellationToken);

    await Task.WhenAll(orderTask, paymentTask, shipmentTask);

    return new(
        await orderTask,
        await paymentTask,
        await shipmentTask);
}
```

Trade-off:

```text
fresh data
but runtime fan-out + partial failure + latency
```

Read model có thể tốt hơn nếu query rất hot và chấp nhận eventual consistency.

---

# 10. Version compatibility

Independent deployment chỉ thực tế nếu contract evolution hỗ trợ overlapping versions.

Bad rollout:

```text
Payment v2 deploys
→ immediately removes field required by Order v1
→ Order breaks
```

Safer evolution:

```text
1. add new field/endpoint/event version
2. deploy producer compatible with old + new consumers
3. migrate consumers
4. observe adoption
5. remove old contract later
```

Đây là **expand → migrate → contract**.

---

# 11. Consumer-driven contract thinking

Consumer assumptions cần explicit test.

Example test:

```csharp
[Fact]
public async Task Payment_response_should_keep_required_checkout_fields()
{
    var response = await client.GetAsync("/payments/pay-123");
    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.True(json.TryGetProperty("paymentId", out _));
    Assert.True(json.TryGetProperty("status", out _));
    Assert.True(json.TryGetProperty("amount", out _));
    Assert.True(json.TryGetProperty("currency", out _));
}
```

Production contract tooling có thể mạnh hơn, nhưng mental model là:

```text
producer change
must not silently violate consumer assumptions
```

---

# 12. Failure drill — shared DB coupling

Setup:

```text
Order Service reads Payment table directly.
```

Change Payment schema:

```sql
EXEC sp_rename
    'dbo.Payments.Status',
    'ProviderStatus',
    'COLUMN';
```

Expected:

```text
Order fails despite no Order deployment.
```

Sau đó replace DB access bằng versioned Payment API/contract và repeat deployment reasoning.

---

# 13. Failure drill — unknown enum value

Payment Service adds:

```text
REQUIRES_REVIEW
```

Old consumer using strict deserialization may fail.

Desired local mapping:

```text
unknown external state
→ local Unknown/PendingReview
→ alert/telemetry
→ no destructive compensation
```

Không convert unknown state thành `FAILED` chỉ để code switch exhaustive.

---

# 14. Architect checklist

```text
[ ] Boundary maps to business capability?
[ ] Service owns data/schema/migrations?
[ ] Other services avoid direct DB access?
[ ] Internal entity/domain package not shared?
[ ] External contracts explicit/versioned?
[ ] Unknown enum/status forward-compatible?
[ ] Cross-service read strategy explicit?
[ ] API composition latency/failure bounded?
[ ] Contract rollout supports overlapping versions?
[ ] Team/service ownership documented?
```

---

# 15. Exit criteria

Bạn hoàn thành khi có thể:

- derive service boundary from domain capability;
- explain bounded context vs package/service name;
- enforce logical data sovereignty;
- reject shared domain entity as integration contract;
- design local mapping/anti-corruption layer;
- handle forward-compatible status values;
- choose API composition vs read model;
- design expand/migrate/contract rollout;
- write a contract test for consumer assumptions.

## Official English Sources

- [Microservices architecture style](https://learn.microsoft.com/en-us/azure/architecture/microservices/)
- [.NET Microservices architecture](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/microservices-architecture)
- [Data sovereignty per microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice)

## Verification metadata

- Verified: 2026-08-13.
- Status: code-first v1.
