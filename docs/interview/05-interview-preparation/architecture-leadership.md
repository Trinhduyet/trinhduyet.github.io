# Architecture & Technical Leadership — 9 câu hỏi có đáp án

<div class="lesson-meta"><span>System Design · ADR · Leadership</span><span>Foundation → Architect</span></div>

## ARCH-01. Monolith, modular monolith hay microservices?

Tách service khi có **ownership, independent scaling/deployment, fault isolation** đủ bù cross-service communication, consistency và operations. Modular monolith có thể tổ chức bounded contexts rõ mà vẫn deploy cùng tiến trình. Shared database/schema trực tiếp giữa services thường làm giảm autonomy.

**Follow-up:** Khi nào tách notification? Khi throughput, release cadence và team ownership đủ độc lập; dùng outbox và versioned contracts.

## ARCH-02. System Design bắt đầu bằng gì?

Requirements, assumptions, NFR (SLO, latency, data durability, RTO/RPO), volume/peaks, authorization và business invariants. Vẽ context → containers → critical request flow → data ownership. Estimate workload trước khi chọn cache, broker hoặc sharding.

**Follow-up:** Người phỏng vấn không cung cấp numbers? Đặt assumptions minh bạch và tính sensitivity; không tuyên bố chúng là production metrics.

## ARCH-03. CAP theorem giúp chọn SQL hay NoSQL?

CAP nói trong điều kiện **network partition**, distributed system phải đánh đổi consistency/availability theo định nghĩa của CAP; không phải một bảng “chọn luôn C/A/P” cho mọi request. Quyết định database còn phụ thuộc query, transaction invariants, latency, ops, data model.

**Follow-up:** PACELC mở rộng suy nghĩ thế nào khi không xảy ra partition?

## ARCH-04. Eventual consistency dùng khi nào?

Phù hợp projections, search index, notifications, analytics có freshness budget. Không áp dụng eventual consistency mù cho authoritative payment debit hoặc ownership authorization. Trả `asOf`, lag/backlog và trạng thái pending để người dùng hiểu độ trễ.

## ARCH-05. Thiết kế notification gửi 10.000 email?

Giả sử yêu cầu dispatch trong 60s → tối thiểu **167 deliveries/s** (chưa retry/provider quota), cần làm rõ accepted vs sent vs delivered. API nhận request, persist idempotency+intent, outbox → durable queue, fan-out bounded workers, provider, DLQ/reconciliation, tenant scope/consent và monitoring.

**Trade-off:** Nếu provider chỉ hỗ trợ 20/s, cần điều chỉnh SLA/provider/capacity; tăng workers một mình không có tác dụng.

## ARCH-06. ADR dùng để làm gì?

ADR chứa Context → Decision Drivers → Alternatives → Decision → Consequences → Verification → Revisit trigger. Mục tiêu lưu lý do và điều kiện đổi quyết định, không dùng ADR để hợp thức hóa công nghệ chọn sẵn.

## ARCH-07. Review một thiết kế do senior khác đề xuất?

Hỏi invariant/constraints, tạo risk list, kiểm tra testability, đo cost/latency, lập alternative nhỏ hơn, ghi quyết định và owner. Không tranh luận “pattern nào enterprise hơn”. Review code cần ưu tiên data correctness, security, regressions rồi mới style.

## ARCH-08. Incident leadership khác debugging cá nhân?

Assign incident lead + technical investigator + communications; mitigate impact, giữ timeline/correlation, kiểm tra rollback/safety, recovery smoke và business data reconciliation, postmortem blameless với preventive actions đo được.

**Follow-up:** Khi nào tạm dừng deploy toàn bộ? Khi blast radius, error budget hoặc data integrity risk vượt threshold đã quy định.

## ARCH-09. Migrate .NET Framework legacy lên .NET hiện đại?

Inventory dependencies, auth contract, DB coupling và performance baseline. Ưu tiên strangler-by-feature, contract tests, dual routing khi cần, expand-contract schema, rollback và observability. Big-bang rewrite có rủi ro mất hidden business rules.

**Bài thực hành:** [3 System Design exercises](../09-solution-architecture-leadership/design-exercises.md). **Đọc:** [Architecture review playbook](../../25-software-architecture/architecture-review-playbook.md).
