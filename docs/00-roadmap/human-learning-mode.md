# Human Learning Mode — học để nhớ, không phải đọc để hết

> Nếu bạn đã đọc rất nhiều chapter nhưng sau đó **không kể lại được hệ thống hoạt động như thế nào**, vấn đề không phải thiếu thêm lý thuyết. Vấn đề là cách học đang có quá nhiều khái niệm mới cùng lúc.

Từ đây repository có hai lớp:

```text
HUMAN MODE
scenario → code → failure → 3 điều cần nhớ
        ↓
DEEP MODE
internals → performance → security → architecture → sources
```

**Human Mode là mặc định để học. Deep Mode dùng để đào sâu hoặc review kiến trúc.**

---

# 1. Không đọc toàn bộ chapter trong một lượt

Một người học không cần giữ đồng thời:

```text
HTTP semantics
OAuth
SQL isolation
EF query plan
Docker network
Kubernetes reconciliation
Outbox
Saga
CAP
RAG
Agent security
SLO
DR
```

trong working memory.

Thay vào đó, học theo **một vấn đề thực tế**.

Ví dụ:

> User bấm **Checkout** hai lần. Làm sao không tạo hai order và không charge hai lần?

Từ một câu hỏi này ta học dần:

```text
API Design
→ Idempotency-Key

SQL
→ UNIQUE constraint

ASP.NET Core
→ endpoint + transaction

Distributed Systems
→ retry + duplicate

Microservices
→ payment timeout = UNKNOWN

System Design
→ reconciliation + SLO + capacity
```

Một problem nối nhiều concept lại với nhau nên dễ nhớ hơn học từng định nghĩa rời rạc.

---

# 2. Quy tắc 20–20–20

Một session khoảng 60 phút:

## 20 phút đầu — chạy ví dụ

Không đọc Internals.

Mục tiêu:

```text
request đi đâu
state đổi thế nào
response ra sao
```

## 20 phút tiếp — làm hỏng

Ví dụ:

```text
send request hai lần
kill dependency
simulate timeout
remove index
return duplicate message
expire token
break readiness probe
```

Mục tiêu:

```text
failure xảy ra ở đâu
system quan sát được gì
recovery nằm ở đâu
```

## 20 phút cuối — đọc lý thuyết vừa đủ

Chỉ đọc phần trả lời cho failure vừa thấy.

Ví dụ:

```text
payment timeout
→ đọc unknown outcome
→ đọc idempotency
→ đọc reconciliation
```

Không đọc Saga/CAP/consistency nếu chưa có problem cần chúng.

---

# 3. Mỗi topic chỉ cần nhớ 3 tầng

Ví dụ **Cache**:

### Tầng 1 — một câu

> Cache đổi freshness/consistency lấy latency và giảm load.

### Tầng 2 — một flow

```text
GET product
→ check cache
→ miss
→ read DB
→ put cache
→ return
```

### Tầng 3 — một failure

```text
DB updated
but cache still old
→ stale data
```

Sau khi nắm ba tầng này mới cần đọc TTL, invalidation, stampede, eviction, hot key.

---

# 4. Mỗi code example phải trả lời 5 câu

Khi gặp code, không cố nhớ syntax. Hỏi:

1. **Input là gì?**
2. **State nào thay đổi?**
3. **Side effect ở đâu?**
4. **Nếu chạy hai lần thì sao?**
5. **Nếu crash giữa chừng thì sao?**

Ví dụ:

```csharp
await payment.ChargeAsync(orderId, amount, cancellationToken);
```

Đừng chỉ hiểu là “gọi payment”. Hỏi:

```text
Có charge thật chưa?
Idempotency key là gì?
Timeout nghĩa là FAILED hay UNKNOWN?
Retry có charge lần hai không?
Có endpoint query payment status không?
```

Đó là cách đọc code ở level Senior/Architect nhưng vẫn bắt đầu từ một dòng code đơn giản.

---

# 5. Một reference system xuyên suốt

Toàn bộ Example-First path dùng cùng một hệ thống:

```text
Web / Mobile
    ↓
Checkout API
    ↓
Order
    ├── Inventory
    ├── Payment
    └── Notification

AI Assistant
    ↓
read-only business tools
```

Không đổi domain mỗi chapter. Khi thêm kiến thức mới, ta **nâng cấp cùng hệ thống**.

Các stage:

```text
01 C# / async
02 SQL
03 API Design
04 ASP.NET Core
05 Cache
06 Docker / Kubernetes
07 Distributed Systems
08 Microservices
09 System Design
10 AI Engineering
11 AI Coding Agents
```

Bắt đầu tại **[Example-First Learning Path](example-first-learning-path.md)**.

Nếu muốn các thí nghiệm nhỏ độc lập thay vì đi cả path, dùng **[Practical Mini-Labs](practical-mini-labs.md)**. Có lab cho SQL race, execution plan, AuthZ, rate limiting, integration test, cache stale data, Docker networking, K8s readiness, Outbox duplicate, queue backlog, tracing, CI và IaC.

Nếu chỉ quên thuật ngữ, mở **[Concept Cards](concept-cards.md)**.

---

# 6. Dấu hiệu bạn đã hiểu

Không cần thuộc định nghĩa.

Bạn hiểu **Idempotency** nếu có thể trả lời:

```text
User bấm checkout hai lần thì chuyện gì xảy ra?
Hai request chạy concurrent thì sao?
Process crash sau DB commit thì sao?
Payment provider đã charge nhưng response mất thì sao?
```

Bạn hiểu **Kubernetes readiness** nếu có thể trả lời:

```text
Pod process vẫn chạy nhưng DB migration chưa xong.
Có nên nhận traffic không?
```

Bạn hiểu **System Design** nếu có thể giải thích:

```text
Vì sao thêm Redis?
Nếu Redis chết thì sao?
Load test nào chứng minh cần Redis?
10x traffic bottleneck chuyển đi đâu?
```

---

# 7. Khi nào đọc Deep Mode

Chỉ deep dive khi một trong các điều sau xảy ra:

```text
code behavior chưa giải thích được
production incident
performance bottleneck
security review
architecture decision
interview/system-design exercise
```

Deep documentation trong repo vẫn rất quan trọng, nhưng coi nó như:

```text
reference manual
```

không phải:

```text
book phải đọc hết
```

---

# 8. Learning evidence mới

Không đánh dấu “đã học” vì đã đọc.

Evidence tốt:

```text
chạy được request
viết được SQL
thấy được execution plan
reproduce duplicate
simulate timeout
fix bằng idempotency
kill Pod và xem rollout
build queue backlog
reconcile UNKNOWN payment
vẽ architecture từ memory
```

Nếu làm được, kiến thức đã đi từ text → mental model → hành vi thật.
