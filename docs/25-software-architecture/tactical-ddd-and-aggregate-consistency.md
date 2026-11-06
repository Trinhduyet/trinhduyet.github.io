# Tactical DDD — Aggregate, Value Object, Domain Events & Consistency

> [← Module 25](README.md) · [Architecture Modeling](architecture-modeling-event-storming-and-c4.md) · [DDD tổng quan](ddd-modular-monolith-and-microservices.md) · [Event-Driven Integration](event-driven-cqrs-and-integration.md)

**Mục tiêu:** chuyển business rules thành model bảo vệ được invariants khi có concurrency, retries và external dependencies. **DDD không đồng nghĩa đặt code vào các thư mục `Domain/Application/Infrastructure`**.

## 1. Strategic DDD đến Tactical DDD

| Strategic DDD | Tactical DDD | Khác biệt |
|---|---|---|
| Subdomains (core/supporting/generic) | Aggregate/Entity/Value Object | Chọn nơi đầu tư năng lực, sau đó model invariant trong boundary |
| Bounded Context | Domain Service, Repository | Một ngôn ngữ nhất quán bên trong context; không phải deployable service bắt buộc |
| Context Mapping | Anti-Corruption Layer, Integration Event | Hợp đồng tích hợp giữa hai model có nghĩa khác nhau |
| Ubiquitous Language | Domain methods, explicit states | Code phản ánh quy tắc nghiệp vụ, không chỉ database columns |

**Ví dụ:** `Order` của Sales mô tả customer commitment; `FulfillmentOrder` mô tả pick/pack/ship; `Receivable` mô tả khoản phải thu. Ba model không cần cùng một giant entity.

## 2. Entity, Value Object và Aggregate Root

**Entity** có identity theo thời gian. **Value Object** được so sánh theo giá trị, thường immutable; Money phải có currency và phép toán giới hạn currency. **Aggregate** là ranh giới consistency được bảo vệ qua một root.

### Example: Inventory Reservation

Business invariant: `Available = OnHand - Reserved >= 0`; cùng một reservation ID không reserve hai lần. Domain root không gọi Kafka, Redis hay PSP để giữ invariant.

```csharp
public readonly record struct ReservationId(Guid Value);

public sealed class InventoryItem
{
    private readonly HashSet<ReservationId> _reservations = new();

    public Guid Id { get; private set; }
    public int OnHand { get; private set; }
    public int Reserved { get; private set; }
    public long Version { get; private set; }

    public void Reserve(ReservationId requestId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (_reservations.Contains(requestId)) return; // same business request
        if (OnHand - Reserved < quantity)
            throw new InvalidOperationException("Insufficient inventory");

        _reservations.Add(requestId);
        Reserved += quantity;
        Version++;
    }
}
```

**Quan trọng:** code trên chỉ minh họa **domain rule trong memory**. Không chứng minh đúng khi 100 processes cùng ghi SQL. Trong production, phải có unique `(item_id, reservation_id)` và optimistic version/atomic conditional update **trong transaction**. HashSet chỉ tồn tại trên một aggregate instance, không phải cluster-wide dedup.

### Persistence boundary trong SQL (minh họa)

```sql
UPDATE inventory_items
SET reserved = reserved + @quantity,
    version = version + 1
WHERE id = @item_id
  AND version = @expected_version
  AND on_hand - reserved >= @quantity;

-- Must assert affected_rows = 1; conflict means retry or business rejection.
-- Insert unique reservation record + outbox in the SAME DB transaction.
```

**Không** gọi provider API bên trong transaction giữ row lock. Hệ thống khác có thể fail/timeout; xử lý bằng durable workflow với state `UNKNOWN`/reconciliation hoặc compensation được thỏa thuận.

## 3. Chọn Aggregate boundary như thế nào?

1. Liệt kê invariants đòi atomic consistency: `not oversell`, `refund <= captured`, `ledger balanced`.
2. Các object cần thay đổi atomic trong **một local transaction** là candidate cùng aggregate; tránh tạo aggregate khổng lồ vì muốn consistency toàn bộ sản phẩm.
3. Những việc có thể eventual: notification, search index, analytics, cross-bounded-context projections.
4. Với cực nhiều concurrent writes lên một root (hot SKU, account balance), đo lock contention, partition strategy và business alternative.
5. Xác định unique keys + database isolation trước khi chọn event-driven. CQRS/EDA không sửa được invariant đã sai trong write model.

