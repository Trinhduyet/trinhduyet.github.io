# AKS on Azure — Production Mapping

> [← Kubernetes overview](README.md) · [Azure overview](../14-cloud/README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;AKS · Azure Integration</span>
  <span><strong>Rule</strong>&nbsp;learn Kubernetes core first</span>
</div>

AKS là managed Kubernetes trên Azure. File này nằm trong **Kubernetes module** vì Pods, Deployments, Services, scheduling, probes, resources, RBAC và NetworkPolicy vẫn là Kubernetes concepts. Azure chỉ cung cấp managed control plane và platform integrations.

---

# 1. Shared responsibility

```text
AKS managed service
├─ Kubernetes control plane service
├─ Azure integration/add-ons according to configuration
└─ cluster management capabilities

Platform/workload team
├─ node pools/capacity
├─ manifests/Helm/GitOps
├─ namespaces/RBAC/policy
├─ requests/limits/probes
├─ ingress/egress
├─ workload identity
├─ upgrades/validation
├─ application correctness
├─ observability
├─ backup/restore/DR
└─ cost/incident response
```

Managed control plane does not mean zero-ops.

---

# 2. AKS Automatic vs Standard

Current Microsoft guidance distinguishes:

```text
AKS Automatic
→ opinionated production defaults
→ Standard cluster-management pricing tier by default
→ lower day-0/day-2 decision surface

AKS Standard
→ more explicit/custom cluster configuration
→ use when specialized platform requirements justify control
```

Microsoft currently positions Automatic as the production-ready default for many common workloads. Standard is still appropriate for custom networking/topology/operations requirements.

Official:

- <https://learn.microsoft.com/en-us/azure/aks/free-standard-pricing-tiers>
- <https://learn.microsoft.com/en-us/azure/aks/core-aks-concepts>

Do not confuse:

```text
AKS Standard cluster mode
!= Standard pricing tier
```

---

# 3. Pricing tiers

Cluster-management pricing tiers:

```text
Free
→ dev/test/learning, no financially backed uptime SLA

Standard
→ production uptime-SLA profile

Premium
→ Standard capabilities + extended/LTS support model
```

AKS bill is much larger than cluster-management tier alone:

```text
cluster management
+ VM nodes
+ managed disks
+ load balancer/public IP
+ NAT/Firewall
+ ACR
+ Log Analytics/Monitor
+ Defender/security features
+ backup
+ network egress
```

This is why `AKS control plane price` is not a useful total-cost estimate.

---

# 4. Baseline topology

```text
Users
  ↓
Azure Front Door + WAF (if justified)
  ↓
regional ingress/gateway
  ↓
AKS
├─ system node pool
├─ user node pool(s)
├─ namespace: checkout
│  ├─ Deployment checkout-api
│  ├─ Service checkout-api
│  ├─ Deployment payment-worker
│  └─ HPA/KEDA
├─ RBAC
├─ NetworkPolicy
└─ workload identities
     │
     ├─ Azure SQL
     ├─ Service Bus
     ├─ Blob Storage
     └─ Key Vault

Images → Azure Container Registry
Telemetry → Azure Monitor / Application Insights / managed metrics/logs
```

AKS is the orchestration layer, not the entire Azure architecture.

---

# 5. Node pools

Separate system/application concerns where justified:

```text
system node pool
→ system-critical components

user node pools
→ business workloads
```

Additional pools need a reason:

```text
GPU/specialized hardware
Windows vs Linux
security/isolation
very different scaling profile
system/user separation
taints/tolerations
```

Each node pool adds:

```text
cost
IP capacity
upgrade surface
scheduling complexity
runbooks
```

Don't create one node pool per microservice.

---

# 6. Networking

Decisions before cluster creation:

```text
public/private API server?
VNet/subnet CIDR?
Pod IP model?
ingress?
egress?
private dependencies?
DNS?
NetworkPolicy?
```

For current production microservices guidance, Microsoft recommends evaluating **Azure CNI powered by Cilium** because of networking/policy/eBPF capabilities. Treat it as a reference baseline, not copy-paste configuration.

Official architecture references:

- <https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks>
- <https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices>

## IP planning

Model:

```text
nodes
+ max Pods/node
+ scale-out headroom
+ upgrade/surge nodes
+ private endpoints/other subnet consumers
```

Insufficient IP space is an expensive architecture problem to discover late.

---

# 7. Ingress/egress

Do not expose every Kubernetes Service as `LoadBalancer`.

Possible path:

```text
Internet
→ Front Door/WAF
→ regional ingress
→ Kubernetes Service
→ Ready Pods
```

Egress:

```text
Pod
→ node/network dataplane
→ controlled NAT/Firewall route if required
→ internet/external provider
```

External provider allow-listing may require stable outbound IP design.

---

# 8. Workload Identity

Preferred Azure cloud-access model:

```text
Pod
→ Kubernetes ServiceAccount
→ federated Microsoft Entra Workload Identity
→ Azure token
→ target data-plane RBAC
```

Benefits:

```text
no shared SQL/Service Bus/Storage keys in Kubernetes Secret
least privilege per workload
credential rotation burden reduced
better auditability
```

Still configure exact data-plane roles.

External third-party API keys may still require Key Vault/secret integration.

---

# 9. Azure Container Registry

Pipeline:

```text
Git SHA
→ build/test
→ image
→ scan/policy
→ ACR
→ immutable digest
→ AKS Deployment
```

Prefer traceable production image:

```text
myacr.azurecr.io/checkout@sha256:...
```

ACR Premium may be needed for advanced networking/geo/high-throughput features; do not choose Premium by default.

---

# 10. Requests/limits + node capacity

Pod resources affect scheduler:

```text
node allocatable
- system overhead
- DaemonSets
- existing Pod requests
= remaining schedulable capacity
```

Include rollout surge:

```text
steady replicas = 20
maxSurge = 25%
→ rollout can need ~5 extra Pods
```

If no room:

```text
new Pods Pending
→ rollout stalls
```

Node autoscaler timing must be considered with HPA/workload autoscaling timing.

---

# 11. HPA / KEDA / node scaling

These are different control loops.

```text
HPA
→ changes Pod replica count

KEDA
→ event/external-metric driven workload replicas

node autoscaling
→ changes node capacity
```

Failure pattern:

```text
queue grows
→ KEDA creates 50 Pods
→ cluster has no capacity
→ Pods Pending
→ node scale takes time
→ queue keeps growing
```

Design end-to-end response time and downstream quotas.

---

# 12. Probes and graceful shutdown

Use core Kubernetes semantics from Module 15.

AKS does not change:

```text
readiness = traffic eligibility
liveness = restart usefulness
startup = delayed liveness window
```

During termination:

```text
SIGTERM
→ stop accepting work
→ bounded drain
→ exit before grace period
```

.NET BackgroundService/host cancellation should propagate correctly.

---

# 13. Deployment + database

```text
new image
→ new ReplicaSet
→ new Pods
→ readiness
→ traffic shift
→ old Pods terminate
```

Database migrations remain external correctness responsibility.

Use:

```text
expand schema
→ deploy compatible app
→ migrate/backfill
→ switch
→ contract later
```

`kubectl rollout undo` does not undo schema/data side effects.

---

# 14. Security baseline

At Kubernetes layer:

```text
namespace/RBAC
ServiceAccount
securityContext
Pod Security Standards
NetworkPolicy
resource quota
image policy/scanning
secret management
```

At Azure layer:

```text
Entra integration
Workload Identity
Private Link/VNet
ACR permissions
Defender if justified
Azure Policy/admission integration where used
Key Vault
```

Security needs negative tests:

```text
unauthorized ServiceAccount cannot list secrets
blocked namespace cannot reach protected service
Pod cannot run privileged where policy denies it
```

---

# 15. Observability

Need three levels:

```text
application
→ traces/business metrics

Kubernetes workload
→ Pod restarts/readiness/OOM/replicas/events

cluster/node
→ allocatable/usage/node pressure/network/storage
```

Azure Monitor/Container Insights/managed Prometheus options can collect telemetry, but retention/sampling/data volume must be cost-controlled.

Important AKS signals:

```text
Pending Pods
restart rate
OOMKilled
readiness failures
node pressure
HPA desired/current replicas
queue/backlog metric
API latency/error
Log Analytics GB/day
```

---

# 16. Availability domains

Different failures:

```text
container
Pod
Node
Availability Zone
Region
```

Pod/node controls:

```text
replicas
topology spread/anti-affinity
PDB
enough capacity
correct probes
graceful shutdown
```

Zone:

```text
zone-aware node topology
spread replicas
dependencies also zone-resilient
```

Region:

```text
usually separate AKS cluster
+ global routing
+ data replication/recovery
+ identity/config synchronization
+ tested failover
```

Kubernetes alone does not create multi-region application correctness.

---

# 17. Backup/restore

Separate:

```text
cluster manifests/config
application persistent volumes
managed databases
object storage
external systems
```

IaC/Git can recreate cluster definitions, but it is not a backup of business data.

For a typical stateless API + Azure SQL:

```text
AKS recreated from IaC/Git
Azure SQL restored/failover separately
Blob/other state recovered separately
```

Test a real restore path.

---

# 18. Upgrade strategy

Track:

```text
Kubernetes supported version window
AKS release/support policy
node image upgrades
add-on compatibility
CNI/CSI versions
admission/controllers
Helm charts
application API deprecations
```

Test non-prod before production.

Never wait until version support expires to discover deprecated APIs.

---

# 19. Cost worksheet

```text
cluster management tier
+ system node pool VM hours
+ user node pools VM hours
+ OS/data disks
+ load balancers/public IP
+ NAT Gateway / Firewall
+ ACR
+ Log Analytics / metrics
+ security features
+ backup
+ cross-zone/region/internet transfer
```

Measure:

```text
node utilization
requested vs actual resources
idle namespace capacity
non-prod uptime
log volume
HPA/node autoscaler behavior
```

Cost optimization without SLO regression:

```text
right-size requests
consolidate safely
scale non-prod down
choose appropriate node SKUs
remove unused public/network resources
control telemetry
commit/reservation only after steady profile exists
```

---

# 20. Failure drills

Minimum production exercises:

```text
delete Pod
cordon/drain node in non-prod
break readiness
bad image rollout
OOM
Service selector failure
NetworkPolicy deny
DNS/private dependency failure
ACR pull permission failure
Workload Identity/RBAC failure
queue spike / HPA-KEDA scale
node capacity exhaustion
restore data dependency
upgrade rehearsal
```

Evidence = commands, metrics, observed recovery time and runbook changes.

---

# 21. AKS review checklist

- [ ] Why Kubernetes instead of App Service/Container Apps is explicit.
- [ ] Automatic vs Standard decision documented.
- [ ] Cluster-management pricing tier documented.
- [ ] System/user node pool topology justified.
- [ ] IP/CIDR has scale + upgrade headroom.
- [ ] Ingress/egress/DNS path explicit.
- [ ] NetworkPolicy model tested.
- [ ] Workload Identity replaces Azure static credentials where possible.
- [ ] ACR permissions/image digest traceability exists.
- [ ] Requests/limits from measurement.
- [ ] Pod and node autoscaling timing modeled.
- [ ] Probes + graceful shutdown correct.
- [ ] Database migrations compatible with rolling releases.
- [ ] Security policies have negative tests.
- [ ] Logs/metrics have retention/cost controls.
- [ ] Backup/restore drills exist.
- [ ] Upgrade cadence exists.
- [ ] Total cost includes nodes/network/logs/security/backup, not just control plane.

## Verification metadata

- Verified: 2026-08-28.
- Current Microsoft AKS docs show Free/Standard/Premium cluster-management pricing tiers; Automatic uses Standard tier by default.
- Microsoft currently presents AKS Automatic as production-ready default for many typical workloads.
- Kubernetes core behavior remains governed by official Kubernetes docs; Azure-specific integration claims use Microsoft Learn.
