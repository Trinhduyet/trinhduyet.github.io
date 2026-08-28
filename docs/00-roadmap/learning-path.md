# Learning Path — Dependency Gates, Not a Fixed Calendar

> Nếu bạn đã biết target role, dùng [Role-based Learning Paths](role-based-learning-paths.md). Trang này mô tả **dependency gates** giữa các nhóm kiến thức đang tồn tại trong repository.

Không skip vì “đã từng dùng”. Skip khi có evidence cho exit outcome.

## 8 phases thực tế

| Phase | Scope | Exit outcome | Evidence type |
|---|---|---|---|
| 1 — Foundations | 01 CS + 02 Linux/Git/Networking | reason process/memory/concurrency + diagnose DNS/TCP/TLS/HTTP | runnable labs 01–02 |
| 2 — .NET Runtime | 03 | async/cancellation/resources/ThreadPool/GC behavior | runnable lab 03 |
| 3 — Backend Core | 04–07 | request → contract → SQL → ASP.NET path có correctness evidence | runnable lab 04 + guided 05–07 |
| 4 — Production Engineering | 08–12 | tests/security/performance/cache decision/container behavior | guided content; integrated lab backlog |
| 5 — Delivery & Platform | 13–15 | artifact/IaC/cloud/Kubernetes deployment reasoning | guided deep content; lab backlog |
| 6 — Distributed & Services | 17–18 | timeout/duplicate/message/reconciliation + justified service boundary | guided deep content |
| 7 — Design | 24–25 | requirements/capacity/failure → architecture decisions/evolution | design dossiers |
| 8 — AI capability | 19 + 21 | production AI/tool/RAG/eval + governed coding-agent workflow | guided deep content; runnable AI lab backlog |

AI không bắt buộc phải học sau Architecture; nó có thể chạy song song sau Backend/Production foundations. Bảng chỉ cho thấy dependency sâu nhất.

---

# Gate A — Backend-ready

Bạn phải có thể:

- explain request path từ DNS/TCP/HTTP tới handler;
- phân biệt process/thread/Task và blocking/async I/O;
- viết async code có cancellation + deterministic cleanup;
- thiết kế relational invariant/transaction boundary;
- đọc SQL/execution plan cơ bản;
- define HTTP contract/AuthZ behavior;
- trace ASP.NET Core request pipeline.

Nếu chưa đạt, không cần thêm Redis/Kubernetes/microservices.

---

# Gate B — Production-backend-ready

Evidence:

```text
unit/integration/contract tests
negative authorization test
performance baseline
failure/dependency behavior
Docker image/runtime diagnostics
```

Bạn phải giải thích được:

```text
correctness
security
latency/capacity
resource lifecycle
rollback/recovery basics
```

Redis chỉ thêm khi measured workload/staleness requirement justify.

---

# Gate C — Delivery/platform-ready

Required reasoning:

```text
Git SHA
→ quality/security gates
→ immutable artifact/image digest
→ registry
→ environment promotion
→ IaC
→ runtime/platform
→ observability
→ rollback/recovery
```

Kubernetes-specific entry criteria:

- Linux process/signals;
- DNS/networking;
- Docker image/container/runtime;
- CPU/memory/resources;
- application health/readiness semantics.

Kubernetes flow phải explain được:

```text
Deployment → ReplicaSet → Pod → Container
Client → DNS → Service → selector → Ready Pods
```

Azure and Kubernetes are related but independent learning dimensions:

```text
Azure = cloud/provider capabilities
Kubernetes = orchestration API/runtime model
AKS = mapping between both
```

---

# Gate D — Distributed-ready

Before distributed architecture, be able to reason:

```text
remote timeout can mean UNKNOWN outcome
retry can duplicate side effects
at-least-once can redeliver
queue does not create processing capacity
```

Evidence target:

- idempotent side effect;
- outbox dual-write reasoning;
- consumer crash-before-ACK scenario;
- backlog/oldest-message-age reasoning;
- ordering scope decision;
- Saga/compensation/reconciliation model.

→ [Module 17](../17-distributed-systems/README.md)

---

# Gate E — Microservices-ready

Do not use “many services” as success criteria.

Before extracting a service:

```text
[ ] business capability / bounded context is clear
[ ] data ownership is explicit
[ ] contract is explicit/version-compatible
[ ] timeout/retry/idempotency semantics are defined
[ ] observability/runbook ownership exists
[ ] independent deployment pressure is real
[ ] operational cost is accepted
```

Progression:

```text
Structured Monolith
→ Modular Monolith
→ distributed-system competence
→ extract one justified capability
→ prove autonomy
→ expand only with pressure
```

→ [Module 18](../18-microservices-architecture/README.md)

---

# Gate F — Design-ready

System Design exercise must start with:

```text
FR/NFR
capacity/workload
source of truth
consistency
failure model
security
cost
```

not:

```text
Redis + Kafka + Kubernetes + sharding
```

Architecture review then adds:

```text
quality attributes
boundaries/ownership
coupling/styles
team/deployment implications
ADR
fitness functions
evolution/migration
```

→ [System Design](../24-system-design/README.md) → [Software Architecture](../25-software-architecture/README.md)

---

# Gate G — Production-AI-ready

AI feature is not production-ready if it only does:

```text
prompt → model → text
```

Need evidence for relevant dimensions:

```text
provider/model config
structured output validation
tool authorization
retrieval ACL/deletion/versioning
evaluation regression
latency/cost
PII-safe telemetry
timeout/fallback/degradation
```

Coding agents additionally require:

```text
repo context
least privilege
sandbox/tool boundary
build/test evidence
human review
no default production merge/deploy authority
```

→ [AI Engineering](../19-ai-engineering/README.md) · [AI Coding Agents](../21-ai-coding-agents/README.md)

---

# Evidence reality check

Dedicated executable lab directories currently cover Modules **01–04**. Later phases contain deep guided exercises but are not all runnable artifacts yet.

Use this distinction:

```text
Content learned from guided scenario
!=
Repository has executable lab
```

The highest-leverage backlog is to build integrated labs for:

```text
05–08 Production Backend
09–13 Production Delivery
15 Kubernetes
17 Distributed Reliability
18 Checkout Saga
14 Azure IaC
19 Production AI
```

→ [Repository Quality Review](repository-quality-review-2026-08-28.md)

---

# Recommended learning slice

```text
Problem
→ mental model
→ minimal implementation
→ expected output
→ failure
→ debugging
→ recovery/fix
→ trade-off
→ evidence
```

If you cannot name the evidence you will produce in the next 60–120 minutes, scope is probably too broad.

## Verification metadata

- Rebuilt: 2026-08-28.
- Uses only modules that actually exist in the repository.
- Removed stale references to phantom Module 20 and hypothetical numbered phases/projects.
- Technology versions: [Technology Baseline](technology-baseline.md).
