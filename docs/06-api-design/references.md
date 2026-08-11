# References — Module 06 API Design

> [← Module overview](README.md)

## Official sources

- [REST API design best practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [ASP.NET Core OpenAPI](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0)
- [HTTP semantics](https://www.rfc-editor.org/rfc/rfc9110)
- [gRPC overview](https://learn.microsoft.com/en-us/aspnet/core/grpc/why-grpc?view=aspnetcore-10.0)
- [API implementation guidance](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-implementation)

## Roadmap and repository

- [Master roadmap](../00-roadmap/master-roadmap.md)
- [Knowledge dependency graph](../00-roadmap/prerequisites.md)
- [Technology baseline](../00-roadmap/technology-baseline.md)
- [Source policy](../00-roadmap/source-policy.md)

## Source decisions

| Decision | Source class | Rule |
| --- | --- | --- |
| Core behavior/protocol | Official documentation/specification | Prefer normative/current source |
| Production trade-off | Official architecture guidance + measured evidence | Do not promote a blog benchmark to a guarantee |
| Version-sensitive API | Current versioned docs | Record version and refresh before deployment |
| Security/operations | Official security/ops guidance + threat model | Validate configuration and failure path |

## Vietnamese Resources

Community Vietnamese material may aid reading, but English official documentation remains canonical for behavior, version and security claims.

## Verification metadata

- Verified: 2026-08-11.
- Technology target: API Design content v1.
- Context7 queries used: none; callable tool unavailable in this run.
- Notes: links are source-of-truth candidates; learner evidence must be produced locally.
