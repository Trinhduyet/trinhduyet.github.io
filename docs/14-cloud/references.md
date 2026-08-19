# References — Module 14 Cloud & Microsoft Azure

> [← Module overview](README.md)

## Source policy

Azure service behavior, limits, retirement timelines, security controls và architecture recommendations có thể thay đổi. Vì vậy module này dùng **Microsoft Learn / Azure Architecture Center / Cloud Adoption Framework / Azure Well-Architected Framework** làm source of truth cho Azure-specific claims.

Community/awesome repositories chỉ dùng để kiểm tra breadth và tìm topic cần học, không dùng thay official behavior documentation.

## Azure foundations / landing zones

- [Azure landing zones — Cloud Adoption Framework](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/)
- [Azure landing zone design areas](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/design-areas)
- [Azure landing zone design principles](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/design-principles)
- [Management groups — Cloud Adoption Framework](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/design-area/resource-org-management-groups)

## Architecture / technology choices

- [Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/)
- [Technology choices for Azure solutions](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/technology-choices-overview)
- [Choose an Azure compute service](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/compute-decision-tree)
- [Choose an Azure container service](https://learn.microsoft.com/en-us/azure/architecture/guide/choose-azure-container-service)
- [Choose compute for microservices](https://learn.microsoft.com/en-us/azure/architecture/microservices/design/compute-options)
- [Cloud design patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)

## Identity / networking / security

- [Azure identity architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/identity)
- [Managed identities for Azure resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
- [Azure role-based access control](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- [Azure Private Link](https://learn.microsoft.com/en-us/azure/private-link/private-link-overview)
- [Azure network security](https://learn.microsoft.com/en-us/azure/security/fundamentals/network-overview)

## Data / cache / messaging

- [Choose a data store — Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/data-store-overview)
- [Compare Azure messaging services: Event Grid, Event Hubs, Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/compare-messaging-services)
- [Integration architecture — Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/integration/integration-get-started)
- [Azure Managed Redis](https://learn.microsoft.com/en-us/azure/redis/overview)
- [Azure Cache for Redis retirement / migration notice](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-whats-new)

> **Current note (verified 2026-08-19):** Microsoft recommends Azure Managed Redis for new Redis workloads; Azure Cache for Redis has announced retirement timelines. Re-check official migration/retirement documentation before a production decision.

## Reliability / Well-Architected

- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
- [What is the Azure Well-Architected Framework?](https://learn.microsoft.com/en-us/azure/well-architected/what-is-well-architected-framework)
- [Reliability overview](https://learn.microsoft.com/en-us/azure/well-architected/reliability/)
- [Define reliability targets: SLI, SLO, SLA, RTO, RPO](https://learn.microsoft.com/en-us/azure/well-architected/reliability/metrics)
- [Disaster recovery design guide](https://learn.microsoft.com/en-us/azure/well-architected/design-guides/disaster-recovery)
- [Azure reliability](https://learn.microsoft.com/en-us/azure/reliability/)

## Observability / operations / cost

- [Azure Monitor](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/overview)
- [Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Cost Management](https://learn.microsoft.com/en-us/azure/cost-management-billing/cost-management-billing-overview)
- [Azure Policy](https://learn.microsoft.com/en-us/azure/governance/policy/overview)

## Supplementary architecture scope

User-supplied repository used as a curated breadth checklist:

- [mehdihadeli/awesome-software-architecture](https://github.com/mehdihadeli/awesome-software-architecture)

Useful scope from that repository includes cloud design patterns, cloud best practices, cloud native, PaaS/IaaS, reverse proxy/load balancing, service discovery, service mesh, messaging, distributed transactions, caching, databases and Microsoft Azure Cloud. Nội dung course này được viết lại theo problem → failure → trade-off thay vì sao chép danh sách links.

## Related repository modules

- [Module 13 — DevOps & IaC](../13-devops-iac/README.md)
- [Module 15 — Kubernetes](../15-kubernetes/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices](../18-microservices-architecture/README.md)
- [Module 24 — System Design](../24-system-design/README.md)
- [Module 25 — Software Architecture](../25-software-architecture/README.md)

## Verification metadata

- Verified: 2026-08-19.
- Azure-specific claims: prefer Microsoft official documentation.
- Community repositories: supplementary discovery/coverage only.
- Learner evidence still phải được tạo bằng code, deployment, metrics, failure drills và ADR thực tế.
