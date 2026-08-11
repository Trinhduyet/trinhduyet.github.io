# AI-Enabled Software Architect Knowledge Roadmap

> Lộ trình thực chiến bằng tiếng Việt: từ nền tảng C#/.NET và production engineering đến thiết kế, vận hành và bảo vệ hệ thống AI ở quy mô lớn.

Repository này không phải danh sách công nghệ cần học thuộc. Đây là một hệ thống học dựa trên dependency và bằng chứng, giúp trả lời ba câu hỏi:

1. Cần học gì trước để không chỉ nhớ syntax?
2. Làm sao chứng minh mình có thể implement, operate và design?
3. Khi nào một công nghệ là lựa chọn đúng, và khi nào không nên dùng?

## Roadmap trong một hình

```mermaid
flowchart TD
    START(["Điểm bắt đầu<br/>C# cơ bản + kinh nghiệm thực tế hiện có"])

    F1["1. Production Foundations<br/>CS essentials · Linux · Git · Networking"]
    F2["2. Backend Engineering<br/>.NET · ASP.NET Core · SQL · API Design"]
    F3["3. Production Platform<br/>Testing · Security · Performance · Docker · Cloud · Kubernetes"]
    F4["4. Distributed Systems<br/>Partial failure · Messaging · Consistency · Observability"]
    F5["5. Production AI<br/>AI Engineering · RAG · Agents · MCP · AI Security · GenAIOps"]
    F6["6. Architecture<br/>System Design · Software Architecture · C4 · ADR · Migration"]

    TARGET(["Đích đến<br/>AI-enabled Software Architect"])

    START --> F1 --> F2 --> F3 --> F4 --> F5 --> F6 --> TARGET

    F1 -.->|mở khóa| F3
    F2 -.->|cung cấp hệ thống thật| F5
    F3 -.->|vận hành và quan sát| F5
    F4 -.->|failure reasoning| F6

    classDef start fill:#e0f2fe,stroke:#0369a1,color:#0c4a6e,stroke-width:2px;
    classDef foundation fill:#ecfccb,stroke:#4d7c0f,color:#365314;
    classDef backend fill:#dcfce7,stroke:#15803d,color:#14532d;
    classDef platform fill:#fef3c7,stroke:#b45309,color:#78350f;
    classDef distributed fill:#ffedd5,stroke:#c2410c,color:#7c2d12;
    classDef ai fill:#f3e8ff,stroke:#7e22ce,color:#581c87;
    classDef architecture fill:#ede9fe,stroke:#5b21b6,color:#3b0764;
    classDef target fill:#dbeafe,stroke:#1d4ed8,color:#1e3a8a,stroke-width:2px;

    class START start;
    class F1 foundation;
    class F2 backend;
    class F3 platform;
    class F4 distributed;
    class F5 ai;
    class F6 architecture;
    class TARGET target;
```

Chi tiết đầy đủ của 27 module và 7 project nằm trong [Master Roadmap](docs/00-roadmap/master-roadmap.md). Thứ tự trên là dependency path, không phải lịch học cứng.

## Học để tạo ra năng lực gì?

| Giai đoạn | Câu hỏi phải trả lời được | Bằng chứng điển hình |
| --- | --- | --- |
| Foundations | Process, memory, filesystem và request path thực sự hoạt động thế nào? | Failure lab, diagnostic timeline, Git recovery |
| Backend | Request đi từ API đến SQL ra sao và contract nào phải được giữ? | Code, tests, API/data dossier, execution plan |
| Platform | Deploy, quan sát, bảo vệ, scale và rollback hệ thống thế nào? | Image/pipeline, threat review, load report, runbook |
| Distributed | Hệ thống phục hồi thế nào khi dependency chậm, mất hoặc trả duplicate? | Failure matrix, idempotency/outbox experiment, SLO |
| Production AI | Làm sao đo quality, kiểm soát data/tool access và rollback AI artifacts? | Eval gate, RAG ACL tests, agent audit, red-team report |
| Architecture | Vì sao chọn kiến trúc này và nó tiến hóa ở 10x/100x ra sao? | NFR, capacity model, C4, ADR, migration và DR plan |

