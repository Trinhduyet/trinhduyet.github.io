# Investor Relations — case study về API, data và notifications

<div class="lesson-meta"><span>Public learning case</span><span>Market data</span><span>Multi-tenant</span></div>

**Case mô phỏng, không phải tài liệu architecture nội bộ.** Nghiệp vụ Investor Relations giúp luyện thiết kế API đọc market data, documents/news, financial calendar và subscription/alerts. Hệ thống read-only của IR **không phải Core Trading hay OMS**.

## Giả định bài toán

- Công ty niêm yết cung cấp quote/chart, disclosures/news, calendar, email subscriptions.
- Market data có historical ticks, delayed/realtime entitlement và multi-company context.
- Notifications được xử lý async; email accepted khác delivered.

```text
Market feed → Stream consumer → Time-series read model → API → Investors
Publication → SQL outbox → Message queue → Subscriber matching → Email
```

## Chọn một bài luyện

| Chủ đề | Câu hỏi trọng tâm | Đi tới |
| --- | --- | --- |
| API market history | Index với instrument + time range, pagination | [SQL Interview](../05-interview-preparation/sql-data.md) |
| Realtime stream | Event Hubs partitions/checkpoint vs queue ACK | [Distributed](../05-interview-preparation/distributed-cloud.md) |
| Notifications | At-least-once, outbox, idempotency | [System Design 01](../09-solution-architecture-leadership/design-exercises.md) |
| Company isolation | AuthZ resource/company, cache leakage | [Security fundamentals](../../09-security-devsecops/README.md) |
| Calendar | Timezone, DST, all-day vs instant | [API contracts](../../06-api-design/README.md) |

## Senior/Architect follow-up

Nếu chart chịu stale 15 phút nhưng alert cần freshness dưới vài giây, vì sao không dùng chung cache policy? Nếu system nhận event lặp, ngăn duplicate notification ở bước nào? Trả lời bằng requirements, flow, tests và observability.

[Case Payment/Fintech](../04-fintech-payments/index.md) · [Mock Interview](../05-interview-preparation/mock-interview.md).
