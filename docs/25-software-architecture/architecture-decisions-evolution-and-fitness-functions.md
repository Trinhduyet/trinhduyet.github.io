# Architecture Decisions, Fitness Functions & Evolution

> [← Software Architecture](README.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;ADR · governance · migration</span>
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Goal</strong>&nbsp;architecture that can evolve safely</span>
</div>

## 1. Architecture decision phải có context

Không ghi:

```text
Decision: use Kafka.
```

Ghi:

```text
Context:
notification fanout must not block checkout;
peak burst 50k/min;
consumers have independent retry/ownership.

Options:
A. synchronous HTTP fanout
B. durable broker

Decision:
B, because temporal decoupling + durable backlog are required.

Consequences:
duplicate handling, DLQ, schema evolution, backlog operations.

Revisit trigger:
if event volume/retention/replay requirements exceed current broker fit.
```

## 2. ADR — Architecture Decision Record

Một ADR ngắn thường đủ:

```text
Title
Status
Context
Decision
Options considered
Consequences
Evidence
Revisit/Migration trigger
```

### Status

```text
Proposed
Accepted
Superseded
Deprecated
```

Không sửa lịch sử quyết định như wiki current-state. Nếu decision đổi, tạo ADR mới supersede ADR cũ.

## 3. Decision quality

Decision tốt trả lời:

```text
What problem?
What constraints?
Why now?
Why this option?
What do we lose?
How do we prove?
When do we revisit?
```

Tên công nghệ không phải justification.

## 4. Architecture governance

Governance không nên chỉ là architecture board họp mỗi tháng.

Có nhiều lớp:

```text
principles
reference patterns
ADRs
code/module rules
CI checks
security policies
platform guardrails
observability standards
review workflow
```

Mục tiêu là **enable safe autonomy**, không centralize mọi decision.

## 5. Guardrails vs Golden Paths

### Guardrail

Boundary không được vượt:

```text
no public database in prod
no direct cross-module data writes
no secrets in repository
```

### Golden Path

Supported easy path:

```text
.NET service template
+ CI
+ logging/tracing
+ health endpoints
+ IaC baseline
+ deployment pattern
```

Golden path giảm cognitive load. Guardrail giảm risk.

## 6. Fitness Functions

Fitness function là automated evidence architecture còn giữ property mong muốn.

### Dependency fitness

```text
Orders.Domain cannot reference Infrastructure
Payments cannot reference Orders.Infrastructure
```

### Performance fitness

```text
P95 checkout intake < 500 ms under target load
```

### Reliability fitness

```text
duplicate message test must not create duplicate payment
```

### Security fitness

```text
no public storage/database endpoint in production IaC
```

### Operability fitness

```text
all critical services emit trace + health + required SLI metrics
```

Architecture test không chỉ là class dependency test.

## 7. Example architecture test in .NET

Conceptually:

```csharp
[Fact]
public void OrdersDomain_ShouldNotDependOnInfrastructure()
{
    var references = typeof(Orders.Domain.Order).Assembly
        .GetReferencedAssemblies()
        .Select(x => x.Name)
        .ToArray();

    Assert.DoesNotContain("Orders.Infrastructure", references);
}
```

Production code có thể dùng dedicated architecture-test tooling, nhưng point là **rule executable**.

## 8. Evolutionary Architecture

Architecture không “done”. Workload/team/business thay đổi.

Healthy evolution:

```text
baseline
→ observe pressure
→ make seam explicit
→ migrate incrementally
→ compare evidence
→ retire old path
```

Không rewrite big-bang khi có incremental path.

## 9. Strangler Pattern

Legacy endpoint:

```text
/api/customers/*
```

Migration:

```text
Gateway/Router
├─ old capabilities → Legacy
└─ migrated capability → New Service/Module
```

Over time:

```text
new path grows
legacy shrinks
```

Need data/transaction ownership migration, not just route split.

## 10. Branch by Abstraction

Thay dependency behind an abstraction gradually:

```text
Application
→ ICustomerStore
   ├─ OldDbAdapter
   └─ NewDbAdapter
```

Can dual-read/compare or switch by feature flag carefully.

Useful for library/provider/storage migration.

## 11. Expand / Contract database migration

Rolling deploy means old/new app coexist.

### Expand

Add new schema without breaking old app.

```text
add NewColumn nullable
new app writes both/compatible form
```

### Migrate

Backfill/read transition.

### Contract

Only remove old field after old readers/writers gone.

This is architecture evolution at data contract level.

## 12. Dual write danger

Migration sometimes suggests:

```text
write OldDB
write NewDB
```

without atomicity.

Failure:

```text
OldDB write success
NewDB timeout
→ divergence
```

Need strategy:

- outbox/CDC replication;
- one source of truth + async projection;
- reconciliation;
- explicit cutover.

## 13. Shadow Traffic

Send copy of production-like requests to new implementation without user-visible side effect.

Good for:

```text
performance
compatibility
response diff
```

Danger: duplicated external side effects if “shadow” path writes/charges/sends.

Shadow system must be safely read-only or isolated.

## 14. Canary / Progressive Delivery

```text
1%
→ 5%
→ 25%
→ 100%
```

Each stage gated by:

```text
error rate
latency
business KPI
resource saturation
```

Rollback decision should be pre-defined where possible.

## 15. Feature Flags

Separate deploy from release.

Useful for:

- gradual rollout;
- kill switch;
- tenant cohort;
- migration.

But flags create state-space complexity. Need owner/expiry/removal.

## 16. Architecture debt

Debt là deliberate compromise có future cost.

Ví dụ:

```text
shared DB accepted for 3-month migration
```

Document:

```text
why accepted
risk
owner
exit trigger/date
```

Without this, “temporary” becomes permanent invisible architecture.

## 17. Platform dependency risk

Golden platform can become hard coupling:

```text
all teams must use internal library vX
→ release bottleneck
```

Prefer contracts/protocols/self-service APIs where possible over giant shared runtime packages.

## 18. Technology Radar / lifecycle

Classify technologies:

```text
Adopt
Trial
Assess
Hold
```

Decision should include:

- support maturity;
- security lifecycle;
- team skills;
- ecosystem;
- migration cost.

Avoid one-off tech snowflakes without strong reason.

## 19. Deprecation is architecture work

Old API/event/schema must have lifecycle:

```text
announce
measure usage
provide migration path
warn
block new consumers
sunset
remove
```

Never remove based only on “nobody told us they use it”. Measure consumers/traffic where possible.

## 20. Architecture Review Record

Review output nên có:

```text
Top risks
Decisions accepted
Decisions conditional on evidence
Open questions
Experiments required
Owners
Deadline/revisit trigger
```

Review không phải approval theater.

## 21. Example — Monolith to Payment Service

### Current

```text
CheckoutHost
├─ Orders
└─ Payments
    ↓ same SQL instance
```

### Trigger

```text
payment compliance ownership separates
provider integration releases 5× more often
unknown-outcome reconciliation needs dedicated operations
```

### Migration

```text
1. enforce Payments module boundary
2. stop Orders direct payment-table writes
3. define Payment contract
4. add outbox/integration messaging where needed
5. move payment data ownership
6. shadow/read compare if safe
7. route cohort to service
8. observe/reconcile
9. retire old module path
```

### Fitness

```text
no Orders → PaymentDB direct access
idempotency test
payment UNKNOWN reconciliation test
P95/SLO gate
trace completeness
```

## 22. Architecture review checklist

- [ ] ADR context/options/consequences clear;
- [ ] important rules executable where possible;
- [ ] deployment/data migration compatibility considered;
- [ ] no unsafe dual write hidden;
- [ ] canary/shadow path cannot cause duplicate side effects;
- [ ] feature flags have cleanup ownership;
- [ ] architecture debt documented with exit trigger;
- [ ] technology lifecycle/support risk known;
- [ ] deprecated contracts have measured retirement plan;
- [ ] review creates experiments/evidence, not opinions only.

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Architecture bền vững không phải architecture “đúng mãi”. Nó là architecture có **decision history, executable guardrails và migration seams** để thay đổi an toàn khi evidence mới xuất hiện.
</div>
