# Master Roadmap — .NET Backend → AI-enabled Software Architect

> Nếu trang này dài, đọc [Cách đọc tài liệu](how-to-read.md) trước. Không cần học tuần tự mọi module.

## Đích nghề nghiệp

```text
Business problem
  ↓
Requirements + NFR
  ↓
Simple design
  ↓
Code + tests
  ↓
Failure / Security / Performance
  ↓
Operations + Evidence
  ↓
Architecture decision
```

Đích đến là **AI-enabled Software Architect** có thể thiết kế và review hệ thống .NET + distributed systems + microservices + AI bằng evidence, không bằng tên pattern.

---

# 1. Roadmap trong một hình

![Roadmap từ foundations đến Software / AI Architecture](../assets/diagrams/roadmap-core-and-ai.svg)

---

# 2. Priority

| Priority | Nghĩa |
| --- | --- |
| **P0 — Core** | phải implement, debug, operate và design/review được |
| **P1 — Important** | phải dùng hoặc review tự tin |
| **P2 — Selective** | học đủ để ra quyết định; deep dive khi project cần |
| **P3 — Awareness** | biết use case, risk và nơi tra cứu |

---

# 3. Module map

| Module | Trọng tâm | Priority | Status |
| --- | --- | --- | --- |
| 00 Roadmap | dependency, baseline, source policy | P0 | v1 |
| 01 Computer Science | complexity, OS, memory, concurrency | P0/P2 | Content v1 |
| 02 Linux/Git/Networking | process, DNS/TCP/TLS/HTTP, troubleshooting | P0 | Content v1 |
| 03 .NET | C#, async, GC, ThreadPool, hosting, diagnostics | P0 | Content v1 |
| 04 Backend | request lifecycle, auth, pagination, jobs/webhooks | P0 | Content v1 |
| **05 SQL** | transactions, indexes, plans, EF→SQL | **P0** | **Code-first deep rewrite v1: 4 guides** |
| **06 API Design** | HTTP contracts, security/OAuth, evolution/OpenAPI, traffic/resilience, REST/GraphQL/gRPC, gateway/realtime | **P0** | **Code-first deep rewrite v1: 6 guides + references; 25-topic coverage** |
| **07 ASP.NET Core** | pipeline, resilience, deployment/OTel | **P0** | **Code-first deep rewrite v1: 3 guides** |
| 08 Testing/Review | test boundaries, integration/load, review | P0 | Structure v1; deep rewrite pending |
| 09 Security/DevSecOps | identity, API/app security, supply chain | P0/P1 | Structure v1; deep rewrite pending |
| 10 Performance | measurement, profiling, load/capacity | P0 | Structure v1; deep rewrite pending |
| 11 Redis/Caching | cache, data structures, HA/failure | P1 | Structure v1; deep rewrite pending |
| **12 Docker** | build, runtime/network/storage, Compose/security | **P0** | **Code-first deep rewrite v1: 3 guides** |
| 13 DevOps/IaC | CI/CD, artifacts, Terraform, safe delivery | P1 | Structure v1; deep rewrite pending |
| 14 Cloud | compute/network/data/identity/regions/DR/cost | P1 | Structure v1; deep rewrite pending |
| **15 Kubernetes** | reconciliation, workloads/network/storage/security | **P1** | **Code-first deep rewrite v1: 3 guides** |
| 16 Observability | logs, metrics, traces, OTel, SLI/SLO | P0/P1 | Foundation integrated; dedicated module planned |
| **17 Distributed Systems** | partial failure, messaging, consistency | **P0** | **Code-first v1: 4 guides + references** |
| **18 Microservices Architecture** | boundaries, data ownership, contracts, Saga, deployment/migration | **P0/P1** | **Code-first v1: 4 guides + references** |
| **19 AI Engineering** | provider abstraction, structured output, tools, eval | **P0** | **Code-first v1: 3 guides** |
| 20 RAG | full ingestion/retrieval lifecycle, ACL, deletion/versioning | P0 | Foundation covered in 19; dedicated module planned |
| 21 Business AI Agents/MCP | tools, workflow, state, HITL, authorization | P0 | Planned — advanced-first |
| **21A AI Coding Agents** | repo context, MCP, build/test/PR safety | **P0/P1** | **Code-first v1: 3 guides** |
| 22 AI Security | injection, exfiltration, tool abuse, red teaming | P0/P1 | Planned |
| 23 GenAIOps/MLOps | prompt/model/index lifecycle, eval gates, rollout | P0/P1/P2 | Planned |
| 24 System Design | requirements, capacity, distributed + AI design | P0 | Planned |
| 25 Software Architecture | boundaries, styles, evolution, migration | P0 | Planned |
| 26 Architecture Docs | C4, ADR, RFC, threat/failure/runbook | P1 | Planned |
| 27 Data Engineering | ingestion, CDC, batch/stream, lineage | P2 | Planned / selective |

