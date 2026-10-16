# Senior .NET & Technical Lead .NET — 96 câu hỏi ưu tiên theo JD

<div class="lesson-meta"><span>Tiếng Việt là chính</span><span>96 câu · 12 nhóm</span><span>Senior Backend / Technical Lead / Architecture</span></div>

> Đây là **bản đồ câu hỏi và gợi ý hướng trả lời**, không phải 96 đáp án chi tiết đã kiểm thử. Các trang [36 câu có đáp án](index.md), [Tech Lead scenarios](techlead-scenarios.md) và [System Design exercises](../09-solution-architecture-leadership/design-exercises.md) là nơi học sâu. Tùy JD, một câu có thể được hỏi ở nhiều vai trò. Thực tế mỗi công ty chọn câu khác nhau.

## Cách sử dụng

- **P0:** Đọc và tự nói 90–120 giây; đưa ví dụ thật và cách kiểm chứng.
- **P1:** Đi sâu theo công nghệ trong JD, không cần học thuộc mọi API.
- **Tình huống Lead:** Trả lời như người chịu trách nhiệm thiết kế/delivery, không chỉ liệt kê pattern.
- Mỗi câu: **bối cảnh → nguyên lý → giải pháp → trade-off/failure → test/evidence**.

## A. C# / .NET runtime — 8 câu (P0)

| ID | Câu hỏi thực tế | Đáp án trọng tâm / follow-up |
| --- | --- | --- |
| CS-01 | Value type vs reference type? | Copy semantics, boxing, mutability; value type không đồng nghĩa luôn nằm stack. |
| CS-02 | async/await hoạt động thế nào? | Compiler state machine, I/O không cần giữ thread; phân biệt CPU-bound. |
| CS-03 | Task, ValueTask, Thread khác gì? | Async outcome vs thread; ValueTask chỉ khi đo performance chứng minh cần. |
| CS-04 | Tại sao dùng .Result/.Wait có thể làm API chậm? | Blocking/thread pool starvation; đo queue/thread growth. |
| CS-05 | IEnumerable vs IQueryable vs IAsyncEnumerable? | In-memory iteration, query translation, streaming; deferred execution. |
| CS-06 | GC Generations, LOH, allocation pressure? | GC/allocations, memory retention; dùng profiling thay dự đoán. |
| CS-07 | IDisposable/IAsyncDisposable, HttpClient lifecycle? | Resource ownership, disposal, handler pooling via IHttpClientFactory. |
| CS-08 | Lock, SemaphoreSlim, Interlocked, ConcurrentDictionary? | Chọn primitive theo shared state/thread safety; không thay DB transaction. |

## B. ASP.NET Core / REST / GraphQL — 8 câu (P0)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| API-01 | Middleware pipeline chạy thứ tự nào? | Authentication trước authorization, exception handling, short circuit. |
| API-02 | Singleton/Scoped/Transient; BackgroundService dùng DbContext? | Scope ownership; scope factory mỗi job/iteration, DbContext không thread-safe. |
| API-03 | Controller vs Minimal API; khi nào dùng gì? | Routing/filter/validation/team convention; không có “luôn nhanh hơn”. |
| API-04 | JWT 401 vs 403? | Token validation khác resource-level authorization. |
| API-05 | GET/POST/PUT/PATCH; idempotency API payment? | HTTP semantics, stable operation key, persisted outcome, payload hash. |
| API-06 | Pagination OFFSET vs keyset? | Stable order, unique cursor, query/index, deep page trade-off. |
| API-07 | GraphQL N+1 và query cost? | DataLoader per operation + projection/paging/cost limits. |
| API-08 | API 5xx/p95 tăng sau deploy? | Correlation/trace, dependency, recent changes, rollback gate. |

