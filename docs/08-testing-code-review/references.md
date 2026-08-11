# References — Module 08 Testing và Code Review

> [← Module overview](README.md)

## Official sources

- [Unit testing .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Integration tests ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [Test ASP.NET Core apps](https://learn.microsoft.com/en-us/aspnet/core/test/overview?view=aspnetcore-10.0)
- [Continuous testing](https://learn.microsoft.com/en-us/azure/well-architected/operational-excellence/continuous-testing)

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
- Technology target: Testing và Code Review content v1.
- Context7 queries used: none; callable tool unavailable in this run.
- Notes: links are source-of-truth candidates; learner evidence must be produced locally.
