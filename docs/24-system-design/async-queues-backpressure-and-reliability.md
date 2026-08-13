# Async, Queue, Backpressure & Reliability

> [← System Design overview](README.md) · [References](references.md)

## Hiểu trong 5 phút

Synchronous path:

```text
Client
  ↓ waits
API
  ↓ waits
Service B
  ↓ waits
Database
```

Async path:

```text
Client
  ↓
API
  ↓ accepted durable work
Queue
  ↓
Workers
```

Queue giải quyết **temporal decoupling và load leveling**, nhưng đổi bài toán thành:

```text
duplicate delivery
ordering
backlog
poison messages
retry
replay
idempotency
consumer lag
operational ownership
```

---

# 1. Khi nào synchronous phù hợp

Dùng synchronous khi caller cần result ngay để hoàn thành user flow:

```text
validate credentials
get product
calculate current price
create simple transactional record
```

Benefits:

```text
simple mental model
immediate result
simple debugging
fewer moving parts
```

Costs at scale:

```text
caller holds connection
latency composes
failure propagates
slow dependency consumes concurrency
```

---

# 2. Khi nào async phù hợp

Use when:

```text
work can complete later
burst absorption matters
producer and consumer scale independently
retry/replay is valuable
fanout exists
external dependency is slow/limited
```

Examples:

```text
email delivery
image/video processing
report generation
webhook dispatch
search indexing
analytics ingestion
notification fanout
```

Do not async everything. A queue adds a distributed subsystem.

---

# 3. Queue-Based Load Leveling

Arrival rate can spike above processor capacity.

Without queue:

```text
spike
→ service overload
→ timeout
→ retry
→ more overload
```

With bounded durable queue:

```text
spike
→ queue backlog grows
→ consumers process at controlled pace
```

Microsoft Queue-Based Load Leveling guidance describes exactly this buffering role: smooth intermittent heavy loads so the service is not forced to process every arrival at peak rate.

---

# 4. Queue is not infinite capacity

If:

```text
arrival > service rate
```

for long enough:

```text
backlog → infinity
```

Real system eventually hits:

```text
retention
storage quota
latency SLO
consumer recovery time
cost
```

Need explicit overload policy.

---

# 5. Backpressure

Backpressure means producer cannot create unbounded work faster than downstream can handle.

Mechanisms:

```text
bounded queue
rate limit
concurrency limit
credit/window
pause consumer intake
load shedding
priority queues
```

In-process .NET example:

```csharp
var channel = Channel.CreateBounded<Job>(new BoundedChannelOptions(1_000)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = false,
    SingleWriter = false
});
```

`BoundedChannelFullMode.Wait` makes pressure visible to producers instead of silently growing memory.

---

# 6. Delivery semantics

Common practical categories:

```text
at-most-once
at-least-once
```

“Exactly once” is usually scoped to a particular broker/transaction boundary, not magical end-to-end business side effects.

For at-least-once:

```text
message delivered
consumer side effect commits
consumer crashes before ACK
broker redelivers
```

Therefore consumer must be idempotent or deduplicate.

---

# 7. Inbox / Dedup

Conceptual table:

```sql
CREATE TABLE processed_messages (
    consumer_name nvarchar(100) NOT NULL,
    message_id uniqueidentifier NOT NULL,
    processed_at datetime2 NOT NULL,
    CONSTRAINT PK_processed_messages
        PRIMARY KEY (consumer_name, message_id)
);
```

Consumer transaction:

```text
BEGIN
  insert processed message id
  apply business mutation
COMMIT
```

If duplicate ID conflicts, skip side effect.

---

# 8. Outbox

Problem:

```text
DB commit succeeds
process crashes
publish never happens
```

Outbox:

```text
Business transaction
  ├─ business row
  └─ outbox row
        ↓ same COMMIT
publisher later sends outbox
```

Still possible:

```text
broker accepted
publisher crashes before marking sent
→ publish again
```

So outbox **does not remove consumer idempotency requirement**.

---

# 9. Retry classification

Retry only failures likely transient.

Good candidates:

```text
connection reset
503 temporary unavailable
429 with Retry-After
short network interruption
```

Bad candidates:

```text
validation error
permission denied
business conflict
permanent schema mismatch
confirmed card decline
```

Retrying permanent failure creates load without progress.

---

# 10. Exponential backoff + jitter

Without jitter:

```text
10k clients fail together
wait 1 sec
10k retry together
```

With jitter:

```text
retry attempts spread over time
```

Concept:

```text
delay = min(maxDelay, base × 2^attempt) + random_jitter
```

Always bound attempts/deadline.

---

# 11. Retry amplification

Suppose request chain:

```text
A → B → C
```

Each layer retries 3 times.

Worst-case attempts toward C can approach:

```text
3 × 3 = 9
```

With more layers it grows quickly.

Policy:

```text
retry near the boundary that understands operation semantics
use end-to-end deadline
avoid every layer retrying independently
```

---

# 12. Circuit Breaker

Circuit breaker is useful when dependency failure is persistent enough that repeated calls waste resources.

