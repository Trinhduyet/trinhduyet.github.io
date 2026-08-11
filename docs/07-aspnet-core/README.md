# Module 07 — ASP.NET Core

> [← Module 06](../06-api-design/README.md) · [Roadmap](../00-roadmap/README.md)

Module này tập trung vào Pipeline, hosting, configuration, resilience, deployment và observability. Mental model đi trước syntax; mọi chapter nối requirement → boundary → evidence → operations.

## Module trong một hình

~~~mermaid
flowchart LR
    A["Requirement + workload"] --> B["Pipeline, Hosting và Configuration"] --> C["Resilience, Security và Middleware Production"] --> D["Deployment, Observability và ASP.NET Operations"] --> E["Production decision"]
    E -. "evidence feedback" .-> A
~~~

## Phạm vi và trạng thái

| Learning slice | Priority | Trạng thái nội dung | Evidence người học |
| --- | --- | --- | --- |
| [Pipeline, Hosting và Configuration](pipeline-hosting-and-configuration.md) | P0 | Content v1 | Pending |
| [Resilience, Security và Middleware Production](resilience-security-and-middleware.md) | P0 | Content v1 | Pending |
| [Deployment, Observability và ASP.NET Operations](deployment-observability-and-operations.md) | P0 | Content v1 | Pending |

## Dependency map

~~~mermaid
flowchart TD
    PRE["Module 06"] --> CURRENT["Module 07<br/>ASP.NET Core"]
    CURRENT --> NEXT["Module 08<br/>Testing và Code Review"]
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

Sau module này, mở Module 08 — Testing và Code Review khi content v1; không bỏ qua prerequisite chỉ vì đã đọc syntax.

## Official references

Xem [references.md](references.md) để lấy source of truth. Roadmap discovery không thay behavior/specification.

## Verification metadata

- Verified: 2026-08-11.
- Content status: Content v1; learner evidence pending.
- Technology focus: ASP.NET Core.
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
