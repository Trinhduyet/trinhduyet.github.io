# Securities Domain Architecture — bản đồ nghiệp vụ cho Solution Architect

> [← Module 25](README.md) · [Ma trận JD](solution-architect-jd-coverage.md) · [Case Studies](solution-architect-dotnet-case-studies.md) · [Template System Design](system-design-case-study-template.md)

**Mục tiêu:** phân rã hệ thống chứng khoán thành capabilities, invariants, luồng giao dịch, hợp đồng tích hợp và failure cases để thảo luận HLD/LLD. Đây là **reference learning model**, không phải thiết kế đã kiểm chứng của một công ty chứng khoán hay đặc tả giao thức sở giao dịch cụ thể. Quy định địa phương và thông số nghiệp vụ phải xác nhận với Product, Risk, Legal/Compliance và đối tác.

## 1. Business capability map và ownership

| Capability / bounded context | Trách nhiệm | Source of truth | Tích hợp quan trọng |
|---|---|---|---|
| Identity & Customer | Customer/KYC, entitlements, account ownership | Customer/Identity store | IdP, KYC provider |
| Trading Account | Trading account, permissions, account status | Account store | OMS, Risk, custody |
| Order Management (OMS) | Nhận, xác thực, chuyển trạng thái lệnh, request cancel/replace | Order journal/state | Risk, Gateway, execution events |
| Pre-trade Risk / Buying Power | Cash/security reservation, limits, order eligibility | Risk/exposure & reservation store | Cash, Margin, OMS |
| Exchange/Broker Gateway | Adapt protocol, client order mapping, sequence/ACK/replay | Durable message journal + exchange authoritative status | External exchange/broker |
| Cash & Ledger | Cash available/blocked, postings, adjustments, reconciliation | Immutable ledger + controlled balance projection | Bank/settlement, risk |
| Custody & Positions | Securities holdings, settled/unsettled positions, corporate action impacts | Position/custody records | Depository, clearing |
| Market Data | Ingest, normalize, distribute quote/trade/reference data | Licensed provider feed + local snapshot | Data vendors, websockets |
| Margin & Risk | Collateral, exposure, margin utilization, calls | Risk exposure ledger/config version | Market data, OMS, liquidation |
| Portfolio & Reporting | Holdings/P&L views, statements, reporting | Derived projections with source references | Ledger/positions/prices |
| Investment Products | Funds, bonds, structured products lifecycle | Product-specific books | Fund manager/registrar/custodian |

Một bounded context không bắt buộc là microservice. Với đội nhỏ, một modular monolith gồm OMS/Risk/Cash/Portfolio có module ownership và transaction rõ có thể là bước đầu hợp lý. Tách độc lập khi có yêu cầu scale, release cadence, compliance hoặc blast-radius có evidence.

## 2. FR và NFR mẫu (giả định, phải xác nhận)

| ID | Requirement | Acceptance / invariant |
|---|---|---|
| FR-01 | Khách đặt lệnh mua/bán | Authz account, instrument/session eligibility, idempotency key; trả `orderId` và trạng thái tiếp nhận |
| FR-02 | Xem/hủy/sửa lệnh | Cancel/replace là **request**, kết quả chỉ xác nhận khi nhận phản hồi có thẩm quyền; xử lý cancel/fill race |
| FR-03 | Xem sức mua/tài sản | Phân biệt available, reserved, unsettled; bảo vệ tenant/account authorization |
| FR-04 | Cập nhật thị trường realtime | Client phát hiện stale quote, reconnect, snapshot + delta sequence |
| FR-05 | Đối soát lệnh/tiền/vị thế | Mismatch có owner, severity, quy trình xử lý và audit |
| NFR-01 | Latency p95/p99 | Mục tiêu, điều kiện tải và ngân sách latency phải do business/market operation chấp thuận |
| NFR-02 | Availability & recovery | SLO, RTO, RPO theo từng capability; failover test thay vì chỉ cấu hình HA |
| NFR-03 | Correctness | Không vượt hạn mức/sức mua, không ghi ledger mất cân đối, xử lý duplicate/out-of-order |
| NFR-04 | Security & audit | MFA/strong auth theo chính sách; least privilege; immutable audit với correlation IDs |
| NFR-05 | Capacity | Peak order rate, market-data fanout, burst tại mở/đóng phiên; đo queue lag, DB IO, connections |
| NFR-06 | Data governance | Retention, classification, entitlements dữ liệu thị trường, PII và audit theo chính sách đã duyệt |

Không gắn số liệu SLA hoặc business rule giả định như một cam kết đã được xác minh.

## 3. Entity model tối thiểu

