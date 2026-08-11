# Chính sách nguồn và xác minh

## Mục tiêu

Repository phải phân biệt rõ ba loại nội dung:

1. **Stable concept** — ví dụ ACID, idempotency, dependency injection, CAP nuance.
2. **Version-specific behavior** — ví dụ API đăng ký middleware, Kubernetes field, package version.
3. **Local engineering decision** — ví dụ baseline, convention hoặc lựa chọn của project.

Ba loại này có tốc độ thay đổi và yêu cầu bằng chứng khác nhau.

## Thứ tự ưu tiên nguồn

| Cấp | Nguồn | Dùng cho |
| --- | --- | --- |
| P0 | Specification, standard, RFC | Protocol, semantics, interoperability |
| P1 | Official project/vendor documentation và release notes | Hành vi, API, version, support, deprecation |
| P2 | Microsoft Learn, CNCF, Kubernetes, Docker, OWASP, IETF, OpenTelemetry, Redis, HashiCorp, official GitHub | Hướng dẫn và implementation details |
| P3 | roadmap.sh | Scope, prerequisite candidate, gap discovery |
| P4 | Engineering organization có uy tín, sách/paper gốc | Deep dive và case study |
| P5 | Community material | Chỉ để discovery hoặc khi không có nguồn tốt hơn |

SEO article, bài copy, transcript không xác minh, Stack Overflow và nội dung do AI tạo không được dùng làm nguồn chân lý kỹ thuật.

## Workflow bắt buộc

~~~text
Read existing content
    ↓
Classify claim: stable / version-specific / local decision
    ↓
Discover scope with roadmap.sh
    ↓
Verify behavior with standards and official docs
    ↓
Use Context7 for framework/library/SDK details
    ↓
Compare Vietnamese material when useful
    ↓
Write original explanation and examples
    ↓
Run technical and link checks
    ↓
Record verification metadata
~~~

## Context7

Với framework, library, SDK hoặc package:

1. resolve library ID;
2. chọn source có reputation và coverage phù hợp;
3. query một câu hỏi hẹp, có version;
4. đối chiếu official release/support page nếu câu trả lời liên quan lifecycle;
5. ghi library ID và query trong Verification Metadata.

Không dùng Context7 thay cho RFC, lifecycle page, security advisory hoặc release note.

## Internet research

- Ưu tiên domain chính thức và URL trỏ thẳng đến trang hỗ trợ claim.
- Với release/support status, ghi ngày xác minh và timezone khi thời điểm có thể gây mơ hồ.
- Với patch release, tài liệu nên nêu policy “latest supported patch” và lưu số patch quan sát được như snapshot.
- Khi official sources xung đột, ghi lại xung đột; không âm thầm chọn một bên.
- Không suy diễn version hiện tại từ kiến thức model.

## Tài liệu tiếng Việt

Official English documentation là source of truth. Tài liệu tiếng Việt được thêm khi:

- do vendor/project chính thức duy trì;
- còn khớp version hoặc khái niệm ổn định;
- giúp giảm tải nhận thức cho người đọc.

Nếu bản tiếng Việt cũ hơn hoặc thiếu nội dung, chương phải nói rõ và link bản tiếng Anh mới hơn. Không thêm tài liệu tiếng Việt chất lượng thấp chỉ để đủ danh sách.

## Quy tắc viết và trích dẫn

- Viết lại bằng mental model, diagram, ví dụ và phân tích; không sao chép đoạn dài.
- Link nguồn ở gần claim phụ thuộc nguồn.
- Source list cuối chương được phân loại: Official, Specifications, Roadmap, Vietnamese, Deep Dive, Books/Papers.
- Code phải ghi rõ target version nếu API phụ thuộc version.
- Benchmark phải có workload, hardware/runtime context, warm-up, sample size và giới hạn diễn giải.
- Không ghi private reasoning; chỉ ghi nguồn, query, xung đột và quyết định có ích cho kiểm chứng.

## Metadata tối thiểu cho chương

~~~text
Verified:
YYYY-MM-DD

Technology version:
...

Official sources:
...

Context7 queries used:
...

Notes:
...
~~~

## Kiểm tra trước khi merge

- Claim version-sensitive có official source và ngày xác minh.
- Link không trỏ tới search result hoặc trang mirror.
- Ví dụ code/config dùng API và field tồn tại ở baseline.
- Stable explanation không bị thay đổi chỉ vì patch update.
- Security, performance, reliability, observability và operational impact đã được xem xét khi có liên quan.
- Prerequisite và next topic được cross-link.
- Không có TODO/TBD placeholder.
- Module references.md đã được cập nhật.

## Chính sách cập nhật

Khi baseline thay đổi:

1. cập nhật [technology-baseline.md](technology-baseline.md);
2. tìm claim phụ thuộc version trong module liên quan;
3. giữ nguyên mental model còn đúng;
4. cập nhật API/config/deprecation và test;
5. ghi migration note nếu có breaking change;
6. không chạy mass rewrite nếu chưa có bằng chứng nội dung đã lỗi thời.

## Verification metadata

- Verified: 2026-08-11
- Official sources: [RFC Editor](https://www.rfc-editor.org/), [Microsoft Learn](https://learn.microsoft.com/), [Docker Docs](https://docs.docker.com/), [Kubernetes Docs](https://kubernetes.io/docs/), [OWASP](https://owasp.org/), [OpenTelemetry](https://opentelemetry.io/docs/), [HashiCorp Developer](https://developer.hashicorp.com/), [Redis Docs](https://redis.io/docs/latest/)
- Context7 queries used: policy derived from the required Context7 workflow; specific queries are recorded per chapter.
- Notes: roadmap.sh is scope guidance, not a behavioral authority.
