# Skills Matrix — Capability, Target Depth & Evidence

> Public repository không hard-code “current skill level” của một cá nhân. Matrix này mô tả **capability cần đạt, target depth, content maturity và evidence thực sự có trong repo**.

Learner progress nên copy vào [Progress Template](progress-template.md) và chỉ tăng level khi có evidence.

## Level model

| Level | Capability | Evidence tối thiểu |
|---:|---|---|
| 1 | Awareness | mô tả problem/use case/vocabulary |
| 2 | Explain | mental model + failure + trade-off |
| 3 | Implement | code/config đúng + tests |
| 4 | Operate | deploy/observe/debug/recover |
| 5 | Design / Review | chọn/loại solution bằng requirements, evidence, cost |

Target level thay đổi theo role. Xem [Role-based Learning Paths](role-based-learning-paths.md).

## Content maturity

```text
Reference < Guided < Deep
```

`Runnable lab` là dimension riêng; Deep content có thể chưa có dedicated executable lab.

---

# Foundations

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| CS | complexity/workload reasoning | P1 | 3 | Deep | **Yes** | [Complexity](../01-computer-science/complexity-and-workload-reasoning.md) + `labs/01-computer-science` |
| CS | data structures by backend workload | P1/P2 | 3 | Deep | **Yes** | [Data structures](../01-computer-science/data-structures-for-backend-systems.md) |
| CS | process/thread/scheduling/concurrency | P0 | 4 | Deep | **Yes** | [Scheduling](../01-computer-science/process-thread-scheduling-and-concurrency.md) |
| CS | memory/virtual memory/cache locality | P0/P1 | 4 | Deep | **Yes** | [Memory](../01-computer-science/memory-stack-heap-virtual-memory-and-cache.md) |
| Linux | filesystem/permissions/identity | P0 | 4 | Deep | **Yes** | [Filesystem](../02-linux-git-networking/filesystem-permissions-and-identities.md) |
| Linux | process/signals/resource pressure | P0 | 4 | Deep | **Yes** | [Processes](../02-linux-git-networking/process-signals-and-resource-pressure.md) |
| Git | history/diff/branch/revert/recovery | P0 | 4 | Deep | **Yes** | [Git mental model](../02-linux-git-networking/git-mental-model-and-safe-recovery.md) |
| Networking | DNS/TCP/TLS/HTTP | P0 | 5 | Deep | **Yes** | [Protocol deep dive](../02-linux-git-networking/dns-tcp-tls-http-deep-dive.md) |
| Networking | proxy/NAT/load balancer | P0/P1 | 4 | Deep | **Yes** | [Network boundaries](../02-linux-git-networking/proxy-nat-load-balancer-and-network-boundaries.md) + incident lab |

---

# .NET & Backend

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| C# | types/generics/collections/LINQ | P0 | 4 | Deep | **Yes** | [Types](../03-dotnet/csharp-types-generics-and-collections.md) + `labs/03-dotnet` |
| .NET | exceptions/IDisposable/resource ownership | P0 | 5 | Deep | **Yes** | [Resource ownership](../03-dotnet/exceptions-disposable-and-resource-ownership.md) |
| .NET | async/await/CancellationToken | P0 | 5 | Deep | **Yes** | [Async/cancellation](../03-dotnet/async-await-cancellation-and-task-lifecycle.md) |
| .NET | ThreadPool/concurrency diagnosis | P0 | 5 | Deep | **Yes** | [ThreadPool](../03-dotnet/threadpool-concurrency-and-diagnostics.md) |
| .NET | GC/allocations/runtime memory | P0 | 4 | Deep | **Yes** | [GC](../03-dotnet/gc-allocations-and-runtime-memory.md) |
| Backend | HTTP request lifecycle | P0 | 5 | Deep | **Yes** | [Request lifecycle](../04-backend/request-lifecycle-and-endpoint-contract.md) + `labs/04-backend` |
| Backend | AuthN/AuthZ/validation | P0 | 5 | Deep | **Yes** | [Authentication/Authorization](../04-backend/authentication-authorization-and-validation.md) |
| Backend | pagination/idempotency/rate limiting/cache boundary | P0 | 5 | Deep | **Yes** | [Idempotency & traffic](../04-backend/pagination-idempotency-rate-limiting-and-caching.md) |
| Backend | background jobs/files/webhooks | P0 | 4 | Deep | **Yes** | [Jobs/webhooks](../04-backend/background-jobs-files-and-webhooks.md) |

---

