# Master Roadmap — Build Systems → Design Systems → Production AI

> Nếu trang này dài, dùng [Role-based Learning Paths](role-based-learning-paths.md) trước. Đây là landscape/dependency map, không phải syllabus bắt buộc học tuần tự.

## Đích nghề nghiệp

```text
Business problem
  ↓
working software
  ↓
correct data/API boundaries
  ↓
secure + measurable production behavior
  ↓
repeatable delivery/platform
  ↓
failure/recovery reasoning
  ↓
system/architecture decisions
  ↓
AI as an optional production capability
  ↓
evidence
```

Mục tiêu không phải “biết nhiều công nghệ”. Mục tiêu là **build, operate, debug và review một hệ thống thật**.

---

# 1. Roadmap trong một hình

```text
FOUNDATIONS
01 Computer Science
02 Linux / Git / Networking
        ↓
BACKEND CORE
03 C# / .NET Runtime
04 Backend Engineering
05 SQL
06 API Design
07 ASP.NET Core
        ↓
PRODUCTION ENGINEERING
08 Testing / Code Review
09 Security / DevSecOps
10 Performance
11 Redis / Caching when justified
12 Docker
        ↓
DELIVERY / PLATFORM
13 DevOps / IaC
14 Azure
15 Kubernetes when justified
        ↓
DISTRIBUTED
17 Distributed Systems
18 Microservices when justified
        ↓
DESIGN
24 System Design
25 Software Architecture
        ↓
AI SYSTEMS
19 AI Engineering
21 AI Coding Agents
        ↓
EVIDENCE
runnable code
load/failure tests
telemetry
recovery
ADR
cost
```

Module numbers reflect repository history, **not mandatory sequence**.

---

# 2. Priority model

| Priority | Nghĩa |
|---|---|
| **P0 — Core** | phải implement/debug/operate hoặc design được cho target role |
| **P1 — Important** | phải dùng/review tự tin khi project cần |
| **P2 — Selective** | học đủ để quyết định và deep dive on demand |
| **P3 — Awareness** | biết problem/risk và nơi tra cứu |

Target level phụ thuộc role. Kubernetes có thể P1 cho Backend Engineer nhưng P0 cho Platform Engineer.

→ [Skills Matrix](skills-matrix.md)

---

# 3. Existing module map — chỉ các module thực sự tồn tại

| Module | Focus | Maturity | Runnable dedicated lab |
|---|---|---|---:|
| 00 Roadmap | learning mode, dependency, quality/source/version policy | Deep | — |
| 01 Computer Science | complexity, OS, memory, concurrency | Deep | **Yes** |
| 02 Linux/Git/Networking | process, DNS/TCP/TLS/HTTP, Git, troubleshooting | Deep | **Yes** |
| 03 .NET | C#, async, GC, ThreadPool, diagnostics | Deep | **Yes** |
| 04 Backend | request lifecycle, auth, jobs/webhooks | Deep | **Yes** |
| 05 SQL | schema, transactions, indexes/plans, EF→SQL | Deep/Guided | No |
| 06 API Design | HTTP contracts, auth, evolution, traffic, REST/gRPC/events | Deep/Guided | No |
| 07 ASP.NET Core | pipeline, resilience, deployment/operations | Deep/Guided | No |
| 08 Testing/Review | boundary tests, contract/load tests, quality gates | Deep/Guided | No |
| 09 Security/DevSecOps | trust boundary, identity/secrets, supply chain | Deep/Guided | No |
| 10 Performance | measurement, bottleneck, capacity, regression | Deep/Guided | No |
| 11 Redis/Caching | consistency, TTL, stampede, HA/operations | Deep/Guided | No |
| 12 Docker | build/runtime/network/storage/resources/security | Deep/Guided | No |
| 13 DevOps/IaC | CI/CD, artifact promotion, Terraform, recovery | Deep/Guided | No |
| 14 Azure | service selection/config/cost/reliability/.NET architecture | Deep handbook | No |
| 15 Kubernetes | reconciliation, workloads/network/storage/security/debugging/AKS | Deep/Guided | No |
| 17 Distributed Systems | partial failure, idempotency, messaging, outbox/saga/backpressure | Deep/Guided | No |
| 18 Microservices Architecture | boundaries, data ownership, saga, deployment/migration | Deep/Guided | No |
| 19 AI Engineering | models/tools/RAG/eval/security/operations | Deep/Guided | No |
| 21 AI Coding Agents | repo context, permissions, tests, review | Deep/Guided | No |
| 24 System Design | requirements/capacity/data/failure/security/cost/cases | Deep | No direct lab |
| 25 Software Architecture | quality attributes, boundaries/styles, DDD, evolution/ADR | Deep | No direct lab |

