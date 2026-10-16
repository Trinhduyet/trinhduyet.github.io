# Senior .NET & Technical Lead — tự kiểm tra mức độ sẵn sàng theo JD

> Không có chứng chỉ “đủ Senior/Lead” chỉ vì đọc xong 200 câu. Checklist này giúp đánh giá **năng lực có thể thể hiện** qua giải thích kỹ thuật, code, design và bằng chứng đã làm. Tiếng Việt là mặc định cho vòng phỏng vấn công ty Việt Nam; CV tiếng Anh giữ theo quy định ứng tuyển.

## 1. Đối chiếu JD theo phạm vi vai trò

| Năng lực | Senior Backend .NET | Technical Lead .NET | Cách chứng minh |
| --- | --- | --- | --- |
| C# / ASP.NET Core | Trực tiếp triển khai, debug async/DI/middleware | Review correctness và định hướng framework conventions | Code walkthrough, tests, perf trace |
| SQL/EF Core | Query/index/concurrency/paging | DB architecture, rollout schema, perf ownership | Execution plan, SQL script, before/after |
| APIs & Security | REST/GraphQL/authZ/multi-tenant | Cross-team contracts, security gates | API contract + negative tests |
| Distributed/messaging | Retry, idempotency, DLQ, Redis | Failure containment, throughput, broker choice | Failure matrix + consumer tests |
| Azure/DevOps | Docker, deployments, monitoring, rollback | Platform trade-offs, release decisions, observability | Pipeline/artifact + readiness evidence |
| Architecture | Phân tích module/data boundaries | NFR, alternative designs, ADR, migration roadmap | C4/sequence/ADR/estimate |
| Testing & quality | Unit/integration/load, PR review | Standards, gates, quality coaching | CI output + PR examples |
| Leadership/delivery | Own technical features; mentor when able | Estimate, breakdown, risk, negotiation, coordination | Real STAR example, sprint outcome |
| Business collaboration | Làm rõ acceptance criteria | Quyết định scope/trade-offs với BA/PM/QA | Requirements/decision log |

**Lưu ý:** Một Senior cũng có thể đảm nhiệm nhiều công việc Lead, và một Tech Lead có thể tiếp tục code sâu. Bảng phản ánh xu hướng của JD mẫu, không định nghĩa chức danh tuyệt đối.

## 2. Tự chấm theo bằng chứng, không theo năm kinh nghiệm

| Level | Mô tả |
| --- | --- |
| 0 — Chưa hiểu | Không thể giải thích cơ chế |
| 1 — Kiến thức cơ bản | Nêu đúng khái niệm nhưng chưa có cách triển khai |
| 2 — Áp dụng | Có code/example đúng và test được |
| 3 — Production/advanced | Xử lý trade-off, failures, performance, authZ, rollback |
| 4 — Design/Lead | Đưa alternatives, NFR, estimate, cost, ownership, mentoring và proof |

Chọn **8 chủ đề nền** (runtime, ASP.NET, SQL, messaging, security, Azure/DevOps, testing, design). Với Tech Lead đánh giá thêm **4 chủ đề trách nhiệm** (estimate, coordination, quality gates, incident leadership). Câu nào thiếu bằng chứng thì đánh dấu *Needs evidence*.

Không tính điểm trung bình để khẳng định trúng tuyển; mỗi công ty khác nhau về bài coding, system design, tiếng Anh và vai trò quản lý.

## 3. Bốn bài đánh giá thực hành

### Test A — Backend API (30 phút)

- Giải thích async await, DI lifetime và request cancellation.
- Review sample ASP.NET API có DbContext trong singleton hoặc thiếu authorization.
- Viết một negative test cho tenant access.

**Pass evidence:** Chỉ được lỗi, cách sửa, test và failure behavior.

### Test B — SQL & event-driven (45 phút)

- Bảng 100M rows, truy vấn theo tenant + time; chọn index và explain plan.
- Consumer chết sau DB COMMIT trước broker ACK; thiết kế idempotency.
- Payment provider timeout: giải thích trạng thái unknown.

**Pass evidence:** SQL/script/diagram, logical reads, transaction & retry boundary.

### Test C — System Design (45 phút)

- Thiết kế notification service cho giả định 10k recipients và SLO 60s.
- Đề xuất 2 options, tính throughput tối thiểu, kiểm tra provider quota.
- Đưa security, data flow, monitoring, cost và rollback.

**Pass evidence:** Diagram, assumptions, API/data model, failure matrix, trade-offs, ADR.

### Test D — Tech Lead situations (45 phút)

- BA requirement mơ hồ cần estimate.
- Security blocker ngay trước release.
- Hai Senior khác quan điểm kiến trúc.
- Một incident không thuộc trách nhiệm một người riêng lẻ.

**Pass evidence:** cách hỏi rõ yêu cầu, phân công/communication, trade-off có trách nhiệm, kế hoạch recovery và STAR thật.

## 4. Khi phỏng vấn tại công ty Việt Nam

- **Giới thiệu bản thân:** tiếng Việt 60–90 giây và CV tiếng Anh chuẩn.
- **Technical:** giải thích tiếng Việt, dùng tên công nghệ/thuật ngữ tiếng Anh tự nhiên.
- **Leadership:** tình huống thật, trách nhiệm, phối hợp, ra quyết định.
- **Tiếng Anh:** chuẩn bị tùy JD/khách hàng/vòng phỏng vấn; **công ty Việt Nam vẫn có thể yêu cầu English communication**.

## 5. Kết luận theo mức hoàn thiện tài liệu hiện nay

- **Đã có:** 36 đáp án kỹ thuật công khai, 24 câu HR, 96 câu hỏi ưu tiên theo JD và 10 Tech Lead situations.
- **Cần đào sâu:** nhiều câu trong ngân hàng private 200 mới là short prompts; 25 System Design chưa có full answer trong chính file ngân hàng.
- **Chưa chứng minh:** khả năng của một ứng viên cụ thể cho một JD cụ thể bằng mock interview/code evidence và STAR stories thật.

Bắt đầu: [96 câu hỏi Senior/Lead](senior-techlead-question-map.md) · [10 Tech Lead scenarios](techlead-scenarios.md) · [Mock Interview](mock-interview.md) · [SQL lab](../06-practical-labs/sql-performance-lab.md).

**JD tham khảo:** [Savvycom Senior/Tech Lead](https://careers.savvycom.vn/vi/jobs/7-senior-tech-lead-dotnet?apply=1) · [VTI Senior .NET](https://tuyendung.vti.com.vn/en-US/Career/JobDetail/2791) · [CMC Senior .NET](https://careers.cmcts.com.vn/job/hn-senior-.net-fullstack-developer-5359).
