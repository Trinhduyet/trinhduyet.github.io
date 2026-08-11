# Knowledge dependency graph

## Mental model

Một topic “đứng trước” topic khác khi thiếu nó sẽ làm người học chỉ nhớ syntax mà không giải thích được behavior, failure hoặc trade-off. Dependency không có nghĩa phải học mọi internals trước; chỉ cần đạt exit criteria tối thiểu.

~~~mermaid
flowchart TD
    CS["CS essentials: process, memory, concurrency"] --> NET["Linux + Networking"]
    CS --> DOTNET["C# / .NET"]
    NET --> HTTP["HTTP / Backend"]
    NET --> DOCKER["Docker"]
    DOTNET --> ASP["ASP.NET Core"]
    HTTP --> API["API Design"]
    HTTP --> ASP
    SQL["Relational model + SQL"] --> EF["EF Core"]
    DOTNET --> EF
    ASP --> PROD["Testing + Security + Performance"]
    EF --> PROD
    SQL --> TX["Transactions + Isolation"]
    TX --> OUTBOX["Outbox / Inbox"]
    DOCKER --> K8S["Kubernetes"]
    NET --> K8S
    IAC["DevOps + IaC + Cloud"] --> K8S
    PROD --> OBS["Observability"]
    MSG["Messaging"] --> DIST["Distributed Systems"]
    NET --> DIST
    TX --> DIST
    OUTBOX --> DIST
    OBS --> DIST
    AI["AI Engineering + Evaluation"] --> RAG["Production RAG"]
    RAG --> AGENT["Agents + MCP"]
    DIST --> AGENT
    SEC["Security + Threat Modeling"] --> AGENT
    AI --> GENOPS["GenAIOps"]
    OBS --> GENOPS
    NFR["Requirements + NFR"] --> DESIGN["System Design"]
    DIST --> DESIGN
    RAG --> DESIGN
    AGENT --> DESIGN
    DESIGN --> ARCH["Software Architecture"]
    DOCS["ADR + C4 + Runbook"] --> ARCH
~~~

## Dependency rules quan trọng

| Học sau | Prerequisite tối thiểu | Vì sao |
| --- | --- | --- |
| ASP.NET Core | HTTP, process/socket, DI, async | Middleware syntax không giải thích request lifecycle hoặc saturation |
| EF Core performance | SQL, indexes, execution plan | LINQ không cho biết database thực sự làm gì |
| Docker | Linux process/filesystem/signals/networking | Container là process isolation, không phải “VM nhẹ” |
| Kubernetes | Docker/container model, DNS/networking, resources | YAML không giải thích reconciliation, readiness hoặc scheduling |
| Outbox | Local transaction, messaging, idempotency | Pattern vô nghĩa nếu chưa hiểu atomic boundary và duplicates |
| Microservices | Modular boundaries, distributed failure, observability | Tách process tạo network/operations/data-consistency cost |
| RAG | Embeddings/retrieval, data lifecycle, evaluation | Demo retrieval không đủ cho ACL, deletion và regression |
| Agentic RAG | Production RAG, tool calling, state, security | Agent tăng agency và blast radius |
| MCP | Tool contract, identity/authz, trust boundaries | Discovery/transport không thay thế authorization |
| GenAIOps | AI evaluation, observability, versioned artifacts | Không thể gate/rollback thứ chưa đo và chưa version |
| System Design | FR/NFR, capacity, data/distributed fundamentals | Component choice phải theo requirement và scale |
| Software Architecture | System Design, boundaries, migration, documentation | Pattern không phải recipe; architecture phải tiến hóa được |

## Entry criteria theo module

### 01 Computer Science Essentials

- Biết C# syntax cơ bản và chạy được .NET CLI.
- Không yêu cầu kiến thức thuật toán phỏng vấn hoặc OS internals trước đó.
- Bắt đầu tại [Module 01 overview](../01-computer-science/README.md); evidence gồm lookup, race và locality experiments.

### 02 Linux, Git và Networking

- Có thể dùng terminal cơ bản.
- Hiểu file, process và client/server ở mức khái niệm.
- Không yêu cầu Docker hoặc Kubernetes.

