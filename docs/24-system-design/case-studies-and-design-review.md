# Case Studies & Design Review Workflow

> [← System Design overview](README.md) · [References](references.md)

## Mục tiêu

Chapter này biến các building block thành **một quy trình thiết kế có thể review**.

Không cố thuộc 50 bài System Design. Học một framework rồi áp vào nhiều domain.

```text
Clarify
→ Estimate
→ Model Data
→ Draw Simple Path
→ Find Pressure
→ Scale
→ Design Failures
→ Secure
→ Operate
→ Cost
→ Review Trade-offs
```

---

# 1. Template 45–60 phút cho một design exercise

## 0–5 phút — Clarify

```text
actors
core use cases
out-of-scope
critical flow
```

## 5–10 phút — NFR

```text
DAU / peak RPS
latency
availability
consistency
retention
RTO/RPO
security/compliance
```

## 10–15 phút — Capacity

```text
reads/writes
bandwidth
storage growth
concurrency
backlog
```

## 15–25 phút — Simple architecture

```text
Client
→ API
→ App
→ DB
```

Rồi thêm component theo pressure.

## 25–35 phút — Data + consistency

```text
source of truth
access patterns
transactions
partitioning
cache
replicas
```

## 35–45 phút — Scale + reliability

```text
load balance
queue
backpressure
idempotency
failure isolation
```

## 45–55 phút — Failure/security/operations

```text
timeout
region failure
deploy failure
abuse
authz
observability
DR
```

## 55–60 phút — Trade-offs

```text
what did we intentionally NOT add?
what breaks at 10×?
what is migration trigger?
```

---

# 2. Case Study — URL Shortener

## Requirements

Functional:

```text
create short URL
redirect short code → original URL
optional expiration
analytics eventually
```

NFR:

```text
read-heavy
redirect P95 < 50 ms regional
high availability
short code uniqueness
analytics can lag
```

## Estimate

Assume:

```text
10M redirects/day
100k creates/day
peak = 5× average
```

Average redirect RPS:

```text
10M / 86,400 ≈ 116 RPS
```

Peak:

```text
~580 RPS
```

This is not huge. Do not start with global sharding just because “URL shortener interview” often does.

## Data

```text
short_code PK
original_url
created_at
expires_at
owner_id optional
```

Critical access:

```text
GET by short_code
```

A relational DB with indexed PK is enough initially.

## Architecture v1

```text
Client
→ Load Balancer
→ Stateless API
→ SQL
```

## Scale read path

If redirect traffic grows massively:

```text
Edge/CDN where redirect semantics permit
or
Cache short_code → URL
```

## Short code generation options

### DB-generated integer + base62

```text
1234567 → base62 → short code
```

Pros:

```text
unique/simple
```

Cons:

```text
central allocation pressure / predictable sequence
```

### Random code

```text
crypto/random bytes → base62
```

Need collision handling with unique constraint.

### Distributed ID generator

Useful only at higher write scale; adds complexity.

## Failure

```text
cache down
→ bounded DB fallback

DB down
→ redirects may use cache/stale data if business permits

analytics down
→ redirect should still work
```

## Evolution trigger

Shard only when:

```text
single store capacity
geography
storage volume
or operational isolation
```

actually demands it.

---

# 3. Case Study — Notification System

## Requirements

```text
receive events
resolve subscribers
fan out notifications
email/push/webhook
retry transient failure
unsubscribe respected
```

NFR:

```text
10k source events/s peak
fanout up to 100k recipients/event
95% delivery intent created < 60s
at-least-once acceptable
no duplicate business notification where preventable
provider quota bounded
```

## Core architecture

```text
Event Source
  ↓
Ingestion
  ↓
Durable Queue / Stream
  ↓
Fanout Worker
  ↓
Delivery Intents
  ↓
Provider-specific Queues
  ↓
Email / Push / Webhook Workers
```

## Why separate fanout from delivery?

