# Azure Production Handbook — Hosting & Compute

> [← Azure overview](README.md) · [Data & Messaging →](azure-production-handbook-data-messaging.md)

<div class="lesson-meta">
  <span><strong>Focus</strong>&nbsp;App Service · Functions · Container Apps · VM</span>
  <span><strong>Goal</strong>&nbsp;choose + configure + scale + estimate cost</span>
</div>

Guide này trả lời câu hỏi thực tế: **backend chạy ở đâu, chọn tier nào, scale như thế nào, networking/identity ra sao và bill tăng theo biến số nào?**

AKS không được dạy ở đây. Nếu requirement thực sự cần Kubernetes API/platform controls, chuyển sang [Module 15](../15-kubernetes/README.md).

---

# 1. Compute decision table

| Workload | Candidate đầu tiên | Lý do |
|---|---|---|
| HTTP API/web app tiêu chuẩn | App Service | PaaS đơn giản, deployment/scale/health managed |
| Containerized API/worker, revisions, event autoscale | Container Apps | container PaaS, không sở hữu Kubernetes cluster |
| Trigger/event/timer execution | Functions | serverless/event model |
| Legacy/custom OS/software | Virtual Machine / VMSS | OS-level control |
| Kubernetes-specific platform requirement | AKS | direct Kubernetes API/ecosystem; học ở Module 15 |

Decision phải xét thêm:

```text
traffic shape
stateful/stateless
startup time
runtime requirements
network isolation
scale-to-zero need
minimum always-on capacity
zone/region requirements
team operating capability
cost at idle + cost at peak
```

---

# 2. Azure App Service

## Problem solved

Managed hosting cho web/API mà team không muốn quản lý VM/container orchestrator trực tiếp.

## Pricing/hosting model

App Service app chạy trong **App Service Plan**. Plan quyết định OS, region, VM size, instance count và pricing tier.

Current tier families cần biết:

```text
Free / Shared         → learning/basic shared compute
Basic                 → dedicated entry level
Standard              → production features cơ bản
Premium v2/v3/v4      → higher performance/features
Isolated v2           → network + compute isolation scenarios
```

Không suy luận:

```text
5 apps = 5 lần compute bill
```

Nếu nhiều apps share cùng một App Service Plan, chúng share plan capacity và plan billing.

Official: <https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans>

## Minimal production configuration

Checklist:

```text
[ ] Linux/Windows choice có reason
[ ] Plan tier phù hợp feature + capacity
[ ] >= 2 instances nếu availability target cần instance redundancy
[ ] Health Check endpoint
[ ] Always On khi plan/runtime cần
[ ] Deployment slot nếu dùng slot-based rollout
[ ] Managed Identity
[ ] HTTPS only / TLS policy
[ ] app settings không chứa secret nếu dùng identity được
[ ] VNet Integration nếu outbound private path cần
[ ] Private Endpoint / access restriction nếu inbound private requirement
[ ] diagnostic settings / Application Insights
[ ] autoscale rule có min/max guardrail
```

## Scale

Scale up:

```text
change App Service Plan tier / worker size
```

Scale out:

```text
increase plan instances
```

Autoscale input không nên chỉ CPU. Có thể cần:

```text
CPU
memory/application pressure
HTTP queue/latency
request rate
business backlog
```

## Deployment slots

Slot phù hợp cho:

```text
staging deploy
warm-up
smoke test
controlled swap
rollback by slot strategy
```

Nhưng slot swap không rollback database migration hoặc external side effects.

## Networking

```text
Inbound
Internet → App Service public endpoint
or Private Endpoint / access restriction where required

Outbound
App Service → VNet Integration → private dependencies / controlled egress
```

VNet Integration và Private Endpoint giải quyết hai hướng traffic khác nhau; không coi chúng là interchangeable.

## Cost drivers

```text
App Service Plan tier
× worker size
× instance count
× running time
+ networking/private connectivity where applicable
+ observability
+ backup/storage extras
```

Cost traps:

- nhiều plan ít utilization thay vì consolidate hợp lý;
- autoscale minimum quá cao;
- Premium plan cho workload nhỏ không cần feature;
- verbose telemetry lớn hơn compute bill;
- staging slots/workloads giữ capacity nhưng không được review.