`Code-first v1` nghĩa tài liệu đã có code/config, production reasoning và failure experiment. Learner evidence vẫn phải tự chạy.

---

# 4. Core path nên học ngay

```text
C# / .NET
→ Backend
→ SQL
→ API Design
→ ASP.NET Core
→ Docker
→ Kubernetes
→ Distributed Systems
→ Microservices Architecture
→ System Design
→ Software Architecture
```

AI chạy song song sau khi backend/distributed foundation đủ chắc:

```text
AI Engineering
→ RAG
→ Agents / MCP
→ AI Security
→ GenAIOps
```

---

# 5. Microservices Architecture — module mới

Bắt đầu:

1. [Microservices Architecture Overview](../18-microservices-architecture/README.md)
2. [Service Boundaries, Data Ownership & Contracts](../18-microservices-architecture/service-boundaries-data-ownership-and-contracts.md)
3. [Checkout Saga: Unknown Outcome & Reconciliation](../18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md)
4. [Communication, Gateway, Discovery & Deployment](../18-microservices-architecture/communication-gateway-discovery-and-deployment.md)
5. [Testing, Observability & Migration](../18-microservices-architecture/testing-observability-and-migration.md)

Mental model:

```text
Business capability
→ bounded context
→ service boundary
→ owned data
→ versioned contract
→ independent deployment
→ SLO/runbook/team ownership
```

Checkout case study đi xuyên suốt:

```text
Idempotency
→ Inventory Reservation
→ Payment Attempt
→ timeout = UNKNOWN
→ PENDING_PAYMENT
→ reconciliation
→ Saga compensation
→ Outbox / Inbox / Dedup
→ contract/versioning
→ tracing / failure drills
```

---

# 6. Distributed Systems vs Microservices

Không đồng nhất hai khái niệm.

**Distributed Systems**:

```text
partial failure
retry/idempotency
messaging
ordering
consistency
outbox/inbox
backpressure
```

**Microservices Architecture** thêm:

```text
service boundaries
data sovereignty
team ownership
API/event contracts
API gateway/BFF
service discovery
independent deployment
migration from modular monolith
```

Microservices sử dụng kiến thức Distributed Systems; vì vậy Module 17 là prerequisite của Module 18.

---

# 7. Architecture progression

Không nhảy từ CRUD thẳng sang microservices.

```text
Monolith
→ Well-structured Monolith
→ Modular Monolith
→ Event-driven integration
→ Distributed Systems competence
→ Microservices when justified
```

Microservices không phải default “production architecture”. Nếu team/domain/scale chưa tạo pressure, Modular Monolith thường đơn giản hơn và rẻ hơn để vận hành.

---

# 8. Code/readability quality gate

Một chapter P0/P1 implementation-heavy phải có, khi phù hợp:

```text
Hiểu trong 5 phút
+ mental model
+ runnable code/config
+ production example
+ failure experiment
+ command/test để verify
+ security/performance/observability
+ architect trade-offs
```

Generic prose có thể copy sang công nghệ khác = **outline**, không phải deep content.

Default reading order:

```text
Problem
→ Code
→ Mental Model
→ Failure
→ Internals
→ Operations
→ Architecture
```

---

# 9. Project spine

| Project | Trọng tâm | Evidence |
| --- | --- | --- |
| 01 Async File Processor | I/O, Channels, cancellation, concurrency | failure tests + resource measurement |
| 02 Order Management | ASP.NET Core, SQL, EF, API/security | contract + data model + tests |
| 03 Production Backend | Redis, Docker, workers, observability, CI | SLO + load report + runbook |
| **04 Distributed Checkout / Notifications** | broker, outbox, dedup, Saga, microservice boundaries | ADR + outage/replay/reconciliation drills |
| 05 Enterprise RAG | ingestion, ACL, retrieval, deletion, eval | lineage + eval + threat model |
| 06 Production Agent Platform | tools, MCP, workflow, approval, audit | trust boundary + red-team gates |
| 07 High-scale AI System Design | multi-region/provider | C4 + capacity + cost + DR + migration |

---

# 10. Definition of Done

Evidence tốt:

```text
code chạy
unit/integration/contract test
execution plan
trace
load/eval report
failure experiment
PR review
ADR/runbook
```

“Đã đọc xong” không phải evidence.

## Verification metadata

- Verified: 2026-08-13
- API Design deep rewrite covers the 25-item contract/security/reliability/protocol checklist in Module 06.
- Microservices sources: Microsoft Learn / Azure Architecture Center
- Technology baseline: [technology-baseline.md](technology-baseline.md)
- Source policy: [source-policy.md](source-policy.md)
