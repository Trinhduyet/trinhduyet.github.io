# Module 14 — Microsoft Azure & Cloud Platform

> [← Module 13 — DevOps & IaC](../13-devops-iac/README.md) · [Roadmap](../00-roadmap/README.md) · [Kubernetes là module riêng →](../15-kubernetes/README.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;Azure Architecture · Services · Configuration · Cost</span>
  <span><strong>Audience</strong>&nbsp;.NET Backend → Senior/Architect</span>
  <span><strong>Mode</strong>&nbsp;architecture-first · production-first</span>
</div>

Module 14 chỉ tập trung vào **Microsoft Azure**: chọn service, cấu hình production, networking/identity, reliability, backup/DR, observability và cost model.

**Kubernetes không còn được dạy trong page Azure này.** AKS chỉ xuất hiện như một lựa chọn compute của Azure; toàn bộ Kubernetes object model, scheduling, networking, storage, security, troubleshooting và CKAD practice nằm ở [Module 15 — Kubernetes](../15-kubernetes/README.md).

<div class="key-takeaway" markdown>
<strong>Mục tiêu không phải thuộc tên service.</strong>

Sau module này bạn phải trả lời được: **dùng service nào, vì sao, cấu hình tier/SKU nào, scale thế nào, public/private ra sao, backup thế nào, failure nào cần test và bill tăng theo biến số nào**.
</div>

---

# 1. Bức tranh Azure production

```text
Users / Mobile / Web
        │
        ▼
Azure DNS
        │
        ▼
Azure Front Door + WAF
        │
        ▼
API Management / Application Gateway
        │
        ▼
┌─────────────────────────────────────────────┐
│ COMPUTE                                     │
│ App Service · Functions · Container Apps    │
│ Virtual Machines · AKS (K8s module riêng)   │
└─────────────────────────────────────────────┘
        │              │
        │              ├──────────► Service Bus / Event Grid / Event Hubs
        │              │
        ├──────────────► Azure SQL / Cosmos DB
        ├──────────────► Azure Managed Redis
        └──────────────► Blob Storage

Runtime identity
        │
        ▼
Microsoft Entra ID / Managed Identity
        │
        ├──────────────► Key Vault
        ├──────────────► SQL / Storage / Messaging RBAC
        └──────────────► private dependencies

Network
VNet · Subnet · NSG · Route · Private Link · NAT Gateway · Firewall

Delivery
GitHub Actions / Azure DevOps → ACR → Terraform/Bicep → deploy

Operations
Azure Monitor · Application Insights · Log Analytics
Backup · PITR · restore · DR · Cost Management · Budgets
```

Azure architecture tốt không phải là stack thật nhiều service. Mỗi lớp phải có **requirement + owner + failure model + cost model**.

---

# 2. Azure service map — các dịch vụ backend thường dùng

| Nhóm | Azure services chính | Bạn phải biết gì ngoài tên service? |
|---|---|---|
| Hosting & Compute | App Service, Functions, Container Apps, VMs, AKS | plan/SKU, CPU/RAM, scale, zone, networking, deployment, cost when idle |
| Database & Storage | Azure SQL, Cosmos DB, Blob Storage, Managed Redis | consistency, tier, throughput, storage, backup, replication, cost/operation |
| Networking & Edge | DNS, VNet, NSG, Private Link, NAT Gateway, Load Balancer, Application Gateway, Front Door | traffic path, public/private, IP/DNS, capacity, WAF/DDoS, data processing cost |
| API | API Management | tier, policies, quotas, auth, VNet/private endpoint, gateway capacity |
| Identity & Secrets | Entra ID, Managed Identity, RBAC, Key Vault | principal, scope, data-plane roles, secret reduction, rotation, audit |
| Messaging | Service Bus, Event Grid, Event Hubs | delivery semantics, tier, throughput/capacity, partition, retention, DLQ |
| Observability | Azure Monitor, Application Insights, Log Analytics | sampling, ingestion, retention, alerting, SLO, telemetry cost |
| CI/CD & IaC | GitHub Actions, Azure DevOps, ACR, Terraform, Bicep | OIDC/federation, immutable artifacts, promotion, drift, rollback |
| Backup & DR | Azure Backup, SQL PITR/LTR, storage redundancy, Site Recovery | RTO/RPO, restore path, replication scope, test frequency, storage cost |
| FinOps | Cost Management, Budgets, Advisor, tags, Pricing Calculator | cost allocation, unit economics, idle/peak, egress, reservations/savings |

---

# 3. Production Service Handbook

Đây là phần nên đọc khi bạn cần **cấu hình dự án thật**, không chỉ học architecture.

| Handbook | Nội dung |
|---|---|
| [Hosting & Compute](azure-production-handbook-compute.md) | App Service, Functions, Container Apps, VM, compute decision, scale, networking, cost |
| [Data, Storage & Messaging](azure-production-handbook-data-messaging.md) | Azure SQL, Cosmos DB, Blob, Managed Redis, Service Bus, Event Grid, Event Hubs |
| [Networking, Edge, IAM & Security](azure-production-handbook-network-security.md) | DNS, VNet, Private Link, NAT, LB, App Gateway, Front Door/WAF/DDoS, APIM, Entra, Key Vault |
| [Observability, Delivery, Backup, DR & Cost](azure-production-handbook-operations-cost.md) | Monitor/App Insights/Logs, GitHub/Azure DevOps, ACR, IaC, backup/restore, FinOps |

Mỗi service card dùng cùng mental model:

```text
Problem solved
→ When to use / when not to use
→ Tier / SKU / purchasing model
→ Minimal production configuration
→ Networking + identity
→ Scale + availability
→ Backup / recovery if applicable
→ Quotas / limits
→ Observability
→ Cost drivers + cost traps
→ Failure drills
→ Official source
```

---

# 4. Chọn compute — Azure trước, Kubernetes sau

```text
HTTP/API thông thường
        │
        ├─ App Service đáp ứng NFR? ─────────► App Service
        │
        ▼ no
Container lifecycle + autoscale/revisions?
        │
        ├─ Container Apps đáp ứng? ──────────► Container Apps
        │
        ▼ no
Event/trigger execution?
        │
        ├─ yes ──────────────────────────────► Functions
        │
        ▼
Need OS-level control / legacy software? ────► VM / VMSS
        │
        ▼
Need Kubernetes API/operators/scheduling/platform controls?
        └────────────────────────────────────► AKS → Module 15
```

**AKS không phải mặc định cho microservices.** Nếu Azure PaaS đáp ứng NFR với ít operating surface hơn, đó thường là phương án cần được cân nhắc trước.

→ [Kubernetes & AKS learning path](../15-kubernetes/README.md)

---

# 5. Reference architecture — .NET backend PaaS-first

```text
Internet
  ↓
Azure DNS
  ↓
Front Door Premium + WAF        (khi global edge/WAF justify)
  ↓
API Management                  (khi cần API product/policy boundary)
  ↓
App Service / Container Apps
  ├─ Checkout API
  ├─ Payment Worker
  └─ Notification Worker
  │
  ├── Azure SQL                 source of truth
  ├── Service Bus               durable async work
  ├── Blob Storage              objects/files
  └── Managed Redis             derived fast state

Managed Identity
  ├── SQL data permission
  ├── Service Bus sender/receiver
  ├── Storage data roles
  └── Key Vault only for real external secrets

Private Link / VNet integration where required

Azure Monitor + Application Insights + Log Analytics
  ↓
SLO · traces · alerts · cost/ingestion control
```

Đây là baseline dễ vận hành hơn một Kubernetes platform nếu team chưa có Kubernetes-specific requirement.

---

# 6. Resource hierarchy và landing zone

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

Production design phải làm rõ:

```text
ownership
billing boundary
policy boundary
prod/non-prod isolation
blast radius
quota
compliance
lifecycle
```

Một setup phổ biến:

```text
Tenant
├─ Platform
│  ├─ Connectivity subscription
│  └─ Management/shared-services subscription
│
└─ Landing Zones
   ├─ Production subscription
   └─ Non-production subscription
```

→ [Azure Foundations & Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md)

---

# 7. Identity + Network là hai control khác nhau

```text
Private Endpoint
!=
authorized caller
```

Một path production nên reason như sau:

```text
Internet
→ WAF / ingress
→ authenticated API caller
→ application
→ Managed Identity
→ private endpoint where required
→ target resource authorization
```

Checklist:

- public endpoints inventory;
- VNet/subnet/IP plan;
- private DNS ownership;
- outbound IP requirement;
- RBAC/data-plane roles;
- secret rotation;
- least privilege;
- audit trail.

→ [Azure Identity, Networking & Zero Trust](azure-identity-networking-and-zero-trust.md)

---

# 8. Cost không phải giá trên pricing page

Không hard-code một con số `$X/tháng` rồi dùng cho mọi project. Azure bill phụ thuộc region, agreement, tier và workload.

Dùng model:

```text
monthly_cost
=
base / allocated capacity
+ runtime compute
+ storage
+ operations / requests
+ data processing
+ network egress
+ observability ingestion + retention
+ backup / replica / DR
+ security / gateway fixed capacity
```

Ví dụ:

```text
App Service
≈ plan instances × running time

Container Apps Consumption
≈ active/idle resource consumption + requests + networking extras

Azure SQL
≈ compute + reserved data storage + backup storage + replica/DR

Cosmos DB
≈ RU capacity/consumption + storage + regions

Blob
≈ capacity + transactions + retrieval + redundancy + egress

Service Bus
≈ namespace/tier capacity + operations/features

Event Hubs
≈ TU/PU capacity + retention/capture/data transfer

Log Analytics
≈ ingestion + retention/query/export model
```

Luôn lưu trong ADR/FinOps note:

```text
Region
Pricing date
Workload assumptions
Peak + idle assumptions
Data volume
Retention
HA/DR assumptions
```

→ [Operations, Backup, DR & Cost Handbook](azure-production-handbook-operations-cost.md)

---

# 9. Cách áp dụng vào dự án thật

## Step 0 — workload brief

```text
Peak RPS / jobs / messages
P95/P99 latency
availability SLO
RTO / RPO
DB size + growth
file volume
log GB/day
external egress
security/compliance
team capability
```

## Step 1 — resource organization

Output:

```text
subscriptions
resource groups
tags
RBAC
budgets
policy
```

## Step 2 — request/network path

Output:

```text
DNS
edge/WAF
API ingress
compute
private dependencies
outbound path
```

## Step 3 — service/SKU decision

Không ghi:

```text
"Use Azure SQL"
```

Ghi:

```text
Azure SQL Database
vCore purchasing model
General Purpose / Hyperscale depending measured workload
serverless vs provisioned decision
backup retention
zone/geo requirement
max compute/storage guardrail
```

## Step 4 — identity

```text
human identities
CI/CD deploy identity
runtime Managed Identity
resource data-plane roles
external secrets only in Key Vault
```

## Step 5 — delivery

```text
commit
→ tests
→ immutable artifact/image
→ security checks
→ IaC plan
→ non-prod
→ smoke/integration
→ production rollout
→ SLO gate
→ rollback
```

## Step 6 — observability + cost baseline

```text
business SLI
service latency/error
capacity
queue age
DB metrics
logs GB/day
monthly cost by resource/tag
budget alert
```

## Step 7 — failure/restore drills

```text
bad deployment
compute instance loss
dependency timeout
DB throttling
queue backlog
secret rotation
restore database
restore object
zone/region scenario when NFR requires
```

---

# 10. Learning path

| Guide | Mục tiêu |
|---|---|
| [Cloud Primitives, Identity & Networking](cloud-primitives-identity-and-networking.md) | hiểu primitive cloud trước Azure product names |
| [Azure Foundations & Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md) | hierarchy, subscription, policy, governance |
| [Compute Handbook](azure-production-handbook-compute.md) | cấu hình + cost App Service/Functions/Container Apps/VM |
| [Data & Messaging Handbook](azure-production-handbook-data-messaging.md) | database/storage/cache/messaging + capacity/cost |
| [Network & Security Handbook](azure-production-handbook-network-security.md) | edge, DNS, VNet, private connectivity, IAM/security |
| [Operations & Cost Handbook](azure-production-handbook-operations-cost.md) | observability, CI/CD, ACR/IaC, backup/DR, FinOps |
| [Azure Reliability, Observability & Cost](azure-reliability-observability-governance-and-cost.md) | reliability reasoning / SLO / failure model |
| [Azure .NET Reference Architecture](azure-dotnet-reference-architecture.md) | ghép các quyết định thành hệ thống checkout thật |
| [References](references.md) | Microsoft source of truth |

---

# 11. Exit criteria

Bạn hoàn thành Module 14 khi có thể lấy một backend .NET và tạo được:

- architecture diagram + traffic path;
- resource hierarchy/ownership;
- compute decision có tier/SKU;
- database/storage/message decision có capacity model;
- network + DNS + private/public boundary;
- Managed Identity/RBAC matrix;
- IaC + CI/CD path;
- observability/SLO dashboard plan;
- backup/restore/DR plan;
- monthly cost worksheet với assumptions;
- failure drills + runbook;
- ADR giải thích trade-off.

Nếu requirement dẫn tới AKS, chuyển sang **[Module 15 — Kubernetes](../15-kubernetes/README.md)** thay vì biến Module 14 thành khóa Kubernetes.

## Verification metadata

- Verified: 2026-08-28.
- Azure-specific behavior: Microsoft Learn / Azure Architecture Center / Azure Well-Architected are canonical.
- Pricing: document cost drivers and calculator assumptions; re-check live price before production decisions.
- Kubernetes: intentionally separated to Module 15.
