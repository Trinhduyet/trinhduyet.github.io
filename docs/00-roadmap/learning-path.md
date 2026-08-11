# Learning path

## Cách dùng

Đây là dependency-based path, không phải lịch cố định. Mỗi phase kết thúc bằng năng lực quan sát được. Nếu đã có kinh nghiệm, làm assessment/lab trước; chỉ bỏ qua nội dung khi có evidence đạt exit outcome.

## Thứ tự 15 phase

| Phase | Phạm vi | Priority | Exit outcome | Project/evidence |
| --- | --- | --- | --- | --- |
| 01 | CS essentials; Linux, Git, Networking | P0 core/P2 selective | Reason được workload/memory/concurrency và chẩn đoán process → socket → DNS/TCP/TLS → HTTP | WorkloadLab, Linux failure lab, Git recovery exercise |
| 02 | C# và .NET runtime | P0 | Viết async/concurrent code có cancellation; đo allocation/GC/ThreadPool | Project 01 |
| 03 | Backend, SQL, API Design | P0 | Thiết kế contract và schema; đọc query/plan/index; reasoning idempotency | API/data design dossier |
| 04 | ASP.NET Core và EF Core | P0 | Theo dõi request qua pipeline đến SQL; xử lý lifecycle/security/failure | Project 02 |
| 05 | Testing, Security, Code Review, Performance | P0 | Chứng minh correctness, threat controls và bottleneck bằng test/measurement | Review packet + load profile |
| 06 | Redis và Docker | P0/P1 | Chọn cache đúng lý do; containerize và xử lý lifecycle/network/resource failure | Project 03 increment |
| 07 | DevOps, Terraform, Cloud | P1 | Xây artifact/promotion flow và IaC có state/plan/drift/rollback | CI/CD + IaC plan review |
| 08 | Kubernetes, Observability, DevSecOps | P1/P0 | Vận hành workload bằng probes/resources/RBAC/telemetry/SLO | Project 03 production platform |
| 09 | Distributed Systems | P0 | Thiết kế recovery cho partial failure, duplicate, backlog, ordering | Project 04 |
| 10 | AI Engineering và RAG | P0 | Thiết kế provider/retrieval lifecycle có eval, ACL, cost, observability | Project 05 |
| 11 | Agents, MCP, AI Security | P0 | Chọn workflow/agent đúng mức; bảo vệ tools/data/identity và approval | Project 06 |
| 12 | GenAIOps; selective MLOps | P0/P1/P2 | Version, gate, canary, monitor và rollback model/prompt/index | AI release pipeline |
| 13 | System Design | P0 | Đi từ FR/NFR/capacity đến component/data/security/cost | Design exercises |
| 14 | Software Architecture | P0 | Chọn boundaries/style và lập migration/evolution plan | Architecture review |
| 15 | AI-enabled Software Architect | P0 | Tổng hợp conventional + AI architecture ở scale và multi-region | Project 07 |

## Phase 01 chi tiết

Thứ tự khuyến nghị:

1. Process, thread, memory, filesystem và resource model.
2. Linux filesystem, permissions, process, signals và diagnostics.
3. DNS → IP route → TCP/UDP → TLS → HTTP.
4. Git object/history model, safe inspection, branch/merge/revert/recovery.
5. Production troubleshooting theo symptom và evidence.

[Computer Science Essentials](../01-computer-science/README.md) cung cấp deep dive có chọn lọc về workload, data structures, scheduling/concurrency và memory/cache. [Linux, Git và Networking](../02-linux-git-networking/README.md) nối các mental model đó với production diagnostics. Hai module đều có content v1; learner có thể chạy WorkloadLab và incident/Git labs như một evidence chain thay vì học hai silo độc lập.

## Phase gates

### Gate A — Backend-ready

- Giải thích được HTTP request/response, DNS/TCP/TLS path.
- Phân biệt process/thread/Task và blocking/async I/O.
- Dùng Git inspection/recovery mà không phá history.
- Viết C# có cancellation, deterministic cleanup và test.

### Gate B — Production-backend-ready

- Thiết kế API contract và relational schema từ requirements.
- Đọc generated SQL và actual execution plan.
- Chẩn đoán auth, validation, database, connection pool và latency issue.
- Có test pyramid theo boundary và telemetry đủ điều tra.

### Gate C — Platform-ready

- Container lifecycle, signals, networking, volume và resource limit.
- CI/CD artifact promotion, secret handling và rollback.
- Terraform state/drift và cloud primitives.
- Kubernetes desired state, probes, requests/limits, service discovery, RBAC.

### Gate D — Distributed-ready

- Viết failure matrix cho dependency slow/unavailable/timeout/duplicate.
- Chọn retry budget, idempotency key, outbox/inbox và DLQ/replay.
- Giải thích consistency boundary và recovery ownership.
- Đo queue backlog, saturation và end-to-end latency.

### Gate E — Production-AI-ready

- Mọi thay đổi model/prompt/retrieval phải qua eval gate.
- Tool call bị giới hạn bởi application authorization, không bởi “model ngoan”.
- RAG tôn trọng ACL, deletion, tenant boundary và data lineage.
- Quality, groundedness, safety, latency, tokens và cost được quan sát.

### Gate F — Architect-ready

- Requirements/NFR và assumptions được viết trước component choice.
- Có capacity estimate, C4, data flow, trust boundary, ADR.
- Có failure analysis, DR, migration, rollback, runbook và cost model.
- Team ownership và operational complexity nằm trong quyết định.

## Chiến lược cho kinh nghiệm AI đã có

Không mặc định level dựa trên số project. Thực hiện gap assessment bằng một hệ thống đã làm:

| Dimension | Evidence cần thu |
| --- | --- |
| Evaluation | Golden dataset, metric, threshold, regression report |
| Retrieval | Corpus lifecycle, chunk/index version, deletion, ACL tests |
| Agents/tools | Tool schema, authorization, approval, audit log |
| Security | Injection/red-team cases, data-flow/trust-boundary review |
| Reliability | Timeout/fallback/retry budget, dependency failure exercise |
| Observability | Trace request → retrieval → model → tool; quality/cost metrics |
| Operations | Canary, rollback, incident/runbook, on-call ownership |

Kết quả assessment quyết định phần nào được rút gọn; production/architecture gaps vẫn là P0.

## Nhịp học khuyến nghị

Một learning slice nên gồm:

~~~text
Mental model
→ minimal implementation
→ production constraints
→ failure experiment
→ telemetry/diagnosis
→ architecture trade-off
→ evidence + reflection
~~~

Không chạy nhiều phase song song nếu phase sau phụ thuộc mental model chưa đạt. Có thể chạy một theory thread và một project thread miễn project không vượt prerequisite.

## Verification metadata

- Verified: 2026-08-11
- Technology version: [technology-baseline.md](technology-baseline.md)
- Official sources: quản lý theo module
- Context7 queries used: none riêng cho tài liệu sequencing
- Notes: sequence kết hợp target role, prerequisite graph và kinh nghiệm AI tự khai báo; skills thực tế vẫn phải được assessment.
