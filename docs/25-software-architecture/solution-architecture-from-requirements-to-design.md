# Solution Architecture từ Business Requirements đến HLD/LLD

> [← Module 25](README.md) · [System Design template](system-design-case-study-template.md) · [Delivery & Governance](solution-architecture-delivery-governance.md)

## Kết quả cần đạt

Sau bài này, người học phải có thể nhận một yêu cầu nghiệp vụ chưa rõ ràng, xác định scope, stakeholders, domain boundaries, FR/NFR, rủi ro, các phương án, rồi bàn giao một solution package đủ để Tech Lead triển khai và các nhóm Security/DBA/DevOps/Integration review. Bài này **không gắn với ngành chứng khoán**; ví dụ đặt hàng chỉ để luyện tư duy.

## 1. Discovery: từ mục tiêu kinh doanh đến capability

Một yêu cầu như “xử lý 100.000 đơn mỗi ngày, phản hồi nhanh” chưa phải đặc tả kiến trúc. Bắt đầu bằng sáu nhóm câu hỏi:

| Góc nhìn | Câu hỏi bắt buộc | Artifact |
|---|---|---|
| Business outcome | Chỉ số nào cần cải thiện? Mất tiền khi sai hay khi chậm? | Problem statement, KPI, success metric |
| Actors & journeys | Ai thao tác, quyền gì, happy path và edge case? | User journey, context diagram |
| Capability | Năng lực nào thuộc domain cốt lõi, supporting, generic? | Capability map, bounded contexts |
| Data | Ai sở hữu trạng thái? Dữ liệu nào nhạy cảm? Ai là nguồn sự thật? | Data inventory, ownership map |
| Constraints | Deadline, team skill, budget, vendor, residency, legacy? | Constraint/assumption log |
| External dependency | Đối tác nào, quota, SLA, protocol, settlement/callback? | Integration inventory |

Phân biệt **requirement** (phải đạt), **constraint** (không được vi phạm), **assumption** (tạm cho là đúng, cần xác nhận), **decision** (phương án đã chọn) và **risk** (khả năng thất bại). Không trộn cả năm thành một đoạn “technical design”.

### Ví dụ capability map

Hệ thống thương mại điện tử có Catalog, Cart, Checkout, Inventory, Payment, Fulfillment. Capability map giúp hỏi ai chịu trách nhiệm về giá, dự trữ hàng, thu tiền và hoàn trả. **Bounded context không tự động bằng microservice**: cùng một deployable vẫn có thể giữ module ownership và interface rõ.

## 2. Functional Requirements: viết để test được

Một FR tốt có ID, actor, trigger, precondition, input/output, invariant, exception flow và acceptance criteria.

| ID | FR ví dụ | Acceptance criteria |
|---|---|---|
| FR-01 | Khách tạo đơn từ giỏ hàng | Request hợp lệ có một logical order; retry cùng key không tạo thêm đơn |
| FR-02 | Khách thanh toán | Trạng thái Pending/Confirmed/Rejected/Unknown rõ; timeout không tự coi là thất bại |
| FR-03 | Khách hủy đơn | Chỉ hủy khi trạng thái cho phép; nếu payment đã thực hiện thì cần quy trình hoàn/đối soát |
| FR-04 | Operator kiểm tra đơn bị treo | Có search theo business ID, audit, reconciliation action và quyền truy cập phù hợp |

Viết thêm **negative requirements**: dữ liệu của tenant A không được lộ sang B; không được thu tiền hai lần; không được thông báo “đã hoàn tất” chỉ dựa vào message đã được broker nhận.

## 3. NFR: đo được và có owner

| Attribute | Câu hỏi định lượng | Verification |
|---|---|---|
| Latency | p95/p99 tại peak RPS, kích thước payload và concurrency nào? | Load test + trace waterfall |
| Throughput | Peak/burst duration, write/read ratio và backlog được chấp nhận? | Soak/stress test, queue age |
| Availability | SLO per critical journey theo cửa sổ thời gian? Planned downtime tính ra sao? | SLI dashboard, error budget |
| Correctness | Bất biến nào tuyệt đối không được phá? Eventual consistency tối đa bao lâu? | Database integration, race/replay tests |
| Security | Authentication, authz theo resource/tenant, data classification? | Threat model, negative tests |
| Durability/DR | RPO/RTO theo capability, không phải một số chung? | Backup restore + failover drill |
| Maintainability | Release frequency, rollback target, blast radius, owner? | Change lead time, architecture fitness |
| Cost | Budget, cost per transaction/user/tenant và headroom? | Cost model, benchmark, monitoring |

Không lấy SLA của cloud provider thay thế SLO của end-to-end business journey. NFR có thể mâu thuẫn: p99 thấp + strong consistency toàn khu vực + chi phí nhỏ; nhiệm vụ của Architect là trình bày trade-off và xin quyết định business.

## 4. Capacity estimation từ workload

Ví dụ **giả định**, không phải capacity thực tế:

- 100.000 orders/day → trung bình `100000 / 86400 ≈ 1.16 orders/s`.
- Peak factor 25 → khoảng `29 orders/s` trong giờ cao điểm.
- Mỗi order phát 4 event, mỗi event 2 KiB → khoảng `232 KiB/s` ở peak trước replication/protocol overhead.
- Nếu worker xử lý 20 events/s nhưng đầu vào có 116 events/s, backlog tăng `96 events/s`; 15 phút tạo khoảng `86.400 events` chưa xử lý.
- Dùng Little's Law `L = λW` để kiểm tra concurrency trung bình; không xem nó là công thức tự quyết định số pod.

Trước khi chọn VM/pod/shard, xác nhận traffic shape, DB IOPS, SQL lock contention, downstream quota, cache skew, retry amplification, retention và cost. Viết rõ **assumptions**, **safety margin** và **revisit trigger**.

## 5. HLD: thể hiện các quyết định chính

Một bộ HLD tốt cần các góc nhìn khác nhau:

1. **System context:** actors, hệ thống nội bộ/ngoài, trust boundaries.
2. **Capability/container:** trách nhiệm, data ownership, contract, sync/async edges.
3. **Deployment:** regions/zones, network, secret/identity, storage, broker, hybrid connectivity.
4. **Critical sequence:** transaction commit, outbox publish, webhook, retry, timeout, compensating action.
5. **Failure model:** dependency down, duplicate, reordered event, network partition, DB failover và degraded mode.

Mỗi mũi tên cần protocol, direction, timeout, retry semantics, ownership, authentication và versioning. Một hộp “Kafka/Redis/API Gateway” không giải thích được lý do nó tồn tại.

### Cách chọn architecture style

| Phương án | Chọn khi | Cần thận trọng khi |
|---|---|---|
| Modular monolith | Team nhỏ/vừa, domain còn biến động, giao dịch local quan trọng | Module boundary yếu, mọi thay đổi đụng shared schema |
| Microservices | Deployment/scale/ownership độc lập có giá trị rõ | Team chưa có observability, contract testing, operational maturity |
| Event-driven | Fan-out, decouple time, tolerable eventual consistency | Ordering, replay, duplicate/external side effect chưa được thiết kế |
| CQRS read model | Read/write workloads khác biệt đáng kể | Chưa có nhu cầu, không chịu nổi projection lag |
| Batch/stream | Data volume/time semantics justify | Không có checkpoint/replay/late-arrival strategy |

## 6. LLD: hợp đồng triển khai được

LLD phải bổ sung những chi tiết HLD không nên chứa:

- OpenAPI endpoint, request/response DTO, status code, error taxonomy, pagination, idempotency, rate limit.
- Event schema, version, partition/order key, delivery semantics, retention, DLQ/replay policy.
- Entity/aggregate/state machine, data invariants, precision, unique/index/check constraints.
- Transaction boundary, isolation, optimistic/pessimistic concurrency, retries và deadlock handling.
- Authz ở API, worker, database boundary; secret/config lifecycle; audit.
- Observability naming, correlation IDs, metrics, alerts, on-call/runbook.
- Deployment/migration/rollback plan với backward compatibility giữa hai phiên bản đồng thời.

Ví dụ `POST /orders` không nên trả `200 completed` khi payment provider chưa xác nhận. Có thể trả `202 Accepted` + `operationId` và endpoint truy vấn trạng thái; cần định nghĩa SLA và eventual consistency cho client.

## 7. Traceability và review đa bên

| Requirement | Decision | Component/contract | Test/evidence | Reviewer |
|---|---|---|---|---|
| No duplicate order | Stable idempotency + unique constraint | API + Orders DB | 100 concurrent same-key requests | Tech Lead, DBA |
| Provider timeout ambiguity | Unknown state + reconciliation | Payment adapter + workflow | Fault injection after provider commit | Integration, Product |
| Sensitive account access | Resource-level authorization | API + worker + query | Cross-tenant negative test | Security |
| Recover within target RTO | Restore and replay runbook | DB, outbox, broker | Timed DR drill | DevOps, DBA |

Ghi review decision thành **approved / conditionally approved / rejected / experiment required**, có owner, due date, evidence và exception expiry. Architect không tự phê duyệt thay chủ sở hữu rủi ro.

## 8. Exercise: một bài từ đầu đến cuối

**Đề bài:** Xây dựng platform đặt lịch có 50.000 bookings/day, một slot không được bán trùng, nhận payment qua provider có timeout và webhook.

Deliverables:

1. Capability map + 8 FR + 8 NFR với số liệu/assumptions rõ.
2. Hai HLD (modular monolith và split booking/payment), ADR so sánh.
3. API table, ERD logic, seat/slot state machine, transaction boundary.
4. Sequence happy/timeout/late webhook/duplicate/recovery.
5. Capacity/cost spreadsheet và risk register.
6. Test plan: concurrency, replay, authz, load, failover, restore.

**Pass:** reviewer có thể truy ngược từ một NFR/FR đến quyết định, contract, test và owner; người học giải thích được khi nào *không* cần broker/microservices. **Không pass:** chỉ vẽ sơ đồ mà không có invariant, failure path hoặc measurable evidence.
