# Event-Driven, CQRS & Integration Architecture

> [← Software Architecture](README.md) · [Distributed Systems](../17-distributed-systems/README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;integration semantics</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Rule</strong>&nbsp;event-driven != eventual chaos</span>
</div>

## 1. Integration starts with business semantics

Không hỏi trước:

```text
REST or Kafka?
```

Hỏi:

```text
Does caller need immediate answer?
Who owns the state?
Can result be eventual?
What happens on timeout?
Can message duplicate?
Does ordering matter?
Can consumer replay?
```

Protocol là implementation choice sau semantics.

## 2. Command vs Event

### Command

Intent yêu cầu một capability làm gì.

```text
AuthorizePayment
ReserveInventory
SendNotification
```

Có target/expectation cụ thể.

### Event

Fact đã xảy ra.

```text
OrderSubmitted
PaymentAuthorized
InventoryReserved
```

Publisher không nên dictate tất cả behavior downstream.

Naming giúp giữ semantics:

```text
CreateInvoiceRequested   ← command-ish
InvoiceCreated           ← fact/event
```

## 3. Domain Event vs Integration Event

Domain event có thể rich/internal:

```text
OrderSubmitted(domain object context)
```

Integration event là external contract:

```json
{
  "eventType": "OrderSubmittedV1",
  "orderId": "O123",
  "occurredAt": "...",
  "total": 120000
}
```

Không serialize nguyên EF/domain entity ra broker.

Reasons:

- internal fields leak;
- consumer coupling;
- schema evolution difficult;
- security/privacy exposure.

## 4. Synchronous request/reply

Useful when user/process needs immediate result.

```text
Checkout API
→ Pricing API
→ result required before response
```

Costs:

```text
temporal coupling
latency accumulation
failure cascade
retry/timeout ambiguity
```

Nếu chain:

```text
API A → B → C → D
```

P99 and availability compound.

## 5. Asynchronous messaging

```text
Producer commits intent
→ broker
→ consumer later
```

Gains:

- decouple availability/time;
- absorb burst;
- fanout;
- retry offline consumers.

Costs:

- eventual state;
- duplicate;
- ordering;
- backlog;
- DLQ/replay;
- tracing harder.

Async không giảm total work. Nó thay **when and how work is coupled**.

## 6. Outbox Pattern

Problem:

```text
DB commit succeeds
message publish fails
```

or reverse.

Outbox:

```text
BEGIN TX
  update business state
  insert outbox record
COMMIT

publisher later sends outbox
```

Guarantee hướng tới:

```text
business commit
→ event intent is durably recorded
```

Nhưng publisher có thể duplicate; consumer still needs idempotency/dedup.

## 7. Inbox / Dedup

Consumer receives message:

```text
MessageId = M123
```

Transaction:

```text
if Inbox contains M123
  skip logical side effect
else
  apply business change
  insert M123 into Inbox
```

Need retention strategy; inbox không tăng vô hạn không kiểm soát.

## 8. Ordering

Global ordering rất đắt và thường unnecessary.

Hỏi:

```text
ordering per what key?
```

Ví dụ:

```text
per OrderId ordering required
across unrelated orders not required
```

Partition/session key có thể map theo aggregate/business key.

Failure:

```text
PaymentCaptured arrives before PaymentAuthorized projection update
```

Consumer phải define behavior khi out-of-order possible.

## 9. Schema evolution

Integration event là public-ish contract giữa teams/services.

### Safer evolution

```json
V1: { orderId, total }
V1 additive: { orderId, total, currency? }
```

Consumers tolerate unknown/new fields.

Breaking rename/remove cần version/transition plan.

### Consumer-driven contracts

Có thể dùng contract tests/schema registry/process để biết producer change có break consumers không.

## 10. CQRS

CQRS tách command/write responsibility khỏi query/read responsibility.

```text
Command
→ validate invariant
→ source-of-truth write model

Query
→ projection optimized for UI/report
```

### Example

Order write model:

```text
Order
OrderLines
Payment state
```

Order dashboard projection:

```text
OrderSummary
- display status
- customer name
- total
- last payment state
- SLA timer
```

Read model có thể denormalized.

## 11. CQRS levels

### Level 0 — methods separated

```text
Commands and Queries separated in code
same DB
```

### Level 1 — separate read projections

Same source of truth, optimized query tables/views.

### Level 2 — async projections

Events update read model eventually.

### Level 3 — separate stores / more distribution

Only if scale/ownership needs justify.

Không cần nhảy thẳng Level 3.

## 12. Event Sourcing

Source of truth là sequence of domain events:

```text
OrderCreated
ItemAdded
OrderSubmitted
PaymentAuthorized
```

Current state = fold/replay events.

Potential gains:

- complete history;
- temporal queries;
- rebuild projections;
- explicit domain transitions.

Costs:

- event versioning;
- replay tooling;
- projection bugs;
- deletion/privacy;
- snapshots;
- unfamiliar debugging/operations.

Event Sourcing và Event-Driven Architecture khác nhau.

```text
EDA can use normal state DB.
Event-sourced system can still have synchronous APIs.
```

## 13. Saga / Process Manager

Multi-service workflow:

```text
Order
→ Inventory
→ Payment
→ Confirmation
```

Process manager stores workflow state:

```text
STARTED
INVENTORY_RESERVED
PAYMENT_PENDING
PAYMENT_UNKNOWN
COMPLETED
FAILED
```

Explicit state makes recovery easier than hidden choreography for long/critical workflows.

## 14. Compensation != rollback

In distributed system, compensation là **new business action**.

```text
Reserve inventory
→ later Release inventory
```

not database rollback of remote commit.

Compensation itself can fail and require retry/reconciliation.

## 15. Exactly-once myth

Even if broker offers features described as exactly-once in specific scope, end-to-end business side effects include:

```text
DB
external API
email provider
payment provider
```

Practical architecture focuses on:

```text
at-least-once transport
+ idempotent/deduplicated business effects
+ reconciliation
```

Scope guarantees precisely.

## 16. Backpressure and load leveling

Queue between producer/consumer:

```text
arrival 1000/s
processing 700/s
```

backlog grows 300/s.

Architecture must define:

```text
max backlog/age
scale trigger
ingress admission
provider quota
load shedding
recovery drain rate
```

## 17. Dead Letter Queue

DLQ holds messages that cannot process under retry policy.

DLQ is not trash bin.

Need:

```text
alert
inspect root cause
fix code/data/config
safe replay
prevent duplicate side effect
retention
```

## 18. Event replay

Replay can rebuild projections or recover downstream processing.

Danger:

```text
old event replay
→ consumer triggers real email/payment again
```

Separate projection rebuild semantics from external side-effect consumers, or use replay-safe modes/idempotency.

## 19. Webhook integration

External provider sends callback:

```text
Provider
→ POST /webhooks/payment
```

Need:

- signature/auth verification;
- dedup event ID;
- quick durable acknowledgment;
- async processing;
- ordering/versioning handling;
- reconcile if webhook missing.

Webhook != source of truth automatically. Provider status API may be authoritative depending on contract.

## 20. API Gateway + Event Broker architecture smell

Using both does not mean architecture is modern.

Bad:

```text
all services sync chain through gateway
+ duplicate all same data as events
```

creates two integration models without clear semantics.

For each interaction, document:

```text
owner
command/query/event
sync/async
contract
failure semantics
consistency expectation
```

## 21. Example — Order → Notification

Requirement:

```text
Order completion must not wait for email provider.
Email can arrive within 30s.
```

Design:

```text
Order transaction
  ├─ status = COMPLETED
  └─ outbox OrderCompleted
        ↓
message broker
        ↓
Notification consumer
        ↓
provider
```

Failure:

```text
provider 500
→ retry bounded
→ DLQ if persistent
```

Order remains completed. Notification has independent status.

This is good decoupling because email outcome is not invariant of order completion.

## 22. Example — Payment

Payment authorization may be critical to order state.

Async design still needs explicit state:

```text
Order PENDING_PAYMENT
→ payment request
→ Payment SUCCEEDED
→ Order CONFIRMED
```

If timeout:

```text
Payment UNKNOWN
```

Do not emit `PaymentFailed` just because local HTTP timed out.

## 23. Integration contract checklist

- [ ] Is this command, query, or event?
- [ ] Who owns schema?
- [ ] Is immediate response required?
- [ ] Duplicate allowed/expected?
- [ ] Ordering scope?
- [ ] Retry policy?
- [ ] Timeout meaning?
- [ ] Version compatibility?
- [ ] PII/security classification?
- [ ] Trace/correlation identity?
- [ ] DLQ/replay behavior?
- [ ] Reconciliation/source of truth?

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Integration architecture tốt không đến từ “dùng event”. Nó đến từ việc làm rõ **ownership, timing, consistency, duplicate, ordering, failure và recovery semantics** cho từng interaction.
</div>
