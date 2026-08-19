# Azure Reliability, Observability, Governance & Cost

> [← Compute, Data & Messaging](azure-compute-data-messaging-and-integration.md) · [Cloud & Azure](README.md)

<div class="lesson-meta">
  <span><strong>Framework</strong>&nbsp;Azure Well-Architected</span>
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Focus</strong>&nbsp;Reliability · Security · Cost · Operations · Performance</span>
</div>

## 1. Reliability không phải “deploy 2 instances”

Reliability phải bắt đầu từ target:

```text
User outcome
   ↓
SLI
   ↓
SLO / availability target
   ↓
Failure model
   ↓
Redundancy + recovery design
   ↓
Test / drill / evidence
```

Ví dụ:

```text
Checkout success SLO = 99.95% / month
P95 latency           < 800 ms
RTO                    30 min
RPO                     5 min
```

Nếu không có target, “multi-zone” hay “multi-region” chỉ là feature selection, chưa phải reliability design.

## 2. Availability Zone vs Region

### Zone failure

Một region có thể có nhiều availability zones ở những nơi hỗ trợ. Zone-aware architecture nhằm giảm impact khi một datacenter/failure domain trong region gặp sự cố.

Mental model:

```text
Region
├── Zone 1
├── Zone 2
└── Zone 3
```

Service support và zone behavior khác nhau theo Azure service/region/SKU nên phải verify official docs lúc design.

### Region failure

Region outage là failure class lớn hơn:

```text
regional compute
+ data plane
+ dependent services
+ networking
```

Multi-region design phải trả lời:

```text
active-passive or active-active?
where is write authority?
how is data replicated?
what consistency is lost during failover?
who triggers failover?
how is failback done?
```

Không chỉ:

```text
deploy same app to Region B
```

## 3. RTO và RPO

### RTO — Recovery Time Objective

Bao lâu hệ thống được phép gián đoạn trước khi business impact vượt ngưỡng chấp nhận.

### RPO — Recovery Point Objective

Mức mất dữ liệu theo thời gian mà business chấp nhận được sau recovery.

Ví dụ:

```text
RTO = 30 minutes
RPO = 5 minutes
```

Không được suy ra:

```text
backup every 5 min
→ automatically RPO 5 min
```

vì restore path, replication lag, corruption detection và usable recovery point đều ảnh hưởng outcome thật.

## 4. Multi-region trade-off

### Active-passive

```text
Region A active
Region B warm/standby
```

Ưu:
- simpler write authority;
- thường dễ reasoning consistency hơn active-active.

Nhược:
- failover time;
- standby cost;
- failover path dễ “untested”.

### Active-active

```text
Region A serves traffic
Region B serves traffic
```

Ưu:
- latency/global availability opportunity.

Nhược:
- data conflict/consistency complexity;
- routing/failback complexity;
- higher operational maturity and cost.

Một requirement `99.9%` có thể không justify active-active multi-region.

## 5. Azure Well-Architected — 5 pillars

Architecture review nên cân bằng:

```text
Reliability
Security
Cost Optimization
Operational Excellence
Performance Efficiency
```

Một decision có thể tốt cho pillar này nhưng xấu cho pillar khác.

Ví dụ:

```text
3 regions active-active
+ reliability potential
+ global latency
- cost
- data consistency complexity
- deployment complexity
- incident surface
```

Architect phải ghi trade-off, không tối ưu một pillar cô lập.

## 6. Observability — user outcome trước resource metric

CPU 40% không nói checkout đang tốt.

Nên bắt đầu:

```text
Business SLI
- checkout success rate
- payment unknown rate
- notification delivery delay

Service SLI
- API P95/P99
- DB dependency latency
- queue age/backlog
- error rate

Resource indicators
- CPU
- memory
- connections
- RU / DTU / vCore / throttling
```

Azure Monitor / Application Insights / Log Analytics cung cấp telemetry platform, nhưng **instrumentation model và alert semantics vẫn là việc của engineering team**.

## 7. Distributed tracing

Request xuyên nhiều service:

```text
Client
→ API Management
→ Checkout API
→ SQL
→ Service Bus
→ Payment Worker
→ Provider
```

Nếu không propagate correlation/trace context, incident team chỉ thấy nhiều log rời rạc.

Trace cần giúp trả lời:

```text
request nào chậm?
dependency nào chậm?
retry xảy ra bao nhiêu lần?
message nào redelivered?
unknown payment được reconcile lúc nào?
```

Không log token/secret/PII chỉ để “trace đủ”.

## 8. Queue backlog là reliability signal

Queue consumer vẫn “green” nhưng backlog tăng:

```text
arrival rate = 10k/min
processing   = 7k/min

backlog growth = 3k/min
```

Sau 30 phút:

```text
90k messages waiting
```

User-facing notification SLO có thể fail dù API vẫn 200 OK.

Các SLI hữu ích:

```text
queue depth
oldest message age
processing rate
failure/retry rate
DLQ growth
```

Autoscale phải dựa trên metric đúng với bottleneck, không mặc định CPU.

## 9. Deployment reliability

Nhiều outage không do Azure region mà do deploy.

Design delivery cần:

```text
versioned artifact
progressive rollout
health/evidence gate
rollback path
database compatibility
feature flag where appropriate
```

### Database migration trap

Release A:

```text
ALTER TABLE DROP COLUMN OldField
```

trong khi old instances vẫn đọc `OldField`.

Rolling deploy:

```text
old + new app versions coexist
```

Schema change phải backward/forward compatible theo deployment strategy.

## 10. Cost là NFR

Không để cost review sau khi design xong.

Các câu hỏi:

```text
cost/request?
cost/tenant?
cost/order?
cost/GB retained?
cost when idle?
cost at peak?
regional egress?
log retention?
standby DR cost?
```

### Example

Nếu API chỉ có 20 RPS peak, một AKS platform lớn + premium multi-region data layer có thể giải quyết problem không tồn tại.

“Cloud native” không đồng nghĩa “economically correct”.

## 11. Cost traps

### Over-retained logs

```text
high-cardinality verbose logs
× many services
× 90-day retention
→ observability bill grows faster than app compute
```

### Cross-region transfer

Multi-region chatty service calls hoặc replication tạo data transfer cost + latency.

### Idle capacity

Always-on premium capacity cho workload ít traffic có thể không hợp unit economics.

### Wrong cache assumption

Thêm Redis để giảm DB cost nhưng cache hit ratio thấp + cluster cost cao → tổng cost tăng.

Phải đo.

## 12. Governance trong production

Governance controls cần được automate:

```text
Azure Policy
RBAC
resource locks where appropriate
budgets/alerts
tags
Defender/security baseline
diagnostic settings
IaC validation
```

Nhưng guardrail cũng cần lifecycle:

```text
propose
→ test
→ canary scope
→ observe
→ wider rollout
```

Một `deny` policy ở parent management group có thể break hàng trăm pipelines nếu rollout kém.

## 13. Disaster Recovery runbook

Một DR plan chỉ có diagram là chưa đủ.

Phải có:

```text
Detection
→ declare incident
→ choose recovery point
→ restore/failover
→ validate data
→ validate critical flows
→ reopen traffic
→ monitor
→ reconcile gaps
→ failback plan
```

Evidence:

```text
last restore test date
actual RTO
actual RPO/data loss
issues found
runbook update
```

## 14. Failure drills nên có

- kill one compute instance;
- block dependency/network path;
- queue backlog spike;
- DB throttling;
- cache unavailable;
- expired secret/certificate;
- zone dependency failure where testable;
- restore database backup to isolated environment;
- simulate region failover at architecture/test level;
- rollback bad deployment;
- replay DLQ messages safely.

## 15. Architecture review matrix

| Dimension | Question |
|---|---|
| Reliability | SLO/RTO/RPO có số và test chưa? |
| Security | identity/network/data boundary rõ chưa? |
| Performance | capacity bottleneck nào được đo? |
| Cost | unit economics + idle/peak cost? |
| Operations | deploy/observe/recover ai làm, bằng runbook nào? |
| Data | source of truth/backup/restore/reconciliation? |
| Messaging | backlog/DLQ/duplicate semantics? |
| Region | failure scope nào thực sự cần survive? |

<div class="key-takeaway" markdown>
<strong>Key takeaway</strong>

Reliability trên Azure không phải checklist service. Nó là **target → failure model → design → operational evidence**. Nếu chưa restore/test/fail/reconcile được thì DR vẫn là giả định.
</div>

## Tiếp theo

→ [Azure .NET Checkout Reference Architecture](azure-dotnet-reference-architecture.md)
