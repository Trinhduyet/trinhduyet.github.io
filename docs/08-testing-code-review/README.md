# Module 08 — Testing và Code Review

> [← Module 07](../07-aspnet-core/README.md) · [Roadmap](../00-roadmap/README.md)

Module này tập trung vào Test boundaries, contract evidence, quality gates và review discipline. Mental model đi trước syntax; mọi chapter nối requirement → boundary → evidence → operations.

## Module trong một hình

~~~mermaid
flowchart LR
    A["Requirement + workload"] --> B["Test Strategy và Boundaries"] --> C["Integration, Contract và Load Testing"] --> D["Code Review, Quality Gates và Failure Analysis"] --> E["Production decision"]
    E -. "evidence feedback" .-> A
~~~

## Phạm vi và trạng thái

| Learning slice | Priority | Trạng thái nội dung | Evidence người học |
| --- | --- | --- | --- |
| [Test Strategy và Boundaries](test-strategy-and-boundaries.md) | P0 | Content v1 | Pending |
| [Integration, Contract và Load Testing](integration-contract-and-load-testing.md) | P0 | Content v1 | Pending |
| [Code Review, Quality Gates và Failure Analysis](code-review-quality-and-failure-analysis.md) | P0 | Content v1 | Pending |

## Dependency map

~~~mermaid
flowchart TD
    PRE["Module 07"] --> CURRENT["Module 08<br/>Testing và Code Review"]
    CURRENT --> NEXT["Module 09<br/>Security và DevSecOps"]
    CURRENT -. "Project spine / production evidence" .-> PROJECT["Architecture artifact"]
~~~

## Cách học

1. Đọc mental model và dependency trước syntax.
2. Chạy hoặc thiết kế lab bounded, lưu output.
3. Review theo Senior Engineer, Security, Performance, Operations và Architect.
4. Chỉ chuyển module khi exit criteria và evidence đạt.

## Evidence tối thiểu

- Một diagram/contract nối input, core state, resources và recovery.
- Một failure/overload/security experiment có expected outcome.
- Một performance/operations note và rollback/recovery path.
- Một decision record nói rõ phương án đơn giản hơn, cost và trigger migrate.

## Tiếp tục từ đây

Sau module này, mở Module 09 — Security và DevSecOps khi content v1; không bỏ qua prerequisite chỉ vì đã đọc syntax.

## Official references

Xem [references.md](references.md) để lấy source of truth. Roadmap discovery không thay behavior/specification.

## Verification metadata

- Verified: 2026-08-11.
- Content status: Content v1; learner evidence pending.
- Technology focus: Testing và Code Review.
- Context7 queries used: none; callable tool unavailable in this run.

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
