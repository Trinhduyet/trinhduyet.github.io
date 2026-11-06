# Messaging Platforms — Kafka, RabbitMQ, NATS, AsyncAPI & Workflow Orchestration

> [← Module 17](README.md) · [Outbox & Inbox](messaging-outbox-inbox-and-dedup.md) · [Consistency / Saga](consistency-ordering-saga-and-backpressure.md) · [Advanced Architecture](../25-software-architecture/advanced-actor-event-sourcing-and-service-mesh.md)

**Mục tiêu:** chọn message broker / event streaming dựa vào workload, ordering, replay, durability và operations; xác định event contracts và workflow recovery. Đây là bài **so sánh quyết định kiến trúc**, không phải bảng xếp hạng sản phẩm.

## 1. Event, Command, Message, Stream

- **Command:** intent muốn một owner thực hiện (`CapturePayment`), có thể reject; thường một logical owner.
- **Event:** fact đã xảy ra (`PaymentCapturedV1`), có thể có nhiều subscribers.
- **Message:** envelope truyền qua broker; có thể chứa command, event hoặc notification.
- **Stream:** ordered log/sequence theo phạm vi partition/subject/stream; không tự bảo đảm toàn cục.

**Lỗi thiết kế thường gặp:** gửi event `OrderCreated` trước khi SQL commit; coi “message delivered” là “side effect hoàn tất”; dùng HTTP retry + broker retry mà không có business idempotency.

## 2. Broker selection matrix — phụ thuộc requirements

| Platform / family | Workload thường phù hợp | Đánh đổi cần review |
|---|---|---|
| **RabbitMQ** | Work queues, routing, commands, task processing | Queue/exchange design, ACK/prefetch, quorum policies, DLQ, capacity |
| **Apache Kafka** | Append-only event log, multiple consumer groups, retention/replay, streams | Partitions/order per key, consumer lag, schema evolution, operations |
| **NATS / JetStream** | Lightweight messaging, request/reply, stream persistence khi dùng JetStream | Core NATS semantics khác JetStream; retention/consumer ACK/config |
| **Azure Service Bus** | Enterprise queue/topic, sessions, scheduled messages, DLQ | Vendor boundary, quotas, session/concurrency/config, cost |
| **Azure Event Hubs** | High-throughput ingestion/event streams | Stream retention/partitioning và consumer checkpoints khác command queue |
| **DB-backed queue/outbox** | Small workload, strong local transaction coupling | Polling contention, storage cleanup, latency, DB pressure |

Không ép dùng Kafka cho mọi event, hoặc RabbitMQ cho mọi task. **Chọn theo message semantics, throughput, ordering, replay window, multi-tenant isolation, team skills và recovery SLO.**

## 3. Delivery semantics và transaction boundaries

```text
Application transaction
  → update business row + insert outbox event
  → COMMIT
  → relay publishes with stable event ID
  → broker may redeliver
  → consumer inbox / unique key + business update in local transaction
  → ACK only after durable effect
```

**At-most-once:** có thể mất event nếu failure đúng thời điểm. **At-least-once:** có thể nhận duplicate; phải idempotent. **Exactly-once scoped processing** ở một số broker không có nghĩa external email/PSP/SQL side effects exactly once end-to-end.

**Ordering:** Kafka duy trì ordering trong partition; queue/session ordering phụ thuộc broker và retry/redelivery/concurrent workers. Nếu user events cần ordering, chọn stable partition/session key và thiết kế version check trong consumer.

## 4. Event contracts, AsyncAPI và schema evolution

Event contract không phải serialize trực tiếp database ORM entity. Cần xác định:

| Contract field | Ý nghĩa |
|---|---|
| `eventId` | Stable unique ID để inbox dedup/replay |
| `eventType` + `schemaVersion` | Compatibility và subscriber routing |
| `aggregateId` + `aggregateVersion` | Ordering/concurrency check per aggregate |
| `tenantId` / owner scope | Isolation và data minimization |
| `occurredAt` | Business time, khác processedAt |
| `correlationId` / `causationId` | Trace flow, root cause, audit |
| `payload` | Minimal domain fact, không leak secrets/PII |

Ví dụ event (mang tính minh họa, không phải AsyncAPI document hoàn chỉnh):

```json
{
  "eventId": "evt_99",
  "eventType": "PaymentCaptured",
  "schemaVersion": 1,
  "aggregateId": "pay_123",
  "aggregateVersion": 4,
  "tenantId": "merchant_1",
  "correlationId": "request_456",
  "occurredAt": "2026-11-06T08:30:00Z",
  "payload": { "amountMinor": 125000, "currency": "VND" }
}
```