## Failure drills

- force one instance unhealthy, verify traffic removal;
- deploy bad version to slot;
- test VNet/DNS dependency failure;
- rotate external secret;
- hit autoscale max and observe saturation behavior.

---

# 3. Azure Functions

## Problem solved

Event/trigger-based compute cho timer, queue, HTTP, storage event và background processing.

## Hosting choices

Azure Functions có nhiều hosting options; với serverless workloads mới, **Flex Consumption** là option cần đánh giá đầu tiên. Premium/Dedicated phù hợp khi workload cần always-warm, predictable capacity hoặc integration requirements khác.

Mental model:

```text
Flex Consumption
→ serverless + per-function scaling + VNet support + execution-based model

Premium
→ prewarmed/always-ready capacity + advanced hosting needs

Dedicated/App Service
→ Functions chạy trên allocated App Service capacity
```

Official: <https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan>

## Minimal production configuration

```text
[ ] trigger semantics rõ
[ ] timeout/execution duration phù hợp plan
[ ] concurrency được đo
[ ] retry policy có bound
[ ] poison/DLQ path
[ ] idempotency cho at-least-once event/message
[ ] Managed Identity
[ ] VNet integration nếu dependency private
[ ] Application Insights sampling/cost control
[ ] host/function settings versioned
```

## Event correctness

Function retry không đồng nghĩa business operation safe.

```text
message redelivery
→ function executes again
→ side effect must be idempotent/deduplicated
```

Ví dụ payment:

```text
timeout != payment failed
```

Function phải persist state/reconcile thay vì retry bằng business identity mới.

## Cost drivers

Tùy hosting plan, nhưng cần model:

```text
executions
+ execution duration
+ allocated/used memory/compute
+ always-ready/prewarmed instances when configured
+ storage account
+ network
+ logs/Application Insights
```

Cost traps:

- function nhỏ nhưng log quá nhiều;
- retry storm làm execution bill tăng;
- high concurrency overload database/external API;
- always-ready capacity set cao hơn nhu cầu.

---

# 4. Azure Container Apps

## Problem solved

Managed container platform khi cần container image, revisions, ingress, jobs, autoscale/KEDA-like scaling nhưng không cần direct Kubernetes API/control plane.

## Plan model

Container Apps environments hiện dùng workload profiles; hai billing models chính:

```text
Consumption
→ app consumes resources while active/idle according to platform model
→ can scale to zero

Dedicated
→ allocated workload-profile instances
→ useful for steady/higher utilization or isolation/custom compute
```

Official:

- <https://learn.microsoft.com/en-us/azure/container-apps/structure>
- <https://learn.microsoft.com/en-us/azure/container-apps/billing>

## Minimal production configuration

```text
[ ] workload profile decision documented
[ ] CPU/memory per replica measured
[ ] min/max replicas
[ ] HTTP/event scaling rule
[ ] ingress internal/external chosen intentionally
[ ] revision mode strategy
[ ] health probes
[ ] Managed Identity
[ ] ACR pull identity
[ ] VNet/private endpoints as required
[ ] secret references minimized
[ ] Log Analytics/OpenTelemetry plan
```

## Revisions

Use revisions để reason release:

```text
new image
→ new revision
→ health/smoke
→ traffic shift
→ old revision retained/deactivated by policy
```

Database compatibility vẫn cần expand/contract.

## Jobs

Container Apps Jobs phù hợp cho:

```text
scheduled batch
manual job
queue/event-driven bounded job
```

Không biến long-running service thành job nếu lifecycle semantics không phù hợp.

## Scaling

```text
HTTP concurrency / request signal
queue depth / external scaler
→ replicas
```

Bound:

```text
min replicas
max replicas
external dependency quota
DB connection capacity
```

Autoscale tăng compute nhanh nhưng database/provider quota không tự tăng.

## Cost drivers

Consumption:

```text
resource usage
+ requests / executions as applicable
+ idle resource behavior
+ private endpoint / environment management extras
+ network
+ logs
```

Dedicated:

