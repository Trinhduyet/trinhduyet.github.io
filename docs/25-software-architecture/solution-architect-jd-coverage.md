# Solution Architect — Ma trận năng lực theo JD chứng khoán

> [← Module 25](README.md) · [Catalog 16 nhóm](solution-architect-technical-catalog.md) · [Domain chứng khoán](securities-domain-architecture.md) · [Quy trình kiến trúc](solution-architecture-delivery-governance.md)

## Phạm vi đánh giá

Đối chiếu JD Solution Architect với tài liệu trên nhánh `main` tại thời điểm review 29/10/2026. **Có tài liệu** không đồng nghĩa **đủ chuyên sâu** hoặc **đã có kinh nghiệm triển khai**. Ma trận phân biệt:

- **Nền tảng:** có bài học/catalog/guide giải thích khái niệm.
- **Bổ sung:** nội dung mới trong bộ tài liệu này để lấp khoảng trống.
- **Thiếu evidence:** chưa có project và kết quả kiểm thử thực tế; không tự nhận đã đạt kinh nghiệm tối thiểu 2 năm hay đã vận hành hệ thống chứng khoán.

## 1. Truy vết mô tả công việc

| Mã | Trách nhiệm JD | Nơi học / bằng chứng cần tạo | Đánh giá |
|---|---|---|---|
| J01 | Phân tích business/technical requirements → solution | [System Design template](system-design-case-study-template.md), [delivery & governance](solution-architecture-delivery-governance.md) | Nền tảng + bổ sung workflow |
| J02 | Hợp tác Product, Security, DevOps, DBA, EDA, Tech Lead | [RACI và design review](solution-architecture-delivery-governance.md) | Bổ sung |
| J03 | Thiết kế HLD → LLD, review application/integration/data | [System Design template](system-design-case-study-template.md), [hồ sơ kiến trúc](solution-architecture-delivery-governance.md) | Nền tảng + bổ sung mẫu bàn giao |
| J04 | Tích hợp nội bộ, đối tác, tổ chức ngoài | [Event-driven/CQRS](event-driven-cqrs-and-integration.md), [securities gateways](securities-domain-architecture.md) | Nền tảng + bổ sung failure semantics |
| J05 | Đánh giá công nghệ, framework, POC | [ADR & fitness](architecture-decisions-evolution-and-fitness-functions.md), [POC scorecard](solution-architecture-delivery-governance.md) | Bổ sung tiêu chí quyết định |
| J06 | Review transaction, security, performance, scalability, availability, operations | [Architecture Review Playbook](architecture-review-playbook.md) | Có nền tảng; thiếu kết quả review thật |
| J07 | Architecture deviation, technical risk/debt, conformance | [Governance workflow](solution-architecture-delivery-governance.md) | Bổ sung |
| J08 | Standard, guideline, reference architecture | [Governance workflow](solution-architecture-delivery-governance.md), [case study](solution-architect-dotnet-case-studies.md) | Bổ sung quy trình; thiếu reference chạy được |

## 2. Truy vết yêu cầu kỹ thuật

