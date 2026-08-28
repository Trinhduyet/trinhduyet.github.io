# Azure Kubernetes Service (AKS) — Production Architecture

> [← Cloud & Azure](README.md) · [Kubernetes fundamentals](../15-kubernetes/README.md) · [Azure Reliability & Operations →](azure-reliability-observability-governance-and-cost.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;AKS · Platform Architecture</span>
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Audience</strong>&nbsp;.NET Backend → Platform/Senior/Architect</span>
  <span><strong>Goal</strong>&nbsp;production Kubernetes on Azure, not YAML memorization</span>
</div>

AKS là managed Kubernetes trên Azure. Azure quản lý Kubernetes control plane ở mức service, nhưng **bạn vẫn sở hữu rất nhiều quyết định platform và workload**: networking, node pools, policy, identities, resource requests/limits, probes, deployment strategy, upgrades, observability, cost và failure recovery.

<div class="key-takeaway" markdown>
<strong>AKS không phải “Container Apps nhưng mạnh hơn”.</strong>

AKS cho nhiều control hơn vì bạn dùng trực tiếp Kubernetes API và ecosystem. Control đó có giá trị khi workload/platform thực sự cần nó; nếu không, nó trở thành operating burden.
</div>

---

# 1. Khi nào AKS là lựa chọn hợp lý?

## Strong signals cho AKS

```text
Need direct Kubernetes API
Need custom operators/controllers
Need fine-grained scheduling / node pools
Need namespace-level policy/isolation
Need Kubernetes-native ecosystem/tooling
Need advanced cluster networking controls
Need to host many workloads on a standardized platform
Organization already has platform/SRE capability
```

## Weak signals — chưa đủ để justify AKS

```text
"production phải Kubernetes"
"microservices thì phải K8s"
"team muốn học K8s"
"container = AKS"
"sợ vendor lock-in"
```

Nếu App Service hoặc Container Apps đáp ứng NFR với ít operating surface hơn, chúng có thể là architecture tốt hơn.

---

# 2. AKS Automatic hay AKS Standard?

Microsoft hiện có hai operating models chính:

| Option | Mental model | Phù hợp khi |
|---|---|---|
| **AKS Automatic** | opinionated Kubernetes platform với nhiều production defaults được Azure cấu hình/quản lý sẵn | workload phổ biến, muốn Kubernetes API nhưng giảm số decision day-0/day-2 |
| **AKS Standard** | managed control plane nhưng bạn kiểm soát nhiều cluster/network/node configuration hơn | cần specialized topology, custom networking/operations hoặc platform requirements cụ thể |

Đừng chọn Standard chỉ vì “nhiều control hơn = tốt hơn”. Mỗi control thêm vào là một decision phải được owner, test và vận hành.

Architecture review nên ghi rõ:

```text
Why AKS?
Why Automatic or Standard?
Which decisions remain owned by workload/platform team?
What operational capability exists today?
```

---

# 3. Shared responsibility — Azure quản lý gì, team quản lý gì?

Mental model đơn giản:

```text
Azure-managed AKS service
├─ Kubernetes control plane service
├─ integration with Azure platform capabilities
└─ managed add-ons/features depending on configuration

Your platform/workload responsibility
├─ cluster architecture choices
├─ node pool capacity and lifecycle
├─ workload manifests / Helm / GitOps configuration
├─ requests / limits / probes
├─ namespaces / RBAC / policy
├─ ingress / egress design
├─ secrets / workload identity
├─ application data correctness
├─ deployment compatibility
├─ observability / SLO / alerts
├─ backup / restore / DR
└─ cost / incident response / upgrade validation
```

Managed Kubernetes giảm control-plane toil. Nó **không biến Kubernetes thành zero-ops**.

---

# 4. Production baseline — nhìn toàn hệ thống trước cluster

Một AKS workload production có thể có topology:

```text
Users
  │
  ▼
Azure Front Door + WAF
  │
  ▼
Regional ingress / gateway
  │
  ▼
┌─────────────────────────────────────────────────────────┐
│ AKS                                                     │
│                                                         │
│ System node pool                                        │
│   └─ cluster/system components                          │
│                                                         │
│ User node pool(s)                                       │
│   ├─ namespace: checkout                                │
│   │    ├─ Deployment checkout-api                       │
│   │    ├─ Service checkout-api                          │
│   │    ├─ Deployment payment-worker                     │
│   │    └─ HPA / KEDA                                    │
│   │                                                     │
│   └─ namespace: catalog                                 │
│        └─ ...                                            │
│                                                         │
│ NetworkPolicy · RBAC · Pod Security · telemetry         │
└─────────────────────────────────────────────────────────┘
   │             │              │              │
   │             │              │              │
   ▼             ▼              ▼              ▼
Azure SQL   Service Bus      Key Vault        Blob
   ▲             ▲              ▲              ▲
   └─────────────┴──── Workload Identity ──────┘

Images
  ▲
  │
Azure Container Registry

Telemetry
  │
  ▼
Azure Monitor / Application Insights / managed metrics/logs
```

AKS là **compute/orchestration layer**, không phải toàn bộ cloud architecture.

---

# 5. Cluster topology — system và user workloads

Production cluster thường phải phân biệt system workload và application workload.

```text
AKS cluster
├─ system node pool
│   └─ system-critical cluster components
│
└─ user node pool(s)
    ├─ general-purpose APIs/workers
    ├─ compute-heavy pool nếu justify
    └─ specialized/GPU/Windows pool nếu workload cần
```

Không tạo nhiều node pools chỉ để “trông enterprise”. Mỗi pool tạo thêm:

```text
capacity planning
upgrade surface
cost
scheduling complexity
quota/IP demand
operational runbooks
```

Use cases có thể justify separate pools:

- specialized hardware;
- Linux vs Windows workload;
- different isolation/security requirement;
- very different scaling behavior;
- system vs user workload separation;
- taints/tolerations/scheduling constraints có business reason.

---

# 6. Kubernetes object model vẫn là nền tảng

Trên AKS, application vẫn là Kubernetes workload:

```text
Deployment
    ↓
ReplicaSet
    ↓
Pods
    ↓
Containers
```

Networking:

```text
External ingress
    ↓
Service
    ↓ selector
Pods
```

Configuration:

```text
ConfigMap / Secret / external secret integration
    ↓
Pod
```

Storage:

```text
Pod
 ↓ PVC
PersistentVolume
 ↓
Azure storage integration
```

Nếu chưa giải thích được Deployment → ReplicaSet → Pod hoặc Service selector hoạt động thế nào, quay lại [Module 15 — Kubernetes](../15-kubernetes/README.md).

---

# 7. Networking — quyết định khó đảo ngược

Networking là phần dễ tạo technical debt lớn nhất trong AKS.

Các câu hỏi phải trả lời trước khi tạo cluster:

```text
Cluster public hay private?
API server access từ đâu?
Pod IP model là gì?
VNet/subnet/IP capacity bao nhiêu?
Ingress path là gì?
Egress đi qua đâu?
Private dependencies resolve DNS thế nào?
NetworkPolicy có cần deny-by-default không?
Cross-namespace traffic được kiểm soát thế nào?
```

## 7.1 Traffic path

Ví dụ:

```text
Internet
  ↓
Front Door + WAF
  ↓
regional ingress/gateway
  ↓
Kubernetes Service
  ↓
Ready Pods
```

Không expose mọi Service bằng public LoadBalancer chỉ vì YAML cho phép.

## 7.2 Production networking model

Microsoft AKS microservices reference architecture hiện khuyến nghị **Azure CNI powered by Cilium** cho production microservices baseline nhờ performance, network policy enforcement và eBPF-based observability capabilities.

Điều này không có nghĩa mọi cluster phải copy một config. Bạn vẫn phải validate:

```text
IP planning
network policy model
VNet integration
ingress/egress requirements
private endpoint/DNS interaction
upgrade compatibility
team operating capability
```

## 7.3 NetworkPolicy

Mental model:

```text
Namespace / Pod reachable by default?
        ↓
Define intended flows
        ↓
Enforce with policy
        ↓
Test allowed + denied paths
```

Policy chỉ có giá trị khi test được traffic graph; một file YAML “deny all” không phải evidence nếu app vẫn cần dozens of hidden flows.

---

# 8. Identity — Pod không nên dùng shared static credentials

Trên Azure, workload identity nên là default mental model cho cloud access khi supported.

```text
Pod / ServiceAccount
      ↓ federation
Microsoft Entra Workload Identity
      ↓
Azure resource authorization
      ├─ Key Vault
      ├─ Storage
      ├─ Service Bus
      └─ other supported Azure resources
```

Benefits:

```text
no shared connection string in Git
no long-lived client secret in Kubernetes Secret
least privilege per workload
rotation burden reduced
identity auditable
```

Still required:

- correct RBAC/data-plane permissions;
- namespace/service-account ownership;
- least privilege;
- separation giữa deployer identity và runtime identity;
- external providers vẫn có thể cần actual API secrets.

<div class="key-takeaway" markdown>
<strong>Secret object không phải secret-management strategy.</strong>

Kubernetes Secret giải quyết distribution/storage abstraction trong cluster. Production secret architecture còn gồm identity, encryption, RBAC, external vault, rotation và audit.
</div>

---

# 9. Container images — artifact phải immutable

Pipeline tốt:

```text
Git commit
   ↓
build + test
   ↓
container image
   ↓
security scan / policy
   ↓
Azure Container Registry
   ↓
immutable digest
   ↓
AKS deployment
```

Prefer:

```text
image: myacr.azurecr.io/checkout@sha256:...
```

thay vì production phụ thuộc vào mutable tag:

```text
image: myacr.azurecr.io/checkout:latest
```

Release phải trace được:

```text
Git SHA
→ image digest
→ deployment version
→ runtime Pods
```

---

# 10. Requests, limits và scheduling

Kubernetes scheduler không đọc dashboard của bạn để đoán workload cần bao nhiêu CPU/RAM.

```yaml
resources:
  requests:
    cpu: 300m
    memory: 384Mi
  limits:
    memory: 768Mi
```

Requests ảnh hưởng scheduling/capacity planning. Limits ảnh hưởng runtime behavior tùy resource semantics.

Không copy numbers từ tutorial. Đo:

```text
normal CPU/memory
peak
.NET GC behavior
P95/P99 latency
OOM events
node allocatable capacity
replica count
rollout surge
```

Capacity reasoning:

```text
node allocatable
- system overhead
- DaemonSets
- requests from existing workloads
= schedulable capacity
```

Nếu rollout tạo thêm replicas tạm thời, cluster phải còn capacity để rollout **không deadlock**.

---

# 11. Autoscaling — Pod scale và Node scale là hai bài toán

## HPA / workload scaling

```text
traffic / CPU / custom metric
       ↓
Horizontal Pod Autoscaler
       ↓
more/fewer Pods
```

## Event-driven scaling

KEDA có thể phù hợp với queue/event-driven worker patterns.

```text
Service Bus queue depth
       ↓
KEDA scaler
       ↓
worker replicas
```

## Cluster/node scaling

Nếu Pods Pending vì không còn capacity:

```text
pending Pods
    ↓
node autoscaling logic
    ↓
more nodes
    ↓
scheduler places Pods
```

Hai loops khác nhau.

Bad architecture:

```text
HPA increases Pods fast
but node capacity grows slowly
→ Pods Pending
→ latency/backlog continues rising
```

Scale design phải có **end-to-end timing model**.

---

# 12. Probes — health semantics phải đúng

Readiness:

```text
"Pod này có nên nhận traffic mới không?"
```

Liveness:

```text
"Process bị stuck tới mức restart hữu ích không?"
```

Startup:

```text
"App startup lâu; đừng để liveness kill quá sớm"
```

Avoid:

```text
liveness → SQL dependency
SQL outage
→ every Pod becomes unhealthy
→ mass restarts
→ cluster churn
→ SQL vẫn outage
```

External dependency outage thường nên làm **readiness/degradation/business flow decision**, không tự động biến thành process death.

---

# 13. Rollout — app rollback không rollback database

Typical flow:

```text
new Deployment revision
→ new Pods start
→ readiness passes
→ traffic shifts
→ old Pods drain/terminate
```

Database migration phải support coexistence nếu old/new versions cùng chạy.

Use expand/contract:

```text
1. add compatible schema
2. deploy app that supports old + new
3. migrate/backfill
4. switch reads/writes
5. remove old schema later
```

Không deploy destructive migration cùng lúc với rolling Deployment rồi kỳ vọng `kubectl rollout undo` cứu mọi thứ.

---

# 14. Reliability — Kubernetes self-healing có giới hạn

Kubernetes có thể:

```text
restart failed container
replace deleted Pod
reschedule workload
maintain replica count
```

Nó không tự giải quyết:

```text
bad application version
logical data corruption
non-idempotent retry
duplicate message
external payment unknown outcome
wrong database migration
misconfigured NetworkPolicy
bad secret rotation
regional data loss
```

Self-healing = **reconcile infrastructure/workload state**, không phải business correctness.

---

# 15. Availability — Pod, Node, Zone, Region là các failure domains khác nhau

Reason từ nhỏ tới lớn:

```text
container crash
↓
Pod failure
↓
node failure
↓
zone failure
↓
region failure
```

Mỗi level cần control khác nhau.

## Pod / node

- multiple replicas;
- anti-affinity/topology spread khi justified;
- PodDisruptionBudget;
- enough cluster capacity;
- correct probes;
- graceful shutdown.

## Zone

- zone-aware node topology khi region/service supports;
- spread replicas;
- dependencies cũng phải có zone strategy;
- test planned disruptions.

## Region

Multi-region AKS không phải “bật một switch”. Nó thường là:

```text
Region A AKS cluster
+ Region B AKS cluster
+ global routing
+ replicated/recoverable data
+ messaging strategy
+ callback/webhook routing
+ deployment coordination
+ failover runbook
+ reconciliation
```

Chỉ làm khi RTO/RPO/SLO justify cost và complexity.

---

# 16. Observability — nhìn từ user outcome tới cluster

## Application/business layer

```text
checkout_accepted_total
payment_unknown_total
order_completion_latency
HTTP P95/P99
error rate
```

## Kubernetes workload layer

```text
Pod restarts
NotReady Pods
Pending Pods
OOMKilled
replica availability
HPA saturation
rollout failures
```

## Node/cluster layer

```text
node CPU/memory pressure
allocatable vs requested
node unavailable
pod scheduling failures
network errors
API/cluster health signals
```

## Dependency layer

```text
SQL latency/connections
Service Bus queue depth
oldest message age
DLQ
Key Vault errors
external provider latency/error
```

Trace nên nối:

```text
HTTP request
→ service
→ OrderId
→ message id
→ worker
→ downstream dependency
```

Đừng biến Kubernetes metrics thành mục tiêu cuối. User không quan tâm node CPU 70% nếu checkout đang fail.

---

# 17. Security baseline

Một review tối thiểu nên nhìn:

```text
Entra integration / cluster access
Kubernetes RBAC
Workload Identity
Pod Security controls
NetworkPolicy
private/public API and ingress exposure
image provenance/scanning
secrets and Key Vault
Azure Policy / admission controls
node OS / cluster upgrades
Defender/runtime signals where required
logging/auditing
```

## Separate identities

```text
CI/CD deployer identity
!=
cluster admin identity
!=
application runtime identity
```

Least privilege phải tồn tại ở cả Azure RBAC và Kubernetes RBAC/data plane.

---

# 18. Cost model — AKS có idle platform cost

PaaS thường scale theo app abstraction hơn. AKS có node infrastructure mà team phải capacity-plan.

Cost drivers:

```text
node count / VM SKU
idle headroom
system node pool
overprovisioning for rollout/HA
logs/metrics ingestion
load balancers/public IPs
managed disks
NAT/egress
cross-zone/cross-region traffic
ACR/storage
non-production clusters
```

Architecture question:

```text
Do we need a dedicated cluster per workload?
Can workloads safely share a platform?
What isolation boundary is required?
Can non-prod scale down?
What is cost per workload/team/business unit?
```

Namespace không phải billing boundary tự động; bạn cần tagging/allocation/metrics strategy phù hợp.

---

# 19. Checkout trên AKS — mapping từ architecture sang Kubernetes

Application architecture:

```text
Checkout API
  ↓
Azure SQL
  ↓ outbox
Service Bus
  ↓
Payment Worker
  ↓
External Provider
```

Kubernetes mapping:

```text
namespace checkout
├─ Deployment checkout-api
│   ├─ 3 replicas
│   ├─ readiness/liveness/startup probes
│   ├─ requests/limits
│   └─ ServiceAccount → Workload Identity
│
├─ Service checkout-api
│
├─ Deployment payment-worker
│   ├─ replicas controlled by queue demand
│   ├─ idempotent consumer
│   └─ graceful shutdown
│
├─ NetworkPolicy
├─ ConfigMap
└─ PodDisruptionBudget
```

External managed services remain outside cluster:

```text
Azure SQL
Service Bus
Key Vault
Blob Storage
Managed Redis
```

Đừng deploy database/message broker vào Kubernetes chỉ để “mọi thứ nằm trong cluster” nếu managed Azure service đáp ứng tốt hơn.

---

# 20. AKS implementation sequence cho dự án thật

## Phase 1 — prove Kubernetes need

Artifact:

```text
ADR: Why AKS instead of App Service/Container Apps?
```

## Phase 2 — cluster/platform design

Define:

```text
Automatic vs Standard
region / zone strategy
private/public API access
VNet/subnet/IP plan
node pools
identity model
ingress/egress
policy baseline
logging/metrics
upgrade strategy
```

Artifact: **platform architecture diagram + ADRs**.

## Phase 3 — workload contract

Define:

```text
namespace
ServiceAccount / Workload Identity
Deployment/Service
requests/limits
probes
autoscaling
NetworkPolicy
PDB
config/secrets
```

Artifact: Helm/Kustomize/manifests + review checklist.

## Phase 4 — delivery

```text
commit
→ test
→ image
→ scan
→ push ACR
→ deploy non-prod
→ smoke/integration
→ progressive rollout
→ SLO gate
→ promote/rollback
```

Artifact: CI/CD pipeline + immutable release evidence.

## Phase 5 — operations

Run drills:

```text
delete Pod
cordon/drain node
break readiness
set bad image
simulate OOM
block dependency
queue backlog
rotate identity/secret
upgrade test environment
restore data dependency
```

Artifact: dashboards + alerts + runbooks + drill result.

---

# 21. Failure review table

| Failure | Kubernetes behavior | Architecture responsibility |
|---|---|---|
| container crash | restart/recreate | root-cause app failure, prevent crash loop |
| Pod deleted | controller replaces | ensure capacity + state externalized |
| node unavailable | reschedule where possible | replicas, spread, capacity, PDB |
| bad image | rollout stalls/fails | pipeline gate + rollback |
| readiness broken | Pod removed from ready endpoints | diagnose app/dependency/config |
| OOMKilled | process terminated | requests/limits + memory/GC tuning |
| SQL unavailable | Pods may still run | degrade/retry/backoff; don't restart fleet blindly |
| duplicate message | consumer receives again | business idempotency/dedup |
| payment timeout | result may be unknown | reconciliation/business state machine |
| cluster upgrade issue | workload disruption possible | version compatibility + maintenance/runbook |
| zone outage | subset infra lost | spread + dependency HA |
| region outage | cluster unavailable | multi-region/restore plan if requirement exists |

---

# 22. Production checklist

Trước khi approve AKS workload:

1. Requirement nào chỉ Kubernetes mới đáp ứng hợp lý?
2. Automatic hay Standard, vì sao?
3. Cluster owner/platform owner là ai?
4. Upgrade policy + maintenance window + test strategy là gì?
5. Node pool topology có business reason không?
6. IP/subnet capacity tính cả rollout/autoscale chưa?
7. API server/ingress public exposure có tối thiểu chưa?
8. Egress được kiểm soát và observable chưa?
9. Workload dùng Entra Workload Identity khi phù hợp chưa?
10. Kubernetes RBAC và Azure RBAC có least privilege không?
11. Requests/limits dựa trên measurement chưa?
12. Readiness/liveness/startup probe có đúng semantics không?
13. HPA/KEDA + node scaling có end-to-end timing model không?
14. PDB/topology spread có phù hợp failure model không?
15. NetworkPolicy có test allowed/denied flows không?
16. Image immutable + trace về Git SHA không?
17. Database migration compatible rolling deploy không?
18. Monitoring có business + workload + node + dependency layers không?
19. Alert có runbook không?
20. Backup/restore/failure drill có evidence không?
21. Multi-region có RTO/RPO justify không?
22. Cost idle capacity/logging/network có owner không?

---

# 23. Exit criteria

Bạn hiểu AKS ở mức production khi có thể:

- giải thích vì sao workload cần AKS thay vì Container Apps/App Service;
- mô tả control plane vs node/workload responsibilities;
- thiết kế node pools có lý do;
- vẽ ingress → Service → Pod → managed dependencies;
- giải thích IP/network policy/private dependency design;
- dùng Workload Identity thay shared static credentials khi phù hợp;
- set requests/limits từ measurement;
- thiết kế probes đúng semantics;
- phân biệt Pod autoscaling và node autoscaling;
- rollout app mà không phá database compatibility;
- reason về Pod/node/zone/region failures;
- debug Pending/CrashLoop/NotReady/OOM/ImagePull;
- thiết kế SLO + telemetry xuyên application/cluster/dependencies;
- có runbook cho upgrades, incidents và failure drills;
- nói được khi **không nên dùng AKS**.

## Official references

- [AKS documentation](https://learn.microsoft.com/en-us/azure/aks/)
- [AKS — Plan your design and operations](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-start-here)
- [Baseline architecture for an AKS cluster](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)
- [Microservices architecture on AKS](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices)
- [Architecture best practices for AKS — Well-Architected](https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-kubernetes-service)
- [Choose an Azure container service](https://learn.microsoft.com/en-us/azure/architecture/guide/choose-azure-container-service)
- [Kubernetes documentation](https://kubernetes.io/docs/)

Xem thêm [References — Module 14](references.md).

## Verification metadata

- Verified: 2026-08-28.
- Kubernetes baseline in repository: 1.36.x.
- Azure-specific recommendations: always re-check Microsoft Learn before production because AKS defaults/features change quickly.
