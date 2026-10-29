# Solution Architect Practice — 7 System Design Cases & Evidence Rubric

> [← Module 25](README.md) · [Business → HLD/LLD](solution-architecture-from-requirements-to-design.md) · [System Design Template](system-design-case-study-template.md) · [Mandatory Labs](solution-architect-mandatory-labs.md)

Đây là **ngân hàng đề luyện kiến thức chung**. Financial và Securities chỉ là hai trong nhiều domain. Mỗi case dùng cùng framework: problem/scope → FR → measurable NFR → capacity → APIs → entities/invariants → HLD → deep dives → failure/security/operations → ADR → implementation/tests/evidence.

## 1. Roadmap kiến thức gắn với thực hành

| Phase | Chủ đề chung | Kết quả đầu ra |
|---|---|---|
| P0 Foundation | CS/Networking, .NET, design patterns, DDD, SQL, concurrency | Reason about runtime bottlenecks, domain boundaries, ACID/invariants |
| P0 Distributed | API, messaging, CQRS, Saga, Outbox/Inbox, idempotency, cache | Review partial failure, replay, consistency, sync/async architecture |
| P1 Production | Cloud/hybrid, Docker/K8s/CI/CD, security, HA/DR, observability, capacity/cost | HLD/LLD, risk model, runbook, load/failover/restore evidence |
| P1 Leadership | Requirement discovery, ADR, review, governance, POC, mentoring | Defend trade-off, lead multi-team review, control deviations |
| P2 Advanced | Multi-region, multi-tenancy at scale, data streaming, specialized domain | Justify selective advanced patterns with evidence |

## 2. Case catalog — không mặc định một tech stack

| Case | Main FR | Hard NFR/invariant | APIs/entities | Architecture deep dives |
|---|---|---|---|---|
| A. Payment/Wallet | Transfer, payout, refund, transaction status | No double posting, audit, recovery | `POST /transfers`, Account, Ledger, Transfer | Atomic transaction, double entry, idempotency, provider timeout/reconciliation |
| B. Online Auction | Create auction, place bid, close auction | One winning bid under concurrency, real-time fanout | `POST /auctions/{id}/bids`, Auction, Bid | Atomic bid acceptance, close race, WebSocket, cache freshness |
| C. Chat/Messaging | Send/read messages, presence, offline delivery | Ordering scope, reconnect/replay, backpressure | `POST /conversations/{id}/messages`, Message, Receipt | Partition key, fanout, delivered/read semantics, dedup, retention |
| D. File Storage | Multipart upload, share, download, sync | Integrity, resumability, tenant security | UploadSession, FileMetadata, FileVersion | Presigned URL, checksum, orphan cleanup, versioning, CDN |
| E. Booking/Inventory | Hold resource, pay, cancel | No oversell; late callback consistency | `POST /holds`, Reservation, Inventory | Race expiry/payment, conditional update, saga, compensation |
| F. Financial Aggregator | Link accounts, sync transactions, insights | Token safety, eventual freshness, partner API limits | BankConnection, SyncCursor, Transaction | Incremental sync, API rate limits, dedup, stale result labeling |
| G. Securities/Trading | Order, cancel/replace, positions, market data | Buying power, order transitions, gateway UNKNOWN | Order, Execution, CashReservation, Quote | Partial fill, sequence gap, risk, reconciliation; [domain reference](securities-domain-architecture.md) |

## 3. Case A — Payment/Wallet: câu hỏi kiến trúc

**FR:** tạo transfer, xem trạng thái, reversal/refund theo quyền, reconciliation. **NFR:** 0 logical duplicate posting, security/audit, RPO/RTO dựa criticality. **Core entities:** Transfer, Account, LedgerEntry, IdempotencyRecord, Outbox, ReconciliationCase.

Deliver:

- Strong local debit/credit transaction với double-entry constraints; partner payout qua durable workflow.
- HTTP contract `202 Accepted` khi external confirmation chưa rõ; status polling/webhook signatures.
- Failure test provider commits rồi timeout, request duplicate, worker crash, ledger mismatch.
- ADR: modular monolith vs split ledger/payment; choice of DB and broker.

## 4. Case B — Online Auction

**FR:** bids, auction close, winner notification. **NFR:** không chọn hai winner do race, p99 bid acceptance theo peak profile. **Entities:** Auction, Bid, AuctionClose, User, Outbox.

- Atomic conditional bid acceptance (price, version, close time) vs serial processing per auction key.
- Redis/WebSocket phục vụ realtime view, không thay source of truth.
- Test bid vs close race, highest bid tie-breaker, duplicate bids và user disconnected.

## 5. Case C — Chat/Messaging

**FR:** conversation, send, history, delivery/read, offline reconnect. **NFR:** ordered per conversation theo contract, bounded lag, privacy/retention. **Entities:** Conversation, Membership, Message, Receipt, DeviceSession.

- Client-generated message ID, server sequence per conversation, dedup on reconnect.
- Presence là ephemeral state (TTL), không đồng nhất với delivery/read receipt.
- Design message persistence + pub/sub fanout + replay by cursor + gap detection.
- Test 1000 subscribers, slow consumer, reconnect after outage, device duplicate.

## 6. Case D — File Storage

**FR:** create upload session, upload parts, finalize, share permissions, sync changes. **NFR:** checksum integrity, durable metadata, owner-level authz. **Entities:** File, FileVersion, UploadSession, BlobReference, ShareGrant.

