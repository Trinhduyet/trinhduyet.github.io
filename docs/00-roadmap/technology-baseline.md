# Technology baseline

Snapshot này chọn baseline để viết ví dụ và lab. Production project vẫn phải kiểm tra compatibility matrix, cloud/provider support và security advisories trước khi nâng cấp.

## Baseline ngày 2026-08-11

| Technology | Target/Baseline | Support | Verified Date | Official Source |
| --- | --- | --- | --- | --- |
| .NET | 10.0 LTS; observed patch 10.0.10 | Active đến 2028-11-14; phải theo latest servicing patch | 2026-08-11 | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) |
| ASP.NET Core | 10.0, đi cùng .NET 10 | Lifecycle đi cùng .NET 10 | 2026-08-11 | [ASP.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/aspnet) |
| EF Core | 10.0; observed package 10.0.10 | Đi cùng release train .NET 10; provider compatibility phải kiểm tra riêng | 2026-08-11 | [EF Core package](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/10.0.10), [EF Core providers](https://learn.microsoft.com/en-us/ef/core/providers/) |
| SQL Server | SQL Server 2025; observed CU7 / July 2026 GDR | Mainstream đến 2031-01-06; extended đến 2036-01-06 | 2026-08-11 | [Latest SQL Server updates](https://learn.microsoft.com/en-us/troubleshoot/sql/releases/download-and-install-latest-updates), [SQL Server 2025 lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2025) |
| Docker Engine | 29.x; observed 29.7.2 | Upstream current release line; theo security/patch releases | 2026-08-11 | [Docker Engine 29 release notes](https://docs.docker.com/engine/release-notes/29/) |
| Kubernetes | 1.36.x; observed 1.36.2 | Latest supported minor; EOL 2027-06-28 | 2026-08-11 | [Kubernetes releases](https://kubernetes.io/releases/), [patch releases](https://kubernetes.io/releases/patch-releases/) |
| Terraform | 1.15.x; observed 1.15.8 | Current official CLI release; pin constraints và lock providers | 2026-08-11 | [Terraform install](https://developer.hashicorp.com/terraform/install) |
| Redis Open Source | 8.10.x; observed 8.10.0 | Current stable upstream release; review license và patch advisories | 2026-08-11 | [Redis 8.10.0 release](https://github.com/redis/redis/releases/tag/8.10.0), [Redis Docs](https://redis.io/docs/latest/operate/oss_and_stack/) |
| OpenTelemetry .NET | 1.17.x; observed 1.17.0 | Traces, metrics và logs stable; experimental components require separate review | 2026-08-11 | [OpenTelemetry .NET releases](https://github.com/open-telemetry/opentelemetry-dotnet/releases/tag/core-1.17.0), [.NET signal status](https://opentelemetry.io/docs/languages/dotnet/) |
| Microsoft.Extensions.AI | 10.8.x; observed 10.8.3 | Stable package line; pin exact package in executable projects | 2026-08-11 | [Microsoft.Extensions.AI 10.8.3](https://www.nuget.org/packages/Microsoft.Extensions.AI/10.8.3), [.NET AI docs](https://learn.microsoft.com/en-us/dotnet/ai/) |
| Microsoft Agent Framework (.NET) | 1.17.x; observed 1.17.0 | Stable release; APIs evolve faster than .NET LTS | 2026-08-11 | [Agent Framework .NET 1.17.0](https://github.com/microsoft/agent-framework/releases/tag/dotnet-1.17.0) |
| OpenAI .NET | 2.13.x; observed 2.13.0 | Official SDK stable release; model availability is a separate deployment concern | 2026-08-11 | [OpenAI .NET 2.13.0](https://github.com/openai/openai-dotnet/releases/tag/OpenAI_2.13.0), [official repository](https://github.com/openai/openai-dotnet) |

## Quyết định baseline

- Dùng .NET 10 LTS cho ví dụ mới. Không dạy preview .NET 11 như production default.
- ASP.NET Core và EF Core dùng major 10. EF Core provider phải hỗ trợ major tương ứng trước khi project nâng cấp.
- SQL Server 2025 là implementation chính; chương khái niệm vẫn bắt đầu từ relational model chuẩn.
- Docker/Kubernetes/Terraform/Redis dùng major/minor hiện hành nhưng lab pin artifact đủ chặt để tái lập.
- OpenTelemetry core stable không đồng nghĩa mọi exporter, instrumentation hoặc semantic convention đều stable.
- AI packages thay đổi nhanh hơn runtime. Module AI phải ghi exact package/model/config snapshot và bắt buộc eval gate.

## Snapshot so với policy

Số patch trong bảng là bằng chứng “đã thấy tại thời điểm xác minh”, không phải hard-coded target lâu dài:

~~~text
Documentation target = supported major/minor + latest compatible security patch
Reproducible lab      = exact version or digest
Production upgrade    = compatibility + test + rollout + rollback
~~~

Riêng Kubernetes, trang release tại thời điểm kiểm tra vẫn ghi 1.36.2 là latest và 1.36.3 là “next patch”, trong khi lịch dự kiến đặt patch tháng 8 vào đúng 2026-08-11. Vì vậy baseline chọn 1.36.x và ghi snapshot 1.36.2 thay vì tuyên bố patch chưa được xuất bản.

## Stable concept và version-specific detail

| Stable concept | Version-specific detail cần tái xác minh |
| --- | --- |
| Dependency Injection | API đăng ký service/middleware |
| HTTP semantics | Kestrel protocol/endpoint configuration |
| Transactions và isolation | SQL Server defaults, engine feature và compatibility level |
| Desired state/reconciliation | Kubernetes API version, field và feature gate |
| Telemetry context propagation | SDK/exporter/semantic convention maturity |
| Tool calling và authorization | Agent framework API, provider capability và model behavior |

## Chu kỳ refresh

- Monthly: .NET servicing, Docker, Kubernetes, Redis, OpenTelemetry, AI SDKs và security advisories.
- Quarterly: Terraform, SQL Server CU/GDR, roadmap.sh catalog và provider compatibility.
- Trước mỗi module: refresh đúng technology liên quan, không chờ lịch chung.
- Trước production lab: pin exact versions/digests và lưu output của version commands.

## Context7 verification

| Library ID | Query | Kết quả dùng |
| --- | --- | --- |
| /dotnet/core | .NET 10 production baseline, LTS, servicing và end-of-support | Xác nhận .NET 10 LTS, 10.0.10 và EOL 2028-11-14 |
| /dotnet/entityframework.docs | EF Core 10 target, release/support alignment và provider caution | Xác nhận net10.0/package major 10; provider compatibility cần kiểm tra riêng |
| /dotnet/extensions | Microsoft.Extensions.AI 10 stable baseline và abstractions | Xác nhận IChatClient/IEmbeddingGenerator và pipeline utilities là trọng tâm abstraction |

## Verification metadata

- Verified: 2026-08-11, Asia/Saigon
- Technology version: xem bảng baseline
- Official sources: link trực tiếp trong bảng
- Context7 queries used: bảng Context7 verification
- Notes: GitHub release API và NuGet V3 index được dùng để kiểm tra tag/package hiện hành; official lifecycle pages quyết định support status.
