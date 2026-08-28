# Module 08 — Testing & Code Review

> [← ASP.NET Core](../07-aspnet-core/README.md) · [DevOps & IaC →](../13-devops-iac/README.md) · [Kubernetes delivery path →](../13-devops-iac/devops-kubernetes-production-delivery.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;Quality Evidence · Review · CI Gates</span>
  <span><strong>Output</strong>&nbsp;deployable change with trustworthy evidence</span>
</div>

Testing và code review không phải giai đoạn đứng ngoài DevOps. Đây là **quality evidence layer đầu tiên của delivery pipeline**.

```text
Requirement
   ↓
Code
   ↓
Unit / integration / contract / load tests
   ↓
Code review
   ↓
Quality gates
   ↓
CI builds artifact
   ↓
CD / Kubernetes rollout
```

Nếu pipeline tự động deploy rất nhanh nhưng quality evidence yếu, automation chỉ làm failure đi production nhanh hơn.

---

# 1. Module này bảo vệ điều gì?

Mỗi loại evidence bảo vệ một boundary khác nhau.

| Evidence | Câu hỏi |
|---|---|
| unit tests | business logic nhỏ có còn đúng? |
| integration tests | app + database/dependency boundary có đúng? |
| contract tests | API/event contract có tương thích? |
| load/performance tests | workload có giữ latency/capacity target? |
| security tests/checks | known unsafe change có bị chặn? |
| code review | design, correctness, operations và maintainability có được challenge? |

Không có một test type thay thế tất cả phần còn lại.

---

# 2. Quality gate là input của CI/CD

Một CI gate tốt không chỉ trả về:

```text
PASS / FAIL
```

Nó phải có semantics rõ:

```text
build gate
→ code compile/reproducible?

unit gate
→ core invariants intact?

integration gate
→ DB/queue/external boundary works?

contract gate
→ caller/consumer compatibility intact?

security gate
→ unacceptable risk introduced?

performance gate
→ regression exceeds budget?
```

Gate nào quá flaky hoặc không gắn với risk thật sẽ khiến team bỏ qua signal.

---

# 3. Từ Pull Request đến Kubernetes

Một change có thể đi theo flow:

```text
Pull Request
  ↓
review
  ↓
unit/integration/contract tests
  ↓
container build
  ↓
image scan
  ↓
immutable image digest
  ↓
non-prod deployment
  ↓
smoke / integration checks
  ↓
production rollout
  ↓
readiness + SLO/business telemetry
```

Kubernetes không biết code có đúng business rule hay không. Nó chỉ reconcile workload state.

Do đó:

```text
Kubernetes self-healing
!=
application correctness
```

---

# 4. Test pyramid không phải luật cứng

Một practical backend test portfolio có thể gồm:

```text
many fast logic tests
+ focused integration tests
+ contract tests at service boundaries
+ a smaller number of end-to-end flows
+ targeted load/failure experiments
```

Chọn test theo risk và feedback speed, không theo số lượng tầng đẹp trên diagram.

---

# 5. Integration test phải chạm boundary thật khi cần

Mocks hữu ích để isolate logic nhưng không chứng minh:

```text
SQL migration works
unique constraint works
transaction isolation behavior
JSON contract serialization
message broker delivery semantics
real HTTP middleware pipeline
```

Với critical boundary, integration evidence nên dùng dependency representative hoặc environment disposable/reproducible.

---

# 6. Contract compatibility quan trọng khi deploy rolling

Khi old/new versions coexist:

```text
caller v1
caller v2
service v1
service v2
```

breaking change có thể fail dù từng version tự test green.

Review contract:

```text
API request/response
message schema
DB schema compatibility
config keys
feature flags
```

Đây là prerequisite trực tiếp cho safe Kubernetes rolling deployment.

---

# 7. Database migration phải được test như release behavior

Bad flow:

```text
deploy new app
+ DROP old column
```

trong khi old Pods/instances vẫn chạy.

Safer mental model:

```text
expand schema
→ deploy compatible app
→ migrate/backfill
→ switch behavior
→ contract/remove later
```

Test phải cover compatibility giữa app version và schema state, không chỉ migration script syntax.

---

# 8. Load test và Kubernetes autoscaling

Load test không chỉ để tìm max RPS.

Nếu workload chạy trên Kubernetes, test phải giúp set:

```text
CPU/memory requests
replica baseline
HPA target
max replicas
node capacity
queue/backpressure limits
```

Observe:

```text
P95/P99 latency
CPU
memory/GC
DB connections
queue depth
Pod readiness/restarts
HPA behavior
Pending Pods
```

Autoscaling cannot repair an inefficient dependency or unbounded workload automatically.

---

# 9. Failure tests trước production

Useful experiments:

```text
duplicate request
DB timeout
queue redelivery
external provider timeout
container SIGTERM
Pod killed
readiness fails
memory limit hit
bad configuration
bad deployment
```

Expected outcome phải viết trước khi chạy experiment.

Example:

```text
Given payment provider timeout
When worker cannot know provider outcome
Then state becomes UNKNOWN
And reconciliation occurs
And retry does not double-charge
```

---

# 10. Code review phải review operational behavior

Một production review không chỉ xem clean code.

Review lenses:

```text
Correctness
Security
Performance
Data/consistency
Failure/retry/idempotency
Deployment compatibility
Observability
Rollback/recovery
Cost/capacity
```

Reviewer nên hỏi:

```text
What happens if this runs twice?
What if dependency is slow?
Can old/new versions coexist?
What metric tells us this failed?
Can this change be rolled back?
Does this increase resource/cost materially?
```

---

# 11. Quality gates và deployment policy

Example policy:

```text
Pull Request
├─ build
├─ unit tests
├─ integration tests
├─ contract checks
├─ security checks
└─ review approval
     ↓
artifact creation allowed
     ↓
non-prod deployment
     ↓
smoke checks
     ↓
production promotion
```

Không nên chạy mọi expensive test trên mọi commit nếu feedback quá chậm. Có thể tier gates theo:

```text
PR
merge/main
pre-production
scheduled/nightly
release candidate
```

Nhưng critical correctness checks phải nằm đủ sớm để failure rẻ.

---

# 12. Evidence phải trace được tới artifact

Production incident cần trả lời:

```text
Which commit?
Which tests passed?
Which image digest?
Which config revision?
Which deployment revision?
```

Vì vậy test report chỉ hữu ích khi nối được với release artifact.

```text
Git SHA
→ CI run
→ test evidence
→ image digest
→ deployment
```

→ [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

---

# 13. Learning slices

| Guide | Focus |
|---|---|
| [Test Strategy & Boundaries](test-strategy-and-boundaries.md) | test portfolio, boundaries, deterministic evidence |
| [Integration, Contract & Load Testing](integration-contract-and-load-testing.md) | real dependencies, contracts, capacity/load evidence |
| [Code Review, Quality Gates & Failure Analysis](code-review-quality-and-failure-analysis.md) | review discipline, gates, production failure reasoning |

---

# 14. Evidence tối thiểu

Để hoàn thành module, learner nên có:

1. unit tests bảo vệ business invariants;
2. integration test với database/dependency thực hoặc representative;
3. API/message contract evidence;
4. một load/capacity experiment;
5. một failure/retry/idempotency experiment;
6. code-review checklist có operational concerns;
7. CI quality gate thực tế;
8. evidence trace từ commit → test run → artifact.

---

# 15. Tiếp tục từ đây

Module này không kết thúc ở "test green".

Flow tiếp theo:

```text
Module 08 — quality evidence
   ↓
Module 09 — security / DevSecOps
   ↓
Module 12 — containers
   ↓
Module 13 — DevOps / CI/CD / IaC
   ↓
Module 15 — Kubernetes when justified
```

→ [Module 13 — DevOps & IaC](../13-devops-iac/README.md)

## Official references

Xem [references.md](references.md). Behavior/specification lấy từ official sources; roadmap resources dùng để kiểm tra coverage và learning order.

## Verification metadata

- Verified: 2026-08-28.
- Updated to make testing/code review an explicit upstream stage of DevOps/Kubernetes delivery.
