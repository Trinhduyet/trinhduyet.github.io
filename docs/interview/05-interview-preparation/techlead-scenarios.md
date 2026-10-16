# Technical Lead .NET — 10 tình huống phỏng vấn và cách trả lời tiếng Việt

<div class="lesson-meta"><span>Hiring Manager · Tech Lead</span><span>Decision & Delivery</span><span>10 lời giải tình huống</span></div>

> **Mục tiêu:** trả lời theo cách người chịu trách nhiệm giải pháp và tiến độ, **không** chỉ nêu design patterns. Tất cả tình huống dưới đây là mô phỏng để luyện, không phải chuyện cá nhân đã xảy ra. Khi phỏng vấn, thay bằng ví dụ có thật và nói rõ phạm vi vai trò.

## TL-01. BA gửi yêu cầu mơ hồ, cần estimate trước cuối ngày

**Câu hỏi:** “Khách hàng muốn thêm notification cho tất cả subscribers trong một ngày. Bạn estimate thế nào?”

**Cách trả lời:** “Tôi chưa đưa một con số tuyệt đối khi scope chưa rõ. Tôi sẽ xác định loại event, nguồn subscriber, notification channel, consent, SLA gửi, volume/peak, multi-tenant, provider quota, retry và reporting. Sau đó chia thành discovery/contract, ingestion, fan-out, dispatch, telemetry và integration tests. Tôi estimate theo những phần đã hiểu, ghi assumptions, rủi ro và khoảng ước lượng; báo cho PM/BA phần cần xác nhận trước khi chốt commitment.”

**Follow-up:** Nếu PO không chấp nhận delay? Đề xuất **MVP** có giới hạn và acceptance criteria rõ, không bỏ security/idempotency.

**Tiêu chí đánh giá:** biết hỏi NFR, decomposition, uncertainty, dependency, scope negotiation.

## TL-02. Team muốn chuyển monolith thành microservices vì “dễ scale”

**Cách trả lời:** “Tôi sẽ yêu cầu bằng chứng bottleneck và lý do organizational ownership. Scale có thể chỉ cần tối ưu query hoặc tách background worker. Microservices làm tăng distributed transaction, deployment, logging và contract testing. Tôi sẽ thử modular boundaries, tách feature bằng strangler nếu thực sự cần independent release/scale, rồi đặt success measures và rollback.”

**Follow-up:** Dùng shared DB lâu dài có gì sai? Tight coupling schema/transactions, khó deploy độc lập và xác định data ownership.

**Không nên nói:** “Hệ thống enterprise luôn dùng microservices.”

## TL-03. Senior A muốn dùng Redis cache, Senior B muốn thêm SQL index

**Cách trả lời:** “Tôi bắt đầu từ business freshness và SLO; lấy actual execution plan, logical reads, latency traces, query volume và hot-key distribution. Chúng tôi đánh giá index (write overhead), cache (staleness, invalidation, tenant scope) với cùng workload. Tôi quyết định hoặc đề xuất dựa evidence, ghi trade-offs thành ADR ngắn, gán owner cho validation và theo dõi regression.”

**Follow-up:** Cả hai giải pháp đều đạt p95? Ưu tiên đơn giản, predictable operations và correctness.

## TL-04. Deadline đến ngày mai nhưng QA báo lỗi authorization cross-tenant

**Cách trả lời:** “Đây là correctness/security blocker, không phải cosmetic bug. Tôi xác định blast radius, dừng hoặc giới hạn release liên quan, ưu tiên fix và negative integration test cho tenant A truy cập tenant B, kiểm tra cache/query boundary. Tôi cung cấp ETA có điều kiện và đề xuất giảm scope/feature flag với PM, không chấp nhận bypass auth để kịp deadline.”

**Follow-up:** Nếu stakeholder yêu cầu chấp nhận rủi ro? Escalate theo security policy và người có quyền risk acceptance; không tự thay đổi security gate.

## TL-05. Junior tạo PR 2.000 dòng, khó review, bị trễ sprint

**Cách trả lời:** “Tôi review các lỗi critical trước: data loss, auth, transaction, performance. Tôi trao đổi riêng với bạn ấy về cách chia PR theo vertical slice và testable commits. Pair để làm mẫu một phần thay vì tự viết lại toàn bộ; đặt feedback rõ về acceptance criteria, regression tests và giải pháp làm việc trong sprint sau.”

**Follow-up:** Nếu tiếp tục không tiến bộ? Tìm blocker về requirements/tooling/mentoring, lập plan cụ thể và trao đổi với manager nếu cần.

**Dấu hiệu Lead:** vừa đảm bảo chất lượng giao hàng vừa giúp người khác phát triển.

## TL-06. GitLab CI xanh nhưng Azure Container Apps revision không Ready

**Cách trả lời:** “Deploy command thành công chưa chứng minh service mới Ready hoặc traffic chạy phiên bản đúng. Tôi xem revision status, probe failures, logs, startup config, DB dependency, secret/network, rồi business smoke test. Pipeline phải fail khi revision failed/timeout. Giữ revision ổn định tiếp tục phục vụ nếu deployment mode và routing cho phép; rollback/roll-forward theo compatibility.”