| Entity | Khóa chính / liên kết | Invariant và ownership |
|---|---|---|
| TradingAccount | accountId, customerId | Chỉ chủ thể có quyền mới thao tác; status/permissions versioned |
| Order | orderId, accountId, instrumentId, clientOrderId | Unique idempotency scope; qty/price validated; trạng thái theo transition |
| OrderEvent / Execution | eventId, orderId, exchangeExecutionId | Dedup external event; cumulativeFilledQty <= orderQty; không apply fill hai lần |
| CashReservation | reservationId, orderId, accountId, amount | Không reserve vượt available buying power theo rule đã duyệt |
| LedgerEntry | entryId, postingGroupId, accountId, currency, debit/credit | Balanced postings trong cùng currency/group; reversal bằng entries mới |
| PositionLot | accountId, instrumentId, lotId | Quantity/settlement status traceable; trade-date vs settled distinction |
| MarketQuote | instrumentId, feedId, sequence, asOf | Snapshot có độ mới và feed provenance; không mặc định authoritative cho pre-trade |
| MarginExposure | accountId, collateralRuleVersion, exposure | Policy/risk source versioned; margin calculations auditable |
| CorporateAction | actionId, instrumentId, effectiveDate | Idempotent event application, effects reconciled |
| ReconciliationCase | caseId, source, discrepancyType | Track mismatch, resolution, owner và supporting evidence |

Ví dụ dữ liệu trên chỉ là conceptual model. Khi làm LLD cần precision/currency, PK/FK, unique/index, isolation, transaction, retention, access pattern và schema migration.

## 4. Order state machine và exchange semantics

| From | Trigger / condition | To |
|---|---|---|
| PendingValidation | Validation/risk reject | Rejected |
| PendingValidation | Risk reserved | PendingSubmit |
| PendingSubmit | Exchange accepted | Working |
| PendingSubmit | Gateway timeout, venue state unknown | Unknown |
| Unknown | Status reconciliation confirms acceptance/reject | Working / Rejected |
| Working | Execution for part/all quantity | PartiallyFilled / Filled |
| PartiallyFilled | Remaining quantity executed | Filled |
| Working / PartiallyFilled | Cancel requested | PendingCancel |
| PendingCancel | Venue confirms cancel | Canceled |
| PendingCancel | Additional executions arrive | PartiallyFilled / Filled |

Đây là sơ đồ tối giản, không bao phủ toàn bộ trạng thái protocol. **Gateway ACK ≠ exchange acceptance ≠ execution fill.** Không tự phát lại với ID mới khi kết quả gửi lệnh chưa xác định. Lưu client/exchange IDs, inbound/outbound sequence, raw payload có kiểm soát, timestamp và correlation chain. Quy tắc sửa/hủy, thời hạn và valid transitions phụ thuộc venue/order type.

### Luồng đặt lệnh tham khảo

1. API xác thực user, account entitlement, request schema và idempotency key + payload hash.
2. OMS tạo intent/operation; Risk kiểm tra limit và reserve có atomic transaction trong source of truth liên quan. Nếu crossing service boundary, phải thiết kế durable workflow và phân biệt trạng thái pending/unknown.
3. Ghi order transition + outbox trong cùng transaction với phần dữ liệu OMS sở hữu.
4. Gateway phát request với stable external client order ID và durable send journal; sau timeout không suy đoán thất bại.
5. Exchange events vào inbox/dedup, validate sequence, kiểm soát trạng thái; fill cập nhật order/ledger/position qua workflow nhất quán.
6. Đối soát exchange execution, reservation, ledger, position và provider/bank settlement; cảnh báo lệch.

| Bước | Sender → Receiver | Contract / observable state |
|---|---|---|
| 1 | Client → OMS API | `POST order` + idempotency key |
| 2 | OMS → Risk/Cash | Check & reserve; return reservation reference |
| 3 | OMS → Client | Return `orderId` + pending status |
| 4 | OMS → Gateway → Venue | Durable submit using stable external client ID |
| 5 | Venue → Gateway → OMS | ACK/reject/fills, dedup + sequence validation |
| 6 | Client → OMS | Query latest order state; no assumption that ACK is fill |

Không giả định atomic transaction giữa OMS DB, Risk DB và venue. Chỉ khi cùng data owner/DB mới có thể gom các bảng vào một local transaction.

## 5. Những domain nghiệp vụ cần học vượt OMS

