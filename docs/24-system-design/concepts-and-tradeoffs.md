# System Design Concepts & Trade-offs — 36 khái niệm phải hiểu bằng failure

> [← System Design overview](README.md) · [Case Studies](case-studies-and-design-review.md)

<div class="lesson-meta">
  <span><strong>Mode</strong>&nbsp;concept → example → failure → trade-off</span>
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Use</strong>&nbsp;design review + interview + production</span>
</div>

Không học System Design bằng cách thuộc sơ đồ:

```text
CDN → LB → API → Redis → Kafka → DB
```

Mỗi building block phải trả lời:

```text
Problem nào?
Mechanism gì?
Failure nào mới xuất hiện?
Trade-off gì phải trả?
Khi nào KHÔNG dùng?
```

## 1. Scalability — khả năng chịu tăng workload

Không phải “có Kubernetes”.

```text
Current peak = 1k RPS
Target peak  = 10k RPS
```

Scale có thể đến từ:

- tối ưu query;
- cache;
- tăng instance;
- horizontal scale;
- partition data;
- async processing;
- giảm work/request.

**Failure:** scale API 10× nhưng DB connection pool/IO không scale → bottleneck chuyển xuống DB.

## 2. Availability — hệ thống có phục vụ khi được yêu cầu không?

Availability là user-visible outcome theo window, không chỉ process uptime.

```text
API process alive
but DB timeout
→ checkout unavailable
```

**Trade-off:** redundancy tăng cost/operational complexity.

## 3. Reliability — đúng và recover được qua failure

Một payment API có thể trả 200 rất nhanh nhưng double-charge → không reliable về correctness.

Reliability gồm:

```text
availability
+ correctness
+ durability
+ recoverability
```

## 4. Latency vs Throughput vs Bandwidth

- **Latency**: một operation mất bao lâu.
- **Throughput**: xử lý bao nhiêu operation mỗi đơn vị thời gian.
- **Bandwidth**: capacity truyền dữ liệu của link/path.

Ví dụ:

```text
P95 = 100 ms
throughput = 5k req/s
payload = 100 KB
```

Có thể latency tốt nhưng bandwidth saturation ở peak.

## 5. P50 / P95 / P99

Average che tail latency.

```text
99 requests = 50 ms
1 request   = 5 s
```

Average vẫn có thể “đẹp” nhưng user xấu nhất bị timeout.

Design SLO nên quan tâm percentile phù hợp user/business flow.

## 6. Vertical vs Horizontal Scaling

### Vertical

```text
8 CPU → 32 CPU
```

+ đơn giản
- có ceiling / bigger failure unit

### Horizontal

```text
2 instances → 20 instances
```

+ scale/failure distribution
- yêu cầu statelessness/coordination/data scaling phù hợp

Không phải workload nào cũng cần horizontal ngay.

## 7. Load Balancer

Phân phối traffic giữa healthy backends.

```text
Client
 ↓
Load Balancer
 ├─ API 1
 ├─ API 2
 └─ API 3
```

Failure cần nghĩ:

- health check quá nông;
- slow instance vẫn “healthy”;
- sticky session tạo hotspot;
- retry ở LB duplicate POST.

## 8. Reverse Proxy vs API Gateway

Reverse proxy tập trung traffic/routing/TLS class concerns.

API Gateway thường thêm API-specific concerns:

```text
auth/policy
quota/rate limit
versioning
transformation
developer exposure
```

Không nhét business logic vào gateway chỉ vì dễ centralize.

## 9. CDN

Cache/serve content gần user, giảm origin load/latency.

Phù hợp static/cacheable content.

Failure:

```text
bad cache-control
→ user sees stale/private content
```

CDN không tự biến dynamic personalized API thành cacheable.

## 10. Cache

Cache đổi:

```text
latency + DB load
```

lấy:

```text
staleness + invalidation + failure path
```

### Cache-aside

```text
read cache
miss → DB
write cache
```

Câu hỏi bắt buộc:

```text
TTL?
invalidation?
source of truth?
cache outage fallback?
cache stampede?
hot key?
```

## 11. Cache Stampede

1000 request cùng miss một key:

```text
cache miss × 1000
→ DB query × 1000
```

Mechanisms:

- single-flight/coalescing;
- jittered TTL;
- refresh-ahead;
- bounded fallback.

## 12. SQL vs NoSQL

Không hỏi:

```text
system lớn → NoSQL?
```

Hỏi:

