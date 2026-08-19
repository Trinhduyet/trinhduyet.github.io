# Diagram Design assets

Các SVG trong thư mục này thay Mermaid auto-layout và theo các nguyên tắc editorial từ [`cathrynlavery/diagram-design`](https://github.com/cathrynlavery/diagram-design): low density, một focal accent, không shadow, corner radius tiết chế, semantic node treatment và connector rõ ràng.

Default skin: paper `#f5f5f5`, ink `#2d3142`, muted `#4f5d75`, accent `#eb6c36`, link `#2e5aa8`.

## Text-bound rules

Để tránh lỗi chữ tràn box trên GitHub Pages:

- không đặt title dài vào box 120px trên một dòng;
- label dài phải wrap bằng `<tspan>` thành 2 dòng hoặc tăng node width;
- giữ padding ngang tối thiểu khoảng 16–20px;
- subtitle dài phải tách dòng trước khi giảm font-size;
- không phụ thuộc remote web font để tính kích thước text;
- SVG phải có `viewBox` và `width="100%"` để responsive;
- sau khi sửa diagram, kiểm tra ở desktop và mobile width.

CI chạy `scripts/normalize-diagrams.py` trước MkDocs build như một safety net cho các legacy SVG. Với diagram mới, ưu tiên layout đúng ngay trong source thay vì dựa vào text compression.

Updated: 2026-08-19.
