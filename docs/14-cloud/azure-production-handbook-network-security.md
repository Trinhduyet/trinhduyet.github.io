# Azure Production Handbook — Networking, Edge, IAM & Security

> [← Data & Messaging](azure-production-handbook-data-messaging.md) · [Azure overview](README.md) · [Operations & Cost →](azure-production-handbook-operations-cost.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;DNS · VNet · Private Link · NAT · Load Balancing · WAF/DDoS · APIM · Entra · Key Vault</span>
  <span><strong>Goal</strong>&nbsp;design traffic + trust boundaries + cost</span>
</div>

Network architecture phải trả lời hai câu riêng:

```text
Can traffic reach the resource?
!=
Is the caller authorized to use the resource?
```

---

# 1. Traffic-path-first design

Trước khi tạo VNet, viết traffic matrix:

| From | To | Protocol | Public/private | Identity | Reason |
|---|---|---|---|---|---|
| Internet | Front Door | HTTPS | public | end user | public ingress |
| Front Door | API origin | HTTPS | controlled | platform/origin auth | application ingress |
| API | Azure SQL | TDS | private where required | Managed Identity | source of truth |
| Worker | Service Bus | AMQP/HTTPS | private where required | Managed Identity | messaging |
| API | payment provider | HTTPS | public controlled egress | API credential | external dependency |

Architecture diagram không thay traffic matrix.

---

# 2. Azure DNS

Azure DNS hiện bao gồm các building blocks cần biết:

```text
Azure Public DNS
Azure Private DNS
Azure DNS Private Resolver
Traffic Manager (DNS-based global routing use cases)
```

Official: <https://learn.microsoft.com/en-us/azure/dns/dns-overview>

## Public DNS

Use:

```text
api.example.com
→ Front Door / public endpoint
```

Azure Public DNS hosts records; nó không phải domain registrar mặc định cho mọi use case.

## Private DNS

Private Endpoint thường kéo theo private DNS design:

```text
app
→ resolves service FQDN
→ private DNS zone
→ private endpoint IP
```

Common failure:

```text
private endpoint exists
but DNS resolves public endpoint
→ timeout / blocked path / unexpected route
```

## Private Resolver

Strong signal:

```text
Azure VNet ↔ on-prem DNS resolution
hub-spoke hybrid resolution
centralized forwarding rules
```

Production checklist:

```text
[ ] public zone owner
[ ] private zones inventory
[ ] VNet links
[ ] resolver/forwarder ownership
[ ] TTL/change strategy
[ ] private endpoint DNS mapping
[ ] hybrid failover/runbook
```

## Cost drivers

```text
hosted zones
DNS queries
Private Resolver endpoints/rules/processing depending service model
```

DNS cost nhỏ trong nhiều hệ thống nhưng DNS failure blast radius rất lớn.

---

# 3. VNet, Subnet, NSG, Route Table

## VNet

VNet = private network address/control boundary, không phải security authorization boundary.

Design inputs:

```text
CIDR/IP growth
peering/hub-spoke
private endpoints
PaaS integration
AKS/VM/container IP demand
hybrid connectivity
future region/environment growth
```

## Subnet

Partition subnet theo **technical control/ownership/lifecycle**, không phải mỗi app một subnet mặc định.

Examples:

```text
ingress subnet
application integration subnet
private endpoint subnet strategy
firewall subnet
DNS resolver delegated subnet
VM workload subnet
```

## NSG

NSG controls L3/L4 reachability.

```text
NSG allow packet
!= application authorization
```

Rules nên bắt nguồn từ traffic matrix và test negative path.

## UDR / routes

Use when traffic cần forced path:

```text
subnet
→ route table
→ Firewall/NVA
→ destination
```

Bad route có thể blackhole whole subnet; IaC + staged rollout + effective-routes troubleshooting cần nằm trong runbook.

---

# 4. Private Link / Private Endpoint

Mental model:

```text
PaaS public service FQDN
        ↓ DNS override
private endpoint IP in VNet
        ↓
Private Link
        ↓
Azure PaaS resource
```

Use when requirement cần giảm public data-plane exposure.

Production checklist:

```text
[ ] public network access decision
[ ] private endpoint subnet/IP
[ ] private DNS zone
[ ] cross-VNet/hybrid resolution
[ ] RBAC/data authorization remains enforced
[ ] fallback/runbook if DNS breaks
```

Cost drivers:

```text
private endpoint hours
+ data processed
+ DNS/networking dependencies
```

Private every resource without requirement can increase cost and troubleshooting surface.

---

# 5. NAT Gateway — outbound egress

NAT Gateway provides managed outbound SNAT for private subnet workloads.

Current SKU families include:

```text
Standard
StandardV2
```

StandardV2 is zone-redundant by default and supports IPv4/IPv6 capabilities beyond Standard. Always confirm regional/feature availability.

Official: <https://learn.microsoft.com/en-us/azure/nat-gateway/nat-gateway-resource>

Strong signals:

```text
stable outbound public IP
large SNAT scale
private workloads needing internet egress
allow-list external provider by source IP
```

NAT Gateway does **not** provide arbitrary inbound internet access.

## Cost model

Think:

```text
NAT resource running time
+ data processed
+ public IP/prefix
```

Cost trap:

```text
high-volume internet egress
→ NAT data processing
+ Azure bandwidth egress
```

Both may matter.

---

# 6. Azure Load Balancer

Azure Load Balancer is L4 TCP/UDP load balancing.

Use when you need:

```text
public/internal L4 distribution
VM/VMSS backend
high-performance non-HTTP traffic
```

Do not choose it for HTTP path rules/WAF; that is Application Gateway/Front Door class.

Current architecture should use supported current SKUs and avoid legacy Basic assumptions.

Think about:

```text
frontend IP
backend pool
health probe
load-balancing rule
outbound behavior
zone model
```

Failure drill:

```text
make one backend probe fail
→ verify it leaves healthy rotation
```

---

# 7. Application Gateway v2 / WAF_v2

Application Gateway = regional application delivery/load balancing, typically L7 HTTP(S) and current v2 capabilities.

**Application Gateway v1 retired on 2026-04-28.** New/current design must use v2-family guidance.

Official: <https://learn.microsoft.com/en-us/azure/application-gateway/overview-v2>

Current SKUs:

```text
Standard_v2
WAF_v2
```

V2 supports autoscaling, zone redundancy capabilities and static VIP behavior.

## Cost model

V2 billing is not simply instance count:

```text
fixed gateway cost
+ capacity-unit cost
+ public IP
+ data/network
```

Capacity Unit depends on the constraining factor such as throughput/connections/compute model defined by current docs.

Official pricing model: <https://learn.microsoft.com/en-us/azure/application-gateway/understanding-pricing>

## Production configuration

```text
[ ] Standard_v2 vs WAF_v2
[ ] frontend public/private
[ ] TLS/certificates
[ ] listeners/rules
[ ] backend health probes
[ ] end-to-end TLS if required
[ ] autoscale min/max
[ ] zone design
[ ] WAF policy mode/rules
[ ] diagnostic logs
```

WAF false positives are a real failure mode; stage/tune policies.

---

# 8. Azure Front Door Standard/Premium

Front Door = global HTTP(S) edge, acceleration, routing, caching/origin failover class.

Current tiers:

```text
Standard
Premium
```

Azure Front Door classic is retiring; new architecture should use Standard/Premium guidance.

WAF has richer native integration/features with Premium; verify exact rule/features before SKU decision.

Official:

- <https://learn.microsoft.com/en-us/azure/frontdoor/>
- <https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/afds-overview>

## Use when

```text
global users
multi-region HTTP origin routing
edge WAF
CDN/caching/acceleration
health-based origin failover
```

Don't stack Front Door + App Gateway + APIM automatically. Every layer adds:

```text
latency
fixed/variable cost
TLS/config complexity
observability boundaries
failure modes
```

Each layer needs a distinct requirement.

---

# 9. WAF vs DDoS Protection

Do not conflate them.

```text
WAF
→ application-layer HTTP attacks
→ injection/XSS/bots/request rules

DDoS Protection
→ network/volumetric/protocol attacks for supported public IP scenarios
```

Azure DDoS Protection current tiers:

```text
DDoS IP Protection
→ per-public-IP model; smaller footprint

DDoS Network Protection
→ VNet-plan model; broader protected public IP footprint + advanced response/cost-protection capabilities
```

Official:

- <https://learn.microsoft.com/en-us/azure/ddos-protection/ddos-protection-sku-comparison>
- <https://learn.microsoft.com/en-us/azure/ddos-protection/fundamental-best-practices>

DDoS applicability differs by PaaS/public-IP deployment model; confirm supported topology.

---

# 10. API Management

APIM solves **API product/policy/governance**, not merely load balancing.

Use cases:

```text
auth/token policy
rate limit/quota
API products/subscriptions
version/revision governance
transformation
central API analytics/policy
external/internal developer exposure
```

Current tier landscape includes:

```text
Consumption
classic Developer/Basic/Standard/Premium
Basic v2 / Standard v2 / Premium v2
```

Current v2 guidance:

```text
Basic v2      → development/test with SLA-related positioning
Standard v2   → production-ready, network-isolated backend support
Premium v2    → enterprise isolation/scale/availability-zone capabilities
```

Official:

- <https://learn.microsoft.com/en-us/azure/api-management/v2-service-tiers-overview>
- <https://learn.microsoft.com/en-us/azure/api-management/api-management-features>
- <https://learn.microsoft.com/en-us/azure/api-management/service-limits>

## Production configuration

```text
[ ] tier chosen by feature/network/capacity, not name
[ ] API/product/version model
[ ] JWT/OAuth validation policy
[ ] rate limit/quota semantics
[ ] backend timeout/retry reviewed
[ ] idempotency impact reviewed
[ ] private endpoint/VNet requirements
[ ] certificates/named values
[ ] managed identity to backend where suitable
[ ] gateway logs sampled/retained
```

Gateway retry on non-idempotent POST can duplicate business actions. Policy must respect application correctness.

## Cost drivers

Depends tier but model:

```text
base/scale units or consumption requests
+ gateway/networking
+ observability
+ multi-region/enterprise features where applicable
```

APIM can become major fixed cost in small systems; require a real API-governance need.

---

# 11. Microsoft Entra ID + Managed Identity + RBAC

## Human vs workload identity

Separate:

```text
human user/group
CI/CD deploy principal
runtime workload identity
external application identity
```

Do not reuse one broad service principal for all.

## Managed Identity

Mental model:

```text
Azure workload
→ Managed Identity obtains Entra token
→ target service validates identity
→ RBAC/data-plane permission authorizes action
```

Managed Identity removes credential lifecycle, **not authorization design**.

## RBAC scope

Prefer narrowest practical scope:

```text
management group
subscription
resource group
resource
resource-specific data plane
```

Don't grant `Owner` to fix a data-plane 403.

Production output should include a permission matrix:

| Principal | Resource | Role/action | Scope | Why |
|---|---|---|---|---|
| checkout-api | SQL | required DB access | checkout DB | order operations |
| payment-worker | Service Bus | receiver | payment queue | process commands |
| CI deploy | workload RG | deploy required resources | resource group | CI/CD only |

---

# 12. Key Vault

Key Vault stores actual secrets/keys/certificates that cannot be replaced by identity.

Good sequence:

```text
Can workload use Managed Identity directly?
  yes → don't create a secret
  no  → secret/key/certificate → Key Vault
```

Production configuration:

```text
[ ] RBAC/access model
[ ] private/public path decision
[ ] soft-delete/purge protection as required
[ ] rotation owner
[ ] expiry alerts
[ ] application reload behavior
[ ] audit logs
[ ] no secret value in logs/pipeline artifacts
```

Failure drill:

```text
rotate secret/certificate
→ old + new instances behavior
→ verify zero unexpected outage
```

Cost model includes operations, key/HSM/certificate choices and networking/features depending SKU/use.

---

# 13. Azure Firewall, VPN Gateway, ExpressRoute

## Azure Firewall

Use when centralized managed firewall/egress/inbound policy requirements justify it.

Cost can be material because firewall deployments have base capacity/processing dimensions. Do not insert it into small architectures only because enterprise diagrams show one.

Think:

```text
hub-spoke routing
forced egress
FQDN/network/application rules
threat intelligence
DNS
logging volume
SNAT
```

## VPN Gateway

Use for encrypted network connectivity:

```text
site-to-site
point-to-site
VNet-to-VNet scenarios
```

## ExpressRoute

Use when private dedicated connectivity between on-prem/connectivity provider and Microsoft cloud is justified by bandwidth/reliability/compliance.

Both hybrid choices require DNS/routing/failover design, not just tunnel/circuit creation.

---

# 14. Network/security cost worksheet

Track:

```text
DNS zones + queries
Private Endpoints × hours + data
NAT Gateway hours + data processed
Public IPs
Load Balancer rules/data where applicable
Application Gateway fixed + capacity units
Front Door requests/data transfer/WAF
APIM tier/units/requests
DDoS tier/protected footprint
Firewall base + processed data
VPN/ExpressRoute gateway/circuit
cross-region / internet egress
Log Analytics from network diagnostics
```

A common cost mistake is to compare only compute/database and ignore network/security fixed services.

---

# 15. Review checklist

- [ ] End-to-end traffic matrix exists.
- [ ] DNS public/private/hybrid design is explicit.
- [ ] VNet CIDR has growth headroom.
- [ ] NSG/route rules derive from required flows.
- [ ] Private Link used only with clear requirement and DNS owner.
- [ ] Outbound public IP/egress path is explicit.
- [ ] L4 vs regional L7 vs global edge vs API gateway roles are separated.
- [ ] Application Gateway uses v2-family architecture.
- [ ] Front Door design uses Standard/Premium current path.
- [ ] WAF and DDoS are treated as different controls.
- [ ] Human/deployer/runtime identities are separated.
- [ ] Managed Identity replaces static Azure credentials where possible.
- [ ] Key Vault stores only secrets that actually must exist.
- [ ] Every gateway/firewall has a cost justification.

## Verification metadata

- Verified: 2026-08-28.
- Azure DNS current service family checked against Microsoft Learn 2026 guidance.
- NAT Gateway Standard/StandardV2 checked against current resource documentation.
- Application Gateway v1 retirement date: 2026-04-28; use v2 guidance.
- Azure Front Door classic is on retirement path; use Standard/Premium for new design.
- APIM v2 tier capabilities/limits change over time; re-check before provisioning.
