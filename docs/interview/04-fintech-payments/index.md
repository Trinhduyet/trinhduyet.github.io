# Payment & Fintech — case study phỏng vấn .NET

<div class="lesson-meta"><span>Domain case study</span><span>Proposed, không gắn với doanh nghiệp</span><span>Transactions · Concurrency · APIs</span></div>

Bộ tình huống giúp luyện **tính đúng đắn của giao dịch**, không phải mô tả hệ thống ngân hàng hoặc đối tác cụ thể.

## Bắt đầu từ bài toán

Ví điện tử/loyalty có 100.000 điểm. Hai request cùng lúc muốn trừ 80.000. Nếu code đọc balance rồi UPDATE không có synchronization, cả hai có thể thấy đủ điểm. Cần bảo vệ invariant ở database bằng transaction + conditional debit, kiểm tra affected rows, ledger record và unique idempotency key.

[Đáp án SQL & transaction](../05-interview-preparation/sql-data.md#db-05-hai-request-tru-cung-so-du-xu-ly-the-nao) · [Database isolation](../../05-sql/transactions-isolation-and-concurrency.md)

## Payment Gateway không phải chỉ HTTP 200

Provider nhận create/capture rồi connection timeout. Caller **không biết chắc** external side effect đã xảy ra chưa; ghi nhận trạng thái PENDING/UNKNOWN, giữ reference ổn định, query provider status và reconcile theo contract. Callback phải được xác thực và dedupe; acceptance khác settlement.

[Distributed reliability](../05-interview-preparation/distributed-cloud.md) · [Checkout saga](../../18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md)

## 5 câu để tự trả lời

1. Điểm thưởng và tiền gửi khác nhau thế nào về nghiệp vụ và compliance?
2. Ledger append-only và materialized balance có invariant gì?
3. Timeout khi gọi provider có nên retry vô điều kiện?
4. Outbox giải quyết dual-write đến broker, còn gì chưa đảm bảo?
5. Ở multi-tenant program, client-supplied WalletId có đủ để authorize?

**Tiếp theo:** [Câu hỏi Architecture](../05-interview-preparation/architecture-leadership.md) · [System Design exercises](../09-solution-architecture-leadership/design-exercises.md).
