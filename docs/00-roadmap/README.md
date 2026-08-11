# Lộ trình AI-enabled Software Architect

> [← README tổng quan của repository](../../README.md)

Kho tri thức này dành cho kỹ sư phần mềm Việt Nam đi từ năng lực triển khai C#/.NET đến năng lực thiết kế, vận hành và bảo vệ hệ thống có AI trong production.

Đích đến không phải là thuộc tên công nghệ. Đích đến là có thể chứng minh một quyết định kiến trúc bằng yêu cầu, bằng chứng, ràng buộc, chi phí và trade-off.

## Trạng thái hiện tại

| Hạng mục | Trạng thái | Bằng chứng |
| --- | --- | --- |
| Nền tảng roadmap | Hoàn thành bản đầu | Tám tài liệu bắt buộc, progress template và research report |
| Technology baseline | Đã xác minh ngày 2026-08-11 | [technology-baseline.md](technology-baseline.md) |
| Dependency graph | Hoàn thành bản đầu | [prerequisites.md](prerequisites.md) |
| Module 01 — Computer Science Essentials | Content v1 hoàn thành 4/4; learner evidence pending | [Module overview](../01-computer-science/README.md) |
| Module 02 — Linux, Git và Networking | Content v1 hoàn thành 7/7; learner evidence pending | [Module overview](../02-linux-git-networking/README.md) |
| Module 03 — C#/.NET Runtime | Content v1 hoàn thành 5/5; RuntimeLab buildable; learner evidence pending | [Module overview](../03-dotnet/README.md) |
| Module 04 — Backend | Content v1 hoàn thành 4/4; BackendLab buildable; learner evidence pending | [Module overview](../04-backend/README.md) |
| Module 05 — SQL | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../05-sql/README.md) |
| Module 06 — API Design | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../06-api-design/README.md) |
| Module 07 — ASP.NET Core | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../07-aspnet-core/README.md) |
| Module 08 — Testing và Code Review | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../08-testing-code-review/README.md) |
| Module 09 — Security và DevSecOps | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../09-security-devsecops/README.md) |
| Module 10 — Performance | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../10-performance/README.md) |
| Module 11 — Redis và Caching | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../11-redis-caching/README.md) |
| Module 12 — Docker | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../12-docker/README.md) |
| Module 13 — DevOps và IaC | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../13-devops-iac/README.md) |
| Module 14 — Cloud | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../14-cloud/README.md) |
| Module 15 — Kubernetes | Content v1 hoàn thành 3/3; learner evidence pending | [Module overview](../15-kubernetes/README.md) |
| Module 16–26 | Planned | [master-roadmap.md](master-roadmap.md) |

## Bắt đầu ở đâu?