```text
workload profile instance type
× allocated instances/time
+ environment management charges where applicable
+ network/logs
```

Cost traps:

- dedicated profile low utilization;
- minimum replicas cao ở nhiều apps;
- scale rule phản ứng với metric nhiễu;
- environment/private networking fixed costs bị bỏ quên;
- logs của containers tăng không giới hạn.

---

# 5. Virtual Machines / VM Scale Sets

## Khi nào cần VM

Strong signals:

```text
custom OS configuration
legacy software/runtime
special kernel/driver requirement
vendor appliance
software không phù hợp PaaS/container
full machine-level control
```

Weak signal:

```text
"VM dễ hiểu hơn"
```

VM chuyển patching, hardening, backup, capacity, image lifecycle và incident surface về team.

## Production configuration

```text
[ ] image source/version
[ ] VM size based on measured workload
[ ] managed disks SKU/size/IOPS
[ ] Availability Zone / VMSS design if required
[ ] no public RDP/SSH by default
[ ] Bastion/JIT/controlled admin access
[ ] Managed Identity
[ ] NSG + route + outbound design
[ ] patch strategy
[ ] backup policy
[ ] monitoring/guest agent
[ ] autoscale if VMSS
```

## Disk choice

VM performance có thể bottleneck bởi disk, không chỉ CPU.

Track:

```text
IOPS
throughput
latency
queue depth
burst behavior
```

## Cost drivers

```text
VM size × running hours
+ OS/software licensing
+ managed disks / snapshots
+ public IP / LB / NAT / Firewall
+ backup
+ network egress
+ monitoring
```

Optimization candidates:

```text
right-size
shutdown non-prod
autoscale VMSS
reservations / savings plans when usage is stable
Azure Hybrid Benefit when eligible
```

Không mua reservation trước khi workload ổn định và ownership rõ.

---

# 6. AKS — chỉ là compute decision ở Module 14

Trong Azure architecture review, AKS card chỉ cần:

```text
Requirement:
need Kubernetes API/ecosystem/control?

Cost model:
cluster management tier
+ node pools
+ disks/network/LB/NAT
+ ACR
+ observability
+ security/backup

Operating model:
platform team owns Kubernetes day-2 operations
```

Chi tiết Kubernetes/AKS production nằm tại:

→ [Module 15 — Kubernetes](../15-kubernetes/README.md)

---

# 7. Compute cost worksheet

Cho mỗi environment:

| Input | Example placeholder |
|---|---:|
| region | `<region>` |
| hours/month | 730 |
| average instances | `<n>` |
| peak instances | `<n>` |
| CPU/memory per instance | `<size>` |
| idle minimum | `<n>` |
| expected egress GB | `<GB>` |
| logs GB/day | `<GB>` |
| DR/secondary capacity | `<none/warm/full>` |

Sau đó tính:

```text
monthly compute estimate
=
allocated/base compute
+ burst/runtime compute
+ networking
+ telemetry
+ backup/DR
```

Không compare service chỉ bằng headline compute price. Một service rẻ hơn về CPU có thể đắt hơn khi cộng fixed gateway/network/log/platform costs hoặc team operating toil.

---

# 8. Review checklist

- [ ] Workload characteristics được viết trước service name.
- [ ] Chọn App Service/Functions/Container Apps/VM/AKS có ADR.
- [ ] Tier/SKU/plan được ghi cụ thể.
- [ ] Min/max scale có guardrail.
- [ ] Dependency capacity được tính cùng autoscale.
- [ ] Managed Identity được ưu tiên.
- [ ] Public/private path rõ.
- [ ] Health semantics đúng.
- [ ] Rollout không giả định DB rollback.
- [ ] Cost at idle và cost at peak đều được estimate.
- [ ] Logs/network/backup được tính trong cost.
- [ ] Failure drill có evidence.

## Verification metadata

- Verified: 2026-08-28.
- App Service plan source: Microsoft Learn current hosting plan documentation.
- Functions serverless baseline: Flex Consumption current guidance.
- Container Apps: workload profiles / Consumption / Dedicated current documentation.
- AKS Kubernetes knowledge intentionally lives in Module 15.
