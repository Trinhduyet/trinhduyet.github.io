# Repository Quality Review — 2026-08-28

> Scope: toàn bộ learning system trong `docs/`, executable evidence trong `labs/`, roadmap/navigation và documentation CI.

Review này không chấm chất lượng bằng số trang. Nó hỏi: **người học có biết bắt đầu đâu, hiểu đúng mental model, áp dụng được vào system thật, debug được failure và có evidence để biết mình đã học chưa?**

## Executive summary

Repository đã có breadth và depth tốt ở nhiều mảng, đặc biệt:

```text
.NET / Backend / SQL / API
Docker
Azure
Kubernetes
Distributed Systems
Microservices
System Design
Software Architecture
AI Engineering
```

Gap lớn nhất không còn là “thiếu topic”. Gap lớn nhất là **consistency của learning experience và executable evidence**.

### 5 findings quan trọng

1. **Content maturity không đồng đều.** Module mới giàu scenario/failure/trade-off; một số overview cũ vẫn generic-template.
2. **Runnable evidence chưa theo kịp content.** Trước review, dedicated executable labs tập trung ở 01–04; review này bổ sung Kubernetes core lab cho Module 15, nhưng nhiều module sâu khác vẫn chưa có runnable artifact tương ứng.
3. **Roadmap/status bị stale.** Một số capability đã có deep module nhưng skills matrix vẫn ghi `Planned`.
4. **Planned coverage bị trộn với module thật.** Điều này khiến người đọc tưởng repository thiếu các module số 16/20/22/23/26/27 dù nhiều nội dung đã integrated vào module khác.
5. **Technology baseline cần refresh thường xuyên.** .NET patch, Kubernetes upstream minor và Terraform stable line đã thay đổi từ snapshot 2026-08-11.

---

# 1. Maturity map

Legend:

- **Deep** — production reasoning tốt, failure/trade-off rõ.
- **Guided** — nội dung tốt, có exercise nhưng executable artifact chưa đầy đủ.
- **Template debt** — chapter có nội dung nhưng module overview chưa đạt quality bar mới.
- **Runnable** — có artifact thực tế trong `labs/`.

| Module | Content maturity | Runnable repo lab | Review |
|---|---|---:|---|
| 00 Roadmap | Deep | — | cần giữ status/matrix đồng bộ |
| 01 Computer Science | Deep | **Yes** | strong foundation + workload lab |
| 02 Linux/Git/Networking | Deep | **Yes** | strong troubleshooting/incident path |
| 03 .NET Runtime | Deep | **Yes** | strong runtime diagnostics lab |
| 04 Backend | Deep | **Yes** | strong backend lab |
| 05 SQL | Deep/Guided | No dedicated lab | cần executable SQL/EF evidence project |
| 06 API Design | Deep/Guided | No dedicated lab | cần contract/integration runnable scenario |
| 07 ASP.NET Core | Deep/Guided | No dedicated lab | nên nối vào backend production lab |
| 08 Testing/Review | Deep/Guided | No dedicated lab | CI gate integration đã tốt; runnable suite cần mở rộng |
| 09 Security/DevSecOps | **Template debt → upgraded in this review** | No | cần threat/security executable lab |
| 10 Performance | **Template debt → upgraded in this review** | No | cần load/profiling runnable lab |
| 11 Redis/Caching | **Template debt → upgraded in this review** | No | cần cache failure/stampede lab |
| 12 Docker | Deep/Guided | No | commands tốt; nên có Compose artifact trong repo |
| 13 DevOps/IaC | Deep/Guided | No | strong delivery reasoning; cần sample pipeline/IaC lab |
| 14 Azure | Deep handbook | No | service-selection depth mạnh; labs/IaC là next gap |
| 15 Kubernetes | Deep/Guided | **Yes — core** | review thêm manifests/Kustomize + reconciliation/network/probe/rollout failure drills |
| 17 Distributed Systems | Deep/Guided | No | failure semantics tốt; executable outbox/dedup lab là P0 next step |
| 18 Microservices | Deep/Guided | No | strong boundary reasoning; checkout saga lab là next step |
| 19 AI Engineering | Deep/Guided | No | production mindset mạnh; eval/RAG/tool runnable project cần bổ sung |
| 21 AI Coding Agents | Deep/Guided | No | governance tốt; có thể thêm repo task simulation |
| 24 System Design | Deep | No direct runnable lab | design/evidence framework mạnh; project specs chưa phải executable project |
| 25 Software Architecture | Deep | No direct runnable lab | decision/evolution framework mạnh |

