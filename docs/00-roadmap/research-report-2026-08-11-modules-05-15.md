# Research report — Modules 05–15

## Scope

Module 05–15 covers the production backend platform path after C#/.NET Runtime and Backend boundaries:

1. SQL and relational execution;
2. API contracts and evolution;
3. ASP.NET Core hosting and resilience;
4. testing/code review;
5. security and DevSecOps;
6. performance and capacity;
7. Redis and caching;
8. Docker;
9. DevOps and IaC;
10. cloud primitives/reliability/cost;
11. Kubernetes workloads/security/observability.

## Source policy

Behavior and version-sensitive claims were checked against official sources: Microsoft Learn/SQL Server, RFCs, Docker Docs, GitHub Docs, HashiCorp Developer, Redis Docs and Kubernetes documentation. Roadmap discovery does not decide runtime behavior. Context7 was not callable in this run; no fictional query IDs were recorded.

## Decisions

- Module 05 keeps SQL Server execution, transactions, indexes, Query Store and operations separate from API/ASP.NET syntax.
- Module 06 treats HTTP/RPC/events as contracts with compatibility, idempotency and replay semantics.
- Module 07 separates application pipeline/hosting from backend contract design and deployment operations.
- Module 08–10 make evidence, security and performance gates explicit before platform scale.
- Module 11–15 teach Redis, containers, delivery, cloud and Kubernetes as ownership/capacity choices, not mandatory complexity.
- Every module uses the same 28-heading chapter template and Mermaid overview/dependency diagrams.

## Delivered files

Each module has `README.md`, `references.md` and three production-reasoning chapters:

- `docs/05-sql/` through `docs/15-kubernetes/`;
- root README, roadmap overview/master roadmap, prerequisites, learning path and skills matrix updated;
- README pages include the Mermaid.js CDN/Jekyll rendering script.

## Validation plan

- 33 chapters must contain exactly 28 standard headings each.
- All local Markdown links must resolve; future modules are plain text until their README exists.
- Mermaid fences must be balanced and begin with `flowchart`, `sequenceDiagram` or `graph`.
- No TODO/TBD/placeholder markers in Modules 05–15.
- Official source links are recorded per module; learner evidence remains pending until labs/reviews are executed.

## Multi-role review

| Role | Cross-module question | Required evidence |
| --- | --- | --- |
| Senior Engineer | Contract, state and ownership có rõ không? | Schema/API/test/release artifacts |
| Security Engineer | Trust boundary, identity, secret và supply chain có được kiểm soát không? | Threat model, policy, scan/attestation, negative tests |
| Performance Engineer | Capacity, tail latency và cost có được đo không? | Plan/profile/load/capacity report |
| Operations Engineer | Deploy, observe, recover và rollback có runbook không? | Probe/SLO/incident/restore/rollback evidence |
| Software Architect | Complexity có trả đúng NFR và scale trigger không? | Decision record, trade-off, migration path |

## Verification metadata

- Verified: 2026-08-11.
- Content status: Modules 05–15 content v1; learner evidence pending.
- Technology versions: SQL Server 17 docs, ASP.NET Core/.NET 10, Docker current docs, Terraform current language docs, Redis current docs, Kubernetes current concepts.
- Notes: production deployment must refresh provider/tool versions and validate organization-specific policy before use.
