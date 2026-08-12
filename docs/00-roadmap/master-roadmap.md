# Master roadmap

## Đích nghề nghiệp

**AI-enabled Software Architect** có thể đi từ business problem đến kiến trúc có thể triển khai, vận hành, bảo vệ, đo lường và tiến hóa.

Mọi quyết định lớn phải trả lời:

- thành phần giải quyết requirement nào;
- phương án đơn giản nhất là gì;
- failure modes và recovery behavior là gì;
- security/trust boundary thay đổi ra sao;
- ai sở hữu vận hành;
- chi phí tiền, độ trễ và độ phức tạp là bao nhiêu;
- điều gì đổi ở 10x và 100x scale;
- khi nào cần migration hoặc đảo quyết định.

## Đối chiếu roadmap.sh hiện hành

Danh mục [roadmap.sh](https://roadmap.sh/roadmaps/) được kiểm tra ngày 2026-08-11. Catalog hiện phân nhóm role-based, skill-based và best-practice; trong phạm vi mục tiêu có Backend, DevOps, DevSecOps, AI Engineer, Data Engineer, Software Architect, API Design, MLOps, AI Red Teaming, Network Engineer; cùng SQL, System Design, ASP.NET Core, Linux, Kubernetes, Docker, Terraform, Redis, AI Agents, Prompt Engineering, Computer Science và DSA.

| Quan sát từ roadmap.sh | Quyết định của repository |
| --- | --- |
| Nhiều roadmap song song, tối ưu cho discovery theo vai trò/kỹ năng | Hợp nhất theo dependency và career target; không học tuần tự từng roadmap |
| Backend/ASP.NET/SQL/API có phạm vi rộng nhưng tách nhau | Nối request lifecycle → generated SQL → execution plan → production behavior |
| DevOps/Docker/Kubernetes/Terraform là các roadmap riêng | Linux/networking trước Docker; Docker trước Kubernetes; IaC và delivery gắn với operations |
| AI Engineer/Agents/Prompt/MLOps/Red Teaming tách nhau | Tập trung production AI: evaluation, RAG, tools, MCP, security, observability, cost, GenAIOps |
| Software Architect/System Design nằm gần cuối theo vai trò | NFR, trade-off và ADR xuất hiện sớm; case study tổng hợp ở cuối |
| Catalog giúp thấy breadth | Official docs/specifications quyết định behavior và version |

Những phần repository cố ý nhấn mạnh hơn catalog:

- SQL Server internals và mối nối LINQ → SQL → plan → index → I/O/CPU;
- failure-first, operability, migration và cost ở từng technology;
- modular monolith trước microservices;
- AI evaluation là P0, không phải phụ lục;
- agent authorization, tool capability, MCP trust boundary và human approval;
- architecture evidence: NFR, capacity estimate, C4, ADR, threat model, runbook.

## Priority model

| Priority | Ý nghĩa | Điều kiện hoàn thành điển hình |
| --- | --- | --- |
| P0 — Core | Bắt buộc hiểu sâu cho target role | Có thể implement, operate, diagnose và design/review |
| P1 — Important | Phải dùng hoặc reasoning tự tin | Có thể implement/operate; design với reference |
| P2 — Selective | Đủ để quyết định kiến trúc; deep dive khi project cần | Có thể giải thích, làm lab chọn lọc và biết trigger nghiên cứu |
| P3 — Awareness | Biết tồn tại và boundary | Có thể mô tả use case, risk và nơi tra cứu |

## Bản đồ module

| Module | Trọng tâm | Priority | Target layer | Trạng thái |
| --- | --- | --- | --- | --- |
| 00-roadmap | Dependency, baseline, source policy, progress | P0 | L5 | Completed v1 |
| 01-computer-science | Complexity, data structures, OS/memory/concurrency | P0 core + P2 selective | L3–L4 | Content v1 complete; learner evidence pending |
| 02-linux-git-networking | Production troubleshooting, Git, DNS/TCP/TLS/HTTP | P0 | L4 | Content v1 complete; learner evidence pending |
| 03-dotnet | C#, runtime, async/concurrency, GC, hosting, diagnostics | P0 | L5 | Content v1 complete; learner evidence pending |
| 04-backend | Request lifecycle, auth, validation, background/integration | P0 | L5 | Content v1 complete; learner evidence pending |
| 05-sql | Relational model, SQL Server, transactions, plans, operations | P0 | L5 | Structure/coverage v1; deep content review pending |
| 06-api-design | Resource/contract design, evolution, REST/RPC/events | P0 | L5 | Structure/coverage v1; deep content review pending |
| 07-aspnet-core | Pipeline, hosting, security, resilience, deployment | P0 | L5 | Structure/coverage v1; deep content review pending |
| 08-testing-code-review | Test boundaries và multi-angle review | P0 | L4 | Structure/coverage v1; deep content review pending |
| 09-security-devsecops | Identity, app/API security, supply chain, gates | P0/P1 | L5 | Structure/coverage v1; deep content review pending |
| 10-performance | Measurement, profiling, load/capacity, bottlenecks | P0 | L5 | Structure/coverage v1; deep content review pending |
| 11-redis-caching | Cache/data structures, failure modes, coordination | P1 | L5 | Structure/coverage v1; deep content review pending |
| 12-docker | Images, runtime, networking, security, operations | P0 | L5 | Structure/coverage v1; deep content review pending |
| 13-devops-iac | CI/CD, artifacts, Terraform, drift, deployment strategy | P1 | L5 | Structure/coverage v1; deep content review pending |
| 14-cloud | Cloud-neutral primitives, regions/AZ, identity, DR | P1 | L4 | Structure/coverage v1; deep content review pending |
| 15-kubernetes | Reconciliation, workloads, networking, security, upgrades | P1 | L5 | Structure/coverage v1; deep content review pending |
| 16-observability | Logs, metrics, traces, OTel, SLI/SLO, incidents | P0/P1 | L5 | Planned |
| 17-distributed-systems | Partial failure, messaging, consistency, recovery | P0 | L5 | Planned |
| 18-data-engineering | Ingestion, CDC, batch/stream, lineage, retention | P2 | L3 | Planned |
| 19-ai-engineering | Model/provider abstraction, structured output, eval/cost | P0 | L5 | Planned — advanced-first |
| 20-rag | Full lifecycle, ACL, deletion, versioning, evaluation | P0 | L5 | Planned — advanced-first |
| 21-ai-agents-mcp | Tool/agent/workflow/MCP, state, authorization, audit | P0 | L5 | Planned — advanced-first |
| 22-ai-security | Injection, exfiltration, tool abuse, red teaming | P0/P1 | L5 | Planned — advanced-first |
| 23-genaiops-mlops | Prompt/model/index lifecycle; selective training ops | P0/P1/P2 | L5 | Planned — advanced-first |
| 24-system-design | Requirements, capacity, components, AI system design | P0 | L5 | Planned |
| 25-software-architecture | Boundaries, styles, evolution, migration | P0 | L5 | Planned |
| 26-architecture-documentation | C4, ADR, RFC, threat/failure/runbook | P1 | L5 | Planned |
| projects | Bảy hệ thống tiến hóa, không phải toy collection | P0 | L2–L5 | Planned |

## Content quality gate

Một chapter substantial **không** được coi là hoàn thành chỉ vì có đủ headings.

`Content v1` yêu cầu:

- mental model và terminology riêng của topic;
- cơ chế thực tế đủ để giải thích observed behavior;
- minimal example và production example khi phù hợp;
- failure modes, troubleshooting và verification/evidence riêng của công nghệ;
- performance, security, observability, operations và cost khi liên quan;
- Architect Perspective: simpler alternative, trade-offs, when-not-to-use, 10x/100x impact;
- official sources cho technical claims và Context7 cho library/framework/version-sensitive details khi phù hợp.

Generic prose có thể copy gần như nguyên văn sang một technology khác được xem là `Structure/coverage`, không phải deep content.

Module 01–04 hiện được dùng làm quality reference. Module 05–15 cần deep-review theo gate này trước khi nâng lại trạng thái `Content v1 complete`.

## Scope theo track

### Foundations

- Big-O và cấu trúc dữ liệu có liên quan trực tiếp đến backend.
- Process, thread, scheduling, memory, filesystem, CPU/cache.
- Linux resources, permissions, signals và production commands.
- DNS, TCP, UDP, TLS, HTTP, ports, sockets, NAT, proxy/load balancer.
- Git history, branching, review, recovery và artifact traceability.

### .NET và backend

- C# type system, generics, collections, LINQ, exceptions, IDisposable.
- Task, async/await, CancellationToken, ThreadPool, synchronization.
- GC, allocations, memory, diagnostics.
- Generic Host, configuration, DI, logging.
- HTTP request lifecycle, authentication/authorization, validation.
- Pagination/filtering/sorting, idempotency, rate limiting, caching.
- Background processing, files, integrations, webhooks và contracts.

### Data và API

- Relational model, schema/constraints, SQL, transactions/isolation.
- Locking, blocking, deadlocks, row versioning.
- Indexes, statistics, optimizer, execution plans, Query Store.
- Plan cache, parameterization, parallelism, memory grants, tempdb/log.
- EF Core query shape và generated SQL.
- REST semantics, error model, OpenAPI, versioning/evolution.
- REST vs RPC/gRPC; GraphQL overview; webhooks và event contracts.

### Production platform

- Testing, code review, security, DevSecOps và performance.
- Redis, Docker, CI/CD, Terraform, cloud.
- Kubernetes, OpenTelemetry, SLI/SLO và incident investigation.
- Operations, upgrades, rollback, DR, supply-chain security và cost.

### Distributed systems

- Timeouts, retries, backoff/jitter, circuit breaker, bulkhead.
- Idempotency, deduplication, ordering và delivery semantics.
- Queues, pub/sub, streams, backpressure, dead letters.
- Outbox/inbox, saga, eventual consistency, replication/partitioning.
- Clock/time, partial failure, capacity và recovery.

### Production AI

- Model selection/fallback, structured output, tool calling, embeddings.
- Evaluation datasets/gates, latency/token/cost và observability.
- RAG ingestion-to-citation lifecycle, ACL, multi-tenancy, deletion.
- Agent vs deterministic workflow; capability-oriented tools.
- MCP client/server/tools/resources, identity và trust boundary.
- Prompt injection, malicious retrieval/MCP, exfiltration, excessive agency.
- Prompt/model/retrieval/index versioning, canary và rollback.

### Architecture

- Functional requirements, NFR, constraints và quality attributes.
- Capacity estimates và technology selection theo requirements.
- Modular monolith → event-driven → distributed systems → microservices khi có lý do.
- Data/security/deployment/migration architecture.
- C4, ADR, RFC, threat model, failure analysis và runbook.

## AI experience policy

Người đọc đã có kinh nghiệm thực tế về AI Engineering, AI Agents và Prompt Engineering. Trạng thái này là **input cho gap analysis**, không phải bằng chứng đã đạt level kiến trúc.

Không lặp tutorial “gọi model đầu tiên”. Bắt đầu bằng assessment:

- có eval dataset và regression gate chưa;
- tool-selection accuracy và task completion được đo chưa;
- prompt/model/retrieval config có version/rollback chưa;
- PII, retention, injection và tool authorization được kiểm soát chưa;
- token/cost/latency/quality được quan sát chưa;
- agent action có approval/audit/blast-radius control chưa.

## Project spine

| Project | Sự tiến hóa chính | Architecture evidence bắt buộc |
| --- | --- | --- |
| 01 Async File Processor | I/O, Channels, cancellation, concurrency | Failure tests và resource measurements |
| 02 Order Management | ASP.NET Core, SQL Server, EF Core, API/security | NFR-lite, API contract, data model |
| 03 Production Backend | Redis, Docker, workers, observability, CI/CD | SLO, load test, threat review, runbook |
| 04 Distributed Notifications | Broker, outbox, idempotency, DLQ, Kubernetes | ADRs, failure analysis, replay procedure |
| 05 Enterprise RAG | Ingestion, ACL, hybrid retrieval, deletion, eval | Data lineage, threat model, index migration |
| 06 Production Agent Platform | Tools, MCP, workflow, approval, audit | Trust boundaries, tool policy, eval/red-team gates |
| 07 High-scale AI System Design | 100k+ users, multi-region/provider | Full C4, capacity, cost, DR, migration, runbook |

## Definition of done cho một module

- Có overview và dependency rõ ràng.
- Chương substantial dùng standard template và mental model trước syntax.
- Có minimal và production example khi phù hợp.
- Có performance, security, reliability, observability, operations và cost.
- Có failure experiment, verification và exit criteria đo được.
- Có official sources, Vietnamese resource khi đủ chất lượng và metadata.
- Có cross-link, skills-matrix update và multi-role review.
- Mỗi section phải có nội dung topic-specific; heading tồn tại một mình không được tính là completion.

## Verification metadata

- Verified: 2026-08-12
- Technology version: [technology-baseline.md](technology-baseline.md)
- Official sources: từng module quản lý trong references.md
- Roadmap sources: [catalog](https://roadmap.sh/roadmaps/), [Backend](https://roadmap.sh/backend), [ASP.NET Core](https://roadmap.sh/aspnet-core), [System Design](https://roadmap.sh/system-design), [AI Engineer](https://roadmap.sh/ai-engineer), [AI Agents](https://roadmap.sh/ai-agents), [Software Architect](https://roadmap.sh/software-architect)
- Context7 queries used: xem [technology-baseline.md](technology-baseline.md)
- Notes: thứ tự đã được tổ chức lại theo prerequisite, không sao chép thứ tự roadmap.sh; status Module 05–15 đã được hạ về structure/coverage để phản ánh đúng quality gate hiện tại.
