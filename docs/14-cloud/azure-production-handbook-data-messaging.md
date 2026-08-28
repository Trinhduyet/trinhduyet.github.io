# Azure Production Handbook — Data, Storage & Messaging

> [← Compute](azure-production-handbook-compute.md) · [Azure overview](README.md) · [Network & Security →](azure-production-handbook-network-security.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;Azure SQL · Cosmos DB · Blob · Managed Redis · Service Bus · Event Grid · Event Hubs</span>
  <span><strong>Goal</strong>&nbsp;data semantics + capacity + recovery + cost</span>
</div>

Không chọn data/messaging service bằng logo. Bắt đầu bằng **source of truth, consistency, access pattern, throughput, retention, replay/recovery và cost model**.

---

# 1. Decision map

```text
Relational transaction / SQL query / strong schema
→ Azure SQL Database

Document/key-value, partitioned distributed workload
→ Cosmos DB candidate

Files / media / exports / backups / immutable objects
→ Blob Storage

Derived fast state / cache
→ Azure Managed Redis

Business command / durable work queue / topic
→ Service Bus

Event notification / reactive routing
→ Event Grid

High-throughput event/telemetry stream
→ Event Hubs
```

---

# 2. Azure SQL Database

## Trước khi chọn

Viết:

```text
entities
transaction boundary
query patterns
write/read ratio
P95 query latency
DB size + monthly growth
connection count
RTO/RPO
zone/region requirement
```

## Purchasing model

Azure SQL Database có hai purchasing models chính:

```text
vCore  ← recommended model for transparency/flexibility
DTU    ← bundled compute + memory + I/O simplicity
```

vCore có:

```text
Provisioned compute
Serverless compute
```

Current service-tier reasoning:

```text
General Purpose / standard business workload
Business Critical when low-latency local storage / high availability requirements justify
Hyperscale for scale/storage/read-scale characteristics that justify it
```

Official:

- <https://learn.microsoft.com/en-us/azure/azure-sql/database/purchasing-models?view=azuresql>
- <https://learn.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-overview?view=azuresql>

## Serverless vs provisioned

Serverless candidate:

```text
intermittent / unpredictable workload
long idle periods
auto-pause/resume trade-off acceptable
```

Provisioned candidate:

```text
steady utilization
predictable latency
always-warm requirement
```

Không chọn serverless chỉ vì tên “rẻ”. Auto-resume latency, minimum compute và features preventing auto-pause phải được test.

## Minimal production configuration

```text
[ ] Microsoft Entra authentication path
[ ] Managed Identity from workload
[ ] public network decision explicit
[ ] Private Endpoint + Private DNS if required
[ ] vCore/DTU and service tier documented
[ ] max compute/storage guardrail
[ ] backup retention
[ ] zone redundancy if justified/supported
[ ] geo restore/failover design if RTO/RPO requires
[ ] connection pooling/retry
[ ] indexes/query plan monitoring
[ ] alerts for CPU/data IO/log IO/storage/connections
```

## Backup/restore

Distinguish:

```text
PITR
Long-Term Retention
geo restore / failover architecture
```

Backup tồn tại không chứng minh restore usable. Runbook phải có restore test vào isolated environment.

## Cost drivers

vCore:

```text
service tier
+ hardware/compute vCores
+ provisioned/used compute model
+ data storage
+ backup storage
+ replicas/geo/zone choices
```

Cost traps:

- scale compute để chữa query/index problem;
- over-provision storage;
- serverless không auto-pause do workload/settings;
- read replica/geo setup không có business requirement;
- verbose auditing/telemetry không có retention policy.

---

# 3. Azure Cosmos DB

## Khi nào candidate

Strong signals:

```text
high-scale distributed document/key-value access
well-defined partition key
low-latency distributed access requirement
flexible schema/use case fits selected API
```

Weak signal:

```text
"NoSQL scale tốt hơn SQL"
```

## Capacity concept — Request Units

Cosmos capacity reasoning phải có RU.

```text
operation cost = RU
workload cost = operations × RU/operation
```

Provisioning options cần hiểu:

```text
Provisioned throughput
Autoscale throughput
Serverless
```

Official: <https://learn.microsoft.com/en-us/azure/cosmos-db/request-units>

## Partition key first

Trước service creation:

```text
partition key = ?
cardinality = ?
write distribution = ?
hot partition risk = ?
queries cross partition = ?
item growth = ?
```

Sai partition key là architecture debt khó sửa.

## Minimal production configuration

```text
[ ] partition key ADR
[ ] consistency level decision
[ ] throughput mode chosen
[ ] RU budget / autoscale max
[ ] indexing policy reviewed
[ ] TTL/retention if applicable
[ ] multi-region write/read decision
[ ] backup mode/recovery requirements
[ ] Managed Identity/RBAC where supported
[ ] private endpoint if required
[ ] 429/throttling telemetry + retry
```

## Cost drivers

```text
provisioned/autoscale RU capacity or serverless RU consumption
+ storage
+ additional regions
+ backup/analytics/features
+ network egress
```

Cost traps:

- cross-partition queries;
- hot partition;
- autoscale max oversized;
- multi-region copy-paste architecture;
- indexing fields never queried;
- using Cosmos for relational invariants that become complex at application layer.

---

# 4. Azure Blob Storage

## Use cases

```text
images/files
receipts/reports
exports/imports
backup/archive objects
large immutable payloads
static assets
```

## Access tiers

Current tiers:

| Tier | Mental model |
|---|---|
| Hot | higher storage cost, lower access cost |
| Cool | lower storage, higher access; infrequent access |
| Cold | lower storage, higher access; longer retention |
| Archive | lowest storage, offline/rehydration latency and retrieval cost |

Official: <https://learn.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview>

## Redundancy

Know these concepts:

```text
LRS   local redundancy
ZRS   zone redundancy
GRS   geo redundancy
GZRS  zone + geo redundancy
RA-*  read access to secondary variants
```

Redundancy choice is availability/durability/cost decision, not naming preference.

## Production configuration

```text
[ ] StorageV2 account unless special reason
[ ] public access disabled unless explicitly required
[ ] Managed Identity + data-plane role
[ ] container scope/ownership
[ ] Private Endpoint when needed
[ ] lifecycle management
[ ] soft delete/versioning based on data-loss model
[ ] redundancy choice
[ ] immutability/legal hold if compliance requires
[ ] diagnostic/audit plan
```

## Cost model

```text
capacity GB-month
+ read/write/list transactions
+ retrieval / early deletion depending tier
+ redundancy
+ replication
+ network egress
```

Cost trap:

```text
move everything to Archive
→ cheap storage
→ emergency restore requires rehydration time + retrieval cost
```

Tier must match recovery/access requirement.

---

# 5. Azure Managed Redis

Azure Managed Redis is the current Azure Redis direction for new workloads; do not start new architecture from old Azure Cache for Redis assumptions without checking migration/retirement guidance.

Current service families include in-memory profiles such as Memory Optimized, Balanced, Compute Optimized and Flash Optimized options for larger cost/performance trade-offs.

Official:

- <https://learn.microsoft.com/en-us/azure/redis/overview>
- <https://learn.microsoft.com/en-us/azure/redis/architecture>

## Correct role

Default mental model:

```text
Redis = derived/temporary fast state
!= relational source of truth by default
```

Common use:

```text
cache
rate counters
session/projection when design fits
short-lived coordination
```

## Production configuration

```text
[ ] data can be reconstructed or durability semantics documented
[ ] TTL policy
[ ] invalidation strategy
[ ] maxmemory/eviction behavior understood
[ ] HA mode decision
[ ] clustering/sharding capacity
[ ] Managed Identity/networking options checked
[ ] private connectivity if required
[ ] connection limits/client pooling
[ ] cache outage fallback bounded
[ ] hot-key detection
```

## Cost drivers

```text
selected tier/size
+ HA/geo options
+ memory/capacity
+ networking
```

Don't scale Redis before measuring:

```text
hit ratio
memory fragmentation
hot keys
ops/sec
latency
connection count
```

---

# 6. Service Bus

## Use case

```text
business commands
work queue
reliable async processing
pub/sub topics
enterprise messaging semantics
```

Not the same problem as Event Hubs streaming.

## Tiers

Know:

```text
Basic
Standard
Premium
```

Premium uses dedicated messaging capacity and is the tier to evaluate for stricter production performance/isolation/networking requirements.

Official limits: <https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quotas>

## Core concepts

```text
namespace
queue
topic
subscription
lock
peek-lock
complete/abandon/dead-letter
DLQ
max delivery count
TTL
scheduled message
session where ordered/session semantics require
duplicate detection where appropriate
```

## Consumer correctness

```text
at-least-once delivery
→ duplicate processing possible
→ consumer idempotency / inbox / business key
```

Never assume:

```text
message received once
```

## Production configuration

```text
[ ] tier documented
[ ] queue/topic ownership
[ ] lock duration vs handler time
[ ] max delivery count
[ ] DLQ alert/runbook
[ ] TTL
[ ] retry/backoff
[ ] duplicate/idempotency strategy
[ ] Managed Identity sender/receiver roles
[ ] private endpoint/network rules if required
[ ] backlog/oldest-message metrics
[ ] quota checked
```

## Cost drivers

```text
tier / messaging capacity
+ operations/messages
+ Premium Messaging Units where applicable
+ network/geo features
```

Capacity math:

```text
arrival rate
processing rate
backlog growth
recovery drain rate
```

Example:

```text
arrival = 100 msg/s
consumer = 70 msg/s
→ backlog +30 msg/s
```

Autoscale worker without checking database/provider capacity can move bottleneck downstream.

---

# 7. Event Grid

## Use case

Event notification/routing:

```text
Blob created
→ Event Grid
→ subscriber
```

Good when event means “something happened; route notification to interested handlers”.

Not default for durable work queue requiring Service Bus-like processing controls.

Know:

```text
topics / system topics
subscriptions
filters
delivery/retry
dead-letter destination
event schema
namespace capabilities where used
```

Production checklist:

```text
[ ] event contract/version
[ ] filters avoid unnecessary fan-out
[ ] subscriber idempotent
[ ] retry/dead-letter configured
[ ] authentication/identity
[ ] observability on delivery failures
[ ] cost by operations/event volume estimated
```

---

# 8. Event Hubs

## Use case

High-throughput append/event ingestion:

```text
telemetry
clickstream
IoT/event ingestion
log/event pipeline
stream processing
```

## Capacity concepts

Standard tier uses **Throughput Units (TU)**. Current guidance: one TU provides up to approximately:

```text
ingress: 1 MB/s or 1,000 events/s

egress: 2 MB/s or 4,096 events/s
```

Premium uses Processing Units and partition/capacity behavior differs.

Official: <https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-scalability>

## Partitions

Partition count affects parallelism and ordering scope.

```text
partition key
→ same partition
→ ordered append within partition context
```

Do not create huge partition count without consumer/capacity reason.

## Production configuration

```text
[ ] tier
[ ] partition count
[ ] partition key strategy
[ ] retention
[ ] consumer groups per independent application
[ ] TU/PU capacity
[ ] auto-inflate/max capacity if applicable
[ ] Capture if needed
[ ] Managed Identity
[ ] private network path if required
[ ] throttling/lag metrics
```

## Cost drivers

```text
TU/PU/CU capacity depending tier
+ retention/storage/capture
+ operations/features
+ network egress
```

Capacity worksheet:

```text
peak events/sec
average event size
peak MB/sec ingress
consumer count × egress
required partitions
retention volume
```

---

# 9. Service Bus vs Event Grid vs Event Hubs

| Question | Service Bus | Event Grid | Event Hubs |
|---|---|---|---|
| Durable business work queue? | Strong candidate | No | No |
| Pub/sub business messages? | Yes | notification style | stream style |
| Reactive event notification? | Possible but heavier | Strong candidate | No |
| High-throughput ordered stream? | Not primary use | No | Strong candidate |
| Consumer backlog/replay stream? | Queue semantics | event delivery | stream retention/offset semantics |

Do not choose based on word `event` alone.

---

# 10. Data & messaging review checklist

- [ ] Source of truth explicitly named.
- [ ] Transaction/consistency boundary documented.
- [ ] Azure SQL tier/purchasing model or Cosmos RU model specified.
- [ ] Blob access tier + redundancy + lifecycle specified.
- [ ] Redis fallback/staleness behavior specified.
- [ ] Queue/stream/event semantics chosen intentionally.
- [ ] Throughput/backlog math exists.
- [ ] Retry is bounded and idempotent.
- [ ] Backup exists **and restore has been tested**.
- [ ] Public/private path and Managed Identity configured.
- [ ] Capacity/quota alerts exist.
- [ ] Cost includes storage, operations, replicas, retention and egress.

## Verification metadata

- Verified: 2026-08-28.
- Azure SQL vCore/DTU/serverless checked against current Microsoft Learn.
- Blob Hot/Cool/Cold/Archive checked against current Storage docs.
- Azure Managed Redis uses current service direction, not legacy-only tier assumptions.
- Event Hubs TU capacity and Service Bus quota/tier claims must be re-checked before production sizing.
