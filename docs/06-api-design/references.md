# References — Module 06 API Design

> [← Module overview](README.md)

Module 06 dùng **official specification/documentation làm source of truth**. Blog/interview material chỉ dùng để tìm checklist, case study và câu hỏi review — không dùng để override protocol/security semantics.

---

# 1. HTTP — normative / primary

- RFC 9110 — HTTP Semantics  
  https://www.rfc-editor.org/rfc/rfc9110
- RFC 9111 — HTTP Caching  
  https://www.rfc-editor.org/rfc/rfc9111
- RFC 9457 — Problem Details for HTTP APIs  
  https://www.rfc-editor.org/rfc/rfc9457
- RFC 6585 — Additional HTTP Status Codes (`428`, `429`, `431`, `511`)  
  https://www.rfc-editor.org/rfc/rfc6585
- IANA HTTP Status Code Registry  
  https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml

Important sections to revisit:

```text
resources / representations
methods
safe vs idempotent
status codes
conditional requests
ETag / validators
content negotiation
authentication fields
```

---

# 2. Authentication, OAuth và tokens — primary

- RFC 6750 — OAuth 2.0 Bearer Token Usage  
  https://www.rfc-editor.org/rfc/rfc6750
- RFC 9700 — Best Current Practice for OAuth 2.0 Security  
  https://www.rfc-editor.org/rfc/rfc9700
- RFC 7636 — Proof Key for Code Exchange (PKCE)  
  https://www.rfc-editor.org/rfc/rfc7636
- RFC 9449 — OAuth 2.0 Demonstrating Proof of Possession (DPoP)  
  https://www.rfc-editor.org/rfc/rfc9449
- OpenID Connect Core 1.0  
  https://openid.net/specs/openid-connect-core-1_0.html

Key distinction:

```text
OAuth 2.0 → delegated authorization
OpenID Connect → authentication / identity layer
Access Token → credential for protected resource
ID Token → authentication claims for client; not an API access token
```

---

# 3. ASP.NET Core / .NET 10 — primary implementation docs

- ASP.NET Core OpenAPI  
  https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0
- ASP.NET Core rate limiting middleware  
  https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0
- ASP.NET Core CORS  
  https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0
- ASP.NET Core authentication overview  
  https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-10.0
- ASP.NET Core authorization  
  https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0
- ASP.NET Core resource-based authorization  
  https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-10.0
- ASP.NET Core Problem Details / API error handling  
  https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0
- Minimal API responses  
  https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0
- .NET HTTP resilience  
  https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
- .NET resilience overview  
  https://learn.microsoft.com/en-us/dotnet/core/resilience/

---

# 4. Microsoft architecture guidance

- REST API design best practices  
  https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design
- API implementation guidance  
  https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-implementation
- Gateway Routing pattern  
  https://learn.microsoft.com/en-us/azure/architecture/patterns/gateway-routing
- Gateway Aggregation pattern  
  https://learn.microsoft.com/en-us/azure/architecture/patterns/gateway-aggregation
- Gateway Offloading pattern  
  https://learn.microsoft.com/en-us/azure/architecture/patterns/gateway-offloading
- Retry pattern  
  https://learn.microsoft.com/en-us/azure/architecture/patterns/retry
- Circuit Breaker pattern  
  https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker
- Throttling pattern  
  https://learn.microsoft.com/en-us/azure/architecture/patterns/throttling

Use architecture patterns as trade-off guidance, not mandatory topology.

---

# 5. OpenAPI — primary

- OpenAPI Specification index/latest versions  
  https://spec.openapis.org/oas/
- OpenAPI Specification 3.2.0  
  https://spec.openapis.org/oas/v3.2.0.html

As of the 2026-08-13 verification, OpenAPI publishes a 3.2.x line. **Do not infer that every .NET generator/client already supports every 3.2 feature.** Verify project tooling before pinning spec version.

---

# 6. GraphQL — primary

- GraphQL official site  
  https://graphql.org/
- GraphQL Specification  
  https://spec.graphql.org/
- GraphQL Learn  
  https://graphql.org/learn/

Focus on:

```text
typed schema
queries/mutations/subscriptions
field selection
schema evolution/deprecation
validation
execution
security/query cost as implementation concern
```

---

# 7. gRPC / Protocol Buffers — primary

- gRPC documentation  
  https://grpc.io/docs/
- gRPC core concepts  
  https://grpc.io/docs/what-is-grpc/core-concepts/
