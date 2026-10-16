# System Design — ba bài có hướng giải quyết và phản biện

Các số dưới đây là **giả định của bài luyện**, không phải thông số hệ thống production.

## Design 01 — Notification cho 10.000 recipients

<div class="lesson-meta"><span>Async processing</span><span>Multi-tenant</span><span>Failure-first</span></div>

### A. Requirements cần hỏi

- Nguồn sự kiện: announcement duy nhất hay mỗi company tùy chỉnh?
- “Gửi trong 60 giây” nghĩa là queued, SMTP accepted hay delivered?
- Subscriber có unsubscribe trong lúc processing không?
- Có business key chống gửi hai lần, multi-language, retries và quota provider không?

Giả định 10k recipients, muốn **provider accept trong 60s** → cần khoảng 167 recipients/s chưa tính retries. Nếu provider quota 20/s, SLO phải thương lượng lại, không thể giải bằng cách tăng worker.

### B. Component proposal

```text
CMS/Public API ──publish domain event──> SQL Outbox
                                        │
                                    dispatcher
                                        │
                                   Message Broker
                                        │
                               bounded FanOut workers
                                        │
                Tenant policy / Consent / Template version
                                        │
                            durable Delivery records
                                        │
                               provider dispatcher
                                        │
                                SMTP/Email service
                                        │
                       status webhook/reconciliation
```

**Authority:** publication DB stores accepted event/outbox; subscriber DB owns consent; delivery DB tracks attempts/status. “Accepted” ≠ “Delivered”.

### C. Failure matrix

| Failure | Outcome | Recovery |
| --- | --- | --- |
| App crash after SQL commit | Outbox remains pending | Dispatcher scans safely |
| Broker publish accepted but ACK lost | Event can publish twice | Stable MessageId + idempotent inbox |
| Provider timeout | Unknown delivery state | Query provider if possible / reconcile; don't claim failed |
| Subscriber unsubscribes after fan-out | Pending work may exist | Policy validation before actual send |
| DLQ grows | Processing issue, not business delivery | Classify poison messages, rate-limited replay with effect dedupe |

### D. Security/operations

Tenant-scoped audience, least privilege, PII protection, provider quota, log redaction. Observe outbox age, delivery lag, errors, attempts, DLQ and acceptance-to-provider p95.

### E. Alternatives

For small volume, single process plus SQL outbox may be simpler than many microservices. At higher workload isolate workers by scaling/failure domain. Multiple queues per priority vs single queue is a trade-off, not a default.

**Challenge:** If you cannot guarantee exactly-once email delivery, what user-facing status would you show?

## Design 02 — API lịch sử giá / dữ liệu lớn

### A. Requirements

Market instruments, time range, adjustment mode, currency/locale, quote freshness, read security and target response size. Data can be much larger than response — never return unbounded historical ticks.

### B. Design

```text
Web/Mobile → GraphQL or REST gateway → policy/entitlement
                                 │
                          data read service
                            ├─ recent bounded ticks
                            ├─ time-bucket aggregates
                            └─ metadata instruments
                                 │
                          indexed SQL / read model
```

**Data model:** trade history keyed by (`InstrumentId`, `TradeTime`, `TradeId`), rollups keyed by (`InstrumentId`, `TimeBucket`, `Interval`). Index and sort according to actual access pattern; benchmark under representative skew.

### C. Correctness and scaling

- Prefer half-open time ranges and bounded points per response.
- Per-tenant entitlement and realtime/delayed mode must affect cache key and policy; header cannot grant entitlement.
- Market data freshness can permit caching with `asOf`; don't apply relaxed quote consistency to money ledger.
- Use data pagination, aggregation, retention and partition pruning **if workload justifies it**.

### D. Failure tests

A query with very wide time range should be rejected or capped, not take down DB. New deployment must keep API contract and database indexes compatible. Simulate hot instrument skew and provider outage.

**Challenge:** Which metric tells you cache hit ratio improved but users still see stale/wrong data?

## Design 03 — Migration legacy .NET Framework to modern .NET

### A. Business context

Legacy ASP.NET MVC module and modern ASP.NET Core API share SQL schema; desire lower deployment risk and separate scaling.

### B. Design options

1. **No immediate rewrite:** stabilize with contract tests, observability and safe deploys.
2. **Strangler-by-route:** proxy selected endpoints to a new .NET service; keep legacy unchanged for other routes.
3. **Event-driven extraction:** add durable integration boundary once data ownership is clear.

### C. Transition plan

- Inventory routes, DB tables, authorization, stored procedures and undocumented side effects.
- Establish compatibility tests and baseline p95/data correctness.
- Add façade/routing, migrate one feature with parallel observation or shadow read (avoid duplicating writes casually).
- Use expand-contract DB migrations, staged traffic and rollback playbook.
- Remove shared database writes when ownership migration is verified, not on a scheduled deadline alone.

### D. Challenges

- If new service returns correct HTTP but uses another timezone, how detect semantic regression?
- Which migration is irreversible? Data/schema rewrite without rollback/backfill plan.
- When are microservices worth the deployment, tracing and transaction complexity?

## Review rubric

For all three cases: requirements, explicit assumptions, capacity math, data/consistency, failure handling, security, SLO/observability, cost, deployment/rollback, alternatives.

**Đọc sâu:** [System Design case studies](../../24-system-design/case-studies-and-design-review.md) · [Software Architecture](../../25-software-architecture/README.md).
