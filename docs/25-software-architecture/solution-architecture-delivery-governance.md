# Solution Architecture Delivery & Governance — từ BRD đến vận hành

> [← Module 25](README.md) · [Ma trận JD](solution-architect-jd-coverage.md) · [Architecture Review](architecture-review-playbook.md) · [System Design Template](system-design-case-study-template.md)

Solution Architect không chỉ thiết kế sơ đồ. Vai trò phải chuyển yêu cầu nghiệp vụ thành quyết định có thể phê duyệt, bàn giao, triển khai, quan sát và kiểm tra sự tuân thủ trong vòng đời thay đổi.

## 1. Delivery workflow và decision gates

| Gate | Input | Hoạt động của Architect | Output & exit criteria |
|---|---|---|---|
| G0 — Discovery | Business problem, stakeholders, timeline | Clarify capability, critical flow, regulatory constraints, assumptions | Scope, stakeholder map, open questions, risks được phân công |
| G1 — Requirements | BRD/user journeys, integrations | FR/NFR workshop, priority, measurable SLO, data classification | FR/NFR IDs, acceptance criteria, NFR owner sign-off |
| G2 — Options | Constraints, baseline landscape | Evaluate build/buy, modular monolith/microservices, sync/async, DB/vendor | Options scorecard, POC hypothesis, ADR draft |
| G3 — HLD review | Capability map, NFR, options | Context/container, trust boundary, data ownership, failure flows, topology | HLD reviewed by Product, Security, DevOps, DBA, EDA/Integration, Tech Lead as relevant |
| G4 — LLD review | Approved HLD, API/data contracts | Detailed sequence, transaction boundaries, indexes, authz, recovery | LLD, schema/API/event contract, test strategy, signed findings |
| G5 — Build/POC | Accepted ADR and risk items | Verify architectural fitness, run POC for disputed claims | Measured evidence, updated ADR and risk register |
| G6 — Release readiness | CI, deploy plan, tests, telemetry | Validate deviations, rollback, runbooks, SLO, DR, support | Go/no-go with owners, unresolved risks explicitly accepted |
| G7 — Post-release | Telemetry, incidents, change requests | Compare NFR actual vs target, manage debt, reconsider ADR | Review report, follow-ups, migration roadmap |

Gate là quy trình gợi ý, có thể gộp theo quy mô dự án; không biến approval thành nút thắt mà không rõ trách nhiệm.

## 2. Responsibility matrix (mẫu, xác nhận với tổ chức)

**R** thực hiện, **A** chịu trách nhiệm quyết định cuối, **C** tham vấn, **I** thông báo. RACI sau là đề xuất, không mặc định là mô hình quản trị của công ty.

| Deliverable | Product | Solution Architect | Tech Lead | Security | DBA | DevOps | EDA/Integration |
|---|---|---|---|---|---|---|---|
| Scope/FR priority | A/R | C | C | I | I | I | C |
| NFR & risk trade-offs | A (business priority) | R | C | C | C | C | C |
| HLD/options/ADR | C | A/R | C | C | C | C | C |
| LLD/application code | I | C | A/R | C | C | C | C |
| Data schema/retention | C | R | R | C | A/C (DB policy) | I | I |
| Threat & security exceptions | I | R | C | A | C | C | C |
| Release/runbook/DR | C | R | R | C | C | A/R (platform) | C |
| Partner/event contracts | C | R | R | C | C | C | A/C (enterprise standard) |

Nếu một hàng có nhiều chủ thể muốn làm **A**, ghi rõ decision rights theo policy hiện hành. RACI không thay thế chữ ký chấp thuận của chủ hệ thống/rủi ro khi chính sách yêu cầu.

## 3. Requirement traceability

Tạo ledger theo format:

| FR/NFR ID | Business outcome | Architecture component | Contract/data | Test/evidence | Owner |
|---|---|---|---|---|---|
| FR-ORDER-01 | Đặt lệnh duy nhất | OMS API + Risk | `POST /orders`, Order/Reservation | duplicate + concurrent order integration test | OMS Lead |
| NFR-PERF-01 | Xử lý burst lệnh | API + queue + DB | p95/p99, queue age | load test với workload xác định | SA + SRE |
| NFR-SEC-01 | Chống truy cập chéo tài khoản | API + resource policy + data access | account entitlement | negative authorization test | Security + Tech Lead |
| NFR-DR-01 | Phục hồi sau mất DB | Backup + journal + gateway | RTO/RPO target | restore drill + reconciliation report | DBA + Operations |

Mỗi NFR cần **target, measurement window, owner, method, result**; nếu chưa đo, đánh dấu **unverified**.

## 4. Hồ sơ HLD và LLD tối thiểu

**HLD package:**
- Problem/scope, assumptions, actors, capability map.
- FR/NFR và priority; context C4, container/deployment view; on-prem/cloud/hybrid connectivity.
- Data ownership, integration matrix (protocol, contract owner, timeout, idempotency, retry, SLA).
- Security trust boundaries/threat model, SLO, availability, DR, capacity/cost model.
- Alternative options, ADR, risk register, migration/release strategy, reviewers and decisions.

**LLD package:**
- Component/sequence diagrams cho happy + failure flows.
- API OpenAPI/schema + example errors; event schema/versioning and contract tests.
- ERD/table/index/constraint, isolation/locking, idempotency and transaction boundary.
- State machine, outbox/inbox, DLQ/replay/reconciliation, distributed failure analysis.
- Authz at resource level, secrets, audit, input validation.
- Load/security/concurrency test cases, monitoring, runbook, deployment/rollback and migrations.

