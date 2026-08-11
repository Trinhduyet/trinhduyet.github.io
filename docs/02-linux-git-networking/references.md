# References — Linux, Git và Networking

## Official

- [Linux man-pages project](https://www.kernel.org/doc/man-pages/)
- [proc(5)](https://man7.org/linux/man-pages/man5/procfs.5.html)
- [ps(1)](https://man7.org/linux/man-pages/man1/ps.1.html)
- [ss(8)](https://man7.org/linux/man-pages/man8/ss.8.html)
- [signal(7)](https://man7.org/linux/man-pages/man7/signal.7.html)
- [path_resolution(7)](https://www.man7.org/linux/man-pages/man7/path_resolution.7.html)
- [inode(7)](https://www.man7.org/linux/man-pages/man7/inode.7.html)
- [credentials(7)](https://www.man7.org/linux/man-pages/man7/credentials.7.html)
- [chmod(1)](https://www.man7.org/linux/man-pages/man1/chmod.1.html)
- [umask(2)](https://www.man7.org/linux/man-pages/man2/umask.2.html)
- [acl(5)](https://www.man7.org/linux/man-pages/man5/acl.5.html)
- [capabilities(7)](https://www.man7.org/linux/man-pages/man7/capabilities.7.html)
- [symlink(7)](https://www.man7.org/linux/man-pages/man7/symlink.7.html)
- [proc_pid_status(5)](https://man7.org/linux/man-pages/man5/proc_pid_status.5.html)
- [getrlimit(2)](https://man7.org/linux/man-pages/man2/getrlimit.2.html)
- [findmnt(8)](https://man7.org/linux/man-pages/man8/findmnt.8.html)
- [Filesystem Hierarchy Standard 3.0](https://refspecs.linuxfoundation.org/FHS_3.0/fhs/index.html)
- [Linux Pressure Stall Information](https://docs.kernel.org/accounting/psi.html)
- [Linux cgroup v2](https://www.kernel.org/doc/html/latest/admin-guide/cgroup-v2.html)
- [systemd manual](https://www.freedesktop.org/software/systemd/man/latest/)
- [systemd.service source documentation](https://github.com/systemd/systemd/blob/main/man/systemd.service.xml)
- [systemd.resource-control source documentation](https://github.com/systemd/systemd/blob/main/man/systemd.resource-control.xml)
- [Git reference](https://git-scm.com/docs)
- [Git restore](https://git-scm.com/docs/git-restore)
- [Git revert](https://git-scm.com/docs/git-revert)
- [Git reset](https://git-scm.com/docs/git-reset)
- [Git reflog](https://git-scm.com/docs/git-reflog)
- [Git worktree](https://git-scm.com/docs/git-worktree)
- [Git bisect](https://git-scm.com/docs/git-bisect)
- [git-config safe.directory](https://git-scm.com/docs/git-config#Documentation/git-config.txt-safedirectory)
- [Linux network namespaces](https://man7.org/linux/man-pages/man7/network_namespaces.7.html)
- [ip-netns(8)](https://man7.org/linux/man-pages/man8/ip-netns.8.html)
- [/proc/PID/net](https://www.man7.org/linux/man-pages/man5/proc_net.5.html)
- [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [.NET HttpClient guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [IHttpClientFactory troubleshooting](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory-troubleshooting)
- [.NET networking events](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/telemetry/events)
- [ASP.NET Core proxy and load balancer configuration](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)
- [ASP.NET Core forwarded-header hardening](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/8/forwarded-headers-unknown-proxies?view=aspnetcore-10.0)
- [ASP.NET Core Kestrel security considerations (.NET 10)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/security-considerations?view=aspnetcore-10.0)

## Specifications

- [RFC 9110 — HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html)
- [RFC 9112 — HTTP/1.1](https://www.rfc-editor.org/rfc/rfc9112.html)
- [RFC 9293 — Transmission Control Protocol](https://www.rfc-editor.org/rfc/rfc9293.html)
- [RFC 8446 — TLS 1.3](https://www.rfc-editor.org/rfc/rfc8446.html)
- [RFC 1034 — Domain Names: Concepts and Facilities](https://www.rfc-editor.org/rfc/rfc1034.html)
- [RFC 1035 — Domain Names: Implementation and Specification](https://www.rfc-editor.org/rfc/rfc1035.html)
- [RFC 3022 — Traditional NAT](https://www.rfc-editor.org/rfc/rfc3022.html)
- [RFC 7239 — Forwarded HTTP Extension](https://www.rfc-editor.org/rfc/rfc7239.html)

## Roadmap

- [Linux roadmap](https://roadmap.sh/linux)
- [Network Engineer roadmap](https://roadmap.sh/network-engineer)
- [Backend roadmap](https://roadmap.sh/backend)
- [Git and GitHub roadmap](https://roadmap.sh/git-github)

## Vietnamese

- [Microsoft Learn — truy cập ứng dụng mạng với WSL](https://learn.microsoft.com/vi-vn/windows/wsl/networking)
- [Microsoft Learn — quyền file giữa Windows và WSL](https://learn.microsoft.com/vi-vn/windows/wsl/file-permissions)
- [Microsoft Learn — tổng quan WSL](https://learn.microsoft.com/vi-vn/windows/wsl/about)

Ghi chú: tài liệu tiếng Việt chính thức cho Linux man-pages và IETF RFC không đầy đủ. English specification/manual là source of truth; không thêm bản dịch cộng đồng chưa được kiểm chứng chỉ để đủ mục.

## Deep Dive

- [Linux Performance](https://www.brendangregg.com/linuxperf.html) — bản đồ công cụ và methodology; dùng như deep dive, không thay man page.
- [Google SRE Book — Monitoring Distributed Systems](https://sre.google/sre-book/monitoring-distributed-systems/) — symptom/metric và production operations.

## Books / Papers

- Michael Kerrisk, *The Linux Programming Interface*.
- Stevens et al., *TCP/IP Illustrated, Volume 1*.
- Chacon & Straub, *Pro Git* — [official free edition](https://git-scm.com/book/en/v2).

## Verification metadata

- Verified: 2026-08-11
- Dead-link check: repository-wide validation checked 104 unique links, then removed one unavailable Vietnamese Git link; the remaining 103 links have no confirmed 404/410. Two NuGet pages reject HEAD but return HTTP 200 with GET.
- Notes: distro-specific administration docs sẽ được thêm theo lab environment, không làm canonical cho mọi Linux.
