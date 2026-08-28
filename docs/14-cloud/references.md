# References — Module 14 Microsoft Azure

> [← Azure overview](README.md)

## Source policy

Module 14 dùng **Microsoft Learn / Azure Architecture Center / Cloud Adoption Framework / Azure Well-Architected Framework** làm source of truth cho Azure-specific behavior.

Pricing, SKU, limits, retirement dates và feature availability thay đổi theo thời gian/region. Vì vậy handbook lưu **cost drivers + sizing model**, không hard-code một bảng giá vĩnh viễn.

Kubernetes đã được tách sang [Module 15](../15-kubernetes/README.md); Kubernetes official documentation nằm trong references của module đó.

---

## Foundations / architecture

- [Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/)
- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
- [Cloud Adoption Framework](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/)
- [Azure landing zones](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/)
- [Technology choices for Azure solutions](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/technology-choices-overview)
- [Choose an Azure compute service](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/compute-decision-tree)
- [Choose an Azure container service](https://learn.microsoft.com/en-us/azure/architecture/guide/choose-azure-container-service)
- [Cloud design patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)

---

## Compute

### App Service

- [App Service plans](https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans)
- [App Service networking](https://learn.microsoft.com/en-us/azure/app-service/networking-features)
- [App Service reliability](https://learn.microsoft.com/en-us/azure/reliability/reliability-app-service)

### Functions

- [Azure Functions hosting options](https://learn.microsoft.com/en-us/azure/azure-functions/functions-scale)
- [Flex Consumption plan](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan)

### Container Apps

- [Container Apps plans / structure](https://learn.microsoft.com/en-us/azure/container-apps/structure)
- [Container Apps billing](https://learn.microsoft.com/en-us/azure/container-apps/billing)
- [Container Apps networking](https://learn.microsoft.com/en-us/azure/container-apps/networking)

### Virtual Machines

- [Azure Virtual Machines documentation](https://learn.microsoft.com/en-us/azure/virtual-machines/)
- [VM sizes](https://learn.microsoft.com/en-us/azure/virtual-machines/sizes/overview)
- [Virtual Machine Scale Sets](https://learn.microsoft.com/en-us/azure/virtual-machine-scale-sets/overview)

> AKS is intentionally not taught here. See [Module 15 — Kubernetes](../15-kubernetes/README.md).

---

## Data / storage / cache

### Azure SQL

- [Azure SQL Database overview](https://learn.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview?view=azuresql-db)
- [vCore vs DTU purchasing models](https://learn.microsoft.com/en-us/azure/azure-sql/database/purchasing-models?view=azuresql)
- [Serverless compute tier](https://learn.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-overview?view=azuresql)
- [Business continuity](https://learn.microsoft.com/en-us/azure/azure-sql/database/business-continuity-high-availability-disaster-recover-hadr-overview?view=azuresql)

### Cosmos DB

- [Cosmos DB request units](https://learn.microsoft.com/en-us/azure/cosmos-db/request-units)
- [Partitioning overview](https://learn.microsoft.com/en-us/azure/cosmos-db/partitioning-overview)
- [Cosmos DB reliability](https://learn.microsoft.com/en-us/azure/reliability/reliability-cosmos-db-nosql)

### Blob Storage

- [Blob access tiers](https://learn.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview)
- [Azure Storage redundancy](https://learn.microsoft.com/en-us/azure/storage/common/storage-redundancy)
- [Blob lifecycle management](https://learn.microsoft.com/en-us/azure/storage/blobs/lifecycle-management-overview)

### Redis

- [Azure Managed Redis overview](https://learn.microsoft.com/en-us/azure/redis/overview)
- [Azure Managed Redis architecture](https://learn.microsoft.com/en-us/azure/redis/architecture)
- [Azure Cache for Redis migration/retirement information](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-whats-new)

---

## Messaging / events

- [Compare Service Bus, Event Grid and Event Hubs](https://learn.microsoft.com/en-us/azure/service-bus-messaging/compare-messaging-services)
- [Service Bus quotas and limits](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quotas)
- [Service Bus reliability](https://learn.microsoft.com/en-us/azure/reliability/reliability-service-bus)
- [Event Grid overview](https://learn.microsoft.com/en-us/azure/event-grid/overview)
- [Event Hubs scalability](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-scalability)
- [Event Hubs features/tier overview](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-features)

---

## Networking / edge

### DNS / VNet / private connectivity

- [Azure DNS overview](https://learn.microsoft.com/en-us/azure/dns/dns-overview)
- [Azure DNS Private Resolver](https://learn.microsoft.com/en-us/azure/dns/dns-private-resolver-overview)
- [Virtual Network overview](https://learn.microsoft.com/en-us/azure/virtual-network/virtual-networks-overview)
- [Network Security Groups](https://learn.microsoft.com/en-us/azure/virtual-network/network-security-groups-overview)
- [Private Link](https://learn.microsoft.com/en-us/azure/private-link/private-link-overview)
- [NAT Gateway resource](https://learn.microsoft.com/en-us/azure/nat-gateway/nat-gateway-resource)

### Traffic / gateway / WAF

- [Azure Load Balancer](https://learn.microsoft.com/en-us/azure/load-balancer/load-balancer-overview)
- [Application Gateway v2](https://learn.microsoft.com/en-us/azure/application-gateway/overview-v2)
- [Application Gateway pricing model](https://learn.microsoft.com/en-us/azure/application-gateway/understanding-pricing)
- [Azure Front Door](https://learn.microsoft.com/en-us/azure/frontdoor/)
- [WAF on Front Door](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/afds-overview)
- [Front Door classic retirement/mapping](https://learn.microsoft.com/en-us/azure/frontdoor/tier-mapping)

### DDoS / firewall / hybrid

- [Azure DDoS Protection](https://learn.microsoft.com/en-us/azure/ddos-protection/)
- [DDoS tier comparison](https://learn.microsoft.com/en-us/azure/ddos-protection/ddos-protection-sku-comparison)
- [DDoS best practices](https://learn.microsoft.com/en-us/azure/ddos-protection/fundamental-best-practices)
- [Azure Firewall](https://learn.microsoft.com/en-us/azure/firewall/overview)
- [VPN Gateway](https://learn.microsoft.com/en-us/azure/vpn-gateway/vpn-gateway-about-vpngateways)
- [ExpressRoute](https://learn.microsoft.com/en-us/azure/expressroute/expressroute-introduction)

---

## API Management

- [API Management overview](https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts)
- [API Management v2 tiers](https://learn.microsoft.com/en-us/azure/api-management/v2-service-tiers-overview)
- [API Management feature comparison](https://learn.microsoft.com/en-us/azure/api-management/api-management-features)
- [API Management limits](https://learn.microsoft.com/en-us/azure/api-management/service-limits)

---

## Identity / secrets

- [Microsoft Entra architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/identity)
- [Managed identities](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
- [Azure RBAC](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)

---

## Observability

- [Azure Monitor overview](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/overview)
- [Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure Monitor cost and usage](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/cost-usage)
- [Azure Monitor cost optimization](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/best-practices-cost)
- [Estimate Azure Monitor costs](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/cost-estimate)
- [Log Analytics retention](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/data-retention-configure)

---

## Delivery / registry / IaC

- [Azure Container Registry SKUs](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-skus)
- [GitHub Actions for Azure](https://learn.microsoft.com/en-us/azure/developer/github/github-actions)
- [OpenID Connect with Azure login](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect)
- [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview)
- [Terraform on Azure](https://learn.microsoft.com/en-us/azure/developer/terraform/)

---

## Backup / DR

- [Azure Backup](https://learn.microsoft.com/en-us/azure/backup/)
- [Site Recovery overview](https://learn.microsoft.com/en-us/azure/site-recovery/site-recovery-overview)
- [Site Recovery recovery plans](https://learn.microsoft.com/en-us/azure/site-recovery/recovery-plan-overview)
- [Disaster recovery design guide](https://learn.microsoft.com/en-us/azure/well-architected/design-guides/disaster-recovery)
- [Azure reliability](https://learn.microsoft.com/en-us/azure/reliability/)

---

## Cost / FinOps

- [Cost Management + Billing](https://learn.microsoft.com/en-us/azure/cost-management-billing/cost-management-billing-overview)
- [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)
- [Cost Optimization — Well-Architected](https://learn.microsoft.com/en-us/azure/well-architected/cost-optimization/)
- [Azure Advisor cost recommendations](https://learn.microsoft.com/en-us/azure/advisor/advisor-cost-recommendations)

---

## Related repository modules

- [Module 13 — DevOps & IaC](../13-devops-iac/README.md)
- [Module 15 — Kubernetes](../15-kubernetes/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices](../18-microservices-architecture/README.md)
- [Module 24 — System Design](../24-system-design/README.md)
- [Module 25 — Software Architecture](../25-software-architecture/README.md)

## Verification metadata

- Verified: 2026-08-28.
- Application Gateway v1 retired 2026-04-28; handbook uses v2 guidance.
- Azure Front Door classic is on retirement path; handbook uses Standard/Premium for new design.
- Current App Service tiers include Premium v4 where available.
- Container Apps uses workload-profile based Consumption/Dedicated models.
- Azure SQL vCore/DTU and serverless purchasing models checked against current docs.
- Azure Monitor logs ingestion remains a major cost dimension and requires explicit retention/sampling design.
- Live pricing and regional availability must be checked again before provisioning.