Because:

```text
subscriber expansion load
≠ provider delivery load
```

Independent queues isolate provider outage.

## Data

Source of truth:

```text
subscription preferences
notification intent/status
idempotency/dedup keys
```

Do not store all state only in queue.

## Backpressure

Email provider quota:

```text
2k requests/s
```

Workers must not scale beyond quota and cause 429 retry storm.

## Failure

Provider unavailable 20 min:

```text
delivery queue grows
source ingestion remains healthy
oldest-message-age alert
recovery drains at bounded rate
```

## Hot fanout

One event has 5M recipients.

Do not create one giant transaction.

Partition fanout:

```text
event + recipient-range/page
```

with resumable progress.

---

# 4. Case Study — Distributed Checkout

Repository Module 18 covers this deeply. Here use it as System Design review.

## Critical invariant

```text
Do not double charge for duplicate checkout.
```

## Requirements

```text
create order
reserve inventory
attempt payment
ship after confirmed payment
recover unknown payment outcome
```

## High-level design

```text
Client
→ Order API
→ Order DB + Outbox
→ Inventory
→ Payment
→ Shipping
```

## Key System Design decisions

### Idempotency

```text
Idempotency-Key UNIQUE
```

### Timeout semantics

```text
payment timeout
≠ payment failed
```

Use:

```text
PENDING_PAYMENT / PAYMENT_UNKNOWN
→ reconciliation
```

### Consistency

No global DB transaction across services.

Use:

```text
local transactions
outbox/inbox
Saga
compensation
reconciliation
```

### Availability

If recommendation service down:

```text
checkout continues
```

If payment provider down:

business policy may:

```text
queue/retry later
or reject new payment attempts
```

but must preserve existing unknown outcomes.

## Capacity

Estimate separately:

```text
checkout RPS
inventory write RPS
payment provider quota
outbox publish rate
reconciliation backlog
```

---

# 5. Case Study — Enterprise AI Assistant

## Requirements

```text
chat with enterprise knowledge
citations required
respect document ACL
optional business tools
conversation history
human approval for risky actions
```

NFR:

```text
100k users
20k concurrent at peak
P95 first useful response < 4s
99.9% service SLO
no cross-tenant document leakage
cost/request budget
model provider outage strategy
```

## Architecture

```text
Client
  ↓
API / Identity
  ↓
AI Orchestrator
  ├─ Retrieval
  │    ↓
  │  Search / Vector Index
  │    ↓
  │  Document ACL metadata
  ├─ Model Provider
  └─ Tools
       ↓
  Application Capabilities
```

## Ingestion path

```text
Document Sources
→ change detection
→ parse/chunk
→ metadata + ACL
→ embeddings
→ index
```

## System Design dimensions unique to AI

```text
context window
tokens/request
provider RPM/TPM
model latency
retrieval latency
embedding/index version
quality/eval SLO
hallucination/grounding tolerance
tool side effects
```

## Capacity

Assume:

```text
100k AI requests/day
4k input tokens
500 output tokens
```

≈

```text
400M input tokens/day
50M output tokens/day
```

Then check provider quota and cost.

## Security

Retrieval flow:

```text
user identity
→ tenant/ACL filter
→ retrieve authorized chunks
→ prompt
```

Never:

```text
retrieve all
→ ask LLM to ignore unauthorized data
```

LLM is not authorization system.

## Failure

Model provider down:

Options:

```text
fail gracefully
fallback model if evaluated
read-only search mode
queue non-interactive jobs
```

Vector index lag:

```text
show freshness indicator
or route critical exact query to source system
```

Tool timeout after side effect:

```text
unknown outcome
→ stable operation ID
→ status/reconciliation
```

AI architecture still follows normal distributed-system rules.

---

# 6. Case Study — News Feed / Timeline

Useful to learn fanout trade-off.

## Fanout on write

When author posts:

```text
post
→ push feed entry to followers
```

Good:

