# Module 14 — Azure & Cloud Platform Architecture

> [← Module 13 — DevOps & IaC](../13-devops-iac/README.md) · [Roadmap](../00-roadmap/README.md) · [Kubernetes →](../15-kubernetes/README.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;Azure Architecture · Platform · AKS</span>
  <span><strong>Audience</strong>&nbsp;.NET Backend → Senior/Architect</span>
  <span><strong>Mode</strong>&nbsp;architecture-first · project-first</span>
</div>

Module này không nhằm giúp bạn “nhớ Azure có những service gì”. Mục tiêu là để bạn có thể nhìn một backend thực tế và trả lời được:

1. **Workload nên được đặt ở đâu trong Azure resource hierarchy?**
2. **Request đi từ Internet tới application và data qua những boundary nào?**
3. **Khi nào chọn App Service, Functions, Container Apps hoặc AKS?**
4. **Làm sao deploy, observe, recover, govern và kiểm soát cost như một production system?**

<div class="key-takeaway" markdown>
<strong>Cloud architecture không bắt đầu bằng service name.</strong>

Bắt đầu bằng workload + NFR + failure model + team capability. Sau đó mới map sang Azure services.
</div>

---

# 1. Bức tranh tổng thể — Azure platform gồm những lớp nào?

Một workload production không chỉ là `API + database`.

```text
┌──────────────────────────────────────────────────────────────┐
│ ORGANIZATION / GOVERNANCE PLANE                              │
│ Tenant → Management Groups → Subscriptions → Resource Groups │
│ Policy · RBAC · Budget · Tags · Compliance · Ownership       │
└──────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────┐
│ PLATFORM / CONNECTIVITY PLANE                                │
│ VNet · DNS · Firewall · Private Link · Shared ACR · Monitor  │
│ Key Vault · central logging · shared ingress/egress controls │
└──────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────┐
│ WORKLOAD PLANE                                               │
│ Edge → API ingress → Compute → Data → Messaging → Storage    │
│ App Service / Container Apps / AKS / Functions               │
└──────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────┐
│ DELIVERY + OPERATIONS PLANE                                  │
│ Git → CI → Artifact → IaC → Deploy → Observe → Rollback      │
│ SLO · alerts · runbooks · backup · restore · DR · FinOps     │
└──────────────────────────────────────────────────────────────┘
```

Nếu chỉ học từng Azure service độc lập, bạn sẽ biết “service làm gì” nhưng vẫn không biết **ghép chúng thành một system**.

---

# 2. Reference architecture — một backend .NET thực tế trên Azure

Ví dụ một hệ thống commerce/checkout:

```text
Users / Mobile / Web
        │
        ▼
Azure Front Door + WAF
        │
        ▼
API Management
        │
        ▼
┌─────────────────────────────────────┐
│ Application compute                 │
│                                     │
│ Option A: App Service               │
│ Option B: Container Apps            │
│ Option C: AKS                       │
└─────────────────────────────────────┘
        │             │
        │             ├──────────────► Azure Service Bus
        │             │                  │
        │             │                  ▼
        │             │              Workers / Jobs
        │             │
        ├─────────────► Azure SQL
        ├─────────────► Azure Managed Redis
        └─────────────► Blob Storage

Application identity
        │
        ▼
Managed Identity / Entra Workload Identity
        │
        ├─────────────► Key Vault
        ├─────────────► SQL
        ├─────────────► Service Bus
        └─────────────► Storage

Telemetry
        │
        ▼
Azure Monitor / Application Insights / Log Analytics
```

Điểm quan trọng là **compute chỉ là một phần của architecture**. Chuyển App Service thành AKS không tự giải quyết:

```text
data consistency
idempotency
message duplication
payment timeout
schema migration
secret rotation
network authorization
backup / restore
SLO / alerting
cost governance
```

---

# 3. Chọn compute: App Service, Functions, Container Apps hay AKS?

Đây là decision quan trọng nhất vì nó thay đổi mức **control vs operating burden**.

| Platform | Nên bắt đầu cân nhắc khi | Bạn phải vận hành nhiều hơn ở đâu? |
|---|---|---|
| **App Service** | HTTP API/web app tương đối thẳng, team muốn PaaS đơn giản | app/runtime/config/scaling policy |
| **Functions** | event/trigger/job, traffic bursty hoặc execution ngắn | retry, concurrency, poison message, idempotency |
| **Container Apps** | cần container, revisions, autoscale, microservice primitives nhưng không cần Kubernetes API | app-level container lifecycle và environment design |
| **AKS** | Kubernetes API/ecosystem/control là requirement thật | cluster + node pools + networking + policy + upgrades + workload ops |

Mental model:

```text
Need a web/API workload
        │
        ├─ App Service đáp ứng NFR? ───────────────► dùng App Service
        │
        ▼ no
Need container-native lifecycle / per-app scale?
        │
        ├─ Container Apps đáp ứng? ────────────────► dùng Container Apps
        │
        ▼ no
Need direct Kubernetes API / operators / scheduling /
cluster networking policy / platform standardization?
        │
        ├─ yes ───────────────────────────────────► cân nhắc AKS
        │
        └─ no ────────────────────────────────────► đừng dùng AKS vì trend
```

Microsoft hiện phân biệt **AKS Automatic** và **AKS Standard**. Automatic phù hợp với nhiều workload phổ biến khi muốn opinionated defaults và giảm số quyết định vận hành; Standard phù hợp khi bạn cần mức custom/control cao hơn. Dù dùng mode nào, **Kubernetes vẫn có operating model riêng** và team phải hiểu workload behavior.

Xem deep dive: [AKS Production Architecture](azure-kubernetes-service-aks-production-architecture.md).

---

# 4. Hai kiến trúc thực tế: PaaS-first và Kubernetes platform

## 4.1 PaaS-first — lựa chọn tốt cho phần lớn team nhỏ/vừa

```text
Internet
  ↓
Front Door + WAF
  ↓
API Management
  ↓
Container Apps / App Service
  ├─ Checkout API
  ├─ Payment Worker
  └─ Notification Worker
  ↓
Azure SQL + Service Bus + Blob + Redis
```

Phù hợp khi:

- team muốn tập trung vào product/backend hơn platform engineering;
- không cần custom Kubernetes operators;
- không cần complex node scheduling;
- không cần cluster-level multi-tenant controls;
- số service vừa phải;
- deploy/revision/autoscale của PaaS đã đáp ứng requirement.

**Lợi ích:** ít control plane phải sở hữu, ít upgrade surface, incident scope nhỏ hơn.

## 4.2 AKS platform — khi control thực sự có giá trị

```text
Internet
  ↓
Front Door + WAF
  ↓
Regional ingress / gateway
  ↓
AKS
  ├─ system node pool
  ├─ application node pool(s)
  ├─ namespace: checkout
  │    ├─ Deployment checkout-api
  │    ├─ Deployment payment-worker
  │    └─ Service checkout-api
  ├─ HPA / KEDA
  ├─ NetworkPolicy
  └─ Workload Identity
       │
       ├─ Key Vault
       ├─ SQL
       ├─ Service Bus
       └─ Storage
```

AKS hợp lý hơn khi:

- organization đã có platform/SRE capability;
- cần Kubernetes API và ecosystem;
- cần namespace/policy/scheduling isolation;
- có nhiều workload cần một platform chuẩn hóa;
- cần custom controllers/operators;
- cần node pools/GPU/specialized compute;
- cần network policy/service mesh/platform-level control mà PaaS không đáp ứng.

**Không dùng AKS chỉ vì “microservices = Kubernetes”.**

---

# 5. Azure resource hierarchy — application nằm ở đâu?

Azure có nhiều boundary khác nhau và mỗi boundary giải quyết một problem khác nhau.

```text
Microsoft Entra tenant
        ↓
Management Groups
        ↓
Subscriptions
        ↓
Resource Groups
        ↓
Resources
```

Một enterprise setup có thể trông như:

```text
Tenant
├─ Platform
│  ├─ Connectivity subscription
│  ├─ Identity/shared services subscription
│  └─ Management/monitoring subscription
│
├─ Landing Zones
│  ├─ Production subscription
│  │    ├─ rg-checkout-prod-compute
│  │    ├─ rg-checkout-prod-data
│  │    └─ rg-checkout-prod-observability
│  │
│  └─ NonProduction subscription
│       └─ dev / test workloads
│
└─ Sandbox
```

Không có một hierarchy universal. Design phải phản ánh:

```text
ownership
billing
policy boundary
blast radius
environment isolation
compliance
quota
lifecycle
```

Xem [Azure Foundations, Resource Hierarchy & Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md).

---

# 6. Identity + Network — đừng biến VNet thành “trusted zone”

Một request production nên được reason theo path:

```text
Internet
  ↓ HTTPS
Edge / WAF
  ↓
API ingress
  ↓ authenticated caller
Application
  ↓ workload identity
Private service endpoint
  ↓ authorization
SQL / Service Bus / Storage / Key Vault
```

Hai control khác nhau:

```text
Network isolation
!=
Authorization
```

Private Endpoint giúp giảm exposure nhưng không thay RBAC, database permission hay application authorization.

Production checklist:

- public endpoint nào thực sự cần public?
- origin có chỉ nhận traffic từ ingress expected không?
- app dùng Managed Identity/Workload Identity thay secret tĩnh được không?
- SQL/Service Bus/Storage có private path khi requirement cần không?
- outbound traffic có kiểm soát không?
- Private DNS có owner/runbook rõ không?

Xem [Azure Identity, Networking & Zero Trust](azure-identity-networking-and-zero-trust.md).

---

# 7. Data + Messaging — chọn theo semantics

## Data

Trước khi chọn Azure SQL/Cosmos/Blob/Redis, viết:

```text
source of truth là gì?
transaction boundary ở đâu?
consistency requirement là gì?
query/access pattern nào quan trọng?
partition key nếu distributed data?
retention + backup + restore ra sao?
```

## Messaging

```text
Business command / work queue
→ Service Bus

Event notification / routing
→ Event Grid

High-throughput telemetry / stream ingestion
→ Event Hubs
```

Đây là starting model, không phải mapping cứng.

Xem [Azure Compute, Data, Messaging & Integration](azure-compute-data-messaging-and-integration.md).

---

# 8. Cách áp dụng vào một dự án thật — từ requirements tới production

Đừng deploy Azure trước rồi mới “thêm architecture”. Đi theo sequence này.

## Step 0 — Viết workload brief

Ví dụ:

```text
System               Checkout
Peak traffic         2,000 checkout/min
Availability         99.95%
P95 intake latency   < 500 ms
RTO                  30 min
RPO                  5 min
Sensitive data       payment/customer identifiers
Team                 5 backend + 1 DevOps
```

## Step 1 — Chốt architecture boundary

```text
Prod subscription
Non-prod subscription
Resource groups theo ownership/lifecycle
Policy + RBAC
Cost budget
```

Output: **resource organization ADR**.

## Step 2 — Thiết kế request path + network

```text
Client
→ Front Door/WAF
→ APIM
→ Compute
→ private data dependencies
```

Output: **network/data-flow diagram + threat boundary**.

## Step 3 — Chọn compute

Hỏi:

```text
App Service đủ không?
Container Apps đủ không?
AKS requirement cụ thể là gì?
Team có day-2 operational capability không?
```

Output: **compute ADR**.

## Step 4 — Chọn source of truth + async boundaries

Ví dụ:

```text
Azure SQL
  ├─ Orders
  ├─ PaymentAttempts
  └─ OutboxMessages
       ↓
Service Bus
       ↓
Payment Worker
```

Output: **data + messaging ADR**, schema constraints, idempotency strategy.

## Step 5 — Identity + secrets

```text
Application identity
→ SQL permission
→ Service Bus sender/receiver permission
→ Storage permission
→ Key Vault only where actual secret is required
```

Output: **permission matrix**.

## Step 6 — IaC + CI/CD

```text
Git
→ test
→ build
→ immutable artifact/container image
→ security checks
→ IaC plan
→ deploy non-prod
→ smoke/integration
→ progressive production rollout
→ SLO gate
→ rollback
```

Output: **repeatable environment**, không phải click Azure Portal bằng tay.

## Step 7 — Observability

Theo dõi cả system metrics và business outcome:

```text
HTTP P95/P99
error rate
SQL latency
queue depth
oldest message age
DLQ count
payment_unknown_total
checkout_completion_latency
```

Output: dashboard + alert + correlation IDs + runbook.

## Step 8 — Failure drills

Test thực tế:

```text
kill instance/pod
break readiness
DB unavailable
Service Bus delayed
provider timeout
secret rotation
bad deployment
restore database
zone/region failure scenario nếu NFR yêu cầu
```

Output: **evidence**, không chỉ diagram.

## Step 9 — Cost + governance

Theo dõi:

```text
cost / environment
cost / workload
cost / request/order nếu có thể
unused capacity
budget alert
expensive log ingestion
AKS idle node cost nếu dùng cluster
```

Output: budget + ownership + optimization backlog.

---

# 9. Checkout example — correctness không biến mất khi lên Azure

Flow:

```text
Client
  ↓
Checkout API
  ↓ local transaction
Azure SQL
  ├─ Order
  ├─ PaymentAttempt
  └─ Outbox
       ↓
Service Bus
       ↓
Payment Worker
       ↓
External Payment Provider
```

Nếu provider timeout:

```text
timeout
!= payment failed

payment may be UNKNOWN
```

Safer flow:

```text
same business/idempotency key
→ query provider / verified webhook
→ reconcile
→ SUCCEEDED or FAILED
```

App Service, Container Apps hay AKS đều **không tự giải quyết business correctness** này.

Xem full case study: [Azure .NET Checkout Reference Architecture](azure-dotnet-reference-architecture.md).

---

# 10. Kubernetes nằm ở đâu trong learning path?

Kubernetes có hai lớp kiến thức khác nhau:

```text
Layer 1 — Kubernetes generic
Deployment · ReplicaSet · Pod · Service · Ingress/Gateway
ConfigMap · Secret · PVC · RBAC · NetworkPolicy
scheduler · controller · reconciliation · probes · resources

Layer 2 — Azure implementation with AKS
AKS control plane · node pools · ACR · Entra Workload Identity
Azure networking · ingress/egress · Key Vault · Monitor
Azure Policy · cluster upgrades · DR · cost
```

Học theo thứ tự:

1. [Module 15 — Kubernetes](../15-kubernetes/README.md) để hiểu object model và reconciliation.
2. [AKS Production Architecture](azure-kubernetes-service-aks-production-architecture.md) để map Kubernetes vào Azure production platform.

Đừng học AKS bằng cách thuộc `az aks create` trước khi hiểu Pod/Service/Deployment/requests/limits/probes.

---

# 11. Learning path của Module 14

| Guide | Học gì | Sau khi đọc phải trả lời được |
|---|---|---|
| [Cloud Primitives, Identity & Networking](cloud-primitives-identity-and-networking.md) | cloud primitives generic | compute/network/storage/identity thực sự là gì? |
| [Azure Foundations, Resource Hierarchy & Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md) | tenant → management groups → subscription → policy | workload nên nằm ở boundary nào và ai được quản trị? |
| [Azure Identity, Networking & Zero Trust](azure-identity-networking-and-zero-trust.md) | Entra, RBAC, Managed Identity, VNet, Private Link, edge/gateway | request đi từ Internet tới app/data an toàn thế nào? |
| [Azure Compute, Data, Messaging & Integration](azure-compute-data-messaging-and-integration.md) | App Service, Functions, Container Apps, AKS, SQL, Redis, Service Bus… | chọn managed service theo workload thế nào? |
| [AKS Production Architecture](azure-kubernetes-service-aks-production-architecture.md) | Kubernetes trên Azure ở production | khi nào AKS đáng dùng và cluster/workload được thiết kế ra sao? |
| [Azure Reliability, Observability, Governance & Cost](azure-reliability-observability-governance-and-cost.md) | zone/region, DR, Monitor, Policy, Cost | system fail/recover ra sao và ai biết? |
| [Azure .NET Checkout Reference Architecture](azure-dotnet-reference-architecture.md) | ghép thành một production system | backend .NET deploy lên Azure thế nào mà giữ correctness? |
| [Regions, Availability & DR](regions-availability-and-disaster-recovery.md) | cloud reliability generic | zone/region/multi-region trade-off là gì? |
| [Cloud Cost Governance & Operations](cloud-cost-governance-and-operations.md) | FinOps + operations | cost và operations trở thành design input thế nào? |
| [References](references.md) | Microsoft Learn + Architecture Center | tra source of truth ở đâu? |

---

# 12. Azure service map cho backend architect

Không cần thuộc hàng trăm service. Map theo responsibility:

| Responsibility | Azure services thường gặp | Architecture question |
|---|---|---|
| Identity | Microsoft Entra ID, Managed Identity, Workload Identity | ai/cái gì đang gọi và quyền tối thiểu là gì? |
| Secrets / keys | Key Vault | secret nào thực sự cần tồn tại? rotate thế nào? |
| Global edge | Front Door | routing/WAF/global failover cần ở đâu? |
| Regional ingress | Application Gateway / platform ingress | request vào workload region thế nào? |
| API management | API Management | auth, policy, quota, version boundary ở đâu? |
| Network | VNet, subnet, NSG, Private Endpoint, Firewall | traffic nào public/private/egress-controlled? |
| Web/API compute | App Service | PaaS HTTP workload có đủ không? |
| Event compute | Functions | workload có event-driven/bursty không? |
| Container PaaS | Container Apps | cần container/revision/autoscale nhưng không cần K8s API? |
| Kubernetes | AKS | requirement nào justify Kubernetes control? |
| Relational data | Azure SQL | transaction + relational source of truth? |
| Distributed NoSQL | Cosmos DB | partition/distribution model có justify không? |
| Object storage | Blob Storage | object/binary/large payload lifecycle? |
| Cache | Azure Managed Redis | staleness budget/fallback/hot-key? |
| Messaging | Service Bus | delivery semantics, queue/topic, DLQ? |
| Event routing | Event Grid | notification/routing event? |
| Streaming | Event Hubs | high-throughput log/stream ingestion? |
| Observability | Azure Monitor, Application Insights | SLI/SLO và trace xuyên service? |
| Governance | Management Groups, Policy, RBAC, tags | guardrail nào được enforce tự động? |
| Cost | Cost Management, budgets | unit economics + budget guardrail? |

---

# 13. Architecture review checklist

Trước khi approve một Azure workload, hỏi:

1. Workload owner, subscription và resource-group boundary rõ chưa?
2. SLO/RTO/RPO có business justification không?
3. Public endpoint nào thực sự cần public?
4. Identity nào gọi SQL/Service Bus/Storage/Key Vault?
5. Có static secret nào thay bằng Managed/Workload Identity được không?
6. Compute choice dựa trên workload hay team preference?
7. Nếu chọn AKS, Kubernetes-specific requirement là gì?
8. Team nào sở hữu cluster upgrades, node pools, policy và incident response?
9. Database source of truth và transaction boundary ở đâu?
10. Retry có idempotency + bounded backoff không?
11. Queue/topic/stream được chọn dựa trên semantics nào?
12. Zone outage xử lý thế nào?
13. Region outage có thực sự nằm trong requirement không?
14. Backup có restore test không?
15. SLO dashboard đo user outcome hay chỉ CPU/memory?
16. Deployment có progressive rollout/rollback strategy không?
17. Database migration có compatible với rolling versions không?
18. Policy/RBAC/tags/budget có guardrail tự động chưa?
19. Cost theo workload/unit business là bao nhiêu?
20. Có runbook + failure drill evidence chưa?

---

# 14. Exit criteria — khi nào coi là “biết Azure”?

Không phải khi bạn thi xong certificate hay deploy được một Web App.

Bạn hoàn thành module khi có thể:

- vẽ được Azure architecture end-to-end từ Internet tới data;
- giải thích resource hierarchy + ownership + blast radius;
- chọn App Service/Functions/Container Apps/AKS bằng trade-off;
- giải thích khi nào **không nên dùng Kubernetes**;
- thiết kế identity/network/private access mà không dựa vào “VNet = secure”;
- chọn data/messaging theo semantics;
- triển khai bằng IaC + repeatable pipeline;
- định nghĩa SLI/SLO + dashboard + alerts;
- test backup/restore/failure paths;
- reason về zone/region/DR;
- có ADR cho những quyết định khó đảo ngược;
- chứng minh architecture bằng code, deployment, metrics và failure drills.

<div class="key-takeaway" markdown>
<strong>Architect-level outcome</strong>

Bạn không chỉ biết “Azure service X dùng để làm gì”. Bạn biết **đưa một workload thật vào Azure, chọn đúng mức abstraction, bảo vệ boundary, vận hành failure và chứng minh decision bằng evidence**.
</div>

## Nguồn tham khảo

Module ưu tiên **Microsoft Learn, Azure Architecture Center, Azure Well-Architected Framework, Cloud Adoption Framework và Kubernetes official documentation** làm source of truth.

Xem [references.md](references.md).