**Important:** `No` không có nghĩa module kém. Nó nghĩa **content evidence và executable evidence là hai maturity khác nhau**.

---

# 2. Learning architecture review

## Điểm mạnh

Repository có một philosophy khá nhất quán ở các module mới:

```text
Business problem
→ mechanism
→ failure
→ evidence
→ trade-off
```

Đây là hướng đúng cho Senior/Architect learning vì tránh học catalog công nghệ.

Project spine `Checkout / Orders / Payment / Notification` cũng tạo continuity tốt giữa Backend → Distributed Systems → Microservices → Architecture → AI.

## Gap

Người mới vẫn có thể bị overload vì:

- homepage có nhiều track cùng lúc;
- master roadmap dài;
- module numbers không phản ánh một đường học bắt buộc;
- chưa có role-specific minimal path rõ ở navigation.

### Improvement

Thêm [Role-based Learning Paths](role-based-learning-paths.md) và rule:

```text
role/problem
→ minimum modules
→ evidence target
→ optional depth
```

---

# 3. Content consistency review

## Strong pattern

Các README tốt như Docker/Kubernetes/Distributed/System Design thường có:

```text
5-minute mental model
real scenario
minimal code/config
failure drills
debug commands
when NOT to use
exit criteria
```

## Legacy pattern

Một số module overview cũ dùng template:

```text
focus
scope table
generic evidence bullets
generic next module
```

Vấn đề không phải wording; vấn đề là người đọc không biết:

```text
Security — threat nào?
Performance — đo gì trước?
Redis — khi nào cache justified?
```

### Improvement in this review

Rewrite overview của:

- Module 09 Security & DevSecOps;
- Module 10 Performance;
- Module 11 Redis & Caching.

Quality bar mới được ghi ở [Learning Quality Standard](learning-quality-standard.md).

---

# 4. Evidence review

Repository nói đúng rằng:

```text
read != learned
```

Nhưng chính repository phải phân biệt rõ:

```text
Guided exercise
!=
Runnable repo lab
```

Dedicated runnable artifacts hiện có cho Modules 01–04 và **Kubernetes core (15)**. Các module sâu còn lại chủ yếu có commands/config snippets/failure exercises trong docs.

Review này thêm:

```text
labs/15-kubernetes
├─ kustomization.yaml
├─ namespace.yaml
├─ deployment.yaml
├─ service.yaml
└─ README.md
```

Lab chứng minh trực tiếp:

```text
Deployment → ReplicaSet → Pod
Service → selector → Ready Pods
reconciliation
ImagePullBackOff
selector failure
readiness failure
resources
scale
rollout/rollback
kubectl debugging
```

### P0 lab backlog

Ưu tiên theo leverage:

1. **05–08 Production Backend Lab** — SQL + API + ASP.NET + tests trong một app thay vì 4 lab nhỏ rời nhau.
2. **09–13 Production Delivery Lab** — security gates + load test + Redis + Docker + CI.
3. **17 Distributed Reliability Lab** — SQL outbox/inbox + duplicate + crash-before-ACK.
4. **18 Checkout Saga Lab** — payment unknown outcome + reconciliation.
5. **14 Azure IaC Lab** — one reference environment with cost-aware defaults.
6. **19 AI Product Lab** — authorized RAG/tool/eval path.
7. **Extend Kubernetes lab** — ConfigMap/Secret, RBAC, PVC, NetworkPolicy, HPA as separate problem-driven exercises.

Một integrated lab có giá trị hơn 20 snippets không liên kết.

---

# 5. Status/matrix review

Stale metadata tạo trust problem: người đọc không biết nên tin content hay roadmap.

Examples trước review:

```text
Module 17 exists and is deep
but Distributed skills = Planned

Module 24 exists and is deep
but System Design skills = Planned

Module 25 exists and is deep
but Architecture skills = Planned
```

Ngoài ra public skills matrix không nên chứa assumed current skill level của một cá nhân.

### Improvement

Skills matrix chuyển thành:

```text
Capability
Priority
Target level
Content maturity
Runnable evidence
Evidence path
```

Learner tự đánh giá level trong progress artifact riêng.

---

# 6. Planned modules review

Master roadmap từng liệt kê:

```text
16 Observability
20 RAG
22 AI Security
23 GenAIOps
26 Architecture Docs
27 Data Engineering
```

trộn cùng module directory đang tồn tại.

Điều này gây ambiguity vì một số capability đã integrated:

```text
Observability → 07/10/14/15/17/19
RAG → 19
AI security → 19/21
Architecture docs → 24/25
```

### Improvement

Tách:

```text
Existing modules
vs
Integrated / future capability backlog
```

Không tạo “module ảo” trong core learning path.

---

# 7. Version/source review

Stable concepts trong repo nhìn chung tốt. Rủi ro lớn là snapshot version nhanh stale.

Refresh 2026-08-28:

- .NET 10 LTS latest servicing patch: `10.0.11`.
- EF Core 10 latest stable package: `10.0.11`.
- Kubernetes upstream current stable: `1.37.0`, released 2026-08-26.
- Terraform current stable release observed: `1.16.0`, released 2026-08-26.
- Docker Engine 29 latest documented patch remains `29.7.2` at review time.
- SQL Server 2025 latest CU shown by Microsoft is CU8 (August 2026).

Rule quan trọng hơn numbers:

```text
Docs conceptual baseline
!=
Managed cloud supported version
!=
Executable lab pin
```

Managed Kubernetes provider có thể lag upstream. Production phải check provider support matrix trước upgrade.

---

# 8. UX/navigation review

## Existing strengths

- MkDocs Material navigation rõ theo domain;
- search tốt;
- top-level groups Backend / Production / DevOps & Kubernetes / Azure / Distributed / Design / AI hợp lý hơn cấu trúc module-number thuần túy.

## Improvements

Start Here ưu tiên sau review:

```text
1. Role-based path
2. Example-first path
3. Human learning mode
4. Master roadmap (deep reference)
```

Không bắt người mới đọc master roadmap trước.

Homepage có CTA rõ:

> “Có 60 phút? Đừng đọc roadmap trước.”

---

# 9. Documentation CI review

Current CI đã tốt ở:

- portable path audit;
- diagram source audit;
- `mkdocs build --strict`;
- generated-site verification;
- live deployed commit verification.

Gap trước review: CI chưa kiểm learning-system quality debt.

### Improvement

Thêm `scripts/audit-learning-quality.py` để report:

- module có README không;
- module có references không;
- README có exit/evidence/verification signals không;
- runnable lab coverage;
- content-vs-lab maturity summary.

Audit ban đầu **report warnings**, không block legacy debt. Khi backlog được giải quyết dần có thể bật strict mode.

---

# 10. Prioritized backlog sau review

## P0 — leverage cao

- integrated Production Backend lab (05–08);
- Docker/Redis/security/performance delivery lab (09–13);
- Distributed outbox/idempotency runnable lab;
- keep roadmap/skills/status generated or audit-checked;
- extend Kubernetes core lab only when each extra concept has a real problem scenario.

## P1

- Azure IaC reference lab;
- Microservices checkout saga executable project;
- dedicated observability learning slice only if integrated coverage becomes hard to navigate;
- Architecture decision packet examples (ADR/C4/failure model/runbook).

## P2

- Data Engineering dedicated track;
- advanced Kubernetes admin/CKA track;
- advanced MLOps/fine-tuning infrastructure.

---

# 11. Quality scorecard after this review

| Dimension | Assessment |
|---|---|
| Breadth | Strong |
| Core technical depth | Strong |
| Production reasoning | Strong in newer modules; 09–11 overview upgraded |
| Failure-first learning | Strong and more consistent after review |
| Navigation | Good → role/action-first |
| Source policy | Strong |
| Version freshness | Refreshed; still requires cadence |
| Executable labs | **Still primary gap, but Kubernetes core now runnable** |
| Evidence honesty | Improved by separating Guided vs Runnable |
| Status consistency | Improved by roadmap/matrix rewrite |

## Final direction

Repository nên tiếp tục theo chiến lược:

```text
STOP maximizing topic count
START maximizing executable learning loops
```

Mỗi lần mở rộng hãy ưu tiên:

```text
one real system
+ one failure
+ one diagnostic path
+ one recovery
+ one decision
```

hơn việc thêm 10 pattern names.

## Verification metadata

- Review date: 2026-08-28.
- Repository base reviewed: `main` at `b18390ee03b0c446bd9a68029597a8b37734cb28`.
- Improvements in this review include roadmap/status/quality CI changes and new `labs/15-kubernetes` core executable evidence.
- Scope: current `docs/`, `labs/`, `mkdocs.yml`, documentation scripts/workflow.
- Current upstream version checks use official/vendor sources; see [Technology Baseline](technology-baseline.md).