```text
fast read
```

Bad:

```text
celebrity with 50M followers → huge write amplification
```

## Fanout on read

Read time:

```text
fetch followed authors
merge recent posts
```

Good:

```text
cheap write
```

Bad:

```text
expensive read
```

## Hybrid

```text
normal users → fanout on write
celebrity → fanout on read / special path
```

This is a classic example that architecture can depend on **distribution of workload**, not average.

---

# 7. Case Study — File Upload / Media Processing

Requirements:

```text
upload large file
virus/format validation
transcode/process
show progress
serve output globally
```

Bad:

```text
browser → API server → buffer 5 GB → app disk
```

Better:

```text
Client
→ obtain scoped upload URL
→ Object Storage
→ event/queue
→ processing workers
→ output object
→ CDN
```

Benefits:

```text
app server not data plane for huge bytes
processing async
workers scale independently
CDN serves output
```

Security:

```text
short-lived scoped upload credential
size/type limits
quarantine before trust
malware scanning
metadata validation
```

---

# 8. Design Review questions

A Senior/Architect reviewer should ask:

## Requirements

```text
Which user flow is critical?
Which requirement forced this component?
What is explicitly out of scope?
```

## Capacity

```text
Peak RPS?
Read/write ratio?
Data growth?
Concurrency?
Largest payload?
Hot-key distribution?
```

## Data

```text
Where is source of truth?
What must transact together?
How stale may reads be?
What is partition key?
How do we re-partition?
```

## Reliability

```text
What happens when dependency times out?
Which retry is safe?
How do duplicates happen?
How does backlog recover?
What is SLO/RTO/RPO?
```

## Security

```text
What are trust boundaries?
Who authorizes resource access?
What PII is stored/logged?
What can be abused at scale?
```

## Operations

```text
How deploy?
How rollback?
How restore?
What alert fires first?
Who owns the queue/DLQ/cache/shard?
```

## Cost

```text
What costs 10× at 10× traffic?
What is idle DR cost?
What is observability/network cost?
```

---

# 9. Anti-pattern — architecture by buzzword

```text
Kubernetes
Kafka
Redis
Elastic
Microservices
Multi-region
```

without numbers/requirements is not a design.

Every box needs a sentence:

```text
This component exists because ______.
If removed, ______ NFR fails.
Its primary failure mode is ______.
The simpler alternative is ______.
```

---

# 10. Anti-pattern — no migration path

A good design can start simple.

Example:

```text
Phase 1
single SQL database

Trigger
write IOPS > threshold or tenant isolation requirement

Phase 2
read replicas / partition heavy tenants

Trigger
single partition capacity exhausted

Phase 3
sharding
```

Architecture evolution is often safer than deploying final-scale complexity on day one.

---

# 11. Anti-pattern — component uptime instead of journey reliability

All services green but checkout fails because:

```text
payment credentials expired
```

Therefore monitor:

```text
synthetic/user journey
business success rate
correct state transitions
```

not only CPU/process health.

---

# 12. Written design document template

```text
# Problem

# Scope / Out of Scope

# Functional Requirements

# NFR / SLO / RTO / RPO

# Workload Assumptions

# Capacity Estimates

# Data Model / Ownership

# High-level Architecture

# Critical Request/Data Flows

# Consistency / Transaction Semantics

# Scaling Strategy

# Failure Modes / Recovery

# Security / Privacy

# Observability / Operations

# Deployment / Migration

# Cost

# Alternatives

# Decision / Trade-offs

# Open Questions
```

---

# 13. Exit criteria

Bạn hoàn thành Module 24 khi có thể design ít nhất 3 systems và mỗi design có:

```text
measurable requirements
capacity math
data/consistency model
simple baseline architecture
scale trigger
failure analysis
security
observability
DR where relevant
cost/trade-offs
migration path
```

Ít nhất một exercise phải có **failure drill** hoặc load-test evidence, không chỉ diagram.
