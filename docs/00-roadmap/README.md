# Roadmap Overview

> Nếu tài liệu dài, bắt đầu bằng **[Cách đọc tài liệu này](how-to-read.md)**. Mục tiêu là chạy code và failure experiment, không đọc tuần tự hàng trăm trang.

Kho tri thức này đi từ .NET Backend tới **AI-enabled Software Architect**.

## Chọn điểm bắt đầu

### .NET Backend

```text
C#/.NET → Backend → SQL → API Design → ASP.NET Core
```

Điểm vào: [Module 03 — .NET](../03-dotnet/README.md).

### Production / Distributed / Microservices

```text
ASP.NET Core
→ Docker
→ Kubernetes
→ Distributed Systems
→ Microservices Architecture
```

Các module code-first hiện có:

- [SQL / SQL Server](../05-sql/README.md)
- [ASP.NET Core](../07-aspnet-core/README.md)
- [Docker](../12-docker/README.md)
- [Kubernetes](../15-kubernetes/README.md)
- [Distributed Systems](../17-distributed-systems/README.md)
- **[Microservices Architecture](../18-microservices-architecture/README.md)**

Microservices module tập trung vào:

```text
Business capability / Bounded Context
→ Service Boundary
→ Data Ownership
→ API/Event Contract
→ Sync/Async Communication
→ Saga + Reconciliation
→ Independent Deployment
→ Testing/Observability/Migration
```

### AI Engineering

```text
IChatClient
→ Structured Output
→ Tool Calling
→ RAG
→ Evaluation
→ Observability
```

Điểm vào: **[Module 19 — AI Engineering](../19-ai-engineering/README.md)**.

### AI Coding Agents

```text
Task
→ inspect repo
→ plan
→ edit
→ build/test
→ diff/security review
→ PR
→ human review
```

Điểm vào: **[AI Coding Agents](../21-ai-coding-agents/README.md)**.

---

## Trạng thái hiện tại

| Phạm vi | Trạng thái |
| --- | --- |
| Module 00–04 | Content v1 / quality reference |
| **Module 05 SQL** | **Code-first deep rewrite v1: 4 guides** |
| Module 06 API Design | Structure v1; deep rewrite pending |
| **Module 07 ASP.NET Core** | **Code-first deep rewrite v1: 3 guides** |
| Module 08–11 | Structure v1; deep rewrite pending |
| **Module 12 Docker** | **Code-first deep rewrite v1: 3 guides** |
| Module 13–14 | Structure v1; deep rewrite pending |
| **Module 15 Kubernetes** | **Code-first deep rewrite v1: 3 guides** |
| Module 16 Observability | foundation tích hợp vào ASP.NET/K8s; dedicated module planned |
| **Module 17 Distributed Systems** | **Code-first v1: 4 guides + references** |
| **Module 18 Microservices Architecture** | **Code-first v1: 4 guides + references** |
| **Module 19 AI Engineering** | **Code-first v1** |
| Module 20 RAG | foundation trong Module 19; dedicated deep module planned |
| Module 21 Business AI Agents/MCP | Planned — advanced-first |
| **AI Coding Agents** | **Code-first v1** |
| Module 22–26 | Planned |
| Module 27 Data Engineering | Planned / selective |

Chi tiết: [Master Roadmap](master-roadmap.md).

---

## Code-heavy pages nên đọc

### Distributed Systems

1. [Distributed Systems Overview](../17-distributed-systems/README.md)
2. [Partial Failure / Retry / Idempotency](../17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md)
3. [Messaging / Outbox / Inbox / Dedup](../17-distributed-systems/messaging-outbox-inbox-and-dedup.md)
4. [Consistency / Ordering / Saga / Backpressure](../17-distributed-systems/consistency-ordering-saga-and-backpressure.md)

### Microservices Architecture

1. [Microservices Overview](../18-microservices-architecture/README.md)
2. [Service Boundaries / Data Ownership / Contracts](../18-microservices-architecture/service-boundaries-data-ownership-and-contracts.md)
3. [Checkout Saga / Unknown Outcome / Reconciliation](../18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md)
4. [Communication / Gateway / Discovery / Deployment](../18-microservices-architecture/communication-gateway-discovery-and-deployment.md)
5. [Testing / Observability / Migration](../18-microservices-architecture/testing-observability-and-migration.md)

### AI

1. [AI Engineering cho .NET](../19-ai-engineering/README.md)
2. [Structured Output và Tool Calling](../19-ai-engineering/structured-output-and-tool-calling.md)
3. [RAG, Evaluation và Observability](../19-ai-engineering/rag-evaluation-and-observability.md)
4. [AI Coding Agents](../21-ai-coding-agents/README.md)
5. [Repository Context, MCP và Instructions](../21-ai-coding-agents/repository-context-mcp-and-instructions.md)
6. [Safe Agentic Coding Workflow](../21-ai-coding-agents/safe-agentic-coding-workflow.md)

---

## Quality Gate

Một chapter P0/P1 implementation-heavy cần, khi phù hợp:

- `Hiểu trong 5 phút`;
- mental model riêng của topic;
- minimal runnable code/config;
- production-oriented example;
- broken/failure example;
- command/test để verify;
- security/performance/observability;
- Architect Perspective.

Generic prose có thể copy sang technology khác chỉ là **outline**, không phải deep content.

---

## Chuẩn năng lực

| Level | Bạn phải làm được |
| --- | --- |
| L0 | biết vấn đề công nghệ giải quyết |
| L1 | vẽ mental model |
| L2 | implement use case điển hình |
| L3 | xử lý failure/security/performance |
| L4 | giải thích behavior bằng internals |
| L5 | design/review bằng requirements + trade-offs |

Evidence tốt:

```text
code chạy
test
execution plan
trace
load/eval report
failure experiment
PR review
ADR/runbook
```

“Đã đọc xong” không phải evidence.

---

## Quy tắc nguồn

- Official English documentation là source of truth.
- Vietnamese resources hỗ trợ giải thích, không ghi đè behavior/version mới hơn.
- Framework/library API phụ thuộc version phải kiểm tra official docs và Context7 khi có.
- roadmap.sh dùng để phát hiện breadth/scope, không phải canonical implementation docs.

Xem [Source Policy](source-policy.md) và [Technology Baseline](technology-baseline.md).

## Verification metadata

- Verified: 2026-08-13
- Active code-first tracks: SQL, ASP.NET Core, Docker, Kubernetes, Distributed Systems, Microservices Architecture, AI Engineering, AI Coding Agents
