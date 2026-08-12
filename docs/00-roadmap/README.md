# Roadmap Overview

> Nếu tài liệu dài, bắt đầu bằng **[Cách đọc tài liệu này](how-to-read.md)**. Mục tiêu là chạy code và failure experiment, không đọc tuần tự hàng trăm trang.

Kho tri thức này đi từ .NET Backend tới **AI-enabled Software Architect**.

## Chọn điểm bắt đầu

### .NET Backend

```text
C#/.NET → Backend → SQL → API Design → ASP.NET Core
```

Điểm vào: [Module 03 — .NET](../03-dotnet/README.md).

### Production / Distributed Systems

```text
ASP.NET Core
→ Docker
→ Kubernetes
→ Distributed Systems
```

Các module code-first hiện có:

- [SQL / SQL Server](../05-sql/README.md)
- [ASP.NET Core](../07-aspnet-core/README.md)
- [Docker](../12-docker/README.md)
- [Kubernetes](../15-kubernetes/README.md)
- [Distributed Systems](../17-distributed-systems/README.md)

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
| Module 16 Observability | foundation đã tích hợp vào ASP.NET/K8s; dedicated module planned |
| **Module 17 Distributed Systems** | **Code-first v1: 4 guides + references** |
| Module 18 Data Engineering | Planned |
| **Module 19 AI Engineering** | **Code-first v1** |
| Module 20 RAG | foundation trong Module 19; dedicated deep module planned |
| Module 21 Business AI Agents/MCP | Planned — advanced-first |
| **AI Coding Agents** | **Code-first v1** |
| Module 22–26 | Planned |

Chi tiết: [Master Roadmap](master-roadmap.md).

---

## Cách học một chapter

```text
Hiểu trong 5 phút
  ↓
Chạy code/config
  ↓
Vẽ mental model
  ↓
Cố tình làm hỏng
  ↓
Quan sát test/log/trace/plan
  ↓
Đọc internals
  ↓
Trả lời trade-offs
```

---

## Code-heavy pages nên đọc

### SQL

1. [SQL Overview](../05-sql/README.md)
2. [Transactions, Isolation và Concurrency](../05-sql/transactions-isolation-and-concurrency.md)
3. [Indexes & Execution Plans](../05-sql/indexes-execution-plans-and-operations.md)
4. [EF Core → SQL → Execution Plan](../05-sql/ef-core-query-shape-and-sql.md)

### ASP.NET Core

1. [ASP.NET Core Overview](../07-aspnet-core/README.md)
2. [Pipeline / Hosting / Configuration](../07-aspnet-core/pipeline-hosting-and-configuration.md)
3. [Resilience / Security / Middleware](../07-aspnet-core/resilience-security-and-middleware.md)
4. [Deployment / OTel / Operations](../07-aspnet-core/deployment-observability-and-operations.md)

### Docker / Kubernetes

- [Docker Overview](../12-docker/README.md)
- [Docker Runtime / Network / Storage](../12-docker/runtime-networking-storage-and-resources.md)
- [Kubernetes Overview](../15-kubernetes/README.md)
- [Kubernetes Architecture / Reconciliation](../15-kubernetes/cluster-architecture-and-reconciliation.md)
- [Kubernetes Workloads / Network / Storage](../15-kubernetes/workloads-networking-and-storage.md)
- [Kubernetes Security / Operations](../15-kubernetes/kubernetes-security-observability-and-operations.md)

### Distributed Systems

1. [Distributed Systems Overview](../17-distributed-systems/README.md)
2. [Partial Failure / Retry / Idempotency](../17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md)
3. [Messaging / Outbox / Inbox / Dedup](../17-distributed-systems/messaging-outbox-inbox-and-dedup.md)
4. [Consistency / Ordering / Saga / Backpressure](../17-distributed-systems/consistency-ordering-saga-and-backpressure.md)

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

- Verified: 2026-08-12
- Active code-first tracks: SQL, ASP.NET Core, Docker, Kubernetes, Distributed Systems, AI Engineering, AI Coding Agents