# SQL, API & ASP.NET Core

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| SQL | relational model/schema/constraints | P0 | 5 | Deep | No dedicated | [Relational model](../05-sql/relational-model-schema-and-sql.md) |
| SQL | joins/windows/CTE/query reasoning | P0 | 4 | Deep | No dedicated | [Relational model](../05-sql/relational-model-schema-and-sql.md) |
| SQL | transactions/isolation/locks/deadlocks | P0 | 5 | Deep | No dedicated | [Transactions](../05-sql/transactions-isolation-and-concurrency.md) |
| SQL | indexes/statistics/execution plans | P0 | 5 | Deep | No dedicated | [Indexes & plans](../05-sql/indexes-execution-plans-and-operations.md) |
| EF Core | LINQ → SQL → plan/query shape | P0 | 5 | **Deep/Active** | No dedicated | [EF Core query shape](../05-sql/ef-core-query-shape-and-sql.md) |
| API | HTTP resources/methods/status/errors | P0 | 5 | Deep | No dedicated | [HTTP contracts](../06-api-design/http-resource-contracts-and-semantics.md) |
| API | auth/OAuth/CORS/security boundary | P0 | 5 | Deep | No dedicated | [API security](../06-api-design/security-auth-oauth-and-cors.md) |
| API | evolution/versioning/compatibility | P0 | 5 | Deep | No dedicated | [API evolution](../06-api-design/api-evolution-errors-and-pagination.md) |
| API | traffic/cache/rate-limit/resilience | P0 | 5 | Deep | No dedicated | [Traffic & resilience](../06-api-design/traffic-caching-rate-limits-and-resilience.md) |
| API | REST/gRPC/events/webhooks/realtime decision | P1 | 4/5 | Deep | No dedicated | [Events/gRPC](../06-api-design/events-grpc-webhooks-and-contracts.md) |
| ASP.NET | hosting/pipeline/routing/config | P0 | 5 | Deep | No dedicated | [Pipeline/hosting](../07-aspnet-core/pipeline-hosting-and-configuration.md) |
| ASP.NET | resilience/security/middleware/health | P0 | 5 | Deep | No dedicated | [Resilience](../07-aspnet-core/resilience-security-and-middleware.md) |
| ASP.NET | deployment/observability/operations | P0 | 4/5 | Deep | No dedicated | [Operations](../07-aspnet-core/deployment-observability-and-operations.md) |

---

# Production Engineering

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| Testing | unit/integration/API/database/contract strategy | P0 | 5 | Deep | No dedicated | [Test strategy](../08-testing-code-review/test-strategy-and-boundaries.md) |
| Testing | load/resilience/security gates | P0 | 4 | Deep | No dedicated | [Integration/contract/load](../08-testing-code-review/integration-contract-and-load-testing.md) |
| Review | correctness/security/perf/ops review | P0 | 5 | Deep | No dedicated | [Code review](../08-testing-code-review/code-review-quality-and-failure-analysis.md) |
| Security | threat modeling/trust boundaries | P0 | 5 | Deep/Guided | No | [Module 09](../09-security-devsecops/README.md) |
| Security | OAuth/OIDC/AuthZ/secrets/data protection | P0 | 5 | Deep/Guided | No | [Identity/secrets](../09-security-devsecops/identity-secrets-and-data-protection.md) |
| DevSecOps | dependencies/secrets/SAST/SBOM/supply chain | P1 | 4 | Deep/Guided | No | [Supply chain](../09-security-devsecops/secure-supply-chain-and-devsecops.md) |
| Performance | latency/throughput/tails/saturation | P0 | 5 | Deep/Guided | No | [Module 10](../10-performance/README.md) |
| Performance | profiling/load/capacity/bottleneck | P0 | 5 | Deep/Guided | No | [Load & capacity](../10-performance/load-capacity-and-scalability.md) |
| Performance | budgets/regression control | P0/P1 | 4 | Deep/Guided | No | [Regression control](../10-performance/optimization-budgets-and-regression-control.md) |
| Redis | data structures/TTL/cache policy | P1 | 4 | Deep/Guided | No | [Module 11](../11-redis-caching/README.md) |
| Redis | invalidation/stampede/failure | P1 | 5 | Deep/Guided | No | [Consistency](../11-redis-caching/cache-consistency-invalidation-and-stampede.md) |
| Redis | memory/eviction/HA/coordination | P1 | 4/5 | Deep/Guided | No | [Operations](../11-redis-caching/redis-operations-ha-and-coordination.md) |
| Docker | image/layers/build/registry identity | P0 | 4 | Deep/Guided | No | [Images](../12-docker/images-builds-and-reproducibility.md) |
| Docker | runtime/network/storage/signals/resources | P0 | 5 | Deep/Guided | No | [Runtime](../12-docker/runtime-networking-storage-and-resources.md) |
| Docker | security/Compose/operations | P0/P1 | 4 | Deep/Guided | No | [Security/Compose](../12-docker/docker-security-compose-and-operations.md) |

---

