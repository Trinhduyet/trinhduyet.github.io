# Roadmap Overview

> Repository này là **learning + reference + practice system**, không phải sách phải đọc từ Module 01 đến Module 25.

## Có 60 phút? Bắt đầu như này

```text
1. Chọn role/problem hiện tại
2. Mở đúng một module
3. Học một mental model
4. Chạy/thiết kế một failure scenario
5. Lưu evidence
6. Viết 3 điều hiểu + 1 gap
```

→ **[Role-based Learning Paths](role-based-learning-paths.md)** — Backend, Senior Backend, DevOps/Platform, Kubernetes, Azure, Architect, Production AI.

Nếu chưa có role rõ, chọn **Backend Engineer** và đi từ request → SQL → API → tests trước.

---

# 1. Bốn lối vào

## Học để hiểu

[Human Learning Mode](human-learning-mode.md)

```text
scenario → code → failure → 3 điều cần nhớ
```

## Học để làm

[Example-First Checkout Learning Path](example-first-learning-path.md)

Một scenario nối:

```text
C# / async
→ SQL
→ API Design
→ ASP.NET Core
→ Testing
→ Cache
→ Docker / DevOps / Kubernetes
→ Distributed Systems
→ Microservices
→ System Design / Architecture
→ AI capability
```

## Học theo nghề

[Role-based Learning Paths](role-based-learning-paths.md)

```text
role
→ minimum concepts
→ core modules
→ evidence target
→ optional depth
```

## Tra nhanh

[Concept Cards](concept-cards.md)

Dùng khi cần nhắc mental model về transaction, index, idempotency, cache, outbox, saga, Kubernetes probes, SLO, RAG, tool calling...

---

# 2. Core learning graph

```text
FOUNDATION
CS + Linux/Git/Networking
        ↓
BACKEND
.NET → Backend → SQL → API → ASP.NET Core
        ↓
PRODUCTION
Testing → Security → Performance → Redis when justified → Docker
        ↓
DELIVERY / PLATFORM
DevOps/IaC → Azure
           ↘ Kubernetes when justified
        ↓
DISTRIBUTED
Distributed Systems → Microservices when autonomy pressure exists
        ↓
DESIGN
System Design → Software Architecture
        ↓
AI
AI Engineering / Coding Agents on top of engineering foundation
```

Không phải mọi Backend Engineer đều cần Kubernetes Level 5. Không phải mọi project cần Microservices. Roadmap là dependency graph, không phải checklist CV.

→ [Master Roadmap](master-roadmap.md)

---

# 3. Content maturity — đọc đúng trạng thái

Tách **content depth** khỏi **runnable evidence**.

| Maturity | Nghĩa |
|---|---|
| Reference | vocabulary + mental model + canonical sources |
| Guided | scenario + examples + guided failure/debug exercise |
| Deep | production/security/operations/cost/trade-off reasoning |
| Runnable | artifact thực sự tồn tại trong repo và chạy được |
| Verified Evidence | expected output/failure/recovery có thể kiểm chứng |

Một page có nhiều commands không tự động trở thành runnable lab nếu artifact tương ứng chưa nằm trong `labs/`.

→ [Learning Quality Standard](learning-quality-standard.md)

---

# 4. Repository hiện mạnh ở đâu?

Deep content hiện đặc biệt mạnh ở:

```text
05 SQL
06 API Design
07 ASP.NET Core
08 Testing & Code Review
12 Docker
13 DevOps & IaC
14 Azure
15 Kubernetes
17 Distributed Systems
18 Microservices Architecture
19 AI Engineering
21 AI Coding Agents
24 System Design
25 Software Architecture
```

Module 09 Security, 10 Performance và 11 Redis đã được nâng overview theo quality bar mới trong review 2026-08-28.

Chi tiết maturity/evidence thật:
[Repository Quality Review — 2026-08-28](repository-quality-review-2026-08-28.md).

---

# 5. Executable evidence hiện có