## C. SQL Server / PostgreSQL / EF Core — 8 câu (P0)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| DB-01 | Index Seek luôn tốt hơn Scan? | Không; đo IO/CPU/rows, selectivity, lookups. |
| DB-02 | Composite index chọn thứ tự cột thế nào? | Predicate equality/range/sort, access patterns, write overhead. |
| DB-03 | Query Plan: estimated vs actual rows? | Cardinality, statistics, parameter sensitivity, spills. |
| DB-04 | SARGability và implicit conversion? | Tránh bọc hàm trên indexed column khi seek/range cần thiết. |
| DB-05 | SQL transaction isolation và deadlock? | Read committed/RCSI/snapshot, contention, retry entire transaction khi phù hợp. |
| DB-06 | EF Core N+1, AsNoTracking, projections, split query? | Đo SQL generated; tránh cartesian explosion và consistency risk. |
| DB-07 | 100M+ rows lọc theo instrument/time range? | Bounded filters, index, retention/aggregate khi justified; không đồng nhất rows với TPS. |
| DB-08 | Hai request trừ cùng số dư, chống double spend? | Conditional update + affected rows + transaction + unique operation ID. |

## D. Distributed / Messaging / Cache — 8 câu (P0)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| MSG-01 | Redis cache-aside và cache stampede? | TTL, bounded concurrency, invalidation, stale/freshness policy. |
| MSG-02 | Cache key multi-tenant cần gì? | Tenant + instrument/entitlement/version; authorize trước khi trả cache. |
| MSG-03 | Event Hubs khác Service Bus/RabbitMQ? | Stream/partition/retention/consumer groups vs queue/broker/settlement. |
| MSG-04 | ACK/checkpoint có đảm bảo exactly-once? | Không; crash between DB commit and ACK → redelivery. |
| MSG-05 | Retry/backoff/circuit breaker dùng khi nào? | Transient failures, idempotency, bounded budget, retry storms. |
| MSG-06 | Transactional outbox/inbox xử lý vấn đề gì? | DB commit + publication intent, duplicate delivery vẫn cần dedupe. |
| MSG-07 | Message ordering vs partitioning? | Order thường chỉ trong partition/session/key; scaling/trade-off. |
| MSG-08 | BackgroundService restart/replay/backpressure? | Checkpoints, bounded channels, durable state, DLQ, observability. |

## E. Security / Authentication / Privacy — 8 câu (P0)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| SEC-01 | JWT validation cần kiểm tra gì? | Signature, issuer/audience/lifetime, token type/claims, resource policy. |
| SEC-02 | OAuth2 vs OpenID Connect; Entra ID vs Keycloak? | AuthZ protocol vs identity layer; trust boundary. |
| SEC-03 | Tenant isolation chống IDOR? | Server-side authorization by resource/company, negative tests. |
| SEC-04 | CORS có thay auth/CSRF protection? | Không; browser-origin policy khác authentication/authorization. |
| SEC-05 | Secret, certificate, Managed Identity/Key Vault? | Least privilege, rotation, nonlogged credentials. |
| SEC-06 | Webhook bị giả mạo/replay? | Signature, timestamp, idempotent handler, business verification. |
| SEC-07 | SQL injection và unsafe EF raw SQL? | Parameterization, no interpolated untrusted SQL identifiers. |
| SEC-08 | OWASP, PII retention và audit log? | Threat modeling, least privilege, redaction, legal requirements. |

## F. Azure / Cloud — 8 câu (P0/P1)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| AZ-01 | App Service vs Container Apps vs AKS vs Functions? | Workload, scaling, team ops, release safety và TCO. |
| AZ-02 | Azure Service Bus queue/topic/DLQ? | Consumer model, settlement, delivery state, locks, poison messages. |
| AZ-03 | Event Hubs consumer group, partition, checkpoint? | Per-partition order, independent consumers, retention/replay. |
| AZ-04 | Blob Storage, SAS, Managed Identity? | Authorization scopes, expiry, container access, audit. |
| AZ-05 | Entra ID và RBAC data vs control plane? | Identity, access scope, role assignment, least privilege. |
| AZ-06 | App Insights/OTel/Prometheus/Grafana? | Metrics/logs/traces, correlation, resource cost. |
| AZ-07 | HA vs DR, RTO/RPO, region failure? | Failover strategy, restore drills, acceptable data loss. |
| AZ-08 | Azure cloud cost tối ưu từ đâu? | Compute-hours, egress, DB IO, logging, quota, operations overhead. |

