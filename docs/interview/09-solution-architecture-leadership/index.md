# Solution Architecture & Technical Leadership — .NET

## Phương pháp ra quyết định

```text
Business goals → functional requirements → NFR & constraints
→ options → design/data ownership → failure modes
→ cost & security → ADR → deployment plan → measured feedback
```

C4 Context/Container/Component diễn tả system boundaries. Container trong C4 là deployable application/data store, **không** nhất thiết Docker container.

## Case: 10.000 notification recipients

Giả sử cần gửi trong 60 giây, throughput mục tiêu tối thiểu khoảng 167 deliveries/second (chưa tính retries/provider throttling). Cần làm rõ accepted-to-queue và delivered-to-inbox là hai SLA khác nhau.

Option: API → transactional outbox → queue → bounded workers → provider. Chọn partitioning, idempotency, consent, retry/DLQ và telemetry theo business constraints. Nếu provider chỉ cho 20/s thì không thể đạt SLA bằng cách chỉ scale workers.

## Leadership scenarios

- Hai senior bất đồng về cache vs query optimization: lấy evidence/query plan, quyết định theo SLA và ghi ADR.
- Migrate legacy .NET Framework: contract tests, strangler pattern, rollback, expand-contract DB.
- Incident: ưu tiên mitigation, coordination và communication; postmortem có corrective action đo được.
- Technical debt: ưu tiên reliability/security/delivery friction theo business impact.

[System Design](../../24-system-design/README.md) · [Software Architecture](../../25-software-architecture/README.md) · [Architecture Review](../../25-software-architecture/architecture-review-playbook.md).
