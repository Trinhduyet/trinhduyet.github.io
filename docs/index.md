<div class="hero-shell" markdown>

<span class="hero-kicker">Software Architecture Engineering · System Builder Roadmap</span>

# Build Systems: .NET → DevOps → Azure/Kubernetes → Design → AI

Học bằng **system thật, failure thật và evidence thật**. Repository nối Backend, Database, API, Production Engineering, DevOps, Cloud, Kubernetes, Distributed Systems, System Design, Software Architecture và AI thành một dependency graph — không phải checklist công nghệ.

<div class="hero-actions" markdown>

[Chọn lộ trình theo role →](00-roadmap/role-based-learning-paths.md){ .hero-button .hero-button-primary }
[Học bằng Checkout system →](00-roadmap/example-first-learning-path.md){ .hero-button }
[Master Roadmap →](00-roadmap/master-roadmap.md){ .hero-button }
[Quality Review →](00-roadmap/repository-quality-review-2026-08-28.md){ .hero-button }

</div>

</div>

<div class="stat-grid">
  <div class="stat-card"><strong>Backend Core</strong><span>.NET · SQL · API · ASP.NET Core</span></div>
  <div class="stat-card"><strong>Production</strong><span>testing · security · performance · Docker</span></div>
  <div class="stat-card"><strong>Platform</strong><span>DevOps · IaC · Azure · Kubernetes</span></div>
  <div class="stat-card"><strong>Design + AI</strong><span>distributed · architecture · RAG · tools · eval</span></div>
</div>

## Có 60 phút? Đừng đọc roadmap trước

<div class="learning-path" markdown>
<strong>First session</strong>

```text
10 phút — chọn role/problem
15 phút — đọc một mental model
20 phút — chạy hoặc thiết kế một scenario
10 phút — cố tình tạo failure / inspect evidence
 5 phút — ghi 3 điều hiểu + 1 gap
```

**Bắt đầu:** [Role-based Learning Paths](00-roadmap/role-based-learning-paths.md)
</div>

Nếu chưa biết chọn role nào, bắt đầu với **.NET Backend Engineer**. Đừng mở Kubernetes, Microservices hoặc AI vì chúng “hot”; mở chúng khi problem thật tạo dependency.

---

## Chọn track theo vấn đề đang giải quyết

<div class="course-grid" markdown>

