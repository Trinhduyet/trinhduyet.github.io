# Module 14 — Cloud

> [← Module 13](../13-devops-iac/README.md) · [Roadmap](../00-roadmap/README.md)

Module này tập trung vào Compute/storage/network/identity primitives, reliability, DR và cost. Mental model đi trước syntax; mọi chapter nối requirement → boundary → evidence → operations.

## Module trong một hình

~~~mermaid
flowchart LR
    A["Requirement + workload"] --> B["Cloud Primitives, Identity và Networking"] --> C["Regions, Availability và Disaster Recovery"] --> D["Cloud Cost Governance và Operations"] --> E["Production decision"]
    E -. "evidence feedback" .-> A
~~~

## Phạm vi và trạng thái

| Learning slice | Priority | Trạng thái nội dung | Evidence người học |
| --- | --- | --- | --- |
| [Cloud Primitives, Identity và Networking](cloud-primitives-identity-and-networking.md) | P0 | Content v1 | Pending |
| [Regions, Availability và Disaster Recovery](regions-availability-and-disaster-recovery.md) | P0 | Content v1 | Pending |
| [Cloud Cost Governance và Operations](cloud-cost-governance-and-operations.md) | P0 | Content v1 | Pending |

## Dependency map

~~~mermaid
flowchart TD
    PRE["Module 13"] --> CURRENT["Module 14<br/>Cloud"]
    CURRENT --> NEXT["Module 15<br/>Kubernetes"]
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

Sau module này, mở Module 15 — Kubernetes khi content v1; không bỏ qua prerequisite chỉ vì đã đọc syntax.

## Official references

Xem [references.md](references.md) để lấy source of truth. Roadmap discovery không thay behavior/specification.

## Verification metadata

- Verified: 2026-08-11.
- Content status: Content v1; learner evidence pending.
- Technology focus: Cloud.
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
