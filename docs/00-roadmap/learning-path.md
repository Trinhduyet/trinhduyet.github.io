# Learning path

## Cách dùng

Đây là dependency-based path, không phải lịch cố định. Mỗi phase kết thúc bằng năng lực quan sát được. Nếu đã có kinh nghiệm, làm assessment/lab trước; chỉ bỏ qua nội dung khi có evidence đạt exit outcome.

## Thứ tự 16 phase

| Phase | Phạm vi | Priority | Exit outcome | Project/evidence |
| --- | --- | --- | --- | --- |
| 01 | CS essentials; Linux, Git, Networking | P0 core/P2 selective | Reason workload/memory/concurrency và chẩn đoán process → DNS/TCP/TLS → HTTP | workload + Linux/Git labs |
| 02 | C# và .NET runtime | P0 | Async/concurrent code có cancellation; đo GC/ThreadPool; host lifecycle | Module 03 + Project 01 |
| 03 | Backend, SQL, API Design | P0 | Trace request; thiết kế contract/auth; đọc schema/query/plan/index | Module 04–06 |
| 04 | ASP.NET Core và EF Core | P0 | Trace request → middleware → persistence; xử lý lifecycle/security/failure | Module 07 + Project 02 |
| 05 | Testing, Security, Code Review, Performance | P0 | Chứng minh correctness/threat/bottleneck bằng evidence | Module 08–10 |
| 06 | Redis và Docker | P0/P1 | Chọn cache đúng lý do; containerize và debug lifecycle/network/resources | Module 11–12 |
| 07 | DevOps, Terraform, Cloud | P1 | Artifact promotion + IaC state/plan/drift/rollback | Module 13–14 |
| 08 | Kubernetes, Observability, DevSecOps | P1/P0 | Vận hành workload bằng probes/resources/RBAC/telemetry/SLO | Module 15 |
| 09 | **Distributed Systems** | **P0** | Recovery cho timeout/duplicate/order/backlog; outbox/inbox/saga | Module 17 + Project 04 |
| 10 | **Microservices Architecture** | **P0/P1** | Define service/data/contract boundaries; checkout unknown outcome; independent deployment | **Module 18 + checkout case study** |
| 11 | AI Engineering và RAG | P0 | Provider/retrieval lifecycle có eval, ACL, cost, observability | Module 19/20 + Project 05 |
| 12 | Agents, MCP, AI Security | P0 | Chọn workflow/agent đúng mức; bảo vệ tools/data/identity | Project 06 |
| 13 | GenAIOps; selective MLOps | P0/P1/P2 | Version/gate/canary/monitor/rollback model-prompt-index | AI release pipeline |
| 14 | System Design | P0 | Đi từ FR/NFR/capacity đến component/data/security/cost | Design exercises |
| 15 | Software Architecture | P0 | Chọn boundaries/style và lập migration/evolution plan | Architecture review |
| 16 | AI-enabled Software Architect | P0 | Tổng hợp conventional + distributed + microservices + AI architecture | Project 07 |

---

# Phase gates

## Gate A — Backend-ready

- Giải thích HTTP request/response và DNS/TCP/TLS path.
- Phân biệt process/thread/Task và blocking/async I/O.
- Viết C# có cancellation và deterministic cleanup.
- Trace request qua middleware/binding/authz.

## Gate B — Production-backend-ready

- Thiết kế API contract + relational schema từ requirements.
- Đọc generated SQL và actual execution plan.
- Chẩn đoán auth/database/connection-pool/latency issue.
- Có test + telemetry đủ điều tra.

## Gate C — Platform-ready

- Container lifecycle/signals/network/volume/resource limit.
- CI/CD artifact promotion, secrets và rollback.
- Terraform state/drift và cloud primitives.
- Kubernetes desired state/probes/service discovery/RBAC.

## Gate D — Distributed-ready

- Viết failure matrix cho slow/unavailable/timeout/duplicate.
- Chọn retry budget + idempotency boundary.
- Giải thích Outbox/Inbox/Dedup và ACK crash window.
- Mô tả eventual consistency/order/Saga/backpressure.
- Đo backlog/oldest-message-age/end-to-end latency.

## Gate E — Microservices-ready

Trước khi gọi một kiến trúc là microservices, phải có evidence:

```text
[ ] Business capability / bounded context rõ.
[ ] Service owns data/schema/migrations.
[ ] Không direct-read DB của service khác.
[ ] API/event contracts explicit và versioned.
[ ] Old/new versions có compatibility overlap.
[ ] Timeout/retry/idempotency semantics explicit.
[ ] Saga/reconciliation cho distributed business flow nếu cần.
[ ] Trace + SLO + runbook + service ownership.
[ ] Deployment độc lập được chứng minh, không chỉ lý thuyết.
```

