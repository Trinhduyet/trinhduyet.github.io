# Azure Production Handbook — Observability, Delivery, Backup, DR & Cost

> [← Network & Security](azure-production-handbook-network-security.md) · [Azure overview](README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;Monitor · App Insights · Log Analytics · CI/CD · ACR · IaC · Backup · DR · FinOps</span>
  <span><strong>Goal</strong>&nbsp;operate and pay for the system intentionally</span>
</div>

Production không kết thúc ở `deploy succeeded`. Bạn phải biết **system có đang đáp ứng SLO không, restore được không, ai được deploy, artifact nào đang chạy và bill tăng vì đâu**.

---

# 1. Observability model

Start từ user outcome:

```text
Business SLI
→ checkout success
→ payment unknown rate
→ order completion latency

Service SLI
→ HTTP P95/P99
→ dependency latency
→ error rate
→ queue oldest-message age

Resource signal
→ CPU
→ memory
→ connections
→ storage/IO
→ throttling
```

CPU 30% không chứng minh user flow khỏe.

---

# 2. Azure Monitor

Azure Monitor là umbrella platform cho metrics/logs/alerts/traces integrations.

Core pieces cần biết:

```text
Platform metrics
Activity Log
Diagnostic Settings
Log Analytics Workspace
Application Insights
Alerts / Action Groups
Workbooks / dashboards
Managed Prometheus / Container Insights where relevant
```

Official: <https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/overview>

## Diagnostic Settings

Không bật mọi diagnostic category rồi giữ vô hạn.

Design:

```text
which resources?
which log categories?
where destination?
retention?
security/privacy?
operational query?
cost owner?
```

---

# 3. Application Insights

Application Insights phù hợp cho application telemetry/APM, especially .NET/OpenTelemetry integration.

Instrument:

```text
requests
exceptions
dependencies
traces
custom business metrics/events where justified
correlation context
```

Do not log:

```text
access tokens
passwords
full card/payment data
sensitive PII without requirement and control
```

## Distributed tracing

```text
HTTP request
→ API
→ SQL
→ Service Bus message
→ Worker
→ external provider
```

Correlation phải survive async boundary. Store business IDs safely, but avoid high-cardinality metric labels.

---

# 4. Log Analytics cost is architecture

For many environments, **log ingestion is the largest Azure Monitor charge**.

Official:

- <https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/cost-usage>
- <https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/best-practices-cost>
- <https://learn.microsoft.com/en-us/azure/azure-monitor/logs/data-retention-configure>

Cost dimensions:

```text
ingested GB
retention
long-term retention / archive
queries for some table plans
export
alerts/tests/other monitoring features
```

Current Log Analytics has table-plan/retention choices; do not assume every log needs Analytics-tier interactive retention for months.

## Cost controls

```text
sampling
Data Collection Rules
filter noisy telemetry before ingestion
Basic/Auxiliary table plan where appropriate
per-table retention
long-term retention for audit
commitment tier when measured volume justifies
alert on ingestion spike
daily cap only as a safety mechanism with understood visibility trade-off
```

Bad pattern:

```text
Debug log every request body
× 100 services
× 90 days
→ observability bill + privacy risk
```

## Cost worksheet

```text
logs_gb_day
× 30
→ monthly ingestion volume

then add:
retention beyond included period
archive/search/export
other telemetry features
```

Measure a representative environment before extrapolating.

---

# 5. Alerting & incident response

Alert on outcome/risk, not every metric fluctuation.

Good alerts:

```text
SLO burn / error rate
P99 latency sustained
queue oldest-message age
DLQ growth
SQL storage near limit
payment_unknown growth
certificate expiry
budget anomaly
backup/restore failure
```

Every alert needs:

```text
owner
severity
runbook
expected action
anti-noise threshold
```

An alert with no owner is only a log with notification noise.

---

# 6. CI/CD identity — no long-lived Azure secret

Preferred GitHub Actions mental model:

```text
GitHub Actions
→ OIDC federation
→ Microsoft Entra workload identity
→ Azure scoped deployment permissions
```

Avoid storing a long-lived Azure client secret in repository secrets when federated identity can be used.

Separate identities:

```text
CI deploy identity
!= application runtime identity
```

Runtime should use Managed Identity appropriate to workload.

---

# 7. Delivery pipeline

Reference flow:

```text
commit
→ restore/build
→ unit tests
→ integration/security/static checks
→ immutable package/container
→ publish artifact/ACR
→ IaC validate/plan
→ deploy non-prod
→ smoke/contract
→ production progressive rollout
→ SLO/error gate
→ rollback/forward fix
```

Deployment evidence:

```text
Git SHA
artifact version / image digest
IaC commit
migration version
environment
release time
approver/pipeline run
```

---

# 8. Azure Container Registry (ACR)

ACR stores private OCI/container artifacts.

Current SKUs:

```text
Basic
Standard
Premium
```

Official: <https://learn.microsoft.com/en-us/azure/container-registry/container-registry-skus>

Mental model:

```text
Basic      → dev/lower volume
Standard   → many normal production scenarios
Premium    → higher throughput + advanced features such as Private Link and geo-replication scenarios
```

## Production configuration

```text
[ ] SKU based on throughput/storage/network requirements
[ ] Entra/Managed Identity authentication
[ ] admin user disabled unless explicit exception
[ ] repository permissions scoped
[ ] immutable release strategy/image digest
[ ] retention/cleanup policy
[ ] vulnerability/supply-chain scanning strategy
[ ] Private Link if networking requires Premium capability
[ ] geo-replication only when recovery/latency justifies
```

## Cost drivers

```text
registry SKU running time
+ storage beyond included amount
+ build/task/scanning features where used
+ geo-replication/network transfer
```

Images with unbounded tags/layers become storage and supply-chain debt.

---

# 9. Terraform / Bicep

Azure infrastructure must be reproducible.

## Bicep

Good when:

```text
Azure-focused estate
native ARM resource coverage
team wants Azure-native declarative IaC
```

## Terraform

Good when:

```text
multi-provider/multi-cloud tooling
existing Terraform modules/workflow
team already operates remote state/locking/provider upgrades
```

The correct choice is less important than operating it safely.

Production requirements:

```text
remote state
state access least privilege
locking/concurrency model
module/version pinning
plan review
policy checks
no secrets in state where avoidable
import/migration strategy
manual drift detection
```

Never treat Portal clicks as source of truth after IaC adoption.

---

# 10. Backup != HA != DR

```text
High Availability
→ survive component/failure-domain issues with little interruption

Backup
→ recover historical data/state

Disaster Recovery
→ restore/fail over service after larger disaster
```

You may need all three.

---

# 11. Azure SQL recovery

Know:

```text
PITR
Long-Term Retention
geo restore
failover architecture / groups when selected
```

Test:

```text
restore to isolated server/database
→ validate schema
→ validate business invariants
→ run critical queries
→ record actual restore time
```

Backup retention is not actual RTO.

---

# 12. Blob/Storage recovery

Protection layers can include:

```text
LRS/ZRS/GRS/GZRS redundancy
soft delete
versioning
immutability where required
lifecycle
backup depending workload
```

Redundancy protects certain infrastructure failures; it does not automatically protect logical deletion/corruption in every scenario.

Example:

```text
app deletes wrong object
→ replication can replicate deletion
```

Versioning/soft delete/backup policy solve a different problem.

---

# 13. Azure Backup

Azure Backup is a managed backup service for supported Azure/on-prem workloads.

Cost model generally must account for:

```text
protected instances/workloads
+ backup storage consumed
+ storage redundancy/retention
```

Do not assume stopping backup immediately removes stored backup cost if retained recovery points remain.

Production checklist:

```text
[ ] vault ownership
[ ] backup policy
[ ] retention
[ ] soft delete/security controls
[ ] cross-region/zone requirements
[ ] alert on failures
[ ] restore permissions separate
[ ] restore test schedule
```

Official entry: <https://learn.microsoft.com/en-us/azure/backup/>

---

# 14. Azure Site Recovery

Site Recovery is primarily a BCDR/replication tool for VM/physical workload scenarios, not a generic PaaS database DR button.

Official: <https://learn.microsoft.com/en-us/azure/site-recovery/site-recovery-overview>

Use when VM workload replication/failover orchestration fits the architecture.

Current cost reasoning includes protected VM instances plus dependent storage/network resources.

Recovery Plan can orchestrate machine groups/order/tasks for failover/failback.

Runbook:

```text
detect incident
→ declare disaster
→ choose failover point
→ execute recovery plan
→ validate dependencies/data
→ open traffic
→ monitor/reconcile
→ failback later
```

---

# 15. Multi-region is not the default

```text
Region A active
Region B standby
```

adds:

```text
compute standby
replication
network/egress
monitoring
secret/config sync
failover testing
operational complexity
```

Active-active adds even more data consistency/routing complexity.

Only use when business SLO/RTO/RPO justify.

---

# 16. Cost Management / Budgets / tags

FinOps baseline:

```text
subscription / resource-group ownership
mandatory tags
budget alerts
cost by environment
cost by workload/team
monthly anomaly review
Azure Advisor recommendations
reservation/savings review after stable usage
```

Useful tags:

```text
environment
workload
owner
cost-center
criticality
data-classification
```

Tags are not security controls; they enable ownership/cost/governance workflows.

---

# 17. How to estimate Azure cost

Do not store a timeless price table in architecture docs.

Store:

```text
Region: <region>
Pricing date: YYYY-MM-DD
Currency/agreement: <context>
Traffic: <RPS/events>
Compute min/avg/max: ...
DB compute/storage: ...
Blob GB + transactions: ...
Logs GB/day + retention: ...
Network ingress/egress: ...
Backup retention: ...
DR capacity: ...
```

Then use the current Azure Pricing Calculator / Cost Management data.

Architecture cost formula:

```text
TOTAL
=
compute
+ database/cache
+ storage/backup
+ messaging
+ edge/API/network/security
+ observability
+ DR
+ egress
```

## Unit economics

Derive:

```text
cost / 1,000 requests
cost / order
cost / tenant
cost / GB retained
cost / event million
```

This allows architecture review at 10x scale.

---

# 18. Common cost traps

## Logs

```text
verbose logs × long retention
→ Azure Monitor becomes top bill
```

## Private networking everywhere

```text
Private Endpoint / NAT / Firewall / DNS Resolver
```

are valuable when required, but can add fixed/data processing costs and operational surface.

## Premium everywhere

Premium tier is not synonymous with production. Pick the feature/capacity/SLA actually required.

## Idle DR

Warm/active secondary region can be expensive even without user traffic.

## Egress

```text
cross-region chatty services
large internet responses
replication
```

can produce unexpected network bill.

## Autoscale without dependency guardrails

More replicas can create:

```text
more DB connections
more messages
more external API calls
more logs
more cost
```

---

# 19. Monthly review template

| Area | Question |
|---|---|
| Compute | avg vs peak utilization? idle capacity? |
| DB | compute/storage/query bottleneck? |
| Cache | hit ratio justify cost? |
| Messaging | backlog/throughput tier appropriate? |
| Network | NAT/Firewall/Front Door/APIM fixed + data cost? |
| Logs | GB/day changed? noisy table/service? |
| Backup | retained recovery points still required? |
| DR | standby level matches RTO? |
| Reservations | workload stable enough to commit? |
| Ownership | orphan resources/tags? |

---

# 20. Production evidence

A real Azure project should contain:

```text
architecture diagram
service/SKU ADRs
Terraform/Bicep
CI/CD with federated identity
immutable artifacts
permission matrix
SLO dashboard
alerts + runbooks
backup policy
restore evidence
DR/failure drill report
cost worksheet
monthly FinOps review
```

<div class="key-takeaway" markdown>
<strong>Azure architecture hoàn chỉnh = build + operate + recover + pay.</strong>

Nếu bạn chỉ chứng minh deploy được nhưng chưa biết restore, observe và explain bill, hệ thống chưa production-ready.
</div>

## Verification metadata

- Verified: 2026-08-28.
- Azure Monitor ingestion/retention and cost guidance checked against current Microsoft Learn.
- ACR Basic/Standard/Premium checked against current SKU documentation.
- Site Recovery and Azure Backup usage must be verified for exact workload support before production design.
- Live prices intentionally not hard-coded; use current Pricing Calculator and agreement-specific prices.
