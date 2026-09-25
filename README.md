# Software Architecture Engineering — Systems Learning Roadmap

> Lộ trình thực chiến bằng tiếng Việt: **.NET Backend → Production Engineering → DevOps/IaC → Azure/Kubernetes → Distributed Systems → System Design/Architecture → Production AI**.

**Live site:** https://trinhduyet.github.io/

Repository này không phải catalog công nghệ. Mục tiêu là học theo dependency và tạo **evidence có thể kiểm chứng**:

```text
Problem
→ Mental model
→ Minimal implementation
→ Failure
→ Debug
→ Recovery
→ Trade-off
→ Evidence
```

## Bắt đầu ở đâu?

- [Role-based Learning Paths](docs/00-roadmap/role-based-learning-paths.md) — chọn path theo Backend, Platform, Azure, Kubernetes, Architect, AI.
- [Example-First Checkout Path](docs/00-roadmap/example-first-learning-path.md) — học xuyên suốt bằng một system.
- [Master Roadmap](docs/00-roadmap/master-roadmap.md) — dependency landscape đầy đủ.
- [Repository Health](docs/00-roadmap/repository-health.md) — maturity, runnable evidence và backlog hiện tại.
- [Technology Baseline](docs/00-roadmap/technology-baseline.md) — version snapshot và source policy.

## Learning graph

```text
FOUNDATIONS
Computer Science + Linux/Git/Networking
        ↓
BACKEND CORE
.NET → Backend → SQL → API → ASP.NET Core
        ↓
PRODUCTION
Testing → Security → Performance → Redis when justified → Docker
        ↓
DELIVERY / PLATFORM
DevOps / Terraform → Azure
                   ↘ Kubernetes when justified
        ↓
DISTRIBUTED
Distributed Systems → Microservices when justified
        ↓
DESIGN
System Design → Software Architecture
        ↓
AI
Production AI Engineering → Coding Agents when relevant
```

Module numbers reflect repository history, **not** a mandatory sequential syllabus.

## Current module groups

| Track | Modules |
|---|---|
| Foundations | 01 Computer Science · 02 Linux/Git/Networking |
| Backend Core | 03 .NET · 04 Backend · 05 SQL · 06 API Design · 07 ASP.NET Core |
| Production | 08 Testing · 09 Security · 10 Performance · 11 Redis · 12 Docker |
| Platform | 13 DevOps/IaC · 14 Azure · 15 Kubernetes |
| Distributed | 17 Distributed Systems · 18 Microservices |
| AI | 19 AI Engineering · 21 AI Coding Agents |
| Design | 24 System Design · 25 Software Architecture |

## Runnable evidence currently committed

```text
labs/01-computer-science
labs/02-linux-git-networking
labs/03-dotnet
labs/04-backend
labs/13-terraform-azure
labs/15-kubernetes
labs/19-ai-engineering
```

Highlights:

- **Terraform/Azure IaC:** Terraform 1.16.4, mock AzureRM provider, positive + negative contract tests.
- **Kubernetes:** Deployment → ReplicaSet → Pod, Service/selectors, readiness, rollout/debugging.
- **AI Engineering:** deterministic core + `Microsoft.Extensions.AI` self-test/eval/failure paths.

Deep content without a dedicated runnable lab is intentionally reported as **Guided/Deep**, not falsely marked “Done”.

## Current highest-leverage gaps

```text
05–08 Production Backend Lab
09–12 Production Runtime/Delivery Lab
17 Distributed Reliability Lab
18 Checkout Saga / UNKNOWN reconciliation
21 Coding Agent sandbox/context/review lab
```

Optional production extensions:

```text
14 Azure live sandbox
15 Kubernetes RBAC/PVC/NetworkPolicy/HPA
19 real-provider/vector-search/eval release gate
```

See [Repository Health](docs/00-roadmap/repository-health.md) for the current review.

## Platform path

```text
Git change
→ tests/security gates
→ immutable artifact
→ Terraform/IaC
→ Azure platform
→ Kubernetes only when orchestration is justified
→ telemetry
→ rollback / recovery
```

Useful guides:

- [Terraform State, Graph, Modules & Drift](docs/13-devops-iac/terraform-state-modules-and-drift.md)
- [Terraform Language, Lifecycle & Validation](docs/13-devops-iac/terraform-language-lifecycle-and-validation.md)
- [Terraform Team Workflow & State Recovery](docs/13-devops-iac/terraform-team-workflows-and-state-recovery.md)
- [Terraform on Azure](docs/13-devops-iac/terraform-on-azure-production.md)
- [Azure Service Selection & Workload Architecture](docs/14-cloud/azure-service-selection-and-workload-architecture.md)
- [Azure Governance, Security & Operations](docs/14-cloud/azure-governance-security-and-operations-playbook.md)
- [Kubernetes Learning Path](docs/15-kubernetes/README.md)

## Production AI path

```text
Software engineering foundation
→ model/provider/runtime
→ messages/context/reasoning
→ Microsoft.Extensions.AI
→ structured output/tools
→ RAG
→ eval/observability/security
→ coding-agent workflow when relevant
```

Useful guides:

- [AI Engineer Vocabulary & System Boundaries](docs/19-ai-engineering/ai-engineer-vocabulary-and-system-boundaries.md)
- [LLM Runtime, Context & Reasoning](docs/19-ai-engineering/llm-runtime-messages-context-and-reasoning.md)
- [Microsoft.Extensions.AI](docs/19-ai-engineering/microsoft-extensions-ai-dotnet-integration.md)
- [AI Coding Agent Vocabulary, Context & Handoffs](docs/21-ai-coding-agents/ai-coding-agent-vocabulary-context-and-handoffs.md)

## Quality/source policy

Canonical behavior comes from official specifications/project/vendor documentation.

```text
conceptual baseline
!= current upstream release
!= managed-cloud supported version
!= executable lab pin
```

Before production use, always re-check:

- runtime/provider compatibility;
- cloud region/SKU support;
- security advisories;
- limits/quotas;
- pricing.

See [Technology Baseline](docs/00-roadmap/technology-baseline.md) and [Source Policy](docs/00-roadmap/source-policy.md).

## CI quality gates

GitHub Actions verifies:

```text
Terraform lab
AI Engineering lab
portable docs paths
learning quality
roadmap/navigation/evidence consistency
diagram policy
mkdocs build --strict
critical generated pages
Pages deployment
live commit marker
```

## Repository philosophy

```text
read != learned
tool count != maturity
Kubernetes != DevOps
microservices != default architecture
private endpoint != authorization
AI agent != default workflow
diagram != production evidence
```

Strong evidence means you can **implement, break, debug, recover and justify the trade-off**.
