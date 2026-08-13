# References — Module 24 System Design

> [← Module overview](README.md)

## Scope / roadmap

- [roadmap.sh — System Design Roadmap](https://roadmap.sh/system-design)

roadmap.sh được dùng để kiểm tra **scope**: databases, CDNs, load balancers, caches, proxies, queues, web/application servers, search, logging/monitoring, scalability và security. Normative/production behavior phải được verify bằng official sources.

---

## Official architecture guidance

### Microsoft Azure Architecture Center

- [Design principles for Azure applications](https://learn.microsoft.com/en-us/azure/architecture/guide/design-principles/)
- [Design to scale out](https://learn.microsoft.com/en-us/azure/architecture/guide/design-principles/scale-out)
- [Cloud Design Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)
- [Load balancing options](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/load-balancing-overview)
- [Caching guidance](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching)
- [Cache-Aside pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [Queue-Based Load Leveling pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling)
- [Sharding pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/sharding)
- [Data partitioning strategies](https://learn.microsoft.com/en-us/azure/architecture/best-practices/data-partitioning-strategies)

### Azure Well-Architected Framework — Reliability

- [Reliability quick links / principles](https://learn.microsoft.com/en-us/azure/well-architected/resiliency/overview)
- [Define reliability targets: SLI, SLO, SLA, RTO, RPO](https://learn.microsoft.com/en-us/azure/well-architected/reliability/metrics)
- [Reliability design patterns](https://learn.microsoft.com/en-us/azure/well-architected/reliability/design-patterns)
- [Reliability testing strategy](https://learn.microsoft.com/en-us/azure/well-architected/reliability/testing-strategy)
- [Disaster recovery design guide](https://learn.microsoft.com/en-us/azure/well-architected/design-guides/disaster-recovery)
- [Mission-critical design methodology](https://learn.microsoft.com/en-us/azure/well-architected/mission-critical/mission-critical-design-methodology)

---

## Related repository modules

System Design không lặp lại toàn bộ lower-level material. Deep-dive các mechanics ở:

- [Module 05 — SQL](../05-sql/README.md)
- [Module 06 — API Design](../06-api-design/README.md)
- [Module 11 — Redis & Caching](../11-redis-caching/README.md)
- [Module 15 — Kubernetes](../15-kubernetes/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices Architecture](../18-microservices-architecture/README.md)
- [Module 19 — AI Engineering](../19-ai-engineering/README.md)

---

## User-supplied LinkedIn PDF

Source provided for this update:

- [LinkedIn-hosted System Design PDF](https://media.licdn.com/dms/document/media/v2/D4D1FAQHArSzz4W-VYg/feedshare-document-sanitized-pdf/B4DZ_b.JbQK4BE-/0/1786101931253?e=1787198400&v=beta&t=fbL1Ee-QWmXiSbekQhdY-z2Egms9gtWbD4aQrmHumrg)

**Retrieval note (2026-08-13):** the automated web and download clients could not retrieve the LinkedIn media asset. Therefore this repository does not attribute technical claims to unseen PDF content. Keep the link as supplementary/manual reading; re-run research if a directly downloadable/uploaded copy becomes available.

---

## Source decisions

| Claim type | Preferred source |
|---|---|
| HTTP/API semantics | RFC / official ASP.NET docs |
| database/index/transaction behavior | SQL Server / provider docs |
| cloud pattern semantics | Azure Architecture Center / equivalent official provider docs |
| SLO/RTO/RPO/reliability | Well-Architected / SRE official guidance |
| roadmap topic coverage | roadmap.sh |
| architecture decision | requirements + measured evidence + ADR |

---

## Verification metadata

- Verified: 2026-08-13.
- Source policy: [Module 00 Source Policy](../00-roadmap/source-policy.md).
- English official documentation remains source of truth for current behavior.
- Cloud-vendor examples illustrate general architectural principles; do not turn Azure service names into universal architecture rules.