**Rule:** Mọi diagram phải nêu version/date/owner và boundary; diagram không thay cho contract hoặc test.

## 5. POC và lựa chọn công nghệ

POC chỉ nên chạy khi quyết định có uncertainty cao hoặc expensive-to-reverse.

| Tiêu chí | Trọng số ví dụ | Phương án A | Phương án B |
|---|---|---|---|
| Functional fit & correctness | 30% | Evidence/score | Evidence/score |
| Performance/capacity | 20% | Benchmark cùng workload | Benchmark cùng workload |
| Security/compliance | 15% | Threat/controls | Threat/controls |
| Reliability/operations/DR | 15% | Failure drill | Failure drill |
| Team skills & time-to-market | 10% | Learning/deployment cost | Learning/deployment cost |
| TCO & vendor lock-in | 10% | 3-year assumptions | 3-year assumptions |

Trọng số **chỉ là minh họa**; workshop phải phê duyệt trước khi đánh giá. POC ghi hypothesis, workload, data shape, environment, success threshold, failure observations và decision. Không suy ra độ tin cậy production từ benchmark nhỏ.

## 6. Architecture review và deviation control

**Thay đổi đáng review:** new external integration, new data owner, sensitive data, new event/transaction boundary, RTO/RPO alteration, migration, vendor/platform change hoặc deviation khỏi ADR đã duyệt.

Workflow:

1. Team đề xuất deviation với **baseline decision**, lý do, deadline, impact, options.
2. SA triage severity: correctness/security/compliance là P0; availability/data loss P0/P1 theo impact; maintainability/cost theo thực tế.
3. Chủ sở hữu liên quan review: Security cho trust boundary; DBA cho schema/backup; DevOps cho deployment/DR; EDA cho integration; Product cho scope/NFR trade-off.
4. Quyết định: **accept** (ADR update), **temporary exception** (expiry + mitigations + owner), **reject**, hoặc **POC required**.
5. Viết action items; kiểm tra conformance ở PR, CI/fitness functions và release gate.
6. Sau release đóng deviation bằng evidence hoặc gia hạn có chấp thuận; tuyệt đối không để exception “tạm” vô thời hạn.

### Deviation record template

```yaml
id: DEV-001
baseline_adr: ADR-007
change: "Tạm sử dụng shared DB read giữa hai module"
reason: "Deadline tích hợp"
risk: "Schema coupling, deploy coordination"
impact: "Availability/maintainability"
controls: ["read-only access", "contract test", "schema ownership"]
owner: "Tech Lead"
reviewers: ["Solution Architect", "DBA", "Security if relevant"]
expiry: "YYYY-MM-DD"
remediation: "Replace with owned read API"
decision: "pending"
evidence_link: "TBD"
```

## 7. Technical risk và architecture debt

| Field | Ý nghĩa |
|---|---|
| Risk ID / scenario | Event → consequence; ví dụ provider timeout tạo trạng thái lệnh unknown |
| Likelihood / impact | Chấm theo rubric của tổ chức; tách correctness, money, security, downtime |
| Detection / controls | Metrics, threshold, circuit breaker, reconciliation, manual approval |
| Residual risk | Sau mitigation còn gì và business owner chấp thuận chưa |
| Owner / due date | Người theo dõi và hạn xử lý |
| Debt source | Decision tạm thời, shortcut, version cũ, shared ownership |
| Trigger | SLO breach, peak load, security requirement hoặc release conflict |
| Closure evidence | Test/report/ADR + review sign-off |

Không biến risk register thành danh sách không ai sở hữu. Review định kỳ theo mức độ nghiêm trọng.

## 8. Engineering leadership và conformance

- **Golden path:** template API/worker có auth, logging, tracing, health checks, config/secret handling, CI/security scan, test harness.
- **Review rubric:** contracts, transaction boundary, cancellation/retry safety, schema migration, authz, telemetry và operations.
- **Fitness functions:** automated check package dependencies, OpenAPI backward compatibility, migration safety, test coverage on critical invariants, secret scanning.
- **Mentoring:** architecture walkthrough, pair review critical flows, incident postmortem không đổ lỗi, ADR clinic và knowledge transfer cho đội trực vận hành.
- **Production feedback:** monitor SLO, near misses, reconciliation drift, cost/unit; cập nhật quyết định dựa trên evidence.

## 9. Go-live checklist

- [ ] FR/NFR, assumptions và owners được chấp thuận.
- [ ] HLD/LLD/API/event/data schema phiên bản đang triển khai khớp nhau.
- [ ] Security/threat review, resource-level authz, secrets and audit verified.
- [ ] Failures: timeout/unknown, duplicate, out-of-order, crash-after-commit, backlog có test.
- [ ] Load test và capacity headroom dựa trên traffic profile.
- [ ] CI/CD, rollback, backward-compatible DB migration và disaster recovery rehearsal.
- [ ] Observability, runbook, escalation, on-call, partner incident contact.
- [ ] Known deviations/risk register, exception expiry và người chấp nhận rủi ro.
- [ ] Evidence links, approval records, post-release verification.

**Kết quả chuẩn:** một solution package có thể được đội phát triển triển khai, các đơn vị liên quan review, đội vận hành hỗ trợ và người chịu trách nhiệm đánh giá quyết định sau khi hệ thống thay đổi.
