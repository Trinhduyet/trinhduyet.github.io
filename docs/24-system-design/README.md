# Module 24 — System Design

> [← Master Roadmap](../00-roadmap/master-roadmap.md) · [Roadmap Overview](../00-roadmap/README.md)

System Design là bước chuyển từ **“biết component”** sang **“biết ghép component thành hệ thống có thể chứng minh được”**.

Module này không học theo kiểu thuộc lòng:

```text
Load Balancer + Cache + Queue + Database + CDN = scalable system
```

Mà luôn bắt đầu từ:

```text
Business problem
    ↓
Functional Requirements
    +
Non-functional Requirements
    ↓
Workload / Capacity Estimates
    ↓
Data + Consistency
    ↓
Failure Model
    ↓
Architecture Options
    ↓
Trade-offs
    ↓
Evidence / Tests / Cost
```

![System Design workflow từ requirements đến evidence](../assets/diagrams/system-design-process.svg)

---

## Hiểu trong 5 phút

Một System Design tốt phải trả lời được 10 câu hỏi:

1. **Who** — ai dùng hệ thống, user flow nào là critical?
2. **What** — hệ thống phải làm gì và không làm gì?
3. **How much** — RPS, concurrency, data volume, bandwidth, growth?
4. **How fast** — latency P50/P95/P99 và user-visible deadline?
5. **How reliable** — availability/SLO, RTO, RPO, correctness?
6. **Where is truth** — source of truth nằm ở đâu, consistency cần mạnh đến mức nào?
7. **How does it scale** — stateless scale-out, partition, cache, async, CDN?
8. **How does it fail** — timeout, partial failure, overload, stale data, region outage?
9. **How is it operated** — deploy, observe, recover, replay, rollback?
10. **Why this design** — vì sao phương án này tốt hơn phương án đơn giản hơn?

Nếu chưa trả lời được workload và NFR thì chưa nên chọn database, queue hay Kubernetes.

---

# Learning path

| Guide | Priority | Bạn phải làm được |
|---|---:|---|
| [Requirements, NFR & Capacity Estimation](requirements-nfr-and-capacity-estimation.md) | **P0** | chuyển yêu cầu mơ hồ thành numbers, limits và SLO |
| [Traffic, Load Balancing, CDN & Cache](traffic-load-balancing-cdn-and-cache.md) | **P0** | scale request path và tránh bottleneck/hotspot |
| [Data, Replication, Partitioning & Consistency](data-partitioning-replication-and-consistency.md) | **P0** | chọn source of truth, shard key, consistency và query shape |
| [Async, Queue, Backpressure & Reliability](async-queues-backpressure-and-reliability.md) | **P0** | tách arrival rate khỏi processing rate và thiết kế failure semantics |
| [Availability, Multi-region, DR, Security & Cost](availability-multiregion-dr-security-and-cost.md) | **P0/P1** | thiết kế theo SLO/RTO/RPO thay vì mặc định active-active |
| [Case Studies & Design Review Workflow](case-studies-and-design-review.md) | **P0** | áp dụng framework vào URL Shortener, Notification, Checkout và AI Assistant |
| [References](references.md) | source | official/current sources + supplementary scope |

---

# System Design framework dùng xuyên module

## Step 1 — Clarify requirements

```text
Functional
- user actions
- admin/operator actions
- integrations
- critical flows

Non-functional
- scale
- latency
- availability
- consistency
- durability
- security/privacy
- compliance
- cost
```

## Step 2 — Estimate workload

Không cần số tuyệt đối chính xác. Cần **order-of-magnitude hợp lý** và assumptions rõ.

```text
DAU
requests/user/day
average RPS
peak multiplier
peak RPS
read/write ratio
payload size
storage/day
retention
bandwidth
concurrency
```

## Step 3 — Define data and ownership

```text
entities
relationships
access patterns
source of truth
transaction boundary
partition key
retention
privacy class
```

## Step 4 — Draw the simplest request path

```text
Client
  ↓
API
  ↓
Application
  ↓
Database
```

Chỉ thêm cache/CDN/queue/shard/region khi một requirement cụ thể tạo pressure.

## Step 5 — Find bottlenecks

```text
CPU
memory
DB connections
DB IOPS
hot key
network bandwidth
external API quota
queue consumers
lock/coordination
single region
```

## Step 6 — Add scale/reliability mechanisms

```text
stateless scale-out
load balancing
cache/CDN
partitioning
queue/backpressure
replication
redundancy
failure isolation
```

## Step 7 — Design failure behavior

```text
timeout
retry
unknown outcome
duplicate
partial commit
stale cache
queue backlog
node loss
zone loss
region loss
operator mistake
```

## Step 8 — Prove the design

```text
load test
failure injection
capacity model
trace
SLO dashboard
restore test
ADR
cost estimate
```

---

# Core component map

System Design roadmap thường xoay quanh các building blocks sau, nhưng **component không phải mục tiêu**:

```text
Clients
DNS
CDN / Edge
Load Balancer / Gateway
Stateless API
Cache
SQL / NoSQL / Search
Object Storage
Queue / PubSub / Stream
Workers
Observability
Identity / Security
Multi-zone / Multi-region
```

Roadmap.sh cũng liệt kê CDN, Load Balancer, Cache, Proxy, Queue, Web/App Server, Database, Search, Logging/Monitoring và scaling như các thành phần phổ biến của system design. Module này dùng chúng như **toolbox**, không như checklist bắt buộc.

---

# Một design answer đạt chuẩn

Không viết:

```text
Use Redis because cache is fast.
Use Kafka because system is large.
Use microservices for scalability.
```

Viết:

```text
Requirement:
P95 GET /products < 150 ms at 20k RPS.
Catalog changes ~20 times/minute.
Staleness <= 30 seconds is acceptable.

Option A:
Read SQL directly.
+ simple consistency
- DB reaches connection/IO capacity at peak

Option B:
Cache-aside Redis, TTL 20–30s + invalidation on write.
+ reduces DB read load
+ meets staleness budget
- introduces stale-data and cache-outage paths

Decision:
Use B only after load test proves DB saturation.
Fallback to DB with bounded concurrency if cache unavailable.
```

Đó là **requirements → evidence → trade-off → decision**.

---

# Kiến thức prerequisite

Module này giả định bạn đã hiểu tương đối chắc:

```text
HTTP / API Design
SQL + Index + Transactions
Caching
Docker/Kubernetes concepts
Observability
Distributed Systems
Microservices boundaries
```

Nếu chưa hiểu `timeout = unknown outcome`, `at-least-once`, idempotency, partition key hoặc SLO thì quay lại Module 17/18 trước.

---

# Quality gate

Một System Design exercise chỉ được tính hoàn thành khi có:

```text
requirements
+ assumptions
+ capacity estimates
+ architecture diagram
+ data model
+ failure analysis
+ trade-offs
+ security/privacy
+ observability
+ cost
+ migration/evolution path
```

“Vẽ được architecture” chưa phải System Design.

---

## LinkedIn PDF supplied for this update

User supplied a LinkedIn-hosted PDF as supplementary material. The automated environment could not retrieve the media asset because LinkedIn media delivery rejected the fetch, so **no unverified claim in this module is attributed to that PDF**. The source is retained in [references.md](references.md) for manual follow-up. Current technical claims are grounded in accessible official architecture/reliability sources.

## Verification metadata

- Verified: 2026-08-13.
- Scope reference: roadmap.sh System Design roadmap.
- Normative/current guidance: Microsoft Azure Architecture Center / Well-Architected Framework and existing protocol/database modules in this repository.
- Diagram policy: Diagram Design editorial SVG; no Mermaid source.