## Trạng thái repository

Trạng thái dưới đây là tiến độ tài liệu, không phải level năng lực của người học.

| Phạm vi | Trạng thái | Điểm vào |
| --- | --- | --- |
| Roadmap foundation | Hoàn thành bản đầu | [Roadmap overview](docs/00-roadmap/README.md) |
| Baseline và source policy | Đã xác minh 2026-08-11 | [Technology baseline](docs/00-roadmap/technology-baseline.md) · [Source policy](docs/00-roadmap/source-policy.md) |
| Module 01 — Computer Science Essentials | Content v1 hoàn thành 4/4; learner evidence pending | [Module overview](docs/01-computer-science/README.md) |
| Module 02 — Linux, Git, Networking | Content v1 hoàn thành 7/7; learner evidence pending | [Module overview](docs/02-linux-git-networking/README.md) |
| Module 03 — C#/.NET Runtime | Content v1 hoàn thành 5/5; RuntimeLab buildable; learner evidence pending | [Module overview](docs/03-dotnet/README.md) |
| Module 04 — Backend | Content v1 hoàn thành 4/4; BackendLab buildable; learner evidence pending | [Module overview](docs/04-backend/README.md) |
| Module 05 — SQL | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/05-sql/README.md) |
| Module 06 — API Design | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/06-api-design/README.md) |
| Module 07 — ASP.NET Core | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/07-aspnet-core/README.md) |
| Module 08 — Testing và Code Review | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/08-testing-code-review/README.md) |
| Module 09 — Security và DevSecOps | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/09-security-devsecops/README.md) |
| Module 10 — Performance | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/10-performance/README.md) |
| Module 11 — Redis và Caching | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/11-redis-caching/README.md) |
| Module 12 — Docker | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/12-docker/README.md) |
| Module 13 — DevOps và IaC | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/13-devops-iac/README.md) |
| Module 14 — Cloud | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/14-cloud/README.md) |
| Module 15 — Kubernetes | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](docs/15-kubernetes/README.md) |
| Module 16–26 | Planned theo dependency | [Module map](docs/00-roadmap/master-roadmap.md#bản-đồ-module) |
| Project spine | Planned; phát triển dần từ Project 01 đến 07 | [Project spine](docs/00-roadmap/master-roadmap.md#project-spine) |

### Nội dung có thể học ngay

1. [Complexity và workload reasoning](docs/01-computer-science/complexity-and-workload-reasoning.md)
2. [Data structures cho backend systems](docs/01-computer-science/data-structures-for-backend-systems.md)
3. [Process, thread, scheduling và concurrency](docs/01-computer-science/process-thread-scheduling-and-concurrency.md)
4. [Memory, virtual memory, GC và CPU cache](docs/01-computer-science/memory-stack-heap-virtual-memory-and-cache.md)
5. [Production troubleshooting: Process → Socket → HTTP](docs/02-linux-git-networking/production-troubleshooting-foundations.md)
6. [Linux filesystem, permissions và identities](docs/02-linux-git-networking/filesystem-permissions-and-identities.md)
7. [Linux processes, signals và resource pressure](docs/02-linux-git-networking/process-signals-and-resource-pressure.md)
8. [DNS, TCP, TLS và HTTP deep dive](docs/02-linux-git-networking/dns-tcp-tls-http-deep-dive.md)
9. [Git mental model và safe recovery](docs/02-linux-git-networking/git-mental-model-and-safe-recovery.md)
10. [Proxy, NAT, load balancer và network boundaries](docs/02-linux-git-networking/proxy-nat-load-balancer-and-network-boundaries.md)
11. [.NET incident lab end to end](docs/02-linux-git-networking/incident-lab-dotnet-service.md)
12. [C# types, generics và collections](docs/03-dotnet/csharp-types-generics-and-collections.md)
13. [Exceptions, IDisposable và ownership](docs/03-dotnet/exceptions-disposable-and-resource-ownership.md)
14. [Async/await, cancellation và Task lifecycle](docs/03-dotnet/async-await-cancellation-and-task-lifecycle.md)
15. [ThreadPool, Generic Host và diagnostics](docs/03-dotnet/threadpool-concurrency-and-diagnostics.md)
16. [GC, allocations và runtime memory](docs/03-dotnet/gc-allocations-and-runtime-memory.md)
17. [Request lifecycle và endpoint contract](docs/04-backend/request-lifecycle-and-endpoint-contract.md)
18. [Authentication, authorization và validation](docs/04-backend/authentication-authorization-and-validation.md)
19. [Pagination, idempotency, rate limiting và caching](docs/04-backend/pagination-idempotency-rate-limiting-and-caching.md)
20. [Background jobs, files và webhooks](docs/04-backend/background-jobs-files-and-webhooks.md)
21. [Relational model, schema và SQL](docs/05-sql/relational-model-schema-and-sql.md)
22. [Transactions, isolation và concurrency](docs/05-sql/transactions-isolation-and-concurrency.md)
23. [Indexes, execution plans và operations](docs/05-sql/indexes-execution-plans-and-operations.md)
24. [HTTP resource contracts và semantics](docs/06-api-design/http-resource-contracts-and-semantics.md)
25. [API evolution, errors và pagination](docs/06-api-design/api-evolution-errors-and-pagination.md)
26. [Events, gRPC và webhooks](docs/06-api-design/events-grpc-webhooks-and-contracts.md)
27. [ASP.NET Core pipeline và hosting](docs/07-aspnet-core/pipeline-hosting-and-configuration.md)
28. [ASP.NET resilience và middleware](docs/07-aspnet-core/resilience-security-and-middleware.md)
29. [ASP.NET deployment và operations](docs/07-aspnet-core/deployment-observability-and-operations.md)
30. [Testing strategy và boundaries](docs/08-testing-code-review/test-strategy-and-boundaries.md)
31. [Integration, contract và load testing](docs/08-testing-code-review/integration-contract-and-load-testing.md)
32. [Code review và failure analysis](docs/08-testing-code-review/code-review-quality-and-failure-analysis.md)
33. [Threat modeling và application security](docs/09-security-devsecops/threat-modeling-and-application-security.md)
34. [Identity, secrets và data protection](docs/09-security-devsecops/identity-secrets-and-data-protection.md)
35. [Secure supply chain và DevSecOps](docs/09-security-devsecops/secure-supply-chain-and-devsecops.md)
36. [Measurement, profiling và bottlenecks](docs/10-performance/measurement-profiling-and-bottlenecks.md)
37. [Load, capacity và scalability](docs/10-performance/load-capacity-and-scalability.md)
38. [Optimization budgets và regression control](docs/10-performance/optimization-budgets-and-regression-control.md)
39. [Redis data structures và command shape](docs/11-redis-caching/redis-data-structures-and-command-shape.md)
40. [Cache consistency và stampede](docs/11-redis-caching/cache-consistency-invalidation-and-stampede.md)
41. [Redis operations và HA](docs/11-redis-caching/redis-operations-ha-and-coordination.md)
42. [Docker images và reproducibility](docs/12-docker/images-builds-and-reproducibility.md)
43. [Container runtime, network và storage](docs/12-docker/runtime-networking-storage-and-resources.md)
44. [Docker security và Compose](docs/12-docker/docker-security-compose-and-operations.md)
45. [CI/CD artifacts và promotion](docs/13-devops-iac/ci-cd-artifacts-and-promotion.md)
46. [Terraform state, modules và drift](docs/13-devops-iac/terraform-state-modules-and-drift.md)
47. [Safe delivery và recovery](docs/13-devops-iac/safe-delivery-drift-and-recovery.md)
48. [Cloud primitives và identity](docs/14-cloud/cloud-primitives-identity-and-networking.md)
49. [Regions, availability và DR](docs/14-cloud/regions-availability-and-disaster-recovery.md)
50. [Cloud cost governance](docs/14-cloud/cloud-cost-governance-and-operations.md)
51. [Kubernetes architecture và reconciliation](docs/15-kubernetes/cluster-architecture-and-reconciliation.md)
52. [Kubernetes workloads, network và storage](docs/15-kubernetes/workloads-networking-and-storage.md)
53. [Kubernetes security và observability](docs/15-kubernetes/kubernetes-security-observability-and-operations.md)

Module 01–15 đã đủ content v1; bước hoàn thành năng lực tiếp theo là chạy labs, review artifacts và lưu evidence bằng progress template.

## Cách bắt đầu

### Nếu muốn đi từ đầu

1. Đọc README này để nắm toàn cảnh.
2. Đọc [Learning Path](docs/00-roadmap/learning-path.md) để hiểu 15 phase và các phase gate.
3. Kiểm tra prerequisite tại [Knowledge Dependency Graph](docs/00-roadmap/prerequisites.md).
4. Bắt đầu [Module 01 — Computer Science Essentials](docs/01-computer-science/README.md).
5. Nối mental model sang [Module 02 — Linux, Git và Networking](docs/02-linux-git-networking/README.md).
6. Học [Module 03 — C#/.NET Runtime](docs/03-dotnet/README.md) và chạy RuntimeLab.
7. Học [Module 04 — Backend](docs/04-backend/README.md) và chạy BackendLab.
8. Lưu kết quả bằng [Progress Template](docs/00-roadmap/progress-template.md).

### Nếu đã có kinh nghiệm backend/.NET

1. Mở [Skills Matrix](docs/00-roadmap/skills-matrix.md).
2. Chọn một topic P0 và làm lab/assessment trước khi đọc lại lý thuyết.
3. Bỏ qua nội dung chỉ khi evidence đạt exit criteria; số năm kinh nghiệm không tự nâng level.
4. Ưu tiên gap có ảnh hưởng production: cancellation, SQL plans, security, resource pressure và observability.

### Nếu đã có kinh nghiệm AI/Agents

Không bắt đầu lại bằng tutorial gọi model. Dùng một hệ thống đã làm để đánh giá:

- eval dataset và regression threshold;
- prompt/model/retrieval versioning và rollback;
- RAG ACL, deletion và data lineage;
- tool authorization, approval và audit;
- injection/exfiltration red-team cases;
- quality, latency, token và cost telemetry.

Checklist chi tiết nằm trong [AI experience policy](docs/00-roadmap/master-roadmap.md#ai-experience-policy).

## Một learning slice vận hành thế nào?

```mermaid
flowchart TD
    P["Problem cụ thể<br/>không phải tên công nghệ"]
    M["Mental model<br/>components · flow · boundaries"]
    I["Minimal implementation<br/>chạy được và tái lập được"]
    C["Production constraints<br/>security · performance · reliability · cost"]
    F["Failure experiment<br/>hypothesis → inject → observe → recover"]
    E["Evidence<br/>test · trace · plan · benchmark · runbook · ADR"]
    G{"Đạt exit criteria?"}
    N["Mở khóa dependency tiếp theo"]
    R["Bổ sung gap và thử lại"]

    P --> M --> I --> C --> F --> E --> G
    G -- "Có" --> N
    G -- "Chưa" --> R --> M

    classDef work fill:#eff6ff,stroke:#2563eb,color:#1e3a8a;
    classDef evidence fill:#ecfdf5,stroke:#059669,color:#064e3b;
    classDef gate fill:#fef3c7,stroke:#d97706,color:#78350f,stroke-width:2px;
    classDef retry fill:#fff1f2,stroke:#e11d48,color:#881337;

    class P,M,I,C,F work;
    class E,N evidence;
    class G gate;
    class R retry;
```

Nguyên tắc quan trọng: đọc xong không đồng nghĩa hoàn thành. Level chỉ tăng khi có artifact hoặc kết quả quan sát được.

## Sáu mức năng lực

| Level | Có thể làm gì? | Evidence tối thiểu |
| --- | --- | --- |
| L0 — Unknown | Chưa đánh giá | Chưa có |
| L1 — Awareness | Nhận biết use case và thuật ngữ | Giải thích phạm vi |
| L2 — Can Explain | Giải thích behavior, failure và trade-off | Review questions/diagram |
| L3 — Can Implement | Tạo implementation đúng | Code và test chạy |
| L4 — Can Operate | Deploy, quan sát, chẩn đoán và phục hồi | Failure lab, trace, runbook |
| L5 — Can Design/Review | Chọn hoặc loại phương án theo requirements | ADR, capacity, threat/failure review |

Nguồn chấm chính thức nằm trong [Skills Matrix](docs/00-roadmap/skills-matrix.md).

## Dependency quan trọng cần nhớ

```mermaid
flowchart LR
    LNX["Linux + Networking"] --> DOC["Docker"] --> K8S["Kubernetes"]
    LNX --> HTTP["HTTP + Backend"] --> ASP["ASP.NET Core"]
    CSHARP["C# + .NET"] --> ASP
    SQL["SQL + Transactions"] --> EF["EF Core"] --> PROD["Production Backend"]
    ASP --> PROD
    PROD --> DIST["Distributed Systems"]
    DIST --> AGENT["Agents + MCP"]
    EVAL["AI Engineering + Evaluation"] --> RAG["Production RAG"] --> AGENT
    AGENT --> DESIGN["AI System Design"]
    DIST --> DESIGN
    DESIGN --> ARCH["Software Architecture"]

    classDef base fill:#e0f2fe,stroke:#0284c7,color:#0c4a6e;
    classDef production fill:#dcfce7,stroke:#16a34a,color:#14532d;
    classDef ai fill:#f3e8ff,stroke:#9333ea,color:#581c87;
    classDef arch fill:#ede9fe,stroke:#6d28d9,color:#3b0764;

    class LNX,CSHARP,SQL base;
    class DOC,K8S,HTTP,ASP,EF,PROD,DIST production;
    class EVAL,RAG,AGENT ai;
    class DESIGN,ARCH arch;
```

Dependency graph đầy đủ và lý do của từng edge nằm tại [Prerequisites](docs/00-roadmap/prerequisites.md).

## Project spine

Roadmap dùng bảy project tiến hóa để tránh học rời rạc:

~~~text
01 Async File Processor
  → 02 Order Management
  → 03 Production Backend
  → 04 Distributed Notifications
  → 05 Enterprise RAG
  → 06 Production Agent Platform
  → 07 High-scale Multi-region AI System
~~~

Mỗi project giữ lại evidence của phase trước và thêm constraints mới. Đây không phải bảy toy projects độc lập.

## Cấu trúc tài liệu

~~~text
docs/
├── 00-roadmap/
│   ├── master-roadmap.md       phạm vi và đích nghề nghiệp
│   ├── learning-path.md        15 phase và phase gates
│   ├── prerequisites.md        dependency graph
│   ├── skills-matrix.md        level và evidence
│   ├── technology-baseline.md  phiên bản đã xác minh
│   ├── source-policy.md        quy tắc nghiên cứu
│   └── progress-template.md    nhật ký học và evidence
├── 01-computer-science/
│   ├── README.md
│   ├── complexity-and-workload-reasoning.md
│   ├── data-structures-for-backend-systems.md
│   ├── process-thread-scheduling-and-concurrency.md
│   ├── memory-stack-heap-virtual-memory-and-cache.md
│   └── references.md
├── 02-linux-git-networking/
│   ├── README.md
│   ├── production-troubleshooting-foundations.md
│   ├── filesystem-permissions-and-identities.md
│   ├── process-signals-and-resource-pressure.md
│   ├── dns-tcp-tls-http-deep-dive.md
│   ├── git-mental-model-and-safe-recovery.md
│   ├── proxy-nat-load-balancer-and-network-boundaries.md
│   ├── incident-lab-dotnet-service.md
│   └── references.md
├── 03-dotnet/
│   ├── README.md
│   ├── csharp-types-generics-and-collections.md
│   ├── exceptions-disposable-and-resource-ownership.md
│   ├── async-await-cancellation-and-task-lifecycle.md
│   ├── threadpool-concurrency-and-diagnostics.md
│   ├── gc-allocations-and-runtime-memory.md
│   └── references.md
└── 04-backend/
    ├── README.md
    ├── request-lifecycle-and-endpoint-contract.md
    ├── authentication-authorization-and-validation.md
    ├── pagination-idempotency-rate-limiting-and-caching.md
    ├── background-jobs-files-and-webhooks.md
    └── references.md
├── 05-sql/
├── 06-api-design/
├── 07-aspnet-core/
├── 08-testing-code-review/
├── 09-security-devsecops/
├── 10-performance/
├── 11-redis-caching/
├── 12-docker/
├── 13-devops-iac/
├── 14-cloud/
└── 15-kubernetes/
    ├── README.md + references.md
    └── 3 production reasoning chapters mỗi module

labs/
├── 01-computer-science/
│   └── workload-lab/           .NET 10 experiments: lookup, race, locality
├── 02-linux-git-networking/
│   └── incident-service/       ứng dụng .NET 10 cho failure lab
├── 03-dotnet/
│   └── runtime-lab/             .NET 10 cancellation/allocation/diagnostics
└── 04-backend/
    └── backend-lab/             .NET 10 pagination/idempotency/backpressure
~~~

Module Planned chỉ có link khi nội dung đã đủ hữu ích. [Technology Baseline](docs/00-roadmap/technology-baseline.md) phải được kiểm tra lại trước lab phụ thuộc phiên bản.

## Nguyên tắc kiến trúc của roadmap

- Mental model trước syntax.
- Official documentation và specifications quyết định behavior.
- Modular monolith trước microservices; Kubernetes chỉ xuất hiện khi có operational reason.
- Retry cần deadline, idempotency và load budget.
- AI là subsystem: authorization, reliability, privacy và observability vẫn áp dụng.
- Prompt, model, retrieval config, index và eval dataset là production artifacts có version.
- Mọi quyết định lớn phải nêu phương án đơn giản hơn, failure modes, cost và điều kiện xem xét lại.

## Tiếp tục từ đây

Điểm vào thực hành hiện tại là [WorkloadLab](labs/01-computer-science/workload-lab/Program.cs), sau đó [.NET incident lab](docs/02-linux-git-networking/incident-lab-dotnet-service.md) và [RuntimeLab](labs/03-dotnet/runtime-lab/Program.cs). Thứ tự tiếp theo:

1. Chạy lookup, race và locality experiments; lưu prediction, output và interpretation.
2. Chạy Git recovery và incident scenarios; nối resource evidence với mental model Module 01.
3. Đối chiếu Phase 01 gate trong Learning Path.
4. Chạy RuntimeLab cancellation/allocation/diagnostics; lưu output và decision note.
5. Chạy BackendLab pagination/idempotency/backpressure; lưu output và contract decision note.
6. Đi Module 05 → 06 → 07 theo dependency và lưu evidence mỗi gate.
7. Tiếp tục Module 08 → 10 để đóng testing/security/performance boundary.
8. Hoàn thành Module 11 → 15 để nối cache, container, delivery, cloud và orchestration.

## Verification metadata

- README verified: 2026-08-11.
- Scope: repository navigation, current documentation status và dependency flow.
- Technology details: [Technology Baseline](docs/00-roadmap/technology-baseline.md).
- Source rules: [Source Policy](docs/00-roadmap/source-policy.md).
- Mermaid target: GitHub/CommonMark-compatible flowchart syntax; diagrams vẫn có phần chữ/bảng tương đương để tài liệu đọc được khi renderer không hỗ trợ Mermaid.

<!-- Mermaid.js Script CDN hỗ trợ tự động render sơ đồ Mermaid trên GitHub Pages (Jekyll) -->
<script type="module">
  import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
  mermaid.initialize({ startOnLoad: true, theme: 'default' });

  document.addEventListener("DOMContentLoaded", function () {
    const elements = document.querySelectorAll("pre.language-mermaid, code.language-mermaid, .language-mermaid pre, pre code.language-mermaid");
    elements.forEach((el) => {
      const container = el.tagName.toLowerCase() === "code" ? el.parentElement : el;
      const div = document.createElement("div");
      div.className = "mermaid";
      div.textContent = el.textContent;
      if (container && container.parentNode) {
        container.parentNode.replaceChild(div, container);
      }
    });
    mermaid.run({ querySelector: '.mermaid' });
  });
</script>
