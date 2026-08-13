# Requirements, NFR & Capacity Estimation

> [← System Design overview](README.md) · [References](references.md)

## Hiểu trong 5 phút

Sai lầm phổ biến nhất trong System Design là chọn technology trước khi biết workload.

Bad:

```text
Need scalable system
→ Kubernetes
→ Redis
→ Kafka
→ Sharding
```

Better:

```text
critical user flow
→ NFR
→ traffic/data estimate
→ bottleneck hypothesis
→ simplest design
→ scale only where pressure appears
```

---

# 1. Functional Requirements vs Non-functional Requirements

## Functional Requirements — FR

Mô tả hệ thống **phải làm gì**.

Ví dụ notification platform:

```text
User subscribes to topics
System receives domain events
System decides recipients
System sends email/push/webhook
User can unsubscribe
Operator can replay failed delivery
```

## Non-functional Requirements — NFR

Mô tả hệ thống **phải tốt đến mức nào**.

```text
Availability      99.95%
P95 API latency   < 250 ms
Peak ingest       10k events/s
Delivery delay    95% < 60 sec
Durability        no accepted event lost
RPO               < 5 min
RTO               < 30 min
Retention         90 days
Regions           Vietnam + EU users
Security          tenant isolation + audit
Budget            <$X/month
```

System Design bắt đầu khi NFR trở thành measurable constraints.

---

# 2. Critical flows trước component

Không phải mọi endpoint đều cần cùng SLO.

Ví dụ checkout:

```text
Flow A — Create checkout          critical
Flow B — Check order status       critical
Flow C — Admin export             non-critical
Flow D — Analytics dashboard      degradable
```

Thiết kế reliability/capacity theo **flow**, không theo logo service.

Microsoft Well-Architected guidance cũng khuyến nghị xác định critical user/system flows và đặt SLO/recovery target cho flow đó thay vì chỉ nhìn từng component riêng lẻ.

---

# 3. SLI, SLO, SLA

## SLI — Service Level Indicator

Measurement:

```text
availability ratio
success ratio
P95 latency
freshness
correctness
```

## SLO — Service Level Objective

Target engineering:

```text
99.95% successful checkout requests per month
P95 checkout API latency < 700 ms excluding payment provider
```

## SLA — Service Level Agreement

Business/contractual commitment. SLA không nên bị dùng như synonym của SLO.

### Availability budget

Một cách estimate đơn giản:

```text
monthly_minutes = 30 × 24 × 60 = 43,200
```

| Availability | Approx downtime / 30 days |
|---:|---:|
| 99% | 432 min |
| 99.9% | 43.2 min |
| 99.95% | 21.6 min |
| 99.99% | 4.32 min |

Nines càng cao → redundancy, operations, testing và cost tăng mạnh.

---

# 4. RTO và RPO

## RTO — Recovery Time Objective

Bao lâu hệ thống được phép unavailable sau incident?

```text
RTO = 30 min
```

## RPO — Recovery Point Objective

Có thể mất tối đa bao nhiêu dữ liệu theo thời gian?

```text
RPO = 5 min
```

Nếu business yêu cầu:

```text
RTO < 1 min
RPO ≈ 0
```

thì backup-only DR gần như chắc chắn không đủ. Nhưng điều đó **không tự động có nghĩa active-active multi-region**; phải đánh giá workload, consistency và cost.

---

# 5. Capacity estimation — nguyên tắc

Không cần giả vờ precision.

Mục tiêu:

```text
10 RPS?
1,000 RPS?
100,000 RPS?
```

Architecture khác nhau theo order of magnitude.

Luôn ghi assumptions:

```text
Assumption A1: 5M MAU
Assumption A2: 1M DAU
Assumption A3: 20 reads/user/day
Assumption A4: 2 writes/user/day
Assumption A5: peak = 4 × average
```

---

# 6. RPS calculation

Ví dụ:

```text
DAU = 1,000,000
requests/user/day = 20
```

Total requests/day:

```text
1,000,000 × 20 = 20,000,000 requests/day
```

Average RPS:

```text
20,000,000 / 86,400 ≈ 231 RPS
```

Peak nếu dùng multiplier 5×:

```text
peak ≈ 1,155 RPS
```

Đừng thiết kế theo average nếu traffic có daily peak, campaign hoặc market-open spike.

---

# 7. Read/write ratio

Ví dụ product catalog:

```text
95% read
5% write
```

Peak 20k RPS:

```text
reads  = 19k RPS
writes = 1k RPS
```

Điều này ngay lập tức tạo câu hỏi:

```text
Can DB serve 19k read RPS?
Would CDN/cache remove repeated reads?
Are writes concentrated on hot keys?
```

---

# 8. Bandwidth estimate

Nếu average response = 12 KB và 20k RPS:

```text
12 KB × 20,000 = 240,000 KB/s
≈ 234 MB/s
≈ 1.87 Gbit/s
```

Chưa tính protocol overhead, TLS, replication, retries.

Nếu public static content có thể cache ở edge, bandwidth từ origin có thể giảm rất mạnh.

---

# 9. Storage growth

Ví dụ event ingestion:

```text
5,000 events/s
average payload = 2 KB
```

Raw data/day:

```text
5,000 × 2 KB × 86,400
≈ 864 GB/day
```

