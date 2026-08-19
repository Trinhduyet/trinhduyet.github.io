# Architecture Review Playbook — review một hệ thống thật

> [← Software Architecture](README.md)

<div class="lesson-meta">
  <span><strong>Use</strong>&nbsp;design review · existing system · migration</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
</div>

Architecture review không bắt đầu bằng sơ đồ component. Bắt đầu bằng **business flow + invariants + evidence production**.

## Review flow

```text
Business outcome
→ Critical flows
→ Data / source of truth
→ Invariants
→ Boundaries / ownership
→ Integration semantics
→ Failure model
→ Security / trust
→ Capacity / performance
→ Operations / DR
→ Cost
→ Evolution / debt
→ Decisions / experiments
```

## 1. Business context

Hỏi:

```text
Who uses it?
What is the critical business outcome?
What happens if it is wrong vs unavailable?
Which flow produces money/risk/compliance impact?
```

Example payment:

```text
Unavailable 5 min
→ checkout loss

Wrong double charge
→ financial correctness breach
```

Correctness and availability may need different priorities.

## 2. Critical flow map

Vẽ một flow cụ thể:

```text
Client
→ API
→ Order state
→ Payment provider
→ Message
→ Fulfillment
```

Đánh dấu:

```text
sync call
async message
DB commit
external side effect
trust boundary
```

## 3. Source of truth

Với mỗi important state:

| State | Owner | Source of truth | Projection/cache |
|---|---|---|---|
| Order | Orders | Orders DB | order dashboard |
| Payment | Payments/provider contract | Payment DB + provider reconciliation | UI projection |
| Inventory | Inventory | inventory store | availability cache |

Nếu câu trả lời là “Redis cũng có, DB cũng có, event cũng có” nhưng không ai biết authoritative source → risk lớn.

## 4. Invariants

Viết điều bắt buộc luôn đúng.

Examples:

```text
one business checkout → one logical order
captured amount <= authorized amount
sell quantity <= sellable quantity
one message replay must not double-create external side effect
```

Architecture review phải hỏi invariant được enforce ở đâu:

```text
DB constraint?
transaction?
aggregate?
idempotency store?
external provider key?
```

## 5. State machines

Status string list không đủ.

```text
PENDING
→ PROCESSING
→ SUCCEEDED
→ FAILED
→ UNKNOWN
```

Review:

- valid transitions;
- terminal states;
- retry states;
- unknown/reconciliation;
- concurrency race.

## 6. Boundary / ownership review

Cho mỗi module/service:

```text
capability
owned data
public contract
team owner
runtime/deploy boundary
on-call owner
```

Smells:

- shared write tables;
- giant shared package;
- service A knows internal enum of B;
- every release requires coordinated fleet deploy.

## 7. Integration review

Mỗi edge trên diagram phải ghi:

```text
command/query/event
sync/async
protocol
contract owner
timeout
retry
idempotency
ordering
source of truth
```

Một arrow “calls Payment” là thiếu thông tin architecture quan trọng.

## 8. Failure model

Dùng table:

| Failure | Local observation | Truth may be | Safe action |
|---|---|---|---|
| HTTP timeout to payment | no response | charged/not charged | UNKNOWN + reconcile |
| message redelivery | same message again | already applied | dedup/idempotent |
| cache stale | old value | DB newer | apply staleness policy |
| DB failover | transient errors | transaction commit maybe known/unknown by driver semantics | retry only where safe |
| queue backlog | delayed work | data durable but late | scale/admission/SLO handling |

## 9. Security review

Map trust boundary:

```text
Internet
→ edge
→ API identity
→ service identity
→ data
→ external provider
```

Questions:

- authentication source;
- authorization point;
- least privilege;
- secrets/managed identity;
- PII classification;
- encryption;
- audit;
- abuse/rate limit;
- tenant isolation.

## 10. Capacity review

At minimum:

```text
peak RPS
concurrency
payload size
read/write ratio
storage/day
retention
queue arrival/processing
external quota
DB connections
```

Ask:

```text
what breaks first at 2×?
what breaks at 10×?
```

Do not accept “autoscaling handles it” without downstream math.

## 11. Performance review

Evidence:

```text
P50/P95/P99
trace waterfall
slow query/execution plan
cache hit ratio
queue age
provider latency
CPU/memory/IO/connection saturation
```

Optimization should target measured bottleneck.

## 12. Observability review

Can operator answer in 10 minutes:

```text
what user flow is failing?
which dependency?
which deploy/version?
which tenant/region?
are retries amplifying load?
is backlog growing?
what business state is stuck?
```

If not, architecture lacks operational evidence.

## 13. Deployment review

- artifact immutability;
- environment promotion;
- database compatibility;
- canary/progressive rollout;
- rollback;
- feature flags;
- config/secret changes;
- migration steps.

Ask whether old/new versions coexist safely.

## 14. DR / recovery review

```text
RTO?
RPO?
backup?
restore test?
region/zone dependency?
message recovery?
external side-effect reconciliation?
```

“Geo-redundant backup enabled” không bằng “business recovery tested”.

## 15. Cost review

Break cost by driver:

```text
compute hours
DB capacity/storage/IO
cache
broker throughput
logs/traces retention
data transfer
egress
AI tokens
external provider fees
DR standby
```

Relate to unit:

```text
cost/order
cost/notification
cost/tenant/month
```

## 16. Architecture evolution review

Ask:

```text
current biggest coupling?
known temporary compromise?
revisit trigger?
extractable seams?
contract deprecation process?
technology lifecycle risk?
```

## 17. Review output

Không kết luận chung chung “looks good”.

Output:

```text
P0 correctness risks
P1 reliability/security risks
P2 maintainability/cost improvements

Accepted decisions
Conditional decisions
Required experiments
Owners
Due date
Revisit trigger
```

## 18. Example finding

### Finding

Payment timeout currently maps to `FAILED` and next retry uses new provider request ID.

### Severity

P0 correctness.

### Why

Provider may have charged before response was lost; retry with new ID can double charge.

### Required design

```text
stable business idempotency key
UNKNOWN state
provider status query/webhook
reconciliation
```

### Evidence required

Integration test:

```text
provider commits
→ response dropped
→ retry/reconcile
→ exactly one logical charge
```

Đây là architecture review actionable.

## 19. Review anti-patterns

### Logo review

```text
Kafka ✓
Redis ✓
Kubernetes ✓
```

không review semantics.

### Pattern policing

“Repository pattern missing” dù không có problem.

### Opinion without experiment

“Redis will be faster” không benchmark.

### Only happy path

Không timeout/duplicate/deploy/restore.

### Diagram without data ownership

Services đẹp nhưng shared DB writes.

## 20. Printable checklist

- [ ] business outcome / critical flow
- [ ] NFR / SLO / capacity
- [ ] source of truth
- [ ] invariants
- [ ] state machines
- [ ] ownership boundaries
- [ ] sync/async semantics
- [ ] timeout / retry / duplicate
- [ ] security/trust
- [ ] performance evidence
- [ ] observability
- [ ] deployment/rollback
- [ ] backup/restore/DR
- [ ] cost drivers
- [ ] architecture debt
- [ ] ADR / migration trigger

<div class="key-takeaway" markdown>
<strong>Architecture review tốt</strong>

Review phải tìm được **failure hoặc coupling cụ thể**, chỉ ra invariant/quality attribute bị đe dọa và yêu cầu một **evidence/test** để đóng finding. Không chỉ đưa opinions hoặc pattern preferences.
</div>
