# AI-Enabled Software Architect Roadmap

> **Tiếng Việt để hiểu nhanh · thuật ngữ English giữ nguyên · học bằng code + failure experiment + architecture reasoning.**

Nếu tài liệu dài, bắt đầu bằng [Cách đọc tài liệu này](00-roadmap/how-to-read.md). Bạn không cần đọc từ đầu tới cuối.

## Chọn điểm bắt đầu

### .NET Backend

```text
C#/.NET → Backend → SQL → API Design → ASP.NET Core
```

Bắt đầu: [C#/.NET Runtime](03-dotnet/README.md).

### Production / Platform / Distributed / Microservices

```text
Docker
→ Kubernetes
→ Distributed Systems
→ Microservices Architecture
```

Bắt đầu:

- [Docker](12-docker/README.md)
- [Kubernetes](15-kubernetes/README.md)
- [Distributed Systems](17-distributed-systems/README.md)
- **[Microservices Architecture](18-microservices-architecture/README.md)**

Microservices module có case study checkout thực tế:

```text
Duplicate checkout
→ Idempotency
→ Inventory reservation
→ Payment attempt
→ timeout = UNKNOWN
→ PENDING_PAYMENT
→ reconciliation
→ Saga compensation
→ Outbox / Inbox / Dedup
```

### System Design

Bắt đầu: **[System Design](24-system-design/README.md)**.

Đây là module tổng hợp các kiến thức phía trên thành decision process:

```text
Requirements / NFR
→ Capacity Estimates
→ Traffic / Cache / CDN / Load Balancer
→ Data / Replication / Partitioning / Consistency
→ Queue / Backpressure / Reliability
→ SLO / Multi-region / DR / Security / Cost
→ Case Study
→ Architecture Trade-off
```

Có case study cho:

- URL Shortener;
- Notification System;
- Distributed Checkout;
- Enterprise AI Assistant;
- News Feed;
- Large File / Media Processing.

### AI Engineering

Bắt đầu: **[AI Engineering cho .NET](19-ai-engineering/README.md)**.

Có code cho:

- `IChatClient` và provider abstraction;
- structured output;
- tool calling;
- authorization/idempotency cho AI tools;
- RAG retrieval boundary;
- evaluation/regression gate;
- AI observability.

### AI Coding Agents

Bắt đầu: **[AI Coding Agents](21-ai-coding-agents/README.md)**.

Workflow:

```text
Task
→ inspect repo
→ plan
→ scoped edit
→ build/test
→ inspect diff
→ security checks
→ PR
→ human review
```

---

## Roadmap trong một hình

![Roadmap từ foundations đến Software / AI Architecture](assets/diagrams/roadmap-core-and-ai.svg)

---

## Cách học mỗi chapter

```text
1. Hiểu trong 5 phút
2. Chạy code/config/calculation
3. Vẽ mental model
4. Cố tình làm hỏng
5. Quan sát test/log/trace/plan
6. Đọc internals
7. Trả lời trade-offs
```

System Design thêm:

```text
8. Ghi assumptions
9. Estimate capacity
10. Ghi failure modes
11. Estimate cost
12. Nêu migration trigger
```

---

## Code-heavy pages nên đọc

### SQL

1. [SQL Overview](05-sql/README.md)
2. [Transactions / Isolation / Concurrency](05-sql/transactions-isolation-and-concurrency.md)
3. [Indexes / Execution Plans](05-sql/indexes-execution-plans-and-operations.md)
4. [EF Core → SQL → Execution Plan](05-sql/ef-core-query-shape-and-sql.md)

### API Design / ASP.NET Core

5. [API Design — 25-topic matrix](06-api-design/README.md)
6. [ASP.NET Core Overview](07-aspnet-core/README.md)
7. [Pipeline / Hosting / Configuration](07-aspnet-core/pipeline-hosting-and-configuration.md)
8. [Resilience / Security / Middleware](07-aspnet-core/resilience-security-and-middleware.md)
9. [Deployment / OTel / Operations](07-aspnet-core/deployment-observability-and-operations.md)

### Docker / Kubernetes

10. [Docker Overview](12-docker/README.md)
11. [Docker Runtime / Network / Storage](12-docker/runtime-networking-storage-and-resources.md)
12. [Kubernetes Overview](15-kubernetes/README.md)
13. [Kubernetes Architecture / Reconciliation](15-kubernetes/cluster-architecture-and-reconciliation.md)
14. [Kubernetes Workloads / Network / Storage](15-kubernetes/workloads-networking-and-storage.md)
15. [Kubernetes Security / Operations](15-kubernetes/kubernetes-security-observability-and-operations.md)

### Distributed Systems / Microservices

16. [Distributed Systems Overview](17-distributed-systems/README.md)
17. [Partial Failure / Retry / Idempotency](17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md)
18. [Messaging / Outbox / Inbox / Dedup](17-distributed-systems/messaging-outbox-inbox-and-dedup.md)
19. [Consistency / Ordering / Saga / Backpressure](17-distributed-systems/consistency-ordering-saga-and-backpressure.md)
20. [Microservices Overview](18-microservices-architecture/README.md)
21. [Checkout Saga / Unknown Outcome / Reconciliation](18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md)

### System Design

22. [System Design Overview](24-system-design/README.md)
23. [Requirements / NFR / Capacity](24-system-design/requirements-nfr-and-capacity-estimation.md)
24. [Traffic / Load Balancing / CDN / Cache](24-system-design/traffic-load-balancing-cdn-and-cache.md)
25. [Data / Replication / Partitioning / Consistency](24-system-design/data-partitioning-replication-and-consistency.md)
26. [Async / Queue / Backpressure / Reliability](24-system-design/async-queues-backpressure-and-reliability.md)
27. [Availability / Multi-region / DR / Security / Cost](24-system-design/availability-multiregion-dr-security-and-cost.md)
28. [Case Studies & Design Review](24-system-design/case-studies-and-design-review.md)

### AI

29. [AI Engineering cho .NET](19-ai-engineering/README.md)
30. [Structured Output / Tool Calling](19-ai-engineering/structured-output-and-tool-calling.md)
31. [RAG / Evaluation / Observability](19-ai-engineering/rag-evaluation-and-observability.md)
32. [AI Coding Agents](21-ai-coding-agents/README.md)
33. [Repository Context / MCP / Instructions](21-ai-coding-agents/repository-context-mcp-and-instructions.md)
34. [Safe Agentic Coding Workflow](21-ai-coding-agents/safe-agentic-coding-workflow.md)

---

## System Design: điều cần nhớ

```text
System Design ≠ collection of infrastructure logos
```

Một design phải có:

```text
requirements
+ measurable NFR
+ workload assumptions
+ capacity estimates
+ data ownership / consistency
+ failure model
+ security
+ operations / observability
+ cost
+ trade-offs
+ evolution path
```

Nếu không biết vì sao một box tồn tại thì box đó chưa được justify.

---

## Quality model

| Level | Bạn phải làm được |
|---|---|
| L0 | nói được công nghệ giải quyết vấn đề gì |
| L1 | vẽ được mental model |
| L2 | viết/chạy được code cơ bản |
| L3 | debug failure, security, performance |
| L4 | giải thích behavior bằng internals |
| L5 | chọn/loại giải pháp bằng requirements + trade-offs |

Một chapter P0/P1 implementation-heavy không được gọi là deep content nếu chỉ có prose. Tối thiểu cần **code/config/calculation + production example + failure experiment + verification** khi chủ đề cho phép.

## Trạng thái hiện tại

- Module 01–04: quality reference.
- **Module 05 SQL: code-first deep rewrite v1.**
- **Module 06 API Design: code-first deep rewrite v1, 25-topic coverage.**
- **Module 07 ASP.NET Core: code-first deep rewrite v1.**
- **Module 12 Docker: code-first deep rewrite v1.**
- **Module 15 Kubernetes: code-first deep rewrite v1.**
- **Module 17 Distributed Systems: code-first v1.**
- **Module 18 Microservices Architecture: code-first v1.**
- **Module 19 AI Engineering: code-first v1.**
- **AI Coding Agents: code-first v1.**
- **Module 24 System Design: code-first v1 với capacity/failure/cost/case studies.**
- Các module còn lại tiếp tục được rewrite/triển khai theo cùng quality gate.

## Đích đến

**AI-enabled Software Architect** không cần thuộc mọi tool. Bạn cần trả lời bằng evidence:

- requirement/NFR là gì;
- peak workload và data growth bao nhiêu;
- boundary và data ownership nằm ở đâu;
- transaction/consistency/retry/idempotency ra sao;
- queue/backpressure/order/failure recovery thế nào;
- SLO/RTO/RPO và DR ra sao;
- data/AI/tool được phép truy cập gì;
- latency/cost/quality được đo ra sao;
- khi nào dùng cache/sharding/multi-region/microservices và khi nào không;
- kiến trúc đổi ra sao ở 10x/100x scale.