[**.NET Backend Engineering** — C#, async, SQL, API Design, ASP.NET Core và production request behavior.](03-dotnet/README.md){ .course-card }

[**Testing / Security / Performance** — quality gates, threat boundaries, bottlenecks, capacity và regression.](08-testing-code-review/README.md){ .course-card }

[**DevOps & Kubernetes** — artifact promotion, IaC, Docker delivery, Kubernetes runtime, GitOps và debugging.](13-devops-iac/README.md){ .course-card }

[**Azure & Platform** — landing zones, identity/network, compute/data/messaging, reliability, DR và cost.](14-cloud/README.md){ .course-card }

[**Distributed Systems** — partial failure, timeout, retry, idempotency, messaging, consistency và reconciliation.](17-distributed-systems/README.md){ .course-card }

[**Microservices Architecture** — business boundaries, data ownership, contracts, Saga, deployment và migration.](18-microservices-architecture/README.md){ .course-card }

[**System Design** — requirements, capacity, data, queues, availability, security, cost và design evidence.](24-system-design/README.md){ .course-card }

[**Software Architecture** — quality attributes, DDD, modularity, styles, ADR, fitness functions và evolution.](25-software-architecture/README.md){ .course-card }

[**AI Engineering & Coding Agents** — structured output, tools, RAG, eval, production operations và governed coding agents.](19-ai-engineering/README.md){ .course-card }

</div>

---

## Một mental model cho toàn site

```text
Business problem
      ↓
Functional requirements + NFR/SLO
      ↓
Simplest working request/data path
      ↓
Correctness + security
      ↓
Measure workload / bottleneck
      ↓
Package + deploy + observe
      ↓
Failure: timeout / duplicate / overload / outage
      ↓
Recover / reconcile
      ↓
Scale / cost / platform decisions
      ↓
System Design
      ↓
Architecture boundaries / evolution
      ↓
AI capability when it creates business value
```

<div class="key-takeaway" markdown>
<strong>Rule cốt lõi</strong>

Không học `Redis`, `Kafka`, `Kubernetes`, Azure services, Microservices hay RAG như đáp án mặc định. Component chỉ xuất hiện khi một **requirement, bottleneck, failure mode, ownership constraint hoặc operating constraint** justify nó.
</div>

---

## Đường học cốt lõi

```text
FOUNDATION
CS + Linux/Git/Networking
        ↓
BACKEND
.NET → Backend → SQL → API → ASP.NET Core
        ↓
PRODUCTION
Testing → Security → Performance → Redis when justified → Docker
        ↓
DELIVERY / PLATFORM
DevOps/IaC → Azure
           ↘ Kubernetes when justified
        ↓
DISTRIBUTED
Distributed Systems → Microservices when justified
        ↓
DESIGN
System Design → Software Architecture
        ↓
AI
AI Engineering / Coding Agents on top of engineering foundation
```

→ [Master Roadmap](00-roadmap/master-roadmap.md)

---

## DevOps và Kubernetes đi cùng nhau như thế nào?

```text
Pull Request
  ↓
Testing + Security gates
  ↓
CI builds immutable artifact/image
  ↓
Registry
  ↓
IaC provisions platform
  ↓
CD / GitOps
  ↓
Kubernetes Deployment
  ↓
Service → Ready Pods
  ↓
Observability
  ↓
Rollback / roll-forward / recovery
```

Nhưng:

```text
DevOps != Kubernetes
Kubernetes != CI/CD
```

Một app nhỏ có thể deploy tốt bằng App Service/Container Apps mà không cần cluster. Kubernetes đáng học sâu khi orchestration/platform requirements justify operational cost.

→ [DevOps → Kubernetes Production Delivery](13-devops-iac/devops-kubernetes-production-delivery.md)

---

## Azure — map requirement sang service, không map ngược

```text
Requirement
→ compute/data/network/messaging capability
→ candidate Azure services
→ tier/config/identity/network
→ HA/backup/DR
→ observability
→ cost
```

Ví dụ:

```text
Need durable command queue + DLQ + duplicate handling
→ evaluate Service Bus
```

không phải:

```text
We know Service Bus
→ force architecture to use it
```

→ [Module 14 — Microsoft Azure & Cloud Platform](14-cloud/README.md)

---

## Kubernetes — hai flow phải hiểu trước YAML

```text
Deployment
  ↓
ReplicaSet
  ↓
Pod
  ↓
Container
```

và traffic:

```text
Client
  ↓
DNS
  ↓
Service
  ↓ selector
Ready Pods
```

Sau đó mới học ConfigMap/Secret, resources, probes, PVC, RBAC, HPA, rollout và `kubectl` debugging.

→ [Module 15 — Kubernetes](15-kubernetes/README.md)

---

## System Design và Software Architecture khác nhau ở đâu?

**System Design** bắt đầu từ workload:

```text
requirements
→ capacity
→ data/consistency
→ failure
→ availability/security/cost
→ evidence
```

**Software Architecture** bắt đầu từ quality attributes và structure/evolution:

```text
quality attributes
→ boundaries/ownership
→ style/integration
→ deployment/team/data coupling
→ ADR/fitness functions
→ migration/evolution
```

→ [System Design](24-system-design/README.md) · [Software Architecture](25-software-architecture/README.md)

---

## AI là capability, không phải toàn system

```text
Software Engineering Foundation
           ↓
Production System
           ↓
Model / RAG / Tools
           ↓
Evaluation + AuthZ + Observability + Cost
```

Một AI product vẫn cần backend, data, API, security, deployment, failure handling và recovery.

→ [AI Engineering cho Software / Backend Engineer](19-ai-engineering/README.md)

---

## Học bằng một system xuyên suốt

```text
Customer
   ↓
Checkout API
   ↓
Order
 ├─ Inventory
 ├─ Payment
 └─ Notification

AI Assistant
   ↓
read-only authorized business tools
```

Cùng scenario được nâng dần:

```text
C# / async
→ SQL
→ API / ASP.NET Core
→ Tests / Security / Performance
→ Redis when justified
→ Docker / DevOps
→ Azure / Kubernetes when needed
→ Distributed failure
→ Microservices when justified
→ System Design / Architecture
→ AI capability
```

→ [Example-First Checkout Path](00-roadmap/example-first-learning-path.md)

---

## Evidence thật — và repo đang ở đâu

Không tính “đã học” chỉ vì đọc xong.

Evidence tốt:

```text
runnable code
schema / constraints / execution plan
API contract + tests
container/IaC/manifests
load result
logs / metrics / traces
failure injection
rollback / restore / reconciliation
security negative test
ADR + cost + migration trigger
```

### Quan trọng: content depth != runnable lab

Repository hiện có dedicated executable labs rõ cho Modules **01–04**. Nhiều module sau đã có deep guided exercises/failure drills nhưng chưa có committed runnable lab tương ứng.

Đây là backlog chính tiếp theo, không được che bằng chữ “Done”.

→ [Repository Quality Review — 2026-08-28](00-roadmap/repository-quality-review-2026-08-28.md)

---

## Quality bar của repository

Một P0/P1 chapter nên hướng tới:

```text
Problem
→ Mental model
→ Minimal implementation
→ Expected state
→ Failure
→ Debug
→ Recovery/Fix
→ Trade-off
→ Evidence
```

→ [Learning Quality Standard](00-roadmap/learning-quality-standard.md)

---

## Tra cứu nhanh

- [Role-based Learning Paths](00-roadmap/role-based-learning-paths.md)
- [Human Learning Mode](00-roadmap/human-learning-mode.md)
- [Master Roadmap](00-roadmap/master-roadmap.md)
- [Skills Matrix](00-roadmap/skills-matrix.md)
- [Concept Cards](00-roadmap/concept-cards.md)
- [Practical Mini-Labs](00-roadmap/practical-mini-labs.md)
- [Technology Baseline](00-roadmap/technology-baseline.md)
- [Repository Quality Review](00-roadmap/repository-quality-review-2026-08-28.md)

<div class="architect-note" markdown>
<strong>Architect / Senior / Production AI mindset</strong>

Một sơ đồ đẹp, cluster chạy được hay prompt demo chưa chứng minh đủ. Engineer mạnh phải giải thích được: **requirement nào tạo ra design, invariant nào phải bảo vệ, failure nào đã test, system deploy/observe/recover ra sao, cost gì phải trả và khi nào architecture cần evolve.**
</div>
