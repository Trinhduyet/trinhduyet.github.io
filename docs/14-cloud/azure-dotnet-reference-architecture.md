# Azure .NET Checkout Reference Architecture

> [← Reliability & Operations](azure-reliability-observability-governance-and-cost.md) · [Cloud & Azure](README.md)

<div class="lesson-meta">
  <span><strong>Scenario</strong>&nbsp;Checkout</span>
  <span><strong>Stack</strong>&nbsp;.NET + Azure</span>
  <span><strong>Goal</strong>&nbsp;business correctness trước cloud complexity</span>
</div>

## 1. Requirements

Giả sử hệ thống checkout có:

```text
Peak traffic           2,000 checkout/min
P95 API latency        < 500 ms before payment completion
Order durability       no accepted order may disappear
Payment correctness    no accidental double charge
Availability target    99.95% monthly for checkout intake
RTO                     30 min
RPO                      5 min max for regional disaster scenario
```

Payment provider là external system có timeout/unknown outcome.

## 2. Business invariants trước Azure services

```text
One CheckoutRequest business key
→ at most one logical Order

One PaymentAttempt business key
→ provider side effect must be idempotent/reconcilable

Order accepted
→ durable state exists

Message redelivery
→ must not duplicate business transition
```

Nếu các invariant chưa rõ, thêm Service Bus/AKS/multi-region chỉ làm system phức tạp hơn.

## 3. Simple architecture first

```text
Client
  ↓ HTTPS
Azure Front Door + WAF
  ↓
API Management
  ↓
.NET Checkout API
  ↓
Azure SQL
```

Đây có thể đã đủ cho giai đoạn đầu nếu payment được gọi synchronous và workload/SLO phù hợp.

Sau khi requirement reliability/decoupling justify async:

```text
Client
  ↓
Front Door
  ↓
API Management
  ↓
Checkout API
  ↓ local transaction
Azure SQL
  ├─ Orders
  ├─ PaymentAttempts
  └─ Outbox
       ↓ publisher
Azure Service Bus
       ↓
Payment Worker
       ↓
External Payment Provider
       ↓ timeout / callback / query
Reconciliation Worker
       ↓
Azure SQL state update

Notification intent
       ↓
Service Bus
       ↓
Notification Worker
```

## 4. Compute choice

Một reference implementation có thể chọn:

```text
Checkout API      → Azure App Service or Container Apps
Payment Worker    → Container Apps / Functions / App Service worker pattern
Reconcile Worker  → Functions timer / Container Apps job / worker service
```

Không cần AKS nếu không có Kubernetes-specific requirement.

Nếu organization đã có mature AKS platform và workload cần K8s ecosystem, decision có thể khác. Architecture phải ghi **context**, không biến service choice thành universal rule.

## 5. Data model

```sql
CREATE TABLE Orders (
    Id uniqueidentifier PRIMARY KEY,
    CheckoutKey nvarchar(100) NOT NULL UNIQUE,
    Status varchar(30) NOT NULL,
    TotalAmount decimal(18,2) NOT NULL,
    CreatedAt datetime2 NOT NULL
);

CREATE TABLE PaymentAttempts (
    Id uniqueidentifier PRIMARY KEY,
    OrderId uniqueidentifier NOT NULL,
    BusinessKey nvarchar(100) NOT NULL UNIQUE,
    ProviderReference nvarchar(100) NULL,
    Status varchar(30) NOT NULL,
    UpdatedAt datetime2 NOT NULL
);

CREATE TABLE OutboxMessages (
    Id uniqueidentifier PRIMARY KEY,
    Type nvarchar(200) NOT NULL,
    Payload nvarchar(max) NOT NULL,
    PublishedAt datetime2 NULL,
    CreatedAt datetime2 NOT NULL
);
```

Reference states:

```text
Order
PENDING_PAYMENT
→ PAID
→ FULFILLING
→ COMPLETED

PaymentAttempt
PENDING
→ SUCCEEDED
→ FAILED
→ UNKNOWN
```

`UNKNOWN` là state business quan trọng khi local service không biết provider đã tạo side effect hay chưa.

## 6. API idempotency

Client gửi:

```http
POST /checkouts
Idempotency-Key: checkout-user123-cart987-v4
```

Application transaction:

```text
lookup / insert by CheckoutKey
→ if existing return existing logical result
→ otherwise create order once
```

Không implement idempotency bằng memory cache duy nhất vì process restart/multi-instance sẽ phá guarantee.

## 7. Outbox + Service Bus

Trong cùng SQL transaction:

```text
INSERT Order
INSERT Outbox PaymentRequested
COMMIT
```

Publisher có thể gửi duplicate nếu crash ở ranh giới:

```text
send message succeeds
→ process crashes before PublishedAt update
→ restart sends again
```

Do đó consumer phải deduplicate theo business/message identity.

## 8. Payment timeout

Timeline:

```text
T0 worker calls provider Charge(IdempotencyKey=P123)
T1 provider commits charge
T2 response packet lost
T3 worker timeout
```

Nếu worker làm:

```text
timeout → FAILED → retry with new provider key
```

có thể double charge.

Safer flow:

```text
timeout
→ PaymentAttempt = UNKNOWN
→ query provider by same business/idempotency key
   or await verified webhook
→ reconcile
→ SUCCEEDED / FAILED
```

Azure resilience policy không được biến network timeout thành business failure.

## 9. Managed Identity / secrets

```text
Checkout API Managed Identity
  ├─ access Azure SQL as configured
  ├─ publish Service Bus if needed
  └─ read required Key Vault secret only for external provider
```

Không lưu SQL password/Service Bus connection string nếu workload/service hỗ trợ identity-based access phù hợp.

External payment API key có thể vẫn cần secret management; rotate và audit riêng.

## 10. Networking

Possible traffic design:

```text
Internet
 ↓
Front Door public endpoint
 ↓ controlled origin
API ingress
 ↓
App integration network
 ├─ Private Endpoint → SQL
 ├─ Private Endpoint → Service Bus where required
 └─ controlled outbound → Payment Provider
```

Private networking chỉ giảm exposure; app/provider authorization vẫn bắt buộc.

## 11. Cache

Đừng cache Order source of truth chỉ để giảm vài ms nếu read volume chưa justify.

Redis phù hợp hơn với derived data như:

```text
product/catalog projection
rate counters
short-lived lookup
```

Nếu cache unavailable:

```text
fallback must be bounded
```

Tránh toàn fleet đập trực tiếp DB tạo thundering herd.

## 12. Observability

Business metrics:

```text
checkout_accepted_total
payment_unknown_total
payment_reconciled_total
duplicate_checkout_blocked_total
order_completion_latency
```

System metrics:

```text
API P95/P99
SQL latency/connections
Service Bus queue depth
oldest message age
DLQ count
provider dependency latency/error
```

Trace cần nối:

```text
HTTP request
→ OrderId
→ OutboxMessageId
→ ServiceBus MessageId
→ PaymentAttemptId
→ ProviderReference
```

PII/card/token data không được log chỉ để correlation.

## 13. Deployment

Reference pipeline:

```text
commit
→ build/test
→ security/static checks
→ container/package artifact
→ deploy staging
→ integration/smoke
→ progressive production rollout
→ SLO/error gate
→ rollback if needed
```

Database change dùng expand/contract khi rolling versions coexist.

## 14. DR

Nếu RTO/RPO thật sự yêu cầu regional recovery:

```text
Primary region
├─ API compute
├─ SQL primary/replication setup appropriate to design
├─ messaging
└─ monitoring

Secondary region
├─ deployable/warm compute
├─ replicated/recoverable data path
└─ validated dependencies
```

DR checklist:

```text
Can DNS/edge route?
Can app start with dependencies?
Is data usable?
What messages are missing/duplicated?
How to reconcile payment during outage window?
Can provider callbacks reach correct region?
```

## 15. Capacity math

Peak:

```text
2,000 checkout/min ≈ 33.3 requests/sec
```

Nếu each accepted checkout emits 2 async messages:

```text
~66 messages/sec average at peak intake
```

Giả sử incident làm worker offline 20 phút:

```text
66 × 60 × 20 ≈ 79,200 messages backlog
```

Recovery design phải tính **drain rate**:

Nếu consumers xử lý 200 msg/s:

```text
net drain ≈ 200 - 66 = 134 msg/s
79,200 / 134 ≈ 591 sec ≈ 9.9 min
```

Đây mới là capacity reasoning; “Service Bus auto scales” không thay được throughput math và external-provider quota.

## 16. Failure review

| Scenario | Local state | Correct response |
|---|---|---|
| duplicate POST | existing checkout | return logical existing result |
| DB commit fail | no accepted durable order | fail request safely |
| outbox duplicate send | duplicate message possible | consumer dedup |
| payment timeout | UNKNOWN | reconcile |
| provider 429 | pending | bounded backoff respecting quota |
| worker crash | message redelivered | idempotent consumer |
| cache outage | slower/degraded | bounded fallback |
| region outage | according to DR target | failover/restore + reconcile |

## 17. Project evidence

Để chứng minh “build được”, learner phải có:

- runnable .NET API;
- SQL migrations + constraints;
- idempotency test;
- outbox integration test;
- duplicate Service Bus message test;
- provider timeout simulation;
- reconciliation job;
- deployment IaC/pipeline;
- tracing/dashboard;
- load test + capacity note;
- restore/failure drill;
- ADR giải thích vì sao chọn compute/data/messaging services.

<div class="key-takeaway" markdown>
<strong>Điều project này chứng minh</strong>

Bạn không chỉ “biết Azure”. Bạn biết đưa **backend + database + API + cloud/infra + deployment + failure recovery** thành một hệ thống hoạt động và có evidence.
</div>
