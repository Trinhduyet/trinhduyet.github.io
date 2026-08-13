# Roadmap Overview

> **Kho này là reference + practice system, không phải sách phải đọc hết.** Nếu bạn đọc nhiều nhưng khó nhớ, bắt đầu lại bằng ví dụ chứ không đọc thêm lý thuyết.

## 3 lối vào

### Học để hiểu

**[Human Learning Mode](human-learning-mode.md)**

```text
scenario → code → failure → 3 điều cần nhớ
```

### Học để làm

**[Example-First Checkout Learning Path](example-first-learning-path.md)**

Một hệ thống duy nhất nối:

```text
C# / async
→ SQL
→ API Design
→ ASP.NET Core
→ Cache
→ Docker / Kubernetes
→ Distributed Systems
→ Microservices
→ System Design
→ AI Engineering
→ AI Coding Agents
```

### Tra nhanh khái niệm

**[Concept Cards](concept-cards.md)** — Transaction, Index, Idempotency, Cache, Outbox, Saga, Reconciliation, Kubernetes probes, SLO, RAG, Tool Calling...

---

# Deep roadmap

Khi đã có một problem cụ thể, đi vào module sâu tương ứng:

```text
.NET
→ Backend
→ SQL
→ API Design
→ ASP.NET Core
→ Production Engineering
→ Docker / Kubernetes
→ Distributed Systems
→ Microservices Architecture
→ System Design
→ Software Architecture
```

AI lane chạy song song sau backend foundations:

```text
AI Engineering
→ RAG
→ Agents / MCP
→ AI Security
→ GenAIOps
→ AI System Design
```

Chi tiết dependency/priority: [Master Roadmap](master-roadmap.md).

---

# Active code-first modules

| Module | Status | Example-first hook |
|---|---|---|
| 05 SQL | deep rewrite v1 | UNIQUE idempotency + transactions + plans |
| 06 API Design | deep rewrite v1 / 25-topic coverage | `POST /checkouts` |
| 07 ASP.NET Core | deep rewrite v1 | checkout endpoint + explicit state |
| 12 Docker | deep rewrite v1 | package Checkout API |
| 15 Kubernetes | deep rewrite v1 | readiness/liveness + rollout |
| 17 Distributed Systems | code-first v1 | Outbox / Inbox / duplicate delivery |
| 18 Microservices Architecture | code-first v1 | payment UNKNOWN + reconciliation |
| 19 AI Engineering | code-first v1 | order-status tool + authorization |
| AI Coding Agents | code-first v1 | issue → edit → build/test → PR |
| 24 System Design | code-first v1 | capacity + scale + failure + cost |

Các module khác vẫn có giá trị reference nhưng một số còn cần thêm scenario/code để đạt cùng quality bar.

---

# Quality Gate mới cho tài liệu

Một chapter không nên chỉ có:

```text
definition
pattern list
architect paragraph
```

Với topic P0/P1, ưu tiên:

```text
1. Problem thật
2. Minimal code/config
3. Expected output/state
4. Failure experiment
5. Fix
6. 3 điều cần nhớ
7. Deep internals (optional)
8. Trade-offs
```

Nếu phần lý thuyết không nối được vào một request, state transition, query, message, failure hoặc metric cụ thể thì nó chưa đủ dễ học.

---

# Evidence thay cho “đã đọc”

```text
reproduce duplicate checkout
viết SQL invariant
xem execution plan
simulate payment timeout
reconcile UNKNOWN state
kill Pod / observe readiness
build queue backlog
measure capacity
viết regression test
```

Mục tiêu:

```text
Problem
→ Mechanism
→ Failure
→ Evidence
→ Trade-off
```

Không phải thuộc càng nhiều thuật ngữ càng tốt.

---

# Source / version policy

Deep technical claims vẫn theo:

- official specifications / official docs;
- English source of truth;
- Vietnamese explanation để hiểu nhanh;
- Context7 cho framework/library version-sensitive APIs khi có;
- roadmap.sh để kiểm scope, không làm canonical implementation source.

Xem [Source Policy](source-policy.md) và [Technology Baseline](technology-baseline.md).
