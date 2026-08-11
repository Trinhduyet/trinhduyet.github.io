# Research report — initial roadmap baseline

## Topic researched

- Current roadmap.sh catalog and target-role coverage.
- Current production baselines for .NET/backend/platform/AI technologies.
- Official Linux, Git, networking and .NET hosting sources for the first module.

## Official sources found

- .NET support policy and Microsoft Learn.
- SQL Server release/lifecycle pages.
- Docker Engine and Kubernetes release pages.
- Terraform install/release page.
- Redis official docs and upstream releases.
- OpenTelemetry language status and .NET releases.
- Microsoft.Extensions.AI NuGet/docs, Microsoft Agent Framework and OpenAI .NET releases.
- RFC Editor for HTTP; Linux man-pages; official Git docs.

## Versions checked

See [technology-baseline.md](technology-baseline.md). Important snapshots:

- .NET/ASP.NET Core/EF Core 10; .NET 10.0.10 observed.
- SQL Server 2025 CU7 observed.
- Docker Engine 29.7.2.
- Kubernetes 1.36.2 observed.
- Terraform 1.15.8.
- Redis 8.10.0.
- OpenTelemetry .NET 1.17.0.
- Microsoft.Extensions.AI 10.8.3.
- Microsoft Agent Framework .NET 1.17.0.
- OpenAI .NET 2.13.0.

## Context7

- /dotnet/core — .NET 10 LTS/support baseline.
- /dotnet/entityframework.docs — EF Core 10 target/provider compatibility.
- /dotnet/extensions — Microsoft.Extensions.AI abstractions and pipeline focus.

## Conflicting or time-sensitive information

- Kubernetes scheduled the August patch release for 2026-08-11, but at verification time the official release page still listed 1.36.2 and called 1.36.3 the next patch. Decision: target 1.36.x, record 1.36.2 as observed snapshot.
- Redis docs search results lagged the upstream release list. Official GitHub release API/tag showed 8.10.0 on 2026-07-29. Decision: use 8.10.x/8.10.0 snapshot and retain direct upstream release link.
- OpenTelemetry search result initially surfaced 1.15.3, while the official release page listed 1.17.0. Decision: use the directly opened current release page.
- Patch versions change faster than chapters. Decision: separate current patch snapshot from the “latest supported compatible patch” policy.

## Scope comparison decision

roadmap.sh remains a discovery/scope source. Repository order is dependency-driven and adds explicit production, failure, security, cost, evaluation, migration and operability concerns.

## Files created

- Eight required files under docs/00-roadmap.
- This concise research report.
- First module under docs/02-linux-git-networking.

## Next research

Before implementing the .NET module, refresh .NET 10/C# documentation and verify runtime diagnostics APIs. Before SQL, verify SQL Server 2025 compatibility level, Query Store defaults and current tooling against official Microsoft sources.
