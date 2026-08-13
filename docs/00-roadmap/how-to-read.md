# Cách đọc tài liệu này mà không bị ngợp

> **Không đọc repository này như một cuốn sách.** Phần deep documentation dài vì nó còn phục vụ debug, review và architecture. Để học, bắt đầu bằng ví dụ.

## Chọn 1 trong 3 mode

| Mode | Dùng khi | Bắt đầu |
|---|---|---|
| **Human Mode** | muốn hiểu/nhớ nhanh | [Human Learning Mode](human-learning-mode.md) |
| **Example-First** | muốn học bằng code và một hệ thống xuyên suốt | [Checkout Learning Path](example-first-learning-path.md) |
| **Deep Mode** | đang debug/review/ra quyết định | các module chi tiết |

Nếu bạn đã từng đọc hết nhiều chapter nhưng sau đó khó kể lại, hãy **dừng Deep Mode** và quay về Example-First.

---

# Cách học 60 phút

```text
20 phút chạy code
20 phút cố tình làm hỏng
20 phút đọc đúng phần lý thuyết giải thích failure vừa thấy
```

Không đọc Internals trước khi đã thấy behavior.

Ví dụ:

```text
payment timeout
      ↓
order status nên là gì?
      ↓
UNKNOWN
      ↓
đọc idempotency + reconciliation
```

Thay vì:

```text
đọc Retry
đọc Saga
đọc CAP
đọc Eventual Consistency
đọc Outbox
→ chưa biết dùng ở đâu
```

---

# Một chapter chỉ cần học 4 lớp

## Lớp 1 — Problem

Ví dụ:

> User bấm Checkout hai lần.

## Lớp 2 — Code

```http
POST /v1/checkouts
Idempotency-Key: checkout-001
```

## Lớp 3 — Failure

```text
2 requests chạy concurrent
```

## Lớp 4 — Rule cần nhớ

```text
application check alone is not enough
→ database UNIQUE invariant
```

Nếu đã nắm 4 lớp này, bạn có thể dừng. Internals là lớp sau.

---

# 5 câu hỏi khi đọc code

Không cố nhớ syntax. Hỏi:

1. **Input là gì?**
2. **State nào đổi?**
3. **Side effect ở đâu?**
4. **Chạy hai lần thì sao?**
5. **Crash giữa chừng thì sao?**

Ví dụ:

```csharp
await payment.ChargeAsync(orderId, amount, cancellationToken);
```

Một người mới nhìn thấy HTTP call.

Một người production-oriented hỏi thêm:

```text
idempotency key đâu?
timeout có phải failure không?
provider đã commit nhưng response mất thì sao?
query status bằng identifier nào?
retry có double charge không?
```

---

# Dùng Concept Cards khi quên thuật ngữ

Không cần mở chapter 1,000 dòng để nhớ `Outbox` là gì.

Mở [Concept Cards](concept-cards.md).

Mỗi card chỉ có:

```text
1 câu dễ nhớ
1 ví dụ
1 hiểu lầm thường gặp
```

Ví dụ:

> **Outbox:** ghi business state và message-to-publish trong cùng local DB transaction.

Sau đó nếu đang implement thật mới mở deep chapter.

---

# Reference system dùng xuyên suốt

Repository sẽ cố gắng dùng lại cùng một domain:

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

Nhờ vậy:

```text
Idempotency
Transaction
Cache
Queue
Outbox
Saga
Kubernetes
System Design
AI Tool Calling
```

không còn là 9 ví dụ rời rạc.

---

# Khi nào đọc Deep Mode

Deep dive khi có lý do cụ thể:

```text
production incident
performance bottleneck
security review
architecture decision
system-design exercise
API/framework behavior chưa giải thích được
```

Lúc đó đọc theo thứ tự:

```text
Problem
→ Minimal example
→ Failure
→ Internals
→ Performance/Security
→ Operations
→ Architecture trade-off
```

Không cần đọc toàn bộ mục nếu câu hỏi đã được trả lời.

---

# Dấu hiệu bạn đã hiểu

## SQL

Không phải:

```text
thuộc clustered/nonclustered index
```

Mà:

```text
nhìn query
→ xem generated SQL
→ đọc execution plan
→ giải thích vì sao scan/seek
→ đo IO trước/sau
```

## Distributed Systems

Không phải:

```text
thuộc Saga definition
```

Mà:

```text
payment timeout
→ biết FAILED != UNKNOWN
→ lưu state durable
→ reconcile
→ compensate nếu cần
```

## Kubernetes

Không phải:

```text
thuộc YAML
```

Mà:

```text
process alive nhưng app chưa sẵn sàng
→ liveness pass
→ readiness fail
→ traffic không route vào Pod
```

## System Design

Không phải:

```text
vẽ Redis + Kafka + K8s
```

Mà:

```text
requirement + NFR
→ capacity math
→ bottleneck
→ simplest design
→ failure/cost
→ evidence
```

---

# Learning evidence

“Đã đọc” không phải evidence.

Evidence tốt:

```text
chạy request
viết SQL
thấy execution plan
reproduce duplicate
simulate timeout
kill Pod
build queue backlog
reconcile unknown payment
viết regression test
vẽ lại flow từ memory
```

Nếu bạn làm được mà không nhìn đáp án, kiến thức đã đi từ **text → mental model → skill**.

---

# Bắt đầu ngay

Nếu chỉ chọn một trang tiếp theo:

**[Example-First Learning Path — Checkout từ C# đến System Design và AI](example-first-learning-path.md)**.
