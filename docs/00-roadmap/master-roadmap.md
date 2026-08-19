# Master Roadmap — Build Systems → Software Architect → Production AI Engineer

> Nếu trang này dài, đọc [Human Learning Mode](human-learning-mode.md) và [Example-First Learning Path](example-first-learning-path.md) trước. Không cần học tuần tự mọi module.

## Đích nghề nghiệp

Mục tiêu không phải “biết nhiều công nghệ”. Mục tiêu là build được một hệ thống thật:

```text
Business problem
  ↓
Backend
  ↓
Database
  ↓
API
  ↓
Cloud / Infrastructure
  ↓
Deployment / Operations
  ↓
Distributed Systems
  ↓
System Design
  ↓
Software Architecture
  ↓
AI as a production capability
  ↓
Project evidence
```

<div class="key-takeaway" markdown>
<strong>AI Engineer trong thực tế</strong>

Không chỉ là người “biết AI”. Một Production AI Engineer phải đủ software/backend engineering để đưa model, RAG và tools vào **một sản phẩm có auth, data, API, cloud, deployment, observability, evaluation và recovery**.
</div>

## 1. Roadmap trong một hình

```text
FOUNDATIONS
Computer Science
Linux / Git / Networking
        ↓
BACKEND CORE
C# / .NET
Backend Engineering
SQL
API Design
ASP.NET Core
        ↓
PRODUCTION ENGINEERING
Testing / Security / Performance
Redis
Docker
DevOps / IaC
        ↓
PLATFORM
Cloud & Microsoft Azure
Kubernetes
Observability
        ↓
DISTRIBUTED SYSTEMS
Partial Failure
Messaging
Idempotency
Consistency
Microservices
        ↓
DESIGN
System Design
Software Architecture
        ↓
AI SYSTEMS
AI Engineering
RAG
Agents / MCP
AI Security / GenAIOps
        ↓
EVIDENCE
Deploy
Load Test
Failure Drill
Recovery
ADR
Portfolio Project
```

## 2. Priority

| Priority | Nghĩa |
|---|---|
| **P0 — Core** | phải implement, debug, operate và design/review được |
| **P1 — Important** | phải dùng hoặc review tự tin |
| **P2 — Selective** | học đủ để ra quyết định; deep dive khi project cần |
| **P3 — Awareness** | biết use case, risk và nơi tra cứu |

## 3. Module map

| Module | Trọng tâm | Priority | Status |
|---|---|---:|---|
| 00 Roadmap | learning mode, dependency, source policy | P0 | Active |
| 01 Computer Science | complexity, OS, memory, concurrency | P0/P2 | Active |
| 02 Linux/Git/Networking | process, DNS/TCP/TLS/HTTP, troubleshooting | P0 | Active |
| 03 .NET | C#, async, GC, ThreadPool, diagnostics | P0 | Active |
| 04 Backend | request lifecycle, auth, jobs/webhooks | P0 | Active |
| 05 SQL | transactions, indexes, plans, EF→SQL | P0 | Deep content |
| 06 API Design | contracts, OAuth/CORS, OpenAPI, resilience, REST/gRPC/realtime | P0 | Deep content |
| 07 ASP.NET Core | pipeline, resilience, deployment/OTel | P0 | Deep content |
| 08 Testing/Review | tests, contracts, load, code review | P0 | Active |
| 09 Security/DevSecOps | identity, threat model, supply chain | P0/P1 | Active |
| 10 Performance | measurement, profiling, capacity | P0 | Active |
| 11 Redis/Caching | cache, consistency, operations | P1 | Active |
| 12 Docker | builds, runtime, network/storage/security | P0 | Deep content |
| 13 DevOps/IaC | CI/CD, Terraform, safe delivery | P1 | Active |
| **14 Cloud & Azure** | **landing zones, identity/network, compute/data/messaging, reliability/cost, .NET reference architecture** | **P0/P1** | **Expanded 2026-08-19** |
| 15 Kubernetes | reconciliation, workloads/network/storage/security | P1 | Deep content |
| 16 Observability | logs, metrics, traces, OTel, SLI/SLO | P0/P1 | Integrated / dedicated expansion planned |
| 17 Distributed Systems | partial failure, messaging, consistency, reconciliation | P0 | Deep content |
| 18 Microservices Architecture | boundaries, ownership, Saga, migration | P0/P1 | Deep content |
| **19 AI Engineering** | **engineering foundation + models/RAG/tools/eval/production operations** | **P0** | **Expanded system-builder track** |
| 20 RAG | ingestion/retrieval, ACL, deletion/versioning | P0 | foundation integrated in 19 |
| 21 AI Coding Agents | repo context, MCP, safe edit/build/test/PR | P0/P1 | Active |
| 22 AI Security | injection, exfiltration, tool abuse | P0/P1 | Planned expansion |
| 23 GenAIOps/MLOps | prompt/model/index lifecycle, eval gates, rollout | P0/P1/P2 | Planned expansion |
| **24 System Design** | **36 concepts, capacity, traffic, data, async, reliability, cases, production projects** | **P0** | **Expanded 2026-08-19** |
| **25 Software Architecture** | **quality attributes, styles, DDD, modularity, CQRS/EDA, ADR, evolution, review** | **P0** | **Active 2026-08-19** |
| 26 Architecture Docs | C4, ADR, RFC, threat/failure/runbook | P1 | partially integrated in 25 |
| 27 Data Engineering | ingestion, CDC, batch/stream, lineage | P2 | selective/planned |