1. Đọc [master-roadmap.md](master-roadmap.md) để hiểu phạm vi, priority và đích kiến trúc.
2. Dùng [skills-matrix.md](skills-matrix.md) để tự đánh giá bằng bằng chứng, không bằng cảm giác.
3. Đi theo [learning-path.md](learning-path.md); chỉ bỏ qua prerequisite khi vượt qua exit criteria tương ứng.
4. Bắt đầu [Computer Science Essentials](../01-computer-science/README.md), rồi nối evidence sang [Linux, Git và Networking](../02-linux-git-networking/README.md).
5. Học [C#/.NET Runtime](../03-dotnet/README.md) và chạy RuntimeLab trước khi đi Backend/ASP.NET Core.
6. Học [Backend request lifecycle](../04-backend/README.md) và chạy BackendLab trước SQL/EF Core.
7. Đi theo Module 05–15 theo dependency map, mỗi module phải có decision/evidence trước khi chuyển tiếp.
7. Tra [prerequisites.md](prerequisites.md) trước khi học một chủ đề nâng cao.
8. Kiểm tra [technology-baseline.md](technology-baseline.md) trước khi viết code hoặc cấu hình phụ thuộc phiên bản.
9. Tuân thủ [source-policy.md](source-policy.md) khi tạo hoặc cập nhật chương.
10. Dùng [glossary.md](glossary.md) để giữ thuật ngữ Việt–Anh nhất quán.
11. Ghi evidence bằng [progress-template.md](progress-template.md), không bằng checkbox “đã đọc”.

## Chuẩn năng lực

Mỗi chủ đề đi qua sáu lớp:

| Lớp | Câu hỏi phải trả lời được |
| --- | --- |
| L0 — Overview | Công nghệ này giải quyết vấn đề gì? |
| L1 — Fundamentals | Khái niệm và primitive cốt lõi là gì? |
| L2 — Working Knowledge | Có thể triển khai đúng một trường hợp điển hình không? |
| L3 — Production Engineering | Hệ thống lỗi, chậm, bị tấn công hoặc cần quan sát thì xử lý thế nào? |
| L4 — Internals | Cơ chế bên trong nào giải thích hành vi đã quan sát? |
| L5 — Architecture | Khi nào nên dùng, không nên dùng, và điều gì đổi ở 10x/100x scale? |

Một chương chỉ được xem là hoàn thành khi có bằng chứng quan sát được: code chạy, test, execution plan, trace, benchmark có phương pháp, failure experiment, runbook, ADR hoặc architecture review.

## Nguyên tắc vận hành repository

- Mental model trước syntax.
- Stable concept tách khỏi implementation phụ thuộc phiên bản.
- Official English documentation là nguồn kỹ thuật chuẩn; tài liệu tiếng Việt hỗ trợ tiếp thu nhưng không được ghi đè hành vi mới hơn.
- Mỗi quyết định công nghệ phải nêu yêu cầu, phương án đơn giản hơn, failure modes, security boundary, chi phí và operational ownership.
- Không mặc định Clean Architecture, CQRS, MediatR, Repository Pattern, Event Sourcing, microservices hoặc Kubernetes.
- AI là một subsystem. Các yêu cầu truyền thống về dữ liệu, authorization, reliability và observability vẫn áp dụng.
- Prompt, model, retrieval config và eval dataset đều là versioned production artifacts.

## Kết quả nghề nghiệp

Người học hoàn thành roadmap phải có thể:

- chuyển business requirements thành functional requirements, NFR và system boundaries;
- thiết kế API contract, data architecture, security/trust boundaries và deployment topology;
- ước lượng capacity, latency, throughput, availability, recovery và cost;
- chọn monolith, modular monolith, event-driven hoặc microservices dựa trên bằng chứng;
- thiết kế RAG/agent/MCP với evaluation, least privilege, audit và human approval;
- lập ADR, threat model, failure analysis, migration plan, rollback và runbook;
- review một kiến trúc ở góc nhìn Senior Engineer, Security, Performance, Operations và Software Architect.

## Nguồn phạm vi

Danh mục roadmap.sh hiện hành được dùng để phát hiện phạm vi và lỗ hổng, không dùng làm nguồn mô tả hành vi công nghệ. Chi tiết đối chiếu nằm trong [master-roadmap.md](master-roadmap.md); quy tắc nguồn nằm trong [source-policy.md](source-policy.md).

## Verification metadata

- Verified: 2026-08-11
- Scope source: [roadmap.sh catalog](https://roadmap.sh/roadmaps/)
- Technology versions: xem [technology-baseline.md](technology-baseline.md)
- Context7 queries used: xem metadata trong [technology-baseline.md](technology-baseline.md)
- Notes: repository hiện có content v1 cho Module 01, 02, 03 và 04; current level vẫn không tăng cho đến khi learner evidence đạt exit criteria.

<!-- Mermaid.js Script CDN hỗ trợ tự động render sơ đồ Mermaid trên GitHub Pages (Jekyll) -->
<script type="module">
  import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
  mermaid.initialize({ startOnLoad: true, theme: 'default' });

  document.addEventListener("DOMContentLoaded", function () {
    const elements = document.querySelectorAll("pre.language-mermaid, code.language-mermaid, .language-mermaid pre, pre code.language-mermaid");
    elements.forEach((el) => {
      const container = el.tagName.toLowerCase() === "code" ? el.parentElement : el;
      const div = document.createElement("div");
      div.className = "mermaid";
      div.textContent = el.textContent;
      if (container && container.parentNode) {
        container.parentNode.replaceChild(div, container);
      }
    });
    mermaid.run({ querySelector: '.mermaid' });
  });
</script>
