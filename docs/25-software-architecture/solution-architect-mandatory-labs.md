# Bài thực hành bắt buộc — .NET Solution Architect

> [← Trang tổng quan](README.md) · [Danh mục 16 nhóm kiến thức](solution-architect-technical-catalog.md) · [Case study Financial / Loyalty / Booking / OMS](solution-architect-dotnet-case-studies.md)

**Nguyên tắc:** Không đánh dấu một lab là “đã chạy” khi chưa có source code, kết quả test và log/metrics thực tế. Các bài dưới đây là **đề bài và tiêu chí nghiệm thu**, không phải bằng chứng đã triển khai.

## 1. Bộ 12 lab cốt lõi và 4 lab mở rộng

| Mã | Lab | Mức | Kết quả cần nộp |
|---|---|---|---|
| [L01](#l01) | Điều tra HTTP chậm và ThreadPool | Bắt buộc | Code + test + log/metrics |
| [L02](#l02) | Refactor payment adapter | Bắt buộc | Code + test + log/metrics |
| [L03](#l03) | DDD cho OMS, Cash và Gateway | Bắt buộc | Code + test + log/metrics |
| [L04](#l04) | SQL index, isolation và deadlock | Bắt buộc | Code + test + log/metrics |
| [L05](#l05) | Chống double-spend và oversell | Bắt buộc | Code + test + log/metrics |
| [L06](#l06) | Outbox, Inbox và Saga recovery | Bắt buộc | Code + test + log/metrics |
| [L07](#l07) | API Idempotency và contract evolution | Bắt buộc | Code + test + log/metrics |
| [L08](#l08) | DLQ, ordering và backpressure | Bắt buộc | Code + test + log/metrics |
| [L09](#l09) | Redis cache stampede và stale data | Bắt buộc | Code + test + log/metrics |
| [L10](#l10) | Load balancing, graceful shutdown, resilience | Bắt buộc | Code + test + log/metrics |
| [L11](#l11) | Phân quyền nhiều tenant | Bắt buộc | Code + test + log/metrics |
| [L12](#l12) | Triển khai và rollback an toàn | Bắt buộc | Code + test + log/metrics |
| [L13](#l13) | Distributed tracing và SLO | Mở rộng | Code + test + ADR/đối soát |
| [L14](#l14) | Restore và disaster recovery | Mở rộng | Code + test + ADR/đối soát |
| [L15](#l15) | HLD, ADR, capacity estimation | Mở rộng | Code + test + ADR/đối soát |
| [L16](#l16) | Chứng minh khi nào không dùng Event Sourcing | Mở rộng | Code + test + ADR/đối soát |

## 2. Đề bài chi tiết và tiêu chí nghiệm thu

<h3 id="l01">L01 — Điều tra HTTP chậm và ThreadPool</h3>

**Stack:** ASP.NET Core + HttpClient + OpenTelemetry + dotnet-counters + k6/NBomber.

**Thực hiện:** Tạo endpoint I/O-bound, CPU-bound và endpoint blocking chỉ dùng trong lab; đo p95/p99, ThreadPool queue, CPU, memory và dependency duration. Nếu chưa dựng DNS/TLS fault thật, ghi rõ simulated latency; không tuyên bố đã tái hiện lỗi DNS/TLS.

**Pass/Fail:** Có trace và giải thích đúng bottleneck; không kết luận chỉ từ một log.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l02">L02 — Refactor payment adapter</h3>

**Stack:** ASP.NET Core + DI + xUnit.

**Thực hiện:** Tách domain và provider; thay mock provider không sửa core; so sánh Clean/Vertical Slice.

**Pass/Fail:** Unit tests chứng minh không phụ thuộc HTTP provider.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l03">L03 — DDD cho OMS, Cash và Gateway</h3>

**Stack:** .NET 8/9 + diagrams.

**Thực hiện:** Vẽ bounded contexts, owner bảng dữ liệu, command/event contract, ACL giữa OMS và Gateway.

**Pass/Fail:** Không có cross-service direct table writes trong kiến trúc đích.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l04">L04 — SQL index, isolation và deadlock</h3>

**Stack:** SQL Server + EF Core.

**Thực hiện:** Đo query plan, gây deadlock có kiểm soát, thử READ COMMITTED và SNAPSHOT, giải thích lock.

**Pass/Fail:** Lưu execution plan, deadlock graph, đo trước/sau.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l05">L05 — Chống double-spend và oversell</h3>

**Stack:** ASP.NET Core + SQL Server + Testcontainers.

**Thực hiện:** 100 yêu cầu đồng thời cùng reserve 80/100 đơn vị; so sánh RowVersion, UPDLOCK, conditional UPDATE.

**Pass/Fail:** Chỉ 1 thành công, không âm số dư; test DB thật, DbContext riêng.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l06">L06 — Outbox, Inbox và Saga recovery</h3>

**Stack:** SQL Server + broker local/compatible để test lặp lại; chạy thêm integration test trên Azure Service Bus thật nếu cần xác minh sessions, DLQ và semantics riêng của dịch vụ Azure.

**Thực hiện:** Kill publisher sau commit; gửi event trùng; crash consumer trước/ sau commit; timeout external API.

**Pass/Fail:** Không mất intent, không lặp side effect DB; UNKNOWN được đối soát.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l07">L07 — API Idempotency và contract evolution</h3>

**Stack:** ASP.NET Core + OpenAPI.

**Thực hiện:** Gửi lại cùng key/payload; gửi key trùng payload khác; version schema không phá client.

**Pass/Fail:** Một business effect; 409 cho payload khác; contract tests pass.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l08">L08 — DLQ, ordering và backpressure</h3>

**Stack:** Azure Service Bus / Event Hubs + Worker.

**Thực hiện:** Gửi duplicate/out-of-order, poison message, backlog vượt tốc độ xử lý.

**Pass/Fail:** Có DLQ replay an toàn, đo queue age và consumer lag.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l09">L09 — Redis cache stampede và stale data</h3>

**Stack:** Redis + SQL Server.

**Thực hiện:** 1000 request cùng miss cache; mô phỏng Redis down; kiểm tra booking vẫn không oversell.

**Pass/Fail:** Không vượt tồn; ghi nhận DB QPS, hit rate, p99.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l10">L10 — Load balancing, graceful shutdown, resilience</h3>

**Stack:** Docker Compose / Kubernetes.

**Thực hiện:** Scale 2–3 instances, kill một instance, mô phỏng provider 503, timeout và retry storm.

**Pass/Fail:** Có p95/p99, error rate, không mất durable work.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l11">L11 — Phân quyền nhiều tenant</h3>

**Stack:** ASP.NET Core Policies + SQL Server.

**Thực hiện:** ClientAdmin thay ID tenant trong URL/body và thử đọc/sửa dữ liệu khác.

**Pass/Fail:** Integration tests 403/404 phù hợp; không rò rỉ dữ liệu.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l12">L12 — Triển khai và rollback an toàn</h3>

**Stack:** Docker + GitHub Actions + Azure.

**Thực hiện:** Deploy bản lỗi health check; xác nhận không promote revision lỗi; rollback.

**Pass/Fail:** Pipeline thất bại đúng; bản Ready trước còn phục vụ.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Bắt buộc** trước khi hoàn thành lộ trình nền tảng.


<h3 id="l13">L13 — Distributed tracing và SLO</h3>

**Stack:** OpenTelemetry + Collector.

**Thực hiện:** Trace HTTP → SQL → Outbox → Broker → Inbox; dashboard queue age, error rate, p99.

**Pass/Fail:** Một correlation ID truy vết được; alert có ngưỡng rõ.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Mở rộng** cho review Solution Architect.


<h3 id="l14">L14 — Restore và disaster recovery</h3>

**Stack:** SQL Server backup + runbook.

**Thực hiện:** Backup, restore, đo mất dữ liệu tối đa và thời gian phục hồi; đối soát lệnh pending.

**Pass/Fail:** RTO/RPO đo thực tế, không chỉ tuyên bố.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Mở rộng** cho review Solution Architect.


<h3 id="l15">L15 — HLD, ADR, capacity estimation</h3>

**Stack:** C4 + Markdown ADR.

**Thực hiện:** Tính RPS, peak, storage, queue drain; so sánh modular monolith vs microservices.

**Pass/Fail:** Có assumptions, số liệu, decision, alternatives và revisit trigger.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Mở rộng** cho review Solution Architect.


<h3 id="l16">L16 — Chứng minh khi nào không dùng Event Sourcing</h3>

**Stack:** CQRS/EDA comparison.

**Thực hiện:** So sánh CRUD + audit + outbox với event-sourced write model cho loyalty.

**Pass/Fail:** ADR nêu chi phí schema evolution, replay, privacy và lợi ích đo được.

**Bằng chứng:** đường dẫn source, lệnh chạy, log test, metrics và kết quả thực tế. **Mở rộng** cho review Solution Architect.


## 3. Bốn dự án tích hợp bắt buộc

| Dự án | Luồng nghiệp vụ | Các lab liên quan | Bất biến cần chứng minh |
|---|---|---|---|
| **Financial Wallet** | Reserve → Transfer → Ledger → Reconcile | L04–L08, L13 | Không double-spend, ledger cân đối |
| **Loyalty Redemption** | Reserve points → Issue voucher → Confirm/Release | L05–L09 | Không phát voucher/đổi điểm trùng |
| **Booking / Ticketing** | Hold seat → Payment → Confirm/Expire | L05–L10 | Không bán hai lần một ghế |
| **Securities OMS** | Order → Buying Power → Gateway → ACK/Fill/Unknown | L03–L08, L13–L15 | Không duplicate submission; xử lý partial fill và unknown |

Mỗi dự án phải có **happy path, duplicate, concurrent requests, provider timeout, worker restart, reconciliation**. Chỉ những scenario phù hợp với nghiệp vụ mới được đánh dấu “áp dụng”.

## 4. Cấu trúc repository lab gợi ý

```text
labs/solution-architect/
├── README.md
├── docker-compose.yml
├── src/
│   ├── Financial.Api/
│   ├── Loyalty.Api/
│   ├── Booking.Api/
│   ├── Trading.Oms.Api/
│   └── Shared.Observability/
├── tests/
│   ├── Concurrency.IntegrationTests/
│   ├── Messaging.IntegrationTests/
│   └── FailureRecovery.IntegrationTests/
└── evidence/
    ├── test-results/
    ├── load-tests/
    ├── traces/
    └── adr/
```

Đây là **cấu trúc đề xuất**, không khẳng định các project hoặc bằng chứng trên đã tồn tại.

## 5. Mẫu báo cáo lab tối thiểu

```markdown
# Lab L05 — Atomic Update
- Mục tiêu: không overspend khi 100 requests đồng thời
- Giả định: balance=100, mỗi request reserve=80
- Môi trường: SQL Server version, .NET version, số instances
- Lệnh chạy: dotnet test ...
- Kết quả: passed/failed, 1 success / 99 rejected (nếu test pass)
- Metrics: p95, p99, lock waits, errors
- Failure injection: timeout, deadlock, worker restart
- Trade-off: conditional update vs rowversion vs pessimistic lock
- Link code / logs / screenshot:
- Giới hạn chưa kiểm chứng:
```

## 6. Điều kiện hoàn thành toàn bộ chương

- [ ] Hoàn thành 12 lab cốt lõi với bằng chứng thực tế.
- [ ] Có 4 dự án tích hợp hoặc tài liệu giải thích rõ phạm vi chưa triển khai.
- [ ] Có tối thiểu một load test và một failure drill trên SQL Server thật.
- [ ] Có C4, sequence diagram, ADR, NFR, SLO và runbook cho OMS.
- [ ] Không tuyên bố production-proven nếu chưa có số liệu production.
