# Architecture Fundamentals — SOLID, GRASP, Design Patterns & 12-Factor

> [← Module 25](README.md) · [Architecture Modeling →](architecture-modeling-event-storming-and-c4.md) · [Anti-patterns](architecture-antipatterns-and-refactoring.md) · [Template](system-design-case-study-template.md)

**Mục tiêu:** hiểu vì sao một cấu trúc dễ thay đổi, dễ test và ít lan truyền lỗi; nhận diện pattern theo *forces* thay vì học thuộc tên. Chủ đề này là nền tảng Solution Architect, không chỉ dành cho developer .NET.

## 1. Quality trước pattern

Một module tốt có **high cohesion** (các trách nhiệm cùng một lý do thay đổi) và **low coupling** (thay đổi một module ít làm module khác đổi theo). Đừng lấy số lượng interface/class làm thước đo kiến trúc.

| Principle | Câu hỏi khi review | Ví dụ lỗi |
|---|---|---|
| SRP | Component có một nhóm lý do thay đổi nhất quán? | `OrderService` vừa tính VAT, gửi email, gọi PSP, sinh PDF |
| OCP | Thêm variation có thể mở rộng điểm thích hợp mà không sửa core nhiều nơi? | `switch(provider)` lặp trong 12 endpoint |
| LSP | Implementation thay thế có giữ đúng contract và invariants? | Adapter `Refund` trả success dù PSP chưa xác nhận |
| ISP | Caller có bị buộc phụ thuộc operation nó không dùng? | `IRepository` 40 methods cho mọi aggregate |
| DIP | Business policy phụ thuộc abstraction được sở hữu đúng boundary? | Domain layer import SDK cloud/provider |
| DRY | Có duplication **kiến thức/decision** cần tránh? | Hai nơi định nghĩa quy tắc tính fee khác nhau |
| KISS / YAGNI | Độ phức tạp có bằng chứng business/NFR biện minh? | Bắt đầu với 12 microservices cho 300 RPS |
| Composition over inheritance | Variation có thể compose behaviors và test riêng? | Base class 8 tầng buộc override method nguy hiểm |
| Fail fast | Sai cấu hình/quyền/contract được phát hiện sớm và an toàn? | Missing secret chỉ phát hiện khi charge production |

**GRASP (General Responsibility Assignment Software Patterns):** Information Expert, Creator, Controller, Low Coupling, High Cohesion, Polymorphism, Pure Fabrication, Indirection và Protected Variations. Đây là *heuristics* gán trách nhiệm; không có nghĩa luôn tạo lớp `Controller`, `Manager`, `Factory` cho mọi tính năng.

## 2. Bản đồ Design Patterns theo bài toán

| Pattern | Giải quyết | Khi không dùng / cái giá |
|---|---|---|
| Strategy | Nhiều thuật toán cùng contract: fee/pricing/route | Một nhánh đơn giản và ổn định không cần một lớp cho mỗi if |
| Factory / Abstract Factory | Khởi tạo phức tạp, lifecycle/provider-specific objects | Không thêm factory nếu constructor rõ ràng |
| Adapter | Bọc legacy/external API về contract nội bộ | Không biến adapter thành domain decision engine |
| Decorator | Cross-cutting behaviors có ordering kiểm soát: metrics, validation | Logging/retry decorator không tạo distributed transaction |
| Facade | Đơn giản hóa subsystem phức tạp | Có thể che giấu lỗi/loss of control nếu interface quá tổng quát |
| Mediator | Giảm coupling giữa command handlers/components | Tránh biến thành service locator/bus nội bộ vô kỷ luật |
| Observer / Pub-Sub | Nhiều subscribers quan tâm fact | Event async có duplicate/ordering; observer in-process khác broker |
| Command | Đóng gói intent, audit/retry khi thích hợp | Không mặc định mọi method phải thành command class |
| State | Rules phụ thuộc state machine | Với 2 trạng thái đơn giản, explicit enum + validation đủ |
| Specification | Đóng gói business predicate có thể compose | Translation sang SQL không phải lúc nào tương thích |
| Repository | Boundary cho aggregate persistence, test semantics | Generic repository che mất query plan, transaction boundary |
| Chain of Responsibility | Pipeline validation/authorization | Ordering và short-circuit phải rõ ràng |
| Builder | Object construction nhiều constraints | Không dùng builder cho object với 2 fields |
| Singleton | Một instance *trong process* với lifecycle rõ | Không đảm bảo cluster-wide singleton; tránh shared mutable state |

**Lựa chọn pattern theo yêu cầu:** người dùng cần **một bước thanh toán idempotent**, không phải “phải áp dụng Factory + Mediator + CQRS”. Business invariant, concurrency, DB boundary và provider outcome quyết định thiết kế.

## 3. Worked example (.NET): Strategy + Adapter + composition

Scenario: một application cần tính shipping fee theo policy, không để domain gọi trực tiếp API nhà vận chuyển.

