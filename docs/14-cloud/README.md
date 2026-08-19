# Module 14 — Cloud & Microsoft Azure

> [← Module 13 — DevOps & IaC](../13-devops-iac/README.md) · [Roadmap](../00-roadmap/README.md) · [System Design →](../24-system-design/README.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;Azure Architecture</span>
  <span><strong>Audience</strong>&nbsp;.NET Backend → Architect</span>
  <span><strong>Mode</strong>&nbsp;example-first</span>
</div>

Cloud không phải là “đổi server vật lý thành VM trên Azure”. Ở mức architect, ta phải hiểu **resource hierarchy, identity boundary, network boundary, managed services, failure domain, operating model và cost model**.

Mental model của module:

```text
Business workload
      ↓
Cloud constraints + NFR
      ↓
Governance boundary
Tenant / Management Group / Subscription / Resource Group
      ↓
Identity + Network boundary
      ↓
Compute + Data + Messaging choices
      ↓
Availability Zone / Region / DR model
      ↓
Observability + Security + Cost controls
      ↓
Evidence: load / failure / restore / cost / runbook
```

<div class="key-takeaway" markdown>
<strong>Không bắt đầu bằng service name.</strong>

Bắt đầu bằng workload: request path nào critical, latency/SLO bao nhiêu, dữ liệu nào cần strong consistency, blast radius chấp nhận được tới đâu, RTO/RPO là gì, team có khả năng vận hành Kubernetes hay không. Sau đó mới chọn App Service, Container Apps, AKS, Azure SQL, Cosmos DB, Service Bus…
</div>

## Learning path

| Guide | Học gì | Câu hỏi phải trả lời được |
|---|---|---|
| [Cloud Primitives, Identity & Networking](cloud-primitives-identity-and-networking.md) | cloud primitives generic | compute/network/storage/identity thực sự là gì? |
| [Azure Foundations, Resource Hierarchy & Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md) | tenant → management groups → subscription → policy | workload nên nằm ở boundary nào và ai được quản trị? |
| [Azure Identity, Networking & Zero Trust](azure-identity-networking-and-zero-trust.md) | Entra, RBAC, Managed Identity, VNet, Private Link, edge/gateway | làm sao request đi từ Internet tới app mà không biến network thành “trusted zone”? |
| [Azure Compute, Data, Messaging & Integration](azure-compute-data-messaging-and-integration.md) | App Service, Functions, Container Apps, AKS, SQL, Cosmos, Blob, Service Bus… | chọn managed service theo workload thay vì theo trend như thế nào? |
| [Azure Reliability, Observability, Governance & Cost](azure-reliability-observability-governance-and-cost.md) | zone/region, DR, Monitor, Policy, Cost | system fail và recover ra sao? ai biết? ai trả tiền? |
| [Azure .NET Checkout Reference Architecture](azure-dotnet-reference-architecture.md) | ghép thành một hệ thống production | một backend .NET thực tế deploy lên Azure như thế nào mà vẫn giữ correctness? |
| [Regions, Availability & DR](regions-availability-and-disaster-recovery.md) | cloud reliability generic | zone/region/multi-region trade-off là gì? |
| [Cloud Cost Governance & Operations](cloud-cost-governance-and-operations.md) | FinOps + operations generic | cost và operations trở thành design input thế nào? |
| [References](references.md) | Microsoft Learn + architecture references | tra source of truth ở đâu? |

## Azure service map cho backend architect

Không cần thuộc hàng trăm service. Hãy map theo **architectural responsibility**:

| Responsibility | Azure services thường gặp | Câu hỏi kiến trúc |
|---|---|---|
| Identity | Microsoft Entra ID, Managed Identity | ai/cái gì đang gọi và quyền tối thiểu là gì? |
| Secrets / keys | Key Vault | secret có còn nằm trong config/appsettings không? |
| Global edge | Front Door | routing toàn cầu, WAF, failover cần ở đâu? |
| Regional ingress | Application Gateway | L7 routing/WAF trong region cần không? |
| API management | API Management | policy, auth, quota, versioning, developer boundary ở đâu? |
| Network | VNet, subnet, NSG, Private Endpoint, Azure Firewall | traffic nào được public, traffic nào chỉ private? |
| Web/API compute | App Service | cần PaaS đơn giản cho HTTP workload không? |
| Event compute | Functions | workload có event-driven/bursty và execution bounded không? |
| Container PaaS | Container Apps | cần container + autoscale/revision nhưng chưa cần Kubernetes control plane không? |
| Kubernetes | AKS | có requirement thật sự cho Kubernetes API/ecosystem/control không? |
| Relational data | Azure SQL | transaction/query relational có phải source of truth không? |
| Globally distributed NoSQL | Cosmos DB | partition key, latency, distribution model có justify không? |
| Object storage | Blob Storage | binary/object/large immutable payload nằm đâu? |
| Cache | Azure Managed Redis | latency/load/staleness budget là gì? |
| Enterprise messaging | Service Bus | command/work queue/topic với delivery semantics nào? |
| Event routing | Event Grid | event notification/routing nhẹ cần push tới subscriber nào? |
| Streaming | Event Hubs | ingest event/telemetry throughput lớn cần log/stream semantics không? |
| Observability | Azure Monitor, Application Insights, Log Analytics | SLI/SLO và trace đi xuyên service thế nào? |
| Governance | Management Groups, Azure Policy, RBAC, tags | guardrail nào được enforce tự động? |
| Cost | Cost Management, budgets | unit economics và cost guardrail là gì? |

## Quy tắc chọn service

### 1. Managed trước, control sau

```text
Need HTTP API
   ↓
App Service đủ không?
   ↓ no
Container Apps đủ không?
   ↓ no
AKS có requirement justify không?
   ↓ no
đừng dùng AKS chỉ vì “production = Kubernetes”
```

Managed service giảm operating surface, nhưng đổi lại bạn chấp nhận provider constraints, quotas, pricing model và portability trade-off.

### 2. Private networking không thay authorization

```text
Private Endpoint
!=
caller được phép đọc dữ liệu
```

Network isolation giảm exposure. Authorization vẫn phải dựa trên identity + policy phù hợp.

### 3. Multi-region không phải default

```text
Multi-region
= cost x nhiều lớp
+ data consistency complexity
+ deployment complexity
+ failover complexity
+ testing obligation
```

Chỉ dùng khi business SLO/RTO/RPO justify.

### 4. Cloud reliability vẫn bắt đầu từ application correctness

Azure có thể restart instance, scale replica hoặc fail zone. Nhưng platform không tự giải quyết:

```text
duplicate message
unknown payment outcome
non-idempotent retry
bad partition key
stale cache
logical data corruption
operator deploy sai schema
```

## Example xuyên module — Checkout trên Azure

```text
Internet
   ↓
Azure Front Door + WAF
   ↓
API Management
   ↓
.NET Checkout API
(App Service hoặc Container Apps)
   ↓
Azure SQL ← source of truth cho Order
   ↓
Outbox
   ↓
Service Bus
   ↓
Payment Worker / Notification Worker

Redis ← cache/projection, không phải source of truth
Blob  ← receipt / large object

Managed Identity
   ↓
Key Vault / SQL / Service Bus / Storage

Azure Monitor + Application Insights
   ↓
trace / metrics / logs / alert / SLO
```

### Failure quan trọng

Customer bấm Checkout. Payment provider timeout.

Sai mental model:

```text
timeout = payment failed
```

Đúng hơn:

```text
timeout = local system chưa biết remote side effect đã xảy ra hay chưa
        = UNKNOWN
```

Azure Service Bus, Container Apps hay AKS không thay đổi bài toán correctness này. Ta vẫn cần **business identity, idempotency, durable state và reconciliation**.

## Azure review checklist

Trước khi approve một workload Azure, hỏi:

1. Subscription/resource-group boundary phản ánh ownership và blast radius chưa?
2. Managed Identity có thay được secret tĩnh không?
3. Public endpoint nào thực sự cần public?
4. Private DNS/Private Endpoint có recovery/runbook chưa?
5. Compute choice dựa trên workload hay dựa trên team preference?
6. Database là source of truth nào, consistency nào?
7. Queue/topic/stream được chọn dựa trên semantics nào?
8. Retry có idempotency và bounded backoff không?
9. Zone outage xử lý thế nào?
10. Region outage có nằm trong business requirement không?
11. RTO/RPO đã được test bằng restore/failover drill chưa?
12. SLO dashboard đo user outcome hay chỉ CPU/memory?
13. Managed services có quota/throttling/failure mode nào?
14. Policy/RBAC/tags/budget có guardrail tự động chưa?
15. Cost theo request/tenant/order là bao nhiêu?
16. Có exit/migration trigger nếu service choice không còn phù hợp không?

## Nguồn tham khảo

Module này ưu tiên **Microsoft Learn / Azure Architecture Center / Azure Well-Architected Framework / Cloud Adoption Framework** làm source of truth cho Azure hiện tại. Repo `awesome-software-architecture` được dùng để kiểm tra breadth của architecture/cloud patterns, không thay official service documentation.

Xem [references.md](references.md).
