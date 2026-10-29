# Integration, Security, Cloud/Hybrid & Platform Architecture

> [← Module 25](README.md) · [Business → HLD/LLD](solution-architecture-from-requirements-to-design.md) · [Delivery Governance](solution-architecture-delivery-governance.md)

Đây là kiến thức thiết kế **ranh giới giữa người dùng, API, services, data, platform và đối tác**. Một hệ thống có TLS và OAuth không tự động an toàn; phải review authorization, credential lifecycle, trust boundaries, supply chain và incident response.

## 1. Integration inventory trước khi chọn transport

Mỗi integration edge phải có:

| Thuộc tính | Câu hỏi |
|---|---|
| Ownership | Ai sở hữu contract, schema, endpoint, SLO và on-call? |
| Semantics | Command/query/event, sync/async, one-way/callback? |
| Data | Classification, retention, residency, encryption, masked logs? |
| Identity | Client/user/service identity, authorization scope, mutual trust? |
| Resilience | Timeout, retry, rate limit/quota, throttling, circuit breaker, fallback? |
| Correctness | Idempotency key, dedup, ordering, UNKNOWN state, reconciliation? |
| Evolution | Versioning, deprecation, backward compatibility, contract tests? |
| Operations | Trace IDs, metrics, alerts, escalation, support windows, runbook? |

### REST, gRPC, event và file batch

| Style | Ưu điểm | Chi phí và sai lầm thường gặp |
|---|---|---|
| REST/HTTP | Phổ biến, debug tốt, API governance thuận tiện | Chained latency, retries, pagination/versioning |
| gRPC | Typed protobuf contract, streaming, efficient binary | Client/network compatibility, deadlines, version evolution |
| Event/message | Async decoupling, fanout và buffering | Duplicate, ordering, schema registry, replay, DLQ |
| File/SFTP/batch | Phù hợp legacy/large periodic exchange | Partial file, checksum, idempotent import, schedule, reconciliation |

Không mặc định event-driven tốt hơn đồng bộ. Đối tác cần kết quả tức thì có thể dùng sync request, nhưng timeout ambiguity phải xử lý.

## 2. API Design và API Gateway

API Gateway có thể thực hiện routing, TLS termination, authn coarse-grained, throttling, observability và request policy. **Gateway không thay resource-level authorization** trong service. Cần xử lý BOLA/IDOR: user có token hợp lệ vẫn không được truy cập resource/account của người khác.

Checklist:

- Endpoint contract + OpenAPI, validation, error semantics, pagination/cursor stable order.
- Safe retry semantics: GET thường idempotent; POST chỉ retry được khi contract/idempotency cho phép.
- Rate limits per consumer/tenant, overload responses `429` + backoff guidance.
- Explicit API lifecycle: compatibility matrix, consumer inventory, deprecation date, contract tests.
- Webhook signature and replay window, timestamps, stable message ID, idempotent side effects.
- Correlation ID propagation không thay authorization; không log token/PII.

## 3. OAuth2/OIDC — phân biệt vai trò

- **OAuth2:** authorization framework để cấp access token theo scopes/audience cho resource server; không mặc định là user authentication protocol.
- **OIDC:** identity layer, ID token mô tả authentication event/client identity; **không dùng ID token làm API access token**.
- **Authorization Code + PKCE:** phù hợp public/mobile/browser clients với cấu hình đúng.
- **Client Credentials:** service/workload identity khi không có user context; vẫn cần per-resource permission.
- **Token validation:** verify signature/issuer/audience/expiry/algorithm/key rotation; tránh chấp nhận token dành cho API khác.
- **Authorization:** RBAC cho coarse role, ABAC/policy/resource ownership khi tenant/account scope quyết định quyền.

Threat model theo STRIDE qua các trust boundaries: Spoofing (stolen token), Tampering (webhook), Repudiation (audit gaps), Information disclosure (PII in logs), DoS (burst/retry), Elevation of privilege (cross-account IDOR).

## 4. Service-to-service và đối tác

| Mối đe dọa | Control | Evidence |
|---|---|---|
| Credential leak | Managed/workload identity, short-lived tokens, secret rotation | Rotation exercise, secret scan |
| MITM / endpoint spoof | TLS validation, service identity, mTLS nếu phù hợp | Negative cert/trust test |
| Replay callback | Signature + timestamp/nonce + message ID | Duplicate/replay test |
| SSRF | Egress allowlist, URL validation, metadata endpoint protection | Security test |
| Partner outage/rate limit | Deadline budget, queue, controlled retry, reconciliation | Fault injection, backlog age |
| Data exfiltration | Least privilege, classification, egress audit, encryption | Threat model + audit evidence |