Dedicated runnable artifacts dưới `labs/` hiện có cho:

```text
01 Computer Science
02 Linux/Git/Networking
03 .NET Runtime
04 Backend
15 Kubernetes Core
```

Kubernetes core lab mới chứng minh trực tiếp:

```text
Deployment → ReplicaSet → Pod
Service → selector → Ready Pods
reconciliation
bad image / ImagePullBackOff
wrong selector
readiness failure
resources / scaling / rollout
kubectl debugging
```

→ [`labs/15-kubernetes`](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/15-kubernetes)

Nhiều module sau vẫn có deep guided commands/config/failure drills nhưng **chưa có dedicated runnable lab**. Đây vẫn là backlog chính tiếp theo, không được che bằng chữ “Done”.

Ưu tiên lab integration:

```text
05–08 Production Backend Lab
09–13 Production Delivery Lab
17 Distributed Reliability Lab
18 Checkout Saga Lab
14 Azure IaC Lab
19 Production AI Lab
15 Kubernetes extensions only when problem-driven
```

Một integrated system được phá/debug xuyên nhiều module tốt hơn nhiều sample rời.

---

# 6. Quality loop cho mọi topic P0/P1

```text
Problem thật
  ↓
Mental model
  ↓
Minimal code/config
  ↓
Expected state
  ↓
Failure experiment
  ↓
Debugging
  ↓
Fix / recovery
  ↓
Trade-off
  ↓
Evidence
```

Bad chapter:

```text
definition
pattern list
tool list
```

Good chapter phải nối được lý thuyết vào ít nhất một:

```text
request
state transition
SQL query
message
container/Pod
failure
metric/trace
business decision
```

---

# 7. Evidence thay cho “đã đọc”

Examples:

```text
reproduce duplicate checkout
prove SQL uniqueness invariant
inspect actual execution plan
simulate dependency timeout
reconcile UNKNOWN payment
break Kubernetes readiness
wrong Service selector → inspect endpoints
create queue backlog
measure P95/P99 at saturation
run authorization-negative test
write ADR rejecting unnecessary complexity
```

Evidence hierarchy:

```text
read
< explain
< implement
< test
< break
< debug
< recover
< design/review from evidence
```

---

# 8. Source/version policy

Use:

```text
Official spec/docs
→ official release/support source
→ provider compatibility matrix
→ measured evidence
```

Community/Vietnamese resources có thể giúp explanation. `roadmap.sh` giúp breadth audit. Chúng không override current official behavior/security/version.

→ [Source Policy](source-policy.md)

Version snapshot:
→ [Technology Baseline](technology-baseline.md)

Important:

```text
upstream current
!= provider supported
!= lab pinned
```

---

# 9. Skills không phải checklist cá nhân trong public docs

[Skills Matrix](skills-matrix.md) mô tả:

```text
capability
priority
target depth
content maturity
runnable evidence
where to prove it
```

Current learner level nên được track riêng bằng progress/evidence, không hard-code giả định kinh nghiệm của một người vào public repository.

→ [Progress Template](progress-template.md)

---

# 10. Nếu bị overload

Đừng mở thêm module.

Chọn một flow:

```text
HTTP request
→ application
→ SQL
```

Sau đó lần lượt thêm pressure thật:

```text
security
performance
cache
container
deployment
failure
queue
cloud/platform
```

Technology chỉ xuất hiện khi pressure yêu cầu nó.

## Tiếp theo

- **Không biết học gì:** [Role-based Learning Paths](role-based-learning-paths.md)
- **Muốn học bằng một system:** [Example-First Path](example-first-learning-path.md)
- **Muốn xem toàn landscape:** [Master Roadmap](master-roadmap.md)
- **Muốn xem debt/chất lượng repo:** [Repository Quality Review](repository-quality-review-2026-08-28.md)
- **Muốn biết chuẩn viết content:** [Learning Quality Standard](learning-quality-standard.md)