- ASP.NET Core gRPC — why/use cases  
  https://learn.microsoft.com/en-us/aspnet/core/grpc/why-grpc?view=aspnetcore-10.0
- Protocol Buffers documentation  
  https://protobuf.dev/
- Proto best practices  
  https://protobuf.dev/best-practices/dos-donts/

Focus on field-number compatibility, deadlines, unary/streaming RPC and generated-contract implications.

---

# 8. WebSocket / SSE / browser platform

- RFC 6455 — WebSocket Protocol  
  https://www.rfc-editor.org/rfc/rfc6455
- MDN WebSocket API  
  https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API
- MDN Server-Sent Events  
  https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events
- MDN Using Server-Sent Events  
  https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events
- Fetch / CORS standard  
  https://fetch.spec.whatwg.org/

MDN is useful for browser behavior/examples; normative protocol claims should be traced to the linked standards when needed.

---

# 9. Supplementary — DesignGurus

User-requested references reviewed on 2026-08-13:

- Complete System Design Interview Guide 2026  
  https://www.designgurus.io/system-design-interview
- DesignGurus Blog  
  https://www.designgurus.io/blog
- API topic archive  
  https://www.designgurus.io/blog/tag/api
- API Design Checklist  
  https://www.designgurus.io/blog/api-design-checklist
- REST vs GraphQL vs gRPC  
  https://www.designgurus.io/blog/rest-graphql-grpc-system-design
- Rate Limiter — algorithms, architecture and trade-offs  
  https://www.designgurus.io/blog/grokking-rate-limiters
- Types of System Design Interviews — API Design section  
  https://www.designgurus.io/blog/types-of-system-design-interviews

Useful ideas incorporated into Module 06:

```text
business constraints before syntax
contract-first thinking
backward compatibility
security by design
correlation/observability
pagination standards
idempotency
standardized errors
rate limiting
HTTP caching
API choice must be defended by workload
cost + operational maturity at senior level
stress-test at 10x / failure conditions
```

These are **supplementary perspectives**, not protocol specifications.

---

# 10. Supplementary — Craft Better Software

User-requested archive:

- Archive  
  https://craftbettersoftware.com/archive?sort=new
- The best way to test Web APIs  
  https://craftbettersoftware.com/p/the-best-way-to-test-web-apis
- What is a UNIT in unit test  
  https://craftbettersoftware.com/p/unit-testing-what-exactly-is-a-unit
- Hexagonal Architecture with TDD  
  https://craftbettersoftware.com/p/hexagonal-architecture-with-tdd
- The TDD Debate — design through public behavior/contracts  
  https://craftbettersoftware.com/p/the-tdd-debate
- How to Catch AI Mistakes In Your Code — deterministic guardrails  
  https://craftbettersoftware.com/p/how-to-catch-ai-mistakes-in-your

Ideas used here:

```text
public API = observable behavior boundary
prefer tests that survive internal refactor
contract/test first can improve API usability
repeated review rules should become deterministic CI gates
property/invariant tests catch more than happy path
```

Again: testing/design perspective only; HTTP/OAuth/security semantics come from official specifications.

---

# 11. Repository cross-links

- [Module 04 — Backend](../04-backend/README.md)
- [Module 05 — SQL](../05-sql/README.md)
- [Module 07 — ASP.NET Core](../07-aspnet-core/README.md)
- [Module 09 — Security](../09-security-devsecops/README.md)
- [Module 11 — Redis & Caching](../11-redis-caching/README.md)
- [Module 17 — Distributed Systems](../17-distributed-systems/README.md)
- [Module 18 — Microservices Architecture](../18-microservices-architecture/README.md)
- [Technology baseline](../00-roadmap/technology-baseline.md)
- [Source policy](../00-roadmap/source-policy.md)

---

# 12. Source rules

| Claim type | Preferred source |
|---|---|
| HTTP/OAuth/protocol semantics | RFC / standards body |
| ASP.NET Core/.NET API behavior | current Microsoft docs |
| OpenAPI/GraphQL/gRPC schema semantics | official spec/docs |
| Architecture trade-off | official architecture guidance + production evidence |
| Interview/review checklist | DesignGurus / similar supplementary source |
| Testing/code-quality perspective | Craft Better Software / test literature + local evidence |
| Version-sensitive claim | re-verify at implementation time |

## Verification metadata

- Verified: **2026-08-13**.
- External supplemental sources were used for discovery/review ideas, not as normative substitutes.
