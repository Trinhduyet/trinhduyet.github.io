# Payment, Loyalty & Fintech — mô phỏng system design

Những bài dưới đây là **case giả định**, không mô tả kiến trúc nội bộ LynkiD, ngân hàng, payment gateway hoặc dự án riêng tư.

## Case: 100.000 điểm, hai yêu cầu đổi 80.000 điểm

Nếu chỉ đọc balance ở C# rồi gọi `UPDATE`, hai request có thể cùng thấy đủ điểm. Dùng transaction + conditional update và affected rows, insert ledger cùng transaction, áp unique idempotency key. Test với hai database connections thật để chứng minh invariant.

## Provider timeout

Một timeout có thể xảy ra **sau khi provider xử lý**. Không tự coi payment thất bại, không retry với key mới vô điều kiện. Giữ stable merchant reference, trạng thái PENDING/UNKNOWN, query provider status, xử lý callback có xác thực và reconciliation.

## Các câu hỏi

- Payment authorization, capture và settlement khác nhau ra sao?
- Webhook signature đúng có đủ? Cần replay protection, business validation và state transition.
- Duplicate callback, out-of-order delivery và timeout được xử lý ở đâu?
- Tại sao outbox không tự bảo đảm exactly-once bên external provider?
- Loyalty points khác balance tiền pháp định thế nào về correctness/compliance?

[Transactions](../../05-sql/transactions-isolation-and-concurrency.md) · [Saga & UNKNOWN](../../18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md).