| Mã | Năng lực JD | Nguồn học chính | Mức độ / khoảng trống |
|---|---|---|---|
| T01 | Backend .NET hoặc Java/Spring, Web/Mobile là lợi thế | [.NET](../03-dotnet/README.md), [ASP.NET Core](../07-aspnet-core/README.md), [API](../06-api-design/README.md) | Tập trung .NET; Java/Spring và mobile không phải trọng tâm |
| T02 | Architecture, microservices, distributed, EDA, DDD | [DDD](ddd-modular-monolith-and-microservices.md), [Event-driven](event-driven-cqrs-and-integration.md), [Distributed Systems](../17-distributed-systems/README.md) | Có nền tảng; thiếu case chạy end-to-end |
| T03 | Saga, CQRS, Outbox, idempotency, eventual consistency | [Event-driven](event-driven-cqrs-and-integration.md), [Lab L06](solution-architect-mandatory-labs.md#l06) | Có lý thuyết; thiếu failure/replay evidence |
| T04 | Transactions, concurrency, locking, consistency, race | [SQL transactions](../05-sql/transactions-isolation-and-concurrency.md), [Lab L05](solution-architect-mandatory-labs.md#l05) | Có; cần concurrency tests DB thật |
| T05 | Oracle/PostgreSQL/MongoDB hoặc tương đương, data design/optimization | [SQL](../05-sql/README.md), [Data model chứng khoán](securities-domain-architecture.md) | SQL Server là trọng tâm; **chưa đủ sâu vendor-specific Oracle/Postgres/MongoDB** |
| T06 | MQ/event streaming, distributed cache, tải lớn | [Messaging](../17-distributed-systems/README.md), [Caching](../11-redis-caching/README.md), [Lab L08/L09](solution-architect-mandatory-labs.md) | Có kiến thức; thiếu broker/load evidence |
| T07 | API Gateway, OAuth2/OIDC, API security | [API](../06-api-design/README.md), [Security](../09-security-devsecops/README.md) | Có nền tảng; cần negative/contract tests |
| T08 | Docker, Kubernetes, CI/CD, on-prem/cloud/hybrid | [Docker](../12-docker/README.md), [Kubernetes](../15-kubernetes/README.md), [DevOps](../13-devops-iac/README.md) | Có; cần hybrid connectivity/deployment drill |
| T09 | HA, scalability, fault tolerance, DR, RTO/RPO | [Catalog nhóm 10/14](solution-architect-technical-catalog.md), [review playbook](architecture-review-playbook.md) | Có; thiếu failover/restore measurement |
| T10 | Logging/monitoring/distributed tracing/observability | [Catalog nhóm 13](solution-architect-technical-catalog.md), [Lab L13](solution-architect-mandatory-labs.md#l13) | Có; thiếu trace/dashboard evidence |
| T11 | Phân tích NFR performance, security, reliability, maintainability | [Quality attributes](quality-attributes-boundaries-and-styles.md), [System Design template](system-design-case-study-template.md) | Có; cần NFR budget và sign-off thực tế |

## 3. Truy vết domain chứng khoán

| Mã | Domain | Nội dung tối thiểu | Trạng thái |
|---|---|---|---|
| S01 | Trading/OMS | Order lifecycle, ACK vs fill, partial fill, cancel/replace, dedup, sequence gap | [Bổ sung chuyên sâu](securities-domain-architecture.md) |
| S02 | Tài khoản & tài sản | Account, custody position, cash balance, available vs blocked | [Bổ sung](securities-domain-architecture.md) |
| S03 | Quản lý tiền | Reservation, settlement, double-entry ledger, reconciliation | [Bổ sung](securities-domain-architecture.md); chưa có integration lab |
| S04 | Margin | Collateral, exposure, maintenance ratio, margin call/liquidation policy | [Bổ sung mức kiến trúc](securities-domain-architecture.md) |
| S05 | Phái sinh | Contract, position, margin, mark-to-market, clearing lifecycle | [Bổ sung mức kiến trúc](securities-domain-architecture.md) |
| S06 | Market data | Snapshot/delta, sequence, fan-out, stale data, entitlements | [Bổ sung](securities-domain-architecture.md) |
| S07 | Portfolio & investment | Holdings, realized/unrealized P&L, corporate actions | [Bổ sung mức kiến trúc](securities-domain-architecture.md) |
| S08 | Fund & bonds | NAV/subscription/redemption; coupon/accrual/maturity | [Bổ sung mức kiến trúc](securities-domain-architecture.md) |

**Giới hạn quan trọng:** Các quy trình, quy tắc giao dịch, clearing, custody và tuân thủ thay đổi theo thị trường/sở giao dịch/đơn vị và phải xác minh với nghiệp vụ, pháp chế, tích hợp thực tế. Những mô hình ở đây là tham khảo kiến trúc, không phải đặc tả tuân thủ hoặc giao thức sở giao dịch.

## 4. Kỹ năng bổ trợ và tiêu chí đánh giá

| Kỹ năng | Artifact chứng minh |
|---|---|
| Chuyển Business Requirement thành kiến trúc khả thi | Capability map, FR/NFR, context diagram, traceability FR → API → component → test |
| Phân tích end-to-end, trade-off theo chi phí, nhân lực, timeline, vận hành | ADR so sánh ≥2 lựa chọn, cost/capacity assumptions, decision owners |
| Tài liệu và bảo vệ phương án | HLD, LLD, C4, sequence, data model, threat model, review minutes, sign-off và action list |
| Technical leadership / mentoring | Review rubric, coding standards, workshop, code-review feedback và knowledge-transfer evidence |

## 5. Những gì vẫn chưa thể gọi là “đầy đủ”

1. **Kinh nghiệm 2 năm và thực chiến large-scale/securities** là yêu cầu nghề nghiệp, không thể đáp ứng chỉ bằng đọc tài liệu.
2. Chưa có **case chứng khoán triển khai hoàn chỉnh**: API .NET + database + broker + gateway giả lập + test các race + trace + deployment + recovery evidence.
3. Vendor-specific Oracle/PostgreSQL/MongoDB và giao thức/chuẩn của từng đối tác chưa được dạy chuyên sâu.
4. Chưa có hồ sơ đã được Product/Security/DBA/DevOps/EDA phê duyệt hoặc review POC thực tế.
5. Chưa có số đo RTO/RPO, SLO, load test, DR drill của reference architecture.

## 6. Lộ trình đóng khoảng trống

- **P0:** Hoàn thiện workflow review/governance; xây dựng case OMS gồm order/asset/cash/market data và các API/data contract; chạy L05/L06/L08.
- **P1:** Margin & derivatives flow, account/portfolio reconciliation, integration security, load/failover/restore tests và ADR theo workload.
- **P2:** Fund/bond lifecycle, Oracle/PostgreSQL/MongoDB portability workshop, hybrid connectivity thực tế.

Áp dụng chuẩn: **Yêu cầu → Thiết kế → Quyết định → Triển khai → Failure test → Evidence → Review/sign-off**. Mọi kết quả chưa chạy phải ghi là kế hoạch, không trình bày như đã kiểm chứng.
