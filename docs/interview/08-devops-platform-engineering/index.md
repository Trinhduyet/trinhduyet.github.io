# DevOps, Terraform & Kubernetes — phỏng vấn theo production lifecycle

## Từ Git commit đến live service

```text
Commit → tests + scan → immutable artifact → staging
→ approved deploy → revision Ready → business smoke test
→ traffic switch → telemetry → rollback gate
```

**CI green ≠ application ready**.

## Các chủ đề

- Docker: multi-stage builds, image layers, resource limits, nonroot runtime.
- GitLab CI/Azure DevOps: promotion, environments, secrets, approvals, artifact integrity.
- Terraform: state/locking, plan, drift, module, provider upgrade, state recovery.
- Kubernetes: Control Plane, Pods, Deployments, ReplicaSets, Services, probes, RBAC, HPA, PVC.
- AKS: managed control plane, node pools, workload identity, network policies, operational ownership.
- SRE: SLI/SLO, tracing, logs, metrics, incident runbook, postmortem.

## Interview case

Pods `Running`, Service trả 503. Kiểm tra selector, endpoints/EndpointSlices, readiness, ingress/service target, container listen address trước khi scale bừa. Nếu p95 tăng vì SQL lock, HPA tăng Pods có thể khiến tình hình tệ hơn.

[DevOps/IaC](../../13-devops-iac/README.md) · [Kubernetes](../../15-kubernetes/README.md) · [AKS](../../15-kubernetes/aks-on-azure-production-architecture.md).
