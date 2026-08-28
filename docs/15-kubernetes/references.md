# References — Module 15 Kubernetes

> [← Module overview](README.md)

## Source policy

Kubernetes changes continuously. Core behavior, API status, security and version-sensitive claims use **kubernetes.io / CNCF / Linux Foundation** as canonical sources.

Vietnamese community resources are useful for explanation, lab sequence and beginner mental models, but they do not override current official behavior. Older articles may use deprecated CLI flags, Docker-era runtime assumptions or old API versions; copy the concept, not stale syntax.

---

## Official Kubernetes sources

### Overview / architecture / API

- [Kubernetes Documentation](https://kubernetes.io/docs/home/)
- [Kubernetes Overview](https://kubernetes.io/docs/concepts/overview/)
- [Kubernetes Components](https://kubernetes.io/docs/concepts/overview/components/)
- [Kubernetes API](https://kubernetes.io/docs/concepts/overview/kubernetes-api/)
- [Kubernetes Objects](https://kubernetes.io/docs/concepts/overview/working-with-objects/)
- [kubectl reference](https://kubernetes.io/docs/reference/kubectl/)

### Workloads

- [Workloads](https://kubernetes.io/docs/concepts/workloads/)
- [Pods](https://kubernetes.io/docs/concepts/workloads/pods/)
- [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [StatefulSets](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/)
- [DaemonSets](https://kubernetes.io/docs/concepts/workloads/controllers/daemonset/)
- [Jobs](https://kubernetes.io/docs/concepts/workloads/controllers/job/)
- [CronJobs](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/)

### Configuration / resources / scheduling

- [Configuration](https://kubernetes.io/docs/concepts/configuration/)
- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Resource management](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/)
- [Scheduling, preemption and eviction](https://kubernetes.io/docs/concepts/scheduling-eviction/)
- [Assign Pods to Nodes](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/)
- [Taints and tolerations](https://kubernetes.io/docs/concepts/scheduling-eviction/taint-and-toleration/)
- [Topology spread constraints](https://kubernetes.io/docs/concepts/scheduling-eviction/topology-spread-constraints/)

### Networking

- [Services, Load Balancing and Networking](https://kubernetes.io/docs/concepts/services-networking/)
- [Service](https://kubernetes.io/docs/concepts/services-networking/service/)
- [DNS for Services and Pods](https://kubernetes.io/docs/concepts/services-networking/dns-pod-service/)
- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Ingress](https://kubernetes.io/docs/concepts/services-networking/ingress/)
- [Gateway API](https://kubernetes.io/docs/concepts/services-networking/gateway/)

### Storage

- [Storage](https://kubernetes.io/docs/concepts/storage/)
- [Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/)
- [Storage Classes](https://kubernetes.io/docs/concepts/storage/storage-classes/)
- [CSI](https://kubernetes.io/docs/concepts/storage/volumes/#csi)

### Security

- [Kubernetes Security](https://kubernetes.io/docs/concepts/security/)
- [RBAC](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Service Accounts](https://kubernetes.io/docs/concepts/security/service-accounts/)
- [Pod Security Standards](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
- [Security Context](https://kubernetes.io/docs/tasks/configure-pod-container/security-context/)

### Operations / observability / debugging

- [Observability](https://kubernetes.io/docs/concepts/cluster-administration/observability/)
- [Application debugging](https://kubernetes.io/docs/tasks/debug/debug-application/)
- [Debug running Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
- [kubectl logs](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_logs/)
- [Probes](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/)

### Releases

- [Kubernetes releases](https://kubernetes.io/releases/)
- [Version skew policy](https://kubernetes.io/releases/version-skew-policy/)

---

## CKAD official sources

- [CNCF — Certified Kubernetes Application Developer](https://www.cncf.io/training/certification/ckad/)
- [Linux Foundation — CKAD](https://training.linuxfoundation.org/certification/certified-kubernetes-application-developer-ckad/)
- [CNCF open-source curriculum repository](https://github.com/cncf/curriculum)

Current domain weights verified 2026-08-28:

| Domain | Weight |
|---|---:|
| Application Design and Build | 20% |
| Application Deployment | 20% |
| Application Observability and Maintenance | 15% |
| Application Environment, Configuration and Security | 25% |
| Services and Networking | 20% |

CNCF plans quarterly exam updates aligned with Kubernetes releases; verify exam environment before exam day.

---

## AKS / Azure mapping

AKS lives inside this Kubernetes module as provider-specific mapping, while generic Azure services remain in Module 14.

- [Azure Kubernetes Service documentation](https://learn.microsoft.com/en-us/azure/aks/)
- [AKS core concepts](https://learn.microsoft.com/en-us/azure/aks/core-aks-concepts)
- [AKS Free/Standard/Premium pricing tiers](https://learn.microsoft.com/en-us/azure/aks/free-standard-pricing-tiers)
- [AKS architecture starting point](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-start-here)
- [AKS baseline architecture](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)
- [AKS microservices reference architecture](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-microservices/aks-microservices)
- [AKS Well-Architected service guide](https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-kubernetes-service)

---

## Vietnamese supplementary resources supplied for this module

### Viblo — Kubernetes là gì?

- [K8S Phần 1 — Kubernetes là gì?](https://viblo.asia/p/k8s-phan-1-kubernetes-la-gi-bJzKmAyDK9N)

Useful for:

```text
why orchestration exists
control plane vs worker node
api-server / etcd / scheduler / controller / kubelet
step-by-step mental model of Pod creation
```

The article was published in 2022. Some CLI/runtime examples can be stale; compare commands/API/runtime assumptions with current official docs.

### Viblo — tổng quan thành phần

- [K8S basic — Tổng quan các thành phần của Kubernetes](https://viblo.asia/p/k8s-basic-tong-quan-cac-thanh-phan-cua-kubernetes-aAY4ql7qVPw)

Useful as Vietnamese explanation of architecture/components for beginners. Official component definitions remain canonical.

### DevOps.vn — từ cơ bản đến CKAD

- [Học Kubernetes từ cơ bản đến chứng chỉ CKAD](https://devops.vn/series/hoc-kubernetes-tu-co-ban-den-chung-chi-ckad-dan98/)

Useful stepwise lab topics include:

```text
cluster + kubectl
Pods
ConfigMap / Secret
environment / command / args
Deployment / ReplicaSet
multi-container Pod
sidecar / init container
resources
logs/events
service/networking
CKAD-style practice
```

Use the series for practice sequencing; use current CNCF CKAD page for official domain weights and exam changes.

---

## Source decision matrix

| Decision | Canonical source | Supplementary source |
|---|---|---|
| API behavior/version | kubernetes.io | blogs only for explanation |
| Architecture components | kubernetes.io | Viblo mental models |
| Hands-on beginner sequence | official tasks/tutorials | Viblo / DevOps.vn |
| CKAD domains/weights | CNCF/Linux Foundation | community study guides |
| AKS-specific behavior | Microsoft Learn | community labs |
| Production performance | measurement + official limits | community experience as hypothesis |
| Security | official security docs + threat model | community examples |

---

## Related repository modules

- [Module 12 — Docker](../12-docker/README.md)
- [Module 13 — DevOps & IaC](../13-devops-iac/README.md)
- [Module 14 — Azure](../14-cloud/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices](../18-microservices-architecture/README.md)
- [Module 24 — System Design](../24-system-design/README.md)

## Verification metadata

- Verified: 2026-08-28.
- Official Kubernetes Components page last-modified 2026-05-30 at verification time.
- CKAD current domain weights checked from CNCF on 2026-08-28.
- User-supplied Vietnamese resources are explicitly supplementary because publication/version age differs.
