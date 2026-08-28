# Module 13 — DevOps, IaC & Delivery Engineering

> [← Docker](../12-docker/README.md) · [Testing & Code Review](../08-testing-code-review/README.md) · [Kubernetes →](../15-kubernetes/README.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;CI/CD · Artifacts · IaC · Delivery · Operations</span>
  <span><strong>Bridge</strong>&nbsp;Application → Platform → Kubernetes</span>
</div>

DevOps không phải một tool và cũng không đồng nghĩa với Kubernetes. Đây là tập hợp practices giúp team đưa một change từ source code đến production **nhanh hơn nhưng vẫn có evidence, kiểm soát failure và recovery**.

Roadmap.sh hiện mô tả DevOps theo các năng lực như development/scripting, automation, containerization, cloud, CI/CD, infrastructure management, monitoring/logging; Kubernetes là một công nghệ thường xuất hiện trong phần container orchestration/runtime của đường học DevOps.

```text
Development
   ↓
Testing / Review
   ↓
CI
   ↓
Artifact / Container
   ↓
Infrastructure as Code
   ↓
Deployment platform
   ├─ PaaS
   └─ Kubernetes
   ↓
Observability
   ↓
Incident / Recovery
```

<div class="key-takeaway" markdown>
<strong>Kubernetes thường đi cùng DevOps vì nó là runtime orchestration target.</strong>

Nhưng Kubernetes không thay thế CI/CD, IaC, testing, secrets, observability hay recovery. Và DevOps không bắt buộc phải dùng Kubernetes.
</div>

---

# 1. DevOps giải quyết vấn đề gì?

Nếu delivery phụ thuộc vào thao tác tay:

```text
build trên laptop
→ copy file
→ SSH server
→ sửa config
→ restart
→ hy vọng production ổn
```

thì team khó trả lời:

```text
artifact nào đang chạy?
change nào gây lỗi?
config production khác staging ở đâu?
rollback thế nào?
manual drift nào đã xảy ra?
release có được test đúng artifact không?
```

DevOps practices đưa những câu hỏi đó thành system có thể kiểm chứng:

```text
version control
+ automated tests
+ immutable artifacts
+ CI/CD
+ IaC
+ policy/security gates
+ observability
+ recovery/runbook
```

---

# 2. Dependency map — học theo lifecycle

```text
Linux / Networking / Git
        ↓
Testing & Code Review
        ↓
Docker / OCI images
        ↓
CI/CD + artifact promotion
        ↓
Infrastructure as Code
        ↓
Cloud / platform
        ↓
Kubernetes when justified
        ↓
Observability / Reliability / Incident response
```

Điều quan trọng: **Testing là đầu vào của delivery**, Docker là packaging/runtime boundary, IaC tạo platform, Kubernetes điều phối workloads.

---

# 3. DevOps vs Kubernetes

| DevOps concern | Kubernetes có giải quyết trực tiếp không? |
|---|---|
| source control | No |
| code review | No |
| unit/integration tests | No |
| CI build | No |
| artifact/image registry | No |
| cloud infrastructure provisioning | No |
| container scheduling | **Yes** |
| self-healing replica management | **Yes** |
| service discovery | **Yes** |
| workload rollout primitives | **Yes** |
| business SLO design | No |
| incident response | No |
| backup/restore of business data | No |

Do đó mental model đúng là:

```text
DevOps delivery system
        ↓
Kubernetes is one possible deployment/runtime platform
```

---

# 4. Core learning slices

| Guide | Priority | Bạn phải chứng minh được |
|---|---:|---|
| [CI/CD, Artifacts & Promotion](ci-cd-artifacts-and-promotion.md) | P0 | build once, immutable artifact, environment promotion |
| [Terraform State, Modules & Drift](terraform-state-modules-and-drift.md) | P0 | repeatable infra, state ownership, drift reasoning |
| [Safe Delivery, Drift & Recovery](safe-delivery-drift-and-recovery.md) | P0 | safe rollout, rollback/forward, operational recovery |
| [DevOps → Kubernetes Production Delivery](devops-kubernetes-production-delivery.md) | P0 | end-to-end pipeline from PR to Pods and recovery |

---

# 5. CI/CD mental model

```text
Pull Request
  ↓
compile / test / analyze
  ↓
artifact creation
  ↓
artifact identity
  ↓
non-prod deploy
  ↓
validation
  ↓
promotion
  ↓
production deploy
  ↓
health/SLO gate
```

Pipeline tốt phải phân biệt:

```text
CI = prove + package the change
CD = move a proven artifact through environments safely
```

Một pipeline dài không tự động là pipeline tốt. Gate phải bảo vệ contract/NFR cụ thể.

---

# 6. Build once, promote same artifact

Prefer:

```text
Git SHA abc123
→ image sha256:xyz
→ test
→ staging
→ production
```

Không rebuild production artifact sau khi staging đã test một artifact khác.

Evidence cần giữ:

```text
Git SHA
artifact/image digest
pipeline run
scan/test result
config revision
deployment revision
```

---

# 7. Infrastructure as Code

IaC không chỉ là "viết Terraform".

Bạn phải hiểu:

```text
desired infrastructure state
state ownership
plan/apply lifecycle
remote state
locking
module boundaries
provider/API versioning
drift
imports/migrations
secrets/identity
rollback/recovery
```

Cloud resource và Kubernetes object thường nên có ownership boundary rõ:

```text
Terraform / Bicep
→ cluster, network, IAM, registry, database, managed services

Kubernetes manifests / Helm / Kustomize
→ Deployment, Service, ConfigMap, HPA, NetworkPolicy...
```

Đừng để nhiều controllers cùng sở hữu một field mà không hiểu reconciliation.

---

# 8. Docker → Kubernetes bridge

Before Kubernetes:

```text
source code
→ Dockerfile
→ image
→ registry
→ container runtime behavior
```

Then Kubernetes:

```text
image
→ Pod
→ Deployment
→ Service
→ rollout/autoscale/reconcile
```

Các kiến thức bắt buộc trước K8s:

- process/signals/graceful shutdown;
- image layers/tags/digests;
- ports/networking;
- volumes/filesystem;
- CPU/memory limits;
- health endpoints;
- registry authentication;
- non-root/container security basics.

---

# 9. Kubernetes delivery

Khi deployment target là Kubernetes, DevOps path mở rộng thành:

```text
PR
→ tests
→ image build
→ image registry
→ deployment config
→ Kubernetes API
→ rollout
→ readiness
→ traffic
→ telemetry
→ recovery
```

Với GitOps:

```text
CI builds artifact
→ Git stores desired deployment config
→ Argo CD / Flux reconciles cluster
```

Xem full guide:

→ [DevOps → Kubernetes Production Delivery](devops-kubernetes-production-delivery.md)

---

# 10. Kubernetes không phải default cho mọi DevOps project

A small service may only need:

```text
GitHub Actions
→ App Service / Container Apps / VM
```

A larger platform may justify:

```text
GitHub Actions
→ registry
→ Terraform
→ Kubernetes
→ Helm/Kustomize
→ Argo CD/Flux
→ observability/policy stack
```

Tool count không phải maturity metric.

Decision phải dựa trên:

```text
workload count
team size
release frequency
scheduling/isolation needs
availability/recovery requirements
platform skill
cost/toil
```

---

# 11. Observability là feedback loop của delivery

Deploy thành công không có nghĩa release thành công.

Check:

```text
business outcome
API error rate
P95/P99 latency
queue backlog
DB dependency
Pod readiness/restarts
resource pressure
```

Production gate tốt có thể làm:

```text
rollout completes
but SLO degrades
→ stop promotion / rollback / roll forward
```

---

# 12. Security / supply chain

Security phải đi xuyên lifecycle:

```text
source
→ dependency
→ build
→ artifact/image
→ registry
→ deployment admission
→ runtime identity
```

Typical controls:

- branch protection / required reviews;
- dependency/image scanning;
- secret scanning;
- SBOM/provenance when required;
- least-privilege CI identity;
- short-lived/federated credentials;
- deployer identity != runtime workload identity;
- policy/admission controls where justified.

---

# 13. Recovery và rollback

Rollback phải reason theo nhiều lớp:

```text
application version
configuration
infrastructure
Kubernetes manifests
schema/data migration
external side effects
```

Example:

```text
kubectl rollout undo
```

có thể rollback image revision, nhưng không tự undo database destructive migration.

Safe delivery cần:

```text
expand/contract schema
feature flags where appropriate
progressive rollout
health gate
backup/restore
runbook
```

---

# 14. Learning path đề xuất

## Phase 0 — Foundations

```text
Linux
networking/DNS/TLS
Git
Bash/PowerShell/Python basics
```

## Phase 1 — Quality

```text
unit/integration/contract/load tests
code review
quality gates
```

→ [Module 08](../08-testing-code-review/README.md)

## Phase 2 — Containers

→ [Module 12 — Docker](../12-docker/README.md)

## Phase 3 — CI/CD

→ [CI/CD, Artifacts & Promotion](ci-cd-artifacts-and-promotion.md)

## Phase 4 — IaC

→ [Terraform State, Modules & Drift](terraform-state-modules-and-drift.md)

## Phase 5 — Kubernetes

→ [Module 15 — Kubernetes](../15-kubernetes/README.md)

## Phase 6 — End-to-end production delivery

→ [DevOps → Kubernetes Production Delivery](devops-kubernetes-production-delivery.md)

---

# 15. Project evidence

Một project hoàn thành DevOps + Kubernetes nên có:

```text
source repo
+ protected/reviewed change flow
+ automated tests
+ immutable container image
+ registry
+ IaC
+ Kubernetes manifests/Helm/Kustomize
+ CI
+ CD or GitOps
+ probes/resources/security config
+ dashboards/alerts
+ rollback/failure drill
```

Evidence tối thiểu:

1. PR quality checks.
2. Git SHA → image digest traceability.
3. IaC plan/apply from clean environment.
4. deployment config in version control.
5. failed rollout simulation.
6. Pod crash/readiness failure simulation.
7. recovery/rollback evidence.
8. ADR explaining why Kubernetes is or is not justified.

---

# 16. Exit criteria

Bạn hoàn thành Module 13 khi có thể:

- explain DevOps as lifecycle/practices rather than a product;
- connect Module 08 quality evidence into CI gates;
- build once and promote immutable artifacts;
- explain CI vs CD;
- manage IaC state and drift;
- separate platform IaC from workload configuration;
- explain why Kubernetes is a deployment/runtime target inside DevOps;
- trace source → artifact → deployment → runtime;
- design rollback/roll-forward with database compatibility;
- use observability to decide release health;
- explain when a simpler PaaS is better than Kubernetes.

## References

- [DevOps roadmap — roadmap.sh](https://roadmap.sh/devops)
- [Kubernetes roadmap — roadmap.sh](https://roadmap.sh/kubernetes)
- [Module references](references.md)

## Verification metadata

- Verified: 2026-08-28.
- roadmap.sh DevOps/Kubernetes scope checked against current 2026 pages.
- Official tool/platform documentation remains canonical for behavior and version-sensitive claims.
