# Skills matrix

## Cách chấm level

| Level | Ý nghĩa | Bằng chứng tối thiểu |
| --- | --- | --- |
| 0 | Unknown | Chưa đánh giá |
| 1 | Awareness | Mô tả use case và vocabulary |
| 2 | Can Explain | Giải thích mental model, failure và trade-off |
| 3 | Can Implement | Tạo implementation đúng và có test |
| 4 | Can Operate | Deploy, observe, diagnose, recover |
| 5 | Can Design / Review | Chọn/loại bỏ phương án bằng requirements, evidence, cost |

Ký hiệu “current → target”. Level current bằng 0 cho đến khi có assessment evidence. Dấu * chỉ input tự khai báo, chưa phải verified evidence.

## Matrix

| Area | Topic | Priority | Level | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| CS | Big-O và workload reasoning | P1 | 0 → 3 | In progress | [Complexity chapter](../01-computer-science/complexity-and-workload-reasoning.md) + WorkloadLab evidence pending |
| CS | Arrays/lists/hash/heap/queue/graph | P1/P2 | 0 → 3 | In progress | [Data structures chapter](../01-computer-science/data-structures-for-backend-systems.md) + learner decision evidence pending |
| CS | Process/thread/scheduling | P0 | 0 → 4 | In progress | [Scheduling/concurrency chapter](../01-computer-science/process-thread-scheduling-and-concurrency.md) + race/diagnostic evidence pending |
| CS | Memory/stack/heap/virtual memory | P0 | 0 → 4 | In progress | [Memory/cache chapter](../01-computer-science/memory-stack-heap-virtual-memory-and-cache.md) + locality/memory investigation pending |
| Linux | Filesystem, permissions, users/groups | P0 | 0 → 4 | In progress | [Filesystem chapter](../02-linux-git-networking/filesystem-permissions-and-identities.md) + permission failure lab output pending |
| Linux | Process, signals, resource diagnosis | P0 | 0 → 4 | In progress | [Process/resource chapter](../02-linux-git-networking/process-signals-and-resource-pressure.md) + lab output pending |
| Git | History, diff, branch, merge, revert/recovery | P0 | 0 → 4 | In progress | [Git recovery chapter](../02-linux-git-networking/git-mental-model-and-safe-recovery.md) + learner recovery exercise pending |
| Networking | DNS/TCP/UDP/TLS/HTTP | P0 | 0 → 5 | In progress | [Protocol deep dive](../02-linux-git-networking/dns-tcp-tls-http-deep-dive.md) + layer-by-layer lab output pending |
| Networking | NAT, proxy, load balancer | P0/P1 | 0 → 4 | In progress | [Network boundaries chapter](../02-linux-git-networking/proxy-nat-load-balancer-and-network-boundaries.md) + [incident lab](../02-linux-git-networking/incident-lab-dotnet-service.md) output pending |
| C# | Type system, generics, collections, LINQ | P0 | 0 → 4 | In progress | [Types chapter](../03-dotnet/csharp-types-generics-and-collections.md) + Project 01 code/review |
| .NET | Exceptions, IDisposable, resource ownership | P0 | 0 → 5 | In progress | [Ownership chapter](../03-dotnet/exceptions-disposable-and-resource-ownership.md) + failure/cleanup tests |
| .NET | Task, async/await, CancellationToken | P0 | 0 → 5 | In progress | [Async chapter](../03-dotnet/async-await-cancellation-and-task-lifecycle.md) + RuntimeLab cancel evidence |
| .NET | ThreadPool và concurrency | P0 | 0 → 5 | In progress | [ThreadPool chapter](../03-dotnet/threadpool-concurrency-and-diagnostics.md) + saturation/race lab |
| .NET | GC, allocations, memory | P0 | 0 → 4 | In progress | [GC chapter](../03-dotnet/gc-allocations-and-runtime-memory.md) + RuntimeLab allocation/diagnostics |
| .NET | Generic Host, DI, config, logging | P0 | 0 → 5 | In progress | [ThreadPool/Host chapter](../03-dotnet/threadpool-concurrency-and-diagnostics.md) + hosted app/lifecycle tests |
| Backend | HTTP request lifecycle | P0 | 0 → 5 | In progress | [Request lifecycle](../04-backend/request-lifecycle-and-endpoint-contract.md) + trace từ socket đến handler |
| Backend | Authn/authz/validation | P0 | 0 → 5 | In progress | [Auth chapter](../04-backend/authentication-authorization-and-validation.md) + threat tests/policy matrix |
| Backend | Pagination/filtering/sorting | P0 | 0 → 4 | In progress | [Capacity chapter](../04-backend/pagination-idempotency-rate-limiting-and-caching.md) + BackendLab contract evidence |
| Backend | Idempotency/rate limiting/caching | P0 | 0 → 5 | In progress | [Capacity chapter](../04-backend/pagination-idempotency-rate-limiting-and-caching.md) + duplicate/overload experiments |
| Backend | Background jobs/files/webhooks | P0 | 0 → 4 | In progress | [Integration chapter](../04-backend/background-jobs-files-and-webhooks.md) + recovery/replay tests |
| SQL | Relational model/schema/constraints | P0 | 0 → 5 | In progress | [Relational chapter](../05-sql/relational-model-schema-and-sql.md) + data model review |
| SQL | Querying, joins, windows, CTEs | P0 | 0 → 4 | In progress | [Relational chapter](../05-sql/relational-model-schema-and-sql.md) + query portfolio |
| SQL | Transactions/isolation/locking/deadlocks | P0 | 0 → 5 | In progress | [Transactions chapter](../05-sql/transactions-isolation-and-concurrency.md) + blocking/deadlock lab |
| SQL | Indexes/statistics/execution plans | P0 | 0 → 5 | In progress | [Plans chapter](../05-sql/indexes-execution-plans-and-operations.md) + actual plan analysis |
| SQL Server | Query Store/plan cache/parameterization | P0/P1 | 0 → 4 | In progress | [Plans chapter](../05-sql/indexes-execution-plans-and-operations.md) + incident diagnosis |
| SQL Server | tempdb/log/storage/backup overview | P1/P2 | 0 → 4 | In progress | [Plans chapter](../05-sql/indexes-execution-plans-and-operations.md) + recovery review |
| EF Core | Query translation/tracking/loading | P0 | 0 → 5 | Planned | LINQ → SQL → plan → index evidence |
| EF Core | Transactions/concurrency/migrations | P0 | 0 → 4 | Planned | Migration/rollback exercise |
| API Design | Resources, methods, status/error model | P0 | 0 → 5 | In progress | [HTTP contract](../06-api-design/http-resource-contracts-and-semantics.md) + API review |
| API Design | Evolution/versioning/compatibility | P0 | 0 → 5 | In progress | [Evolution chapter](../06-api-design/api-evolution-errors-and-pagination.md) + breaking-change exercise |
| API Design | REST/RPC/gRPC/GraphQL/events | P1 | 0 → 5 | In progress | [Events chapter](../06-api-design/events-grpc-webhooks-and-contracts.md) + decision record |
| ASP.NET Core | Hosting/pipeline/routing/binding | P0 | 0 → 5 | In progress | [Pipeline chapter](../07-aspnet-core/pipeline-hosting-and-configuration.md) + request trace |
| ASP.NET Core | Security/rate limit/health checks | P0 | 0 → 5 | In progress | [Resilience chapter](../07-aspnet-core/resilience-security-and-middleware.md) + probe tests |
| ASP.NET Core | HttpClientFactory/workers/caching | P0 | 0 → 4 | In progress | [Operations chapter](../07-aspnet-core/deployment-observability-and-operations.md) + dependency lab |
| Testing | Unit/integration/API/database/contract | P0 | 0 → 5 | In progress | [Test strategy](../08-testing-code-review/test-strategy-and-boundaries.md) + boundary evidence |
| Testing | Load/resilience/security/AI eval | P0 | 0 → 4 | In progress | [Load testing](../08-testing-code-review/integration-contract-and-load-testing.md) + automated gates |
| Code Review | Correctness/design/security/perf/ops | P0 | 0 → 5 | In progress | [Review chapter](../08-testing-code-review/code-review-quality-and-failure-analysis.md) + multi-role report |
| Security | OAuth/OIDC/JWT/cookies/CSRF/CORS | P0 | 0 → 5 | In progress | [Identity chapter](../09-security-devsecops/identity-secrets-and-data-protection.md) + threat tests |
| Security | Injection/SSRF/secrets/TLS/least privilege | P0 | 0 → 5 | In progress | [Threat modeling](../09-security-devsecops/threat-modeling-and-application-security.md) + mitigation lab |
| DevSecOps | SAST/dependencies/secrets/SBOM/supply chain | P1 | 0 → 4 | In progress | [Supply-chain chapter](../09-security-devsecops/secure-supply-chain-and-devsecops.md) + CI gates |
| Performance | Latency/throughput/tails/resources | P0 | 0 → 5 | In progress | [Measurement](../10-performance/measurement-profiling-and-bottlenecks.md) + performance report |
| Performance | Profiling/load/capacity | P0 | 0 → 5 | In progress | [Capacity chapter](../10-performance/load-capacity-and-scalability.md) + bottleneck cycle |
| Redis | Data structures/TTL/cache patterns | P1 | 0 → 4 | In progress | [Redis data types](../11-redis-caching/redis-data-structures-and-command-shape.md) + cache lab |
| Redis | Persistence/replication/eviction/failure | P1 | 0 → 5 | In progress | [Redis operations](../11-redis-caching/redis-operations-ha-and-coordination.md) + outage/stampede lab |
| Docker | Image/layers/build/cache/registry | P0 | 0 → 4 | In progress | [Docker images](../12-docker/images-builds-and-reproducibility.md) + reproducible image |
| Docker | Runtime/network/volume/signals/resources | P0 | 0 → 5 | In progress | [Docker runtime](../12-docker/runtime-networking-storage-and-resources.md) + failure lab |
| DevOps | Git strategy/CI/CD/artifact promotion | P1 | 0 → 5 | In progress | [CI/CD chapter](../13-devops-iac/ci-cd-artifacts-and-promotion.md) + rollback |
| Terraform | State/plan/apply/drift/modules | P1 | 0 → 4 | In progress | [Terraform chapter](../13-devops-iac/terraform-state-modules-and-drift.md) + drift exercise |
| Cloud | Compute/network/data/identity/regions/DR | P1 | 0 → 5 | In progress | [Cloud primitives](../14-cloud/cloud-primitives-identity-and-networking.md) + architecture |
| Kubernetes | Reconciliation/control plane/workloads | P1 | 0 → 5 | In progress | [Reconciliation](../15-kubernetes/cluster-architecture-and-reconciliation.md) + rollout diagnosis |
| Kubernetes | Networking/storage/resources/autoscaling | P1 | 0 → 5 | In progress | [Workloads](../15-kubernetes/workloads-networking-and-storage.md) + capacity lab |
| Kubernetes | RBAC/policy/upgrades/troubleshooting | P1 | 0 → 5 | In progress | [Kubernetes operations](../15-kubernetes/kubernetes-security-observability-and-operations.md) + security runbook |
| Observability | Structured logs/metrics/traces/correlation | P0 | 0 → 5 | Planned | End-to-end telemetry |
| Observability | OpenTelemetry/SLI/SLO/alerts/incidents | P0/P1 | 0 → 5 | Planned | SLO + incident investigation |
| Distributed | Timeout/retry/backoff/jitter/resilience | P0 | 0 → 5 | Planned | Dependency failure matrix |
| Distributed | Messaging/delivery/order/backpressure/DLQ | P0 | 0 → 5 | Planned | Project 04 |
| Distributed | Outbox/inbox/saga/consistency | P0 | 0 → 5 | Planned | ADR + recovery tests |
| Distributed | Replication/partition/sharding/time | P0/P2 | 0 → 5 | Planned | System design review |
| Data Engineering | Ingestion/batch/stream/CDC/lineage | P2 | 0 → 3 | Planned | Project 05 ingestion |
| AI Engineering | Model/provider/structured output/tools | P0 | 2* → 5 | Gap assessment | Existing experience; production evidence pending |
| AI Evaluation | Dataset/ground truth/judge/regression | P0 | 2* → 5 | Gap assessment | Eval suite + gate required |
| RAG | Ingestion/retrieval/rerank/citation | P0 | 2* → 5 | Gap assessment | Project 05 quality evidence |
| RAG | ACL/tenancy/deletion/versioning/cost | P0 | 1* → 5 | Gap assessment | Lifecycle/security evidence required |
| Agents | Agent/workflow/tool/state design | P0 | 2* → 5 | Gap assessment | Project 06 |
| MCP | Client/server/tools/resources/auth/trust | P0 | 1* → 5 | Gap assessment | Threat model + audited tools |
| AI Security | Injection/exfiltration/tool abuse/agency | P0 | 1* → 5 | Gap assessment | Red-team suite |
| GenAIOps | Prompt/model/index version/canary/rollback | P0/P1 | 1* → 5 | Gap assessment | AI release pipeline |
| MLOps | Dataset/experiment/registry/training/drift | P2 | 0 → 3 | Planned | Selective architecture review |
| System Design | FR/NFR/capacity/component selection | P0 | 0 → 5 | Planned | Design dossiers |
| System Design | AI latency/cost/fallback/safety/data | P0 | 1* → 5 | Gap assessment | Project 07 |
| Architecture | Boundaries/styles/DDD/data/security/deploy | P0 | 0 → 5 | Planned | Architecture review |
| Architecture | Evolution/migration/rollback/cost | P0 | 0 → 5 | Planned | Migration plan + ADR |
| Documentation | C4/sequence/data/deployment/trust | P1 | 0 → 5 | Planned | Diagram set |
| Documentation | ADR/RFC/NFR/threat/failure/runbook | P1 | 0 → 5 | Planned | Project architecture packet |

## Update rule

Level chỉ tăng khi cột Evidence trỏ đến artifact hoặc kết quả kiểm tra. “Đã đọc”, số năm kinh nghiệm hoặc hoàn thành tutorial không tự động nâng level.

## Verification metadata

- Verified: 2026-08-11
- Technology version: [technology-baseline.md](technology-baseline.md)
- Official sources: evidence sẽ được quản lý theo module/project
- Context7 queries used: /dotnet/docs cho evidence links của module Linux/networking; roadmap-wide matrix không dùng Context7 để tự nâng level
- Notes: AI current levels có dấu * là giả định bảo thủ từ kinh nghiệm người đọc đã nêu; phải thay bằng assessment evidence.
