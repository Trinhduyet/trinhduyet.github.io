# .NET Solution Architect — Practical System Design Cases

> [Competency map](solution-architect-dotnet-roadmap.md) · [Architecture Review Playbook](architecture-review-playbook.md)

## 1. Four cases, different business invariants

| Domain | Resource | Invariant | Main failure to design for | Recovery |
|---|---|---|---|---|
| Financial Wallet / Transfer | Money / ledger | Never spend above available; balanced ledger | Debit committed, external payout unknown | Query provider + reconciliation, reversal entries when appropriate |
| Loyalty Redemption | Points / buckets / voucher | Never redeem points twice; expiry respected | Points reserved, voucher provider timed out | Verify voucher, issue or release points idempotently |
| Booking / Ticketing | Seat, room, timeslot | One active exclusive allocation per capacity unit | Payment succeeds after hold expiry | Verify ownership before confirmation; refund or alternative |
| Securities Order Management | Buying power, order, execution | No double reservation; no duplicate exchange submission | Gateway timed out after sending order | Order ID correlation, exchange status reconciliation |

**Local atomicity** and **distributed convergence** are different goals. SQL Transaction + Atomic Update + Unique Constraint guard local state; idempotency and Outbox/Inbox reduce duplicate/lost effects; reconciliation resolves unknown or divergent external states.

## 2. Financial: reserve funds in SQL Server

Example assumes `CashAccounts(AccountId, AvailableBalance, ReservedBalance)`; add real ledger records and reservation uniqueness within the **same** database transaction:

```sql
BEGIN TRAN;
UPDATE dbo.CashAccounts
SET AvailableBalance = AvailableBalance - @Amount,
    ReservedBalance = ReservedBalance + @Amount
WHERE AccountId = @AccountId
  AND AvailableBalance >= @Amount;
IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK;
    THROW 50001, 'Insufficient buying power', 1;
END;
INSERT dbo.CashReservations(ReservationId, AccountId, Amount)
VALUES (@ReservationId, @AccountId, @Amount);
INSERT dbo.OutboxMessages(EventId, EventType, AggregateId)
VALUES (@EventId, 'FundsReserved', @ReservationId);
COMMIT;
```

Prerequisites: `@Amount > 0`; unique `ReservationId` and idempotency scope; SQL decimal fixed precision; ledger integrity and audit rules. Claim idempotency key transactionally *before* any update, handle constraint violation by reading original committed result. Unique constraint alone is not equivalent to idempotency handling.

## 3. Loyalty: reserve points then issue voucher

```text
Redeem request (customer + idempotency key)
  → SQL TX: claim key, conditional reserve, redemption(Pending), outbox
  → worker publishes event
  → voucher consumer: inbox + durable issue state
  → provider invocation with provider idempotency key
  → confirmed / unknown / failed
  → scheduled reconciliation
```

Points can expire and carry different acquisition/expiry dates. For FEFO point buckets, single `AvailablePoints` column may not be sufficient. Design reserve/release operations per bucket inside a transaction, plus point ledger and invariant tests.

## 4. Booking: hold slot with expiry and payment saga

```text
Available --atomic hold--> Held(expiresAt, reservationId)
Held --paid and still owned--> Confirmed
Held --expire/fail--> Released
Payment result unknown --> Reconcile payment provider
Late payment --> Refund/alternative, never sell same seat twice
```

For a single seat in SQL:

```sql
UPDATE dbo.Seats
SET State = 'Held', HeldBy = @ReservationId,
    HoldExpiresAt = @ExpiresAt
WHERE SeatId = @SeatId AND State = 'Available';
```

A reaper may release expired holds transactionally, with a predicate checking current reservation and expiry. Confirmation must also compare the current `HeldBy` and valid state; do not trust the cache or only the payment callback. Multi-seat booking needs all-or-nothing reservation in the local DB transaction or explicit partial-allocation semantics.

## 5. Securities: Order API → Cash → OMS → Exchange Gateway

```text
Client --idempotent request--> Trading API
                  → order intent (Pending)
                  → cash owner reserves buying power
                  → OMS submits with stable business/exchange identifiers
                  → gateway/exchange acknowledgment or UNKNOWN
                  → execution/fills update order and ledger
                  → portfolio/read models converge asynchronously
```

Key questions: Who owns buying power? Does order submission require reservation confirmed? Which identifier prevents duplicate exchange submissions? How are amend/cancel races resolved? How do partial fills affect reserved amount? How are trade corrections and settlement reconciled? Do not assume exchange exactly-once behavior without a documented contract.

## 6. .NET components and tests

| Concern | Implementation candidate | Required proof |
|---|---|---|
| HTTP + auth | ASP.NET Core + policies | Tenant isolation and idempotent retry |
| Local state | EF Core + SQL Server/PostgreSQL | Real-DB concurrency / deadlock tests |
| Atomic balance | ExecuteUpdate / parametrized SQL | 100–1000 requests cannot overspend |
| Outbox | DB table + BackgroundService relay | Crash after commit before publish |
| Inbox | Unique `(Consumer, MessageId)` + business TX | Duplicate and concurrent delivery |
| Transport | Azure Service Bus / Kafka per semantics | DLQ/replay/ordering behavior |
| Workflow | Explicit state machine / durable workflow | Timeout=Unknown, resume after restart |
| Telemetry | OpenTelemetry + audit + domain metrics | Trace one end-to-end transaction |
| Load testing | k6 / NBomber | p95/p99, lock wait, queue age, saturation |
| Recovery | Reconciliation worker + alerts | Safe resolution without double side effects |

## 7. Failure injection matrix

| Injection | Expected result |
|---|---|
| 100 concurrent requests for 1 balance/seat | Only permitted allocations commit |
| Duplicate idempotency key with same payload | One business effect, response/status reused |
| Same key with different payload | Conflict, no additional effect |
| DB commit then publisher crash | Outbox eventually published after restart |
| Broker delivers same event twice | Inbox/business uniqueness prevents repeated effect |
| External timeout after remote success | UNKNOWN until verified; no blind new submission |
| Worker dies mid-saga | Durable state resumes / reconciles |
| Confirmation races hold expiry | One outcome wins under authoritative DB condition |
| Replayed event | External side effects guarded or replay suppressed |
| DR failover | Restore within measured RPO/RTO; reconcile in-flight external actions |

## 8. Architecture decision checklist

1. Decide which invariants require strong consistency; which projections may lag.
2. Specify state owner, DB/transaction scope and unique keys.
3. Choose synchronization: local transaction first; saga only across independent owners.
4. Define retry + idempotency **at every side-effect boundary**.
5. Quantify capacity, p95/p99, SLO, failover and operating cost.
6. Provide diagrams, ADR, test evidence, dashboard and runbook.

Avoid claiming exactly-once across arbitrary SQL + broker + external provider. Seek exactly-once **business effect** using constraints, deduplication and reconciliation.