| Domain | Bài toán Solution Architect phải đặt câu hỏi | Failure/consistency trọng tâm |
|---|---|---|
| Cash / settlement | Available vs blocked vs settled? Posting finality? Bank cut-off? | External settlement UNKNOWN, reversal, ledger reconciliation |
| Account / custody | Ai sở hữu chứng khoán? Trade date/settlement date? Corporate action? | Duplicate custody event, late settlement, ownership |
| Margin | Initial/maintenance margin? Collateral haircut? Stress/market move? | Stale prices, exposure drift, concurrent liquidation/order |
| Derivatives | Contract multiplier, expiry, mark-to-market, variation margin? | Risk recalculation under volatility; clearing/position reconciliation |
| Market data | Licensed entitlements, snapshot/delta, gap detection, backfill? | Out-of-order ticks, stale quotes, burst backpressure |
| Portfolio | P&L realized/unrealized, cost basis, tax/corporate action? | Derived read model stale, rebuild and provenance |
| Funds | NAV cut-off, subscription/redemption confirmation, settlement? | Async accepted vs allotted, NAV restatement |
| Bonds | Coupon/accrual, maturity, settlement calendar, corporate actions? | Calendar/rule version, delayed external confirmations |

Đối với Margin/phái sinh/quỹ/trái phiếu, nội dung trên là **bản đồ câu hỏi và rủi ro**, chưa thay thế giáo trình product mechanics và regulations.

## 6. Integration, security và deployment

- **Gateway/EDA:** versioned API/event schema, contract ownership, timeout budget, retry only when idempotent, poison message/DLQ, message replay authorization; certify protocol against external partner test environment.
- **Security:** OAuth2/OIDC for appropriate client identities, service-to-service mTLS/identity where supported, account-level authorization, token scope, secret rotation, network segmentation, signed callbacks, maker-checker for sensitive operations where required.
- **Architecture topology:** on-prem or hybrid connectivity, network latency/jitter, link redundancy, secure egress, DNS/certificate renewal, external venue operational windows; no claim multi-region active-active without data/sequence design.
- **Observability:** trace `requestId → orderId → clientOrderId → exchangeExecutionId`; metrics for rejects, unknown orders, event sequence gaps, queue lag, risk reservation mismatch, reconciliation aged cases.
- **DR:** test restoring order journal, ledger, inbox/outbox and gateway sequences together; reconcile external authoritative state before resuming command processing.

## 7. Trade-off/ADR gợi ý

| Decision | Option A | Option B | Evidence cần có |
|---|---|---|---|
| OMS modular monolith vs service split | Local transaction, vận hành đơn giản | Independent scaling, separate failure boundary | Team ownership, peak workload, deployment/incident cost |
| Sync gateway vs durable async submit | Immediate path nhưng phụ thuộc venue latency | Durable retry/reconcile nhưng eventual status | Venue protocol, p99, unknown/recovery simulation |
| SQL vs cache cho buying power | Authoritative transactional store | Cache cho read-only projection | Concurrency tests, stale-read risk |
| Market data distribution | Direct fanout | Broker/stream + scaled gateways | Subscriber count, messages/s, reconnect burst, cost |
| DB vendor | SQL Server | PostgreSQL/Oracle/MongoDB theo access pattern | Isolation/locking semantics, queries, operations, licensing, POC |

## 8. Bài thực hành tích hợp đề xuất: OMS Reference Lab

**Bộ source chưa tồn tại trong phạm vi tài liệu này; dưới đây là acceptance contract để triển khai.**

- ASP.NET Core API: `POST /orders`, `POST /orders/{id}/cancel`, `GET /orders/{id}`, `GET /accounts/{id}/buying-power`; account-level authorization.
- DB thật (SQL Server ban đầu): orders, order events, cash reservations, ledger entries, outbox/inbox, reconciliation cases; migrations/index/constraints.
- Venue simulator: ACK, reject, partial fill, fill-before-cancel, out-of-order, duplicate, timeout-after-accept, disconnect/reconnect.
- Integration tests: 100 concurrent orders không vượt buying power; retry cùng key không tạo lệnh thứ hai; publisher crash sau commit; duplicate fills không tăng position/cash hai lần.
- Load/ops: order acceptance p95/p99, burst market feed, queue age, lock waits, trace correlation, rollback, backup/restore và RTO/RPO measured.
- Evidence: scripts, versions, test logs, environment/workload, failed-case reproduction, recovery outcome và ADR. Không gắn nhãn production-ready nếu chưa đạt.

## 9. Câu hỏi review

1. Nếu gateway timeout sau khi venue đã nhận lệnh, ta phục hồi ra sao mà không gửi lệnh đôi?
2. Khi partial fill và cancel ACK đến chéo thứ tự, order state và reservation có còn đúng?
3. Quyết định sức mua dựa vào dữ liệu nào; cache stale được phép ở đâu?
4. Nếu feed gap 500 sequence, UI và risk engine phải xử lý khác nhau thế nào?
5. Ledger, order journal và custody positions lệch sau failover: ai là authoritative và đối soát theo thứ tự nào?
6. Quyết định split OMS/Risk/Cash thành services có giảm hay tăng rủi ro hiện tại?
7. Khi nào phải chặn giao dịch thay vì degrade sang quote cũ?
8. Tài liệu và bằng chứng nào đủ để Product/Risk/Security/DBA/DevOps phê duyệt go-live?
