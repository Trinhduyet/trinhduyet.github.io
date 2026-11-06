# Đối chiếu Awesome Software Architecture — Coverage Map & Learning Gaps

> [← Module 25](README.md) · [Completeness Review](solution-architect-completeness-review.md) · [Mandatory Labs](solution-architect-mandatory-labs.md) · [Awesome Software Architecture](https://github.com/mehdihadeli/awesome-software-architecture)

**Mục đích:** đối chiếu những nhóm kiến thức đáng học từ catalog `mehdihadeli/awesome-software-architecture` với lộ trình **Solution Architect tổng quát** của website này. Awesome là thư viện tài nguyên gồm bài viết, video, project và tools; số lượng topic file **không đo** chất lượng hay mức thành thạo.

## 1. Các khoảng trống đã thêm giáo trình

| Nhóm từ Awesome | Chuyên đề mới tại website | Maturity được phép ghi nhận |
|---|---|---|
| Software design principles, GoF patterns, GRASP, 12-factor | [Principles & Patterns](architecture-principles-patterns-and-12-factor.md) | L1–L2 bài học + code snippet; lab chưa được xác nhận |
| Foundations modeling, Event Storming, C4, UML, Domain Storytelling | [Architecture Modeling](architecture-modeling-event-storming-and-c4.md) | L1–L2 workshop template/artifacts; cần làm case thật |
| Tactical DDD, aggregate, domain service, domain/integration events | [Tactical DDD](tactical-ddd-and-aggregate-consistency.md) | L1–L2 invariant/SQL ví dụ; cần concurrency integration tests |
| Architecture anti-patterns / refactoring / design smells | [Anti-patterns & Refactoring](architecture-antipatterns-and-refactoring.md) | L1–L2 diagnostic + rollout/checklist; cần before/after metrics |
| Actor model, Event Sourcing, service mesh, micro-frontends | [Advanced Architecture](advanced-actor-event-sourcing-and-service-mesh.md) | L1–L2 selective trade-offs; cần POC khi workload yêu cầu |
| Messaging platform variants, AsyncAPI, workflow orchestration | [Messaging Platforms & AsyncAPI](../17-distributed-systems/messaging-platforms-asyncapi-and-workflows.md) | L1–L2 contracts + failure test plans; vendor POC còn backlog |
| AI inference runtimes, model gateways, evaluation | [AI Serving & Platform](../19-ai-engineering/ai-platform-serving-gateways-and-evaluation.md) | L1–L2 design/metrics; benchmark/guardrail còn phải chạy |
| Multi-cloud / hybrid / platform portability | [Multi-cloud Decision Guide](../14-cloud/multicloud-platform-decisions-and-portability.md) | L1–L2 ADR + decision framework; chưa có multi-cloud DR evidence |

**Maturity vocabulary:** L0 = mentioned, L1 = explained, L2 = designed, L3 = verified lab, L4 = operated production. Theo [rubric](solution-architect-case-practice-and-assessment.md), **không** nâng lên L3/L4 chỉ vì có trang mới.

## 2. Những nhóm đã có kiến thức nền từ trước

| Nhóm | Bài hiện hữu |
|---|---|
| Distributed Systems, Saga, Outbox, retry, ordering | [Distributed Deep Dive](distributed-systems-architecture-deep-dive.md) · [Module 17](../17-distributed-systems/README.md) |
| DDD, Modular Monolith, Microservices | [DDD & Boundaries](ddd-modular-monolith-and-microservices.md) · [Module 18](../18-microservices-architecture/README.md) |
| Database architecture, caching and consistency | [Data Architecture](data-architecture-polyglot-persistence.md) · [Module 05 SQL](../05-sql/README.md) |
| API/identity/security | [Integration & Security](integration-security-platform-architecture.md) · [Module 09](../09-security-devsecops/README.md) |
| Cloud/Kubernetes/DevOps | [Module 14](../14-cloud/README.md) · [Module 15](../15-kubernetes/README.md) · [Module 13](../13-devops-iac/README.md) |
| System Design cases and architectural governance | [Module 24](../24-system-design/README.md) · [Architecture Leadership](architecture-leadership-decisions-and-governance.md) |

## 3. Không nhằm nhập toàn bộ catalog 1:1

Những công nghệ, thư viện hoặc vendor rất cụ thể như nhiều Kubernetes ingress controllers, service meshes, specialized NoSQL stores, .NET messaging libraries, Python AI frameworks và từng design pattern riêng lẻ không nhất thiết cần **một trang độc lập** nếu chưa có workload hoặc nhu cầu học cụ thể.

**Quy tắc biên soạn:** học concepts + usage constraints + trade-offs + failure/recovery + evidence; liên kết nguồn gốc từ docs chính thức. Tránh copy cả danh sách liên kết hoặc nội dung có bản quyền. Bổ sung vendor-specific POC chỉ khi nó giải quyết gap thực sự trong capstone/production requirement.

## 4. Thứ tự học theo năng lực Solution Architect

1. **P0:** Principles/Patterns → Architecture Modeling → Tactical DDD → Business/FR/NFR/HLD/LLD.
2. **P0/P1:** Distributed consistency → API/Security → Data/Concurrency → Performance/Observability/HA/DR.
3. **P1:** Anti-patterns/Refactoring → Messaging/AsyncAPI → Architecture Governance/ADR/Reviews.
4. **P2 (chọn theo workload):** Actor/Event Sourcing/Service Mesh → Multi-cloud portability → AI Platform serving.
5. **Capstone:** chọn một case cross-domain và hoàn thiện [template](system-design-case-study-template.md) với ít nhất 2 ADR, 2 failure sequences, source/test, metrics, security và DR runbook.

## 5. Evidence checklist cho các bài mới

- [ ] Có requirements, assumptions và measurable NFR.
- [ ] Có C4/context + API/data contracts + transaction ownership.
- [ ] Có tối thiểu 2 alternatives và một ADR với revisit trigger.
- [ ] Có negative paths, timeout, duplicate, stale data và recovery.
- [ ] Có source runnable + unit/integration/contract tests tại boundary quan trọng.
- [ ] Có logs/metrics/traces, load/fault/restore evidence ở quy mô được ghi rõ.
- [ ] Phân biệt simulated, reasoned, verified in lab và real production evidence.

**Nguồn catalog:** [Awesome — README & hierarchy](https://github.com/mehdihadeli/awesome-software-architecture/blob/main/README.md). Review này là một learning coverage map, **không phải chứng nhận** kiến trúc production hay bản sao đầy đủ của upstream.