## G. DevOps / Docker / Kubernetes / IaC — 8 câu (P0/P1)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| DEV-01 | Docker image vs container; multi-stage build? | Immutable image, lean runtime, secure supply chain. |
| DEV-02 | GitLab CI/Azure DevOps pipeline từ commit đến release? | Build once, tests/security gates, artifact promotion, approvals. |
| DEV-03 | Deploy CLI success nhưng app NotReady? | Readiness status + smoke test; fail pipeline và rollback. |
| DEV-04 | Kubernetes Deployment → ReplicaSet → Pod → Container? | Reconciliation and desired state. |
| DEV-05 | Pod Running nhưng Service 503? | Selectors/EndpointSlices/readiness/ports/ingress. |
| DEV-06 | HPA vs node autoscaler, requests/limits? | Scaling bottleneck, scheduling, quotas. |
| DEV-07 | Terraform state/plan/drift/replacement? | State protection, reviewed plan, avoid destructive surprise. |
| DEV-08 | Blue-green/canary/DB expand-contract? | Backward compatibility và rollback toàn bộ critical path. |

## H. Testing / Quality / Observability — 8 câu (P0)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| QA-01 | Unit vs integration vs contract vs E2E? | Test boundary và dependency ownership; test pyramid phù hợp. |
| QA-02 | Mock external payment API thế nào? | Contract/error/timeout/retry, provider sandbox khi có. |
| QA-03 | Load test phân biệt p50/p95/p99, RPS, concurrency? | Workload profile, saturation, bottleneck, baseline. |
| QA-04 | Code review ưu tiên gì? | Correctness, auth, data races, tests, rollout/telemetry trước style. |
| QA-05 | EF migration cho schema lớn an toàn thế nào? | Expand-contract, index creation risk, canary, rollback plan. |
| QA-06 | Log vs metric vs trace? | Event detail, trend/alert, end-to-end latency path. |
| QA-07 | Cách review PR do AI coding tạo ra? | Ownership, tests, secret scan, dependencies và diff scrutiny. |
| QA-08 | Incident postmortem có gì? | Timeline, customer impact, root cause evidence, preventive action. |

## I. Architecture / System Design — 8 câu (P0)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| DES-01 | Modular monolith vs microservices? | Business boundaries, autonomy, deployment/ops/coupling costs. |
| DES-02 | Clean/Hexagonal/Vertical Slice khác nhau ra sao? | Dependency direction, feature ownership, testability. |
| DES-03 | DDD bounded context và aggregate boundaries? | Invariants/transactions, naming và ownership. |
| DES-04 | CQRS vs event sourcing có luôn đi cùng? | Không; independent patterns, read model projection lag. |
| DES-05 | Thiết kế 10k notifications trong 60s? | Throughput estimate, queue, consent, provider quotas, DLQ. |
| DES-06 | Thiết kế rate limiter đa instance? | Scope, algorithms, fairness, Redis consistency/fallback. |
| DES-07 | Thiết kế high-read financial data API? | Hot keys, bounded time-range, index/read model, freshness. |
| DES-08 | Migrating legacy ASP.NET Framework? | Strangler routes, contracts, compatibility, phased migration. |

## J. Tech Lead — Delivery & People Collaboration — 8 câu (P0 cho Lead)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| LEAD-01 | BA đưa requirement mơ hồ, xử lý? | Clarify acceptance criteria, NFR, dependency và scope before estimate. |
| LEAD-02 | Estimate feature/team sprint thế nào? | Decompose, dependency, risk buffer, communicate uncertainty. |
| LEAD-03 | Junior viết code khó maintain, phản hồi ra sao? | Risk-focused PR feedback, example/tests, coaching, ownership. |
| LEAD-04 | Hai senior bất đồng kiến trúc? | Align constraints, compare evidence, ADR, accountable decision. |
| LEAD-05 | Deadline gấp nhưng security/tests chưa đạt? | Negotiate scope/feature flag, protect correctness; explicit escalation. |
| LEAD-06 | Một thành viên chậm liên tục? | Understand blocker privately, pairing/expectations, feedback; không vội kết luận. |
| LEAD-07 | Technical debt vs new features? | Risk/ROI, capacity and roadmap, measurable improvement. |
| LEAD-08 | Incident P1 xảy ra trong sprint? | Incident roles, mitigation, communication, controlled recovery, retrospective. |

