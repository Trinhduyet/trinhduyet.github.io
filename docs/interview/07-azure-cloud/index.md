# Azure Cloud — .NET workload design

## Chọn compute theo bài toán

| Dịch vụ | Hợp lý khi | Điểm đánh đổi |
| --- | --- | --- |
| App Service | HTTP API, ít platform operations | Ít orchestration control |
| Container Apps | Container services/worker, event-driven autoscale | Cần quản lý revisions, ingress, scale rules |
| Azure Functions | Trigger-based jobs | Cold start/runtime constraints tùy plan |
| AKS | Complex multi-workload orchestration | Network/RBAC/upgrades/operational cost |

**Không** tự chọn Kubernetes vì có nhiều API.

## Kiến thức interview

- Entra ID: authentication vs authorization; token issuer/audience, scopes/roles.
- Managed Identity: không cần hardcode client secret để truy cập Azure service; vẫn cần cấp RBAC.
- Blob Storage: private access, SAS expiry, lifecycle và versioning.
- Service Bus: queue/topics, DLQ, duplicates; Event Hubs: partitioned stream/consumer groups.
- App Insights: distributed trace, dependency latency, errors và correlation.
- Availability: zones, RTO/RPO, restore testing và cost.

## Case production

API Container Apps deploy thành công nhưng revision NotReady. Check startup/readiness, logs, ingress target, smoke tests; pipeline chỉ success khi application healthy. DB migrations phải backwards compatible để rollback an toàn.

[Azure in-depth](../../14-cloud/README.md) · [Azure identity](../../14-cloud/azure-identity-networking-and-zero-trust.md) · [Reference Architecture](../../14-cloud/azure-dotnet-reference-architecture.md).
