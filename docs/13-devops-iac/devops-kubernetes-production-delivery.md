# DevOps → Kubernetes Production Delivery

> [← DevOps & IaC](README.md) · [Kubernetes →](../15-kubernetes/README.md) · [Testing & Code Review](../08-testing-code-review/README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;CI/CD · Containers · IaC · Kubernetes · GitOps</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Goal</strong>&nbsp;build → test → package → provision → deploy → observe → recover</span>
</div>

DevOps và Kubernetes thường xuất hiện cùng nhau vì Kubernetes giải quyết **runtime orchestration**, còn DevOps giải quyết **software delivery + infrastructure automation + operations lifecycle**.

Nhưng hai khái niệm không đồng nghĩa:

```text
DevOps
= collaboration + automation + delivery + operations practices

Kubernetes
= container orchestration platform
```

Kubernetes không thay CI/CD, testing, IaC, security, observability hay incident response. DevOps cũng không bắt buộc phải dùng Kubernetes.

Roadmap.sh mô tả DevOps theo các năng lực như development/scripting, automation, containerization, cloud, CI/CD, infrastructure management, monitoring/logging; Kubernetes là một công nghệ orchestration nằm trong đường học đó. Kubernetes roadmap tập trung sâu hơn vào deployment, scaling, networking và operations của containerized workloads.

- DevOps roadmap: <https://roadmap.sh/devops>
- Kubernetes roadmap: <https://roadmap.sh/kubernetes>

---

# 1. Mental model — một production change đi qua những boundary nào?

```text
Developer change
   ↓
Pull Request
   ↓
Quality gates
   ├─ unit tests
   ├─ integration/contract tests
   ├─ static/security checks
   └─ review
   ↓
CI build
   ↓
Immutable artifact / container image
   ↓
Registry
   ↓
Infrastructure / platform configuration
   ├─ Terraform / Bicep
   └─ cluster/platform dependencies
   ↓
Application deployment config
   ├─ raw manifests
   ├─ Kustomize
   └─ Helm
   ↓
Continuous Delivery / GitOps
   ↓
Kubernetes API
   ↓
Deployment / Service / Config / Policy
   ↓
Pods
   ↓
Observe SLI/SLO + logs + metrics + traces
   ↓
Rollback / roll forward / incident response
```

Nếu một team chỉ có:

```text
git push
→ docker build
→ kubectl apply
```

thì team có thể đang dùng Kubernetes nhưng **chưa có production delivery system tốt**.

---

# 2. Testing là đầu pipeline, không phải bước tách rời DevOps

Module 08 tạo evidence để CI/CD quyết định một change có được promote hay không.

```text
Code
 ↓
Unit test
 ↓
Integration / contract test
 ↓
Security / quality check
 ↓
Build artifact
```

Quality gate nên trả lời:

```text
Is the code buildable?
Are business invariants still correct?
Are API/contracts compatible?
Is the artifact traceable?
Did security checks pass?
Can the release be rolled back or rolled forward safely?
```

Không nên biến pipeline thành danh sách check không có semantics:

```text
"test passed"
```

mà không biết test bảo vệ contract nào.

→ [Module 08 — Testing & Code Review](../08-testing-code-review/README.md)

---

# 3. Source control và change model

Production delivery bắt đầu từ version control.

Một change nên trace được:

```text
Issue / requirement
→ Git commit
→ Pull Request
→ CI run
→ artifact digest
→ deployment revision
→ runtime version
```

Minimum controls:

- protected main branch where appropriate;
- required checks;
- review ownership;
- no production secrets in repository;
- reproducible build inputs;
- deployment change history.

Git history không chỉ để collaboration; nó là một phần của audit/recovery path.

---

# 4. CI — build once, promote the same artifact

Một pipeline tốt tránh rebuild source khác nhau cho từng environment.

Prefer:

```text
commit abc123
  ↓
build once
  ↓
image digest sha256:xyz
  ↓
non-prod
  ↓ promote same digest
production
```

Avoid:

```text
build dev image
build staging image
build production image
```

vì bạn đã thay artifact giữa các gate.

Container example:

```text
Git SHA
→ docker build
→ tests/scans
→ registry/repository:version
→ immutable digest
```

Release evidence nên giữ:

```text
source SHA
image digest
SBOM / scan result where required
build logs
config revision
deployment timestamp
```

---

# 5. Docker/OCI là bridge trước Kubernetes

Kubernetes scheduler không build source code cho bạn. Nó chạy workloads từ container images đã có.

Learning order:

```text
application process
→ Dockerfile
→ image layers
→ registry
→ container runtime behavior
→ Kubernetes Pod
```

Bạn phải hiểu trước:

```text
entrypoint / command
ports
filesystem
signals / graceful shutdown
CPU / memory
health endpoint
non-root execution
image provenance
```

Nếu container behavior không rõ, Kubernetes chỉ làm failure khó debug hơn.

→ [Module 12 — Docker](../12-docker/README.md)

---

# 6. IaC và Kubernetes manifests giải quyết hai lớp khác nhau

Một lỗi phổ biến là dùng một tool để sở hữu mọi resource.

Useful boundary:

```text
Terraform / Bicep
→ cloud/platform infrastructure
→ VNet/VPC, cluster, registry, databases, IAM, DNS, managed services

Kubernetes manifests / Helm / Kustomize
→ workload objects inside cluster
→ Deployment, Service, ConfigMap, HPA, NetworkPolicy...
```

Không phải hard rule, nhưng ownership phải rõ.

Questions:

```text
Who owns this resource?
Which state system owns it?
How is drift detected?
How is rollback/recovery done?
Can two controllers fight over the same object?
```

Avoid:

```text
Terraform owns Deployment replicas
+
HPA owns Deployment replicas
```

mà không hiểu field ownership/lifecycle.

---

# 7. Helm và Kustomize nằm ở đâu?

Raw YAML phù hợp để học object model nhưng production config thường cần composition.

## Kustomize mental model

```text
base manifests
+ environment overlays/patches
→ rendered Kubernetes resources
```

Tốt khi muốn giữ manifest gần Kubernetes API và override có cấu trúc.

## Helm mental model

```text
Chart templates
+ values
→ rendered Kubernetes resources
```

Helm là package manager cho Kubernetes; chart đóng gói application resources và hỗ trợ release-oriented workflow.

Không dùng template engine để che Kubernetes semantics. Team vẫn phải đọc được rendered output.

Useful validation:

```bash
helm template ...
kubectl diff -f ...
kubectl apply --dry-run=server -f ...
```

Current Helm documentation: <https://helm.sh/docs/>

---

# 8. CD push model vs GitOps pull model

## Push-based deployment

```text
CI/CD runner
  ↓ credentials
kubectl / Helm
  ↓
cluster
```

Simple and effective for many teams.

Risk surface:

- CI runner needs cluster access;
- deployment state may live mostly in pipeline history;
- manual changes can drift from Git.

## GitOps-style pull/reconciliation

```text
CI
→ build image
→ update desired config in Git

Git
  ↓
Argo CD / Flux controller in or near cluster
  ↓ reconcile
Kubernetes API
```

Argo CD describes itself as a declarative GitOps continuous delivery tool for Kubernetes. Flux exposes controllers/APIs for building continuous delivery on Kubernetes.

GitOps advantages:

```text
Git = desired deployment state
audit trail
continuous drift detection/reconciliation
separation between CI build and CD reconciliation
```

But GitOps adds:

```text
controller lifecycle
RBAC/policy
secret/config strategy
multi-environment repo design
sync semantics
incident/runbook complexity
```

Do not adopt GitOps because it is fashionable; adopt it when declarative reconciliation and auditability justify another controller layer.

References:

- <https://argo-cd.readthedocs.io/en/stable/>
- <https://fluxcd.io/flux/>

---

# 9. Reference pipeline — .NET API to Kubernetes

Example:

```text
Pull Request
  ↓
dotnet restore/build/test
  ↓
integration + contract tests
  ↓
container build
  ↓
security / dependency / image scan
  ↓
push immutable image to registry
  ↓
update deployment config with image digest
  ↓
non-prod deployment
  ↓
smoke/integration checks
  ↓
promotion approval / policy
  ↓
production rollout
  ↓
SLO/error gate
  ├─ healthy → continue
  └─ unhealthy → stop / rollback / roll forward
```

Runtime:

```text
Ingress/Gateway
  ↓
Service
  ↓
Deployment
  ↓
Ready Pods
  ↓
database / queue / external dependencies
```

Traceability:

```text
request trace
→ app version
→ image digest
→ deployment revision
→ Git SHA
```

---

# 10. Kubernetes rollout is part of DevOps delivery, not the whole thing

Kubernetes supports declarative rollout behavior for workloads such as Deployments, but production release still needs:

```text
artifact correctness
configuration compatibility
database migration compatibility
capacity during rollout
readiness semantics
rollback decision
business telemetry
```

Bad assumption:

```text
Deployment rolling update
= zero-downtime release
```

Reality:

```text
new Pod starts
→ startup succeeds?
→ readiness means what?
→ dependencies compatible?
→ database schema compatible?
→ enough node capacity for surge?
→ user-visible SLO healthy?
```

`kubectl rollout undo` cannot reverse database side effects automatically.

---

# 11. Autoscaling is an operations feedback loop

Kubernetes can scale workloads, but DevOps/platform design decides the metrics and bounds.

```text
load
→ metric
→ HPA/KEDA/controller
→ replicas
→ scheduler
→ node capacity
→ application throughput
```

Kubernetes HPA adjusts workload replicas based on configured metrics; resource-utilization scaling depends on resource requests being meaningful.

Do not set:

```text
HPA CPU 60%
```

without checking:

```text
CPU requests
startup behavior
traffic model
queue backlog
dependency quota
node autoscaling delay
max replicas
cost ceiling
```

Official HPA docs: <https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/>

---

# 12. Secrets and identity across CI → cluster → cloud

There are at least three identities:

```text
CI identity
→ can build/push/provision/deploy

CD/GitOps controller identity
→ can reconcile allowed cluster resources

Runtime workload identity
→ application accesses database/queue/cloud APIs
```

Do not reuse one admin credential for all three.

Prefer short-lived/federated credentials where platform supports them.

Secret lifecycle questions:

```text
where is source of truth?
who can read/write?
how rotated?
how injected?
how audited?
what happens during rotation failure?
```

Kubernetes Secret is an API object, not a complete enterprise secret-management strategy.

---

# 13. Policy and supply-chain controls

A production pipeline may require:

```text
branch protection
required test checks
dependency scanning
container image scanning
SBOM/provenance
signed/approved artifacts
admission policy
namespace/RBAC boundaries
NetworkPolicy
runtime security controls
```

The important architecture idea is **where enforcement occurs**:

```text
before artifact creation
before registry promotion
before deployment
at Kubernetes admission
at runtime
```

Do not duplicate every control everywhere; choose meaningful gates and keep failure messages actionable.

---

# 14. Observability closes the DevOps loop

Deployment success does not mean user success.

Observe:

```text
Business
- checkout completion
- payment success/unknown

Application
- request success/error
- P95/P99 latency
- dependency latency

Kubernetes
- Pod readiness/restarts
- Pending Pods
- CPU/memory
- HPA state
- node pressure

Delivery
- deployment frequency
- failed deployment rate
- rollback/restore duration
```

Release decision should combine deployment state with user-visible evidence.

Example:

```text
rollout complete
but checkout success drops
→ release is unhealthy
```

---

# 15. DevOps metrics vs Kubernetes metrics

Kubernetes metrics answer platform/runtime questions.

```text
Pods available?
CPU/memory?
restarts?
pending workloads?
```

DevOps delivery metrics answer process/outcome questions.

```text
How often can we deploy safely?
How long from commit to production?
How often do changes fail?
How quickly can service recover?
```

Do not optimize cluster utilization while deployment lead time and recovery remain poor, or vice versa.

---

# 16. Incident path — delivery system must be operable

A release incident runbook should answer:

```text
What changed?
Which Git SHA/image digest/config revision?
Which workloads are affected?
Is problem app/config/platform/dependency?
Can traffic be reduced/drained?
Rollback safe?
Database migration reversible/compatible?
Can we roll forward faster?
What evidence confirms recovery?
```

Useful commands/evidence:

```bash
kubectl rollout status deployment/<name>
kubectl rollout history deployment/<name>
kubectl get pods -o wide
kubectl describe pod <pod>
kubectl logs <pod> --previous
kubectl get events --sort-by=.lastTimestamp
```

The goal is not memorizing commands; it is shortening diagnosis and recovery with traceable state.

---

# 17. Learning path — DevOps + Kubernetes together

## Phase 0 — Engineering foundations

```text
Linux
processes/signals
networking/DNS/HTTP/TLS
Git
basic scripting
```

## Phase 1 — Quality engineering

```text
unit tests
integration tests
contract tests
load tests
code review
quality gates
```

→ [Module 08](../08-testing-code-review/README.md)

## Phase 2 — Containers

```text
Dockerfile
image layers
registry
runtime resources
network/storage
security
```

→ [Module 12](../12-docker/README.md)

## Phase 3 — CI/CD + artifacts

```text
GitHub Actions / Azure Pipelines / GitLab CI / Jenkins concepts
build once
immutable artifacts
promotion
environments
approvals
rollback
```

→ [CI/CD, Artifacts & Promotion](ci-cd-artifacts-and-promotion.md)

## Phase 4 — Infrastructure as Code

```text
Terraform/Bicep
remote state
modules
drift
plan/apply
identity
network/cloud prerequisites
```

→ [Terraform State, Modules & Drift](terraform-state-modules-and-drift.md)

## Phase 5 — Kubernetes fundamentals

```text
API objects
controllers
Pods
Deployments
Services
ConfigMaps/Secrets
storage
networking
security
```

→ [Module 15 — Kubernetes](../15-kubernetes/README.md)

## Phase 6 — Kubernetes delivery

```text
Helm / Kustomize
immutable image references
CI → CD
GitOps where justified
progressive rollout
migration compatibility
```

## Phase 7 — Operations

```text
metrics/logs/traces
SLO/alerts
capacity/autoscaling
incident response
backup/restore
upgrade/recovery
```

---

# 18. Project — production-grade delivery evidence

Build one small service and prove the entire path:

```text
.NET API
+ unit/integration tests
+ Docker image
+ registry
+ Terraform/Bicep environment
+ Kubernetes Deployment/Service
+ ConfigMap/Secret strategy
+ readiness/liveness/startup probes
+ resource requests/limits
+ CI pipeline
+ CD or GitOps deployment
+ observability
+ rollback/failure drill
```

Required evidence:

1. Pull Request with quality gates.
2. CI output linking commit → artifact digest.
3. IaC plan and repeatable environment.
4. Deployment configuration stored in Git.
5. Runtime version traceable to source commit.
6. Failed rollout experiment.
7. Pod crash/readiness failure experiment.
8. Rollback or roll-forward evidence.
9. Dashboard/metrics proving recovery.
10. ADR explaining why Kubernetes is justified over a simpler platform.

---

# 19. When NOT to combine everything

Do not force the full stack onto every project.

For a small internal API:

```text
GitHub Actions
→ App Service / Container Apps
```

may be better than:

```text
GitHub Actions
→ Terraform
→ AKS
→ Helm
→ Argo CD
→ service mesh
```

Choose the smallest delivery/platform system that meets:

```text
reliability
security
scale
team capability
recovery
cost
```

DevOps maturity is not measured by tool count.

---

# 20. Exit criteria

Bạn hiểu mối quan hệ DevOps + Kubernetes khi có thể:

- explain why Kubernetes is one runtime platform inside a broader DevOps lifecycle;
- connect testing/code review to CI quality gates;
- build once and promote immutable artifacts;
- separate cloud IaC ownership from Kubernetes workload configuration;
- explain Helm/Kustomize purpose without hiding Kubernetes object semantics;
- compare push CD and GitOps pull/reconciliation;
- trace Git SHA → image digest → deployment revision → running Pods;
- design runtime/deployer identities separately;
- connect rollout health to SLO/business telemetry;
- debug a failed Kubernetes deployment from pipeline to Pod;
- describe rollback/roll-forward and database migration constraints;
- explain when a simpler PaaS deployment is better than Kubernetes.

## Source policy

- Kubernetes behavior/API: <https://kubernetes.io/docs/>
- Helm: <https://helm.sh/docs/>
- Argo CD: <https://argo-cd.readthedocs.io/en/stable/>
- Flux: <https://fluxcd.io/flux/>
- DevOps breadth/learning discovery: <https://roadmap.sh/devops>
- Kubernetes breadth/learning discovery: <https://roadmap.sh/kubernetes>

Roadmap/community sources help with **coverage and learning order**; normative behavior remains in official project/platform documentation.

## Verification metadata

- Verified: 2026-08-28.
- roadmap.sh DevOps/Kubernetes pages checked for current 2026 learning scope.
- Claude artifact URL supplied by the learner could not be fetched from this environment, so no content was inferred or copied from it.
