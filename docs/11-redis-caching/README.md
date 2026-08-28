# Module 11 — Redis & Caching

> [← Performance](../10-performance/README.md) · [Docker →](../12-docker/README.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P1</span>
  <span><strong>Focus</strong>&nbsp;cache consistency · TTL · stampede · operations</span>
  <span><strong>Mode</strong>&nbsp;workload → cache policy → failure → evidence</span>
</div>

Redis không phải “database nhanh hơn SQL”. Trong backend production, câu hỏi đầu tiên là:

> **Tại sao workload này cần Redis/cache, và source of truth vẫn ở đâu?**

## Hiểu trong 5 phút

Baseline:

```text
Client
 ↓
API
 ↓
SQL source of truth
```

Nếu measured read workload tạo pressure thật và staleness được chấp nhận:

```text
Client
 ↓
API
 ├─ Redis cache
 └─ SQL source of truth
```

Gain:

```text
lower repeated-read latency
reduce downstream read work
absorb selected hot reads
```

New failures:

```text
stale data
invalidation bugs
stampede
hot key
memory/eviction pressure
Redis outage
split-brain/replication expectations
```

Cache chỉ đáng dùng khi gain lớn hơn consistency + operational cost.

---

# 1. Source of truth trước cache

Trước khi cache, ghi rõ:

```text
Authoritative data = ?
Allowed staleness = ?
Read/write ratio = ?
Hot-key distribution = ?
What happens if cache is empty/down = ?
```

Nếu không trả lời được source of truth và stale policy, cache design chưa hoàn chỉnh.

---

# 2. Cache-aside — mental model phổ biến

Read path:

```text
GET key from Redis
   ↓ hit
return cached value

   ↓ miss
read SQL
   ↓
store Redis with TTL
   ↓
return value
```

Pseudo-code:

```csharp
var cached = await cache.GetStringAsync(key, cancellationToken);
if (cached is not null)
{
    return Deserialize(cached);
}

var value = await repository.GetAsync(id, cancellationToken);

await cache.SetStringAsync(
    key,
    Serialize(value),
    cacheOptions,
    cancellationToken);

return value;
```

Điểm khó không phải `GET/SET`. Điểm khó là **write/invalidation/failure semantics**.

→ [Cache Consistency, Invalidation & Stampede](cache-consistency-invalidation-and-stampede.md)

---

# 3. TTL là consistency policy

TTL trả lời:

```text
Nếu không có invalidation event,
chúng ta chấp nhận data stale tối đa khoảng bao lâu?
```

Ví dụ:

```text
product description → 5 min maybe acceptable
inventory available-to-promise → 5 min may be dangerous
permission/revocation → long TTL may be unacceptable
```

Không chọn TTL bằng convention kiểu “mọi cache = 30 phút”.

TTL trade-off:

```text
short TTL
→ fresher
→ more misses/downstream load

long TTL
→ fewer misses
→ staler data / harder revocation
```

---

# 4. Write + invalidate race

Naive flow:

```text
update SQL
↓
delete cache
```

có thể vẫn gặp races với concurrent readers.

Một design phải answer:

```text
Can stale read repopulate cache after write?
Is invalidation best-effort or guaranteed?
How much stale time is allowed?
Would versioned key / event / write-through help?
```

Không có universal invalidation algorithm; consistency requirement quyết định complexity.

---

# 5. Cache stampede

Hot key hết hạn:

```text
10,000 concurrent requests
       ↓
all see cache miss
       ↓
10,000 SQL calls
```

Cache intended to protect SQL nhưng expiry moment lại làm SQL overload.

Mitigations tùy workload:

```text
request coalescing / single-flight
TTL jitter
background refresh
stale-while-revalidate style behavior
rate/concurrency limit
bounded fallback
```

Distributed lock có thể là một mechanism nhưng cũng thêm failure/lease complexity; không chọn mặc định.

---

# 6. Redis data structures — chọn theo operation

Không học như command catalog. Hỏi operation cần gì:

| Need | Candidate |
|---|---|
| key → value | String |
| object fields / partial field access | Hash |
| unique membership | Set |
| ordered score/ranking | Sorted Set |
| append/read stream semantics | Stream |

Ví dụ rate/window/ranking/coordination có thể dùng data structure phù hợp, nhưng correctness vẫn phụ thuộc atomicity, TTL, retries và failure semantics.

→ [Redis Data Structures & Command Shape](redis-data-structures-and-command-shape.md)

---

# 7. Cache key design

Key cần encode identity/version scope đúng:

```text
orders:{tenantId}:{orderId}:v2
```

Review:

```text
tenant isolation?
locale/currency?
permission-dependent response?
schema version?
key cardinality?
TTL policy?
```

Danger:

```text
cache response for User A
under key only /profile
→ User B receives A's data
```

Cache key là security/correctness boundary khi response phụ thuộc principal/context.

---

# 8. Serialization/versioning

Cached value là data contract giữa application versions.

Rolling deployment:

```text
v1 Pods + v2 Pods coexist
```

Nếu v2 ghi format mà v1 không đọc được:

```text
cache can break rollout
```

Options:

```text
backward-compatible serialized shape
versioned key prefix
short migration TTL
explicit cache flush only when blast radius accepted
```

Cache rollback compatibility phải được nghĩ cùng deployment.

---

# 9. Eviction != expiry

Expiry:

```text
key removed because policy/time says so
```

Eviction:

```text
Redis under memory policy removes keys to manage memory
```

Nếu system correctness phụ thuộc “key chắc chắn tồn tại cho tới TTL”, eviction policy/memory pressure có thể phá assumption đó.

Monitor:

```text
used memory
hit/miss
key count
evictions
expired keys
latency
connections
hot keys where observable
```

---

# 10. Redis outage behavior

Câu hỏi quan trọng:

```text
Redis down
→ core transaction down luôn?
```

Nếu Redis chỉ là performance cache, often desired:

```text
Redis unavailable
→ bounded fallback to SQL
→ protect SQL with concurrency/rate controls
→ degraded latency
```

Nhưng nếu Redis đang giữ coordination/session/rate state thì semantics khác.

Không nói “Redis failure should always be ignored”. Define role first.

---

# 11. Cache vs durable state

Đừng dùng cache cho data mà mất nó sẽ phá invariant nếu chưa có durable source.

```text
Cache
→ can be rebuilt

Durable authoritative state
→ recovery semantics matter
```

Redis có persistence/replication features, nhưng việc bật persistence không tự biến mọi cache design thành authoritative database design.

Nếu Redis được dùng như primary/coordination data store, phải review durability/HA/recovery theo role đó.

→ [Redis Operations, HA & Coordination](redis-operations-ha-and-coordination.md)

---

# 12. Distributed coordination caution

Examples:

```text
distributed lock
leader lease
idempotency marker
rate limit counter
```

Phải reason:

```text
atomic operation?
lease expiry?
client pauses?
network partition?
retry duplicate?
what is source of truth?
```

“Redis is atomic” không đủ để chứng minh business workflow đúng.

---

# 13. Performance and cost

Redis justification nên nối [Module 10 — Performance](../10-performance/README.md):

```text
Baseline SQL read cost/latency
→ measured hot/repeated reads
→ cache option
→ expected hit rate
→ memory footprint
→ Redis/network cost
→ failure behavior
```

Useful unit metrics:

```text
cache hit ratio
DB reads avoided
memory per cached object
P95 with/without cache
fallback load during outage
```

High hit rate không tự động tốt nếu data stale sai requirement.

---

# 14. Failure experiments

## A — Cold cache

Flush/test empty cache in controlled environment.

Observe:

```text
DB load
latency
recovery/warm-up
```

## B — Redis outage

Stop/unreachable Redis.

Verify:

```text
fallback behavior
SQL protection
error/latency telemetry
recovery when Redis returns
```

## C — Stampede

Expire hot key under concurrency; verify whether downstream load spikes.

## D — Stale write race

Run concurrent read + write around invalidation and capture stale behavior.

## E — Memory pressure

Constrain Redis/test eviction policy; observe evictions and application assumptions.

---

# 15. Khi nào KHÔNG dùng Redis

Don't add Redis because:

```text
"every production app needs cache"
"Redis is faster than SQL"
"we might scale later"
```

Skip/simplify when:

```text
SQL already meets SLO
low traffic
high write ratio
strict freshness makes cache low-value
team cannot own invalidation/failure semantics
memory/network/managed-service cost exceeds benefit
```

Sometimes best optimization is a SQL index/query fix.

---

# 16. Module map

| Guide | Focus |
|---|---|
| [Redis Data Structures & Command Shape](redis-data-structures-and-command-shape.md) | choose structure by operation/atomicity |
| [Cache Consistency, Invalidation & Stampede](cache-consistency-invalidation-and-stampede.md) | TTL, cache-aside, races, hot-key protection |
| [Redis Operations, HA & Coordination](redis-operations-ha-and-coordination.md) | memory/eviction, persistence/HA, coordination and failure |
| [References](references.md) | canonical sources |

## Evidence status

Module có deep/guided content nhưng chưa có dedicated `labs/11-redis-caching` runnable artifact.

Recommended evidence:

```text
before/after load result
key + TTL policy
stale-data decision
stampede experiment
Redis outage result
eviction/memory observation
fallback behavior
```

---

# 17. Exit criteria

Bạn hoàn thành Redis/cache foundation khi có thể:

- explain source of truth vs cache;
- justify cache from measured workload;
- design cache-aside read path;
- choose TTL from staleness requirement;
- reason invalidation races;
- explain/detect cache stampede;
- choose basic Redis data structures by operation;
- design tenant/context-safe keys;
- preserve compatibility during rolling deployment;
- distinguish expiration vs eviction;
- define Redis outage/degraded behavior;
- explain when Redis should not be added.

## Verification metadata

- Reviewed: 2026-08-28.
- Maturity: Deep/Guided; dedicated runnable lab pending.
- Quality model: [Learning Quality Standard](../00-roadmap/learning-quality-standard.md).
