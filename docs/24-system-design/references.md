# References — Module 24 System Design

> [← Module overview](README.md)

## Source policy

System Design có hai lớp nguồn:

1. **Scope / learning breadth** — roadmap/course/community resources giúp kiểm tra chủ đề có bị thiếu không.
2. **Normative / production behavior** — protocol specs, database/provider docs và official architecture guidance dùng để xác minh behavior hiện tại.

Không dùng một interview diagram làm production guarantee.

## User-supplied system-design sources

### Karan Pratap Singh — System Design

- [karanpratapsingh/system-design](https://github.com/karanpratapsingh/system-design)

Coverage hữu ích để audit roadmap:

```text
Networking
IP / OSI / TCP / UDP / DNS
Load balancing / clustering / caching / CDN / proxy
Availability / scalability / storage

Data
SQL / NoSQL
replication / indexes
ACID / BASE / CAP / PACELC
transactions / distributed transactions
sharding / consistent hashing

Architecture
message brokers / queues / pub-sub
monolith / microservices
EDA / event sourcing / CQRS
API gateway / REST / GraphQL / gRPC
WebSockets / SSE

Reliability / Security
circuit breaker / rate limit / discovery
SLA / SLO / SLI / DR
OAuth / OIDC / SSO / TLS / mTLS

Cases
URL shortener / WhatsApp / Twitter / Netflix / Uber
```

Course này chuyển breadth đó thành reasoning `problem → capacity → data → failure → evidence → trade-off`.

### Ashish Pratap Singh — Awesome System Design Resources

- [ashishps1/awesome-system-design-resources](https://github.com/ashishps1/awesome-system-design-resources)

Dùng để kiểm tra coverage cho:

```text
scalability / reliability / SPOF
latency-throughput-bandwidth
network/API/database/cache fundamentals
pub-sub / queues / CDC
distributed locking / consensus / tracing
architecture patterns
system-design trade-offs
interview cases
engineering articles / distributed-system papers
```

### Mehdi Hadeli — Awesome Software Architecture

- [mehdihadeli/awesome-software-architecture](https://github.com/mehdihadeli/awesome-software-architecture)

Dùng cho overlap System Design ↔ Software Architecture:

```text
architecture styles
DDD / CQRS / EDA
microservices / modular monolith
cloud patterns
messaging / distributed transactions
caching / sharding / database
backpressure / service discovery / load balancing
```

## Official architecture guidance

### Microsoft Azure Architecture Center

- [Design principles for Azure applications](https://learn.microsoft.com/en-us/azure/architecture/guide/design-principles/)
- [Technology choices](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/technology-choices-overview)
- [Cloud Design Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)
- [Load balancing options](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/load-balancing-overview)
- [Caching guidance](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching)
- [Cache-Aside pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [Queue-Based Load Leveling pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling)
- [Sharding pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/sharding)
- [Data partitioning strategies](https://learn.microsoft.com/en-us/azure/architecture/best-practices/data-partitioning-strategies)

### Azure Well-Architected Framework

- [Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
- [Reliability principles](https://learn.microsoft.com/en-us/azure/well-architected/reliability/)
- [Define SLI, SLO, SLA, RTO, RPO](https://learn.microsoft.com/en-us/azure/well-architected/reliability/metrics)
- [Reliability testing strategy](https://learn.microsoft.com/en-us/azure/well-architected/reliability/testing-strategy)
- [Disaster recovery design guide](https://learn.microsoft.com/en-us/azure/well-architected/design-guides/disaster-recovery)

## Important distributed-systems papers / engineering sources

The user-supplied awesome-system-design repository points to classic papers such as Paxos, MapReduce, GFS, Dynamo, Kafka, Spanner, Bigtable, ZooKeeper, LSM-tree and Chubby. Use original papers when deep-diving the actual algorithm/system design rather than relying on second-hand summaries.

Similarly, engineering case studies from companies such as Discord, Netflix, Canva, Airbnb, Stripe and Slack are useful because they expose **real workload evolution and trade-offs**; treat them as context-specific evidence, not universal blueprints.

## Related repository modules

- [Module 02 — Networking](../02-linux-git-networking/README.md)
- [Module 05 — SQL](../05-sql/README.md)
- [Module 06 — API Design](../06-api-design/README.md)
- [Module 11 — Redis & Caching](../11-redis-caching/README.md)
- [Module 14 — Cloud & Azure](../14-cloud/README.md)
- [Module 15 — Kubernetes](../15-kubernetes/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices Architecture](../18-microservices-architecture/README.md)
- [Module 19 — AI Engineering](../19-ai-engineering/README.md)
- [Module 25 — Software Architecture](../25-software-architecture/README.md)

## Source decisions

| Claim type | Preferred source |
|---|---|
| HTTP/API semantics | RFC / official framework docs |
| database/index/transaction behavior | DB/provider documentation |
| cloud service behavior | current official provider docs |
| SLO/RTO/RPO/reliability | Well-Architected / SRE-style official guidance |
| algorithm/system mechanism | original paper/spec when available |
| roadmap topic coverage | user-supplied course/awesome repos + roadmap sources |
| architecture decision | requirements + measured evidence + ADR |

## Verification metadata

- Updated: 2026-08-19.
- User-supplied repositories are supplementary scope/reading maps.
- Current Azure behavior must follow Microsoft official documentation.
- Every design claim should eventually be backed by workload assumptions, measurements or failure evidence.
