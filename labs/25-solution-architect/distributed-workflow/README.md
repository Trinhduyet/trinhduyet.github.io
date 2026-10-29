# Domain-neutral Distributed Workflow Lab (.NET 10 + SQL Server + RabbitMQ)

This is a **teaching lab**, not a production reservation, payment, or trading engine. The resource can represent one booking slot, one inventory unit, one limited quota, or a synthetic account. No OMS-specific rules are required.

## Principles demonstrated

**Verified integration evidence:** [GitHub Actions run #40816563843 — success](https://github.com/Trinhduyet/trinhduyet.github.io/actions/runs/40816563843), .NET 10 build plus real SQL Server/RabbitMQ smoke checks. More extensive load/DR/partner semantics are not verified by this run.

1. **Concurrency:** an atomic conditional SQL update protects resource availability under 100 simultaneous requests.
2. **Idempotency:** `(AccountId, Idempotency-Key)` is unique; same payload replays the ID, different payload conflicts.
3. **Transaction boundary:** reservation and Outbox row commit together.
4. **At-least-once messaging:** durable RabbitMQ queue + publisher confirmations; a crash after publish before outbox mark causes a redelivery.
5. **Inbox:** unique EventId and business state mutation in one SQL transaction prevent duplicate effects.
6. **Unknown/reconciliation:** simulated provider timeout leaves resource reserved until operator confirms/rejects.
7. **DLQ:** malformed/poison message is rejected into `architecture.workflow.dlq` for operator review.

## Requirements

- Docker Engine + Compose (Linux/macOS/Windows), enough RAM for SQL Server.
- .NET 10 SDK for running smoke tests from the host.
- No real customer/financial data. Supply **local-only** credentials through `.env`; `.env` is git-ignored.

## Run

From this directory:

```bash
cp .env.example .env
# Edit .env with unique local values before starting.
docker compose up -d --build
curl -fsS http://localhost:8088/health
set -a; source .env; set +a
dotnet run --project SmokeTests/SmokeTests.csproj -- http://localhost:8088
```

The API can take up to ~60 seconds to wait for SQL Server initialization. For deterministic cleanup, use `docker compose down` (keeps SQL volume). `docker compose down -v` **deletes the lab SQL data volume**; use only when you explicitly want to discard it.

## API contracts

Every non-health endpoint requires `X-Lab-Key: <the LAB_API_KEY value in .env>`.

| Endpoint | Purpose | Semantics |
|---|---|---|
| `POST /lab/accounts` | Seed synthetic capacity | Development-only; no production use |
| `GET /lab/accounts/{accountId}` | Read available/reserved | Lab-only read model |
| `POST /api/reservations` | Claim capacity | `Idempotency-Key` required; 202 Accepted or 409 Conflict |
| `GET /api/reservations/{id}` | Read workflow status | `Pending`, `Unknown`, `Completed`, `Rejected` |
| `POST /lab/reconcile/{id}` | Resolve Unknown | JSON `{ "outcome": "confirmed" }` or `rejected` |

Request example:

```json
{"accountId":"demo", "quantity":8, "simulation":"none"}
```

`simulation` may be `none` (completed), `reject` (compensate/release), or `timeout` (Unknown until reconciliation). The simulation is local and **does not model actual payment or brokerage protocol guarantees**.

## Failure drills

| Drill | Expected result | Evidence |
|---|---|---|
| 100 concurrent reservations of 8 from capacity 10 | Exactly one Accepted; balance never negative | SmokeTests output + SQL constraints |
| Same key + payload retry | Same business ID, no extra reservation | SmokeTests replay check |
| Same key, different payload | 409 conflict | SmokeTests conflict check |
| `FailAfterPublishOnce=true` | First publish confirmed then worker exception; Outbox resent; Inbox dedup | Logs + `LabOutbox`/`LabInbox` rows |
| `simulation=timeout` | Pending → Unknown; quantity stays reserved; reconciliation releases/commits | SmokeTests and status query |
| `simulation=reject` | Reservation rejected, available restored | SmokeTests |
| Broker disconnect | Outbox remains unpublished until reconnect | Manual `docker compose stop rabbitmq`, restart and inspect logs |
| Malformed message | DLQ receives rejected delivery | Publish malformed payload manually in RabbitMQ management UI |

### Useful SQL inspection

```sql
SELECT TOP (20) * FROM dbo.LabReservations ORDER BY CreatedAt DESC;
SELECT TOP (20) * FROM dbo.LabOutbox ORDER BY CreatedAt DESC;
SELECT TOP (20) * FROM dbo.LabInbox ORDER BY ProcessedAt DESC;
SELECT * FROM dbo.LabAccounts;
```

## Important limitations (intentional)

- Authentication is a static lab key, **not OAuth2/OIDC, multi-tenant authorization, mTLS or production secret management**.
- The design has one SQL Server database and does **not** prove multi-region disaster recovery, cross-database Saga, or external provider exactly-once semantics.
- The publisher does not implement a distributed leasing strategy, long-term outbox archive or ordered delivery across instances; duplicate publishing is safe but scale/ordering need additional design.
- Consumer poison messages are put in a DLQ; there is no automatic retry taxonomy, replay UI, schema version migration or quarantine workflow.
- No real payment/stock/OMS domain rule, audit-grade ledger, encryption policy, security certification or load/DR report is implied.
- The smoke tests require real Docker services. Build success alone is **not** integration-test evidence.

## Reuse this lab for other domains

- **Booking:** resource = seat/slot, add expiry race and late payment policy.
- **E-commerce:** resource = inventory quantity, add shipment/payment Saga.
- **Loyalty:** resource = points; extend to expiring point buckets.
- **Finance:** resource = synthetic balance; replace with double-entry ledger and provider reconciliation before treating as money.
- **Trading:** resource = buying-power reservation; extend to order state machine/exchange protocol.

See [Module 25 distributed systems](../../../docs/25-software-architecture/distributed-systems-architecture-deep-dive.md) and [practice cases](../../../docs/25-software-architecture/solution-architect-case-practice-and-assessment.md).