# DevOps, Azure & Kubernetes

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| DevOps | CI/CD/artifact promotion/rollback | P1 | 5 | Deep/Guided | No | [CI/CD](../13-devops-iac/ci-cd-artifacts-and-promotion.md) |
| Terraform | state/plan/apply/modules/drift | P1 | 4 | Deep/Guided | No | [Terraform](../13-devops-iac/terraform-state-modules-and-drift.md) |
| Delivery | testing → image → registry → K8s/GitOps | P1 | 4/5 | Deep/Guided | No | [Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md) |
| Azure | landing zone/resource hierarchy/governance | P1 | 4 | Deep handbook | No | [Azure foundations](../14-cloud/azure-foundations-resource-hierarchy-and-landing-zones.md) |
| Azure | identity/network/private access/edge | P0/P1 | 5 | Deep handbook | No | [Network/Security handbook](../14-cloud/azure-production-handbook-network-security.md) |
| Azure | compute service selection/config/cost | P0/P1 | 5 | Deep handbook | No | [Compute handbook](../14-cloud/azure-production-handbook-compute.md) |
| Azure | SQL/storage/cache/messaging selection | P0/P1 | 5 | Deep handbook | No | [Data/Messaging handbook](../14-cloud/azure-production-handbook-data-messaging.md) |
| Azure | monitor/delivery/backup/DR/cost | P0/P1 | 5 | Deep handbook | No | [Operations/Cost handbook](../14-cloud/azure-production-handbook-operations-cost.md) |
| Kubernetes | cluster/reconciliation/control plane/workers | P1 | 4/5 | Deep/Guided | No | [Architecture](../15-kubernetes/cluster-architecture-and-reconciliation.md) |
| Kubernetes | Deployment/Pod/Service/network/storage | P1 | 5 | Deep/Guided | No | [Workloads](../15-kubernetes/workloads-networking-and-storage.md) |
| Kubernetes | config/resources/scheduling/autoscaling | P1 | 4/5 | Deep/Guided | No | [Configuration/Scheduling](../15-kubernetes/application-configuration-and-scheduling.md) |
| Kubernetes | RBAC/security/observability/operations | P1 | 4/5 | Deep/Guided | No | [Security/Operations](../15-kubernetes/kubernetes-security-observability-and-operations.md) |
| Kubernetes | kubectl troubleshooting/CKAD application skills | P1 | 4/5 | Deep/Guided | No | [Debugging](../15-kubernetes/kubectl-debugging-and-ckad-practice.md) |
| AKS | map K8s core to Azure production platform | P1 | 4/5 | Deep/Guided | No | [AKS mapping](../15-kubernetes/aks-on-azure-production-architecture.md) |

---

# Distributed Systems & Microservices

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| Distributed | timeout/unknown outcome/retry/idempotency | P0 | 5 | **Deep/Active** | No | [Partial failure](../17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md) |
| Distributed | messaging/at-least-once/outbox/inbox/dedup | P0 | 5 | **Deep/Active** | No | [Messaging](../17-distributed-systems/messaging-outbox-inbox-and-dedup.md) |
| Distributed | consistency/ordering/saga/backpressure | P0 | 5 | **Deep/Active** | No | [Consistency](../17-distributed-systems/consistency-ordering-saga-and-backpressure.md) |
| Microservices | business boundaries/data ownership/contracts | P0/P1 | 5 | Deep/Active | No | [Boundaries](../18-microservices-architecture/service-boundaries-data-ownership-and-contracts.md) |
| Microservices | saga/unknown outcome/reconciliation | P0/P1 | 5 | Deep/Active | No | [Checkout saga](../18-microservices-architecture/checkout-saga-unknown-outcome-and-reconciliation.md) |
| Microservices | communication/gateway/discovery/deployment | P1 | 4/5 | Deep/Active | No | [Communication](../18-microservices-architecture/communication-gateway-discovery-and-deployment.md) |
| Microservices | testing/observability/migration | P1 | 4/5 | Deep/Active | No | [Testing/Migration](../18-microservices-architecture/testing-observability-and-migration.md) |

---

