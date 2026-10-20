# Case Study .NET Solution Architect — Tài chính, Loyalty, Booking và Chứng khoán

> [← Lộ trình học](solution-architect-dotnet-roadmap.md) · [Architecture Review](architecture-review-playbook.md)

**Mục tiêu:** hiểu vì sao cùng một bộ kỹ thuật SQL Transaction, Atomic Update, Unique Constraint, Idempotency, Outbox/Inbox và Reconciliation lại được triển khai khác nhau tùy nghiệp vụ. Ví dụ dùng **ASP.NET Core + EF Core + SQL Server + Azure Service Bus**.

## 1. So sánh nghiệp vụ trước khi chọn công nghệ

| Tiêu chí | Tài chính (Financial) | Điểm thưởng (Loyalty) | Đặt chỗ (Booking) | Đặt lệnh chứng khoán (OMS) |
|---|---|---|---|---|
| Tài nguyên | Tiền và sổ cái | Điểm, lô điểm, voucher | Ghế, phòng, slot | Sức mua, lệnh, khớp lệnh |
| Bất biến phải bảo vệ | Không chi vượt; ledger cân đối | Không đổi vượt điểm | Không cấp trùng slot độc quyền | Không phong tỏa tiền hai lần; không gửi lệnh trùng |
| Tính nhất quán mạnh | Ghi tiền và ledger | Trừ/giữ điểm | Giữ/confirm slot | Phong tỏa sức mua và order state |
| Có thể eventual consistency | Báo cáo, notification | Voucher/notification | Email, tìm kiếm | Portfolio projection, bảng tổng hợp |
| Trạng thái không xác định | Bank/provider timeout | Voucher provider timeout | Payment callback trễ | Exchange gateway timeout |
| Phục hồi | Đối soát giao dịch, bút toán điều chỉnh | Đối soát điểm/voucher | Kiểm tra thanh toán, refund | Tra cứu trạng thái lệnh và khớp |

**Bất biến (invariant)** là điều kiện nghiệp vụ luôn phải đúng dù có concurrency, retry hoặc crash. Không thể chọn kiến trúc trước khi xác định bất biến.

## 2. Sáu lớp bảo vệ phối hợp như thế nào?

```text
Client POST /operations + Idempotency-Key
              |
              v
ASP.NET Core: validate + payload hash
              |
              v
SQL TRANSACTION
  1. Claim unique idempotency key
  2. Atomic conditional UPDATE tài nguyên
  3. Ghi bản ghi nghiệp vụ + ledger/reservation
  4. Ghi Outbox event
  5. Ghi kết quả idempotency
COMMIT
              |
              v
BackgroundService gửi Outbox → Azure Service Bus
              |
              v
Consumer: SQL TRANSACTION [Inbox unique + business update]
              |
              v
External provider / hệ thống khác
              |
              v
Reconciliation: xác minh và sửa sai lệch
```

| Kỹ thuật | Bảo vệ điều gì? | Nếu bỏ đi |
|---|---|---|
| SQL Transaction | Các ghi trong một DB commit/rollback cùng nhau | Có tiền bị trừ nhưng không có reservation |
| Atomic Update | Chỉ ghi nếu còn đủ tài nguyên tại thời điểm cập nhật | Race condition, oversell, overspend |
| Unique Constraint | DB chặn business key trùng | Hai worker cùng tạo cùng một giao dịch |
| Idempotency | Retry cùng request không tạo tác động mới | Timeout rồi retry gây trừ tiền hai lần |
| Outbox/Inbox | Ghi nhận ý định phát event và dedup consumer | Mất event hoặc xử lý event trùng |
| Reconciliation | Phát hiện trạng thái lệch/không xác định | Giao dịch kẹt Pending vô hạn |

**Lưu ý:** Outbox thường là *at-least-once*, không bảo đảm exactly-once end-to-end. Inbox phải ghi cùng transaction với side effect nội bộ; gọi API bên ngoài cần idempotency của provider hoặc reconciliation.

