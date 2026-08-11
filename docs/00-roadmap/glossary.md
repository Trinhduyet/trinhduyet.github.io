# Glossary Việt–Anh

English term giữ vai trò canonical. Bản dịch tiếng Việt được dùng ở lần xuất hiện quan trọng đầu tiên; code, API, protocol và pattern name không dịch.

| English term | Cách dùng tiếng Việt ưu tiên | Ghi chú |
| --- | --- | --- |
| Availability | Tính sẵn sàng | Khả năng hệ thống phục vụ khi được yêu cầu; không đồng nghĩa reliability |
| Reliability | Độ tin cậy | Khả năng thực hiện đúng chức năng trong một khoảng thời gian |
| Resilience | Khả năng phục hồi | Khả năng chịu lỗi, thích nghi và khôi phục dịch vụ |
| Recoverability | Khả năng khôi phục | Khả năng phục hồi dữ liệu/dịch vụ sau sự cố; gắn với RTO/RPO |
| Consistency | Tính nhất quán | Luôn nêu model và boundary; tránh nói “consistent” chung chung |
| Idempotency | Tính lũy đẳng | Lặp cùng operation không tạo thêm hiệu ứng ngoài contract |
| Throughput | Thông lượng | Lượng công việc hoàn thành trên đơn vị thời gian |
| Latency | Độ trễ | Thời gian một operation; ưu tiên distribution thay vì chỉ average |
| Tail latency | Độ trễ đuôi | Thường biểu diễn bằng P95/P99/P99.9 |
| Scalability | Khả năng mở rộng | Khả năng đáp ứng tải tăng bằng tài nguyên/kiến trúc phù hợp |
| Elasticity | Khả năng co giãn | Tự tăng/giảm tài nguyên theo tải |
| Fault | Lỗi tiềm ẩn | Nguyên nhân có thể dẫn đến error |
| Error | Trạng thái sai | Trạng thái nội bộ có thể dẫn đến failure |
| Failure | Sự cố phục vụ | Hành vi quan sát được không đáp ứng contract |
| Partial failure | Lỗi cục bộ | Một phần distributed system lỗi trong khi phần khác vẫn chạy |
| Backpressure | Điều tiết ngược | Cơ chế làm chậm producer khi consumer không theo kịp |
| Retry | Thử lại | Phải đi cùng budget, backoff, jitter và idempotency |
| Timeout | Giới hạn chờ | Một deadline/policy, không phải bằng chứng nguyên nhân gốc |
| Circuit breaker | Circuit breaker | Pattern name giữ nguyên; ngăn tiếp tục gọi dependency đang lỗi |
| Bulkhead | Bulkhead | Cô lập tài nguyên/failure domain |
| Transaction Isolation Level | Mức cô lập giao dịch | Cơ chế kiểm soát hiện tượng đọc/ghi đồng thời |
| Execution Plan | Kế hoạch thực thi | Cách database engine chọn để thực thi query |
| Cardinality estimation | Ước lượng số lượng dòng | Ước lượng số dòng; ảnh hưởng join, memory grant và plan |
| Observability | Khả năng quan sát | Suy ra trạng thái bên trong từ output như logs, metrics, traces |
| Distributed Tracing | Truy vết phân tán | Theo dõi request qua nhiều process/service |
| Service Level Indicator (SLI) | Chỉ số mức dịch vụ | Phép đo trực tiếp về hành vi dịch vụ |
| Service Level Objective (SLO) | Mục tiêu mức dịch vụ | Mục tiêu định lượng cho một SLI trong cửa sổ thời gian |
| Non-functional Requirement (NFR) | Yêu cầu phi chức năng | Phải đo được hoặc kiểm chứng được |
| Trust boundary | Ranh giới tin cậy | Nơi identity, privilege hoặc data trust thay đổi |
| Threat model | Mô hình đe dọa | Phân tích asset, actor, attack path và mitigation |
| Blast radius | Phạm vi ảnh hưởng | Phần hệ thống/dữ liệu/người dùng bị tác động khi lỗi hoặc bị xâm nhập |
| Least privilege | Đặc quyền tối thiểu | Chỉ cấp capability cần thiết, trong scope và thời gian cần thiết |
| Capacity planning | Hoạch định năng lực | Ước lượng nhu cầu và headroom theo workload |
| Graceful shutdown | Dừng an toàn | Ngừng nhận việc mới, hoàn tất/huỷ có kiểm soát, flush state/telemetry |
| Deployment | Deployment | Kubernetes resource name không dịch; nghĩa chung có thể nói “triển khai” |
| Reconciliation | Đối soát trạng thái | Controller liên tục đưa actual state về desired state |
| Eventual consistency | Nhất quán cuối cùng | Các replica hội tụ nếu không có update mới; không có nghĩa “không nhất quán” |
| Groundedness | Mức bám nguồn | Mức output được hỗ trợ bởi context/evidence được phép dùng |
| Hallucination | Nội dung bịa/không có căn cứ | Đánh giá theo task và evidence, không chỉ theo độ trôi chảy |
| Prompt injection | Prompt injection | Dữ liệu không tin cậy cố thay đổi instruction/control flow |
| Tool calling | Tool calling | Model đề xuất/khởi tạo lời gọi tool theo schema |
| Excessive agency | Quyền hành động quá mức | Agent có capability hoặc autonomy vượt nhu cầu |
| Human-in-the-loop | Con người trong vòng kiểm soát | Approval/review là control có audit, không phải nút bấm trang trí |
| Retrieval-Augmented Generation (RAG) | Sinh tăng cường bằng truy xuất | Dùng retrieval để xây context trước generation |
| Model drift | Độ lệch model theo thời gian | Hành vi/quality thay đổi do model, dữ liệu hoặc môi trường |
| Cost per request | Chi phí mỗi request | Bao gồm model, retrieval, compute, network, telemetry và vận hành |

## Quy tắc dùng

- Lần đầu: “Khả năng phục hồi (Resilience)”; sau đó dùng thuật ngữ phù hợp với ngữ cảnh.
- Không thay Availability bằng Reliability hoặc Resilience.
- Không gọi mọi lỗi là “exception”; phân biệt fault, error và failure.
- Không dùng “exactly-once” nếu chưa nêu boundary, assumptions và mechanism.
- Không dùng “real-time”, “high availability” hoặc “scalable” nếu chưa có chỉ số.

## Verification metadata

- Verified: 2026-08-11
- Technology version: stable terminology, không phụ thuộc một implementation
- Official sources: [Google SRE terminology](https://sre.google/sre-book/service-level-objectives/), [OpenTelemetry concepts](https://opentelemetry.io/docs/concepts/), [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110.html)
- Context7 queries used: none
- Notes: glossary là convention của repository; mở rộng khi module mới đưa thêm thuật ngữ có nguy cơ dùng không nhất quán.
