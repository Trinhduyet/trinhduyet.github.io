# Roadmap Overview

> Nếu bạn thấy tài liệu dài và khó đọc, bắt đầu bằng **[Cách đọc tài liệu này](how-to-read.md)**.

Kho tri thức này dành cho kỹ sư .NET/Backend muốn tiến tới **AI-enabled Software Architect** bằng code, test, failure experiment và architecture reasoning.

## Bắt đầu theo mục tiêu

### Backend/.NET

```text
C#/.NET → Backend → SQL → API Design → ASP.NET Core
```

Điểm vào: [Module 03 — .NET](../03-dotnet/README.md).

### Production/DevOps/Architect

```text
Testing/Security/Performance
→ Docker/DevOps/Kubernetes
→ Distributed Systems
→ System Design
→ Architecture
```

Điểm vào: [Master Roadmap](master-roadmap.md).

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

Điểm vào: **[Module 21A — AI Coding Agents](../21-ai-coding-agents/README.md)**.

---

## Trạng thái

| Phạm vi | Trạng thái |
| --- | --- |
| Module 00–04 | Content v1 / quality reference |
| Module 05–15 | Structure/coverage v1; deep rewrite + code pending |
| Module 16–18 | Planned |
| **Module 19 AI Engineering** | **Code-first v1 available** |
| Module 20 RAG | Foundation đã có trong Module 19; deep module planned |
| Module 21 Business AI Agents/MCP | Planned — advanced-first |
| **Module 21A AI Coding Agents** | **Code-first v1 available** |
| Module 22–26 | Planned |

Chi tiết đầy đủ: [Master Roadmap](master-roadmap.md).

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
Quan sát test/log/trace
  ↓
Đọc internals
  ↓
Trả lời trade-offs
```

Một chapter implementation-heavy không được gọi là deep content nếu thiếu code/config và verification.

---

## Quality Gate

### Tối thiểu cho P0/P1

- mental model riêng của topic;
- minimal code/config;
- production-oriented example;
- failure experiment;
- verification/test;
- common mistakes;
- security/performance/observability khi liên quan;
- Architect Perspective.

Generic prose có thể copy sang technology khác chỉ là **outline/structure**, không phải `Content v1`.

---

## Các trang code-heavy nên đọc trước

### AI

1. [AI Engineering cho .NET](../19-ai-engineering/README.md)
2. [Structured Output và Tool Calling](../19-ai-engineering/structured-output-and-tool-calling.md)
3. [RAG, Evaluation và Observability](../19-ai-engineering/rag-evaluation-and-observability.md)
4. [AI Coding Agents](../21-ai-coding-agents/README.md)
5. [Repository Context, MCP và Instructions](../21-ai-coding-agents/repository-context-mcp-and-instructions.md)
6. [Safe Agentic Coding Workflow](../21-ai-coding-agents/safe-agentic-coding-workflow.md)

### .NET/Backend

- [Async/Await, Cancellation và Task Lifecycle](../03-dotnet/async-await-cancellation-and-task-lifecycle.md)
- [Request Lifecycle và Endpoint Contract](../04-backend/request-lifecycle-and-endpoint-contract.md)

Hai chapter này tiếp tục là quality reference cho cách viết sâu.

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
unit/integration test
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
- Vietnamese resources dùng để giải thích nhanh, không ghi đè behavior/version mới hơn.
- Framework/library API phụ thuộc version phải được kiểm tra official docs và Context7 khi có.
- roadmap.sh dùng để xác định breadth/scope, không dùng làm canonical implementation docs.

Xem [Source Policy](source-policy.md) và [Technology Baseline](technology-baseline.md).

## Verification metadata

- Verified: 2026-08-12
- Scope: roadmap.sh + official technology documentation
- New active tracks: AI Engineering + AI Coding Agents