## 3. Case A — Ví tiền và chuyển tiền

**Tình huống:** Tài khoản A có 1.000.000 đồng. Hai request cùng chuyển 700.000 đồng sang B.

**Sai:** đọc số dư, kiểm tra rồi ghi trong hai bước không có concurrency guard. Hai request đều có thể cho rằng còn đủ tiền.

**Đúng:** claim idempotency key; dùng conditional update; ghi debit/credit/ledger, transfer record và outbox trong cùng transaction nếu cả hai tài khoản thuộc cùng DB.

```sql
BEGIN TRAN;

UPDATE dbo.Accounts
SET AvailableBalance = AvailableBalance - @Amount
WHERE AccountId = @FromAccount
  AND AvailableBalance >= @Amount;

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK;
    THROW 50001, 'Không đủ số dư', 1;
END;

UPDATE dbo.Accounts
SET AvailableBalance = AvailableBalance + @Amount
WHERE AccountId = @ToAccount;

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK;
    THROW 50002, 'Không tìm thấy tài khoản nhận', 1;
END;

-- Cùng transaction: ledger debit/credit, transfer, outbox.
COMMIT;
```

Ví dụ rút gọn; production cần kiểm tra số tiền dương, tài khoản nguồn/đích, tiền tệ, unique transfer ID, thứ tự khóa chống deadlock, audit và ledger cân đối.

**Khi không dùng local transaction:** chuyển tiền sang ngân hàng đối tác. Không thể bao transaction SQL quanh HTTP request. Thay bằng durable workflow với `Pending → Submitted → Confirmed/Rejected/Unknown` và đối soát.

## 4. Case B — Loyalty đổi điểm lấy voucher

**Tình huống:** Khách có 1.000 điểm, gửi hai request đổi voucher 800 điểm.

**Luồng:**

1. API nhận `Idempotency-Key` và tính payload hash.
2. SQL transaction claim key, reserve 800 điểm bằng atomic update, tạo `Redemption(Pending)` và Outbox.
3. Worker phát `RedemptionRequested`.
4. Voucher consumer dùng Inbox để chống xử lý event trùng; gửi provider bằng cùng business ID nếu provider hỗ trợ idempotency.
5. Thành công thì confirm; timeout thì đánh dấu `Unknown`, query provider trước khi phát lại.
6. Không phát được voucher thì hoàn điểm bằng transaction idempotent.

```sql
UPDATE dbo.LoyaltyAccounts
SET AvailablePoints = AvailablePoints - @Points,
    ReservedPoints = ReservedPoints + @Points
WHERE CustomerId = @CustomerId
  AND AvailablePoints >= @Points;
```

**Điểm khác Financial:** điểm có thể hết hạn và được lưu theo lô. Nếu dùng quy tắc điểm hết hạn trước (FEFO), một cột tổng điểm không đủ; phải có point bucket, ledger và transaction bảo vệ các bucket.

**Lỗi production cần test:** voucher đã phát nhưng API timeout; message bị giao hai lần; hai request cùng key nhưng payload khác nhau; worker crash trước khi ghi kết quả.

## 5. Case C — Booking một ghế cuối cùng

**Tình huống:** Hai khách cùng giữ ghế A1, chỉ một khách được giữ thành công.

```sql
UPDATE dbo.Seats
SET Status = 'Held',
    HeldBy = @ReservationId,
    HoldExpiresAt = @ExpiresAt
WHERE SeatId = @SeatId
  AND Status = 'Available';
```

Nếu affected rows = 0, request không giành được ghế. SQL transaction tạo reservation + outbox cùng lần giữ ghế.

**State machine:**

```text
Available → Held(reservationId, expiresAt)
                 ├── Paid + hold vẫn hợp lệ → Confirmed
                 ├── Payment Failed → Released
                 └── Hold Expired → Released
Payment Unknown → Verify provider → Confirm hoặc Refund
```

