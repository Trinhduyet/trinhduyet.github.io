# System Design, Solution Architecture & Technical Leadership

<div class="lesson-meta"><span>Requirements / NFR</span><span>Architecture Decisions</span><span>Failures & Cost</span><span>Leadership</span></div>

<div class="course-grid" markdown>

[**9 câu có đáp án** — modular monolith, CAP, ADR, scale, modernization, leadership.](../05-interview-preparation/architecture-leadership.md){ .course-card }

[**3 System Design exercises** — notification 10k, market history API, legacy migration.](design-exercises.md){ .course-card }

[**Mock Interview** — luyện whiteboard, trade-off và feedback.](../05-interview-preparation/mock-interview.md){ .course-card }

</div>

## Trình bày một hệ thống theo 7 bước

1. Business goal, actors và use cases — *không chọn công nghệ trước*.
2. NFR: p95, availability, peak traffic, consistency, RTO/RPO, data protection.
3. Capacity estimate và bottleneck giả định; nói rõ nguồn số liệu.
4. C4 context/container + critical request/data flows.
5. Source of truth, transaction boundaries, async workflows và failure modes.
6. Trade-offs: simpler alternative, cost, operations ownership, scaling.
7. Rollout, telemetry, tests, rollback, ADR và revisit trigger.

## Một tình huống mẫu

**Thiết kế gửi 10.000 emails trong 60 giây.** Nếu yêu cầu là provider acceptance thì tối thiểu ~167 deliveries/s, chưa tính retries. Nếu provider quota chỉ 20/s, cần đổi SLA/provider hoặc phân bổ tải, tăng workers không thể vượt quota đó. Phân biệt accepted, queued, sent-to-provider và delivered.

[Đọc lời giải chi tiết, failure matrix và architecture](design-exercises.md#design-01-notification-cho-10000-recipients).

## Phân biệt năng lực, không ràng buộc title

| Focus | Người phỏng vấn muốn nghe |
| --- | --- |
| Implementation | Code/API/DB đúng và có tests |
| Reliability | Partial failure, retries, rollback, monitoring |
| Ownership | Scope công việc, review, mentoring, delivery |
| Architecture | Constraints, alternatives, data boundaries, cost, evolution |

[Học System Design chuyên sâu](../../24-system-design/README.md) · [Software Architecture](../../25-software-architecture/README.md) · [Architecture review](../../25-software-architecture/architecture-review-playbook.md).
