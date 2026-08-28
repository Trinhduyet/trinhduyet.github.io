# Role-based Learning Paths

> Không cần đọc toàn bộ repository theo số module. Chọn role/problem hiện tại, học core path, tạo evidence rồi mới mở rộng.

## Bắt đầu hôm nay — 60 phút

Nếu chưa biết chọn đâu:

```text
10 phút — chọn một role bên dưới
15 phút — đọc overview module đầu tiên
20 phút — chạy/thiết kế một scenario + failure
10 phút — lưu evidence
 5 phút — viết 3 điều hiểu + 1 gap cần học tiếp
```

Evidence có thể là:

```text
command output
unit/integration test
execution plan
HTTP trace
container logs
kubectl describe/events
load-test result
architecture decision note
```

Đừng dùng “đã đọc trang” làm progress signal.

---

# 1. .NET Backend Engineer

## Core path

```text
01 Computer Science essentials
→ 02 Linux / Git / Networking
→ 03 .NET Runtime
→ 04 Backend Engineering
→ 05 SQL
→ 06 API Design
→ 07 ASP.NET Core
→ 08 Testing & Code Review
```

## Production layer

```text
09 Security
→ 10 Performance
→ 11 Redis when justified
→ 12 Docker
→ 13 DevOps basics
```

## Không cần deep ngay

```text
AKS internals
multi-region
microservices
advanced Terraform
AI agents
```

trừ khi project hiện tại cần.

## Evidence target

Build một API có:

- auth/AuthZ;
- relational schema + constraints/indexes;
- integration tests;
- idempotent write;
- Docker image;
- logs/metrics/traces;
- dependency timeout/failure test;
- basic CI pipeline.

**Đạt path này khi:** tự debug được request từ HTTP → app → SQL/dependency và giải thích correctness/performance/security trade-off.

---

# 2. Senior Backend Engineer

Start từ Backend Engineer path, sau đó:

```text
10 Performance
→ 13 DevOps / IaC
→ 14 Azure service selection
→ 17 Distributed Systems
→ 24 System Design
→ 25 Software Architecture
```

Kubernetes là **conditional**:

```text
12 Docker
→ 15 Kubernetes
```

khi runtime/platform requirements justify orchestration.

## Phải làm được

```text
capacity estimate
transaction/failure boundary
retry + idempotency
queue backlog reasoning
cache consistency
safe rollout
SLO / telemetry
cost / simpler alternative
ADR
```

## Evidence target

Một production backend dossier:

```text
architecture diagram
API contract
schema + query plans
load test
failure matrix
trace/dashboard
Docker/deployment config
rollback/recovery path
ADR
```

---

# 3. DevOps / Platform Engineer

## Prerequisites

```text
02 Linux / Git / Networking
→ 07 application hosting/health behavior
→ 08 testing gates
→ 09 security/supply chain
→ 12 Docker
```

Sau đó:

```text
13 DevOps / CI/CD / IaC
→ 14 Azure / Platform
→ 15 Kubernetes
```

Đừng học Kubernetes trước khi hiểu:

```text
process
signals
DNS/TCP/TLS
container image/runtime
CPU/memory
health checks
CI artifact identity
```

## Minimum platform capability

- CI quality gates;
- immutable artifacts/digests;
- environment promotion;
- Terraform state/plan/drift;
- identity/secrets without long-lived credentials when possible;
- networking/ingress/egress;
- Kubernetes Deployment/Service/config/resources/probes;
- rollout/rollback;
- observability;
- backup/DR boundary;
- incident/runbook.

## Evidence target

```text
Git SHA
→ build/test
→ image digest
→ registry
→ IaC
→ deployment
→ telemetry version
→ rollback/recovery
```

Recommended integration guide:
[DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md).

---

# 4. Kubernetes Application / Platform-ready Engineer

Không bắt đầu bằng YAML catalog.

## Core sequence

```text
12 Docker
→ 13 delivery lifecycle
→ 15 Kubernetes Overview
→ Cluster Architecture & Reconciliation
→ Workloads / Service / Networking / Storage
→ Config / Resources / Scheduling
→ Security / Observability
→ kubectl Debugging
```

## 20 concepts tối thiểu

```text
Cluster
Control Plane vs Worker
kube-apiserver
etcd
scheduler
kubelet
Pod
Deployment / ReplicaSet
Service
Labels / Selectors
DNS
ConfigMap
Secret
Requests / Limits
Readiness / Liveness
PVC / StorageClass
ServiceAccount / RBAC
Rollout / Rollback
HPA
kubectl debugging
```

Hai flow phải giải thích được:

```text
Deployment
  ↓
ReplicaSet
  ↓
Pod
  ↓
Container
```

```text
Client
  ↓
DNS
  ↓
Service
  ↓ selector
Ready Pods
```

## Evidence target

- deploy API với ≥2 replicas;
- expose bằng Service;
- set requests/limits + probes;
- break readiness;
- wrong image → debug ImagePullBackOff;
- crash → inspect previous logs;
- wrong selector → inspect EndpointSlice;
- rollout + rollback;
- explain HPA vs node capacity.

