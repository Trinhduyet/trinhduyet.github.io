# References — Module 04 Backend

> [← Module overview](README.md)

## Official — ASP.NET Core pipeline and contracts

- [ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware?view=aspnetcore-10.0)
- [Routing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-10.0)
- [Handle requests with controllers](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-10.0)
- [Controller action return types](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-10.0)
- [Model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0)
- [Model validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)
- [Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-10.0)

## Official — Authentication and authorization

- [Overview of ASP.NET Core authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-10.0)
- [Introduction to authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0)
- [Policy-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0)
- [Authentication and authorization in Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-10.0)
- [API endpoint authentication behavior](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/api-endpoint-auth?view=aspnetcore-10.0)

## Official — Capacity and integrations

- [Rate limiting middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)
- [Caching overview](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview?view=aspnetcore-10.0)
- [HTTP requests with IHttpClientFactory](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-10.0)
- [HttpClient guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Background tasks with hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-10.0)
- [Idempotency in integration events](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/subscribe-events)

## Roadmap and baseline

- [Technology baseline](../00-roadmap/technology-baseline.md)
- [Knowledge dependency graph](../00-roadmap/prerequisites.md)
- [Source policy](../00-roadmap/source-policy.md)

## Source decisions

| Câu hỏi | Nguồn quyết định | Boundary |
| --- | --- | --- |
| Middleware order | ASP.NET Core middleware docs | Order affects security, performance and short-circuiting |
| Binding/validation | Model binding and validation docs | Binding errors differ from business validation errors |
| Error contract | ProblemDetails/error handling docs | Clients receive stable errors, not stack traces |
| Authn/authz | Authentication + policy docs | Identity is not permission; resource checks remain application responsibility |
| Rate/cache | Rate limiting and caching docs | Capacity policy must be load-tested and privacy-aware |
| Background/integration | Hosted services + HTTP client docs | Scope, deadline, retry and replay are explicit |

## Vietnamese Resources

Không dùng tutorial community tiếng Việt làm canonical. Dùng language switch của Microsoft Learn để hỗ trợ đọc; behavior và version-sensitive API vẫn đối chiếu bản English.

## Verification metadata

- Verified: 2026-08-11.
- Technology version: ASP.NET Core/.NET 10 target.
- Official sources: links above.
- Context7 queries used: none; callable Context7 tool unavailable in this run.
- Notes: không coi roadmap.sh là nguồn behavior; module giữ SQL/EF Core ở Module 05.
