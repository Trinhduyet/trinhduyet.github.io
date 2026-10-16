# Walk Me Through Your Resume — kể quá trình làm việc như một câu chuyện

<div class="lesson-meta"><span>CV storytelling</span><span>2–3 phút</span><span>Project ownership</span></div>

Bạn không cần recite mọi bullet. Người phỏng vấn muốn thấy sự liên tục: **bài toán trước đây → phạm vi trách nhiệm → năng lực đã phát triển → lý do phù hợp với cơ hội mới**.

## 1. Cách sắp xếp theo CV

| Mốc | Chọn thông tin | Dành bao lâu? |
| --- | --- | --- |
| Bắt đầu nghề nghiệp | Stack đầu tiên, loại ứng dụng, bài học về xây API/DB | 15–25 giây |
| Giai đoạn trưởng thành | Product scope lớn hơn, integration/testing, collaboration | 30–45 giây |
| 1–2 công việc gần nhất | Core responsibilities, một challenge, impact có chứng cứ | 60–90 giây |
| Liên hệ JD | Vì sao kinh nghiệm transfer tốt và điều muốn phát triển | 15–30 giây |

**Không tự đặt năm, title, achievement hoặc lý do chuyển việc nếu CV chưa chứng minh.**

## 2. Mẫu câu trả lời (HYPOTHETICAL)

> “I started my career by developing web applications using .NET and relational databases. As the projects grew, I gained experience in more complex business workflows, external integrations, and API performance. My recent work has involved backend APIs, cloud services, data processing, and production troubleshooting. Along the way, I also became more involved in code reviews and technical discussions. These experiences helped me understand both implementation details and system-level trade-offs, which is why I'm interested in continuing to work on reliable backend platforms.”

Đây là câu chuyện **minh họa**, không phải hồ sơ đã được xác minh của một ứng viên.

## 3. Project ownership và bằng chứng

Trong mỗi dự án, hãy hoàn thành bảng sau:

| Câu hỏi | Câu trả lời cần chuẩn bị |
| --- | --- |
| Product solved what? | Một nhu cầu nghiệp vụ mà người không biết project cũng hiểu |
| What did **you** build? | Endpoint/service/query/integration cụ thể, phân biệt team responsibility |
| Which tech and why? | Chỉ giải thích stack thực tế từng dùng và design trade-off |
| Hardest failure? | Một challenge thật; không dùng giả định làm production incident |
| How verified? | Tests, query plan, logs, metrics, release validation |
| Result? | Tác động thật; nếu không đo metric, nói rõ |

### Mẫu dự án: Backend API có tích hợp bên thứ ba (giả định)

**Question:** “Tell me about one difficult integration.”

**EN answer outline:** “The application needed to integrate with [provider type]. I was responsible for [specific endpoint or client code]. We had to clarify [authentication, timeout, error mapping]. I verified the contract with [tests/logs/provider sandbox]. If the outcome was unknown after a timeout, we avoided assuming that the operation had failed and used the provider's documented status flow.”

**Follow-up:** What did you personally own? Which callback did you implement? How did you prevent duplicates?

## 4. Khi hỏi lý do chuyển việc

Không dùng một câu trả lời mẫu để che sự thật. Chuẩn bị riêng cho từng giai đoạn:

- **Real reason:** [verified and comfortable to disclose].
- **Positive learning:** [skill or domain acquired].
- **Next-step link:** [how the next role fit your direction].

Ví dụ khung: “At that point, I was looking for [real goal]. The next project allowed me to work on [verified area], where I learned [relevant skill].”

## 5. Cách tùy biến theo role

Backend: nói rõ code và tests. Senior: thêm performance/failure/trade-offs. Tech Lead: một ví dụ thật về design review, mentoring và delivery coordination. Architect: requirements, NFR, alternatives, migration/cost và evidence. Đó là **góc nhìn**, không gắn cứng số năm kinh nghiệm.

## 6. Câu hỏi liên quan CV thường gặp

1. Why did you move from one company/project to another?
2. Why was that technology chosen?
3. What were your contributions versus the team's?
4. Which project was the most challenging?
5. What did you learn from legacy systems?
6. How have your responsibilities changed over time?
7. Which area would you like to own next?
8. Can you explain a result you listed on your CV?

**Tiếp theo:** [Common HR questions](common-hr-questions.md) · [STAR stories](common-hr-questions.md#behavioral-va-starr) · [Mock interview](../05-interview-preparation/mock-interview.md).