30 days raw:

```text
≈ 25.9 TB
```

Sau đó phải cộng:

```text
index overhead
replication
backup
metadata
compression ratio
hot/warm/cold tiers
```

Một “2 KB event” có thể trở thành nhiều hơn đáng kể khi index + replicas được tính.

---

# 10. Concurrency estimate

Little's Law mental model:

```text
concurrency ≈ throughput × time in system
```

Nếu:

```text
1,000 RPS
average latency = 0.2 sec
```

thì roughly:

```text
in-flight ≈ 200 requests
```

Nếu dependency latency tăng từ 200 ms → 2 sec:

```text
in-flight ≈ 2,000
```

Nếu không bound concurrency, latency spike có thể biến thành:

```text
more in-flight
→ more memory/connections
→ more contention
→ higher latency
→ collapse
```

---

# 11. Connection budget

Ví dụ 20 application instances, mỗi instance pool 100 DB connections:

```text
20 × 100 = 2,000 potential DB connections
```

Nếu DB chỉ healthy ở 800 concurrent connections thì autoscaling application có thể **làm DB chết nhanh hơn**.

Scale-out app không đồng nghĩa scale-out dependency.

Architect question:

```text
What shared resource becomes the next bottleneck when I add app instances?
```

---

# 12. Queue capacity estimate

Producer:

```text
arrival = 10,000 msg/s
```

Consumers:

```text
20 workers
400 msg/s/worker
capacity = 8,000 msg/s
```

Backlog growth:

```text
10,000 - 8,000 = 2,000 msg/s
```

Trong 10 phút:

```text
2,000 × 600 = 1.2M messages backlog
```

Nếu message retention hoặc storage không đủ thì queue chỉ trì hoãn failure.

---

# 13. AI-specific capacity

Nếu design AI assistant:

```text
requests/day
prompt tokens/request
completion tokens/request
retrieval queries/request
tool calls/request
model latency
provider RPM/TPM quota
cost/1M tokens
```

Ví dụ:

```text
100k requests/day
4k input tokens/request
500 output tokens/request
```

Daily token volume:

```text
input  = 400M tokens/day
output = 50M tokens/day
```

Architecture phải tính token budget như tính DB IOPS hay bandwidth.

---

# 14. Capacity worksheet

Dùng template:

```text
Users
- MAU:
- DAU:
- peak concurrent:

Traffic
- avg RPS:
- peak RPS:
- read/write:
- payload request:
- payload response:

Data
- writes/day:
- bytes/write:
- storage/day:
- retention:
- replication factor:

Latency
- P50:
- P95:
- P99:
- external dependency deadline:

Reliability
- SLO:
- RTO:
- RPO:

Cost
- compute:
- storage:
- network:
- observability:
- AI/model:
```

---

# 15. Code example — capacity calculator

Không phải production library; chỉ là executable reasoning artifact.

```csharp
public static class Capacity
{
    public static double AverageRps(long dailyUsers, double requestsPerUserPerDay)
        => dailyUsers * requestsPerUserPerDay / 86_400d;

    public static double PeakRps(
        long dailyUsers,
        double requestsPerUserPerDay,
        double peakMultiplier)
        => AverageRps(dailyUsers, requestsPerUserPerDay) * peakMultiplier;

    public static double StorageGbPerDay(double eventsPerSecond, double kbPerEvent)
        => eventsPerSecond * kbPerEvent * 86_400d / 1024d / 1024d;

    public static double ApproxInFlight(double rps, TimeSpan latency)
        => rps * latency.TotalSeconds;
}
```

Test assumptions:

```csharp
Assert.InRange(
    Capacity.PeakRps(1_000_000, 20, 5),
    1_150,
    1_160);
```

---

# 16. Common mistakes

## Mistake — fake precision

```text
peak = 17,392 RPS
```

khi assumptions chỉ là phỏng đoán.

Better:

```text
~20k RPS design target
with 2× safety headroom
```

## Mistake — only traffic, no data growth

10k RPS có thể nhẹ nếu cache hit cao; 100 write RPS có thể nặng nếu mỗi write tạo 10 MB object.

## Mistake — only infrastructure metrics

Business-critical flow có thể fail dù CPU 20%.

## Mistake — no growth horizon

Design cho:

```text
now
6 months
2 years
```

không phải “infinite scale”.

---

# 17. Failure experiment

Cho một service hiện tại:

1. lấy baseline RPS + P95 + DB connections;
2. tăng load 2×;
3. tăng dependency latency 10×;
4. quan sát in-flight requests, memory, pool usage;
5. xác định first saturated resource;
6. viết một ADR: scale app, cache, queue hay optimize DB?

Expected evidence:

```text
load-test report
metrics screenshot
trace sample
capacity sheet
ADR
```

---

# Exit Criteria

Bạn phải có thể:

- biến “hệ thống phải nhanh và scale” thành NFR cụ thể;
- estimate RPS, peak, bandwidth, storage, concurrency và queue backlog;
- phân biệt SLI/SLO/SLA, RTO/RPO;
- giải thích why autoscaling application có thể làm dependency tệ hơn;
- ghi rõ assumptions/safety margin;
- dùng numbers để justify hoặc reject cache/queue/sharding/multi-region.
