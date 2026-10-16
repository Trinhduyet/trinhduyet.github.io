# Microsoft Azure — từ API deployment đến platform decisions

<div class="lesson-meta"><span>Azure Compute</span><span>Identity / Network</span><span>Data / Messaging</span><span>Reliability / Cost</span></div>

[Bắt đầu với 9 câu Azure / Distributed →](../05-interview-preparation/distributed-cloud.md){ .hero-button .hero-button-primary }

## Trả lời câu hỏi “chọn service nào?” bằng requirements

| Workload | Lựa chọn cần cân nhắc | Không được bỏ qua |
| --- | --- | --- |
| ASP.NET Core HTTP API | App Service, Container Apps, AKS | Ingress, auth, revisions, probes, cost |
| Event-driven worker | Functions, Container Apps jobs/worker | Concurrency, retries, idempotency |
| Durable business messaging | Azure Service Bus | Settlement/DLQ, lock, quotas |
| High-volume stream | Event Hubs | Partition, consumer groups, lag, retention |
| File/object data | Blob Storage | RBAC, SAS, retention, private endpoint |
| API observability | Application Insights / Azure Monitor | Traces, dependency errors, ingestion cost |

## Đọc sâu từ bộ tài liệu Azure hiện có

<div class="course-grid" markdown>

[**Service Selection** — lựa chọn compute, data, messaging theo workloads.](../../14-cloud/azure-service-selection-and-workload-architecture.md){ .course-card }

[**Security & Identity** — Entra ID, Managed Identity, zero trust, networking.](../../14-cloud/azure-identity-networking-and-zero-trust.md){ .course-card }

[**Operations, DR & Cost** — availability zones, recovery, cloud budget.](../../14-cloud/azure-reliability-observability-governance-and-cost.md){ .course-card }

[**Reference Architecture** — triển khai hệ thống .NET trên Azure với ranh giới rõ.](../../14-cloud/azure-dotnet-reference-architecture.md){ .course-card }

</div>

## Failure drill: revision NotReady

Deploy CLI báo thành công nhưng API không Ready. Kiểm tra image, startup exception, readiness, ingress, dependencies và traffic. CI chỉ xanh nếu **revision thực sự healthy + smoke test**; có kế hoạch rollback khi DB migration không tương thích.

[Đáp án DIST-07](../05-interview-preparation/distributed-cloud.md#dist-07-azure-container-apps-deploy-thanh-cong-nhung-revision-notready) · [DevOps track](../08-devops-platform-engineering/index.md).

## 4 góc nhìn phỏng vấn

- **Foundation:** Service Bus và Event Hubs khác nhau ở đâu?
- **Applied:** Cấu hình Managed Identity/RBAC như thế nào?
- **Advanced:** Xử lý duplicate, retries và monitoring workload event-driven?
- **Architecture:** Compute selection, SLO, cost, availability zones, DR, team operations.

**Sang bài khác:** [DevOps →](../08-devops-platform-engineering/index.md) · [Architecture →](../09-solution-architecture-leadership/index.md).
