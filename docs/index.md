# AI-Enabled Software Architect Roadmap

> **Đừng đọc toàn bộ site từ đầu đến cuối.** Deep docs dài để tra cứu. Muốn học và nhớ, hãy bắt đầu bằng một hệ thống thật.

## Bắt đầu ở đây

### 1. Tôi muốn hiểu nhanh

→ **[Human Learning Mode](00-roadmap/human-learning-mode.md)**

Bạn sẽ học theo:

```text
scenario
→ code
→ failure
→ 3 điều cần nhớ
```

không phải:

```text
50 định nghĩa
→ 20 patterns
→ quên sau một ngày
```

### 2. Tôi muốn học bằng code xuyên suốt

→ **[Example-First Checkout Learning Path](00-roadmap/example-first-learning-path.md)**

Một hệ thống duy nhất đi xuyên roadmap:

```text
Customer
   ↓
Checkout API
   ↓
Order
 ├─ Inventory
 ├─ Payment
 └─ Notification

AI Assistant
   ↓
read-only business tools
```

Bạn sẽ thấy cùng một bài toán được nâng cấp qua:

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

### 3. Tôi chỉ muốn làm một bài nhỏ 10–30 phút

→ **[Practical Mini-Labs](00-roadmap/practical-mini-labs.md)**

Có thí nghiệm cho:

```text
SQL race + execution plan
AuthN/AuthZ
rate limiting
integration test
cache stale data
P99 latency
Docker localhost trap
Kubernetes readiness
Outbox duplicate
queue backlog
tracing
CI
Terraform workflow
AI tool authorization
```

### 4. Tôi quên một khái niệm

→ **[Concept Cards — 35 khái niệm trong 30 giây](00-roadmap/concept-cards.md)**

Ví dụ:

```text
Idempotency
= request lặp lại không tạo side effect ngoài ý muốn

Outbox
= business state + message cùng local transaction

Reconciliation
= hỏi source of truth khi local state là UNKNOWN
```

### 5. Tôi đang debug/review architecture

→ **[Master Roadmap](00-roadmap/master-roadmap.md)**

Lúc này mới dùng deep modules như reference manual.

---

# Một mental model cho toàn site

Nếu chỉ nhớ một flow, nhớ flow này:

```text
Request
   ↓
API Contract
   ↓
Application State
   ↓
Database Invariant
   ↓
External Side Effect
   ↓
Failure / Retry / Duplicate
   ↓
Observability
   ↓
Recovery
   ↓
Capacity / Cost
   ↓
Architecture Decision
```

Các module chỉ giúp bạn trả lời sâu hơn từng đoạn của flow này.

---

# Core practical path

| Stage | Học gì | Ví dụ xuyên suốt |
|---:|---|---|
| 1 | C# / async | gọi Payment API + timeout |
| 2 | SQL | order + UNIQUE idempotency invariant |
| 3 | API Design | `POST /checkouts` + `Idempotency-Key` |
| 4 | ASP.NET Core | endpoint + explicit order state |
| 5 | Cache | product cache + stale-data failure |
| 6 | Docker / Kubernetes | deploy API + readiness/liveness |
| 7 | Distributed Systems | Outbox + Inbox/Dedup |
| 8 | Microservices | `FAILED != UNKNOWN` + reconciliation |
| 9 | System Design | capacity math + scale/failure/cost |
| 10 | AI Engineering | read-only business tools + AuthZ |
| 11 | AI Coding Agents | task → edit → build/test → PR |

**[Đi theo path này →](00-roadmap/example-first-learning-path.md)**

---

# Deep modules

Khi một stage tạo câu hỏi cụ thể, mở module tương ứng:

- [.NET / C# Runtime](03-dotnet/README.md)
- [Backend Engineering](04-backend/README.md)
- [SQL / SQL Server](05-sql/README.md)
- [API Design](06-api-design/README.md)
- [ASP.NET Core](07-aspnet-core/README.md)
- [Redis & Caching](11-redis-caching/README.md)
- [Docker](12-docker/README.md)
- [Kubernetes](15-kubernetes/README.md)
- [Distributed Systems](17-distributed-systems/README.md)
- [Microservices Architecture](18-microservices-architecture/README.md)
- [AI Engineering](19-ai-engineering/README.md)
- [AI Coding Agents](21-ai-coding-agents/README.md)
- [System Design](24-system-design/README.md)

Không cần đọc hết một module. Chỉ đọc phần giải thích failure/problem bạn vừa gặp.

---

# Cách học 60 phút

```text
20 phút — chạy example
20 phút — cố tình làm hỏng
20 phút — đọc lý thuyết giải thích behavior vừa thấy
```

Ví dụ:

```text
payment timeout
→ local order không biết provider đã charge hay chưa
→ state = UNKNOWN
→ đọc idempotency + reconciliation
```

Cách này giữ kiến thức tốt hơn việc đọc Saga/CAP/Eventual Consistency trước khi có tình huống cần chúng.

---

# Khi nào coi là “đã học”?

Không phải khi đọc xong.

Evidence tốt:

```text
reproduce duplicate checkout
viết UNIQUE constraint
xem execution plan
simulate timeout
reconcile unknown payment
kill Pod và quan sát readiness
build queue backlog
viết regression test
vẽ lại flow từ memory
```

Mục tiêu cuối cùng không phải “biết nhiều thuật ngữ”, mà là:

```text
Problem
→ Mechanism
→ Failure
→ Evidence
→ Trade-off
```

Đó là cách một Senior Engineer / Architect thực sự sử dụng kiến thức.
