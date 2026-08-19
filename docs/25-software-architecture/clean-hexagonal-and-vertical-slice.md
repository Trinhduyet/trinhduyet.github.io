# Clean, Hexagonal & Vertical Slice Architecture

> [← Software Architecture](README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;application structure</span>
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Question</strong>&nbsp;business logic phụ thuộc vào framework tới đâu?</span>
</div>

## 1. Ba pattern này giải quyết những problem khác nhau

Đừng coi:

```text
Clean Architecture
Hexagonal Architecture
Vertical Slice Architecture
```

là ba “framework” cạnh tranh trực tiếp.

Chúng nhấn mạnh khác nhau:

```text
Clean / Onion
→ dependency direction / policy isolation

Hexagonal / Ports & Adapters
→ boundary giữa application và external world

Vertical Slice
→ organize code theo feature/use-case
```

Một system có thể kết hợp chúng vừa đủ.

## 2. Clean Architecture mental model

```text
Framework / UI / DB / Provider
          ↓
Application boundary
          ↓
Business policies / domain
```

Dependency source code hướng vào policy/core, thay vì core phụ thuộc trực tiếp DB/framework.

Ví dụ:

```csharp
public interface IPaymentGateway
{
    Task<PaymentResult> AuthorizeAsync(
        PaymentRequest request,
        CancellationToken cancellationToken);
}
```

Application biết `IPaymentGateway`, không biết Stripe/Adyen/provider SDK cụ thể.

### Good fit

- business rules đáng test độc lập;
- nhiều external adapters;
- framework/provider có thể thay;
- domain/app policy cần bảo vệ.

### Overengineering smell

CRUD endpoint đơn giản nhưng có:

```text
Controller
→ IRequest
→ Handler
→ Service
→ DomainService
→ Repository
→ UnitOfWork
→ Mapper
```

mà mỗi layer chỉ forward một dòng.

Architecture nên tăng **signal**, không tăng ceremony.

## 3. Hexagonal / Ports & Adapters

Mental model:

```text
             HTTP Adapter
                 ↓
            Input Port
                 ↓
          Application Core
            ↑         ↑
      DB Port         Payment Port
        ↑                  ↑
 SQL Adapter       Provider Adapter
```

### Port

Contract mà core/application cần.

### Adapter

Implementation kết nối external technology/system.

Ví dụ:

```text
IOrderRepository      ← SQL adapter
IPaymentGateway       ← external provider adapter
IClock                ← system clock adapter
INotificationSender   ← email/SMS adapter
```

## 4. Anti-Corruption Layer

External API có model/status riêng:

```json
{
  "code": "CAPTURED_07",
  "gateway_state": "DONE"
}
```

Không để `CAPTURED_07` lan khắp domain.

Adapter mapping:

```text
Provider result
→ ACL/adapter
→ PaymentOutcome.Succeeded
```

Nếu provider đổi schema, impact nằm ở boundary.

## 5. Vertical Slice Architecture

Thay vì technical folders:

```text
Controllers/
Services/
Repositories/
DTOs/
```

organize theo use case:

```text
Orders/
  Create/
    Endpoint.cs
    Command.cs
    Validator.cs
    Handler.cs
  GetById/
    Endpoint.cs
    Query.cs
    Handler.cs
  Cancel/
    Endpoint.cs
    Command.cs
    Handler.cs
```

### Benefit

Change “Cancel Order” thường nằm gần nhau hơn, giảm horizontal scattering.

### Important nuance

Vertical Slice không có nghĩa copy business rule vào mỗi handler.

Shared domain invariant vẫn nên ở appropriate domain object/policy.

## 6. Feature slice vs domain module

Một modular monolith có thể:

```text
Orders Module
├── CreateOrder slice
├── CancelOrder slice
├── GetOrder slice
└── Domain model
```

Đây là combination hữu ích:

```text
module = business ownership boundary
slice  = use-case organization inside module
```

## 7. CQRS và Vertical Slice

Vertical Slice thường đi tốt với command/query separation vì mỗi use case tự có read/write shape.

```text
CreateOrder Command
→ enforce invariant
→ write normalized transaction model

GetOrder Query
→ return optimized projection DTO
```

Không cần full event sourcing/two databases để hưởng benefit này.

## 8. Repository — khi nào có value?

Repository hữu ích nếu nó represent aggregate persistence abstraction/business collection.

Bad generic abstraction:

```csharp
IRepository<T>
{
    GetById();
    GetAll();
    Add();
    Update();
    Delete();
}
```

cho mọi table, rồi mất query-specific power của EF/SQL.

Better khi domain semantics justify:

```csharp
public interface IOrderRepository
{
    Task<Order?> GetForUpdateAsync(OrderId id, CancellationToken ct);
    void Add(Order order);
}
```

Query/read path có thể dùng direct projection/query service nếu rõ hơn.

## 9. Dependency inversion — đừng tạo interface cho mọi class

Dependency Inversion không nói:

```text
every service must have IService interface
```

Interface có value khi:

- crossing architecture boundary;
- multiple implementations;
- external side-effect abstraction;
- stable contract needed for testing/module separation.

Internal pure class không cần interface chỉ để “follow SOLID”.

## 10. Domain purity — pragmatic approach

Có hai extreme:

### Framework everywhere

Domain entity phụ thuộc EF/HTTP/provider types khắp nơi.

### Purity obsession

Tạo mapping layer/copy types quá nhiều, complexity lớn hơn business value.

Pragmatic rule:

```text
Keep business invariants independent from volatile external details.
Accept framework coupling where it is cheap, stable and local.
```

## 11. Example — Payment capability

### Application port

```csharp
public interface IPaymentProvider
{
    Task<ProviderPaymentStatus> GetStatusAsync(
        string businessKey,
        CancellationToken cancellationToken);
}
```

### Use case

```csharp
public sealed class ReconcilePayment(
    IPaymentProvider provider,
    IPaymentAttemptRepository repository)
{
    public async Task ExecuteAsync(Guid paymentAttemptId, CancellationToken ct)
    {
        var attempt = await repository.GetAsync(paymentAttemptId, ct)
            ?? throw new InvalidOperationException("Payment attempt not found");

        if (!attempt.IsUnknown)
            return;

        var remote = await provider.GetStatusAsync(attempt.BusinessKey, ct);
        attempt.Reconcile(remote);
    }
}
```

### Provider adapter

```text
HTTP SDK
→ provider-specific DTO
→ mapper
→ ProviderPaymentStatus
```

Business state machine không biết HTTP status/provider enum details.

## 12. Testing strategy

### Domain tests

Test invariants/state transitions without infrastructure.

### Application/use-case tests

Fake ports where appropriate.

### Adapter integration tests

Real SQL/container/sandbox/provider contract.

### End-to-end

Critical flows only; expensive/brittle if everything relies on E2E.

Architecture giúp đặt test ở boundary đúng, không phải “mock everything”.

## 13. Common mistakes

### Clean architecture dependency inversion without business complexity

Hundreds of abstractions for basic CRUD.

### Vertical slice duplicate logic

Mỗi handler tự tính BuyingPower/Payment rule → divergence.

### Hexagonal giant port

```text
IInfrastructureService with 50 methods
```

boundary quá broad.

### DTO explosion

Mapping types nhiều hơn logic, maintenance cost không justified.

### Domain isolated from useful database semantics

Refuse transactions/constraints/indexes because “domain must be pure” → correctness/performance tệ.

## 14. Decision guide

| Problem | Useful approach |
|---|---|
| volatile external integrations | ports/adapters |
| rich business rules | domain-centric core |
| feature changes scattered across layers | vertical slices |
| independent business modules | modular monolith boundaries |
| mostly CRUD admin app | simple layered/vertical approach may suffice |
| multiple read/write models | selective CQRS |

## 15. Review checklist

- [ ] external details có được giữ ở adapter/boundary hợp lý?
- [ ] business invariants nằm nơi dễ tìm/test?
- [ ] interface có architecture value hay chỉ ceremony?
- [ ] feature change chạm bao nhiêu folder/layer?
- [ ] read/query path có bị ép qua repository abstraction vô ích?
- [ ] provider DTO có leak vào domain không?
- [ ] domain purity có đang tạo mapping tax quá lớn?
- [ ] tests chạy ở đúng boundary?

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Clean/Hexagonal giúp quản **dependency direction và external boundaries**; Vertical Slice giúp quản **change locality theo use case**. Dùng vừa đủ để code dễ thay đổi và dễ chứng minh correctness hơn.
</div>
