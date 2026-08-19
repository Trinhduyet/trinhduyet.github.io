# Production System Design Projects & Evidence

> [← System Design](README.md) · [Azure Reference Architecture](../14-cloud/azure-dotnet-reference-architecture.md)

<div class="lesson-meta">
  <span><strong>Goal</strong>&nbsp;chứng minh build được</span>
  <span><strong>Output</strong>&nbsp;code + deploy + metrics + failure evidence</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
</div>

Một architecture portfolio mạnh không chỉ có ảnh C4. Mỗi project nên chứng minh chuỗi:

```text
Requirement
→ Code
→ Database
→ API
→ Infra
→ Deploy
→ Observe
→ Break
→ Recover
→ Explain trade-off
```

## Project 1 — Production Checkout

### Build

```text
ASP.NET Core Checkout API
SQL Server / Azure SQL
Idempotency-Key
Order state machine
Outbox
Message broker
Payment provider fake/sandbox
Reconciliation worker
```

### Failure drills

- duplicate checkout;
- DB timeout;
- broker redelivery;
- payment timeout after remote success;
- worker crash after DB commit;
- queue backlog;
- bad deployment rollback.

### Evidence

```text
integration tests
SQL constraints
trace showing one logical checkout
payment UNKNOWN → reconcile → SUCCEEDED
load test report
ADR for sync vs async payment path
```

## Project 2 — Notification Platform

### Requirements

```text
email + SMS + push
provider quotas
50k/min burst
scheduled delivery
retry transient failures
per-channel status
```

### Build

```text
Notification API
SQL intent/state
Outbox
Topic / subscriptions
Channel workers
DLQ
provider adapters
rate limiting
```

### Evidence

- backlog age dashboard;
- provider 429 test;
- duplicate message test;
- DLQ replay runbook;
- capacity/drain-rate calculation;
- fail one provider and degrade gracefully.

## Project 3 — URL Shortener evolution

### Version 1

```text
API
→ SQL indexed by short_code
```

### Evolve only with evidence

```text
read pressure
→ cache

global latency requirement
→ edge/CDN strategy

single-store capacity pressure
→ partitioning plan
```

### Evidence

- baseline benchmark;
- cache hit ratio;
- stale-cache scenario;
- DB fallback protection;
- explicit migration trigger before shard design.

## Project 4 — Distributed File Processing

### Build

```text
Upload API
→ Blob/Object Storage
→ queue
→ workers
→ metadata DB
→ status API
```

### Failure drills

- same file event twice;
- poison file;
- worker OOM/crash;
- backlog spike;
- storage unavailable;
- cancellation halfway.

### Evidence

```text
content/business idempotency key
bounded concurrency
DLQ
replay
memory profile
throughput vs file size chart
```

## Project 5 — AI Assistant inside a real system

Đây là project nối engineering foundation với AI.

### Không build kiểu

```text
React chat UI
→ model API
```

### Build kiểu

```text
Authenticated User
      ↓
ASP.NET Core API
      ↓
Business authorization
      ↓
AI orchestration
 ├─ model
 ├─ retrieval
 └─ read-only tools
      ↓
SQL / search / business APIs
      ↓
Evaluation + tracing + cost + audit
```

### Required engineering

- API auth/AuthZ;
- SQL/data model;
- provider abstraction;
- structured output;
- tool authorization outside prompt;
- cancellation/timeouts;
- evaluation dataset;
- PII-safe telemetry;
- deployment/IaC;
- rate/cost limits;
- fallback when model unavailable.

### Failure drills

- malformed model output;
- model timeout;
- prompt injection tries unauthorized tool;
- retrieval returns stale/unauthorized data;
- provider outage;
- token/cost spike;
- duplicate tool request.

## Portfolio artifact template

Mỗi project nên publish:

### 1. Problem statement

```text
who
critical flow
scale
latency
availability
consistency
security
cost constraint
```

### 2. Architecture diagram

Vẽ simplest path và evolution stages, không chỉ final architecture.

### 3. Data model

Source of truth, keys, indexes, invariants, state machines.

### 4. API contract

OpenAPI/sample requests + idempotency/error semantics.

### 5. Infrastructure

Docker/IaC/deployment environment + secrets/identity/network model.

### 6. Tests

Unit chỉ là một lớp. Cần:

```text
integration
contract
load
failure/recovery
security
```

### 7. Observability

Dashboard/traces/log examples trả lời user outcome.

### 8. Failure report

```text
Experiment
Expected
Observed
Invariant
Recovery
Evidence
```

### 9. ADR

Ít nhất:

```text
Decision
Context
Options
Trade-offs
Why now
Migration trigger
```

### 10. Demo

Một script/video/README để người review chạy được critical path và failure drill.

## Definition of Done

Project chỉ được gọi là “production-style” nếu anh có thể trả lời:

- [ ] system bảo vệ invariant nào?
- [ ] source of truth ở đâu?
- [ ] duplicate xử lý sao?
- [ ] timeout external side effect xử lý sao?
- [ ] peak/load math là gì?
- [ ] bottleneck hiện tại ở đâu?
- [ ] deploy rollback sao?
- [ ] secret/identity quản lý sao?
- [ ] SLO/user metrics ở đâu?
- [ ] backup/restore/reconciliation ra sao?
- [ ] chi phí chính đến từ đâu?
- [ ] khi nào architecture cần evolve?

<div class="key-takeaway" markdown>
<strong>Portfolio signal</strong>

“Biết System Design” mạnh nhất khi repository cho thấy **anh đã build, deploy, làm hỏng, đo và recover** một hệ thống — không phải khi README có nhiều logo công nghệ.
</div>
