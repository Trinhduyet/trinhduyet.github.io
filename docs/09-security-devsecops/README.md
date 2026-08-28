# Module 09 — Security & DevSecOps

> [← Testing & Code Review](../08-testing-code-review/README.md) · [DevOps & IaC →](../13-devops-iac/README.md) · [References](references.md)

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Focus</strong>&nbsp;trust boundaries · identity · secrets · supply chain</span>
  <span><strong>Mode</strong>&nbsp;threat → control → test → evidence</span>
</div>

Security trong backend production không phải “thêm JWT + HTTPS”. Mục tiêu là trả lời:

```text
Ai đang gọi?
Được phép làm gì?
Dữ liệu nào đang đi qua trust boundary?
Input nào attacker kiểm soát?
Credential nào tồn tại ở đâu?
Build artifact có đáng tin không?
Nếu control fail thì phát hiện/recover thế nào?
```

## Hiểu trong 5 phút

Dùng một Orders API:

```text
Internet
  ↓
Edge / API
  ↓
Authentication
  ↓
Authorization
  ↓
Orders application
  ├─ SQL
  ├─ Redis
  └─ Payment API
```

Mỗi mũi tên là một trust/dependency boundary.

Security review không dừng ở endpoint:

```text
source code
→ dependency
→ build runner
→ artifact/image
→ registry
→ deployment identity
→ runtime secrets
→ network/data access
→ logs/audit
```

Đó là lý do **application security và DevSecOps phải nối với nhau**.

---

# 1. Bắt đầu bằng asset + threat, không bằng tool

Trước khi chọn scanner/WAF/vault, ghi:

```text
Assets
- customer PII
- order/payment state
- access tokens
- deployment credentials
- source/artifacts

Attack surfaces
- public HTTP input
- file/webhook payload
- outbound URL
- CI dependency/install step
- secret/config injection
- admin/operator path
```

Sau đó hỏi theo STRIDE-style reasoning hoặc threat checklist phù hợp:

```text
spoof identity?
tamper state?
repudiate action?
leak information?
denial of service?
escalate privilege?
```

→ [Threat Modeling & Application Security](threat-modeling-and-application-security.md)

---

# 2. Authentication != Authorization

```text
Authentication
= bạn là ai?

Authorization
= identity này được làm action nào trên resource nào?
```

Ví dụ:

```text
User A authenticated ✓
GET /orders/A-123 ✓
GET /orders/B-999 ✗
```

Nếu app chỉ check “token valid” mà không check ownership/policy, đó vẫn là security bug.

Backend phải reason:

```text
principal
claims / roles / permissions
resource ownership
operation
policy
```

Không đưa authorization boundary vào UI hoặc prompt.

---

# 3. OAuth/OIDC/JWT — học boundary trước token syntax

Mental model:

```text
Identity Provider
   ↓ authenticate / issue token
Client
   ↓ bearer credential
Resource Server / API
   ↓ validate issuer/audience/signature/time
Authorization policy
```

Questions bắt buộc:

- token dành cho audience nào?
- access token hay ID token?
- expiry/clock skew?
- key rotation?
- scope/role semantics?
- token bị leak ở log/browser/storage không?

JWT là token format/carrying mechanism; **JWT không tự tạo authorization model đúng**.

---

# 4. Input security — validation != attack prevention toàn bộ

Input từ client luôn untrusted.

Common boundaries:

```text
SQL / ORM
shell/process
HTML
URL / SSRF
file path
JSON/XML parser
header
redirect URL
webhook
```

Rule:

```text
validate business shape
+ encode/parameterize at sink
+ constrain outbound capabilities
+ least privilege downstream
```

Ví dụ SQL:

```text
parameterized query
+ DB permission scoped
+ business authorization
```

Không dùng regex “sanitize everything” như universal solution.

---

# 5. Secrets — secret value không nên trở thành application architecture

Bad lifecycle:

```text
secret in source
→ copied into image
→ copied into CI variable
→ logged accidentally
→ never rotated
```

Preferred direction:

```text
workload identity where possible
→ authorize identity to secret/data service
→ short-lived credential
→ rotation/audit
```

Khi secret vẫn cần:

```text
approved secret store
least-privilege access
no source/image embedding
rotation path
audit access
redaction in telemetry
```

→ [Identity, Secrets & Data Protection](identity-secrets-and-data-protection.md)

---

# 6. Data protection

Tách ba câu hỏi:

```text
Data in transit
→ TLS / authenticated channel

Data at rest
→ storage/database encryption + key ownership

Data in use / application
→ who can read, query, log, export?
```

Encryption at rest không sửa được broken authorization.

PII/security-sensitive fields cần policy về:

```text
collection
retention
masking
logging
backup
export
deletion
```

---

# 7. Least privilege theo từng identity

Một production system thường có nhiều identity:

```text
Developer
CI workflow
Deployment workflow
Runtime API
Worker
Operator
Database user
Kubernetes ServiceAccount
```

