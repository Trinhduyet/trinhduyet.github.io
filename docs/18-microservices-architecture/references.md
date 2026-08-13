# References — Module 18 Microservices Architecture

> [← Module overview](README.md)

## Official sources

- [Microservices architecture style — Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/microservices/)
- [Design a microservices architecture](https://learn.microsoft.com/en-us/azure/architecture/microservices/design/)
- [Microservices design patterns](https://learn.microsoft.com/en-us/azure/architecture/microservices/design/patterns)
- [.NET Microservices architecture](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/microservices-architecture)
- [Data sovereignty per microservice](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice)
- [API gateways in microservices](https://learn.microsoft.com/en-us/azure/architecture/microservices/design/gateway)
- [API gateway vs direct client communication](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/direct-client-to-microservice-communication-versus-the-api-gateway-pattern)
- [Asynchronous message-based communication](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
- [Microservices assessment and readiness](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/microservices-assessment)
- [Architecture styles](https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/)

## Related repository modules

- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Partial Failure, Timeouts, Retries & Idempotency](../17-distributed-systems/partial-failure-timeouts-retries-and-idempotency.md)
- [Messaging, Outbox, Inbox & Dedup](../17-distributed-systems/messaging-outbox-inbox-and-dedup.md)
- [Consistency, Ordering, Saga & Backpressure](../17-distributed-systems/consistency-ordering-saga-and-backpressure.md)
- [Module 15 — Kubernetes](../15-kubernetes/README.md)
- [Module 07 — ASP.NET Core](../07-aspnet-core/README.md)

## Source policy

- Official English documentation is canonical for platform behavior and current guidance.
- Microservices architecture recommendations are contextual, not universal rules.
- Code examples in this module are learning/architecture shapes; adapt provider-specific retry, broker ACK, database locking and security details to the actual stack.
- A pattern is only justified after requirements, failure semantics, operational ownership and cost are understood.

## Verification metadata

- Verified: 2026-08-13.
- Source class: Microsoft Learn / Azure Architecture Center / .NET Architecture guidance.
