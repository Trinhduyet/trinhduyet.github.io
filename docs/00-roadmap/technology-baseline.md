# Technology Baseline

> Snapshot này dùng để giữ ví dụ nhất quán. Nó **không** thay compatibility matrix của cloud/provider, security advisory hoặc exact version pin của executable lab.

## Baseline reviewed 2026-08-28

| Technology | Documentation baseline | Observed stable/current signal | Verified | Official source |
|---|---|---|---|---|
| .NET | **10.0 LTS** | latest servicing patch `10.0.11`; support đến 2028-11-14 | 2026-08-28 | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) |
| ASP.NET Core | **10.0** | lifecycle đi cùng .NET 10 | 2026-08-28 | [ASP.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/aspnet) |
| EF Core | **10.0** | stable package `10.0.11`; provider compatibility kiểm tra riêng | 2026-08-28 | [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore) · [providers](https://learn.microsoft.com/en-us/ef/core/providers/) |
| SQL Server | **SQL Server 2025** | latest CU listed by Microsoft: **CU8 / August 2026** | 2026-08-28 | [SQL Server latest updates](https://learn.microsoft.com/en-us/troubleshoot/sql/releases/download-and-install-latest-updates) |
| Docker Engine | **29.x** | documented latest 29.x patch: `29.7.2` | 2026-08-28 | [Docker Engine 29 release notes](https://docs.docker.com/engine/release-notes/29/) |
| Kubernetes | **1.37 concepts/API line for new upstream docs** | upstream `1.37.0` released 2026-08-26; managed-provider support may lag | 2026-08-28 | [Kubernetes 1.37](https://kubernetes.io/releases/1.37/) · [release announcement](https://kubernetes.io/blog/2026/08/26/kubernetes-v1-37-release/) |
| Terraform | **1.16.x** for new docs; existing labs/config must be compatibility-tested before bump | stable `1.16.0` released 2026-08-26; 1.17 is prerelease | 2026-08-28 | [Terraform releases](https://github.com/hashicorp/terraform/releases) |
| Redis Open Source | **8.10.x** | observed stable `8.10.0` | 2026-08-28 | [Redis releases](https://github.com/redis/redis/releases) · [Redis docs](https://redis.io/docs/latest/) |
| OpenTelemetry .NET | **1.17.x** | `core-1.17.0` remains latest stable release signal | 2026-08-28 | [OpenTelemetry .NET releases](https://github.com/open-telemetry/opentelemetry-dotnet/releases) |
| Microsoft.Extensions.AI | **10.8.x** | NuGet stable `10.8.3` | 2026-08-28 | [Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI) · [.NET AI docs](https://learn.microsoft.com/en-us/dotnet/ai/) |
| Microsoft Agent Framework (.NET) | **1.17.x** | `dotnet-1.17.0` marked latest at review time | 2026-08-28 | [Agent Framework releases](https://github.com/microsoft/agent-framework/releases) |
| OpenAI .NET | **2.12.x released line** | latest public release/package observed `2.12.0`; `main` version prefix is not treated as a released version | 2026-08-28 | [openai-dotnet releases](https://github.com/openai/openai-dotnet/releases) · [OpenAI NuGet](https://www.nuget.org/packages/OpenAI) |

---

# 1. Ba version khác nhau phải phân biệt

Một trong những lỗi documentation phổ biến là dùng một number cho ba mục đích khác nhau.

```text
Upstream current release
!=
Cloud/provider supported release
!=
Executable lab pinned release
```

Ví dụ Kubernetes:

```text
kubernetes.io current upstream = 1.37.x

AKS/EKS/GKE supported versions
= provider-specific and may lag upstream

repo lab
= exact version/digest chosen for reproducibility
```

Do đó Module 15 có thể dạy core behavior theo current Kubernetes docs nhưng AKS chapter phải kiểm Microsoft support matrix trước production deployment.

---

# 2. Stable concept vs version-sensitive detail

| Stable mental model | Version-sensitive detail cần re-check |
|---|---|
| .NET async/cancellation | runtime/package patch and APIs |
| HTTP semantics | hosting/protocol configuration |
| Relational transactions | SQL engine feature/compatibility level |
| Container process/network boundary | Docker runtime/build behavior |
| Kubernetes desired state/reconciliation | API versions, feature gates, supported minors |
| Terraform state/plan/apply | CLI/provider constraints and state format behavior |
| Telemetry context | SDK/exporter/semantic conventions |
| AI tool calling/AuthZ boundary | SDK/provider/model exact APIs |

Rule:

```text
Teach stable mechanism deeply.
Link/pin version-sensitive surface explicitly.
```

---

# 3. Upgrade policy

Không upgrade production chỉ vì upstream vừa release.

Use:

```text
new release
  ↓
read breaking/security notes
  ↓
provider/library compatibility
  ↓
reproducible build/tests
  ↓
load/failure tests where relevant
  ↓
staged rollout
  ↓
rollback/recovery path
```

Examples:

### .NET servicing

Stay on supported major and move to latest compatible servicing/security patch through normal validation.

### Kubernetes minor

Check:

```text
managed provider support
API removals/deprecations
version skew
CNI/CSI/ingress compatibility
policy/observability add-ons
```

### Terraform

Pin CLI/provider constraints for executable environments. A documentation baseline change does not silently rewrite production state.

### AI SDKs

SDK and model/provider behavior evolve quickly. Pin exact packages in runnable projects and keep eval regression gate.

---

# 4. Important corrections from previous snapshot

The 2026-08-11 snapshot is now stale in several places:

```text
.NET       10.0.10 → 10.0.11 servicing
EF Core    10.0.10 → 10.0.11 stable
SQL 2025   CU7      → CU8 listed by Microsoft
Kubernetes 1.36.x   → upstream 1.37.0 current
Terraform  1.15.x   → 1.16.0 stable
```

OpenAI .NET is deliberately recorded from **released package/release artifacts**. A version prefix on a development branch is not used as release evidence.

---

# 5. Reproducibility policy

Documentation prose:

```text
supported major/minor + current behavior
```

Executable lab:

```text
exact package version
exact container tag/digest when practical
provider lock file
version command output
```

Production architecture:

```text
supported version
+ compatibility
+ security
+ rollout
+ rollback
```

Never copy `latest` into production examples as an identity guarantee.

---

# 6. Refresh cadence

| Cadence | Review |
|---|---|
| Before each production lab | exact package/runtime/provider versions |
| Monthly | .NET servicing, Docker, Kubernetes, Redis, OTel, AI SDKs/security advisories |
| Quarterly | Terraform/SQL updates, cloud support matrices, roadmap breadth |
| Before cloud/Kubernetes deploy | provider-supported versions + region/SKU constraints |
| Before AI release | SDK/model/config + evaluation baseline |

A date in this file means “verified then”, not “safe forever”.

---

# 7. Source policy

Priority:

```text
support/release policy
→ official documentation
→ official package/release artifact
→ provider compatibility matrix
→ community source for explanation only
```

Roadmap sites/blogs can help audit breadth; they do not decide runtime/security/version behavior.

→ [Source Policy](source-policy.md)

## Verification metadata

- Repository baseline review: **2026-08-28**, Asia/Bangkok.
- Critical refreshes verified against official .NET, Microsoft, Docker, Kubernetes, HashiCorp, OpenTelemetry, Microsoft package/framework and OpenAI release/package sources.
- Managed cloud versions are intentionally not inferred from upstream Kubernetes current release.