| Chọn boundary | Lợi ích | Cái giá cần đo |
|---|---|---|
| Aggregate nhỏ | Ít contention, dễ thay đổi | Cross-aggregate eventual consistency và process coordination |
| Aggregate lớn | Nhiều rules atomic | Lock/serialization hotspot, load toàn object graph, coupling |
| DB transaction trên nhiều rows trong một service | Bảo vệ invariant cục bộ tốt | Không tự mở rộng qua network và PSP |
| Saga/process manager | Chia workflow qua nhiều owner | Compensation không phải rollback tuyệt đối; failure states phức tạp |

## 4. Domain Service vs Application Service

- **Domain Service:** quy tắc nghiệp vụ thuần không thuộc tự nhiên vào một entity/value object; không phụ thuộc network để quyết định invariant.
- **Application Service:** orchestration: authorize, load aggregate, call domain method, commit, publish event qua outbox; có retries và transaction context.
- **Infrastructure Adapter:** translate SDK/SQL/broker/provider contract; không sửa business policy tùy ý.

Ví dụ `TransferEligibilityPolicy` thuần tính `canTransfer`; `TransferApplicationService` tạo durable transfer và gọi provider adapter; `BankGatewayAdapter` convert provider statuses. Không để một `TransferService` làm tất cả.

## 5. Domain Events, Integration Events & Outbox

```text
Command: ReserveInventory
  → Aggregate validates invariant
  → Local SQL transaction:
        inventory update + reservation record
        + outbox record InventoryReservedV1
  → COMMIT
  → Outbox publisher sends broker event
  → Consumers handle duplicates via inbox/unique key
```

**Domain Event** diễn đạt fact bên trong model, có thể publish in-process. **Integration Event** là public/partner contract: versioned, minimal payload, no internal entity leaks, tenant/security rules và long-lived compatibility. Không dùng `OrderUpdated` chứa toàn bộ Entity Framework aggregate làm event contract.

**Tactical rule:** Event handler in-process chạy trước commit có thể tham gia transaction; external broker send xảy ra sau commit qua outbox. Dù dùng CDC, duplicate delivery/replay vẫn là khả năng có thật.

## 6. Context Mapping & Anti-Corruption Layer (ACL)

Khi external provider gọi `settled`, internal ledger có thể gọi `posted` hoặc `reconciled` với nghĩa khác. ACL cần translate explicit:

| Provider meaning | Internal meaning | Policy |
|---|---|---|
| `HTTP 200 accepted` | `SUBMITTED` | Không tự đánh dấu `SETTLED` |
| `processor timeout` | `UNKNOWN` | Status query/reconciliation; reuse idempotency reference |
| `refund succeeded` | `REFUND_CONFIRMED` | Kiểm tra refund ceiling và posting unique |

ACL không chỉ đổi tên DTO. Nó bảo vệ model nội bộ khỏi semantic drift của đối tác và phải có contract tests.

## 7. Anti-patterns và failure analysis

| Anti-pattern | Vì sao sai | Hướng sửa |
|---|---|---|
| Anemic domain model với CRUD service khổng lồ | Invariant rải ở controller/handler | Behavior ở aggregate/domain service khi có business complexity |
| One aggregate per DB table theo thói quen | Không bảo vệ transaction business boundary | Model invariant và business lifecycle trước |
| One aggregate cho cả payment + inventory + shipping | Cross-service lock giả tưởng | Local invariants + Saga/compensation/reconciliation |
| Domain references cloud SDK, broker client | Vendor lock-in làm domain test phụ thuộc infra | Port/adapter rõ, scope hợp lý |
| Giữ idempotency key chỉ trong Redis | Eviction/restart gây duplicate business effect | Unique DB business key + durable reconciliation |

## 8. Lab và tiêu chí hoàn thành

1. Event Storming một luồng Booking/Wallet/Finance, xác định 3 invariant và 2 bounded contexts.
2. Implement aggregate + value object + domain methods; 10 unit tests cho transitions/validation.
3. Concurrent integration test: 100 attempts reserve cùng một inventory item, tổng accepted không vượt stock.
4. Test replay cùng request ID 10 lần và DB failover/crash sau commit; không double-reserve.
5. Implement outbox + idempotent inbox với DB/broker thật; đảm bảo event có thể rebuild read model.
6. ADR: one larger aggregate vs two aggregates + Saga; ghi measured latency và trade-off.

**Pass:** invariant được bảo vệ bởi dữ liệu bền vững khi concurrent/replayed. *Mock-only unit tests không đủ* để tuyên bố giải quyết race hoặc exactly-once.

**Tham khảo:** [Microsoft — DDD-oriented Microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/) · [Awesome — DDD](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/software-architecture/architecture-patterns/domain-driven-design) · [Distributed Workflow Lab](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/25-solution-architect/distributed-workflow).
