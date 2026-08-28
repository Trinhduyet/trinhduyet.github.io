# Kubernetes kubectl Troubleshooting & CKAD Practice

> [← Configuration & Scheduling](application-configuration-and-scheduling.md) · [Module overview](README.md) · [Security & Operations →](kubernetes-security-observability-and-operations.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;kubectl · Failure Analysis · Application Troubleshooting · CKAD</span>
  <span><strong>Mode</strong>&nbsp;command-line first</span>
</div>

Bạn chưa thật sự biết Kubernetes nếu chỉ tạo được YAML. Production và CKAD đều yêu cầu khả năng **quan sát object hiện tại, đọc Events/logs, sửa nhanh và xác minh outcome**.

---

# 1. Debugging mental model

Đi theo thứ tự:

```text
What changed?
→ Which Kubernetes object owns the failing behavior?
→ Desired spec?
→ Current status/conditions?
→ Events?
→ Logs?
→ Node/resource scheduling?
→ Service/endpoints/DNS/network?
→ Config/Secret/RBAC?
→ Storage?
→ Verify after fix
```

Không bắt đầu bằng `kubectl delete pod` cho mọi lỗi.

---

# 2. Command toolbox

## Discover

```bash
kubectl get pods -A
kubectl get deploy,rs,pods -n <ns>
kubectl get svc,endpointslices -n <ns>
kubectl api-resources
kubectl explain deployment.spec.template.spec
```

## Inspect

```bash
kubectl describe pod <pod> -n <ns>
kubectl get pod <pod> -n <ns> -o yaml
kubectl get events -n <ns> --sort-by=.lastTimestamp
```

## Logs

```bash
kubectl logs <pod> -n <ns>
kubectl logs <pod> -c <container> -n <ns>
kubectl logs <pod> --previous -n <ns>
kubectl logs deployment/<name> -n <ns>
```

## Runtime access

```bash
kubectl exec -it <pod> -n <ns> -- sh
kubectl port-forward svc/<service> 8080:80 -n <ns>
```

## Resource/identity

```bash
kubectl top pod -n <ns>
kubectl top node
kubectl auth can-i get pods -n <ns>
kubectl auth can-i get secrets \
  --as=system:serviceaccount:<ns>:<sa> -n <ns>
```

## Rollout

```bash
kubectl rollout status deployment/<name>
kubectl rollout history deployment/<name>
kubectl rollout undo deployment/<name>
```

---

# 3. Pod Pending

Start:

```bash
kubectl describe pod <pod>
```

Look at Events.

Common causes:

```text
Insufficient cpu
Insufficient memory
nodeSelector/affinity mismatch
taint without toleration
unbound PVC
ResourceQuota
node unavailable
```

## Resource case

Pod requests:

```yaml
requests:
  cpu: "8"
  memory: 32Gi
```

Cluster only has small nodes.

Fix is not necessarily lower requests blindly. Validate actual need vs cluster capacity.

## Scheduling case

```text
required node affinity says zone=A
all nodes in zone=B
→ Pending by design
```

Inspect labels:

```bash
kubectl get nodes --show-labels
```

---

# 4. ImagePullBackOff / ErrImagePull

Check:

```bash
kubectl describe pod <pod>
```

Common causes:

```text
wrong repository/tag/digest
registry auth
network/DNS to registry
image doesn't exist
rate limit / permission
```

Verify Deployment image:

```bash
kubectl get deployment <name> \
  -o jsonpath='{.spec.template.spec.containers[*].image}'
```

Production prefers immutable digest for traceability.

---

# 5. CrashLoopBackOff

Means container starts and repeatedly exits/fails.

Commands:

```bash
kubectl logs <pod>
kubectl logs <pod> --previous
kubectl describe pod <pod>
```

Look for:

```text
bad command/args
missing config
permission denied
startup exception
bad dependency assumption
failed migration
probe restart
```

`--previous` is critical because current container may have restarted.

---

# 6. OOMKilled

Check:

```bash
kubectl describe pod <pod>
kubectl get pod <pod> -o jsonpath='{.status.containerStatuses[0].lastState}'
```

Reason may show `OOMKilled`.

Investigate:

```text
memory limit
actual working set
memory leak
startup peak
cache size
.NET GC behavior
concurrency
```

Do not just double the memory forever; measure.

---

# 7. Running but NotReady

Pod phase can be Running while readiness is false.

Check:

```bash
kubectl describe pod <pod>
kubectl get pod <pod> -o wide
kubectl logs <pod>
```

Potential causes:

```text
readiness endpoint returns failure
wrong probe port/path
app listens on wrong address
startup not complete
dependency policy
```

Then verify EndpointSlice:

```bash
kubectl get endpointslices -l kubernetes.io/service-name=<service>
```

A not-ready Pod may not receive Service traffic, which is correct behavior.

---

# 8. Service not reachable

Debug path:

```text
Service exists?
↓
selector matches Pod labels?
↓
EndpointSlice has endpoints?
↓
Pod Ready?
↓
port vs targetPort correct?
↓
app listens on expected port/address?
↓
DNS?
↓
NetworkPolicy?
```

Commands:

```bash
kubectl get svc <svc> -o yaml
kubectl get pods --show-labels
kubectl get endpointslices
kubectl describe svc <svc>
```

Common failure:

```yaml
Service selector:
  app: orders

Pod label:
  app: order-api
```

DNS can resolve Service while it has no working endpoints.

---

# 9. DNS troubleshooting

From a debug Pod or app container:

```bash
nslookup demo-api
nslookup demo-api.<namespace>.svc.cluster.local
```

Check:

```text
Service exists
namespace correct
CoreDNS/addon health
NetworkPolicy allowing DNS
search domains/resolv.conf
```

Do not hard-code Pod IPs as service contracts.

---

# 10. NetworkPolicy failure

Symptom:

```text
Service/EndpointSlice look correct
Pods Ready
connection timeout
```

Inspect policies:

```bash
kubectl get networkpolicy -A
kubectl describe networkpolicy <name> -n <ns>
```

Reason from allowed flow:

```text
source namespace/pod labels
→ destination labels
→ port/protocol
```

Test both allowed and denied paths.

---

# 11. PVC Pending / mount failure

Commands:

```bash
kubectl get pvc,pv
kubectl describe pvc <name>
kubectl describe pod <pod>
kubectl get storageclass
```

Common causes:

```text
no default StorageClass
requested mode/size unsupported
provisioner failure
topology mismatch
volume attachment issue
access mode conflict
```

Application developer needs to understand storage contract even if cluster admin owns CSI setup.

---

# 12. RBAC / Forbidden

Symptom:

```text
Error from server (Forbidden)
```

Check current identity:

```bash
kubectl auth can-i <verb> <resource> -n <ns>
```

Check workload ServiceAccount:

```bash
kubectl get pod <pod> -o jsonpath='{.spec.serviceAccountName}'
```

Then inspect Role/RoleBinding or ClusterRole/ClusterRoleBinding.

Do not solve by granting `cluster-admin`.

---

# 13. Bad rollout

```bash
kubectl rollout status deployment/api
kubectl describe deployment api
kubectl get rs
kubectl get pods
```

Potential causes:

```text
new image fails readiness
image pull fails
no surge capacity
quota blocks new Pod
new config invalid
migration incompatible
```

Rollback:

```bash
kubectl rollout undo deployment/api
```

But validate database/data side effects separately.

---

# 14. Node pressure / eviction

Pods may be evicted due to node resource pressure.

Inspect:

```bash
kubectl describe node <node>
kubectl get events --all-namespaces --sort-by=.lastTimestamp
```

Signals:

```text
MemoryPressure
DiskPressure
PIDPressure
```

Application design contributes through requests/limits, ephemeral storage use and runaway processes.

---

# 15. Ephemeral debug containers

Where supported and permitted, `kubectl debug` can help troubleshoot a Pod/node without modifying the original image.

Always respect cluster security policy; do not assume production allows privileged debug containers.

Use case:

```text
minimal/distroless app image
→ no shell/network tools
→ attach controlled debug environment
```

---

# 16. Fast YAML generation

For hands-on work/CKAD, imperative commands can generate baseline YAML faster, then you edit declaratively.

Examples:

```bash
kubectl create deployment web --image=nginx --dry-run=client -o yaml > deployment.yaml

kubectl create service clusterip web \
  --tcp=80:8080 --dry-run=client -o yaml > service.yaml

kubectl create configmap app-config \
  --from-literal=MODE=prod --dry-run=client -o yaml
```

Do not memorize every field; know `kubectl explain` and official docs navigation.

---

# 17. CKAD current domains

Current CNCF weighting:

```text
Application Design and Build                      20%
Application Deployment                            20%
Application Observability and Maintenance         15%
Application Environment, Configuration & Security 25%
Services and Networking                           20%
```

Official: <https://www.cncf.io/training/certification/ckad/>

## Application Design & Build

Practice:

```text
Pod/container image
Deployment/Job/CronJob
multi-container patterns
volumes
```

## Deployment

Practice:

```text
rolling update
rollback
blue/green/canary concepts
Helm
Kustomize
```

## Observability & Maintenance

Practice:

```text
probes
logs
events
kubectl troubleshooting
API deprecation awareness
```

## Environment, Configuration & Security

Practice:

```text
ConfigMap/Secret
requests/limits/quota
ServiceAccount
securityContext
RBAC/admission concepts
CRD/operator awareness
```

## Services & Networking

Practice:

```text
Service
Ingress
NetworkPolicy
troubleshoot connectivity
```

---

# 18. Practice scenarios

## Scenario A — broken API

Given:

```text
Deployment desired=3
Ready=0
```

You must find:

```text
wrong readiness path
```

Fix, verify Ready=3 and Service endpoints.

## Scenario B — Pending worker

Given:

```text
Pod Pending
```

Find:

```text
node taint
```

Choose whether to tolerate it or schedule elsewhere; explain reason.

## Scenario C — config change

Create ConfigMap, mount/inject it, update config, explain when running process observes change.

## Scenario D — secret + ServiceAccount

Create workload-specific ServiceAccount and a Secret; prove RBAC doesn't allow unrelated secret listing.

## Scenario E — broken service

Fix selector/targetPort and verify connectivity.

## Scenario F — resource failure

Cause OOMKilled in lab, observe last state, then set measured limit.

## Scenario G — rollout

Deploy bad image, observe stall, rollback, verify history.

---

# 19. Time-efficient workflow

```text
read task/failure
→ set namespace/context
→ inspect existing resources
→ use dry-run/explain
→ edit minimal fields
→ apply
→ verify exact outcome
```

Always verify:

```bash
kubectl get ...
kubectl describe ...
```

Don't assume command exit code proves application works.

---

# 20. Troubleshooting checklist

- [ ] Context/namespace correct.
- [ ] Owner object identified.
- [ ] Desired spec inspected.
- [ ] Status/conditions inspected.
- [ ] Events read.
- [ ] Current and previous logs read.
- [ ] Scheduling/resources checked.
- [ ] Service selector/EndpointSlice checked.
- [ ] DNS/NetworkPolicy checked.
- [ ] Config/Secret/RBAC checked.
- [ ] PVC/storage checked.
- [ ] Recent rollout/change identified.
- [ ] Fix verified by observable outcome.

## Supplementary Vietnamese learning path

The learner-supplied DevOps.vn series is useful for stepwise kubectl/CKAD practice:

<https://devops.vn/series/hoc-kubernetes-tu-co-ban-den-chung-chi-ckad-dan98/>

Use it for exercises; use CNCF/Kubernetes official sources for current exam/API truth.

## Verification metadata

- Verified: 2026-08-28.
- CKAD weights checked against current CNCF certification page.
- Exam environment/API versions can change quarterly; check official candidate resources before exam day.