**AsyncAPI** dùng để mô tả event-driven APIs (channels, messages, operations, security, servers). Định nghĩa contract trong repo, validate compatibility và generate docs khi cần; đừng tự nhận event is backward compatible chỉ vì JSON parser không crash.

**Schema evolution rules:** thêm optional field có default rõ; thay đổi meaning/unit hoặc rename property là breaking nếu consumer phụ thuộc; event version migration/upcasters và deprecation window phải được thỏa thuận. Kiểm tra evolution với contract tests cho producer/consumer cũ và mới.

## 5. Choreography vs Orchestration (Saga/workflow)

| Option | Phù hợp | Rủi ro |
|---|---|---|
| Choreography: services tự phản ứng events | Flow ngắn, loose coupling, ownership rõ | Business flow ẩn trong nhiều consumers, khó theo dõi timeout |
| Orchestration: coordinator/process manager | Complex multi-step booking/payment/refund | Coordinator ownership, persistence, migration của workflow state |
| Durable workflow engine | Long-running workflows, retries, timers, recovery | Thêm vendor/framework semantics và operational burden |

**Ví dụ:** Booking cần hold inventory → charge PSP → confirm hoặc release. Nếu PSP trả timeout, coordinator phải lưu `UNKNOWN`, không mặc định chạy compensation “release” trước khi biết provider đã charge chưa.

## 6. .NET ecosystem — chọn abstraction có lý do

| Tool/framework | Điểm cần đánh giá trước POC |
|---|---|
| MassTransit | Broker transports, retry/redelivery, saga state, transactional outbox setup |
| NServiceBus | Endpoint persistence, recoverability, saga, licensing/operations |
| Wolverine | Message handlers, persistence, broker integration, local vs remote durability |
| CAP | Transactional messaging/integration, supported persistence and broker semantics |
| Dapr pub/sub | Sidecar abstraction, component configuration, delivery guarantees per component |
| Raw client SDK | Control chi tiết nhưng team tự viết retries/metrics/contracts và runbooks |

**Đừng so sánh chỉ theo syntax `PublishAsync`:** cần test duplicate, crash-before-ack, poison message, ordering, data migration, recovery và upgrades theo version/transport thực tế.

## 7. Failure matrix và SLO

| Fault | Strategy | Verification |
|---|---|---|
| DB commit, relay crash | Durable outbox, retry | Restart relay; event eventually delivered |
| Broker accepts, ACK lost | Stable event ID, publisher retry | Duplicate broker delivery nhưng business effect đúng 1 |
| Consumer dead after SQL commit | Inbox + idempotent handler | Replay 10 lần không duplicate business row |
| Poison event | Bounded retry, DLQ/quarantine, manual reprocess | No infinite loop, redacted payload logs |
| Partition hot-key | Capacity/partition strategy, bounded concurrency | p95 lag at realistic skew, not only average |
| Schema break | Contract compatibility gates | Old consumer handles new event or blocked release |
| Broker outage | Backpressure, quota, admission control | Queue age/time-to-drain and graceful degradation |

**SLIs:** `outbox_oldest_age`, `publish_retries`, `consumer_lag/age`, `dead_letter_count`, `replay_success`, `message_to_effect_latency`, duplicate business effects, broker disk/partition availability.

## 8. Lab và ADR

1. Cài RabbitMQ hoặc Kafka trong Docker Compose, DB thật, 2 services .NET; trace `request → DB + outbox → broker → inbox → downstream`.
2. Test 100 concurrent duplicates; crash relay/consumer tại các commit/ack boundaries.
3. Định nghĩa event schema và AsyncAPI contract; chạy version compatibility tests.
4. Inject broker outage 5 phút; đo queue backlog, drain time, retry storm prevention.
5. Đưa `UNKNOWN` external provider vào workflow có reconciliation, không tự coi retry là safe.
6. ADR so sánh Kafka vs RabbitMQ vs DB outbox cho workload cụ thể; nêu tiêu chí đổi broker.

**Exit criteria:** chạy được bằng chứng failure/replay/contract trên broker thật; không tuyên bố production exactly-once hay broker equivalence khi chưa test.

**Nguồn:** [Kafka Documentation](https://kafka.apache.org/documentation/) · [RabbitMQ Docs](https://www.rabbitmq.com/docs) · [NATS Docs](https://docs.nats.io/) · [AsyncAPI Specification](https://www.asyncapi.com/docs/reference/specification/latest) · [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview).
