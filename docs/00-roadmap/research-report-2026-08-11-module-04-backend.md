# Research report — Module 04 Backend

## Topic researched

- ASP.NET Core request pipeline, middleware ordering, routing, endpoint metadata;
- model binding, validation, DTO boundaries and ProblemDetails error contracts;
- authentication schemes, challenge/forbid, policy/resource authorization and tenant checks;
- pagination, idempotency, rate limiting, caching and overload behavior;
- hosted services, bounded queues, file lifecycle, HTTP integrations and webhook replay.

## Official sources found

- Microsoft Learn ASP.NET Core 10 middleware, routing, controller/action, model binding and validation pages.
- Microsoft Learn authentication, authorization and policy-based authorization pages.
- Microsoft Learn ProblemDetails/error handling, rate limiting and caching pages.
- Microsoft Learn hosted services, `IHttpClientFactory`, `HttpClient` guidelines and integration-event idempotency.

Canonical URLs are maintained in [Module 04 references](../04-backend/references.md).

## Versions checked

- Repository target: ASP.NET Core/.NET 10.
- .NET 10 API endpoint authentication behavior returns 401/403 for recognized API endpoints instead of cookie redirects; this is treated as version-sensitive and linked to the official page.
- Rate-limiter configuration and diagnostics require load testing before production rollout.

## Nuance and decisions

1. Module 04 stops at backend application boundaries; SQL/EF Core execution plans remain Module 05.
2. Authentication creates identity; authorization makes permission decisions; validation handles input/business invariants. They are not interchangeable.
3. `202 Accepted` is a protocol contract for accepted asynchronous work, not proof that a side effect completed.
4. In-process bounded channels are useful experiments but do not provide durable delivery after process restart.
5. Pagination, idempotency, rate limiting and caching are capacity/consistency contracts, not independent decorators.
6. Webhook verification uses exact raw bytes plus timestamp/event-id replay controls; normalized JSON is not a signature substitute.

## Context7

Context7 did not expose a callable tool in this run. No fictional library IDs or queries were recorded. Version-sensitive behavior was checked against direct Microsoft Learn pages and the repository baseline.

## Files delivered

- `docs/04-backend/README.md`
- `docs/04-backend/references.md`
- four Module 04 chapters using the 28-heading template
- `labs/04-backend/backend-lab/BackendLab.csproj`
- `labs/04-backend/backend-lab/Program.cs`
- root README, roadmap status, prerequisites, learning path and skills matrix.

## Verification evidence

- `dotnet build -c Release`: succeeded with 0 warnings and 0 errors.
- `pagination 10000 100 3`: returned IDs 201–300, `stableOrdering=true`, 100 pages and deterministic checksum.
- `idempotency 1000 10`: 100 unique keys/side effects and 900 replays, with zero conflicts.
- `backpressure 10000 64 25`: bounded run produced/consumed 590 items and stopped cooperatively.
- Invalid bounds/unknown command cases returned usage exit code `64`.
- Static validation: four chapters with 28 headings each, balanced fences, no broken local links and no Module 04 placeholders.

## Multi-role review

| Role | Review question | Module 04 answer |
| --- | --- | --- |
| Senior Engineer | Request/worker code có contract rõ không? | DTO, status, cancellation, bounded queue và ownership được tách rõ. |
| Security Engineer | Trust boundary và abuse path ở đâu? | Authn/authz, tenant/resource policy, body/path limits, signatures, replay và privacy. |
| Performance Engineer | Capacity được bound và đo thế nào? | Pagination, limiter, cache, queue capacity, tail latency và downstream budget. |
| Operations Engineer | Diagnose/recover thế nào? | Structured request/worker evidence, DLQ/replay, shutdown drain và runbook. |
| Software Architect | Boundary nào tiến hóa sang SQL/API? | Transport/application/integration boundaries; Module 05 nối persistence, Module 06 nối API evolution. |

## Verification metadata

- Verified: 2026-08-11.
- Scope: Module 04 content, BackendLab and roadmap integration.
- Official sources: [Module 04 references](../04-backend/references.md).
- Context7 queries used: none; tool unavailable.
