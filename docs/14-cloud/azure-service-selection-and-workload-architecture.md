# Azure Service Selection & Workload Architecture

> [← Azure Overview](README.md) · [Core Platform](azure-core-platform-control-plane-and-governance.md) · [Production Handbooks](azure-production-handbook-compute.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;requirements → service choice → topology → trade-offs → evidence</span>
</div>

Azure có hàng trăm services. Mục tiêu không phải nhớ tên service, mà là **chọn đúng managed capability với đủ reliability/security/cost/operations evidence**.

## 1. Start from workload, not service catalog

```text
Business requirement
↓
NFR / SLO
↓
Data + traffic shape
↓
Security / compliance
↓
Operational model
↓
Azure capability
↓
SKU / topology
↓
Failure + cost review
```

Không:

```text
"team biết AKS"
→ mọi workload chạy AKS
```

## 2. Five Well-Architected lenses

Mọi decision phải review theo:

```text
Reliability
Security
Cost Optimization
Operational Excellence
Performance Efficiency
```

Một solution mạnh ở một pillar có thể làm pillar khác đắt/phức tạp hơn.

Ví dụ multi-region:

```text
reliability ↑
cost ↑
operations complexity ↑
data consistency complexity ↑
security surface ↑
```

## 3. Compute decision

### App Service

Phù hợp khi:

- web/API;
- managed PaaS desired;
- simple deployment slots/autoscale;
- không cần container orchestrator semantics.

### Container Apps

Phù hợp khi:

- containerized workloads;
- HTTP/event-driven scale;
- managed environment;
- microservices/container use case nhưng không muốn AKS operational burden.

### Functions

Phù hợp cho event/serverless execution khi trigger/runtime model phù hợp.

### VM / VMSS

Phù hợp khi cần OS-level control, legacy/runtime constraints, appliance/special workload.

### AKS

Chỉ chọn khi cần Kubernetes capabilities/standardization đủ để justify platform complexity.

Decision factors:

```text
runtime control
network requirements
scale pattern
startup latency
statefulness
team skill
operations burden
portability requirement
cost model
```

## 4. Container choice is not only Docker support

Hỏi:

```text
Need Kubernetes API/ecosystem?
Need custom controllers/operators?
Need daemonsets/node-level features?
Need complex scheduling?
Need service mesh/platform extensions?
```

Nếu không, Container Apps/App Service có thể đơn giản hơn.

## 5. Ingress / load-balancing decision

Các primitives có purpose khác nhau:

```text
Azure Front Door
= global HTTP(S) edge / acceleration / WAF

Application Gateway
= regional L7 proxy / WAF

Azure Load Balancer
= L4

Traffic Manager
= DNS-based global routing

API Management
= API gateway/management; not general-purpose LB
```

Một architecture có thể combine:

```text
Internet
→ Front Door
→ regional App Gateway
→ private workload
```

nhưng chỉ khi requirements justify.

## 6. Public vs private workload

Design question:

```text
Which component must be internet reachable?
Which dependencies can stay private?
```

Prefer:

```text
public ingress boundary
→ private application network
→ private PaaS dependencies
```

khi phù hợp.

Private-by-default tăng security nhưng cũng tăng:

- DNS complexity;
- build-agent connectivity requirements;
- troubleshooting complexity;
- cost.

## 7. Private Endpoint mental model

```text
PaaS resource
+ Private Endpoint NIC/IP in VNet
+ Private DNS resolution
+ routing/firewall
= usable private connectivity
```

Private Endpoint mà DNS sai = application outage.

Bạn phải test từ actual workload network.

## 8. Service Endpoint vs Private Endpoint

Service Endpoint:

```text
subnet identity/routing to Azure service
service still has public endpoint model
```

Private Endpoint:

```text
private IP in your VNet
Private Link
```

Không chọn theo “cái nào mới hơn”; chọn theo threat model/network requirement/service support.

## 9. Identity decision

Prefer:

```text
Managed Identity
→ Entra token
→ RBAC/data-plane permission
```

trước static secret khi service supports it.

Need distinguish:

```text
control-plane RBAC
data-plane RBAC
application authorization
```

Contributor trên resource group không đồng nghĩa đọc mọi data-plane secret/data.

## 10. Key Vault role

Key Vault phù hợp cho secrets/keys/certificates lifecycle.

Nhưng workload identity vẫn thường tốt hơn storing service credential khi Azure service supports Entra auth.

```text
identity first
secret only when credential is unavoidable
```

## 11. Relational data decision

Azure SQL Database thường là default candidate cho managed relational SQL workload.

Review:

- vCore/DTU/serverless model;
- HA/SLA;
- zone redundancy availability;
- backup/PITR;
- geo-replication/failover requirement;
- connection limits;
- private connectivity;
- cost.

Không chọn tier chỉ theo dev load.

## 12. Cosmos DB decision

Use when data/access pattern really fits distributed NoSQL model.

Must understand:

```text
partition key
RU/s
consistency
hot partition
indexing
multi-region write/read
cost
```

Cosmos DB không phải “scale SQL replacement”.

## 13. Storage decision

Distinguish:

```text
Blob
Files
Queues
Tables
Managed Disks
```

Với Blob, review:

- redundancy;
- access tier;
- lifecycle;
- versioning/soft delete;
- private access;
- immutable/WORM requirement;
- transaction/egress cost.

## 14. Cache

Azure Managed Redis / cache layer cần reason theo:

```text
source of truth
TTL
eviction
failover
memory pressure
network
serialization
cost
```

Cache outage không nên mặc định thành total application outage nếu architecture cho phép degrade.

## 15. Messaging choice

### Service Bus

Business messaging:

- queue/topic;
- durable command/event delivery;
- retries/dead-letter;
- ordering/session features where required.

### Event Grid

Event routing/fan-out/integration events.

### Event Hubs

High-throughput streaming/telemetry ingestion.

Decision:

```text
command/work queue?
event notification?
streaming log/telemetry?
ordering?
retention?
throughput?
consumer model?
```

## 16. API Management

Use for API product/gateway concerns:

- authentication policy integration;
- quotas/rate limits;
- transformation;
- versioning;
- developer/API management;
- observability.

Don't deploy APIM solely because “all APIs need a gateway”.

## 17. Integration services

Logic Apps phù hợp cho workflow/integration orchestration khi connector/workflow model phù hợp.

Functions phù hợp code-centric event processing.

Service Bus/Event Grid/Event Hubs solve messaging/event transport, not workflow ownership by themselves.

## 18. Observability architecture

```text
Application telemetry
→ Application Insights / OpenTelemetry
→ Azure Monitor / Log Analytics
→ alerting/workbooks/dashboards
```

Design:

- sampling;
- retention;
- PII handling;
- cardinality;
- workspace topology;
- alert ownership;
- cost budget.

Logs không miễn phí và unlimited retention không phải default tốt.

## 19. Azure Monitor vs application SLO

Azure resource health != business health.

Need both:

```text
platform metrics
+ application metrics
+ dependency telemetry
+ business indicators
```

Ví dụ App Service “healthy” nhưng checkout failure 20%.

## 20. Reliability topology

Distinguish:

```text
instance redundancy
availability-zone redundancy
regional redundancy
backup
disaster recovery
```

Chúng giải quyết failure classes khác nhau.

## 21. Availability Zones

Zones bảo vệ một số datacenter/zone failure trong region khi service/SKU supports.

Không suy luận:

```text
zone redundant
= region disaster recovery solved
```

## 22. Multi-region

Trước khi multi-region, define:

```text
RTO
RPO
traffic failover
data replication
write ownership
DNS/edge behavior
secret/config replication
deployment sequence
operational ownership
failback
```

Nếu không test failover, architecture diagram không phải DR evidence.

## 23. Backup vs replica

Replica phục vụ availability.

Backup phục vụ recovery/history/corruption/ransomware scenarios.

```text
replication
!= backup
```

## 24. Quotas and capacity

Cloud managed service vẫn có limits.

Review before production:

- subscription quota;
- regional capacity;
- service scale unit;
- connection/throughput limits;
- IP/subnet consumption;
- private endpoint limits;
- API quotas.

Capacity incident có thể xảy ra dù code không đổi.

## 25. Azure Policy and governance

Use governance hierarchy:

```text
Management Group
→ Subscription
→ Resource Group
→ Resource
```

Apply policy at correct scope.

Policy can audit/deny/modify/deploy supporting config depending definition/effect.

Avoid hundreds of unowned exemptions.

## 26. Resource locks

Locks reduce accidental delete/change at ARM control plane.

They do not replace:

- RBAC;
- Policy;
- backup;
- application authorization.

Locks can also block IaC/operations if used without runbook.

## 27. Tags

Tags support cost/ownership/governance metadata.

Typical:

```text
environment
owner
cost_center
application
data_classification
managed_by
```

Tag policy must distinguish inherited/platform-managed tags from workload-owned values.

## 28. Defender / security posture

Defender for Cloud and security benchmark initiatives help posture/governance, but application security still requires:

- AuthN/AuthZ;
- secure coding;
- secrets/identity;
- network design;
- dependency/supply chain controls;
- logging/incident response.

## 29. Cost architecture

Major hidden drivers:

```text
data egress
logs ingestion/retention
premium networking
private endpoints
firewalls/NAT
zone/multi-region replicas
provisioned DB throughput
idle compute
backup retention
```

Cost review belongs in architecture, not after invoice.

## 30. Reference production flow

```text
Internet
→ Front Door/WAF when needed
→ regional ingress
→ App Service / Container Apps / AKS
→ Managed Identity
→ private Azure SQL / Storage / Redis
→ Service Bus
→ Azure Monitor / App Insights
```

Surrounding platform:

```text
Management Groups
Subscriptions
Azure Policy
RBAC
Key Vault
Defender
Cost Management
Terraform/Bicep
CI/CD
```

## 31. Decision record template

For every major Azure service choice:

```text
Requirement
Alternatives considered
Chosen service/SKU
Network model
Identity model
Reliability model
Backup/DR
Observability
Cost drivers
Known limits
Failure modes
IaC ownership
Exit/migration plan
```

## 32. Exit criteria

- [ ] choose compute by requirements, not familiarity;
- [ ] distinguish Front Door/App Gateway/LB/Traffic Manager/APIM;
- [ ] design public/private boundaries and Private Endpoint DNS;
- [ ] prefer managed identity when supported;
- [ ] choose relational/NoSQL/storage/cache intentionally;
- [ ] distinguish Service Bus/Event Grid/Event Hubs;
- [ ] design observability + cost;
- [ ] distinguish zones/multi-region/backup/DR;
- [ ] review quota/capacity;
- [ ] apply WAF five-pillar review to service choices;
- [ ] produce ADR with IaC/recovery/exit plan.

## Official sources

- Azure technology choices: https://learn.microsoft.com/azure/architecture/guide/technology-choices/technology-choices-overview
- Compute decision: https://learn.microsoft.com/azure/architecture/guide/technology-choices/compute-decision-tree
- Load balancing: https://learn.microsoft.com/azure/architecture/guide/technology-choices/load-balancing-overview
- Well-Architected Framework: https://learn.microsoft.com/azure/well-architected/
- Azure Architecture Center: https://learn.microsoft.com/azure/architecture/

## Verification metadata

- Verified: 2026-09-25.
- Exact SKU, regional availability, limits and pricing must be rechecked before deployment.
