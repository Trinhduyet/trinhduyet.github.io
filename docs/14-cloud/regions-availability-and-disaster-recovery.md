# Regions, Availability và Disaster Recovery

> [← Module 14 overview](README.md) · [Cloud references](references.md)

## Mục tiêu / Learning Objectives

- giải thích regions, availability và disaster recovery bằng mental model và boundary;
- implement một minimal path có bound, invariant và observable output;
- phân tích failure, security, performance, reliability và operational ownership;
- đối chiếu behavior với nguồn chính thức thay vì dựa vào folklore;
- viết decision note và biết trigger để chuyển sang alternative.

## Tại sao cần học? / Why It Matters

AZ/region failure, replication, RTO/RPO, backup/restore và failover. Đây là boundary nơi một quyết định nhỏ có thể đổi correctness, latency, security và operational ownership.

## Tổng quan / Overview

~~~mermaid
flowchart LR
    A["Input / workload"] --> B["Regions, Availability và Disaster Recovery"] --> C["State / result"] --> D["Evidence / decision"]
~~~

## Mental Model

| Boundary | Câu hỏi | Evidence |
| --- | --- | --- |
| Input | Dữ liệu/traffic đến từ đâu và bound nào? | Contract, validation, limit |
| Core | Invariant/state transition nào phải đúng? | Test, query/plan, policy |
| Resource | CPU, memory, network, storage, quota nào tiêu thụ? | Metrics, profile, capacity |
| Recovery | Khi dependency/change fail thì ai xử lý? | Retry, rollback, runbook |

AZ/region failure, replication, RTO/RPO, backup/restore và failover. Học stable concept trước version/tool syntax; mọi claim production phải có measurement hoặc source.

## Thuật ngữ / Terminology

| Thuật ngữ | Ý nghĩa |
| --- | --- |
| Contract | Cam kết giữa producer và consumer |
| Version | Identity của behavior/schema |
| Compatibility | Cũ và mới cùng hoạt động |
| Deadline | Thời gian còn lại cho operation |
| Replay | Thực hiện lại có kiểm soát |
| Audit | Record cho điều tra |

## Prerequisites

- [Module 13 prerequisite](../13-devops-iac/README.md).
- [Roadmap dependency graph](../00-roadmap/prerequisites.md).
- Có thể ghi failure hypothesis và output reproducible.

## How It Works

Bắt đầu từ requirement, chọn primitive, đặt safety bound, quan sát behavior và xác định owner. AZ/region failure, replication, RTO/RPO, backup/restore và failover. Không coi framework/platform abstraction là proof của correctness.

## Minimal Example

~~~text
RPO/RTO -> backup -> restore rehearsal -> failover
~~~

Minimal example chỉ chứng minh shape; production cần validation, cancellation/timeout, migration, security và test tùy boundary.

## Production Example

AZ/region failure, replication, RTO/RPO, backup/restore và failover. Production path bổ sung contract test, structured telemetry, failure classification, rollout/rollback và data/privacy policy.

~~~text
decision = requirement + workload + failure + security + cost
evidence = implementation + test + measurement + runbook
~~~

## .NET Integration

- DI/configuration/host composition giữ lifetime và ownership rõ.
- Cancellation, timeout và disposal phải đi xuyên boundary; không fire-and-forget vô chủ.
- HTTP/API layer map lỗi thành contract ổn định, không leak exception nội bộ.
- Persistence/cache/queue adapter không che transaction, consistency hoặc retry semantics.
- Metrics/traces/logs dùng low-cardinality labels và retention/privacy policy.

## Internals

Đọc access path/state machine/controller/plan theo đúng module để giải thích observed behavior. Provider, runtime, platform và version có thể thay đổi implementation detail; giữ normative claim ở official docs.

## Common Mistakes

- retry hoặc scale trước khi phân loại failure.
- coi timeout là rollback.
- bỏ qua tenant/secret/audit boundary.
- dùng cache/queue/platform như source of truth mặc định.
- không test partial failure.

## Performance Considerations

Đo workload representative với warm-up, concurrency, payload mix và tail latency. Bound state/queue/cache trước khi micro-optimize; so sánh before/after cùng environment và tính cả cost của measurement.

## Security Considerations

Threat model asset, identity, trust boundary, input abuse, secret handling và artifact access. Least privilege, data minimization, encryption, audit và expiry phải có negative test.

## Reliability / Failure Modes