Không dùng một credential “admin” xuyên tất cả boundary.

Review matrix:

| Identity | Need | Must not need |
|---|---|---|
| CI | build/test/push artifact | production DB admin |
| deploy | update approved runtime | source secrets unrelated to deploy |
| API | read/write owned app data | cluster admin |
| worker | process assigned queue/data | deploy infrastructure |

---

# 8. Secure supply chain

Code đúng vẫn có thể bị compromise qua delivery path.

```text
PR
 ↓
review + tests
 ↓
dependency / secret / SAST checks
 ↓
build in controlled runner
 ↓
immutable artifact/image digest
 ↓
registry
 ↓
approved deployment
```

Phải biết purpose của:

```text
dependency scanning
secret scanning
SAST
SBOM
artifact provenance/signing concepts
branch protection
least-privilege CI token
pinning/reviewing third-party actions
```

Scanner finding != security decision. Cần severity + exploitability + exposure + remediation ownership.

→ [Secure Supply Chain & DevSecOps](secure-supply-chain-and-devsecops.md)

---

# 9. Security gates phải nằm trong delivery lifecycle

Module 08 tạo quality evidence:

```text
unit/integration/contract tests
```

Module 09 thêm:

```text
secret/dependency/security gates
```

Module 13 dùng các gates trước artifact promotion:

```text
PR
→ quality/security gates
→ artifact
→ deploy
```

Security không phải audit cuối release.

---

# 10. Logging và audit

Security telemetry hữu ích:

```text
authentication failures
authorization denials
admin changes
credential/secret access when available
suspicious request patterns
security-control failures
artifact/deployment identity
```

Nhưng không log mù:

```text
password
access token
refresh token
API key
full sensitive payload
```

Audit log cần integrity/retention/access policy phù hợp requirement.

---

# 11. Failure experiments

## A — Broken object authorization

Attempt user A reading/updating user B's resource.

Expected:

```text
403/404 according to contract
no data leak
security event observable where appropriate
```

## B — Expired/wrong-audience token

Verify API rejects token independent of frontend behavior.

## C — Secret exposure test

Search source/build output/logs for seeded fake secret; gate should detect expected class of leak.

## D — Dependency vulnerability workflow

Introduce/use a test fixture finding and verify:

```text
scanner finding
→ triage
→ ownership
→ remediation/exception evidence
```

## E — SSRF-style outbound input

If application accepts user-controlled URLs, verify private/metadata/internal destinations are constrained according to design.

---

# 12. Khi nào control trở thành security theater?

Examples:

```text
WAF exists but API authorization broken
secret vault exists but workload uses shared admin credential
SAST passes but dangerous business authorization is untested
TLS everywhere but tokens logged in plaintext
SBOM generated but nobody owns vulnerable dependency response
```

Control chỉ có giá trị khi nối tới threat + enforcement + evidence + operations.

---

# 13. Module map

| Guide | Focus |
|---|---|
| [Threat Modeling & Application Security](threat-modeling-and-application-security.md) | trust boundaries, injection/SSRF/input, abuse/failure reasoning |
| [Identity, Secrets & Data Protection](identity-secrets-and-data-protection.md) | OAuth/OIDC/AuthZ, secrets, least privilege, data protection |
| [Secure Supply Chain & DevSecOps](secure-supply-chain-and-devsecops.md) | dependency/SAST/secret/SBOM gates and delivery trust |
| [References](references.md) | canonical sources |

## Evidence status

Module hiện có **deep/guided content**, chưa có dedicated executable `labs/09-security-devsecops` artifact trong repo.

Evidence nên lưu từ system/project đang học:

```text
threat model
AuthZ test matrix
security regression tests
CI gate output
secret/identity decision
incident/failure note
```

---

# 14. Exit criteria

Bạn hoàn thành foundation khi có thể:

- draw trust boundaries của một backend system;
- phân biệt AuthN/AuthZ và test resource ownership;
- explain OAuth/OIDC access-token boundary;
- identify injection/SSRF/file/webhook attack surfaces;
- keep secrets out of source/image/logs;
- design least-privilege identities cho CI/deploy/runtime;
- explain encryption vs authorization vs data lifecycle;
- design security gates trong CI/CD;
- interpret scanner finding thay vì blindly fail/pass;
- run ít nhất một authorization + one supply-chain failure experiment;
- explain why a simpler control set may be safer than a complex unowned stack.

## Học tiếp

```text
Security controls
→ Performance/capacity
→ Docker
→ DevOps delivery
→ Cloud/Kubernetes security mapping when needed
```

## Verification metadata

- Reviewed: 2026-08-28.
- Maturity: Deep/Guided; dedicated runnable lab pending.
- Canonical behavior/security guidance: official sources in [references.md](references.md).
- Quality model: [Learning Quality Standard](../00-roadmap/learning-quality-standard.md).
