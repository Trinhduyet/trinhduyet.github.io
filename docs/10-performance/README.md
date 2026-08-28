# Module 10 — Performance Engineering

> [← Security & DevSecOps](../09-security-devsecops/README.md) · [Redis & Caching →](../11-redis-caching/README.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;measurement · bottleneck · capacity · regression</span>
  <span><strong>Mode</strong>&nbsp;measure → explain → change → prove</span>
</div>

Performance không phải “làm code nhanh hơn”. Production performance là khả năng trả lời bằng evidence:

```text
workload là gì?
SLO/latency target là gì?
bottleneck nằm ở đâu?
resource nào đang saturate?
change có cải thiện tail latency thật không?
cost/complexity đổi thế nào?
```

## Hiểu trong 5 phút

Một Orders API chậm có thể do:

```text
Client
 ↓
Network / TLS
 ↓
ASP.NET Core
 ↓
ThreadPool / CPU / GC
 ↓
SQL connection pool
 ↓
query / lock / I/O
 ↓
external dependency
```

Nếu chỉ nhìn `average response time`, bạn không biết layer nào tạo pressure.

Performance loop đúng:

```text
Requirement
  ↓
Representative workload
  ↓
Baseline
  ↓
Measure latency + throughput + resources
  ↓
Find bottleneck
  ↓
Change one mechanism
  ↓
Re-measure
  ↓
Regression gate
```

---

# 1. Vocabulary tối thiểu

Phân biệt:

```text
Latency
= một operation mất bao lâu

Throughput
= xử lý bao nhiêu work / time

Concurrency
= bao nhiêu work đang in-flight

Utilization
= resource bận mức nào

Saturation
= demand vượt usable capacity, queue/wait tăng mạnh
```

Tail latency quan trọng:

```text
P50 = typical
P95/P99 = slow tail users experience
```

Average có thể đẹp trong khi P99 rất xấu.

---

# 2. Requirement trước benchmark

Bad:

```text
API must be fast.
```

Better:

```text
Peak: 2,000 RPS
P95 GET /orders/{id} < 150 ms
P99 < 400 ms
Error rate < 0.5%
Payload ~20 KiB
Read/write = 80/20
```

Benchmark không có representative workload dễ tối ưu sai system.

---

# 3. Baseline trước optimization

Trước change, capture:

```text
RPS
P50/P95/P99
error rate
CPU
memory / allocation / GC
ThreadPool / queueing
DB connections
query duration / plans / waits
external dependency latency
```

Sau change, chạy cùng workload.

Không kết luận từ:

```text
"cảm giác nhanh hơn"
"CPU giảm"
"benchmark micro-method tốt hơn"
```

nếu end-to-end SLO không cải thiện hoặc correctness bị đổi.

→ [Measurement, Profiling & Bottlenecks](measurement-profiling-and-bottlenecks.md)

---

# 4. Bottleneck — tối ưu resource đang giới hạn system

Ví dụ:

```text
API CPU = 35%
SQL CPU = 90%
DB connection wait tăng
P99 tăng theo peak RPS
```

Tăng API replicas có thể không giúp; nó còn có thể mở thêm DB connections và làm SQL tệ hơn.

Mental model:

```text
System capacity
≈ capacity của constrained path / bottleneck
```

Bottleneck có thể đổi sau mỗi optimization.

---

# 5. Queueing intuition

Khi arrival rate tiến gần service capacity:

```text
arrival ≈ capacity
→ waiting grows
→ tail latency explodes
```

Đó là lý do system thường cần headroom thay vì target 100% sustained utilization.

Little's Law intuition:

```text
concurrency ≈ throughput × latency
```

Ví dụ 1,000 req/s với 200 ms average latency:

```text
~200 requests in-flight
```

Nếu dependency latency tăng gấp 5, concurrency in-flight có thể tăng mạnh dù RPS không đổi → pressure lên connections/memory/thread/task state.

---

# 6. .NET performance path

Đối với ASP.NET Core, review theo order:

```text
request workload
↓
async/cancellation correctness
↓
ThreadPool starvation?
↓
CPU hot path?
↓
allocations / GC pressure?
↓
locks/contention?
↓
DB/external I/O?
```

Không mặc định GC là root cause chỉ vì memory cao.

Useful evidence có thể gồm:

```text
dotnet-counters / metrics
dotnet-trace / profiler
allocation profile
ThreadPool counters
application traces
```

Tool names/version có thể thay đổi; mental model là **measure the constrained resource**.

---

# 7. SQL thường là phần của performance story

Backend performance review phải nối [Module 05 — SQL](../05-sql/README.md):

```text
query shape
index
execution plan
cardinality
locking/blocking
connection pool
transaction duration
```

Bad optimization:

```text
add cache
```

trước khi biết query chậm vì missing index hoặc N+1.

---

# 8. Load test không phải spam endpoint

Representative load model cần:

```text
arrival pattern
read/write mix
payload distribution
auth/session behavior
dependency behavior
warm-up
steady state
spike/soak where relevant
```

Measure:

```text
latency distribution
throughput
errors/timeouts
resource saturation
queue depth
recovery after load
```

→ [Load, Capacity & Scalability](load-capacity-and-scalability.md)

---

# 9. Scale up vs scale out

## Scale up

```text
more CPU/RAM per instance
```

Simple nhưng có ceiling/cost/failure-domain implications.

## Scale out

```text
more instances
```

Works best khi request processing/state can be distributed.

Nhưng scale-out app tier không tự scale:

```text
SQL
Redis
external API quota
queue consumer partition limit
network egress
```

System capacity phải review end-to-end.

---

# 10. Cache chỉ là performance mechanism khi workload justify

Cache có thể giảm latency/read load nhưng tạo:

```text
stale data
invalidation
stampede
memory/eviction pressure
cache outage behavior
```

Do đó flow đúng:

```text
measure read bottleneck
→ prove cache can reduce constrained work
→ define staleness policy
→ test failure
```

→ [Module 11 — Redis & Caching](../11-redis-caching/README.md)

---

# 11. Performance budget

Một request budget có thể được chia:

```text
P95 target = 200 ms

edge/network       20 ms
application        30 ms
SQL                80 ms
external service   50 ms
headroom            20 ms
```

Không phải contract toán học tuyệt đối; nó giúp identify dependency nào ăn hết latency budget.

Same idea cho resource/cost:

```text
CPU per request
DB queries/request
bytes/request
external calls/request
```

---

# 12. Regression control

Optimization không có regression gate sẽ mất dần theo thời gian.

CI/nightly/per-release tùy cost:

```text
representative test
→ compare baseline/budget
→ detect statistically/materially meaningful regression
→ investigate before release
```

Không fail CI vì noise 1–2% nếu benchmark không đủ stable.

→ [Optimization Budgets & Regression Control](optimization-budgets-and-regression-control.md)

---

# 13. Common failure patterns

## ThreadPool starvation

Symptom:

```text
CPU không nhất thiết 100%
request latency tăng
work queue / thread growth abnormal
```

Potential cause: blocking async flow.

## DB saturation

```text
API scale-out
→ more concurrent DB work
→ locks/connections/CPU saturate
→ latency worse
```

## Retry amplification

```text
dependency slow
→ requests timeout
→ retry
→ more load
→ dependency slower
```

## Cache stampede

```text
hot key expires
→ thousands requests miss
→ all hit DB
```

## Memory pressure

```text
allocations / retained state rise
→ GC/working set pressure
→ latency/restart/OOM risk
```

---

# 14. Failure/performance experiments

## A — Increase RPS until knee point

Plot/record:

```text
RPS
P95/P99
errors
CPU
DB/resource saturation
```

Find where throughput stops scaling linearly and latency accelerates.

## B — Slow dependency

Inject latency in downstream service.

Observe:

```text
in-flight requests
timeouts
retry rate
resource pressure
recovery
```

## C — Remove/use bad index in controlled lab

Compare actual query plan + API latency, not only SQL duration.

## D — Memory bound

Run container/process with reduced memory budget; inspect GC/working set/OOM behavior.

## E — Regression

Introduce known slower implementation; confirm performance gate/report can detect it.

---

# 15. Khi nào KHÔNG optimize

Don't optimize because:

```text
"this allocation looks ugly"
"Redis is fast"
"microservice should scale"
"we can parallelize it"
```

Optimize when:

```text
requirement exists
+ measured bottleneck exists
+ change benefit can be verified
+ complexity/cost is acceptable
```

Sometimes best improvement is:

```text
better index
smaller response
fewer remote calls
shorter transaction
remove unnecessary abstraction
```

---

# 16. Module map

| Guide | Focus |
|---|---|
| [Measurement, Profiling & Bottlenecks](measurement-profiling-and-bottlenecks.md) | baseline, profiler, CPU/memory/contention diagnosis |
| [Load, Capacity & Scalability](load-capacity-and-scalability.md) | representative load, saturation, headroom, scaling |
| [Optimization Budgets & Regression Control](optimization-budgets-and-regression-control.md) | budgets, evidence and preventing regressions |
| [References](references.md) | canonical sources |

## Evidence status

Module có deep/guided content nhưng chưa có dedicated `labs/10-performance` runnable artifact.

Recommended evidence:

```text
load model
baseline report
P50/P95/P99
resource graphs/counters
profiler evidence
bottleneck hypothesis
before/after result
regression budget
```

---

# 17. Exit criteria

Bạn hoàn thành foundation khi có thể:

- define latency/throughput/concurrency/saturation correctly;
- turn vague performance requirement into measurable SLO/workload;
- capture baseline before optimization;
- identify CPU vs memory vs ThreadPool vs DB vs dependency bottleneck;
- reason about queueing/headroom and tail latency;
- design representative load test;
- explain why scaling app tier can overload data/dependency tier;
- connect SQL plan/index evidence to API performance;
- decide when cache helps and what failures it adds;
- prove an optimization with before/after evidence;
- define a practical regression gate/budget.

## Verification metadata

- Reviewed: 2026-08-28.
- Maturity: Deep/Guided; dedicated runnable lab pending.
- Quality model: [Learning Quality Standard](../00-roadmap/learning-quality-standard.md).