**Maturity != lab coverage.** Xem [Learning Quality Standard](learning-quality-standard.md).

---

# 4. Core Backend path

```text
03 .NET
→ 04 Backend
→ 05 SQL
→ 06 API Design
→ 07 ASP.NET Core
→ 08 Testing
→ 09 Security
→ 10 Performance
→ 12 Docker
```

Module 01/02 là prerequisite foundations; Module 11 Redis là conditional optimization/data-structure tool, không phải mặc định cho mọi app.

Backend foundation phải chứng minh được:

```text
request lifecycle
data invariant
transaction behavior
API contract
AuthN/AuthZ
integration tests
performance baseline
container runtime behavior
```

---

# 5. DevOps / Platform path

```text
02 Linux/Git/Networking
→ 08 quality gates
→ 09 supply-chain/security
→ 12 Docker
→ 13 DevOps/IaC
→ 14 Azure
→ 15 Kubernetes when orchestration justified
```

Kubernetes không thay CI/CD/IaC.

```text
CI
= prove + package

IaC
= provision/configure platform infrastructure

Kubernetes
= reconcile containerized application runtime

GitOps
= one CD operating model for desired cluster state
```

→ [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

---

# 6. Azure path

```text
cloud primitives
→ resource hierarchy / landing zones
→ identity / network
→ compute selection
→ data/storage/cache
→ messaging
→ edge/API/network security
→ observability/deployment
→ HA/backup/DR
→ cost/governance
→ .NET reference architecture
```

System Design should determine capability need first; Azure module maps that need to provider services.

Bad:

```text
We have Service Bus → design around it.
```

Better:

```text
Need durable work queue + DLQ + duplicate handling
→ evaluate Service Bus against requirements/limits/cost.
```

---

# 7. Kubernetes path

Prerequisite mental model:

```text
process + signal
DNS/TCP
container image/runtime
CPU/memory
health endpoint
artifact/deployment lifecycle
```

Then:

```text
Cluster / Control Plane / Worker
→ API objects + reconciliation
→ Deployment / ReplicaSet / Pod
→ Service / DNS / selectors
→ ConfigMap / Secret
→ resources / scheduling
→ probes
→ storage
→ RBAC/security
→ rollout/autoscaling
→ kubectl debugging
→ AKS mapping
```

Core flow:

```text
Deployment → ReplicaSet → Pod → Container
Client → DNS → Service → selector → Ready Pods
```

→ [Module 15](../15-kubernetes/README.md)

---

# 8. Distributed Systems → Microservices

Distributed Systems first:

```text
timeout / unknown outcome
retry / duplicate
idempotency
messaging
outbox/inbox
ordering
backpressure
reconciliation
saga/compensation
```

Then Microservices only when business/ownership/deployment boundary pressure exists:

```text
business capability
→ bounded context
→ data ownership
→ explicit contract
→ independent lifecycle
```

Kubernetes does not solve distributed correctness.

→ [Distributed Systems](../17-distributed-systems/README.md) · [Microservices](../18-microservices-architecture/README.md)

---

# 9. System Design → Software Architecture

## System Design

Answers:

```text
requirements
capacity
traffic
data/consistency
failure
availability
security
cost
```

## Software Architecture

Answers:

```text
quality attributes
boundaries/ownership
styles/coupling
integration/data ownership
team/deployment structure
evolution/governance
```

Sequence:

```text
24 System Design
→ 25 Software Architecture
```

not because architecture always happens later, but because this learning order helps ground architecture choices in workload/evidence rather than pattern fashion.

---

# 10. Production AI path

AI sits on software engineering foundation:

```text
Backend/API/Data/Security
→ Cloud/Delivery
→ Distributed failure reasoning
→ 19 AI Engineering
→ 21 Coding Agents when relevant
→ 24/25 system/architecture review
```

AI-specific capabilities:

```text
model/provider
structured output
tool calling
retrieval/RAG
evaluation
AI authorization/security
latency/cost
prompt/model/index lifecycle
fallback/degradation
```

Do not create dedicated complexity before project pressure exists.

---

# 11. Integrated/future capability backlog — không giả làm module đang tồn tại

Một số topics chưa có dedicated directory nhưng đã được tích hợp hoặc là future expansion.

| Capability | Current coverage | Dedicated module? | Decision |
|---|---|---:|---|
| Observability / OTel / SLO | 07, 10, 14, 15, 17, 19 | No | expand only if navigation/reuse justify |
| RAG | 19 | No | keep production RAG inside AI Engineering until depth demands split |
| AI Security | 19 + 21 | No | dedicated red-team/security track is future candidate |
| GenAIOps/MLOps | 19 | No | future when executable eval/release pipeline exists |
| Architecture Docs | 24 + 25 | No | add reusable ADR/C4/runbook examples before new numeric module |
| Data Engineering | selective references only | No | future P2 track |

Repository không reserve fake module numbers trong navigation cho các capability này.

---

# 12. Project/evidence spine — distinguish spec vs executable

## Executable labs currently committed

```text
labs/01-computer-science
labs/02-linux-git-networking
labs/03-dotnet
labs/04-backend
```

## Documented project/evidence targets

Các module sau mô tả project/failure drills nhưng chưa phải tất cả đều có runnable artifact committed:

| Target | Modules connected | Desired evidence |
|---|---|---|
| Production Backend | 05–08 | schema/tests/OpenAPI/query plan |
| Production Delivery | 09–13 | security/load/cache/Docker/CI evidence |
| Kubernetes App | 15 | manifests + failures + rollout/debug |
| Distributed Checkout | 17–18 | outbox/dedup/UNKNOWN/reconciliation |
| Azure .NET Platform | 14 | IaC + network/identity/cost/DR |
| Enterprise AI Assistant | 19 | AuthZ/RAG/tools/eval/telemetry |
| Design Dossier | 24–25 | capacity/failure/ADR/evolution |

Do not label these “runnable project” until artifact exists.

---

# 13. Definition of Done

Learning evidence, increasing strength:

```text
Can define
→ Can explain failure/trade-off
→ Can implement
→ Can test
→ Can break/debug
→ Can deploy/operate/recover
→ Can design/review from evidence/cost
```

Strong evidence:

```text
runnable code
schema/constraints
OpenAPI
unit/integration/contract tests
Docker/IaC/manifests
load result
trace/dashboard
failure injection
restore/reconcile
security negative tests
ADR
cost note
```

---

# 14. Architect quality gate

A serious design/review should answer:

1. Business outcome?
2. Invariants?
3. Source of truth/data ownership?
4. Workload/capacity/SLO?
5. Critical failure modes?
6. Timeout/unknown outcome semantics?
7. Duplicate/retry semantics?
8. Security/trust boundary?
9. Deploy/rollback/recovery?
10. Cost drivers?
11. Why not a simpler solution?
12. Revisit/migration trigger?

## Use next

- [Role-based Learning Paths](role-based-learning-paths.md)
- [Skills Matrix](skills-matrix.md)
- [Learning Quality Standard](learning-quality-standard.md)
- [Repository Quality Review](repository-quality-review-2026-08-28.md)
- [Technology Baseline](technology-baseline.md)