```csharp
public readonly record struct Money(decimal Amount, string Currency);

public interface IShippingPolicy
{
    Money Quote(int grams, string zone);
}

public sealed class DomesticShippingPolicy : IShippingPolicy
{
    public Money Quote(int grams, string zone)
    {
        if (grams <= 0) throw new ArgumentOutOfRangeException(nameof(grams));
        return new Money(grams <= 1000 ? 30000m : 45000m, "VND");
    }
}

public interface ICarrierGateway
{
    Task<string> CreateShipmentAsync(string orderId, CancellationToken ct);
}

public sealed class ShipOrderHandler(IShippingPolicy policy, ICarrierGateway carrier)
{
    public async Task<string> HandleAsync(string orderId, int grams,
        string zone, CancellationToken ct)
    {
        var price = policy.Quote(grams, zone);  // pure domain decision
        // Persist shipment intent with idempotency/outbox before an external side effect.
        // A real implementation also handles provider timeout = UNKNOWN.
        _ = price;
        return await carrier.CreateShipmentAsync(orderId, ct);
    }
}
```

**Đánh giá đúng giới hạn ví dụ:** đoạn trên minh họa dependency direction và composition, **chưa** triển khai idempotency, DB transaction hay outbox. Không đem handler đó chạy production nếu external call có side effect mà chưa có durable intent/reconciliation.

### Test design

1. Unit: gram không hợp lệ, currency, boundary 1.000g; các policy có semantics tương đương tại interface.
2. Contract: carrier adapter convert error/timeout đúng `UNKNOWN` hoặc error type; test không expose internal SDK types.
3. Integration: unique business request, atomic transaction + outbox, retry không tạo shipment thứ hai.
4. Architecture test: domain assembly không reference provider SDK hoặc infrastructure namespace.

## 4. 12-Factor — áp dụng thế nào trong container/Kubernetes?

| Factor | Diễn giải trong backend hiện đại |
|---|---|
| I. Codebase | Một source of truth, versioned; trace commit → artifact |
| II. Dependencies | Pin/version, SBOM và supply-chain controls |
| III. Config | Tách config khỏi code; secrets dùng secret manager, không đặt trong Git |
| IV. Backing services | DB/cache/broker là explicit dependencies có timeout/health contract |
| V. Build, release, run | Build image immutable, promote một artifact qua environments |
| VI. Processes | Stateless workers khi khả thi; state bền vững ở store đã chọn |
| VII. Port binding | Application expose endpoint/port theo deployment contract |
| VIII. Concurrency | Scale theo process/worker và measured resource bottleneck |
| IX. Disposability | Fast startup, graceful shutdown, drain in-flight work |
| X. Dev/prod parity | Giảm khác biệt behavior quan trọng; test thật với DB/broker |
| XI. Logs | Structured events ra pipeline tập trung; redact PII và secrets |
| XII. Admin processes | Migration/backfill dạng one-off, có audit/idempotency/rollback |

12-Factor là **hướng dẫn thiết kế**, không thay thế threat modeling, multi-region DR hay data consistency. Ví dụ stateless app vẫn cần graceful shutdown, durable queue và idempotent retry.

## 5. Trade-off & failure review

| Quyết định | Trade-off | Failure test |
|---|---|---|
| Strategy per payment provider | Thêm indirection nhưng dễ contract test | Provider A `timeout`, provider B returns malformed status |
| Shared utility vs duplicated business policy | Shared module giảm drift nhưng tăng coupling deploy | Thay fee logic cho tenant A không làm hỏng tenant B |
| Decorator retry | Giảm transient failures nhưng có thể nhân đôi side effect | Provider commit thành công rồi mất response |
| Stateless processes | Scale dễ, nhưng cần durable workflow state | Kill pod giữa transaction/callback |
| Generic repository | API đơn giản nhưng dễ che query shape/locking | SQL plan khi có millions rows, N+1 và deadlocks |

## 6. Architecture review deliverables & lab

- [ ] Vẽ module dependency graph; đánh dấu compile-time vs runtime coupling.
- [ ] Refactor một service đang `switch` nhiều PSP sang Adapter + Strategy; chứng minh không đổi business behavior.
- [ ] Viết 6 unit tests và 3 adapter contract tests; test timeout/unknown và concurrency riêng.
- [ ] Ghi ADR “Strategy hay simple conditional?”, có requirement, alternatives, cost, revisit trigger.
- [ ] Đối chiếu 12-Factor với Docker/Kubernetes/CI hiện tại, xác nhận từng factor bằng evidence.

**Exit criteria:** giải thích được pattern theo problem, nêu được khi nào không dùng, chứng minh bằng test/SQL plan/ADR — không chấm điểm bằng số lớp/interface tạo ra.

**Nguồn tham khảo:** [Awesome — Principles](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/software-architecture/principles) · [Awesome — Design Patterns](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/software-architecture/architecture-patterns/design-patterns) · [The Twelve-Factor App](https://12factor.net/) · [Microsoft .NET Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/).
