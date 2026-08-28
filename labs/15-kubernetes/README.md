# Kubernetes Core Lab — Deployment, Service, Probes & Debugging

> Goal: biến các khái niệm Kubernetes core thành object/state nhìn thấy được. Lab không yêu cầu AKS; dùng bất kỳ local/test cluster nào có `kubectl` access.

## Prerequisites

```text
kubectl
+ Kubernetes cluster
+ permission create namespace/workloads/services
```

Bạn có thể dùng kind, minikube, k3d, Docker Desktop Kubernetes hoặc một non-production test cluster.

Kiểm tra trước:

```bash
kubectl version
kubectl get nodes
```

Không chạy failure drills này trên production cluster.

---

# 1. Deploy baseline

Từ repo root:

```bash
kubectl apply -k labs/15-kubernetes
kubectl get all -n k8s-basics
```

Expected relationship:

```text
Deployment/web
  ↓ owns/manages rollout
ReplicaSet/web-xxxxx
  ↓ keeps replica count
Pod/web-xxxxx-a
Pod/web-xxxxx-b
  ↓ each contains
Container nginx
```

Inspect owner references instead of memorizing the diagram:

```bash
kubectl get deployment web -n k8s-basics
kubectl get rs -n k8s-basics
kubectl get pods -n k8s-basics -o wide
kubectl describe deployment web -n k8s-basics
```

Evidence to save:

```text
Deployment desired/available replicas
ReplicaSet name
Pod names + Nodes
```

---

# 2. Observe Service → selector → Ready Pods

```bash
kubectl get service web -n k8s-basics
kubectl get pods -n k8s-basics --show-labels
kubectl get endpointslices -n k8s-basics \
  -l kubernetes.io/service-name=web
```

Mental model:

```text
Service/web
  ↓ selector app=web
Pods with label app=web
  ↓ readiness
Ready endpoints
```

The Service does not own Pods. It selects eligible endpoints by labels.

Port-forward the stable Service endpoint:

```bash
kubectl port-forward -n k8s-basics service/web 8080:80
```

Then from another terminal:

```bash
curl http://localhost:8080/
```

Expected: nginx welcome HTML.

---

# 3. Reconciliation drill — delete a Pod

List Pods:

```bash
kubectl get pods -n k8s-basics
```

Delete one:

```bash
kubectl delete pod -n k8s-basics <pod-name>
```

Immediately observe:

```bash
kubectl get pods -n k8s-basics -w
```

Expected:

```text
one Pod terminates
ReplicaSet observes replicas < desired
new Pod is created
replica count returns to 2
```

Key lesson:

```text
You did not ask Kubernetes to recreate that exact Pod.
You declared desired replica state through Deployment/ReplicaSet.
Controllers reconcile the difference.
```

---

# 4. ImagePullBackOff drill

Break the image intentionally:

```bash
kubectl set image deployment/web \
  web=nginx:this-tag-does-not-exist \
  -n k8s-basics
```

Watch rollout:

```bash
kubectl rollout status deployment/web -n k8s-basics
```

In another terminal inspect:

```bash
kubectl get pods -n k8s-basics
kubectl describe pod -n k8s-basics <new-pod-name>
kubectl get events -n k8s-basics --sort-by=.lastTimestamp
```

Find evidence such as:

```text
ErrImagePull
ImagePullBackOff
image/tag pull failure event
```

Recover:

```bash
kubectl rollout undo deployment/web -n k8s-basics
kubectl rollout status deployment/web -n k8s-basics
```

Lesson:

```text
Deployment can exist
while rollout is unhealthy.
"kubectl apply succeeded" != application is healthy.
```

---

# 5. Service selector failure drill

Break only networking discovery, not the Pods:

```bash
kubectl patch service web -n k8s-basics \
  -p '{"spec":{"selector":{"app":"does-not-exist"}}}'
```

Check:

```bash
kubectl get pods -n k8s-basics
kubectl get service web -n k8s-basics
kubectl get endpointslices -n k8s-basics \
  -l kubernetes.io/service-name=web
```

Expected mental state:

```text
Pods Running/Ready ✓
Service exists ✓
Service has usable backend endpoints ✗
```

This is why debugging only `kubectl get pods` is insufficient.

Recover from source-of-truth manifests:

```bash
kubectl apply -k labs/15-kubernetes
```

Verify endpoints return.

---

# 6. Readiness failure drill

Patch the readiness path to an HTTP 404:

```bash
kubectl patch deployment web -n k8s-basics --type=strategic \
  -p '{"spec":{"template":{"spec":{"containers":[{"name":"web","readinessProbe":{"httpGet":{"path":"/does-not-exist","port":"http"}}}]}}}}'
```

Observe:

```bash
kubectl get pods -n k8s-basics -w
```

Then:

```bash
kubectl describe pod -n k8s-basics <new-pod-name>
kubectl get endpointslices -n k8s-basics \
  -l kubernetes.io/service-name=web
```

Expected:

```text
container process can be Running
but Pod Ready = false
and it should not be a ready Service backend
```

This distinguishes:

```text
process exists
!=
workload should receive traffic
```

Recover:

```bash
kubectl apply -k labs/15-kubernetes
kubectl rollout status deployment/web -n k8s-basics
```

---

# 7. Requests / limits observation

Inspect resource contract:

```bash
kubectl get deployment web -n k8s-basics -o yaml
```

