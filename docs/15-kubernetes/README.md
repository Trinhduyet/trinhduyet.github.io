# Module 15 — Kubernetes

> [← DevOps & IaC](../13-devops-iac/README.md) · [DevOps → Kubernetes Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md) · [Docker prerequisite](../12-docker/README.md) · [Azure mapping](aks-on-azure-production-architecture.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;Kubernetes Core · Delivery · Operations · CKAD</span>
  <span><strong>Context</strong>&nbsp;runtime platform inside a DevOps lifecycle</span>
</div>

Kubernetes là một module riêng vì object model, controllers, scheduling, networking, storage và security của nó đủ lớn để học độc lập. Tuy nhiên trong production, Kubernetes thường nằm **bên trong DevOps delivery lifecycle**:

```text
Code
 ↓
Testing / Review
 ↓
CI
 ↓
Container image
 ↓
Registry
 ↓
IaC / Platform
 ↓
Kubernetes
 ↓
Observability / Recovery
```

Kubernetes giải quyết orchestration của containerized workloads. Nó không tự cung cấp toàn bộ source control, testing, artifact build, cloud provisioning, secrets lifecycle, SLO design hay incident process.

Roadmap.sh cũng tách DevOps và Kubernetes thành hai roadmap: DevOps là breadth của delivery/operations, còn Kubernetes là deep specialization cho orchestration. Module này giữ đúng boundary đó.

- DevOps roadmap: <https://roadmap.sh/devops>
- Kubernetes roadmap: <https://roadmap.sh/kubernetes>

---

# 1. Kubernetes giải quyết vấn đề gì?

Container runtime chạy một container. Production platform phải giải quyết nhiều container/nodes:

```text
Where should workloads run?
How many replicas?
What if a Pod or Node fails?
How does traffic find healthy backends?
How is configuration injected?
How are CPU/RAM resources bounded?
How are versions rolled out?
How is persistent storage attached?
How are identities and network paths restricted?
```

Kubernetes dùng declarative API + reconciliation loops:

```text
desired state
   ↓
Kubernetes API
   ↓
controllers / scheduler / kubelet
   ↓
observed state
   ↓
reconcile continuously
```

Official concepts: <https://kubernetes.io/docs/concepts/>

---

# 2. Kubernetes nằm ở đâu trong DevOps?

| Delivery concern | Primary owner/tool class |
|---|---|
| code + review | Git / PR workflow |
| tests | CI/testing tools |
| build artifact/image | CI + OCI tooling |
| registry | container registry |
| cloud/cluster infrastructure | Terraform/Bicep/cloud IaC |
| workload orchestration | **Kubernetes** |
| packaging/config composition | Helm/Kustomize/raw manifests |
| continuous delivery | pipeline or GitOps controller |
| telemetry | metrics/logs/traces stack |
| incident/recovery | team process + platform controls |

Mental model:

```text
DevOps provides lifecycle
Kubernetes provides orchestration runtime
```

→ [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

---

# 3. Cluster architecture

```text
Control Plane
├─ kube-apiserver
├─ etcd
├─ kube-scheduler
├─ kube-controller-manager
└─ cloud-controller-manager when applicable

Worker Node
├─ kubelet
├─ container runtime
├─ network/data-plane components
└─ Pods
```

Important flow:

```text
kubectl / controller
→ API server
→ desired object stored
→ scheduler/controller observes
→ node kubelet acts
→ status reported
→ reconciliation continues
```

→ [Cluster Architecture & Reconciliation](cluster-architecture-and-reconciliation.md)

---

# 4. Core object relationships

```text
Deployment
   ↓ owns
ReplicaSet
   ↓ owns
Pods
   ↓ contain
Containers
```

Other workload controllers:

```text
StatefulSet
DaemonSet
Job
CronJob
```

Networking:

```text
Ingress / Gateway / LoadBalancer
   ↓
Service
   ↓ selector / EndpointSlice
Pods
```

Configuration:

```text
ConfigMap / Secret
→ env / files / projected volumes
→ Pod
```

Storage:

```text
Pod
→ PVC
→ PV / StorageClass / CSI
```

Security:

```text
User / ServiceAccount
→ authentication
→ RBAC authorization
→ admission/policy
→ API object
```

---

# 5. Prerequisites — đừng bắt đầu từ YAML

Trước Kubernetes cần vững:

```text
Linux processes/signals
DNS/TCP/HTTP/TLS
Git
Docker image/container
container ports/networking
volumes/filesystem
CPU/memory behavior
health endpoints
basic YAML
```

Và để chạy production delivery cần thêm:

```text
CI/CD
artifact registry
IaC
secrets/identity
observability
```

→ [Module 12 — Docker](../12-docker/README.md)
→ [Module 13 — DevOps & IaC](../13-devops-iac/README.md)

---

# 6. Learning path — Kubernetes core đến production

## Phase 1 — Foundation & kubectl

Học:

```text
cluster architecture
Namespace
Pod
labels/selectors
apiVersion/kind/metadata/spec/status
kubectl get/describe/logs/exec/explain
```

Evidence:

```bash
kubectl get nodes
kubectl get pods -A
kubectl explain pod.spec
kubectl describe pod <pod>
kubectl logs <pod>
```

## Phase 2 — Workloads

```text
Deployment / ReplicaSet
Job / CronJob
StatefulSet / DaemonSet concepts
init containers
sidecars
probes
rollout / rollback
```

→ [Workloads, Networking & Storage](workloads-networking-and-storage.md)

## Phase 3 — Configuration & Scheduling

```text
ConfigMap / Secret
command / args / env
requests / limits
ResourceQuota / LimitRange
nodeSelector
affinity / anti-affinity
taints / tolerations
topology spread
ServiceAccount / securityContext
```

→ [Application Configuration & Scheduling](application-configuration-and-scheduling.md)

## Phase 4 — Networking & Storage

```text
Service
DNS
EndpointSlice
Ingress / Gateway API concept
NetworkPolicy
PVC / PV / StorageClass / CSI
```

## Phase 5 — Security & Operations

```text
RBAC
Pod Security
NetworkPolicy
resource policies
metrics/logs/events
PDB
autoscaling
upgrades
backup/recovery boundaries
```

→ [Security, Observability & Operations](kubernetes-security-observability-and-operations.md)

## Phase 6 — Debugging / CKAD

```text
Pending
ImagePullBackOff
CrashLoopBackOff
OOMKilled
NotReady
no endpoints
DNS failure
NetworkPolicy deny
PVC Pending
bad rollout
RBAC forbidden
```

→ [kubectl Debugging & CKAD Practice](kubectl-debugging-and-ckad-practice.md)

## Phase 7 — Delivery integration

```text
CI image build
registry
Kubernetes config
Helm / Kustomize
push CD or GitOps
rollout gates
observability
recovery
```

→ [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

## Phase 8 — Managed provider mapping

Core concepts first, cloud-specific mapping later.

```text
Kubernetes core
   ↓
AKS / EKS / GKE / on-prem implementation choices
```

→ [AKS on Azure](aks-on-azure-production-architecture.md)

---

# 7. First workload lab

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: demo-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app: demo-api
  template:
    metadata:
      labels:
        app: demo-api
    spec:
      containers:
        - name: api
          image: registry.example/demo-api@sha256:replace-me
          ports:
            - name: http
              containerPort: 8080
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
            limits:
              memory: 256Mi
          readinessProbe:
            httpGet:
              path: /health/ready
              port: http
          livenessProbe:
            httpGet:
              path: /health/live
              port: http
```

Observe objects, not only apply result:

```bash
kubectl apply -f deployment.yaml
kubectl get deploy,rs,pods
kubectl describe deployment demo-api
kubectl get pods -o wide
kubectl logs deployment/demo-api
```

Add Service and inspect endpoints:

```bash
kubectl get svc,endpointslices
```

---

# 8. CI should deliver immutable images

Kubernetes should receive an image identity created earlier in CI.

Prefer:

```text
commit abc123
→ registry.example/demo-api@sha256:xyz
→ Deployment references digest
```

Avoid relying on mutable production tags such as `latest` without strong controls.

Traceability:

```text
Git SHA
→ CI run
→ image digest
→ deployment revision
→ running Pod imageID
```

This is where Module 13 and Module 15 connect directly.

---

# 9. Helm / Kustomize are delivery tools, not replacements for Kubernetes knowledge

Raw YAML helps learn the API.

Production config may use:

```text
Kustomize
→ base + overlays/patches

Helm
→ chart + values → rendered Kubernetes resources
```

Always be able to inspect rendered resources.

Useful checks:

```bash
helm template ...
kubectl diff -f ...
kubectl apply --dry-run=server -f ...
```

Helm docs: <https://helm.sh/docs/>

---

# 10. GitOps integration

A common DevOps + Kubernetes production model:

```text
CI
→ build/test image
→ push registry
→ update deployment config in Git

Git
→ Argo CD / Flux
→ Kubernetes API
→ reconcile desired state
```

Argo CD and Flux are GitOps/CD implementations, not Kubernetes core requirements.

Choose them when you value:

```text
declarative desired deployment state
auditability
drift reconciliation
separation of build from deploy credentials
multi-cluster/environment consistency
```

Do not add GitOps if a simpler pipeline already meets the requirements.

---

# 11. Requests/limits and autoscaling

Requests matter for scheduling and resource-utilization autoscaling.

```yaml
resources:
  requests:
    cpu: 250m
    memory: 256Mi
  limits:
    memory: 512Mi
```

Measure before choosing values:

```text
normal/peak CPU
memory working set
GC behavior
P95/P99 latency
OOM events
node allocatable
rollout surge
```

Kubernetes HPA can update replica counts based on configured metrics. That feedback loop only works well when requests, metrics and capacity bounds make sense.

Official HPA: <https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/>

---

# 12. Probe semantics matter to CD

Readiness:

```text
Should this Pod receive traffic?
```

Liveness:

```text
Is restarting this process useful?
```

Startup:

```text
Does this app need a longer startup window?
```

A rollout gate relying on bad readiness semantics gives false confidence.

Bad example:

```text
liveness → database health
DB outage
→ all Pods restart repeatedly
```

---

# 13. Rollout ≠ safe release automatically

```text
new image
→ new Pods
→ readiness
→ traffic shifts
→ old Pods terminate
```

Still need:

```text
database compatibility
config compatibility
capacity for surge
external dependency compatibility
business SLO checks
rollback/roll-forward strategy
```

`kubectl rollout undo` cannot reverse destructive data migrations.

---

# 14. Debug from pipeline to Pod

When deployment fails:

```text
1. Did CI create the expected image?
2. Is digest/tag in registry?
3. Did deployment config change correctly?
4. Did CD/GitOps sync?
5. Did API accept resources?
6. Can scheduler place Pods?
7. Can image be pulled?
8. Does process start?
9. Do probes pass?
10. Does Service have endpoints?
11. Are DNS/network/storage/identity correct?
```

Commands:

```bash
kubectl get <resource> -o wide
kubectl describe <resource>
kubectl get events --sort-by=.lastTimestamp
kubectl logs <pod>
kubectl logs <pod> --previous
kubectl get endpointslices
kubectl auth can-i ...
kubectl top pod
kubectl top node
```

→ [Debugging & CKAD Practice](kubectl-debugging-and-ckad-practice.md)

---

# 15. CKAD-oriented skills

CKAD focuses on application-developer tasks around design/build, deployment, observability/maintenance, configuration/security, services/networking.

Official certification source: <https://www.cncf.io/training/certification/ckad/>

This module uses CKAD tasks to strengthen hands-on fluency, but production reasoning remains broader than exam tasks.

---

# 16. When NOT to use Kubernetes

Don't choose Kubernetes because:

```text
production = K8s
microservices = K8s
containers = K8s
DevOps = K8s
```

A simpler platform may be better when:

```text
few services
small team
simple networking/security
low deployment complexity
PaaS meets NFR
cluster operations add more toil than value
```

DevOps maturity is not measured by Kubernetes adoption.

---

# 17. Module map

| Guide | Focus |
|---|---|
| [Architecture & Reconciliation](cluster-architecture-and-reconciliation.md) | control plane, nodes, desired/observed state |
| [Workloads, Networking & Storage](workloads-networking-and-storage.md) | controllers, Service, probes, network/storage basics |
| [Application Configuration & Scheduling](application-configuration-and-scheduling.md) | config, resources, scheduling, ServiceAccount/securityContext |
| [Security, Observability & Operations](kubernetes-security-observability-and-operations.md) | RBAC, NetworkPolicy, telemetry, reliability operations |
| [kubectl Debugging & CKAD Practice](kubectl-debugging-and-ckad-practice.md) | failure states, troubleshooting, application tasks |
| [AKS on Azure](aks-on-azure-production-architecture.md) | provider-specific production mapping |
| [References](references.md) | official and supplementary resources |

Cross-module guide:

→ [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

---

# 18. Exit criteria

Bạn hoàn thành Kubernetes core khi có thể:

- explain control plane vs worker node;
- reason desired state and reconciliation;
- create/debug Pods, Deployments, Jobs/CronJobs;
- use ConfigMap/Secret/resources/scheduling controls;
- expose workloads with Service and debug endpoints/DNS;
- reason PVC/StorageClass at application level;
- use RBAC/ServiceAccount/securityContext fundamentals;
- apply NetworkPolicy basics;
- use probes correctly;
- reason HPA/resources/capacity;
- debug Pending/CrashLoop/ImagePull/NotReady/OOM/network/PVC/RBAC failures;
- rollout and rollback application revisions safely;
- connect CI artifact identity to Kubernetes deployment revision;
- explain Helm/Kustomize/GitOps roles;
- explain when Kubernetes is unnecessary;
- map Kubernetes to AKS without confusing provider features with Kubernetes core.

## Source policy

Kubernetes official docs are canonical. roadmap.sh and Vietnamese community materials are supplementary for coverage, learning order and explanation.

## Verification metadata

- Verified: 2026-08-28.
- Updated to explicitly connect Kubernetes with DevOps delivery while keeping Kubernetes as a distinct technical module.
