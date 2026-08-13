# Knowledge dependency graph

## Mental model

Một topic “đứng trước” topic khác khi thiếu nó sẽ làm người học chỉ nhớ syntax mà không giải thích được behavior, failure hoặc trade-off.

![Dependency layers từ foundations đến architecture](../assets/diagrams/prerequisites-layer-stack.svg)

---

## Dependency rules quan trọng

| Học sau | Prerequisite tối thiểu | Vì sao |
| --- | --- | --- |
| ASP.NET Core | HTTP, process/socket, DI, async | Middleware syntax không giải thích request lifecycle/saturation |
| EF Core performance | SQL, indexes, execution plan | LINQ không cho biết database thực sự làm gì |
| Docker | Linux process/filesystem/signals/networking | Container là process isolation, không phải “VM nhẹ” |
| Kubernetes | container model, DNS/networking, resources | YAML không giải thích reconciliation/readiness/scheduling |
| Outbox | local transaction, messaging, idempotency | Pattern vô nghĩa nếu chưa hiểu atomic boundary và duplicates |
| **Microservices** | **modular boundaries + Distributed Systems + observability** | **Tách process tạo network/data-consistency/deployment/operations cost** |
| RAG | embeddings/retrieval, data lifecycle, evaluation | Demo retrieval không đủ cho ACL/deletion/regression |
| MCP/Agents | tool contract, identity/authz, trust boundaries | Discovery/transport không thay authorization |
| System Design | FR/NFR, capacity, data/distributed fundamentals | Component choice phải theo requirement/scale |
| Software Architecture | System Design, boundaries, migration, docs | Pattern không phải recipe; architecture phải tiến hóa được |

---

# Entry criteria theo track

## 05 SQL

- logic/sets và data types cơ bản;
- có thể đọc code backend đơn giản;
- không yêu cầu EF Core trước.

## 07 ASP.NET Core

- HTTP semantics;
- C# async/cancellation;
- DI/config/logging;
- API design fundamentals;
- SQL/EF basics.

## 15 Kubernetes

- Linux/network diagnostics;
- container image/runtime/lifecycle;
- configuration/secrets;
- observability basics.

## 17 Distributed Systems

Phải có thể:

```text
network timeout
transaction boundary
idempotency
basic messaging
logs/metrics/traces
```

Trước khi qua Module 18, cần giải thích được:

- timeout = unknown outcome trong một số side-effect operations;
- retry amplification;
- Outbox/Inbox/Dedup;
- at-least-once delivery;
- eventual consistency;
- Saga/compensation;
- backpressure/order basics.

## 18 Microservices Architecture

Prerequisite:

```text
Module 17 Distributed Systems
+
module boundaries / domain ownership
+
API contract evolution
+
observability / deployment basics
```

Entry evidence tối thiểu:

```text
[ ] Có thể thiết kế idempotent side effect.
[ ] Có thể mô tả Outbox dual-write problem.
[ ] Có thể phân biệt authoritative state và read model.
[ ] Có thể trace request qua ít nhất 2 network boundaries.
[ ] Có thể giải thích rolling deployment + backward compatibility.
```

Không nên học Microservices bằng cách chỉ dựng:

```text
3 Spring/.NET services + Docker Compose
```

nếu chưa reasoning được failure/data/deployment semantics.

---

# Microservices dependency detail

## Step 1 — Modular boundary trước network boundary

```text
Order module
Payment module
Inventory module
```

Nếu code trong một process còn xuyên module bằng shared table/internal type, tách thành HTTP services thường chỉ biến coupling thành remote coupling.

## Step 2 — Distributed failure

```text
local call
→ deterministic result/exception boundary

remote call
→ request may execute while response is lost
```

Do đó microservice checkout cần `Unknown/Pending` state và reconciliation cho operation không thể prove outcome.

## Step 3 — Data ownership

```text
service boundary
→ owned data/schema/migrations
→ integration via API/events
```

Shared DB schema phá independent lifecycle.

## Step 4 — Contract evolution

Independent deploy cần overlap:

```text
old consumer + new provider
new consumer + old provider
```

hoặc migration strategy rõ.

## Step 5 — Operations

Mỗi service thêm:

```text
CI/CD
runtime resources
SLO
trace/log/metrics
on-call/runbook
security patching
cost
```

Đây là lý do “nhiều service” không tự động tốt hơn monolith.

---

# Anti-shortcut checks

- Nếu không đọc được execution plan, chưa tối ưu EF query bằng “best practice”.
- Nếu không giải thích được SIGTERM/readiness, chưa thiết kế rolling shutdown.
- Nếu không có idempotency boundary, chưa thêm retry cho side effect.
- Nếu không hiểu timeout = unknown outcome, chưa thiết kế payment checkout phân tán.
- Nếu service share DB/domain entity, chưa có data/service autonomy thật.
- Nếu release phải lock-step mọi service, chưa đạt independent deployment.
- Nếu không có trace/SLO/runbook, chưa đủ production readiness cho microservices.
- Nếu không có eval dataset, chưa tuyên bố model/prompt mới “tốt hơn”.
- Nếu không có NFR/capacity, chưa chọn microservices/Kubernetes/multi-region.

---

# Recommended next path

```text
Computer Science
→ Linux/Networking
→ C#/.NET
→ Backend + SQL + API
→ ASP.NET Core
→ Docker/Kubernetes
→ Distributed Systems
→ Microservices Architecture
→ System Design
→ Software Architecture
```

AI Engineering có thể chạy song song sau khi backend foundation đủ chắc.

## Verification metadata

- Verified: 2026-08-13.
- Microservices dependency is explicit: Module 17 → Module 18.
- Official sources: module-specific references.