**Follow-up:** DB migration đã DROP column được app cũ dùng? Phải dùng expand-contract trước; image rollback một mình không cứu được.

**Evidence:** revision ID, health status, smoke result, rollout timeline và error metrics.

## TL-07. Service Bus worker gửi trùng notification khi restart

**Cách trả lời:** “At-least-once consumer có thể nhận lại sau crash giữa SQL commit và Complete. Tôi xác định business uniqueness (company, subscriber, event, channel), lưu operation/delivery status theo unique constraint, commit trước ACK, tách retryable/nonretryable lỗi, kiểm soát DLQ. Nếu provider HTTP timeout, delivery có thể đã xảy ra: cần provider idempotency contract hoặc reconciliation; không coi ACK là exactly-once external effect.”

**Follow-up:** User unsubscribe sau fan-out? Check active consent ngay trước side effect, theo policy.

## TL-08. Một endpoint đang chậm, PM muốn thêm 10 API instances

**Cách trả lời:** “Trước hết phân tách request latency bằng traces: app CPU/allocations, SQL waits, connection pool, dependency errors, saturation. Nếu DB lock/I/O là bottleneck, tăng API instances có thể làm nhiều concurrent queries hơn và tệ hơn. Ưu tiên giảm impact bằng rate limit/circuit breaker/throttle hoặc rollback, rồi test query/index/caching theo correctness. Chỉ scale khi bottleneck và capacity justify.”

**Follow-up:** Vì sao average giảm mà p99 tăng? Tail latency, queueing và noisy-neighbor; theo SLO.

## TL-09. Cần dẫn dắt design review cho module mới

**Cách trả lời:** “Tôi chuẩn bị requirements/NFR, ADR và hai lựa chọn tối thiểu. Review data ownership, auth boundary, transaction/failure, API contracts, deployment, logs/metrics, cost và migration. Tôi không bắt team dùng DDD/CQRS/Event Sourcing nếu business invariants chưa justify. Sau review, ra decision, action items, owner, due date và trigger revisit.”

**Follow-up:** Không đồng thuận thì sao? Người chịu trách nhiệm decision rõ ràng, dissent documented, kiểm chứng bằng spike khi rủi ro cao.

## TL-10. Tech Lead khác Senior Backend và Engineering Manager?

**Cách trả lời:** “Senior Backend thường chịu trách nhiệm kỹ thuật sâu và delivery phần việc phức tạp. Tech Lead thêm trách nhiệm technical direction, decomposition, review giải pháp, phối hợp dependencies và rủi ro tiến độ. Engineering Manager có thể chịu people management, hiring, performance evaluation và team health tùy tổ chức. Các vai trò chồng lấn nhưng không đồng nhất; tôi sẽ mô tả chính xác mình đã làm gì.”

**Follow-up:** Nếu chưa chính thức làm Tech Lead, trình bày thế nào? “Tôi đã review PR, tham gia design discussions và mentoring. Tôi đang phát triển thêm estimation, stakeholder communication và ownership end-to-end; tôi chưa tự nhận quản lý toàn team.”

## 60 phút luyện một vòng Lead

- **10 phút:** Chọn TL-01 hoặc TL-10, nói rõ scope và ownership.
- **20 phút:** Chọn TL-02/TL-03/TL-09, vẽ hai phương án kiến trúc, trade-off.
- **15 phút:** Chọn TL-06/TL-07/TL-08, phân tích incident và cách recovery.
- **10 phút:** Chọn TL-04/TL-05, trả lời theo STAR bằng câu chuyện có thật.
- **5 phút:** Ghi 3 điều chưa chứng minh được, 1 cách thu evidence.

## Rubric cho Lead

| Tiêu chí | Dấu hiệu câu trả lời tốt |
| --- | --- |
| Technical depth | Nguyên lý chính xác, không kể tool names suông |
| Delivery | Requirement rõ, estimate có assumptions, triển khai kiểm soát |
| Risk/security | Thấy failure window, multi-tenant/correctness, rollback |
| Collaboration | Feedback cụ thể, coordination, conflict resolution |
| Architecture judgment | Alternatives, simplicity, ADR, cost và operating constraints |
| Integrity | Phân biệt trách nhiệm thật với bài mô phỏng/kinh nghiệm của team |

## Đọc tiếp

[96 câu ưu tiên theo JD](senior-techlead-question-map.md) · [Senior technical answers](index.md) · [System Design](../09-solution-architecture-leadership/design-exercises.md) · [Mock Interview](mock-interview.md).

Tham khảo JD: [Savvycom](https://careers.savvycom.vn/vi/jobs/7-senior-tech-lead-dotnet?apply=1) · [VTI](https://tuyendung.vti.com.vn/en-US/Career/JobDetail/2791) · [Azure Well-Architected Operational Excellence](https://learn.microsoft.com/en-us/azure/well-architected/operational-excellence/principles).