States conceptually:

```text
Closed
→ failures exceed threshold
Open
→ fail fast
→ after cool-down
Half-open
→ probe
→ Closed or Open
```

It protects the caller/downstream from repeated hopeless work.

It does **not** replace:

```text
timeout
retry policy
fallback
capacity control
```

---

# 13. Bulkhead

One dependency should not consume every resource.

Bad:

```text
all outbound calls share one unbounded concurrency pool
slow reporting provider
→ exhaust connections/threads
→ checkout also fails
```

Bulkhead:

```text
checkout capacity
reporting capacity
notification capacity
```

Failure in one partition of capacity has bounded blast radius.

---

# 14. Timeout vs deadline

Timeout:

```text
this call may take max 2 sec
```

Deadline:

```text
whole operation has 5 sec remaining
```

Deadline propagation prevents downstream from starting 4-second work when only 300 ms remains.

For financial/write operations, timeout may produce **unknown outcome**, not failure.

---

# 15. Unknown outcome

Payment example:

```text
Order → Payment provider
provider commits charge
response lost
timeout at Order
```

Caller knows only:

```text
no response observed
```

not:

```text
payment failed
```

Safe state:

```text
PAYMENT_UNKNOWN / PENDING_PAYMENT
→ reconciliation
→ final state
```

This is why retries require stable operation/payment IDs.

---

# 16. Ordering

Global ordering is expensive and often unnecessary.

Question:

```text
Do we need order globally
or only per entity/customer/order?
```

Per-key ordering:

```text
partition by orderId
→ ordered stream per order
→ parallel across orders
```

Still protect against stale event:

```text
sequence/version
```

---

# 17. Out-of-order protection

Event:

```json
{
  "orderId": "123",
  "version": 42,
  "status": "PAID"
}
```

Consumer state already version 43:

```text
ignore version 42
```

Without version, delayed old messages can regress state.

---

# 18. Poison messages

Message always fails due to data/schema/business bug.

Blind retry:

```text
fail → retry → fail forever
```

Need:

```text
bounded delivery attempts
DLQ/quarantine
alert
inspect/fix
controlled replay
```

DLQ is not a trash bin; it is an operational queue with owner and runbook.

---

# 19. Queue backlog SLO

Not enough to measure queue depth.

Useful metrics:

```text
oldest message age
consumer lag
arrival rate
processing rate
failure rate
retry count
DLQ rate
```

10k messages could mean:

```text
10 seconds of work
```

or:

```text
2 hours of work
```

depending on throughput.

---

# 20. Fanout

One event → many recipients.

Naive:

```text
request thread loops through 100k recipients
```

Better:

```text
source event
→ durable intent
→ fanout partitions
→ delivery workers
```

Need capacity questions:

```text
fanout cardinality
partition key
provider quota
priority
retry isolation
per-recipient idempotency
```

---

# 21. Pub/Sub vs Queue

Queue/work distribution:

```text
one logical piece of work processed by one worker group
```

Pub/Sub:

```text
one event consumed independently by multiple subscriber groups
```

Example:

```text
OrderConfirmed
  ├─ Email subscriber
  ├─ Analytics subscriber
  └─ Fulfillment subscriber
```

Each subscriber needs own retry/dedup semantics.

---

# 22. Event stream

Stream useful when:

```text
ordered append log
multiple consumers
replay
high-volume event history
partitioned processing
```

But “Kafka” should not be default answer if simple queue meets requirements.

Operational cost matters.

---

# 23. Graceful degradation

During failure:

```text
critical checkout works
recommendations disabled
analytics delayed
email queued
```

This is often better than all-or-nothing availability.

Requires identifying feature criticality before incident.

---

# 24. Reliability pattern composition

Patterns work together:

```text
Timeout
  +
Retry for transient faults
  +
Circuit breaker for persistent faults
  +
Bulkhead for isolation
  +
Rate/concurrency limit for capacity
  +
Idempotency for duplicate safety
```

Do not apply every pattern blindly. Each adds policy/state/telemetry.

---

# 25. Failure drill

Notification pipeline:

```text
API → Queue → Worker → Email Provider
```

Run:

1. provider latency 200 ms → 10 sec;
2. provider returns 503 for 5 minutes;
3. observe queue backlog age;
4. ensure worker concurrency stays bounded;
5. ensure retry uses backoff+jitter;
6. confirm no duplicate email after redelivery;
7. restore provider;
8. measure drain time.

Evidence:

```text
arrival/processing rate graph
oldest-message age
retry count
provider call rate
recovery time
```

---

# Exit Criteria

Bạn phải giải thích/thiết kế được:

- sync vs async trade-off;
- queue-based load leveling;
- bounded backpressure;
- at-least-once + idempotent consumer;
- outbox/inbox failure windows;
- retry/backoff/jitter và retry amplification;
- circuit breaker/bulkhead;
- unknown outcome + reconciliation;
- per-key ordering/version protection;
- poison/DLQ/replay;
- queue lag/capacity và graceful degradation.
