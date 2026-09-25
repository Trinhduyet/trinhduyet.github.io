# Repository Health — Current Learning System

> Current review: **2026-09-25** · Scope: `docs/`, `labs/`, navigation, version baseline and Pages CI.

This page is the **current status** of the repository. Historical review snapshots remain available for audit/history, but roadmap and homepage should link here when describing present maturity.

## Executive summary

The repository is now strong on **breadth, production reasoning and source discipline**. The primary gap is no longer missing theory; it is **executable evidence across the middle of the learning path**.

Current shape:

```text
Foundations        → strong + runnable
Backend core       → strong; 05–08 still need integrated runnable evidence
Production         → deep/guided; 09–12 need executable failure loops
DevOps/IaC         → deep + Terraform/Azure runnable baseline
Azure              → deep handbooks + architecture/governance guidance
Kubernetes         → deep + runnable core
Distributed        → deep; runnable reliability lab still missing
AI Engineering     → deep + runnable core/MEAI
Coding Agents      → deep/guided; dedicated sandbox/review lab missing
System/Architecture→ deep design/evidence guidance
```

---

# 1. Actual module inventory

Current learning modules:

```text
01 Computer Science
02 Linux / Git / Networking
03 .NET Runtime
04 Backend Engineering
05 SQL
06 API Design
07 ASP.NET Core
08 Testing & Code Review
09 Security & DevSecOps
10 Performance
11 Redis & Caching
12 Docker
13 DevOps & IaC
14 Azure
15 Kubernetes
17 Distributed Systems
18 Microservices Architecture
19 AI Engineering
21 AI Coding Agents
24 System Design
25 Software Architecture
```

Module numbers reflect repository evolution, not a mandatory sequential syllabus.

---

# 2. Runnable evidence that actually exists

Dedicated lab directories currently committed:

```text
labs/01-computer-science
labs/02-linux-git-networking
labs/03-dotnet
labs/04-backend
labs/13-terraform-azure
labs/15-kubernetes
labs/19-ai-engineering
```

Meaning:

| Area | Runnable evidence |
|---|---|
| Foundations 01–04 | code/runtime/network/backend scenarios |
| Terraform + Azure IaC 13 | Terraform 1.16.4 fmt/init/validate/test + AzureRM mock-provider contracts |
| Kubernetes 15 | Deployment/ReplicaSet/Pod, Service/selectors, probes, rollout/debugging |
| AI Engineering 19 | deterministic core + Microsoft.Extensions.AI self-test/eval/failure paths |

Important:

```text
deep content
!= runnable evidence

guided exercise
!= CI-verified lab
```

---

# 3. CI evidence

Pages CI currently verifies:

```text
Terraform 1.16.4 lab
        ↓
AI Engineering .NET 10 lab
        ↓
portable-path audit
        ↓
learning-quality audit
        ↓
site-consistency audit
        ↓
diagram audit
        ↓
mkdocs build --strict
        ↓
critical-page verification
        ↓
GitHub Pages deploy
        ↓
live commit marker verification
```

This means repository health is not judged only by Markdown compiling.

---

# 4. Findings from the 2026-09-25 review

## Finding A — status pages had drifted from executable reality

Homepage/master roadmap still described runnable coverage as only 01–04 + Kubernetes even after Terraform/Azure and AI labs were added.

Fix:

```text
homepage
master roadmap
repository health
CI consistency audit
```

now use the actual lab-directory set.

## Finding B — Terraform chapters existed but were hidden from navigation

The following production-depth chapters existed but were not exposed in the sidebar:

```text
terraform-language-lifecycle-and-validation.md
terraform-team-workflows-and-state-recovery.md
```

Fix: expose them directly under DevOps & IaC.

## Finding C — Azure navigation contained a duplicate core-platform entry

Duplicate navigation is easy to introduce through text-based edits.

Fix: remove the duplicate and audit duplicate nav targets, with a tiny explicit allow-list only for intentional shared pages.

## Finding D — technology baseline had stale servicing versions

The baseline had mixed verification dates. The 2026-09-25 refresh updates fast-moving rows:

```text
.NET 10      → 10.0.12 servicing
EF Core 10   → 10.0.12
SQL Server 2025 → CU9 / September 2026
Docker Engine 29 → 29.8.1
Terraform    → 1.16.4 stable
Kubernetes   → upstream 1.37.0 current release line
```

The repository still separates:

```text
documentation baseline
!= executable lab pin
!= managed-cloud supported version
```

---

# 5. Maturity map