Đừng bỏ qua DNS, trust chain, certificate renewal, clock skew, firewall/NAT/SNAT, outbound proxy, VPN/private link và support hours của đối tác. Một kênh private network không thay authorization hay encryption khi policy yêu cầu.

## 5. On-prem, cloud và hybrid reference considerations

| Topology | Khi phù hợp | Rủi ro kiến trúc |
|---|---|---|
| On-prem | Data locality, legacy systems, specialized connectivity | Capacity procurement, operations, DR site cost |
| Cloud | Elastic services, managed HA, rapid provisioning | Egress, vendor lock-in, quota, shared-responsibility gaps |
| Hybrid | Gradual migration, compliance/legacy and cloud workloads | Network latency/partition, identity federation, split DNS, data sync |
| Multi-cloud | Regulatory/strategic requirement rõ | Operational complexity, data movement, skill dispersion |

Thiết kế network/deployment view gồm region/zone, ingress/egress, load balancers, private endpoints, DNS, certs, firewall policies, secrets/keys, observability, backup/restore và on-call ownership. Với hybrid, **định nghĩa failure mode khi link WAN mất**: phần nào đọc được, phần nào phải ngừng ghi, reconcile sau reconnect.

## 6. Docker, Kubernetes, IaC, CI/CD và supply chain

- **Build:** multi-stage, minimal signed/scanned image, SBOM/provenance theo policy, immutable artifact và pinned dependencies.
- **Runtime:** non-root, read-only filesystem khi khả thi, CPU/memory limits, graceful termination, readiness/liveness/startup probes khác nhau.
- **Config/secrets:** không commit credentials; workload identity/secret store, rotation, audit access.
- **Delivery:** unit/integration/security/contract checks, environment promotion, canary/blue-green, feature flag, rollback.
- **Schema:** expand–contract, ensure old/new app versions coexist, migration dry-run + restore path.
- **IaC:** reproducible modules, remote state locking, drift detection, scoped access, plan review.
- **Recovery:** deployment failure, image rollback và data rollback không giống nhau; migration có thể chỉ forward-fix.

Kubernetes tự restart container nhưng không tự đảm bảo idempotency, business recovery, correct transactions hoặc DR target. HPA có thể scale app trong khi DB/broker/partner quota vẫn là bottleneck.

## 7. Platform Engineering & standards

**Golden path** giảm cognitive load cho team: template ASP.NET Core API/worker, logging/tracing conventions, auth middleware, health checks, Docker/IaC modules, CI policies, test harness, runbooks. Cung cấp self-service trong guardrails, không buộc mọi service dùng một kiến trúc cứng.

Architecture standards nên phân loại:

1. **Mandatory controls:** authz, secret handling, PII logging, backup, change review.
2. **Recommended defaults:** API conventions, telemetry, retry libraries, container base images.
3. **Allowed exceptions:** process review + expiry + compensating controls.

Gắn standard với kiểm thử tự động: OpenAPI compatibility, policy-as-code, dependency boundaries, security scans, infrastructure tests. Document exception không đủ nếu production drift không được phát hiện.

## 8. Design review exercise

Một ERP on-prem gọi payment provider qua internet; cloud worker nhận webhook; customer data có yêu cầu residency.

Deliverables:

- Trust boundary và network/deployment diagram; data classification, secrets, account-level authorization.
- Integration table cho ERP → API → provider → webhook/worker → DB; timeout/retry/duplicate contract.
- Hai phương án: on-prem-only vs hybrid, bao gồm latency, WAN outage, DR, cost và team operations.
- Authn/authz negative tests, certificate rotation, partner-committed-but-timeout fault, backlog recovery.
- CI/CD release strategy, migration compatibility, monitoring/runbook và exception process.

**Pass:** trình bày được end-to-end ownership và cách hệ thống xử lý khi WAN/partner/auth provider hỏng; security review không dừng ở “dùng HTTPS và OAuth”.

## Nguồn chính thống

- [OAuth 2.0 Security Best Current Practice (RFC 9700)](https://www.rfc-editor.org/rfc/rfc9700)
- [OpenID Connect Core](https://openid.net/specs/openid-connect-core-1_0.html)
- [OWASP API Security Top 10](https://owasp.org/API-Security/editions/2023/en/0x00-header/)
- [Kubernetes documentation](https://kubernetes.io/docs/)
