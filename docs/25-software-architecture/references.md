# References — Module 25 Software Architecture

> [← Software Architecture](README.md)

## User-supplied primary scope repository

- [mehdihadeli/awesome-software-architecture](https://github.com/mehdihadeli/awesome-software-architecture)

Repository này có breadth lớn cho:

```text
Software Architecture
Actor Model
Clean / Onion / Hexagonal / Vertical Slice
Event-Driven Architecture
SOA
DDD
CQRS
Microservices
Modular Monolith
Architectural principles / design patterns
Cloud patterns / cloud native
Load balancing / service discovery / service mesh
Backpressure / eventual consistency
Messaging / distributed transactions / locking
REST / gRPC / caching / sharding / database
Microsoft Azure
```

Course dùng nó để **audit coverage**, không sao chép danh sách tài nguyên hoặc coi mọi pattern là best practice.

## User-supplied System Design overlap

- [karanpratapsingh/system-design](https://github.com/karanpratapsingh/system-design)
- [ashishps1/awesome-system-design-resources](https://github.com/ashishps1/awesome-system-design-resources)

Hai nguồn này bổ sung system forces mà architecture style phải chịu:

```text
networking
availability / scalability
cache / data / replication / sharding
message queues / pub-sub
CQRS / EDA
API gateway
rate limiting / circuit breaker
DR / security
trade-offs / case studies
```

## Primary / canonical references nên ưu tiên khi deep dive

### Domain-Driven Design / architecture concepts

- Original author/book/source material khi một pattern có nguồn gốc rõ ràng.
- Official framework/runtime documentation khi behavior phụ thuộc .NET/Azure/Kubernetes/database.
- Protocol specifications/RFCs cho HTTP/TLS/API semantics.
- Original distributed-system papers cho algorithm/system mechanics.

### Microsoft architecture guidance

- [Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/)
- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
- [.NET architecture guidance](https://learn.microsoft.com/en-us/dotnet/architecture/)

## Related modules in this repository

- [Module 06 — API Design](../06-api-design/README.md)
- [Module 14 — Cloud & Azure](../14-cloud/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices Architecture](../18-microservices-architecture/README.md)
- [Module 24 — System Design](../24-system-design/README.md)

## Source-use rule

| Need | Source |
|---|---|
| learn breadth / discover terms | awesome/course repositories |
| exact service/runtime behavior | official current docs |
| algorithm/distributed mechanism | original paper/spec |
| architecture decision | requirements + evidence + ADR |
| production confidence | tests + metrics + failure drills + operations evidence |

## Verification metadata

- Module activated: 2026-08-19.
- User-supplied repos used as supplementary coverage maps.
- Architecture recommendations in this site are framed as context-dependent decisions, not universal pattern rules.

## Additional reviewed system-design learning repositories (2026-10-19)

- [liquidslr/system-design-notes](https://github.com/liquidslr/system-design-notes): structured case studies, estimation, scaling, consistent hashing, rate limiting, job systems. Repository uses topic directories rather than a root README.
- [ByteByteGoHq/system-design-101](https://github.com/ByteByteGoHq/system-design-101): visual explanations and communication/API/design reference. **CC BY-NC-ND**: link and independently explain; do not copy diagrams or adapt licensed assets into this site.
- [karanpratapsingh/system-design](https://github.com/karanpratapsingh/system-design): fundamentals-first networking, databases, distributed designs and interviews.
- [ashishps1/awesome-system-design-resources](https://github.com/ashishps1/awesome-system-design-resources): interview exercises and core design trade-offs. Respect GPL-3.0 when reusing actual source material.
- [mehdihadeli/awesome-software-architecture](https://github.com/mehdihadeli/awesome-software-architecture): architecture patterns and principles curated reference, already covered earlier in this module.

**Editorial rule:** use these repositories as discovery/coverage references only. All new text, diagrams, .NET examples and domain reasoning are independently written. Prefer original vendor specifications for implementation guarantees.