| Failure | Signal | Response |
| --- | --- | --- |
| Invalid input/state | 4xx, constraint/test failure | Reject rõ, không partial side effect |
| Dependency slow/unavailable | Timeout, queue/latency tăng | Deadline, bounded retry, fallback hoặc shed |
| Capacity exhausted | CPU/memory/quota/429 | Backpressure, scale, degrade hoặc stop |
| Change incompatible | Error/contract drift | Canary, migration, rollback/forward fix |
| Operator mistake | Audit/event anomaly | Least privilege, approval, runbook |

## Observability

Ghi success/error rate, latency percentile, resource usage, state transitions và version/deployment. Trace nối request → core operation → dependency; log structured theo operation ID. Alert theo SLO/error budget.

## Operational Considerations

- Pin tool/provider/image/schema version phù hợp.
- Readiness không báo healthy trước invariant cần thiết.
- Runbook có preflight, read-only command, rollback và artifact retention.
- Rehearse backup/restore, key rotation, failover, drift hoặc upgrade tùy module.
- Manual exception có owner, expiry và post-incident review.

## Architect Perspective

Regions, Availability và Disaster Recovery trở thành architectural boundary khi ảnh hưởng ownership, consistency, deployment, capacity hoặc team topology. Chọn phương án đơn giản nhất thỏa NFR; document điều gì đổi ở 10x/100x và trigger migrate.

## Trade-offs

| Lựa chọn | Lợi ích | Chi phí/rủi ro |
| --- | --- | --- |
| Simple/local | Dễ hiểu, ít toil | Giới hạn scale/durability |
| Specialized/distributed | Capacity/feature tốt | Coupling, failure và vận hành |
| Managed/platform | Giảm control-plane toil | Quota, lock-in và cost |

## When NOT to Use It

- Không dùng pattern này nếu requirement chưa chứng minh need hoặc không có owner vận hành.
- Không dùng abstraction để che failure/latency/consistency semantics.
- Không chọn managed/distributed option chỉ vì production-ready mà thiếu cost/capacity evidence.
- Không mở rộng privilege, retention hoặc data exposure để làm lab nhanh hơn.
- Không tối ưu một metric nếu làm hỏng SLO, security hoặc rollback.

## Alternatives

- Giữ local/simple implementation khi scale và durability chưa yêu cầu.
- Dùng managed service khi team không muốn sở hữu control plane và cost hợp lý.
- Dùng queue/batch/stream hoặc synchronous path tùy latency/durability.
- Dùng immutable artifact/configuration và migration thay manual mutation.
- Dùng standard protocol/contract trước custom framework.

## Review Questions

1. Invariant nào phải đúng dù request/retry/deploy lặp lại?
2. Boundary nào sở hữu state, timeout, cleanup hoặc rollback?
3. Evidence nào chứng minh bottleneck/security/reliability claim?
4. Điều gì sẽ hỏng khi dependency chậm hoặc state stale?
5. Cost và operational toil tăng theo scale nào?
6. Khi nào phương án đơn giản hơn là lựa chọn tốt hơn?

## Hands-on Lab

Tạo một experiment bounded cho regions, availability và disaster recovery: ghi workload, expected output, failure scenario và safety bound; chạy baseline rồi so sánh; lưu decision note. Không đưa credential, production data hoặc diagnostic artifact nhạy cảm vào repository.

## Exit Criteria

- Giải thích được az/region failure, replication, rto/rpo, backup/restore và failover..
- Implement minimal example có validation/bound phù hợp.
- Mô tả failure, security, performance và operational response.
- Có evidence reproducible và decision note.
- Biết dependency tiếp theo và trigger cần nghiên cứu thêm.

## Related Topics

- [Module 13 prerequisite](../13-devops-iac/README.md).
- [Cloud Primitives, Identity và Networking](cloud-primitives-identity-and-networking.md).
- [Regions, Availability và Disaster Recovery](regions-availability-and-disaster-recovery.md).
- [Cloud Cost Governance và Operations](cloud-cost-governance-and-operations.md).
- Module 15 — Kubernetes khi content được mở.

## Official English Sources

- [Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/).
- [Azure reliability](https://learn.microsoft.com/en-us/azure/reliability/).
- [Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/what-is-well-architected-framework).
- [Azure identity architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/identity).
- [Cloud design patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/).

## Vietnamese Resources

- Dùng [glossary](../00-roadmap/glossary.md) để giữ canonical English term.
- Viết reflection bằng tiếng Việt nhưng giữ tên API/protocol/metric chính xác.
- Tuân thủ [source policy](../00-roadmap/source-policy.md) cho claim version-sensitive.

## Verification Metadata

- Verified: 2026-08-11.
- Technology version: Cloud content v1; refresh version-sensitive behavior before production.
- Context7 queries used: none; callable tool unavailable in this run.
- Notes: content v1 không thay thế learner evidence; cần lab/review/production artifact để nâng level.