```text
access pattern?
transaction boundary?
relationship/query?
partition model?
consistency?
scale/cost?
```

Relational DB có thể scale rất xa nếu workload/schema/index hợp lý.

## 13. Index

Index là data structure tối ưu access pattern đổi lấy:

```text
storage
write amplification
maintenance
```

Index mọi column sẽ làm write đắt và optimizer complexity tăng.

## 14. Replication

Copy data sang replicas để availability/read scale/geo goals.

Trade-off:

```text
replication lag
failover semantics
write authority
consistency
```

Read replica không phù hợp nếu user vừa write xong bắt buộc read-your-own-write mà replica lag.

## 15. Sharding / Partitioning

Chia data/workload theo key.

```text
CustomerId % N
```

hoặc range/hash/domain partition.

Hard part:

```text
partition key
hot partition
cross-shard query
rebalance
transaction across shards
```

Đừng shard trước khi single-store capacity thực sự tạo pressure.

## 16. Consistent Hashing

Giảm lượng key cần remap khi node set thay đổi trong distributed cache/storage/routing class scenarios.

Không phải magic “giải quyết sharding”; vẫn còn replication, hotspot, membership và failure handling.

## 17. ACID

Transaction giúp một boundary giữ invariants atomically theo DB semantics.

Ví dụ:

```text
reserve stock
+ create order
```

trong cùng DB có thể transaction.

Across payment provider:

```text
local DB transaction
!=
remote side effect transaction
```

## 18. CAP — partition xảy ra thì chọn behavior gì?

CAP useful khi reasoning distributed system trong **network partition**.

Không dùng CAP để nói:

```text
SQL = CP
NoSQL = AP
```

một cách tuyệt đối.

Phải nói operation/system mode cụ thể.

## 19. PACELC

Ngoài partition (`P`), khi hệ thống chạy bình thường (`E`lse), distributed design vẫn trade **Latency vs Consistency**.

Nó nhắc rằng consistency trade-off không chỉ xuất hiện lúc outage.

## 20. Strong vs Eventual Consistency

### Strong-ish requirement

```text
available inventory must not oversell
```

### Eventual acceptable

```text
analytics dashboard can lag 30 seconds
```

Consistency là business requirement theo field/flow, không phải checkbox database toàn hệ thống.

## 21. Message Queue

Decouple arrival rate khỏi processing rate.

```text
1000 req/s intake
→ queue
→ workers process 600/s
```

Queue giúp absorb burst, nhưng backlog tăng 400/s.

Nếu kéo dài 10 phút:

```text
400 × 600 = 240,000 pending
```

Queue không tạo capacity; nó mua **time**.

## 22. Pub/Sub

Một event có nhiều independent consumers:

```text
OrderPaid
 ├─ fulfillment
 ├─ notification
 └─ analytics
```

Need:

```text
schema evolution
consumer isolation
retry/DLQ
duplicate handling
```

## 23. At-least-once Delivery

Message có thể delivered nhiều lần.

Consumer design:

```text
message/business ID
→ dedup/invariant
→ apply once logically
```

Đừng thiết kế “duplicate sẽ hiếm”.

## 24. Idempotency

Cùng logical command nhiều lần không tạo side effect ngoài ý muốn.

```http
POST /payments
Idempotency-Key: pay-order-123
```

Idempotency cần durable business identity, không chỉ retry middleware.

## 25. Outbox / Inbox

### Outbox

Business state + event/message intent trong cùng local transaction.

### Inbox/Dedup

Consumer ghi nhận message/business identity đã xử lý.

Hai pattern giúp at-least-once system đạt **logical once-effects** trong boundary, không tạo distributed exactly-once tuyệt đối.

## 26. Backpressure

Downstream chậm → upstream phải biết giảm/limit/admit.

Không có backpressure:

```text
arrival > processing
→ queue/memory/connections grow
→ latency grows
→ timeout/retry
→ more load
→ collapse
```

Mechanisms:

- bounded queue;
- semaphore/concurrency limit;
- rate limit;
- load shedding;
- retry-after;
- autoscale where effective.

## 27. Rate Limiting vs Throttling

Rate limit bảo vệ resource/fairness bằng giới hạn requests/window/token budget.

Throttling có thể reject, delay hoặc shape traffic theo system semantics.

Algorithm examples:

- token bucket;
- leaky bucket;
- fixed/sliding window.

## 28. Retry + Exponential Backoff + Jitter

Retry chỉ cho transient failures và operation safe/idempotent.

