# Kubernetes Application Configuration & Scheduling

> [← Module overview](README.md) · [Workloads/Network/Storage](workloads-networking-and-storage.md) · [Debugging →](kubectl-debugging-and-ckad-practice.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;ConfigMap · Secret · Resources · Scheduling · SecurityContext</span>
  <span><strong>Audience</strong>&nbsp;Application developer / CKAD path</span>
</div>

Guide này tập trung vào cách một application khai báo **configuration, runtime contract và scheduling constraints** mà không hard-code environment vào image.

---

# 1. Command, args và environment

Container image có default `ENTRYPOINT/CMD`, nhưng Pod spec có thể override runtime command/args.

```yaml
containers:
  - name: worker
    image: registry.example/worker@sha256:...
    command: ["dotnet"]
    args: ["Worker.dll", "--mode", "payments"]
```

Use override khi deployment environment thực sự cần runtime composition. Đừng biến YAML thành shell script lớn.

Environment:

```yaml
env:
  - name: ASPNETCORE_ENVIRONMENT
    value: Production
```

For many values, prefer ConfigMap/Secret references.

---

# 2. ConfigMap

ConfigMap lưu non-sensitive configuration.

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: checkout-config
data:
  Feature__NewCheckout: "true"
  Payment__TimeoutSeconds: "5"
```

Consume:

```yaml
envFrom:
  - configMapRef:
      name: checkout-config
```

or individual key:

```yaml
env:
  - name: PAYMENT_TIMEOUT
    valueFrom:
      configMapKeyRef:
        name: checkout-config
        key: Payment__TimeoutSeconds
```

## Update semantics

Configuration delivered as environment variables is normally observed by a newly started container, not magically hot-reloaded into an existing process.

Mounted configuration can have different update behavior, but application reload semantics must still be designed/tested.

Production question:

```text
config changed
→ when does app observe it?
→ is restart required?
→ can old/new config coexist during rollout?
```

---

# 3. Secret

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: provider-secret
type: Opaque
stringData:
  ApiKey: replace-me
```

But:

```text
Kubernetes Secret
!= complete secret-management architecture
```

You still need:

```text
encryption at rest
RBAC
external secret store/workload identity where appropriate
rotation
audit
expiry
backup/recovery
```

Avoid committing real Secret manifests to Git.

For cloud services, prefer workload identity instead of creating a long-lived cloud credential secret when possible.

---

# 4. Downward API

Application can consume Pod metadata without duplicating it into ConfigMap.

```yaml
env:
  - name: POD_NAME
    valueFrom:
      fieldRef:
        fieldPath: metadata.name
  - name: POD_NAMESPACE
    valueFrom:
      fieldRef:
        fieldPath: metadata.namespace
```

Useful for diagnostics/correlation, but avoid turning dynamic Pod identity into business identity.

---

# 5. Requests and limits

```yaml
resources:
  requests:
    cpu: 250m
    memory: 256Mi
  limits:
    memory: 512Mi
```

## Requests

Scheduler uses requests to evaluate whether a Node has capacity.

```text
Pod request
≤ node allocatable remaining
```

## Limits

Memory limit can produce OOM kill when exceeded.

CPU limit/requests interact with cgroup/runtime scheduling semantics and can affect latency. Measure instead of blindly setting ratios.

## .NET note

For .NET workloads observe:

```text
GC heap behavior
working set
thread pool
CPU throttling
startup allocation
P95/P99 latency
OOMKilled events
```

---

# 6. LimitRange and ResourceQuota

Namespace policies can prevent unbounded workloads.

## LimitRange

Can define defaults/min/max constraints for resource requests/limits.

## ResourceQuota

Can cap namespace aggregate resources/object counts.

Mental model:

```text
individual Pod resource contract
+
namespace capacity guardrail
```

Failure symptom:

```text
kubectl apply rejected
→ quota exceeded / request invalid
```

Use:

```bash
kubectl describe resourcequota -n <namespace>
kubectl describe limitrange -n <namespace>
```

---

# 7. nodeSelector

Simple scheduling constraint:

```yaml
nodeSelector:
  workload: compute
```

Use only when node labels have stable ownership/governance.

Bad:

```text
hard-code random node hostname
```

because nodes are replaceable infrastructure.

---

# 8. Node affinity / anti-affinity

Affinity gives expressive label-based scheduling constraints/preferences.

Examples:

```text
require workload on SSD-labelled nodes
prefer same zone as something
avoid colocating replicas on same topology
```

Required constraints can leave Pods Pending if no node matches.

Always debug with:

```bash
kubectl describe pod <pod>
```

and inspect scheduling events.

---

# 9. Pod affinity/anti-affinity and topology spread

Goal may be:

```text
3 replicas
→ avoid all 3 on one node/zone
```

Possible tools:

```text
pod anti-affinity
topologySpreadConstraints
```

Don't add complex scheduling rules until you define the failure domain you are protecting:

```text
process?
node?
zone?
```

Hard constraints can make rollout impossible when cluster has insufficient topology/capacity.

---

# 10. Taints and tolerations

Mental model:

```text
Taint on Node
→ repel Pods by default

Toleration on Pod
→ Pod may be scheduled there
```

Toleration does **not** force a Pod onto that node; combine with labels/affinity if placement must be constrained.

Use cases:

```text
dedicated workload nodes
special hardware
system isolation
GPU nodes
```

---

# 11. Priority and preemption

PriorityClass can influence scheduling/preemption under resource pressure.

This is production-sensitive:

```text
high priority workload
→ may evict lower priority Pod
```

Do not label everything critical. A priority design needs service criticality and capacity reasoning.

---

# 12. ServiceAccount

Pod gets a ServiceAccount identity context for Kubernetes API/auth integrations.

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: payment-worker
```

Deployment:

```yaml
spec:
  template:
    spec:
      serviceAccountName: payment-worker
```

Default ServiceAccount should not automatically receive broad permissions.

Check:

```bash
kubectl auth can-i get secrets \
  --as=system:serviceaccount:checkout:payment-worker \
  -n checkout
```

---

# 13. SecurityContext

Application-level hardening:

```yaml
securityContext:
  runAsNonRoot: true
  seccompProfile:
    type: RuntimeDefault
```

Container:

```yaml
securityContext:
  allowPrivilegeEscalation: false
  capabilities:
    drop:
      - ALL
```

Exact policy depends on workload/image, but default direction should be:

```text
non-root
least Linux capabilities
no privilege escalation
read-only filesystem where app supports it
seccomp default
```

Test the app; security YAML copied blindly often breaks runtime or becomes cargo cult.

---

# 14. Multi-container Pod patterns

## Init container

Runs before app containers and must complete successfully.

Good uses:

```text
prepare files
wait/validate bounded prerequisite
one-time initialization scoped to Pod
```

Avoid infinite dependency wait loops hiding infrastructure issues.

## Sidecar

Runs alongside main container for tightly coupled supporting functionality.

Use only if lifecycle/resource coupling belongs inside one Pod.

Remember:

```text
containers in one Pod
→ share scheduling/lifecycle boundary
→ can share network namespace/volumes
```

Don't put independent microservices into one Pod.

---

# 15. Ephemeral and persistent volumes

## emptyDir

Pod-scoped ephemeral storage.

```text
Pod deleted
→ data not durable across replacement
```

Use for scratch/shared temporary files.

## PVC

Application declares storage requirement; storage class/provisioner maps to actual storage.

```text
Pod
→ PVC
→ PV
→ CSI/storage provider
```

Scheduling can depend on volume topology. Pending PVC can block Pod scheduling.

---

# 16. Deployment strategy

Rolling update defaults need capacity and readiness to work.

Review:

```text
maxSurge
maxUnavailable
readiness
terminationGracePeriodSeconds
preStop if required
```

If cluster is fully packed and rollout requires surge Pod:

```text
new Pod Pending
→ rollout stalls
```

Capacity plan must include deployment overhead.

---

# 17. Graceful shutdown

When Pod terminates:

```text
SIGTERM
→ app stops taking new work
→ finishes/drains bounded work
→ exits before grace period
```

.NET should wire host cancellation through background services/HTTP lifecycle.

Don't rely on `SIGKILL` as normal shutdown.

---

# 18. CKAD-oriented exercises

Practice without copy/paste templates:

1. Create Deployment with command/args override.
2. Inject one ConfigMap key and one Secret key.
3. Add requests/limits and explain scheduler effect.
4. Create init container writing into `emptyDir`; main container reads it.
5. Add ServiceAccount and verify RBAC with `kubectl auth can-i`.
6. Add `runAsNonRoot` and remove capabilities.
7. Schedule Pod only to a labelled node.
8. Add a taint and toleration; explain why toleration isn't node selection.
9. Add topology spread for replicas.
10. Create Job and CronJob with correct restart policy.

---

# 19. Common failures

| Symptom | Likely area |
|---|---|
| Pod Pending | requests too large, affinity, taint, PVC, quota |
| Create rejected | ResourceQuota/LimitRange/admission |
| App config old | env/config reload semantics |
| Permission denied | ServiceAccount/RBAC/securityContext |
| OOMKilled | memory limit/workload behavior |
| rollout stuck | readiness, capacity, scheduling constraints |
| container won't start | command/args/image/user/filesystem |

---

# 20. Review checklist

- [ ] Image stays environment-agnostic where practical.
- [ ] ConfigMap only contains non-sensitive config.
- [ ] Secrets aren't committed in plaintext.
- [ ] Workload identity preferred for cloud auth where available.
- [ ] Requests/limits are measured.
- [ ] Namespace quota exists where multi-team capacity needs guardrails.
- [ ] Scheduling constraints have explicit reason.
- [ ] Taint/toleration semantics understood.
- [ ] ServiceAccount is workload-specific where permissions differ.
- [ ] SecurityContext is tested, not cargo-culted.
- [ ] Rollout has surge capacity and graceful shutdown.

## Official references

- <https://kubernetes.io/docs/concepts/configuration/>
- <https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/>
- <https://kubernetes.io/docs/concepts/scheduling-eviction/>
- <https://kubernetes.io/docs/concepts/security/>

## Verification metadata

- Verified: 2026-08-28.
- This guide targets stable concepts; exact API fields/version status must be checked against the cluster's active Kubernetes version.
