# Azure Core Platform — Resource Model, Control Plane & Governance

> [← Azure overview](README.md) · [Landing Zones](azure-foundations-resource-hierarchy-and-landing-zones.md) · [Terraform on Azure](../13-devops-iac/terraform-on-azure-production.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;ARM · scopes · regions/zones · RBAC · Policy · networking · identity · operations</span>
  <span><strong>Goal</strong>&nbsp;hiểu Azure platform trước khi chọn service</span>
</div>

<div class="key-takeaway" markdown>
<strong>Azure không phải danh sách services.</strong>

Muốn thiết kế đúng, hãy hiểu trước resource model, control plane/data plane, identity, network path, governance inheritance, availability scope và cost/operational boundaries.
</div>

---

# 1. Azure mental model

~~~text
Microsoft Entra tenant
      ↓ identity trust
Management Groups
      ↓ governance inheritance
Subscriptions
      ↓ billing / quota / blast radius
Resource Groups
      ↓ lifecycle grouping
Resources
      ↓
Azure Resource Manager control plane

Applications
      ↓
service data planes
      ↓
SQL / Storage / Service Bus / Key Vault / ...
~~~

Hai câu hỏi khác nhau:

~~~text
Can I create/configure this Azure resource?
→ control plane

Can my application read/write data inside this resource?
→ data plane
~~~

---

# 2. Control plane vs data plane

## Control plane

Management operations:

~~~text
create storage account
change App Service configuration
assign role
create VNet
delete SQL server
~~~

Thường đi qua Azure Resource Manager.

## Data plane

Service-specific operations:

~~~text
read Blob
send Service Bus message
query SQL
read Key Vault secret
~~~

Principal có quyền Contributor trên resource chưa chắc có data-plane permission cần thiết.

Ví dụ:

~~~text
Contributor on Storage Account
!=
Storage Blob Data Contributor
~~~

Đây là distinction bắt buộc khi debug 403.

---

# 3. Azure Resource Manager

ARM là control-plane model/API của Azure.

Terraform/Bicep/Portal/CLI đều cuối cùng tương tác với Azure management APIs theo cách tương ứng.

Mental model:

~~~text
Portal / CLI / Bicep / Terraform
        ↓
Azure Resource Manager
        ↓
Resource Provider
        ↓
Azure resource
~~~

IaC tool khác nhau không thay resource model cơ bản.

---

# 4. Resource Provider và Resource Type

Azure resources thuộc namespace/type.

Ví dụ conceptual:

~~~text
Microsoft.Web/sites
Microsoft.Storage/storageAccounts
Microsoft.Network/virtualNetworks
Microsoft.Sql/servers/databases
~~~

Resource provider là control-plane service chịu trách nhiệm API cho resource family.

Khi API/version/provider support thay đổi, IaC/provider behavior có thể thay đổi dù architecture concept vẫn giữ nguyên.

---

# 5. Resource ID

Azure Resource ID encode hierarchy.

Shape:

~~~text
/subscriptions/<subscription-id>
/resourceGroups/<rg>
/providers/<namespace>/<type>/<name>
~~~

Resource ID quan trọng trong:

- RBAC scopes;
- Terraform references;
- diagnostic settings;
- Private Link targets;
- policy;
- cross-resource relationships.

Treat resource ID as identity, not display name.

---

# 6. Scope hierarchy

Azure management scopes:

~~~text
Management Group
      ↓
Subscription
      ↓
Resource Group
      ↓
Resource
~~~

RBAC/Policy assigned ở scope cao có thể inherit xuống dưới.

Blast radius tăng khi scope rộng hơn.

Principle:

~~~text
assign at the lowest practical scope
unless platform governance intentionally needs inheritance
~~~

---

# 7. Subscription là boundary mạnh

Subscription thường là boundary cho:

- billing/cost allocation;
- quotas;
- RBAC scope;
- Policy scope;
- platform ownership;
- deployment blast radius.

Resource Group hữu ích nhưng không thay subscription isolation khi requirement cần boundary mạnh hơn.

Common enterprise shape:

~~~text
Platform subscriptions
├── connectivity
├── management
└── identity/shared services when justified

Application landing zones
├── commerce-prod
├── commerce-nonprod
└── payments-prod
~~~

---

# 8. Resource Group

Resource Group là lifecycle/management grouping.

Good question:

~~~text
Which resources are normally deployed/operated/decommissioned together?
~~~

Bad pattern:

~~~text
rg-all-databases
rg-all-storage
rg-all-apps
~~~

nếu chúng thuộc nhiều products/owners/lifecycles khác nhau.

---

# 9. Platform Landing Zone vs Application Landing Zone

Platform landing zone:

~~~text
management groups
policy
shared connectivity
DNS
central security/monitoring
subscription vending
governance baseline
~~~

Application landing zone:

~~~text
workload subscription
workload network/resources
application identities
data services
workload monitoring
~~~

Platform team tạo guardrails.
Workload team design workload bên trong guardrails.

---

# 10. RBAC — Who can do what?

RBAC model:

~~~text
Security principal
+
Role definition
+
Scope
=
authorization assignment
~~~

Principal có thể là:

- user;
- group;
- service principal;
- managed identity.

Use built-in roles when they fit.
Custom role only when needed.

Avoid defaulting to Owner/Contributor at subscription scope.

---

# 11. Azure Policy — What configuration may exist?

Azure Policy evaluates resource/config compliance.

Typical intents:

- approved regions;
- required tags;
- deny public network;
- require diagnostics;
- allowed SKUs;
- enforce security settings.

Distinction:

~~~text
RBAC
= actor authorization

Policy
= resource/configuration governance
~~~

A deploy identity can be authorized by RBAC but still blocked by Policy.

---

# 12. Resource Locks

Locks protect control-plane operations.

Common modes:

~~~text
CanNotDelete
ReadOnly
~~~

Lock is guardrail, not backup.

It can also block Terraform/Bicep/operator operations.

Document:

- who can remove lock;
- emergency workflow;
- expected interaction with deployment pipelines.

---

# 13. Tags

Tags help:

- owner;
- application;
- environment;
- cost center;
- criticality;
- data classification.

They are metadata, not strong security boundaries.

A useful tag must answer an operational question.

---

# 14. Region

Region is geographic Azure deployment area.

Region choice affects:

- latency;
- service availability;
- data residency;
- price;
- quota/capacity;
- paired/DR options;
- network egress.

Do not copy region from tutorial without checking workload requirements.

---

# 15. Availability Zones

Availability Zones are physically separated datacenter groups within supported regions.

Zone-aware design asks:

~~~text
Is service zonal or zone-redundant?
What happens if one zone is unavailable?
Does the SKU support zones?
How is data replicated?
Does traffic fail over automatically?
What extra cost exists?
~~~

Zone support varies by service/region/SKU.

---

# 16. Region failure is different from zone failure

~~~text
instance failure
!=
zone failure
!=
region failure
~~~

Each requires different design.

Example:

~~~text
zone-redundant service
may tolerate zone loss

but

regional outage
may still require cross-region strategy
~~~

Define RTO/RPO before choosing geo architecture.

---

# 17. Quotas and capacity

Azure capacity is not infinite.

Deployment may fail due to:

- subscription quota;
- regional capacity;
- SKU availability;
- service limits;
- network/IP constraints.

Quota is architecture input.

Production launch checklist should include required quota/capacity preflight.

---

# 18. Shared Responsibility

Managed Azure service reduces infrastructure operations but does not remove application responsibility.

Example managed database:

Azure owns much of:

~~~text
hardware
host OS
service control plane
~~~

You still own:

~~~text
schema
query/index design
identity
network exposure
data lifecycle
backup/restore settings
capacity/tier
application retry semantics
~~~

Managed != no operations.

---

# 19. Microsoft Entra ID

Entra ID is identity platform for:

- users;
- groups;
- applications/service principals;
- managed identities;
- federated identities.

Azure resource hierarchy and Entra tenant are related but not the same hierarchy.

Identity decisions cross:

~~~text
human access
CI/CD access
runtime workload access
external application access
~~~

Keep them separate.

---

# 20. Managed Identity

Managed Identity lets Azure workloads authenticate without application-managed secret.

~~~text
Azure workload
→ Managed Identity
→ Entra token
→ Azure service
~~~

Prefer this for Azure-to-Azure access when supported.

Two common forms:

~~~text
system-assigned
user-assigned
~~~

Choose based on lifecycle/identity-sharing requirements.

---

# 21. Authentication vs Authorization

~~~text
Authentication
= who are you?

Authorization
= what are you allowed to do?
~~~

A token can be valid but caller still gets 403 because RBAC/data-plane role is missing.

Debug separately.

---

# 22. VNet and Subnet

VNet is private IP network boundary.

Subnet is address/lifecycle/security subdivision.

Design needs:

- non-overlapping CIDR;
- growth headroom;
- service delegation requirements;
- private endpoint placement;
- route/NSG strategy;
- hub/spoke connectivity.

Do not allocate tiny subnets from tutorials without capacity reasoning.

---

# 23. NSG

Network Security Group controls network traffic by rules.

Think:

~~~text
source
destination
port/protocol
priority
allow/deny
~~~

NSG is network control, not application authorization.

Even allowed TCP connectivity still requires identity/auth at target service.

---

# 24. Route Table

Routes decide next hop.

Common path:

~~~text
spoke workload
→ route
→ firewall/NVA/NAT
→ destination
~~~

A broken route can look like:

- DNS works but connection times out;
- outbound dependency unreachable;
- asymmetric path;
- private endpoint path wrong.

Network troubleshooting should inspect route, DNS and security controls separately.

---

# 25. Hub-Spoke

Hub hosts shared network services:

~~~text
firewall
gateway
DNS resolver
Bastion
shared routing
~~~

Spokes host workloads.

Avoid placing application workloads in hub without strong reason.

Benefits:

- centralized connectivity;
- team ownership separation;
- reusable security/DNS.

Trade-off:

- routing complexity;
- cost;
- shared-hub blast radius.

---

# 26. Virtual WAN

Virtual WAN provides managed transit/hub capability for larger/multi-region/hybrid networks.

Don't choose it because "enterprise".

Use when connectivity scale/operations justify it.

Private DNS design differs from classic hub/spoke and must be planned explicitly.

---

# 27. Public Endpoint vs Service Endpoint vs Private Endpoint

## Public endpoint

Service reachable via public network endpoint, potentially protected by firewall/auth.

## Service Endpoint

Extends VNet identity/routing semantics to supported Azure services while service still uses its public endpoint model.

## Private Endpoint

Creates private IP in your VNet mapped to supported PaaS resource via Private Link.

Key rule:

~~~text
Private Endpoint
!=
authorization
~~~

Network private + identity permission are separate controls.

---

# 28. Private DNS

Private Endpoint design is incomplete without DNS.

Expected path:

~~~text
service FQDN
→ private DNS resolution
→ private endpoint IP
→ service
~~~

Failure pattern:

~~~text
Private Endpoint exists
but DNS still resolves public address
~~~

or on-prem/hub clients cannot resolve linked private zones.

Define DNS ownership centrally for enterprise networks.

---

# 29. Azure DNS Private Resolver

Private Resolver can bridge Azure private DNS and custom/on-prem DNS without self-managed DNS VMs.

Concepts:

~~~text
inbound endpoint
outbound endpoint
forwarding ruleset
VNet links
~~~

Use when hybrid/hub-spoke/private-link DNS requirements justify it.

---

# 30. Outbound connectivity

Private workload still often needs outbound:

- package/API endpoints;
- external business services;
- certificate/identity endpoints.

Plan:

~~~text
egress path
source IP
NAT
firewall
DNS
allowlist
logging
cost
~~~

Don't only design inbound traffic.

---

# 31. Load Balancer vs Application Gateway vs Front Door

Simplified mental model:

~~~text
Azure Load Balancer
→ regional L4 TCP/UDP

Application Gateway
→ regional L7 HTTP(S), WAF capability

Azure Front Door
→ global edge L7, routing, acceleration, WAF
~~~

Use requirement first:

~~~text
global?
regional?
L4 or L7?
WAF?
private origin?
TLS?
session/path routing?
~~~

---

# 32. API Management

APIM is API gateway/product/policy boundary, not default reverse proxy for every internal call.

Good use cases:

- external/partner API product;
- subscription/product management;
- API policy;
- versioning/developer surface;
- controlled gateway boundary.

Trade-offs:

- tier/cost;
- latency;
- network complexity;
- operational dependency.

---

# 33. Data service selection

Ask workload shape first.

~~~text
relational transaction/query
→ Azure SQL / managed relational option

document/global distribution
→ Cosmos DB when semantics justify

object/file
→ Blob Storage

cache/derived state
→ Managed Redis

durable command/work queue
→ Service Bus

event routing
→ Event Grid

high-throughput event stream
→ Event Hubs
~~~

Don't choose service by brand familiarity.

---

# 34. Compute selection

Order of consideration:

~~~text
managed PaaS sufficient?
├─ App Service
├─ Functions
└─ Container Apps

need OS/runtime control?
→ VM / VMSS

need Kubernetes API/platform capabilities?
→ AKS
~~~

AKS is not synonym for "production".

---

# 35. Azure Monitor mental model

Azure operations surface includes:

~~~text
Metrics
Logs
Traces / Application Insights
Alerts
Dashboards / Workbooks
Resource Health
Service Health
Activity Log
~~~

Distinguish:

~~~text
application telemetry
platform metrics
control-plane audit
Azure incident/service health
~~~

Do not put everything into one unbounded Log Analytics retention policy.

---

# 36. Activity Log

Activity Log records subscription-level control-plane events.

Useful questions:

~~~text
Who changed this resource?
Which operation failed?
Which deployment modified config?
~~~

It is not application request logging.

---

# 37. Resource Health vs Service Health

Conceptually:

~~~text
Resource Health
→ state/availability of a specific resource

Service Health
→ Azure service incidents/planned maintenance/advisories affecting your context
~~~

Both matter for incident triage.

---

# 38. Azure Advisor

Advisor provides recommendations across areas such as reliability, security, performance, operational excellence and cost.

Treat as signal/input.

Don't auto-apply recommendation without workload context.

---

# 39. Well-Architected Framework

Azure WAF uses five pillars:

~~~text
Reliability
Security
Cost Optimization
Operational Excellence
Performance Efficiency
~~~

Every significant Azure design should explain trade-offs across pillars.

Example:

~~~text
add cross-region active-active
→ reliability ↑
→ cost ↑
→ operational complexity ↑
→ data consistency complexity ↑
~~~

---

# 40. Cost model

Azure cost is workload function:

~~~text
compute time/capacity
+ storage capacity
+ transactions/operations
+ data transfer
+ replicas/DR
+ monitoring ingestion/retention
+ fixed gateway/security capacity
+ support/platform extras
~~~

Always record:

- region;
- SKU/tier;
- expected idle/peak;
- data growth;
- retention;
- HA/DR assumptions.

---

# 41. Cost controls

Tools/practices:

~~~text
Cost Management
Budgets
Alerts
Tags
Advisor
Reservations / Savings Plans when appropriate
rightsizing
autoscaling
log retention/sampling
storage lifecycle
~~~

Discount commitment is capacity planning decision, not automatic optimization.

---

# 42. Resource deletion is not data recovery strategy

Terraform/Bicep can recreate infrastructure.

They do not recreate lost business data automatically.

Need separate:

~~~text
infrastructure recovery
data backup/restore
configuration recovery
secret/key recovery
DNS/traffic recovery
~~~

RTO/RPO must cover data path.

---

# 43. IaC ownership

Good ownership:

~~~text
Terraform/Bicep
→ Azure platform/resources

application deployment
→ artifact/CD system

Kubernetes GitOps
→ workload objects inside AKS
~~~

Avoid multiple desired-state controllers fighting over same fields.

---

# 44. Azure architecture debugging order

When a workload is unreachable:

~~~text
1. DNS resolves to what?
2. public/private endpoint?
3. route/next hop?
4. NSG/firewall?
5. service public-network setting?
6. TLS/hostname?
7. authentication?
8. authorization/data-plane role?
9. service health?
10. application health?
~~~

Don't start by changing firewall rules blindly.

---

# 45. Governance debugging order

When deployment is denied:

~~~text
1. correct tenant/subscription?
2. correct deploy identity?
3. RBAC action allowed?
4. Policy deny/modify?
5. resource lock?
6. resource provider/API?
7. region/SKU/quota?
8. dependency state?
~~~

This prevents mixing authorization with platform capacity errors.

---

# 46. Architecture learning path

Study in this order:

~~~text
Resource model + scopes
↓
Control plane vs data plane
↓
Identity / RBAC / Policy
↓
Region / Zone / Quota
↓
VNet / Routing / DNS / Private Link
↓
Compute / Data / Messaging selection
↓
Observability / Reliability / Cost
↓
Landing Zone
↓
Terraform / Bicep automation
↓
Failure drills / restore
~~~

Then go deep into service handbooks.

---

# 47. Production review checklist

Before approving Azure architecture:

- [ ] workload SLO/RTO/RPO known;
- [ ] subscription/owner/cost boundary known;
- [ ] region and zone strategy explicit;
- [ ] public endpoints inventoried;
- [ ] DNS/route/egress path diagrammed;
- [ ] human/CI/runtime identities separate;
- [ ] control-plane and data-plane roles understood;
- [ ] Policy/locks reviewed;
- [ ] quotas checked;
- [ ] compute/data/messaging service chosen by workload;
- [ ] backup/restore tested;
- [ ] platform + application telemetry defined;
- [ ] cost drivers modeled;
- [ ] IaC ownership clear;
- [ ] failure scenarios rehearsed.

---

# 48. Exit Criteria

Bạn hoàn thành chapter khi có thể:

- [ ] explain tenant/MG/subscription/RG/resource;
- [ ] explain ARM/resource provider/resource ID;
- [ ] distinguish control plane and data plane;
- [ ] distinguish AuthN/AuthZ;
- [ ] distinguish RBAC/Policy/resource locks;
- [ ] choose subscription boundary by governance/blast radius;
- [ ] explain region vs zone vs regional DR;
- [ ] reason about quota/capacity;
- [ ] explain VNet/subnet/NSG/routes;
- [ ] explain public endpoint/service endpoint/private endpoint;
- [ ] design Private Endpoint DNS;
- [ ] explain managed identity;
- [ ] choose L4/L7/global edge primitive;
- [ ] distinguish service health/application health;
- [ ] review design against five WAF pillars;
- [ ] separate infrastructure recovery from data recovery;
- [ ] map IaC ownership cleanly.

## Official sources

- Azure landing zones: https://learn.microsoft.com/azure/cloud-adoption-framework/ready/landing-zone
- Management groups: https://learn.microsoft.com/azure/cloud-adoption-framework/ready/landing-zone/design-area/resource-org-management-groups
- Azure Well-Architected Framework: https://learn.microsoft.com/azure/well-architected/what-is-well-architected-framework
- Hub-spoke network: https://learn.microsoft.com/azure/networking/design-guide/hub-spoke
- Private Link/DNS: https://learn.microsoft.com/azure/architecture/networking/guide/private-link-virtual-wan-dns-guide
- Azure DNS Private Resolver: https://learn.microsoft.com/azure/dns/private-resolver-endpoints-rulesets

## Verification metadata

- Verified: 2026-09-25.
- Focus is stable Azure platform mental models; exact SKU, region support and service limits must be checked before provisioning.