### 03 .NET

- Biết syntax C# cơ bản.
- Hiểu process/thread/memory ở mức L1.
- Có thể dùng Git để lưu và review thay đổi.
- Chạy được [RuntimeLab](../../labs/03-dotnet/runtime-lab/Program.cs) và đọc output cancellation/allocation/diagnostics.

### 04 Backend

- Đạt entry criteria Module 03: hiểu async/cancellation, ownership, host lifetime và diagnostics.
- Đọc được HTTP method/status/header, route và request body ở mức L1.
- Có thể chạy [BackendLab](../../labs/04-backend/backend-lab/Program.cs) và giải thích pagination, idempotency, rate/backpressure output.

### 05–15 Production Backend Track

| Module | Entry evidence tối thiểu |
| --- | --- |
| 05 SQL | Đọc schema/constraint, transaction/isolation và actual plan ở workload bounded |
| 06 API Design | Viết resource/status/error/version contract và compatibility matrix |
| 07 ASP.NET Core | Trace pipeline, config/health/readiness, resilience và rollback |
| 08 Testing/Review | Có test boundary, integration/contract evidence và review packet |
| 09 Security/DevSecOps | Có threat model, identity/secret matrix và supply-chain gate |
| 10 Performance | Có baseline, load/capacity model, p95/p99 và regression gate |
| 11 Redis | Có key/type/TTL policy, consistency/stampede và restore/failover note |
| 12 Docker | Build image reproducibly, inspect runtime/resource/signal/security boundary |
| 13 DevOps/IaC | Có artifact promotion, Terraform plan/state/lock và rollback path |
| 14 Cloud | Có identity/network map, RTO/RPO, cost/unit economics và DR rehearsal |
| 15 Kubernetes | Có reconciliation/readiness, workload/network/storage và RBAC/observability review |

### 05 SQL

- Logic/sets và data types cơ bản.
- Có thể đọc code backend đơn giản.
- Không yêu cầu EF Core.

### 07 ASP.NET Core

- HTTP semantics, C# async, DI/config/logging.
- API design fundamentals.
- SQL/EF basics cho phần persistence.

### 15 Kubernetes

- Linux/networking diagnostics.
- Container image/runtime/lifecycle.
- Deployment artifact, configuration/secrets và observability basics.

### 17 Distributed Systems

- Networking và partial failure.
- Transactions/isolation.
- Messaging fundamentals, idempotency và telemetry.

### 20–23 Production AI

- Conventional backend security/reliability/observability.
- AI model/tool/retrieval concepts.
- Evaluation fundamentals trước optimization hoặc agent autonomy.

### 24–26 Architecture

- NFR và capacity estimation.
- Data, security, deployment và failure reasoning.
- Ít nhất một project đã được vận hành và điều tra sự cố.

## Anti-shortcut checks

- Nếu không đọc được execution plan, chưa tối ưu EF query bằng “best practice”.
- Nếu không giải thích được SIGTERM/readiness, chưa thiết kế rolling shutdown.
- Nếu không có idempotency boundary, chưa thêm retry cho side effect.
- Nếu không có eval dataset, chưa tuyên bố prompt/model mới “tốt hơn”.
- Nếu không có tool authorization matrix, chưa tăng agent autonomy.
- Nếu không có NFR/capacity, chưa chọn microservices/Kubernetes/multi-region.

## Next path

Bắt đầu: [Computer Science Essentials](../01-computer-science/README.md) → [Linux, Git và Networking](../02-linux-git-networking/README.md) → [C#/.NET Runtime](../03-dotnet/README.md) → [Backend](../04-backend/README.md) → SQL/API → ASP.NET Core.

Các link tới module Planned chỉ được thêm khi file có nội dung hữu ích; roadmap status nằm trong [master-roadmap.md](master-roadmap.md).

## Verification metadata

- Verified: 2026-08-11
- Technology version: stable dependency concepts
- Official sources: module-specific references
- Context7 queries used: none
- Notes: dependency graph ưu tiên production reasoning; không mô phỏng đầy đủ mọi edge có thể có.
