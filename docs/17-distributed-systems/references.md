# References — Module 17 Distributed Systems

> [← Module overview](README.md)

## Official Microsoft / .NET sources

- [Retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Circuit Breaker pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- [Transient fault handling](https://learn.microsoft.com/en-us/azure/architecture/best-practices/transient-faults)
- [Queue-Based Load Leveling pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling)
- [Sequential Convoy pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/sequential-convoy)
- [Asynchronous message-based communication in .NET microservices](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
- [Cloud Design Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)
- [Transactional outbox guidance](https://learn.microsoft.com/en-us/azure/architecture/databases/guide/transactional-outbox-cosmos)

## How to use these sources

Official pattern documentation explains the stable architectural problem and trade-offs. Actual broker/framework APIs must be checked against the selected product documentation and Context7 where applicable.

Examples in this module are intentionally broker-neutral until a project chooses Azure Service Bus, RabbitMQ, Kafka or another implementation.

## Repository links

- [Master roadmap](../00-roadmap/master-roadmap.md)
- [Technology baseline](../00-roadmap/technology-baseline.md)
- [Source policy](../00-roadmap/source-policy.md)
- [ASP.NET Core resilience](../07-aspnet-core/resilience-security-and-middleware.md)
- [SQL transactions](../05-sql/transactions-isolation-and-concurrency.md)

## Verification metadata

- Verified: 2026-08-12.
- Source class: official Microsoft/Azure architecture and .NET guidance.
- Broker-specific source set: add when a concrete broker implementation is introduced.