Checkout exercise bắt buộc:

```text
Duplicate checkout
→ one Order
→ inventory reservation
→ stable payment attempt
→ payment timeout after remote commit
→ PAYMENT_UNKNOWN / PENDING_PAYMENT
→ reconciliation
→ final state
```

Nếu timeout bị map thành `FAILED` mà không prove provider outcome, chưa đạt gate.

## Gate F — Production-AI-ready

- Model/prompt/retrieval changes qua eval gate.
- Tool call bị giới hạn bởi application authorization.
- RAG tôn trọng ACL/deletion/tenant/data lineage.
- Quality/safety/latency/tokens/cost được quan sát.

## Gate G — Architect-ready

- Requirements/NFR/assumptions viết trước component choice.
- Capacity estimate, C4, data flow, trust boundary, ADR.
- Failure analysis, DR, migration, rollback, runbook, cost model.
- Team ownership/operational complexity nằm trong quyết định.
- Có thể giải thích khi nào **không** dùng microservices/agents/Kubernetes.

---

# Phase 09 — Distributed Systems chi tiết

Thứ tự:

```text
Partial failure
→ timeout/deadline
→ retry + jitter
→ idempotency
→ messaging
→ Outbox/Inbox/Dedup
→ ordering
→ eventual consistency
→ Saga/compensation
→ backpressure
```

Đọc: [Module 17](../17-distributed-systems/README.md).

---

# Phase 10 — Microservices Architecture chi tiết

Prerequisite: Gate D.

Thứ tự:

1. [Microservices overview](../18-microservices-architecture/README.md)
2. [Service Boundaries, Data Ownership & Contracts](../18-microservices-architecture/service-boundaries-data-ownership-and-contracts.md)
3. [Checkout Saga: Unknown Outcome & Reconciliation](../18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md)
4. [Communication, Gateway, Discovery & Deployment](../18-microservices-architecture/communication-gateway-discovery-and-deployment.md)
5. [Testing, Observability & Migration](../18-microservices-architecture/testing-observability-and-migration.md)

Learning progression:

```text
Modular Monolith
→ clear module boundaries
→ distributed-system competence
→ extract one justified capability
→ prove contract/data/deployment autonomy
→ expand only when pressure justifies it
```

Không dùng “service nhỏ” làm definition. Kích thước kém quan trọng hơn cohesion, business boundary và autonomy.

---

# Microservices assessment exercise

Lấy một hệ thống hiện có và trả lời:

| Dimension | Evidence |
| --- | --- |
| Business boundary | capability/bounded context map |
| Data ownership | authoritative store + forbidden cross-access |
| Contract | HTTP/event schema + compatibility policy |
| Failure | timeout/retry/idempotency matrix |
| Consistency | local transaction + async consistency/Saga |
| Deployment | old/new compatibility + rollback |
| Observability | trace + service version + business metrics |
| Operations | SLO + on-call + runbook + cost |
| Organization | owning team + release cadence |

Nếu phần lớn ô không có evidence, ưu tiên modularize/productionize trước khi tách thêm service.

---

# Chiến lược cho kinh nghiệm AI đã có

Không mặc định level dựa trên số project. Thực hiện gap assessment bằng một hệ thống đã làm:

| Dimension | Evidence cần thu |
| --- | --- |
| Evaluation | Golden dataset, metric, threshold, regression report |
| Retrieval | Corpus lifecycle, chunk/index version, deletion, ACL tests |
| Agents/tools | Tool schema, authorization, approval, audit log |
| Security | Injection/red-team cases, trust-boundary review |
| Reliability | Timeout/fallback/retry budget, dependency failure exercise |
| Observability | Trace request → retrieval → model → tool; quality/cost metrics |
| Operations | Canary, rollback, incident/runbook, on-call ownership |

---

# Nhịp học khuyến nghị

Một learning slice nên gồm:

```text
Mental model
→ minimal implementation
→ production constraints
→ failure experiment
→ telemetry/diagnosis
→ architecture trade-off
→ evidence + reflection
```

Không chạy nhiều phase song song nếu phase sau phụ thuộc mental model chưa đạt.

## Verification metadata

- Verified: 2026-08-13.
- New Phase 10: Microservices Architecture.
- Data Engineering moved to selective module numbering outside the core sequential path.
- Technology versions: [technology-baseline.md](technology-baseline.md).
