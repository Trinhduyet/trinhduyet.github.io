# Multi-cloud & Hybrid Architecture — Portability, Governance and Platform Decisions

> [← Azure & Cloud](README.md) · [Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md) · [Reliability & DR](regions-availability-and-disaster-recovery.md) · [Solution Architecture](../25-software-architecture/integration-security-platform-architecture.md)

**Priority P2 (bổ sung kiến thức tổng quát):** hiểu cách thiết kế workload trên Azure/AWS/GCP/on-prem và đánh giá vendor trade-offs. *Multi-cloud không đồng nghĩa resilience tốt hơn* nếu team không vận hành được complexity.

## 1. Clarify trước khi chọn platform

| Requirement | Câu hỏi kiến trúc |
|---|---|
| Data residency | Data và backups được phép lưu/replicate tại regions nào? |
| Business continuity | Outage một AZ/region/cloud provider có cần sống sót không? RTO/RPO? |
| Existing estate | Identity, network, licenses, skills, vendor contracts hiện có? |
| Platform dependencies | Managed databases, messaging, IAM, APIs có semantics khác nhau? |
| Cost | Egress, replication, storage, operations, support và dual-run? |
| Security | Zero Trust, key ownership, compliance, audit và attack surface? |

**Có bốn hướng khác nhau:** single cloud, cloud + on-prem hybrid, multi-region một cloud, multi-cloud nhiều providers. Chọn đúng problem. Multi-region trong một cloud thường ít complexity hơn active-active xuyên cloud.

## 2. So sánh platform building blocks (minh họa, không phải equivalence đầy đủ)

| Capability | Azure | AWS | Google Cloud |
|---|---|---|---|
| Virtual compute | Virtual Machines | EC2 | Compute Engine |
| Managed Kubernetes | AKS | EKS | GKE |
| Object storage | Blob Storage | S3 | Cloud Storage |
| API front door | API Management / Front Door | API Gateway / CloudFront | API Gateway / Cloud CDN |
| Identity workload | Managed Identity / Entra ID | IAM roles | IAM / Workload Identity |
| Observability | Azure Monitor | CloudWatch | Cloud Monitoring |
| IaC option | Bicep / Terraform | CloudFormation / Terraform | Terraform / infrastructure tooling |

**Chú ý:** những tên dịch vụ trong một dòng không tương đương 1:1 về quotas, network trust, authentication, semantics, SLA hay pricing. Không di chuyển Azure Service Bus sang SQS/Pub/Sub mà mặc định giữ nguyên sessions/ordering/retry.

## 3. Portable design vs portable operations

| Layer | Portability thường có thể đạt | Chi phí/lock-in còn lại |
|---|---|---|
| Application | Stateless containers, HTTP/OpenAPI, runtime chuẩn | Framework SDK, background execution và auth integrations |
| Kubernetes | Deployments/Services, Helm/Kustomize/GitOps | Cloud LB/CNI/CSI, identity, autoscaling, policy |
| Data | SQL schemas, open protocols, export/replay | Replication, isolation, extensions, PITR, migration downtime |
| Messaging | Domain event contracts/Outbox | Broker delivery, ordering, transactions, quotas và DLQ differences |
| Security | Common threat model, secrets policy | Cloud IAM models, key management, network topology |
| Operations | SLO/SLI, traces/logs, runbooks | Different cloud APIs/toolchains, incident expertise |

**Architectural boundary:** adapter giúp giảm coupling ở các tích hợp *dễ thay đổi*, nhưng abstract tất cả cloud features có thể làm mất các lợi ích của managed service. Document lock-in có ý thức như một ADR.

## 4. Hybrid network & data architecture

```text
On-prem applications/databases
   ↔ private connectivity (VPN / dedicated link, subject to design)
Cloud network / subnet / identity boundary
   → workload API / workers
   → managed DB, storage, messaging
```

**Checklist bắt buộc:** overlapping CIDR, DNS resolution, TLS/mTLS, ingress/egress filtering, NAT/SNAT exhaustion, private endpoints, secret delivery, time synchronization, provider API rate limits, circuit breakers và observed network latency.

**Data gravity:** nơi dữ liệu nằm quyết định transfer/latency/cost. Cross-region/cloud distributed writes tạo conflict/consistency issues; không gọi nó là “strongly consistent” nếu thiếu proof và provider-specific constraints.

## 5. HA/DR decisions, tránh ngộ nhận active-active

| Pattern | Fits | Verification |
|---|---|---|
| Single region, multi-zone | Availability local, chi phí hợp lý | Zone failure, DB failover, queue drain |
| Warm standby region | Lower RTO/RPO theo backup/replication design | Region switch, replay, data integrity, rollback |
| Active-active multi-region | Truly distributed global workload, strict business case | Conflict strategy, global identity/route, failure partitions |
| Multi-cloud standby | Provider concentration risk thật, đủ team ops | Real failover drill, data restore, IAM/network/cost |

RTO/RPO là mục tiêu cần kiểm thử, không phải mặc định của một kiến trúc diagram. Đặc biệt external PSP/bank callbacks có thể tiếp tục sau region failover; idempotency và reconciliation phải survive.

## 6. Platform ADR & cost/security governance

```text
ADR: Run payment API on AKS in one region with zone redundancy.
Alternatives: App Service, multi-region AKS, multi-cloud Kubernetes.
Reason: workload/skills, database ownership, SLO and cost at current scale.
Trade-off: regional outage requires standby/restore strategy.
Evidence: load, zone failover, DB restore, identity/secret negative tests.
Revisit: measured SLO breach, regulatory residency, global active users.
```

**FinOps view:** compute reserved/on-demand, object storage and retention, NAT/egress, replication traffic, logs/traces, CI runners, operational toil. **Security view:** tenant boundaries, workload identity, least privilege, KMS/HSM, audit, recovery credentials, incident response.

## 7. Lab / decision exercises

1. Deploy cùng một container API trên một platform managed và trên Kubernetes local; so sánh operational burden.
2. Document C4/deployment view on-prem ↔ cloud; simulate network timeout/DNS issue và request retry storm.
3. ADR: single-region HA vs warm-standby vs multi-cloud, có capacity/cost/RTO/RPO.
4. Test restore, late callbacks và reconciliation; ghi thời gian thực tế, không tự nhận RPO=0.
5. Mapping 3 native services giữa hai clouds; chỉ rõ **ít nhất 5 semantics không tương đương**.

**Exit criteria:** chọn cloud theo measurable requirements, operational capabilities, costs và evidence; không chọn multi-cloud chỉ vì muốn “portable”.

**Nguồn:** [Microsoft Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/) · [AWS Well-Architected](https://docs.aws.amazon.com/wellarchitected/latest/framework/welcome.html) · [Google Cloud Architecture Framework](https://cloud.google.com/architecture/framework) · [Awesome — Cloud Platforms](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/cloud-platforms).
