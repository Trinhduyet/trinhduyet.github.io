# Question Bank — 36 câu có lời giải và follow-up

<div class="lesson-meta"><span>.NET · SQL · Azure · Distributed · Architecture</span><span>36 solved questions</span><span>Đọc theo chủ đề</span></div>

**Ưu tiên phỏng vấn tại công ty Việt Nam:** giải thích bằng tiếng Việt, dùng thuật ngữ .NET tiếng Anh khi cần. Không yêu cầu học thuộc đáp án tiếng Anh; CV viết tiếng Anh vẫn phù hợp.

[**96 câu hỏi theo JD Senior/Tech Lead**](senior-techlead-question-map.md) · [**10 tình huống Lead có đáp án**](techlead-scenarios.md) · [**Checklist đánh giá mức sẵn sàng theo JD**](job-readiness-checklist.md).

**Đây không còn là bảng câu hỏi chỉ có từ khóa.** Mỗi bài đưa ra short explanation, mechanism, trade-off, failure scenario hoặc follow-up, kèm liên kết đến tài liệu kiến thức chuyên sâu trên website.

<div class="course-grid" markdown>

[**C# / .NET & API — 9 câu** · Async, DI, cancellation, middleware, GraphQL, idempotency, performance.](dotnet-api.md){ .course-card }

[**SQL Server & EF Core — 9 câu** · Seek/scan, SARGability, keyset, NOLOCK, wallet race, query plans.](sql-data.md){ .course-card }

[**Distributed, Azure, DevOps — 9 câu** · Brokers, ACK/checkpoint, retry, Redis, Managed Identity, AKS, Terraform.](distributed-cloud.md){ .course-card }

[**Architecture & Leadership — 9 câu** · Modular monolith, NFR, CAP, ADR, incident, legacy migration.](architecture-leadership.md){ .course-card }

</div>

## Câu hỏi giới thiệu bản thân và quá trình làm việc

Ngoài 36 câu kỹ thuật, website đã có phần **[Career Story & HR Interview](../career/index.md)** gồm giới thiệu tiếng Việt/Anh, kể CV theo dự án, 24 câu hỏi HR có mẫu trả lời và bài luyện nói. Phần này **không** tính vào 36 câu technical answers.

[Giới thiệu 60 giây](../career/tell-me-about-yourself.md) · [Walk through CV](../career/walk-through-resume.md) · [Common HR questions](../career/common-hr-questions.md).

## Chọn câu theo tình huống

| Interviewer hỏi | Bài có đáp án |
| --- | --- |
| Vì sao `async/await` không có nghĩa là tạo thread? | [NET-01](dotnet-api.md#net-01-asyncawait-co-tao-thread-moi) |
| Scope của DbContext trong BackgroundService? | [NET-02](dotnet-api.md#net-02-di-singleton-scoped-transient-khac-nhau-the-nao) |
| SQL có 100 triệu rows bắt buộc chậm? | [DB-01](sql-data.md#db-01-index-seek-luon-tot-hon-index-scan) |
| Hai yêu cầu debit đồng thời? | [DB-05](sql-data.md#db-05-hai-request-tru-cung-so-du-xu-ly-the-nao) |
| Crash sau DB commit trước ACK? | [DIST-02](distributed-cloud.md#dist-02-checkpoint-hoac-ack-co-nghia-exactly-once-khong) |
| Azure App Service, Container Apps hay AKS? | [DIST-05](distributed-cloud.md#dist-05-azure-app-service-container-apps-hay-aks) |
| Modular monolith hay microservices? | [ARCH-01](architecture-leadership.md#arch-01-monolith-modular-monolith-hay-microservices) |
| Thiết kế notification cho 10k recipients? | [Architecture exercise](../09-solution-architecture-leadership/design-exercises.md#design-01-notification-cho-10000-recipients) |

## Trả lời theo 4 lớp độ sâu

=== "Foundation"

    Nêu định nghĩa chính xác và một ví dụ nhỏ. Ví dụ: Message ACK nói broker có thể xóa/đánh dấu message hoàn thành, **không** có nghĩa external effect exactly-once.

=== "Applied"

    Triển khai bằng C#/SQL hoặc cấu hình. Đưa ra test để kiểm tra happy path và negative path.

=== "Advanced"

    Thảo luận concurrency, duplicates, failure window, data correctness, performance và observability.

=== "Architecture / Leadership"

    Đề xuất alternatives, NFR/SLO, system boundaries, cost, rollout/rollback; review quyết định hoặc dẫn dắt xử lý incident.

## Từ câu trả lời sang thực hành

[SQL Server lab](../06-practical-labs/sql-performance-lab.md) · [3 System Design exercises](../09-solution-architecture-leadership/design-exercises.md) · [Mock Interview 30/60/90 phút](mock-interview.md).

!!! note "Coverage trung thực"
    36 câu là bộ **đáp án chọn lọc hiện đã viết trên website public**. Ngân hàng 200 câu trong kho nghiên cứu private có độ sâu không đồng đều và không được tự động đưa lên web. Không nên hiểu số 36 là giới hạn nội dung tương lai.
