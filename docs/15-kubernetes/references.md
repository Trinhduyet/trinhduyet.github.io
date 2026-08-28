# References — Module 15 Kubernetes

> [← Module overview](README.md) · [DevOps → Kubernetes Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

## Source policy

Kubernetes core behavior, API status, security and version-sensitive claims use **kubernetes.io / CNCF / Linux Foundation** as canonical sources.

Roadmap.sh and Vietnamese community resources are useful for coverage, learning order and explanation, but they do not override official behavior. GitOps/packaging claims use official Helm, Argo CD and Flux documentation.

---

## Kubernetes official sources

### Architecture / API

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kubernetes Components](https://kubernetes.io/docs/concepts/overview/components/)
- [Kubernetes API](https://kubernetes.io/docs/concepts/overview/kubernetes-api/)
- [Kubernetes Objects](https://kubernetes.io/docs/concepts/overview/working-with-objects/)
- [kubectl reference](https://kubernetes.io/docs/reference/kubectl/)

### Workloads / configuration / scheduling

- [Workloads](https://kubernetes.io/docs/concepts/workloads/)
- [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Jobs](https://kubernetes.io/docs/concepts/workloads/controllers/job/)
- [CronJobs](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/)
- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Resource management](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/)
- [Scheduling, preemption and eviction](https://kubernetes.io/docs/concepts/scheduling-eviction/)
- [Horizontal Pod Autoscaling](https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/)

### Networking / storage

- [Services, Load Balancing and Networking](https://kubernetes.io/docs/concepts/services-networking/)
- [Service](https://kubernetes.io/docs/concepts/services-networking/service/)
- [DNS for Services and Pods](https://kubernetes.io/docs/concepts/services-networking/dns-pod-service/)
- [NetworkPolicy](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Gateway API concept](https://kubernetes.io/docs/concepts/services-networking/gateway/)
- [Storage](https://kubernetes.io/docs/concepts/storage/)
- [Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/)
- [Storage Classes](https://kubernetes.io/docs/concepts/storage/storage-classes/)

### Security / operations / debugging

- [Kubernetes Security](https://kubernetes.io/docs/concepts/security/)
- [RBAC](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Service Accounts](https://kubernetes.io/docs/concepts/security/service-accounts/)
- [Pod Security Standards](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
- [Probes](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/)
- [Observability](https://kubernetes.io/docs/concepts/cluster-administration/observability/)
- [Debug applications](https://kubernetes.io/docs/tasks/debug/debug-application/)

### Releases

- [Kubernetes releases](https://kubernetes.io/releases/)
- [Version skew policy](https://kubernetes.io/releases/version-skew-policy/)

---

## DevOps delivery integration

These tools are not Kubernetes core, but commonly connect CI/CD to Kubernetes environments.

- [Helm documentation](https://helm.sh/docs/)
- [Kustomize documentation](https://kubectl.docs.kubernetes.io/references/kustomize/)
- [Argo CD — Declarative GitOps CD for Kubernetes](https://argo-cd.readthedocs.io/en/stable/)
- [Flux](https://fluxcd.io/flux/)

Mental model:

```text
CI builds/tests artifact
→ registry
→ manifests/Helm/Kustomize
→ push CD or GitOps controller
→ Kubernetes API
→ reconciliation
```

See [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md).

---

## Learning breadth — roadmap.sh

- [DevOps Roadmap](https://roadmap.sh/devops)
- [Kubernetes Roadmap](https://roadmap.sh/kubernetes)

At verification time, roadmap.sh describes DevOps as a broad delivery/operations discipline involving automation, containerization, cloud, CI/CD, infrastructure management and monitoring/logging. The Kubernetes roadmap is a separate deep path for orchestration. This repository uses the same relationship:

```text
DevOps = lifecycle breadth
Kubernetes = orchestration specialization
```

---

## CKAD official sources

- [CNCF — Certified Kubernetes Application Developer](https://www.cncf.io/training/certification/ckad/)
- [Linux Foundation — CKAD](https://training.linuxfoundation.org/certification/certified-kubernetes-application-developer-ckad/)
- [CNCF curriculum repository](https://github.com/cncf/curriculum)

Verify exam domains/environment near exam date because Kubernetes and certification content evolve.

---

## AKS / Azure mapping

AKS stays inside this Kubernetes module as provider-specific mapping, while generic Azure services remain in Module 14.

- [Azure Kubernetes Service](https://learn.microsoft.com/en-us/azure/aks/)
- [AKS core concepts](https://learn.microsoft.com/en-us/azure/aks/core-aks-concepts)
- [AKS pricing tiers](https://learn.microsoft.com/en-us/azure/aks/free-standard-pricing-tiers)
- [AKS architecture starting point](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-start-here)
- [AKS baseline architecture](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks)
- [AKS Well-Architected guide](https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-kubernetes-service)

---

## Vietnamese supplementary resources

### Viblo

- [K8S Phần 1 — Kubernetes là gì?](https://viblo.asia/p/k8s-phan-1-kubernetes-la-gi-bJzKmAyDK9N)
- [K8S basic — Tổng quan các thành phần của Kubernetes](https://viblo.asia/p/k8s-basic-tong-quan-cac-thanh-phan-cua-kubernetes-aAY4ql7qVPw)

Useful for beginner architecture mental models. Re-check commands/API/runtime assumptions against current official docs.

### DevOps.vn

- [Học Kubernetes từ cơ bản đến chứng chỉ CKAD](https://devops.vn/series/hoc-kubernetes-tu-co-ban-den-chung-chi-ckad-dan98/)

Useful for practice sequencing around Pods, ConfigMap/Secret, Deployment/ReplicaSet, multi-container workloads, resources, logging/networking and CKAD-style tasks.

---

## Related repository modules

- [Testing & Code Review](../08-testing-code-review/README.md)
- [Security & DevSecOps](../09-security-devsecops/README.md)
- [Docker](../12-docker/README.md)
- [DevOps & IaC](../13-devops-iac/README.md)
- [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)
- [Azure](../14-cloud/README.md)

## Verification metadata

- Verified: 2026-08-28.
- roadmap.sh DevOps/Kubernetes pages checked against current 2026 scope.
- Kubernetes/CNCF official docs remain canonical.
- Vietnamese sources are supplementary.
