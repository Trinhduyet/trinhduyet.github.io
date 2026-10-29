# Technical Leadership, Architecture Decisions, Review & Governance

> [← Module 25](README.md) · [Delivery workflow](solution-architecture-delivery-governance.md) · [Architecture Review Playbook](architecture-review-playbook.md)

Solution Architect không chỉ trả lời “dùng công nghệ gì?”. Vai trò chính là **tổ chức quá trình ra quyết định dưới ràng buộc**, làm rõ ownership, đưa ra phương án có thể thực hiện với nguồn lực hiện tại và giữ conformance qua các lần thay đổi.

## 1. Architecture decision như một hypothesis

Mỗi quyết định quan trọng cần ghi:

1. **Context & business driver:** FR/NFR ID, stakeholders, regulatory/operational constraint.
2. **Options:** ít nhất một phương án đơn giản và một phương án thay thế đáng tin.
3. **Evaluation:** correctness, latency, scalability, security, reliability, TCO, skill, delivery timeline.
4. **Decision:** owner, date, rationale, accepted negative consequences.
5. **Evidence:** POC, production baseline, vendor limit hoặc benchmark có điều kiện.
6. **Revisit triggers:** measurable SLO breach, 3× traffic, team boundary, contract change.

### ADR example: split service hay modular monolith?

| Factor | Modular monolith | Microservices |
|---|---|---|
| Transaction consistency | Dễ local ACID trong shared database owner | Cross-service workflow/consistency phức tạp |
| Delivery/ownership | Một deployment, dễ debug | Team/service autonomy khi ownership thật sự độc lập |
| Scaling | Scale cả app hoặc optimize module hot path | Scale độc lập nhưng tăng networking/operations |
| Failure isolation | Có thể ảnh hưởng process chung | Có cơ hội cô lập, nhưng vẫn dependency cascading |
| Cost | Platform/observability đơn giản hơn | Mỗi service cần CI/CD, tracing, secrets, on-call |

**Decision mẫu:** chọn modular monolith cho team 6 người, peak 200 RPS, một deployment cadence; ranh giới modules bảo vệ bằng tests. **Revisit:** payment workflow có ownership/compliance độc lập hoặc checkout p99 vi phạm do coupling đã đo. Các số chỉ là giả định của ví dụ.

## 2. POC: đo rủi ro, không chỉ demo happy path

Một POC tốt bắt đầu bằng **hypothesis có thể bị bác bỏ**:

```text
Hypothesis: PostgreSQL có thể đáp ứng 1k writes/s với p99 < 100ms
trên schema X, 50 concurrent writers, 5M rows, cost ceiling Y.

Test: normal + burst + hot-key contention + failover.
Evidence: exact version/config, dataset, workload generator,
query plans, traces, resource usage, retry/errors, raw results.

Decision: pass/conditional/fail; known risks and next verification gate.
```

Không dùng POC để hợp thức hóa lựa chọn đã quyết định từ trước. Chạy cùng workload và constraints cho các phương án; thêm operational burden, vendor support và license trong TCO.

## 3. Architecture review như một hoạt động cộng tác

| Stakeholder | Điều họ phải xác nhận |
|---|---|
| Product/Business | FR, priority, customer impact, accepted degraded flows, budget |
| Tech Lead/Developers | Module boundaries, implementation feasibility, LLD, testing, migration |
| Security | Threat model, identities, authz, data classification, exceptions |
| DBA/Data | Schema, isolation, indexes, retention, backup, migration |
| DevOps/SRE | CI/CD, infra, SLO, observability, DR, runbook, capacity |
| EDA/Integration | Contract ownership, async semantics, schema version, provider SLA |
| Risk/Compliance (when relevant) | Financial correctness, audit, regulatory constraints, residual risk |

Tạo **review packet** trước họp: context, option decision, diagrams, threat/failure table, unresolved questions. Kết thúc bằng action/owner/date, không phải “mọi người thống nhất” nhưng không có record.

## 4. Architecture deviation và technical debt

Thay đổi implementation không khớp HLD/ADR có thể hợp lý nếu yêu cầu mới, nhưng phải được review. Workflow: register deviation → impact/risk classification → review owners → approve/temporary exception/reject/POC → expiry/mitigation → evidence closure. Quy trình và RACI chi tiết nằm trong [Delivery Governance](solution-architecture-delivery-governance.md).

**Debt register** cần thêm principal (phần cần sửa), interest (chi phí lặp lại/incident/release), owner, trigger, severity và kế hoạch remediation. Không xem mọi workaround là sai; nhưng không biến exception 3 tháng thành kiến trúc vĩnh viễn.

### Fitness function examples

- `Orders.Domain` không phụ thuộc trực tiếp vào `Payments.Infrastructure`.
- Public OpenAPI không vô tình phá backward compatibility.
- Service không được lưu secret trong source/config plaintext.
- Data migration không xóa cột khi consumer cũ còn hoạt động.
- Critical transaction path phải có concurrency/failure integration test trước merge.

Một rule “architect đã nói” không bền vững bằng automated evidence + owner rõ ràng.

## 5. Engineering practices và mentoring

Mentoring không phải “review code cho junior”. Nó bao gồm:

1. **Explain the why:** workshop về transaction boundaries, retry, consistency, authentication; minh họa bằng bug production cụ thể.
2. **Design reviews:** để developer tự trình bày alternatives/risks/evidence, phản biện dựa NFR thay vì preferences.
3. **Shared standards:** golden path, coding conventions, ADR templates, quality gates, testing pyramid và runbook.
4. **Pairing:** debug trace/query plan/cancellation/deadlock hoặc threat-model cùng team.
5. **Feedback loop:** blameless postmortem, action owner, kiến thức cập nhật vào reference docs và CI.
6. **Measure impact:** defect escape rate, deployment failure/recovery, lead time, SLO, review cycle, service ownership maturity.

**Evidence mentoring:** agenda/workshop, recorded design rationale, reviewed ADR, representative PR discussion, incident improvement outcome; không chỉ ghi “đã hướng dẫn team”.

## 6. Quản trị roadmaps và ưu tiên

| Priority | Ví dụ | Decision policy |
|---|---|---|
| P0 | Money/data corruption, cross-tenant access, unrecoverable loss | Fix before launch hoặc explicit risk owner approval theo policy |
| P1 | SLO instability, manual recovery, unsupported vendor | Schedule roadmap, mitigation + exit criteria |
| P2 | Premature extraction, minor framework inconsistency | Tối ưu khi evidence cho thấy giá trị |

Phân biệt **architecture risk** (sự kiện có thể xảy ra) với **architecture debt** (chi phí do lựa chọn/shortcut đã biết). Khi prioritizing, xét probability × impact, blast radius, detection latency, reversibility, team cost và business deadline.

## 7. Interview/assessment: technical decision defense

Cho đề bài một SaaS 100 tenants, mỗi tenant có mức tải khác nhau, cần hybrid integration với ERP.

- Trình bày capability map, FR/NFR và hai HLD.
- Bảo vệ một ADR trước Product, Security, DBA và DevOps.
- Giải thích why/why not microservices, SQL vs document, queue vs sync, cache và tenant isolation.
- Lập POC để bác bỏ một assumption quan trọng.
- Review một thay đổi vi phạm schema ownership; tạo deviation record và risk mitigation.
- Viết code-review checklist, game-day plan và mentoring session cho team.

**Pass:** quyết định liên kết business + NFR + evidence + owner + revisit trigger, chấp nhận uncertainty minh bạch. **Fail:** chọn stack theo trào lưu, không nêu downside, không có validation plan hoặc quá phụ thuộc cá nhân architect.
