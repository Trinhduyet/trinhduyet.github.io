# Module 04 — Backend Request Lifecycle và Production Boundaries

> [← Module 03](../03-dotnet/README.md) · [Roadmap](../00-roadmap/README.md)

Module này nối runtime execution với backend application behavior: một HTTP request đi qua middleware, routing, binding, validation, authorization, handler, downstream call và response như thế nào. Trọng tâm là contract, failure boundary, abuse resistance và evidence; SQL/EF Core được mở ở Module 05.

## Module trong một hình

![Sơ đồ Readme — diagram 1](../assets/diagrams/04-backend-readme-1.svg)

## Phạm vi

| Learning slice | Priority | Evidence |
| --- | --- | --- |
| [Request lifecycle và endpoint contract](request-lifecycle-and-endpoint-contract.md) | P0 | request trace |
| [Authentication, authorization và validation](authentication-authorization-and-validation.md) | P0 | authn/authz matrix |
| [Pagination, idempotency, rate limiting và caching](pagination-idempotency-rate-limiting-and-caching.md) | P0 | bounded workload |
| [Background jobs, files và webhooks](background-jobs-files-and-webhooks.md) | P0 | retry/replay/failure note |
| [BackendLab source](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/04-backend/backend-lab) | P0 | build + run report |

## Dependency map

![Sơ đồ Readme — diagram 2](../assets/diagrams/04-backend-readme-2.svg)

## Bốn boundary phải giữ rõ

### Request boundary

HTTP input là untrusted và bounded bằng method, route, header, body size, timeout và content type. Không đưa raw request vào domain hoặc log.

### Security boundary

Authentication tạo identity; authorization quyết định action/resource. Policy phải kiểm tra resource và tenant, không chỉ có claim `role=admin`.

### Capacity boundary

Pagination, idempotency, rate limiting, cache và queue là cách phân bổ capacity. Chúng không phải decorator gắn tùy ý sau khi endpoint đã viết xong.

### Integration boundary

HTTP client, file, queue và webhook đều có timeout, retry budget, duplicate, replay và ownership. Không biến request thread thành durable job.

## Cách chạy lab — portable path

Clone repo một lần:

```powershell
git clone https://github.com/Trinhduyet/trinhduyet.github.io.git
cd trinhduyet.github.io
```

Từ **repository root**:

```powershell
cd labs/04-backend/backend-lab
dotnet build -c Release
dotnet run -c Release --no-build -- pagination 10000 100 3
dotnet run -c Release --no-build -- idempotency 1000 10
dotnet run -c Release --no-build -- backpressure 10000 64 25
```

Không dùng absolute path theo máy cá nhân. Forward slash chạy được trong PowerShell/Bash và giữ tài liệu portable giữa Windows, Linux, WSL và CI.

BackendLab là executable experiment, không phải server public. Nó kiểm tra các invariant của boundary bằng workload bounded; production endpoint cần thêm framework hosting, TLS, identity provider, persistence và deployment policy.

![Sơ đồ Readme — diagram 3](../assets/diagrams/04-backend-readme-3.svg)

## Evidence tối thiểu

1. Một request trace có pipeline order và correlation ID.
2. Một matrix authn/authz gồm anonymous, authenticated, wrong tenant và forbidden action.
3. Pagination output ổn định khi dữ liệu thay đổi; duplicate request không duplicate side effect.
4. Rate limit/backpressure experiment có bounded memory và explicit rejection/cancellation.
5. Webhook/integration note có timeout, signature, replay, idempotency và audit.

## Exit criteria

Người học hoàn thành Module 04 khi có thể:

- trace request từ ingress đến response và downstream;
- thiết kế DTO, validation, status code và ProblemDetails không lộ nội bộ;
- tách authentication khỏi authorization và viết policy/resource check;
- chọn offset/cursor pagination, idempotency store, rate limit và cache theo workload;
- thiết kế worker/file/webhook integration có bounded queue, retry/deadline và replay;
- review endpoint qua correctness, security, latency, cost và operability.

## Tiếp tục

Sau Module 04, mở Module 05 — SQL khi contract/persistence boundary đã rõ; không tối ưu LINQ trước khi đọc generated SQL và execution plan.
