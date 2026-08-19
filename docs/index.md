<div class="hero-shell" markdown>

<span class="hero-kicker">Software Architecture Engineering · System Builder Roadmap</span>

# Build Systems: .NET → Azure → System Design → Architecture → AI

Học bằng **hệ thống thật, failure thật và project có evidence**. Roadmap nối Backend, Database, API, Cloud/Infra, Deployment, Distributed Systems, System Design, Software Architecture và AI thành một đường học thống nhất.

<div class="hero-actions" markdown>

[Đi theo lộ trình thực hành →](00-roadmap/example-first-learning-path.md){ .hero-button .hero-button-primary }
[Master Roadmap →](00-roadmap/master-roadmap.md){ .hero-button }
[System Design →](24-system-design/README.md){ .hero-button }
[Azure →](14-cloud/README.md){ .hero-button }

</div>

</div>

<div class="stat-grid">
  <div class="stat-card"><strong>Backend</strong><span>.NET · SQL · API · testing · security</span></div>
  <div class="stat-card"><strong>Azure</strong><span>identity · network · compute · data · messaging · DR</span></div>
  <div class="stat-card"><strong>System Design</strong><span>capacity · consistency · failure · trade-offs · projects</span></div>
  <div class="stat-card"><strong>Architecture + AI</strong><span>DDD · evolution · RAG · tools · eval · production</span></div>
</div>

## Hãy học cách build hệ thống

<div class="learning-path" markdown>
<strong>Đường học cốt lõi</strong>

```text
Làm backend
→ Hiểu database
→ Biết API
→ Hiểu cloud / infrastructure
→ Biết deploy và operate sản phẩm
→ Hiểu distributed failure
→ Biết System Design / Software Architecture
→ Có project thật để chứng minh build được
```
</div>

**AI Engineer trong thực tế không chỉ là người “biết AI”.** Một AI Engineer production phải đủ kỹ năng software/backend engineering để đưa model, RAG và tools vào **một hệ thống có authentication, database, API, cloud, deployment, observability, evaluation và recovery**.

```text
Software Engineering Foundation
           ↓
Production System
           ↓
AI becomes a capability
—not the whole system—
```

→ [AI Engineering cho Software / Backend Engineer](19-ai-engineering/README.md)

## Chọn track theo vấn đề anh đang giải quyết

<div class="course-grid" markdown>