**Không làm:** giữ SQL transaction mở 10 phút chờ khách trả tiền. Chỉ giữ trạng thái trong DB rồi commit ngay.

**Race khó:** payment callback đến đồng thời với worker nhả ghế hết hạn. Cả hai phải UPDATE có điều kiện trên `reservationId`, trạng thái và hạn giữ. Không được chỉ dựa vào Redis TTL hoặc trạng thái cache.

**Trade-off:** giữ ghế lâu tăng khả năng thanh toán nhưng làm giảm inventory khả dụng; giữ ngắn tăng turnover nhưng nhiều payment late/refund.

## 6. Case D — Securities OMS đặt lệnh mua

**Tình huống:** Tài khoản có 100 triệu sức mua, hai lệnh mua 80 triệu đồng đến đồng thời.

```text
Mobile/Web
   ↓
Trading API (Auth + Idempotency)
   ↓
Order Service (Order Pending)
   ↓
Cash Service (Reserve buying power)
   ↓
OMS (Accepted / Pending Exchange)
   ↓
Exchange Gateway (Submit stable OrderId)
   ↓
Exchange ACK / Reject / Fill / Unknown
   ↓
Ledger, positions, read models, reconciliation
```

**Điểm khác biệt quan trọng:** ACK từ gateway không đồng nghĩa lệnh đã khớp. Một lệnh có thể `New → PartiallyFilled → Filled`, bị reject, cancel hoặc amend. Khi timeout sau khi gửi lệnh, **không gửi lệnh mới với ID mới một cách mù quáng**. Phải tra cứu trạng thái bằng định danh ổn định và đối soát thông điệp sở.

**Khi nào strong consistency:** phong tỏa sức mua, order state transition và ledger trong phạm vi owner. **Khi nào eventual consistency:** báo cáo, dashboard, portfolio projection, notification.

**Cần kiểm tra:** partial fill giải phóng/điều chỉnh tiền thế nào; cancel và fill đến gần đồng thời; event bị trễ/out-of-order; gateway reconnect; recovery sau restart.

## 7. Code .NET — Atomic Update bằng EF Core

```csharp
public async Task<bool> TryReserveAsync(
    long accountId, decimal amount, CancellationToken ct)
{
    if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

    var affected = await db.CashAccounts
        .Where(x => x.AccountId == accountId &&
                    x.AvailableBalance >= amount)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.AvailableBalance,
                         x => x.AvailableBalance - amount)
            .SetProperty(x => x.ReservedBalance,
                         x => x.ReservedBalance + amount), ct);

    return affected == 1;
}
```

**Lưu ý:** đoạn code chỉ minh họa atomic update. Production phải bọc nó cùng thao tác claim idempotency key, insert reservation/ledger và outbox trong một transaction rõ ràng. Mỗi request/task sử dụng DbContext riêng. Không dùng EF Core InMemory để kết luận concurrency của SQL Server.

## 8. Ma trận lỗi production và cách chứng minh

| Tình huống giả lập | Kết quả mong đợi |
|---|---|
| 100 request tranh cùng số dư/ghế | Không vượt số dư, không bán trùng |
| Retry cùng idempotency key | Trả lại cùng business result, không trừ thêm |
| Cùng key nhưng payload khác | Báo conflict, không thực hiện |
| DB commit xong, publisher crash | Outbox còn và phát lại sau restart |
| Broker giao cùng event hai lần | Inbox/unique business key chặn tác động lặp |
| Provider xử lý thành công rồi HTTP timeout | Đánh dấu Unknown, tra cứu trạng thái |
| Booking expiry và payment callback chạy đồng thời | Chỉ một transition hợp lệ thắng |
| Consumer replay lịch sử | Không gửi lại thanh toán/voucher ngoài ý muốn |
| Database failover | Phục hồi theo RPO/RTO đã đo; đối soát giao dịch dở dang |

## 9. Câu hỏi phỏng vấn Solution Architect