## K. Tech Lead — Technical Strategy & Cross-team — 8 câu (P0 cho Lead/Architect)

| ID | Câu hỏi | Đáp án trọng tâm |
| --- | --- | --- |
| STR-01 | Ownership của nhiều teams chia thế nào? | Service/API contracts, data ownership, escalation và runbooks. |
| STR-02 | Thiết kế API để hai frontend teams triển khai song song? | Contract, versioning, generated clients/contract tests. |
| STR-03 | Quyết định khi nào chọn Azure Service Bus thay RabbitMQ? | Requirements, SLA, operability, cost, expertise. |
| STR-04 | System chậm, nên thêm Redis hay index SQL? | Trace/plan, freshness, failure/cost comparison. |
| STR-05 | Muốn chuyển monolith sang microservices? | Real scaling/ownership pain, strangler plan, operational maturity. |
| STR-06 | API 99.9% SLA, phương án backup/HA? | SLI/SLO, RTO/RPO, capacity and DR test with budget. |
| STR-07 | Đặt coding standards và quality gates? | CI tests, reviews, analyzer, security, SLO, improvement feedback. |
| STR-08 | Bạn đã thực sự lead hay chỉ mentor/review? | Trình bày ownership thật; không biến team size thành reports. |

## L. HR / CV / Communication — 8 câu (P0)

| ID | Câu hỏi | Gợi ý |
| --- | --- | --- |
| HR-01 | Giới thiệu bản thân trong 60–90 giây? | Hiện tại → dự án trước → kỹ năng → mục tiêu liên quan JD. |
| HR-02 | Kể quá trình đi làm theo CV? | 3–4 giai đoạn, dành nhiều thời gian cho vị trí gần nhất. |
| HR-03 | Dự án khó nhất bạn trực tiếp làm? | Problem → responsibility → action → observed result. |
| HR-04 | Vì sao muốn đổi công ty? | Động cơ thật, không bịa tiêu cực/đóng cửa dự án. |
| HR-05 | Điểm mạnh/yếu? | Có ví dụ thật và kế hoạch cải thiện. |
| HR-06 | Kể một lỗi/mâu thuẫn/kết quả không đạt? | STAR(R), nhận trách nhiệm, hậu kiểm. |
| HR-07 | Vì sao phù hợp Senior vs Tech Lead? | Khác biệt hands-on, delivery, mentoring, design ownership. |
| HR-08 | Hỏi gì lại interviewer? | Chất lượng production, team ownership, project challenge, 90-day success. |

## Lộ trình đề xuất (14 ngày, tùy chỉnh)

| Ngày | Học |
| --- | --- |
| 1–2 | C#/.NET & API — CS/API P0 |
| 3–4 | SQL/EF Core & transaction |
| 5–6 | Messaging/Redis, Security |
| 7–8 | Azure & DevOps/Production |
| 9–10 | Architecture, System Design |
| 11–12 | Tech Lead: estimate, mentoring, ownership, incident |
| 13 | Mock technical + system design (60–90 phút) |
| 14 | CV/HR, luyện 3 câu chuyện thật, review gaps |

## Coverage so với JD mẫu (tham khảo, không phải tất cả nhà tuyển dụng)

- **Senior .NET backend** cần CS/API/DB/MSG/SEC/QA/AZ/DEV và một production case.
- **Tech Lead .NET** cần toàn bộ nền Senior + LEAD/STR, quality gates, delivery và một design review thật.
- **Solution Architect .NET** mở rộng DES/AZ/STR đến NFR, capacity, cost, security, migration và business stakeholders.

Nguồn đối chiếu: [Savvycom Senior/Tech Lead](https://careers.savvycom.vn/vi/jobs/7-senior-tech-lead-dotnet?apply=1) · [VTI Senior .NET](https://tuyendung.vti.com.vn/en-US/Career/JobDetail/2791) · [Microsoft ASP.NET Core performance](https://learn.microsoft.com/en-us/aspnet/core/performance/performance-best-practices) · [Azure Well-Architected](https://learn.microsoft.com/en-us/azure/well-architected/).

**Tiếp theo:** [36 câu có đáp án](index.md) · [10 Tech Lead situations](techlead-scenarios.md) · [Mock Interview](mock-interview.md).