[**.NET Backend Engineering** — C#, async, SQL, API Design, ASP.NET Core và production behavior.](03-dotnet/README.md){ .course-card }

[**Platform & Azure** — cloud primitives → landing zones → identity/network → compute/data/messaging → DR/cost.](14-cloud/README.md){ .course-card }

[**Distributed Systems** — partial failure, timeout, retry, idempotency, messaging, consistency và reconciliation.](17-distributed-systems/README.md){ .course-card }

[**Microservices Architecture** — boundaries, data ownership, contracts, Saga, migration và operations.](18-microservices-architecture/README.md){ .course-card }

[**System Design** — 36 concepts, capacity math, cache, partitioning, queues, multi-region, security, cost và project evidence.](24-system-design/README.md){ .course-card }

[**Software Architecture** — quality attributes, DDD, Clean/Hexagonal, Modular Monolith, EDA/CQRS, ADR và evolution.](25-software-architecture/README.md){ .course-card }

[**AI Engineering** — engineering foundation + structured output, tools, RAG, evaluation, security, deploy và operate.](19-ai-engineering/README.md){ .course-card }

[**AI Coding Agents** — repository context, MCP, safe edit/build/test/PR workflow.](21-ai-coding-agents/README.md){ .course-card }

</div>

## Một mental model cho toàn site

```text
Business problem
      ↓
Functional requirements + NFR
      ↓
Backend/API + data model
      ↓
Database invariants / transaction boundary
      ↓
External side effects
      ↓
Failure: timeout / duplicate / overload
      ↓
Cloud / Infrastructure / Deployment
      ↓
Observe / Recover / Reconcile
      ↓
Capacity / Cost
      ↓
System Design
      ↓
Architecture boundary / evolution
      ↓
AI capability when it creates business value
```

<div class="key-takeaway" markdown>
<strong>Điểm cốt lõi</strong>

Không học `Redis`, `Kafka`, `Kubernetes`, `Azure Service Bus`, `microservices` hay `RAG` như đáp án mặc định. Mỗi component chỉ xuất hiện khi một **requirement, bottleneck, failure mode, data constraint hoặc operating constraint** justify nó.
</div>

## Azure track — từ cloud generic tới kiến trúc Azure thực tế

```text
Cloud mental model
      ↓
Tenant / Management Group / Subscription / Resource Group
      ↓
Landing Zone + Policy + RBAC
      ↓
Entra ID + Managed Identity + Key Vault
      ↓
VNet / Private Endpoint / Front Door / APIM
      ↓
App Service / Functions / Container Apps / AKS
      ↓
Azure SQL / Cosmos DB / Blob / Managed Redis
      ↓
Service Bus / Event Grid / Event Hubs
      ↓
Availability Zones / DR / Azure Monitor / Cost
      ↓
.NET production reference architecture
```

**Bắt đầu:** [Module 14 — Cloud & Azure](14-cloud/README.md)

## System Design — học component bằng failure và trade-off

```text
Clarify
→ Estimate
→ Model data
→ Draw simple path
→ Find pressure
→ Add scale mechanisms
→ Design failure behavior
→ Prove with evidence
```

Module mới có **36 Concepts & Trade-offs**, capacity math, data/consistency, queues/backpressure, multi-region/DR, case studies và production projects.

**Bắt đầu:** [Module 24 — System Design](24-system-design/README.md)

## Software Architecture — cấu trúc hệ thống để thay đổi được

```text
Quality attributes
      ↓
Boundaries / ownership
      ↓
DDD / domain model
      ↓
Architecture style
      ↓
Sync vs async integration
      ↓
Modular Monolith vs Microservices
      ↓
ADR + fitness functions
      ↓
Evolution / migration
```

**Bắt đầu:** [Module 25 — Software Architecture](25-software-architecture/README.md)

## Học bằng một hệ thống xuyên suốt

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

Cùng bài toán được nâng cấp qua:

```text
C# / async
→ SQL
→ API Design
→ ASP.NET Core
→ Redis
→ Docker
→ Azure / Kubernetes when needed
→ Distributed Systems
→ Microservices when justified
→ System Design
→ Software Architecture
→ AI Engineering
```

[Đi theo Example-First Checkout Path →](00-roadmap/example-first-learning-path.md)

## Project thật là Definition of Learning

Không tính “đã học” chỉ vì đọc xong.

Evidence tốt:

```text
runnable code
+ SQL schema / constraints / execution plan
+ OpenAPI / API contract
+ Docker / IaC
+ Azure deployment
+ logs / metrics / traces
+ load test
+ duplicate / timeout / backlog failure drills
+ backup / restore / reconciliation
+ ADR / cost / migration trigger
```

→ [Production System Design Projects & Evidence](24-system-design/production-projects-and-evidence.md)

## Cách học 60 phút

```text
20 phút — chạy/build scenario
20 phút — cố tình làm hỏng
20 phút — đọc lý thuyết giải thích behavior vừa thấy
```

Ví dụ:

```text
Payment provider timeout
→ local service không biết provider đã charge hay chưa
→ FAILED không đủ nghĩa
→ state = UNKNOWN
→ idempotency + reconciliation
→ sau đó mới bàn queue, Saga hay microservices
```

## Tra cứu nhanh

- [Human Learning Mode](00-roadmap/human-learning-mode.md)
- [Master Roadmap](00-roadmap/master-roadmap.md)
- [Concept Cards](00-roadmap/concept-cards.md)
- [Practical Mini-Labs](00-roadmap/practical-mini-labs.md)
- [Glossary](00-roadmap/glossary.md)
- [Azure References](14-cloud/references.md)
- [System Design References](24-system-design/references.md)
- [Software Architecture References](25-software-architecture/references.md)

<div class="architect-note" markdown>
<strong>Architect / AI Engineer mindset</strong>

Một sơ đồ đẹp hoặc một prompt demo chưa chứng minh nhiều. Một engineer mạnh phải giải thích được: **requirement nào tạo ra design, invariant nào phải bảo vệ, failure nào đã test, system deploy/observe/recover ra sao, cost gì phải trả và khi nào architecture cần evolve.**
</div>