1. Vì sao `lock` trong C# không bảo vệ được hai instance ASP.NET Core?
2. Vì sao Atomic Conditional Update thường phù hợp hơn RowVersion với hot balance?
3. SQL Transaction bảo vệ được những bảng nào và không bảo vệ được HTTP provider nào?
4. Tại sao cần cả Unique Constraint và Idempotency Key?
5. Outbox giải quyết dual-write như thế nào, vì sao vẫn cần Inbox?
6. Broker cam kết exactly-once trong phạm vi nào? Vì sao external side effect vẫn có thể trùng?
7. Nếu payment đã thành công nhưng booking hết hạn, hệ thống phải làm gì?
8. Nếu exchange gateway timeout, làm sao biết lệnh đã nhận?
9. Saga compensation khác rollback của SQL ở điểm nào?
10. Làm sao chứng minh không double-spend bằng integration test thay vì unit test mock?

**Tiêu chí đạt:** câu trả lời phải có invariant, transaction boundary, tình huống lỗi, trade-off và cách kiểm chứng. Không chỉ kể tên pattern.

## 10. Tài liệu đọc tiếp

- [SQL — Transactions, Isolation & Concurrency](../05-sql/transactions-isolation-and-concurrency.md)
- [Distributed Systems — Partial Failure & Idempotency](../17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md)
- [Distributed Systems — Outbox, Inbox & Dedup](../17-distributed-systems/messaging-outbox-inbox-and-dedup.md)
- [Software Architecture — Event-Driven & CQRS](event-driven-cqrs-and-integration.md)
- [System Design — Case Studies](../24-system-design/case-studies-and-design-review.md)


## 11. Các phần cần hoàn thiện trước khi coi đây là reference architecture

Các ví dụ trong tài liệu này là **mẫu thiết kế để học**, chưa phải triển khai production đã được chứng minh. Khi biến thành dự án thực hành, bổ sung tối thiểu:

### Financial Wallet

- Double-entry ledger với debit/credit cân bằng theo currency và business transaction.
- Idempotency claim/response replay, transaction boundary và unique constraints.
- Reversal/adjustment bằng bút toán mới; không sửa/xóa ledger entry đã ghi sổ.
- Reconciliation giữa ledger, payment provider/settlement và trạng thái giao dịch nội bộ.

### Loyalty

- Point bucket theo expiry date; quy tắc consume điểm sắp hết hạn trước và xử lý refund/reversal.
- Voucher issuance có idempotency và state machine; timeout provider là UNKNOWN cho đến khi query/reconcile.
- Kiểm thử đồng thời khi hai yêu cầu cùng reserve số điểm còn lại.

### Booking

- Quy tắc hết hạn hold và xử lý payment thành công đến muộn.
- Race giữa expiry worker và payment callback; một state transition phải thắng bằng conditional update/row version hoặc transaction phù hợp.
- Chính sách refund/compensation khi ghế đã release nhưng payment provider báo thành công muộn.

### Securities OMS

- State machine rõ ràng cho New/PendingNew/Accepted/PartiallyFilled/Filled/PendingCancel/Canceled/Rejected/Unknown.
- Phân biệt gateway ACK, exchange acceptance và execution fill; xử lý partial fill và cancel/replace race.
- Client order ID/idempotency, sequence-gap detection, replay và reconciliation sau reconnect.
- Buying power reservation/release theo trạng thái order/execution; không giả định HTTP success đồng nghĩa lệnh đã khớp.

### Tiêu chí nghiệm thu

- [ ] Có domain invariants và state transition table.
- [ ] Có sequence diagram cho happy path, timeout/unknown và recovery.
- [ ] Có test đồng thời trên SQL Server thật hoặc Testcontainers.
- [ ] Có duplicate, delayed callback, process restart và reconciliation tests.
- [ ] Mọi giả định nghiệp vụ và giới hạn mô phỏng được ghi rõ.
