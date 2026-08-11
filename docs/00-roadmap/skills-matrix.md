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
| C# | Type system, generics, collections, LINQ | P0 | 0 → 4 | Planned | Project 01 code/review |
| .NET | Exceptions, IDisposable, resource ownership | P0 | 0 → 5 | Planned | Failure/cleanup tests |
| .NET | Task, async/await, CancellationToken | P0 | 0 → 5 | Planned | Project 01 load/cancel evidence |
| .NET | ThreadPool và concurrency | P0 | 0 → 5 | Planned | Saturation/race lab |
| .NET | GC, allocations, memory | P0 | 0 → 4 | Planned | Trace/profile report |
| .NET | Generic Host, DI, config, logging | P0 | 0 → 5 | Planned | Hosted app + lifecycle tests |
| Backend | HTTP request lifecycle | P0 | 0 → 5 | Planned | Trace từ socket đến handler |
| Backend | Authn/authz/validation | P0 | 0 → 5 | Planned | Threat tests + policy matrix |
| Backend | Pagination/filtering/sorting | P0 | 0 → 4 | Planned | Contract + SQL/load evidence |
| Backend | Idempotency/rate limiting/caching | P0 | 0 → 5 | Planned | Duplicate/overload experiments |
| Backend | Background jobs/files/webhooks | P0 | 0 → 4 | Planned | Recovery/replay tests |
| SQL | Relational model/schema/constraints | P0 | 0 → 5 | Planned | Data model review |
| SQL | Querying, joins, windows, CTEs | P0 | 0 → 4 | Planned | Query portfolio |
| SQL | Transactions/isolation/locking/deadlocks | P0 | 0 → 5 | Planned | Blocking/deadlock lab |
| SQL | Indexes/statistics/execution plans | P0 | 0 → 5 | Planned | Actual plan analysis |
| SQL Server | Query Store/plan cache/parameterization | P0/P1 | 0 → 4 | Planned | Incident-style diagnosis |
| SQL Server | tempdb/log/storage/backup overview | P1/P2 | 0 → 4 | Planned | Recovery/operations review |
| EF Core | Query translation/tracking/loading | P0 | 0 → 5 | Planned | LINQ → SQL → plan → index evidence |
| EF Core | Transactions/concurrency/migrations | P0 | 0 → 4 | Planned | Migration/rollback exercise |
| API Design | Resources, methods, status/error model | P0 | 0 → 5 | Planned | API contract review |
| API Design | Evolution/versioning/compatibility | P0 | 0 → 5 | Planned | Breaking-change exercise |
| API Design | REST/RPC/gRPC/GraphQL/events | P1 | 0 → 5 | Planned | Requirement-based decision record |
| ASP.NET Core | Hosting/pipeline/routing/binding | P0 | 0 → 5 | Planned | Request pipeline trace |
| ASP.NET Core | Security/rate limit/health checks | P0 | 0 → 5 | Planned | Security + probe failure tests |
| ASP.NET Core | HttpClientFactory/workers/caching | P0 | 0 → 4 | Planned | Dependency failure lab |
| Testing | Unit/integration/API/database/contract | P0 | 0 → 5 | Planned | Test strategy by boundary |
| Testing | Load/resilience/security/AI eval | P0 | 0 → 4 | Planned | Automated gates |
| Code Review | Correctness/design/security/perf/ops | P0 | 0 → 5 | Planned | Multi-role review report |
| Security | OAuth/OIDC/JWT/cookies/CSRF/CORS | P0 | 0 → 5 | Planned | Threat model + tests |
| Security | Injection/SSRF/secrets/TLS/least privilege | P0 | 0 → 5 | Planned | Attack/mitigation lab |
| DevSecOps | SAST/dependencies/secrets/SBOM/supply chain | P1 | 0 → 4 | Planned | CI security gates |
| Performance | Latency/throughput/tails/resources | P0 | 0 → 5 | Planned | Reproducible performance report |
| Performance | Profiling/load/capacity | P0 | 0 → 5 | Planned | Bottleneck hypothesis cycle |
| Redis | Data structures/TTL/cache patterns | P1 | 0 → 4 | Planned | Cache decision + lab |
| Redis | Persistence/replication/eviction/failure | P1 | 0 → 5 | Planned | Redis outage/stampede lab |
| Docker | Image/layers/build/cache/registry | P0 | 0 → 4 | Planned | Reproducible image |
| Docker | Runtime/network/volume/signals/resources | P0 | 0 → 5 | Planned | Failure/security lab |
| DevOps | Git strategy/CI/CD/artifact promotion | P1 | 0 → 5 | Planned | Delivery pipeline + rollback |
| Terraform | State/plan/apply/drift/modules | P1 | 0 → 4 | Planned | Reviewed plan + drift exercise |
| Cloud | Compute/network/data/identity/regions/DR | P1 | 0 → 5 | Planned | Cloud-neutral architecture |
| Kubernetes | Reconciliation/control plane/workloads | P1 | 0 → 5 | Planned | Diagnose rollout/readiness |
| Kubernetes | Networking/storage/resources/autoscaling | P1 | 0 → 5 | Planned | Capacity/failure lab |
| Kubernetes | RBAC/policy/upgrades/troubleshooting | P1 | 0 → 5 | Planned | Security/upgrade runbook |
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
