# Mock Interview — tự luyện 30/60/90 phút

<div class="lesson-meta"><span>Practical</span><span>Đánh giá dựa trên bằng chứng</span><span>Không cố định chức danh</span></div>

Mỗi buổi có thể luyện theo năng lực hiện tại. Không cần chứng minh đã làm 8 hoặc 10 năm; cần giải thích đúng một quyết định kỹ thuật và khả năng xử lý khi thông tin chưa đủ.

## 1. Chọn phiên luyện

| Thời lượng | Định dạng | Thành quả |
| --- | --- | --- |
| 30 phút | 5 intro + 10 fundamentals + 10 SQL/messaging + 5 feedback | Biết điểm yếu và giải thích bằng ví dụ |
| 60 phút | 5 intro + 15 coding/runtime + 15 data/reliability + 15 system design + 10 review | Sửa một đáp án thiếu sâu |
| 90 phút | 10 experience + 20 backend + 20 data/distributed + 25 architecture + 10 leadership + 5 review | Bảo vệ một thiết kế và decision trade-offs |

## 2. Kịch bản 60 phút thực hành ngay

### Phút 0–5 — Giới thiệu (đọc mẫu trước khi luyện)

Chuẩn bị [Tell Me About Yourself](../career/tell-me-about-yourself.md) và [Walk Through Resume](../career/walk-through-resume.md); nếu là vòng HR, thêm [24 common HR questions](../career/common-hr-questions.md).

### Tình huống giới thiệu thực tế

**Interviewer:** “Bạn hãy kể một tính năng backend bạn từng xây, giới hạn công việc của bạn là gì?”

Trả lời theo: **context → business need → your responsibility → technical choices → observable result**. Nếu kết quả chưa đo, nói rõ thiếu dữ liệu. Không tự nhận cả team hoặc stack là do mình xây.

### Phút 5–20 — .NET

1. Khi nào async I/O giúp scalability mà không tạo thêm thread?
2. DbContext Scoped được dùng trong BackgroundService thế nào?
3. Một REST API có request hủy giữa lúc ghi DB, bạn xử lý thế nào?

**Follow-up sâu:** So sánh cancellation trước commit và sau commit; idempotency ở retry.

### Phút 20–35 — SQL / Messaging

4. Một bảng 100 triệu rows trả response chậm: cần lấy bằng chứng gì?
5. Service Bus consumer crash sau commit trước ACK: điều gì xảy ra?
6. Đưa ví dụ SQL keyset pagination với thứ tự stable.

**Follow-up:** Nếu operation đã gửi đến payment provider trước khi API timeout thì sao? Trạng thái UNKNOWN, query provider status, reconciliation; không tự giả định rollback external side effect.

### Phút 35–50 — Design

**Đề:** thiết kế gửi email cho 10.000 subscribers sau publication. Clarify SLO, consent, tenant, provider quota. Mô tả API acceptance, outbox, queue, worker, delivery state, retry/DLQ, observability. Vẽ ít nhất một luồng failure.

**Follow-up:** Hệ thống chỉ có 1 worker, DB shared; bạn bắt đầu ở đâu trước khi tách microservices? Khi nào không nên đưa AKS vào?

### Phút 50–60 — Kết quả

Ghi “câu tôi chưa giải thích được”, lệnh/test sẽ dùng để kiểm chứng, và một cải thiện cụ thể cho lần sau.

## 3. Rubric — chấm từng câu 0–4

| Mức | Tính đúng và chiều sâu |
| --- | --- |
| 0 | Không trả lời được hoặc sai cơ chế |
| 1 | Nhớ tên/định nghĩa nhưng không giải thích được |
| 2 | Trả lời đúng cơ bản, có ví dụ hoặc code đơn giản |
| 3 | Trình bày được trade-off, failure, correctness và cách test |
| 4 | Đánh giá được alternatives, NFR, observability, cost và rủi ro rollout |

Chấm 5 chiều: **correctness, engineering mechanism, failure & recovery, evidence, communication**. Vai trò ứng tuyển ảnh hưởng trọng số từng chiều, không biến năm kinh nghiệm thành tiêu chí cứng.

## 4. Mẫu ghi feedback

```text
Date / target role / topic:
Question:
My answer (record 90 seconds):
Accuracy 0-4:
Mechanism 0-4:
Failure/recovery 0-4:
Evidence/test 0-4:
Communication 0-4:
One concrete gap:
One verification action:
Follow-up question for next session:
```

## 5. Khi chưa có kinh nghiệm thực tế với technology

Nói: “Tôi chưa trực tiếp vận hành AKS production; tôi đã thực hành Kubernetes Deployment/Service/probes và có thể giải thích các failure modes. Để production, tôi sẽ kiểm tra RBAC, network policies, upgrade/backup và đội ngũ vận hành.” Không biến kiến thức lab thành ownership production.

## 6. Luyện riêng HR / Hiring Manager trong 30 phút

- 5 phút: giới thiệu bản thân Present → Past → Future.
- 10 phút: đi qua CV và project ownership.
- 10 phút: 2 câu STAR về teamwork, conflict, mistake hoặc failure (phải có sự kiện thật).
- 5 phút: why us, định hướng, và hỏi ngược interviewer.

Chi tiết: [Career & HR](../career/index.md) · [Speaking Practice](../career/english-speaking-practice.md).

## 7. Bộ đề dùng lại

[C# / API](dotnet-api.md) · [SQL/EF](sql-data.md) · [Azure/Distributed](distributed-cloud.md) · [Architecture/Leadership](architecture-leadership.md) · [Design exercise](../09-solution-architecture-leadership/design-exercises.md).