AKS chỉ học sau core Kubernetes:
[AKS Production Mapping](../15-kubernetes/aks-on-azure-production-architecture.md).

---

# 5. Cloud / Azure Backend Architect

## Foundation

```text
Backend + SQL + API
→ Security
→ Performance
→ Docker / DevOps fundamentals
```

## Azure path

```text
14 Azure Overview
→ Resource hierarchy / Landing Zones
→ Identity / Network / Private access
→ Compute selection
→ Data / Messaging selection
→ Operations / Cost
→ Reliability / DR
→ .NET Reference Architecture
```

## Service-selection skill

Không hỏi:

```text
Azure service nào tốt nhất?
```

Hỏi:

```text
workload shape?
state?
latency/SLO?
scale pattern?
network boundary?
identity?
HA/DR?
operational ownership?
cost driver?
```

Sau đó mới chọn App Service, Container Apps, Functions, VM, AKS, SQL, Cosmos, Service Bus...

## Evidence target

Một Azure architecture review có:

- subscription/resource boundary;
- identity/RBAC;
- network flow/private endpoints;
- compute/data/messaging decisions;
- availability-zone/region assumptions;
- monitoring/alerts;
- backup/restore/DR;
- cost formula + top cost risks;
- IaC/deployment strategy.

---

# 6. Distributed Systems / Microservices Engineer

## Prerequisites

```text
Backend
SQL transactions
API contracts
Testing
Observability mindset
```

Core:

```text
17 Distributed Systems
→ 18 Microservices only when autonomy boundaries matter
```

Kubernetes không phải prerequisite cho distributed correctness.

## Phải hiểu

```text
timeout = unknown outcome
retry can duplicate
at-least-once requires idempotent consumer
DB + broker dual-write gap
outbox/inbox
ordering scope
backpressure
saga/compensation
reconciliation
```

## Evidence target

```text
DB commit then response timeout
consumer crash before ACK
broker outage with outbox catch-up
poison message → bounded retry/DLQ
producer > consumer capacity
payment UNKNOWN → reconciliation
```

---

# 7. Software Architect / Staff Engineer

Không nhảy thẳng vào pattern catalog.

## Core sequence

```text
Backend + Data + API
→ Production Engineering
→ Distributed Systems
→ 24 System Design
→ 25 Software Architecture
```

Azure/Kubernetes/Microservices học theo project constraints, không phải title.

## Capability target

Có thể review:

```text
business outcome
NFR/SLO
capacity
source of truth
transaction boundary
timeout/duplicate/failure
security/trust boundary
deployment/recovery
cost
team/data ownership
architecture evolution
```

## Evidence target

Một design dossier phải chứa:

```text
requirements + assumptions
capacity model
data model/ownership
context/container/deployment diagram
failure model
security model
SLO/observability
cost
ADR/options rejected
migration/revisit trigger
```

---

# 8. Production AI Engineer

AI là capability đặt trên software engineering foundation.

## Required foundation

```text
03–08 Backend foundation
→ 09 Security
→ 13 Delivery
→ 14 Cloud basics
→ 17 Distributed failure
```

Then:

```text
19 AI Engineering
→ structured output / tools
→ RAG / evaluation / observability
→ 21 AI Coding Agents when relevant
→ 24/25 design & architecture
```

## Minimum production AI capability

- provider abstraction where useful;
- structured output + deterministic validation;
- tool authorization outside prompt;
- ACL-aware retrieval;
- eval dataset/regression gate;
- timeout/cancellation;
- latency/cost telemetry;
- prompt/model/index versioning concept;
- safe degradation/fallback;
- PII/security controls.

## Evidence target

Enterprise read-only assistant:

```text
authenticated user
→ authorized retrieval
→ cited answer
→ read-only business tool
→ structured result
→ eval + traces + cost
```

with failure drills for model timeout, retrieval outage, ACL leak attempt, malformed output and stale index.

---

# 9. Chọn depth theo level

| Level | Cần làm được |
|---|---|
| Awareness | biết problem + vocabulary |
| Explain | giải thích mechanism/failure/trade-off |
| Implement | code/config đúng + test |
| Operate | deploy/observe/debug/recover |
| Design / Review | chọn hoặc loại bỏ solution bằng requirements/evidence/cost |

Không phải mọi topic đều cần Level 5. Ví dụ Backend Engineer có thể cần Kubernetes Level 2–3, trong khi Platform Engineer cần Level 4–5.

→ [Skills Matrix](skills-matrix.md)

---

# 10. Rule để không học lan man

Trước khi mở module tiếp theo, trả lời:

```text
Tôi đang giải quyết problem nào?
Tôi thiếu concept nào để giải quyết nó?
Tôi sẽ tạo evidence gì sau 60–120 phút?
```

Nếu không trả lời được, quay lại một scenario cụ thể thay vì thêm topic vào backlog.

## Related

- [Human Learning Mode](human-learning-mode.md)
- [Example-First Learning Path](example-first-learning-path.md)
- [Learning Quality Standard](learning-quality-standard.md)
- [Repository Quality Review](repository-quality-review-2026-08-28.md)
