# DevOps, Terraform & Kubernetes — từ Commit tới Recovery

<div class="lesson-meta"><span>CI/CD</span><span>Docker / Linux</span><span>Terraform</span><span>AKS / Kubernetes</span></div>

[Bắt đầu với câu hỏi DevOps, Azure →](../05-interview-preparation/distributed-cloud.md){ .hero-button .hero-button-primary }

## Production delivery mental model

```text
Change / Pull request
    ↓ quality gates (tests, static analysis, security)
Build immutable image/artifact → Registry
    ↓
IaC plan/review + deployment
    ↓ readiness & business smoke tests
Promote traffic
    ↓ metrics/traces, error budget
Rollback or roll-forward + postmortem
```

Một pipeline GitLab CI hoặc Azure DevOps chạy xanh **không có nghĩa** revision mới phục vụ người dùng thành công.

## Chọn bài theo vấn đề

| Gặp vấn đề | Đọc sâu | Phỏng vấn cần chứng minh |
| --- | --- | --- |
| Build image quá lớn/lộ secret | [Docker](../../12-docker/README.md) | Multi-stage, immutable digest, nonroot |
| CI green nhưng API lỗi | [CI/CD](../../13-devops-iac/ci-cd-artifacts-and-promotion.md) | Readiness, rollout, rollback |
| Terraform muốn replace resource chứa data | [State/Drift](../../13-devops-iac/terraform-state-modules-and-drift.md) | Không apply mù, protect/recovery |
| Pods Running nhưng 503 | [Kubernetes debugging](../../15-kubernetes/kubectl-debugging-and-ckad-practice.md) | Selector, EndpointSlices, readiness |
| AKS tăng replicas không giảm latency | [AKS architecture](../../15-kubernetes/aks-on-azure-production-architecture.md) | Bottleneck, HPA, node capacity |
| Alert 5xx tăng sau deploy | [Production troubleshooting](../../02-linux-git-networking/production-troubleshooting-foundations.md) | Incident triage, mitigation |

## 3 câu phỏng vấn thực tế

1. **Revision NotReady:** lệnh deploy trả exit 0, điều kiện CI success nào chưa đủ?
2. **Service 503:** vì sao Running Pods không đồng nghĩa Ready Endpoints?
3. **Terraform:** plan replace SQL Storage; người chịu trách nhiệm làm gì trước khi apply?

[Đáp án và follow-up](../05-interview-preparation/distributed-cloud.md) · [Mock Interview](../05-interview-preparation/mock-interview.md).
