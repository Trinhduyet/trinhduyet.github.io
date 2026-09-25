# Azure Governance, Security & Operations Playbook

> [← Service Selection](azure-service-selection-and-workload-architecture.md) · [Core Platform](azure-core-platform-control-plane-and-governance.md) · [Operations/Cost Handbook](azure-production-handbook-operations-cost.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;landing zone · RBAC · Policy · identity · network · observability · incident · FinOps</span>
</div>

Một Azure workload production không chỉ là collection resources. Nó sống trong **governed platform** với ownership, identity, policy, network, logs, cost, backup và incident path.

## 1. Platform vs workload

```text
Platform team
├─ management groups
├─ subscriptions
├─ connectivity
├─ identity baseline
├─ policy
├─ central logging/security
└─ shared DNS

Workload team
├─ application resources
├─ workload network
├─ workload identities
├─ data services
├─ alerts/SLO
└─ workload IaC
```

Boundary phải explicit để tránh two controllers/teams cùng sở hữu resource.

## 2. Landing zone mental model

Landing zone không phải một VNet template.

Nó là environment foundation:

```text
resource organization
identity/access
network topology
security baseline
governance
management/monitoring
automation/IaC
subscription vending
```

Application landing zone = nơi workload được deploy dưới platform guardrails.

## 3. Management groups

Dùng management group để apply governance across subscriptions.

Không mirror org chart một cách máy móc.

Design theo policy/governance needs.

```text
Tenant Root
├─ Platform
├─ Landing Zones
│  ├─ Corp
│  └─ Online
├─ Sandbox
└─ Decommissioned
```

Topology thực tế phụ thuộc enterprise.

## 4. Subscription as boundary

Subscription là boundary quan trọng cho:

- billing/cost;
- quotas;
- RBAC;
- policy;
- blast radius;
- lifecycle.

Một workload critical có thể tách prod/nonprod subscription.

Không dùng resource group như replacement cho mọi isolation need.

## 5. Resource group

RG là lifecycle/management scope hữu ích.

Group resources có lifecycle/ownership gần nhau.

Không group chỉ theo resource type:

```text
rg-all-storage
rg-all-networks
```

nếu điều đó phá workload ownership.

## 6. RBAC model

```text
principal
+ role definition
+ scope
= role assignment
```

Scope inheritance:

```text
management group
→ subscription
→ resource group
→ resource
```

Least privilege cần cả role và scope nhỏ nhất hợp lý.

## 7. Built-in vs custom roles

Start built-in roles.

Custom role khi:

- built-in quá rộng;
- repeatable permission set rõ;
- ownership/governance exists.

Tránh custom role explosion.

## 8. Privileged access

Permanent broad access là risk.

Production nên reason về:

- PIM/JIT;
- approval;
- break-glass;
- audit;
- separation of duties.

IaC deployment identity không cần quyền user administration trừ khi task thật sự yêu cầu.

## 9. Managed Identity

System-assigned:

- lifecycle tied to resource.

User-assigned:

- reusable identity object;
- independent lifecycle.

Choose based on ownership/lifecycle, not “user-assigned always better”.

## 10. Control plane vs data plane

Ví dụ Storage:

```text
ARM control plane
→ create/configure storage account

Storage data plane
→ read/write blob
```

Permissions khác nhau.

Terraform thường làm control-plane operations; application thường cần data-plane role.

## 11. Azure Policy

Policy answers:

```text
Is this resource configuration allowed/compliant?
```

RBAC answers:

```text
Who may perform action?
```

Policy effects có thể audit/deny/modify/deploy/remediate depending definition.

## 12. Initiatives

Initiative groups policies into governed baseline.

Examples:

- tagging baseline;
- allowed regions;
- diagnostics requirements;
- private networking;
- security benchmark controls.

Need owner for remediation and exemptions.

## 13. Policy exemptions

Exemption cần:

```text
business reason
owner
scope
expiration/review date
compensating control
```

Permanent unexplained exemption = policy debt.

## 14. Locks

Use delete/read-only locks carefully on critical resources.

Understand impact on:

- Terraform;
- updates;
- incident operations;
- child resources.

Document unlock/relock runbook.

## 15. Hub-spoke vs Virtual WAN

Hub-spoke:

- centralized connectivity/security;
- shared firewall/DNS;
- spoke workload isolation.

Virtual WAN may simplify large-scale managed hub/connectivity scenarios.

Decision depends on:

- number of networks/regions;
- branch connectivity;
- routing complexity;
- operational ownership;
- cost.

## 16. DNS as platform dependency

Private cloud networking fails frequently because DNS ownership is ignored.

Need explicit:

```text
public DNS
private DNS zones
VNet links
on-prem conditional forwarding
Private Resolver
record lifecycle
```

Treat DNS change like production change.

## 17. Egress architecture

Ingress gets attention, egress often does not.

Define:

- allowed destinations;
- stable outbound IP requirement;
- NAT Gateway;
- firewall/proxy;
- TLS inspection constraints;
- package registry access;
- telemetry endpoints.

Uncontrolled egress increases exfiltration risk.

## 18. Private build/deploy connectivity

If PaaS dependencies disable public access, CI/CD runner may need line-of-sight.

Options:

- self-hosted runner in private network;
- private deployment agent;
- controlled public management endpoint when service allows;
- separate control-plane/data-plane strategy.

Private networking affects delivery architecture.

## 19. Key Vault operations

Design:

- RBAC/access model;
- private endpoint/DNS;
- soft delete/purge protection where applicable;
- rotation;
- alert/audit;
- certificate renewal ownership.

Do not log secret value during Terraform/debug.

## 20. Diagnostic settings

Not every log category must go everywhere forever.

Design:

```text
signal needed
→ destination
→ retention
→ owner
→ alert/use case
→ cost
```

“Enable all diagnostics” can create massive cost with low value.

## 21. Log Analytics topology

Questions:

- central workspace or workload workspace?
- data residency?
- cross-resource query?
- RBAC boundary?
- retention?
- Sentinel integration?
- chargeback?

There is no universal one-workspace answer.

## 22. Application Insights

Use for application telemetry, dependency traces, request performance and failures.

Prefer OpenTelemetry-compatible instrumentation where appropriate.

Need sampling/cardinality rules.

## 23. Alerts

Good alert:

```text
actionable
owned
severity defined
runbook linked
low noise
business/service impact related
```

Bad alert:

```text
CPU > 50% once
→ wake someone
```

## 24. Service Health vs Resource Health vs App Health

Distinguish:

```text
Azure Service Health
= Azure platform incidents/advisories

Resource Health
= resource availability signal

Application health
= your workload behavior/SLO
```

Need all relevant layers.

## 25. Defender for Cloud

Use posture/recommendation/protection capabilities based on workload risk.

But don't treat score as security proof.

Review actual controls, exceptions and findings.

## 26. Microsoft Sentinel

SIEM/SOAR use case for security analytics/incident workflow.

Not every small workload needs its own Sentinel workspace/design; follow organization SOC model.

## 27. Backup architecture

Define per data source:

```text
what is backed up?
frequency?
retention?
immutability?
cross-region/cross-subscription?
restore procedure?
restore test?
RPO/RTO?
```

Backup success without restore test is weak evidence.

## 28. Disaster recovery

DR plan needs:

```text
failure scenario
traffic failover
data recovery/replication
secrets/config
identity
DNS
capacity
deployment
validation
failback
human roles
```

Terraform can recreate infrastructure but does not recreate lost business data automatically.

## 29. Incident access

During incident, teams often bypass controls.

Design break-glass safely:

- limited identities;
- strong authentication;
- monitoring;
- documented use criteria;
- post-incident review;
- revoke/rotate if used.

## 30. Quota management

Track critical quotas before launch.

Examples:

- vCPU;
- public IP;
- private endpoints;
- networking limits;
- service throughput;
- database capacity.

Capacity increase can require lead time.

## 31. Cost allocation

Require tags/subscription hierarchy sufficient for:

- owner;
- app;
- environment;
- cost center.

Then monitor:

- budget;
- anomalies;
- trend;
- unit economics;
- idle resources.

## 32. FinOps loop

```text
measure
→ allocate
→ optimize
→ forecast
→ govern
→ verify business value
```

Don't optimize only compute; logs/network/data services often dominate.

## 33. IaC ownership map

Example:

```text
Platform Terraform
→ management group policy
→ shared hub
→ central DNS
→ monitoring baseline

Workload Terraform
→ RG
→ spoke/VNet integration
→ App Service/Container Apps
→ SQL/Storage
→ workload identities
→ alerts
```

Define outputs/contracts between stacks.

## 34. Change management

High-risk Azure change examples:

- route table;
- DNS;
- firewall;
- RBAC;
- policy;
- private endpoint;
- region failover;
- database tier/replication;
- state backend.

Require plan + validation + rollback/recovery note.

## 35. Failure drills

### A — Private Endpoint DNS broken

Break zone link in nonprod; debug name resolution from workload.

### B — RBAC denied

Remove required data-plane role; observe 403 and distinguish identity from network failure.

### C — Policy deny

Attempt non-compliant resource in sandbox; understand policy assignment/effect.

### D — quota exhausted

Use tabletop/safe simulation; define request/escalation path.

### E — restore test

Restore backup to isolated target and validate application-level data.

## 36. Production readiness checklist

Before go-live:

```text
subscription/ownership clear
RBAC least privilege
policy compliant/exemptions reviewed
network + DNS tested
managed identities configured
public access reviewed
backup/restore tested
DR/RTO/RPO documented
alerts/runbooks owned
quota/capacity checked
cost budget/alerts configured
Terraform state/backend/recovery documented
security findings triaged
```

## 37. Exit criteria

- [ ] separate platform/workload responsibilities;
- [ ] design management group/subscription/RG boundaries;
- [ ] reason about RBAC scope and privileged access;
- [ ] distinguish control/data plane roles;
- [ ] use Policy/initiative/exemption correctly;
- [ ] design hub/spoke, egress and DNS ownership;
- [ ] operate private endpoint + private CI connectivity;
- [ ] design Azure Monitor/App Insights/log retention;
- [ ] distinguish platform/resource/application health;
- [ ] design backup + restore + DR drills;
- [ ] manage quotas and FinOps;
- [ ] map Terraform ownership to Azure platform/workload boundaries.

## Official sources

- Azure landing zones: https://learn.microsoft.com/azure/cloud-adoption-framework/ready/landing-zone/
- Azure Policy: https://learn.microsoft.com/azure/governance/policy/overview
- Azure RBAC: https://learn.microsoft.com/azure/role-based-access-control/overview
- Managed identities: https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview
- Azure Well-Architected Framework: https://learn.microsoft.com/azure/well-architected/
- Azure Monitor: https://learn.microsoft.com/azure/azure-monitor/fundamentals/overview
- Defender for Cloud: https://learn.microsoft.com/azure/defender-for-cloud/

## Verification metadata

- Verified: 2026-09-25.
- Organization-specific landing-zone topology and security operations must follow the organization's platform/SOC model.
