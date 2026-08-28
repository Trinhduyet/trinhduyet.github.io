# References — Module 08 Testing & Code Review

> [← Module overview](README.md)

## Official sources

- [Unit testing .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Integration tests ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [Test ASP.NET Core apps](https://learn.microsoft.com/en-us/aspnet/core/test/overview?view=aspnetcore-10.0)
- [Azure Well-Architected — Continuous testing](https://learn.microsoft.com/en-us/azure/well-architected/operational-excellence/continuous-testing)
- [GitHub Actions](https://docs.github.com/en/actions)

## Downstream delivery context

Testing and review evidence becomes CI/CD quality gates before artifacts are promoted to runtime platforms.

- [DevOps Roadmap — roadmap.sh](https://roadmap.sh/devops)
- [Kubernetes Roadmap — roadmap.sh](https://roadmap.sh/kubernetes)
- [DevOps → Kubernetes Production Delivery](../13-devops-iac/devops-kubernetes-production-delivery.md)

The roadmaps are used for learning-path coverage; testing/framework behavior remains grounded in official documentation and local evidence.

## Repository path

```text
Testing & Code Review
→ Security / Performance
→ Docker
→ DevOps & IaC
→ Kubernetes when justified
```

Related:

- [DevOps & IaC](../13-devops-iac/README.md)
- [Kubernetes](../15-kubernetes/README.md)

## Source decisions

| Decision | Source class | Rule |
|---|---|---|
| framework/test behavior | official docs | prefer normative/current source |
| production trade-off | official guidance + measured evidence | don't promote a tutorial result to guarantee |
| quality gate | risk/contract + reproducible test | gate must protect a known failure class |
| deployment compatibility | test + runtime evidence | include old/new version and schema/config compatibility where relevant |

## Verification metadata

- Verified: 2026-08-28.
- Updated to connect test/review evidence with DevOps/Kubernetes delivery gates.