# System Design & Software Architecture

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| System Design | requirements/NFR/capacity estimation | P0 | 5 | **Deep/Active** | No direct | [Requirements/Capacity](../24-system-design/requirements-nfr-and-capacity-estimation.md) |
| System Design | traffic/LB/CDN/cache | P0 | 5 | **Deep/Active** | No direct | [Traffic](../24-system-design/traffic-load-balancing-cdn-and-cache.md) |
| System Design | data/replication/partition/consistency | P0 | 5 | **Deep/Active** | No direct | [Data](../24-system-design/data-partitioning-replication-and-consistency.md) |
| System Design | async/queue/backpressure/reliability | P0 | 5 | **Deep/Active** | No direct | [Async/Queues](../24-system-design/async-queues-backpressure-and-reliability.md) |
| System Design | availability/multi-region/DR/security/cost | P0/P1 | 5 | **Deep/Active** | No direct | [Availability/DR](../24-system-design/availability-multiregion-dr-security-and-cost.md) |
| System Design | case/design review | P0 | 5 | **Deep/Active** | No direct | [Case studies](../24-system-design/case-studies-and-design-review.md) |
| Architecture | quality attributes/boundaries/styles | P0 | 5 | **Deep/Active** | No direct | [Quality/Styles](../25-software-architecture/quality-attributes-boundaries-and-styles.md) |
| Architecture | DDD/modular monolith/microservices | P0 | 5 | **Deep/Active** | No direct | [DDD/Modularity](../25-software-architecture/ddd-modular-monolith-and-microservices.md) |
| Architecture | Clean/Hexagonal/Vertical Slice | P0/P1 | 5 | **Deep/Active** | No direct | [Application structure](../25-software-architecture/clean-hexagonal-and-vertical-slice.md) |
| Architecture | EDA/CQRS/integration | P0/P1 | 5 | **Deep/Active** | No direct | [Integration architecture](../25-software-architecture/event-driven-cqrs-and-integration.md) |
| Architecture | ADR/fitness functions/evolution | P0 | 5 | **Deep/Active** | No direct | [Evolution](../25-software-architecture/architecture-decisions-evolution-and-fitness-functions.md) |
| Architecture | architecture review | P0 | 5 | **Deep/Active** | No direct | [Review playbook](../25-software-architecture/architecture-review-playbook.md) |

---

# AI Engineering & Coding Agents

| Area | Capability | Priority | Typical target | Content | Runnable lab | Evidence path |
|---|---|---:|---:|---|---:|---|
| AI Engineering | model/provider abstraction + structured output | P0 | 4/5 | Deep/Guided | No | [AI Engineering](../19-ai-engineering/README.md) |
| AI Tools | tool calling + application authorization | P0 | 5 | Deep/Guided | No | [Structured Output & Tools](../19-ai-engineering/structured-output-and-tool-calling.md) |
| RAG | ingestion/retrieval/ACL/deletion/versioning | P0 | 4/5 | **Integrated in 19** | No | [RAG/Eval/Observability](../19-ai-engineering/rag-evaluation-and-observability.md) |
| AI Evaluation | dataset/regression/latency/cost | P0 | 5 | **Integrated in 19** | No | [RAG/Eval/Observability](../19-ai-engineering/rag-evaluation-and-observability.md) |
| AI Security | injection/tool abuse/data boundary | P0 | 4/5 | Integrated 19/21 | No | [AI Engineering](../19-ai-engineering/README.md) + [Coding Agents](../21-ai-coding-agents/README.md) |
| Coding Agents | repo context/instructions/task scoping | P0/P1 | 4 | Deep/Guided | No | [Repository Context](../21-ai-coding-agents/repository-context-mcp-and-instructions.md) |
| Coding Agents | permissions/sandbox/build-test-review workflow | P0/P1 | 4/5 | Deep/Guided | No | [Safe Agentic Workflow](../21-ai-coding-agents/safe-agentic-coding-workflow.md) |

---

# Integrated capabilities without dedicated module

Không gán trạng thái `Planned` cho capability đã có content sâu ở module khác.

| Capability | Current home | Status |
|---|---|---|
| Observability / OTel / SLO | 07, 10, 14, 15, 17, 19 | Integrated; dedicated module optional later |
| RAG | 19 | Integrated |
| AI Security | 19, 21 | Integrated foundation; dedicated red-team track future candidate |
| GenAIOps | 19 | Integrated foundation; executable release/eval lab pending |
| Architecture documentation | 24, 25 | Integrated; reusable examples can expand |
| Data Engineering | — | Selective/future P2 |

---

# Repository evidence gap

Current dedicated runnable lab directories cover modules 01–04. That means the highest-leverage backlog is **not adding more rows**; it is turning later deep content into executable integrated scenarios.

Priority:

```text
05–08 Production Backend
09–13 Production Delivery
15 Kubernetes Local
17 Distributed Reliability
18 Checkout Saga
14 Azure IaC
19 Production AI
```

→ [Repository Quality Review](repository-quality-review-2026-08-28.md)

## Update rule

Learner level only increases when evidence exists.

```text
"đã đọc"
"đã dùng 3 năm"
"đã xem tutorial"
```

không tự động nâng level.

Use:

```text
behavior test
failure reproduction
debug evidence
deployment/recovery
measured trade-off
design review
```

## Verification metadata

- Rebuilt: 2026-08-28.
- Scope aligned with actual repository modules at review time.
- Removed public user-specific assumed current levels.
- Distributed Systems, System Design, Software Architecture and EF Core statuses corrected from stale `Planned` labels to current active/deep coverage.