The manifest declares:

```text
requests
→ scheduler capacity signal

limits
→ runtime resource boundary
```

Do not copy these nginx learning numbers into a .NET production API. Production values must come from workload measurement and rollout capacity planning.

If metrics-server exists:

```bash
kubectl top pods -n k8s-basics
kubectl top nodes
```

If it does not exist, that absence itself shows `kubectl top` depends on a metrics pipeline; Kubernetes core does not magically provide every metric.

---

# 8. Scale drill

```bash
kubectl scale deployment web -n k8s-basics --replicas=4
kubectl get deployment,rs,pods -n k8s-basics
```

Expected:

```text
Deployment desired replicas = 4
ReplicaSet reconciles to 4 Pods
Service automatically selects new Ready Pods by label
```

Restore source state:

```bash
kubectl apply -k labs/15-kubernetes
```

This returns replicas to `2` because the committed manifest is desired state for this lab.

---

# 9. Rollout history

```bash
kubectl rollout history deployment/web -n k8s-basics
kubectl rollout restart deployment/web -n k8s-basics
kubectl rollout status deployment/web -n k8s-basics
kubectl rollout history deployment/web -n k8s-basics
```

Observe old/new ReplicaSets:

```bash
kubectl get rs -n k8s-basics
```

Mental model:

```text
Deployment rollout
→ creates/uses ReplicaSet revisions
→ shifts desired replicas between revisions
→ Pods are replaced
```

Application rollback is not database rollback. Real application migrations must be compatible with overlapping old/new versions or use an explicit migration strategy.

---

# 10. Debugging order

When something breaks, avoid random commands.

```text
1. Which object fails?
2. What desired state?
3. Current status/conditions?
4. Events?
5. Logs/current + previous?
6. Scheduling/resources?
7. Service/EndpointSlice/DNS?
8. Config/Secret/identity?
9. Storage?
10. Recent rollout/change?
```

Core toolbox:

```bash
kubectl get <resource> -o wide
kubectl describe <resource>
kubectl get events --sort-by=.lastTimestamp
kubectl logs <pod>
kubectl logs <pod> --previous
kubectl get endpointslices
kubectl rollout status deployment/<name>
kubectl rollout history deployment/<name>
```

---

# 11. Map concepts to what you just observed

| Concept | Evidence in this lab |
|---|---|
| Cluster | `kubectl get nodes` + all objects share one API/cluster context |
| Control Plane | accepts desired objects and runs reconciliation/scheduling |
| kube-apiserver | `kubectl` API boundary |
| etcd | control-plane state store concept; not directly manipulated in app lab |
| scheduler | Pods receive Node assignments |
| kubelet | node agent materializes/reports assigned Pods |
| Pod | disposable runtime unit |
| Deployment | rollout + desired replica object |
| ReplicaSet | maintains replica count/revision |
| Service | stable network endpoint |
| Labels/selectors | `app=web` joins Service to Pods |
| DNS | Service name is cluster-discoverable when DNS is installed |
| ConfigMap/Secret | not required in baseline; add in next exercise |
| Requests/limits | resource scheduling/runtime contract |
| Readiness | traffic eligibility |
| Liveness | restart signal for irrecoverable process state |
| PVC/StorageClass | not needed for stateless nginx; learn with stateful exercise |
| ServiceAccount/RBAC | cluster/API identity authorization; next security exercise |
| Rollout/Rollback | Deployment revision behavior |
| HPA | not enabled here; requires metrics + autoscaler object |
| kubectl debugging | get/describe/events/rollout/endpoints evidence |

The lab intentionally does **not** pretend every concept needs an object in one exercise. PVC/RBAC/HPA deserve separate scenarios because adding them without a problem makes the YAML harder to understand.

---

# 12. Evidence checklist

Save enough output to explain, not screenshots of every command.

```text
[ ] Deployment → ReplicaSet → Pod relationship
[ ] two Pods placed on Nodes
[ ] Service selector + EndpointSlice
[ ] Pod deletion reconciles
[ ] ImagePullBackOff root cause from Events
[ ] wrong Service selector produces no usable endpoints
[ ] readiness failure removes traffic eligibility
[ ] rollout history / ReplicaSet revisions
[ ] one paragraph: requests vs limits
[ ] one paragraph: readiness vs liveness
```

---

# 13. Cleanup

```bash
kubectl delete namespace k8s-basics
```

Verify:

```bash
kubectl get namespace k8s-basics
```

Expected: namespace no longer exists after deletion completes.

## Learn next

- [Module 15 Overview](../../docs/15-kubernetes/README.md)
- [Architecture & Reconciliation](../../docs/15-kubernetes/cluster-architecture-and-reconciliation.md)
- [Workloads, Networking & Storage](../../docs/15-kubernetes/workloads-networking-and-storage.md)
- [Application Configuration & Scheduling](../../docs/15-kubernetes/application-configuration-and-scheduling.md)
- [kubectl Debugging & CKAD Practice](../../docs/15-kubernetes/kubectl-debugging-and-ckad-practice.md)

## Lab maturity

- Introduced: 2026-08-28.
- Scope: Kubernetes core application mental model and failure/debug loop.
- Not covered deeply: ConfigMap/Secret, PVC, RBAC, NetworkPolicy, HPA, AKS provider mapping.
- Run only on local/non-production learning clusters.