| Module | Content | Runnable evidence | Current review |
|---|---|---:|---|
| 01–04 Foundations/Backend | Deep | **Yes** | strong executable foundation |
| 05 SQL | Deep/Guided | No dedicated | integrate into production backend lab |
| 06 API Design | Deep/Guided | No dedicated | add contract/API evidence with 05–08 |
| 07 ASP.NET Core | Deep/Guided | No dedicated | integrate hosting/ops evidence |
| 08 Testing/Review | Deep/Guided | No dedicated | connect tests to runnable app/CI |
| 09 Security | Deep/Guided | No | needs negative/security failure lab |
| 10 Performance | Deep/Guided | No | needs load/profiling evidence |
| 11 Redis | Deep/Guided | No | needs invalidation/stampede/degraded-mode lab |
| 12 Docker | Deep/Guided | No dedicated | needs Compose/runtime evidence |
| 13 DevOps/IaC | **Deep/Runnable** | **Yes — Terraform/Azure baseline** | CI verifies Terraform contracts |
| 14 Azure | Deep handbook | **Partial via 13** | live Azure integration remains optional/credentialed |
| 15 Kubernetes | **Deep/Runnable** | **Yes — core** | strong core; advanced ops can expand |
| 17 Distributed Systems | Deep | No | outbox/idempotency/recovery lab is high leverage |
| 18 Microservices | Deep | No | checkout saga/UNKNOWN reconciliation lab missing |
| 19 AI Engineering | **Deep/Runnable** | **Yes — core + MEAI** | deterministic and MEAI paths verified |
| 21 AI Coding Agents | Deep/Guided | No | sandbox/context/review lab missing |
| 24 System Design | Deep | design evidence | runnable system comes from connected modules |
| 25 Software Architecture | Deep | design evidence | ADR/evolution/review evidence |

---

# 6. Highest-leverage backlog

Stop maximizing topic count. Prefer integrated executable learning loops.

## P0

```text
05–08 Production Backend Lab
→ SQL + EF Core + API + ASP.NET + integration/contract tests

09–12 Production Runtime Lab
→ security negative tests + load + Redis + Docker

17 Distributed Reliability Lab
→ outbox/inbox + duplicate + crash-before-ACK + reconciliation

18 Checkout Saga Lab
→ payment UNKNOWN + compensation/reconciliation

21 Coding Agent Lab
→ sandbox + permission boundary + malicious repo instruction + diff/test/review evidence
```

## P1

```text
14 Azure live sandbox extension
→ OIDC + remote state + private network/identity + drift/recovery

15 Kubernetes advanced exercises
→ RBAC + PVC + NetworkPolicy + HPA

19 AI provider/RAG release extension
→ real provider + vector/search + eval release gate
```

---

# 7. Navigation quality rule

A deep chapter that is part of the primary learning path should not be discoverable only through a README link.

Required sidebar visibility now includes:

```text
repository health / technology baseline
Terraform language/lifecycle
Terraform team/state recovery
Terraform on Azure
Azure core/service-selection/governance
AI vocabulary/runtime/MEAI
Coding-agent vocabulary/workflow
```

Historical reports/research files may remain outside primary navigation.

---

# 8. Evidence rule

A module should move from Guided → Runnable only when repository evidence exists.

Good signals:

```text
build/test output
schema/query plan
HTTP contract
Terraform plan/test
container/manifests
load result
logs/metrics/traces
failure injection
rollback/restore/reconciliation
security negative test
ADR + measured trade-off
```

Not sufficient alone:

```text
read the chapter
copied a snippet
architecture diagram with no failure model
tool says "success"
```

---

# 9. Source/version policy

Canonical behavior comes from official project/vendor docs.

For fast-moving technologies:

```text
concept page
→ stable mental model

technology baseline
→ current repository snapshot

lab config
→ exact executable pin

production deploy
→ provider/cloud support matrix checked again
```

---

# 10. Exit criteria for repository quality

This repository review is healthy when:

- [x] role/problem-first entry path exists;
- [x] module maturity is separated from lab maturity;
- [x] current runnable labs are accurately listed;
- [x] Terraform/Azure/AI/Kubernetes deep chapters are visible in navigation;
- [x] technology baseline has per-row verification dates;
- [x] CI validates runnable Terraform and AI labs;
- [x] CI builds docs strictly and verifies live deployment;
- [x] CI checks status/navigation consistency;
- [ ] integrated executable labs exist for 05–12;
- [ ] distributed reliability and checkout saga are runnable;
- [ ] coding-agent sandbox/review path is runnable.

## Related

- [Role-based Learning Paths](role-based-learning-paths.md)
- [Master Roadmap](master-roadmap.md)
- [Skills Matrix](skills-matrix.md)
- [Technology Baseline](technology-baseline.md)
- [Learning Quality Standard](learning-quality-standard.md)
- [Historical review — 2026-08-28](repository-quality-review-2026-08-28.md)

## Verification metadata

- Review date: **2026-09-25**.
- Inventory checked against the repository tree on `main`.
- Runnable coverage checked against actual `labs/*` directories.
- CI/navigation/status inconsistencies found during this review are addressed in the same change set.
