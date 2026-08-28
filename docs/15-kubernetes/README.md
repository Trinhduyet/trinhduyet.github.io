# Module 15 — Kubernetes

> [← Docker prerequisite](../12-docker/README.md) · [Azure là module riêng](../14-cloud/README.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;Kubernetes Core · Application Operations · CKAD</span>
  <span><strong>Audience</strong>&nbsp;.NET Backend → DevOps/Platform-ready</span>
  <span><strong>Mode</strong>&nbsp;concept → kubectl → YAML → failure/debug</span>
</div>

Kubernetes là **một module độc lập**, không còn nằm bên trong page Azure. Kubernetes có thể chạy trên AKS, EKS, GKE, on-prem hoặc local lab; vì vậy trước hết phải học đúng Kubernetes object/API model, sau đó mới map sang cloud provider.

Mục tiêu không phải nhớ YAML. Mục tiêu là hiểu:

```text
desired state
reconciliation
API objects
scheduling
networking
storage
configuration
security
observability
rollout
troubleshooting
```

Các bài tiếng Việt bạn cung cấp rất hữu ích để xây mental model từ beginner: kiến trúc control plane/worker node, lifecycle tạo Pod và chuỗi thực hành kubectl/ConfigMap/Deployment. Module này dùng các nguồn đó như **supplementary learning path**, còn behavior/version-sensitive claims luôn đối chiếu `kubernetes.io`.

---

# 1. Kubernetes giải quyết vấn đề gì?

Container runtime giúp chạy container. Nhưng production cần quản lý nhiều workloads/nodes:

```text
Where should a container run?
How many replicas?
What if one dies?
How is traffic routed?
How is configuration injected?
How are CPU/RAM bounded?
How do we roll out a new version?
How do we persist data?
How do we restrict access?
```

Kubernetes cung cấp declarative API + controllers để liên tục đưa **observed state** gần **desired state**.

```text
You declare desired state
        ↓
kube-apiserver stores/exposes intent
        ↓
controllers + scheduler + kubelet act
        ↓
observed state changes
        ↓
reconcile continuously
```

Official overview: <https://kubernetes.io/docs/concepts/overview/>

---

# 2. Cluster architecture — bức tranh phải thuộc bằng mental model

Một cluster gồm:

```text
Control Plane
├─ kube-apiserver
├─ etcd
├─ kube-scheduler
├─ kube-controller-manager
└─ cloud-controller-manager (when applicable)

Worker Node
├─ kubelet
├─ container runtime
├─ kube-proxy or equivalent data-plane implementation
└─ Pods
```

Official components: <https://kubernetes.io/docs/concepts/overview/components/>

## kube-apiserver

Core API boundary. `kubectl`, controllers, scheduler, kubelet và integrations communicate through Kubernetes API.

## etcd

Consistent key-value store for Kubernetes API state.

## scheduler

Watches unscheduled Pods and chooses Nodes based on resources/constraints/policies.

## controller manager

Runs reconciliation loops such as Deployment/ReplicaSet/Node controllers.

## kubelet

Node agent that ensures Pod/container state assigned to the node is materialized.

<div class="key-takeaway" markdown>
<strong>Kubernetes không phải chuỗi script orchestration.</strong>

Nhiều control loops độc lập cùng reconcile system state. Đây là concept phải hiểu trước khi học YAML sâu.
</div>

→ [Cluster Architecture & Reconciliation](cluster-architecture-and-reconciliation.md)

---

# 3. Một Pod được tạo như thế nào?

Mental flow:

```text
kubectl apply
   ↓
kube-apiserver
   ↓ validates/auth/admission
etcd stores desired object
   ↓
scheduler notices unscheduled Pod
   ↓
select Node + bind
   ↓
kubelet on Node observes assignment
   ↓
container runtime pulls image / starts containers
   ↓
kubelet reports status
   ↓
controllers keep reconciling
```

Đây là flow quan trọng hơn việc thuộc `kubectl run` syntax.

---

# 4. Object model — học theo relationship

```text
Deployment
    ↓ owns
ReplicaSet
    ↓ owns
Pods
    ↓ contain
Containers
```

Other workloads:

```text
StatefulSet  → stable identity/storage-oriented workloads
DaemonSet   → one/some Pods per eligible Node
Job         → run-to-completion
CronJob     → scheduled Jobs
```

Networking:

```text
Client
  ↓
Ingress / Gateway / LoadBalancer
  ↓
Service
  ↓ selector / EndpointSlice
Pods
```

Configuration:

```text
ConfigMap / Secret
      ↓
env / volume / projected config
      ↓
Pod
```

Storage:

```text
Pod
 ↓ PVC
PersistentVolume
 ↓ CSI / storage provider
```

Security:

```text
User / ServiceAccount
→ authn
→ RBAC authz
→ admission/policy
→ API object
```

---

# 5. Learning path — từ beginner đến production/CKAD

## Phase 0 — Prerequisites

Bạn cần biết:

```text
Linux process
TCP/DNS/HTTP
Docker image/container
container ports
filesystem/volume
CPU/memory limits
Git/YAML basics
```

Nếu chưa vững Docker, quay lại [Module 12](../12-docker/README.md).

## Phase 1 — Foundation & kubectl

Học:

```text
cluster
control plane / worker node
API server / etcd / scheduler / controller / kubelet
Pod
Namespace
kubectl get / describe / logs / exec
YAML apiVersion/kind/metadata/spec/status
labels / selectors
```

Evidence:

```bash
kubectl get nodes
kubectl get pods -A
kubectl explain pod.spec
kubectl describe pod <pod>
kubectl logs <pod>
```

## Phase 2 — Application workloads

Học:

```text
Pod
Deployment / ReplicaSet
Job / CronJob
multi-container Pods
init containers
sidecars
rollout / rollback
probes
resources
```

→ [Workloads, Networking & Storage](workloads-networking-and-storage.md)

## Phase 3 — Configuration & Scheduling

Học:

```text
ConfigMap
Secret
env / command / args
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

Học:

```text
Pod network
Service ClusterIP/NodePort/LoadBalancer
DNS
EndpointSlice
Ingress / Gateway API concept
NetworkPolicy
emptyDir
PVC / PV / StorageClass
CSI
StatefulSet storage identity
```

→ [Workloads, Networking & Storage](workloads-networking-and-storage.md)

## Phase 5 — Security, Operations & Observability

Học:

```text
RBAC
ServiceAccount
securityContext
Pod Security Standards
NetworkPolicy
secrets boundary
metrics/logs/events
rollout safety
PDB
autoscaling
backup/restore boundary
cluster/workload upgrades
```

→ [Security, Observability & Operations](kubernetes-security-observability-and-operations.md)

## Phase 6 — Troubleshooting + CKAD practice

Học theo failure:

```text
Pending
ImagePullBackOff
CrashLoopBackOff
OOMKilled
Running but NotReady
Service has no endpoints
DNS failure
NetworkPolicy deny
PVC Pending
bad rollout
resource pressure
```

→ [kubectl Troubleshooting & CKAD Practice](kubectl-debugging-and-ckad-practice.md)

## Phase 7 — Managed Kubernetes / AKS

Sau khi core Kubernetes rõ mới học cloud mapping:

```text
Kubernetes core objects
       ↓
AKS managed control plane
       ↓
Azure node pools / networking / identity / ACR / Monitor
```

→ [AKS on Azure — Production Mapping](aks-on-azure-production-architecture.md)

---

# 6. First application lab

Use a simple .NET API/container.

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

Apply + observe:

```bash
kubectl apply -f deployment.yaml
kubectl get deploy,rs,pods
kubectl describe deployment demo-api
kubectl get pods -o wide
kubectl logs deployment/demo-api
```

Add Service:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: demo-api
spec:
  selector:
    app: demo-api
  ports:
    - name: http
      port: 80
      targetPort: http
```

Check:

```bash
kubectl get svc,endpointslices
```

Don't stop at `kubectl apply` success. Inspect the objects the controllers created.

---

# 7. Requests/limits — scheduling trước performance

```yaml
resources:
  requests:
    cpu: 250m
    memory: 256Mi
  limits:
    memory: 512Mi
```

Mental model:

```text
requests
→ scheduler capacity signal

limits
→ runtime resource bound according to resource semantics
```

Do not copy tutorial numbers into production.

Measure:

```text
normal/peak CPU
memory working set
.NET GC behavior
OOM events
P95/P99 latency
node allocatable
rollout surge capacity
```

---

# 8. Probe semantics

Readiness:

```text
Should this Pod receive new traffic?
```

Liveness:

```text
Is restart useful because process is irrecoverably stuck?
```

Startup:

```text
Does app need more startup time before liveness begins?
```

Bad:

```text
liveness checks database
DB outage
→ all Pods restart
→ more churn
→ DB still outage
```

Health endpoint semantics are application architecture.

---

# 9. Rollout & database compatibility

```bash
kubectl set image deployment/demo-api api=registry.example/demo-api@sha256:new
kubectl rollout status deployment/demo-api
kubectl rollout history deployment/demo-api
kubectl rollout undo deployment/demo-api
```

But:

```text
kubectl rollout undo
!= database rollback
```

Use expand/contract migrations when old/new versions can coexist.

---

# 10. Debugging order

Use a repeatable flow:

```text
1. What object is failing?
2. Desired state?
3. Current status?
4. Events?
5. Logs/current + previous?
6. Scheduling/resource issue?
7. Networking/DNS/endpoints?
8. Config/Secret/identity?
9. Storage?
10. Recent rollout/change?
```

Commands:

```bash
kubectl get <resource> -o wide
kubectl describe <resource>
kubectl get events --sort-by=.lastTimestamp
kubectl logs <pod>
kubectl logs <pod> --previous
kubectl exec -it <pod> -- sh
kubectl get endpointslices
kubectl auth can-i ...
kubectl top pod
kubectl top node
```

→ [Debugging & CKAD Practice](kubectl-debugging-and-ckad-practice.md)

---

# 11. CKAD-oriented path

CKAD is application-developer focused, not full cluster-admin certification.

Current CNCF domains:

| Domain | Weight |
|---|---:|
| Application Design and Build | 20% |
| Application Deployment | 20% |
| Application Observability and Maintenance | 15% |
| Application Environment, Configuration and Security | 25% |
| Services and Networking | 20% |

Official: <https://www.cncf.io/training/certification/ckad/>

Module 15 covers CKAD-relevant skills but **không biến toàn bộ Kubernetes curriculum thành exam tricks**. Production reasoning vẫn ưu tiên correctness, failure, capacity và security.

---

# 12. Khi nào KHÔNG nên dùng Kubernetes

Don't use Kubernetes because:

```text
"production = K8s"
"microservices = K8s"
"we need containers"
"team wants Kubernetes on CV"
```

Simpler managed platforms can be better if:

```text
few services
simple deployment requirements
small team
no cluster-level scheduling/policy needs
low operational maturity
PaaS already meets NFR
```

Kubernetes pays off when orchestration/platform control value exceeds platform toil.

---

# 13. Failure experiments

Minimum lab set:

## Delete Pod

```bash
kubectl delete pod <pod>
```

Observe ReplicaSet/Deployment reconciliation.

## Wrong image

Observe:

```text
ImagePullBackOff
Events
rollout stall
```

## Crash app

Observe:

```text
restart count
CrashLoopBackOff
previous logs
```

## OOM

Set too-low memory limit and observe `OOMKilled` evidence.

## Break readiness

Pod may be Running but removed from Service endpoints.

## Wrong Service selector

Service DNS exists but EndpointSlice has no matching backend.

## PVC failure

Observe Pending reason / storage provisioning events.

---

# 14. Module map

| Guide | Focus |
|---|---|
| [Cluster Architecture & Reconciliation](cluster-architecture-and-reconciliation.md) | control plane, node components, desired/observed state |
| [Workloads, Networking & Storage](workloads-networking-and-storage.md) | workloads, Service, probes, PVC, network/storage basics |
| [Application Configuration & Scheduling](application-configuration-and-scheduling.md) | ConfigMap/Secret, env, resources, affinity, taints, ServiceAccount/security context |
| [Security, Observability & Operations](kubernetes-security-observability-and-operations.md) | RBAC, NetworkPolicy, telemetry, rollout, incident/runbook |
| [kubectl Debugging & CKAD Practice](kubectl-debugging-and-ckad-practice.md) | hands-on commands, failure states, exam-oriented application tasks |
| [AKS on Azure](aks-on-azure-production-architecture.md) | map Kubernetes onto Azure after fundamentals |
| [References](references.md) | official + Vietnamese supplementary resources |

---

# 15. Exit criteria

Bạn hoàn thành Kubernetes core khi có thể:

- explain control plane vs worker node;
- explain Pod creation flow;
- reason desired/observed/reconciliation;
- create/debug Pod, Deployment, Job/CronJob;
- use ConfigMap/Secret/env/command/args;
- explain Deployment → ReplicaSet → Pod;
- expose workload with Service and debug EndpointSlice;
- use probes correctly;
- size requests/limits from measurement;
- reason scheduling constraints;
- use PVC/StorageClass at application level;
- apply RBAC/ServiceAccount/securityContext fundamentals;
- use NetworkPolicy basics;
- debug Pending/CrashLoop/ImagePull/NotReady/OOM/PVC/network issues;
- rollout and rollback application safely;
- explain when **not** to use Kubernetes;
- then map Kubernetes to AKS without confusing Azure constructs with Kubernetes constructs.

## Verification metadata

- Verified: 2026-08-28.
- Kubernetes official docs are canonical.
- Vietnamese sources supplied by the learner are supplementary mental-model/lab resources.
- CKAD domain weights checked against current CNCF certification page on 2026-08-28.
- Version-sensitive syntax/API must be checked against the active Kubernetes release used by the lab/exam environment.