## 4. Core path — học để build được

```text
C# / .NET
→ Backend
→ SQL
→ API Design
→ ASP.NET Core
→ Testing / Security / Performance
→ Docker
→ DevOps / IaC
→ Cloud & Azure
→ Kubernetes (when needed)
→ Distributed Systems
→ Microservices (when justified)
→ System Design
→ Software Architecture
```

Đây là foundation trước khi tự nhận mình “AI Engineer production”.

## 5. AI Engineer path — AI nằm trên engineering foundation

### Phase A — Build normal systems first

Bạn phải làm được:

```text
API endpoint
DB schema/index/transaction
AuthN/AuthZ
background job
cache
queue
Docker
CI/CD
cloud deployment
logs/metrics/traces
```

### Phase B — Add AI capability

```text
model abstraction
structured output
retrieval / RAG
tool calling
evaluation
AI security
cost / latency
```

### Phase C — Operate AI product

```text
provider timeout/outage
fallback/degradation
prompt/model/index versioning
ACL-aware retrieval
PII-safe telemetry
evaluation gate
rollback
```

→ [Module 19 — AI Engineering](../19-ai-engineering/README.md)

## 6. Azure track — cloud generic → Azure production architecture

```text
Cloud primitives
→ Resource hierarchy / Landing Zones
→ Entra / RBAC / Managed Identity
→ VNet / Private Link / Edge / APIM
→ App Service / Functions / Container Apps / AKS
→ Azure SQL / Cosmos / Blob / Managed Redis
→ Service Bus / Event Grid / Event Hubs
→ Zones / Regions / DR
→ Azure Monitor / Cost / Governance
→ .NET reference architecture
```

→ [Module 14 — Cloud & Azure](../14-cloud/README.md)

## 7. System Design — module expanded

Start:

1. [System Design Overview](../24-system-design/README.md)
2. [36 Concepts & Trade-offs](../24-system-design/concepts-and-tradeoffs.md)
3. [Requirements, NFR & Capacity](../24-system-design/requirements-nfr-and-capacity-estimation.md)
4. [Traffic, LB, CDN & Cache](../24-system-design/traffic-load-balancing-cdn-and-cache.md)
5. [Data, Replication, Partitioning & Consistency](../24-system-design/data-partitioning-replication-and-consistency.md)
6. [Async, Queue, Backpressure & Reliability](../24-system-design/async-queues-backpressure-and-reliability.md)
7. [Availability, Multi-region, DR, Security & Cost](../24-system-design/availability-multiregion-dr-security-and-cost.md)
8. [Case Studies](../24-system-design/case-studies-and-design-review.md)
9. [Production Projects & Evidence](../24-system-design/production-projects-and-evidence.md)

Mental model:

```text
Requirements
+ NFR / SLO
+ Capacity
+ Data / Consistency
+ Failure Model
+ Security / Cost
        ↓
Architecture Options
        ↓
Trade-offs
        ↓
Evidence
```

## 8. Software Architecture — module active

System Design trả lời “system chịu workload/failure thế nào”. Software Architecture đi sâu “code/domain/team/data/deployment boundaries được cấu trúc và evolve thế nào”.

Start:

1. [Software Architecture Overview](../25-software-architecture/README.md)
2. [Quality Attributes, Boundaries & Styles](../25-software-architecture/quality-attributes-boundaries-and-styles.md)
3. [DDD, Modular Monolith & Microservices](../25-software-architecture/ddd-modular-monolith-and-microservices.md)
4. [Clean, Hexagonal & Vertical Slice](../25-software-architecture/clean-hexagonal-and-vertical-slice.md)
5. [Event-Driven, CQRS & Integration](../25-software-architecture/event-driven-cqrs-and-integration.md)
6. [Decisions, Fitness Functions & Evolution](../25-software-architecture/architecture-decisions-evolution-and-fitness-functions.md)
7. [Architecture Review Playbook](../25-software-architecture/architecture-review-playbook.md)

## 9. Distributed Systems vs Microservices vs System Design vs Architecture

### Distributed Systems — mechanics

```text
partial failure
retry/idempotency
messaging
ordering
consistency
outbox/inbox
backpressure
reconciliation
```

### Microservices — autonomy boundaries

```text
service boundaries
data ownership
team ownership
contracts
deployment lifecycle
migration
```

### System Design — workload composition

```text
requirements
capacity
traffic
storage
cache/CDN
partitioning
availability
security
cost
```

### Software Architecture — structure/evolution

```text
quality attributes
coupling
DDD/bounded contexts
architecture styles
sync/async integration
ADRs
fitness functions
migration/evolution
```

## 10. Project spine — evidence thay vì “đã đọc”

| Project | Trọng tâm | Evidence |
|---|---|---|
| 01 Async File Processor | I/O, Channels, cancellation | failure tests + resource measurement |
| 02 Order Management | ASP.NET Core, SQL, API/security | contract + data model + tests |
| 03 Production Backend | Redis, Docker, workers, observability | SLO + load report + runbook |
| 04 Distributed Checkout / Notifications | broker, outbox, dedup, Saga | outage/replay/reconciliation drills |
| 05 Azure .NET Platform | identity/network/data/messaging/deploy | IaC + traces + DR/cost review |
| 06 Enterprise RAG | ingestion, ACL, retrieval, eval | lineage + eval + threat model |
| 07 Production AI Assistant | tools, AuthZ, RAG, eval, cloud | deployed demo + failure/cost/eval evidence |
| 08 High-scale System Design | multi-region/data/failure | capacity + ADR + DR + load/failure drills |

→ [Production System Design Projects](../24-system-design/production-projects-and-evidence.md)

## 11. Definition of Done

Không phải:

```text
read article
watch video
memorize pattern
```

Evidence tốt:

```text
runnable code
schema + constraints
OpenAPI / contract
Docker / IaC
deployment
load test
trace/dashboard
failure injection
restore/reconcile
security test
ADR
cost note
migration trigger
```

## 12. Architect quality gate

Một design/review tốt phải trả lời:

1. Business outcome là gì?
2. Invariants nào không được vi phạm?
3. Source of truth/data ownership ở đâu?
4. Capacity/NFR có số chưa?
5. Failure nào quan trọng nhất?
6. Timeout có nghĩa business gì?
7. Duplicate/retry xử lý thế nào?
8. Security/trust boundary ở đâu?
9. Deploy/rollback/recovery ra sao?
10. Cost driver là gì?
11. Tại sao không dùng solution đơn giản hơn?
12. Khi nào architecture cần evolve?

<div class="architect-note" markdown>
<strong>Đích cuối</strong>

Senior/Architect/AI Engineer giỏi không được đo bằng số pattern nhớ được. Được đo bằng khả năng **build một hệ thống đúng, deploy được, quan sát được, chịu failure, recover được và giải thích trade-off bằng evidence**.
</div>
