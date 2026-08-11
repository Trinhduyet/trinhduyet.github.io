# Research Report — Module 02 Completion

## Scope

Đợt nghiên cứu này hoàn tất ba learning slices còn lại của Module 02:

1. Git mental model và safe recovery.
2. Proxy, NAT, load balancer và Linux network boundaries.
3. Incident lab end to end bằng ASP.NET Core/.NET 10.

Trạng thái “content v1 complete” chỉ nói tài liệu và lab scaffold đã đủ. Level người học không thay đổi cho đến khi có output/evidence được review.

## Research Questions

### Git

- Working tree, index, object database, refs, HEAD và reflog liên hệ thế nào?
- restore, revert và reset tác động state nào?
- Recovery nào phù hợp cho uncommitted, local-only, published và unreachable commits?
- Reflog có giới hạn retention/reachability gì?
- Git revision được nối với artifact/deployment provenance thế nào?
- Repository ownership, hooks/config, safe.directory và secret history tạo security boundary nào?

### Network intermediaries

- Network namespace cô lập network resources nào?
- NAT/NAPT thêm state và thay address identity ra sao?
- L4 load balancer và L7 reverse proxy tạo những connection boundaries nào?
- Forwarded/X-Forwarded metadata đáng tin dưới điều kiện nào?
- ASP.NET Core 10 xử lý KnownProxies/KnownNetworks, ForwardLimit và middleware ordering thế nào?
- Health, drain, timeout và retry phải phối hợp qua các hop ra sao?

### Incident lab

- Làm sao tạo failure shape có thể tái lập mà không cần package/service bên ngoài?
- Endpoint nào đủ tách liveness, readiness, transport metadata, slow work và outbound dependency?
- Làm sao bound delay/connect/dependency deadline và dừng automated process an toàn?
- Assertion nào phải environment-sensitive thay vì hard-code?

## Primary Sources

### Git

- Official Git reference cho restore, revert, reset, reflog, worktree, bisect và config.
- Pro Git official edition cho object/ref internals và data recovery.

Findings:

- restore cập nhật working tree/index từ source nhưng không di chuyển branch;
- revert tạo commit mới đảo effect và phù hợp shared history;
- reset với commit di chuyển current branch và tùy mode thay index/working tree;
- reflog là local update history và có expiration/garbage-collection boundary;
- branch/ref là pointer; commit object chỉ an toàn khi còn reachable hoặc được giữ bằng ref;
- safe.directory là trust exception, không nên wildcard toàn hệ thống.

### Networking

- Linux man-pages cho network_namespaces, ip-netns và /proc/PID/net.
- RFC 3022 cho traditional NAT/NAPT state and limitations.
- RFC 7239 cho standardized Forwarded metadata, integrity và privacy concerns.
- RFC 9110 cho HTTP semantics/status boundaries.
- Microsoft Learn ASP.NET Core 10 cho proxy/load-balancer và forwarded-header configuration.

Findings:

- network namespace cô lập interfaces, routes, firewall rules và socket port space;
- NAT dịch address/transport identifiers và yêu cầu flow state; nó không tự là firewall/auth;
- reverse proxy thường tạo client-side và upstream connections riêng;
- raw Forwarded/X-Forwarded values có thể bị client/proxy sửa;
- ASP.NET Core chỉ nên consume metadata từ exact known proxy/network và đúng ForwardLimit;
- servicing hardening từ ASP.NET Core 8.0.17/9.0.6 bỏ qua X-Forwarded-* từ unknown proxies.

## Lab Design Decisions

| Decision | Reason |
| --- | --- |
| Microsoft.NET.Sdk.Web net10.0 | Dùng shared framework, không external package |
| Loopback 127.0.0.1 | Không expose diagnostic service ra network |
| Maximum /work delay 30 seconds | Bounded failure experiment |
| Connect timeout 2 seconds | Tách connect phase, giữ lab ngắn |
| Dependency deadline 3 seconds | Tạo deterministic slow-upstream 504 |
| PooledConnectionLifetime 5 minutes | Minh họa production pool/DNS concept |
| Self-upstream | Tạo success, 404 và slow response local |
| READINESS_MODE | Tách live process khỏi traffic readiness |
| Raw forwarded headers only | Chứng minh spoofability mà không giả lập trusted proxy |
| LAB_ALLOW_STOP opt-in | Automated smoke test dừng app qua graceful host lifecycle |

Các giá trị timeout/lifetime là lab controls, không phải universal production defaults.

## Runtime Verification

Environment:

- Windows PowerShell workspace.
- .NET SDK 10.0.301.
- Git 2.49.0.windows.1.
- Workspace root không phải valid Git repository; existing incomplete .git directory không bị sửa.

Verified results:

- IncidentService Release build: 0 warnings, 0 errors.
- /health/live: HTTP 200, status live.
- /health/ready baseline: HTTP 200, status ready.
- READINESS_MODE=fail: HTTP 503.
- /work?delayMs=25: HTTP 200, bounded delay returned.
- Missing UPSTREAM_URL: HTTP 503 configuration category.
- Self-upstream /health/live: HTTP 200, http-response category.
- Spoofed X-Forwarded-For was visible as raw header while transport remote IP remained 127.0.0.1.
- Unused loopback port on this Windows path reached the 2-second connect timeout and returned lab 504, not an immediate refusal. The lab therefore requires observed classification.
- Automated instances bound only loopback, ran hidden and exited through opt-in graceful stop endpoint.

Linux-only signal, /proc, ss and namespace scenarios were source-reviewed but not executed because no active general-purpose Linux runtime was available. Learner output remains required.

## Rejected Shortcuts

- Không initialize hoặc repair root .git vì đó là user-owned workspace state ngoài scope.
- Không dùng reset hard/clean trong Git lab.
- Không bật trust-all forwarded headers để đơn giản hóa proxy demo.
- Không dùng Docker/Kubernetes trước prerequisite.
- Không đưa memory/CPU destructive stress vào incident lab.
- Không hard-code unused port = connection refused.
- Không xem build/smoke test của tác giả là evidence năng lực của người học.

## Artifacts Produced

- [Git mental model and safe recovery](../02-linux-git-networking/git-mental-model-and-safe-recovery.md)
- [Proxy, NAT, load balancer and network boundaries](../02-linux-git-networking/proxy-nat-load-balancer-and-network-boundaries.md)
- [.NET incident lab](../02-linux-git-networking/incident-lab-dotnet-service.md)
- [Incident service project](../../labs/02-linux-git-networking/incident-service/IncidentService.csproj)
- Updated [Module 02 overview](../02-linux-git-networking/README.md)
- Updated [root README](../../README.md)
- Updated [skills matrix](skills-matrix.md) and [master roadmap](master-roadmap.md)

## Verification Metadata

- Research date: 2026-08-11.
- Source policy: official Git docs, Linux man-pages, IETF RFCs and Microsoft Learn.
- Version-sensitive scope: Git manuals through 2.55.0; local Git 2.49.0; ASP.NET Core/.NET 10.
- Context7: not required; official primary pages directly covered the selected behaviors.
- Completion meaning: documentation and lab scaffold v1 complete; learner assessment/evidence pending.