Bad:

```text
1000 clients fail
→ retry exactly after 1 second
→ retry storm
```

Jitter phá synchronization.

Bound retry bằng deadline/attempt budget.

## 29. Circuit Breaker

Khi dependency đang fail, stop hammering trong một thời gian/condition để fail fast/degrade.

Circuit breaker không thay timeout, retry budget hay fallback design.

## 30. Timeout = UNKNOWN trong distributed side effect

Payment call timeout:

```text
provider may have charged
or may not
```

Do đó:

```text
TIMEOUT != FAILED
```

Cần:

```text
idempotency key
query/status API
webhook
reconciliation
```

Đây là một trong những mental models quan trọng nhất của production distributed systems.

## 31. Service Discovery

Giải quyết dynamic service location.

Trong modern platform, discovery có thể do DNS, orchestrator/platform hoặc service registry.

Không thêm Consul/Eureka nếu platform đã giải quyết đủ requirement.

## 32. Synchronous vs Asynchronous

### Sync

+ simple request/response
+ immediate result
- temporal coupling
- cascade latency/failure

### Async

+ decouple time/load
+ buffer/retry
- eventual result
- duplicate/ordering/backlog/operations complexity

Chọn theo user flow và failure semantics.

## 33. WebSockets / SSE / Long Polling

Dùng khi server cần push/update gần realtime.

- WebSocket: bidirectional persistent channel.
- SSE: server → client stream over HTTP semantics.
- Long polling: request held/repeated.

Trade-off gồm connection count, fanout, ordering, reconnect/resume và load balancer state.

## 34. SLI / SLO / SLA

- **SLI**: measure như success rate, latency.
- **SLO**: internal reliability target.
- **SLA**: contractual/business commitment, thường có consequence.

Đừng alert mọi metric như nhau. Alert nên map tới user/business impact và error budget strategy.

## 35. RTO / RPO / Disaster Recovery

- **RTO**: bao lâu để recover.
- **RPO**: mất bao nhiêu data/time point chấp nhận.

DR chỉ có giá trị khi restore/failover được test.

```text
backup exists
!=
restore works within RTO
```

## 36. Single Point of Failure / Fault Isolation

SPOF không chỉ là “một server”. Có thể là:

```text
one database
one DNS dependency
one identity tenant dependency
one deployment pipeline
one region
one human approval account
one shared quota
```

Fault isolation thiết kế blast radius để failure không lan toàn hệ thống.

---

# Trade-off matrix cần nhớ

| Decision | Gain | Cost / Failure introduced |
|---|---|---|
| cache | lower latency/load | stale data/invalidation/outage path |
| replicas | availability/read scale | lag/failover consistency |
| shards | write/data scale | routing/rebalance/cross-shard complexity |
| queue | burst absorption/decoupling | backlog/duplicate/DLQ/operations |
| microservices | independent boundaries/deploy | network failure/data consistency/ops |
| multi-region | geo resilience/latency | cost/data/failover complexity |
| strong consistency | correctness/simple mental model | latency/availability constraints |
| eventual consistency | availability/scale flexibility | stale/conflicting views |
| synchronous call | simple immediate result | temporal coupling/cascade |
| async call | decoupling | state machine/eventual result |

# Design review questions

Một component chỉ được thêm khi trả lời được:

1. Requirement nào tạo pressure?
2. Baseline đơn giản fail ở đâu?
3. Metric/evidence nào chứng minh?
4. Component mới cải thiện gì?
5. Failure mode mới là gì?
6. Operational burden mới là gì?
7. Cost mới là gì?
8. Khi nào remove/migrate?

<div class="key-takeaway" markdown>
<strong>System Design maturity</strong>

Junior nhớ tên components. Senior biết component hoạt động thế nào. Architect biết **khi nào không cần component**, failure nào nó tạo ra và evidence nào justify trade-off.
</div>

## Nguồn scope

Trang này đối chiếu breadth với các tài liệu user cung cấp:

- `karanpratapsingh/system-design`: networking, data, messaging, architecture, reliability/security và case studies.
- `ashishps1/awesome-system-design-resources`: core concepts, networking/API/database/cache/async/distributed concepts, trade-offs, interview cases và engineering readings.
- `mehdihadeli/awesome-software-architecture`: architecture styles/patterns và distributed/cloud topics.

Các nguồn trên dùng để kiểm tra coverage; production decisions trong roadmap vẫn theo official specifications/provider docs + measured evidence.