- Bytes → object storage; metadata → transactional DB; upload `pending/verified/committed` state.
- Multipart, checksum, dedup/privacy, orphan cleanup và legal retention.
- Test interrupted upload, concurrent overwrite, shared link expiry, poisoned metadata.

## 7. Case E — Booking/Inventory

**FR:** search, hold slot, pay, cancel. **NFR:** không overbook, hold expiry policy, clear UI unknown. **Entities:** InventorySlot, Reservation, PaymentAttempt, Outbox.

- Atomic slot claim + expiration version; no SQL transaction held open waiting for payment.
- Late successful callback khi hold expired → explicit confirm or refund policy agreed by Product.
- Test expiry worker vs callback simultaneous, 100 concurrent claims, replay webhook.

## 8. Case F — Financial Aggregator

**FR:** link bank, refresh, show aggregated transactions, budgets. **NFR:** freshness transparency, token security, API rate limits. **Entities:** Connection, ProviderTokenReference, SyncCheckpoint, Transaction, SyncRun.

- Secure token lifecycle, provider-specific throttling and cursor pagination.
- Incremental sync with stable external transaction IDs, late/corrected entries, dedup.
- Test expired token, provider downtime, duplicate page, cursor invalidation, reconciliation gaps.

## 9. Case G — Securities/Trading

**FR:** submit/cancel order, status, positions, real-time quote. **NFR:** risk invariant, sequence/fill correctness, audit, failover. **Entities:** Order, Execution, RiskReservation, Position, Ledger, MarketQuote.

- See [Securities Domain Architecture](securities-domain-architecture.md) for specialized vocabulary and order state.
- Đây là case ngành cụ thể, không phải framework bắt buộc áp dụng cho mọi sản phẩm.

## 10. Deliverables chung cho mỗi case

| Artifact | Yêu cầu tối thiểu |
|---|---|
| Business/FR | Actors, scope, 6+ FR, 3+ negative/exception flows, acceptance criteria |
| NFR/capacity | 6+ measurable attributes, assumptions, peak RPS, data growth, cost drivers |
| API/events | 4+ endpoints hoặc contract tương đương, auth, idempotency, versioning, errors |
| Entities | Source of truth, invariant, unique/index, tenant/PII, retention |
| HLD/LLD | Context, component/sequence, data ownership, transaction boundary, deployment |
| Failure/security | 6+ failure scenarios, recovery/reconciliation, threat boundaries, authz tests |
| ADR | 2+ credible options, rationale, downside, evidence and revisit trigger |
| Implementation | Reproducible .NET or equivalent source, real DB/broker where semantics matter |
| Verification | Concurrency, contract, integration, load, failure, rollback/DR evidence |
| Leadership | Review packet, stakeholder decisions, technical risks, runbook/ownership |

## 11. Evidence maturity ladder

| Level | Meaning | Không được tự nhận |
|---|---|---|
| L0 — Mentioned | Đã liệt kê khái niệm | Hiểu sâu hoặc triển khai được |
| L1 — Explained | Có principles, example, trade-off và failure analysis | Code chạy hay production behavior |
| L2 — Designed | FR/NFR/HLD/LLD/ADR và review artifacts | Đã chứng minh transaction/race/DR |
| L3 — Verified in lab | Reproducible source + actual integration/load/failure evidence | Đã hoạt động production |
| L4 — Operated | Có production SLO, incidents, operational metrics, approved reviews | Áp dụng được cho mọi domain mà không cần constraint review |

Tài liệu trong Module 25 **không tự động đạt L3/L4**. Một design review chỉ đánh giá chất lượng artifacts, không thay 2+ năm kinh nghiệm thực tế.

## 12. Self-assessment rubric (100 điểm, mẫu)

| Năng lực | Điểm | Bằng chứng |
|---|---:|---|
| Business discovery, capability, FR/NFR | 15 | Traceability, signed assumptions |
| Architecture boundaries and trade-offs | 15 | HLD, ADR alternatives, cost/team impact |
| Data/transactions/concurrency | 15 | Schema/invariants, database tests |
| Distributed/event-driven/recovery | 15 | Outbox/Inbox/Saga/replay failure tests |
| API/integration/security | 15 | Contracts, threat model, negative tests |
| Performance/HA/DR/observability | 15 | Load, failover/restore, traces, SLO |
| Governance/mentoring/operations | 10 | Review findings, risk/debt, runbooks, knowledge transfer |

**Interpretation gợi ý:** ≥80/100 với không nhóm nào dưới 50% là design review pass; L3 evidence chỉ được công nhận khi test thật. Rubric là công cụ đào tạo, không phải chứng nhận nghề nghiệp.

## 13. Next implementation backlog — ưu tiên bằng evidence

1. L05/L06/L08 domain-neutral: reserve resource, outbox/inbox, broker/DLQ/replay, provider UNKNOWN/reconciliation.
2. L04/L09: SQL Server vs PostgreSQL query/isolation POC, MongoDB document model, cache stale experiment.
3. L10–L14: load/failover, tenant authz, hybrid deployment, tracing, restore/RTO/RPO drills.
4. One complete capstone selected by learner (any case A–G) with FR/NFR → design → code → tests → runbooks → ADR.

Không đánh dấu backlog là “done” chỉ vì có đề bài. Đối chiếu [Mandatory Labs](solution-architect-mandatory-labs.md) để biết pass/fail và yêu cầu evidence.
