# Research Report — Module 02 Expansion

## Scope

Đợt nghiên cứu này phục vụ ba chương production-grade:

1. Linux filesystem, permissions và identities.
2. Processes, signals và resource pressure.
3. DNS, TCP, TLS, HTTP và .NET HttpClient.

Mục tiêu không phải liệt kê command. Mục tiêu là xác minh semantics từ nguồn chính thức, tách stable concepts khỏi behavior phụ thuộc runtime/distro và tạo lab có target giới hạn.

## Research Questions

- Kernel resolve path và chọn permission class như thế nào?
- Directory execute/search khác directory read ra sao?
- ACL, umask, capabilities, mount và namespace thay đổi authorization boundary thế nào?
- Process utilization khác pressure/throttling/hard limit ra sao?
- Signal, rlimit, systemd grace period và .NET Generic Host tương tác thế nào?
- Evidence nào tách DNS, route, TCP, TLS và HTTP failures?
- HttpClient connection pooling tác động DNS refresh và port usage thế nào?
- Nguồn tiếng Việt nào đủ đáng tin để hỗ trợ WSL lab mà không thay source of truth?

## Source Selection

### Linux filesystem and identity

Nguồn chuẩn:

- Linux man-pages cho path_resolution, inode, credentials, chmod, umask, ACL, capabilities và symlink.
- Filesystem Hierarchy Standard 3.0 cho directory purpose ở cấp standard.
- Microsoft Learn cho khác biệt permission giữa Windows filesystem và WSL.

Kết luận quan trọng:

- path access cần search permission trên từng parent directory;
- kernel chủ yếu làm việc với numeric identity;
- mode bits không phải boundary duy nhất;
- file đã unlink có thể tiếp tục chiếm storage khi descriptor còn mở;
- permission không thay encryption hoặc secret lifecycle.

### Processes and resource pressure

Nguồn chuẩn:

- Linux kernel PSI và cgroup v2 documentation.
- Linux man-pages cho signals, /proc/PID/status và resource limits.
- systemd source documentation do freedesktop web pages không trả nội dung ổn định cho research client.
- Microsoft Learn cho Generic Host, dotnet-counters và dotnet-trace.

Kết luận quan trọng:

- utilization, pressure và limit events là ba evidence classes khác nhau;
- host metric có thể che cgroup throttling;
- memory.high throttle/reclaim, memory.max là hard boundary;
- process limits phải đọc từ process thật;
- SIGTERM bắt đầu shutdown contract, SIGKILL không cho cleanup;
- counters phù hợp triage ban đầu, trace/profiler cần câu hỏi và duration bounded.

### DNS, TCP, TLS, HTTP and .NET

Nguồn chuẩn:

- RFC 1034/1035 cho DNS.
- RFC 9293 cho current TCP standard.
- RFC 8446 cho TLS 1.3.
- RFC 9110 cho HTTP semantics.
- Microsoft Learn và Context7 /dotnet/docs cho HttpClient lifecycle, connection pooling, DNS refresh và networking telemetry.

Kết luận quan trọng:

- DNS answer, connect, TLS handshake và HTTP response là các boundary riêng;
- TCP không cung cấp cryptographic identity/confidentiality;
- HTTP semantics dùng chung qua các protocol versions;
- HttpClient resolve DNS khi tạo connection và không tự theo dõi DNS TTL;
- PooledConnectionLifetime cần phù hợp DNS/failover contract;
- retry phải nằm trong deadline/idempotency/load budget.

## Version-Sensitive Findings

| Finding | Scope verified |
| --- | --- |
| Linux man-pages content | Pages accessed 2026-08-11; selected pages expose man-pages 6.18 |
| FHS | Version 3.0 final |
| TCP | RFC 9293, current Internet Standard replacing RFC 793 |
| HTTP semantics | RFC 9110 |
| TLS | TLS 1.3 RFC 8446 |
| .NET runtime guidance | .NET 10 documentation context |
| dotnet one-shot tools | .NET 10 dnx guidance; environment/tool availability still must be checked |
| systemd behavior | main-branch source docs; deployed version must be checked before relying on every property |

## Rejected or Deferred Sources

- Community tutorials không được dùng để quyết định kernel/protocol semantics.
- roadmap.sh được giữ làm scope discovery, không làm behavioral authority.
- Full Vietnamese translations của Linux man-pages/RFC không được tìm thấy ở mức authoritative; không thêm link yếu chỉ để đủ mục.
- Distro-specific SELinux/AppArmor/NFS operational details được deferred đến environment-specific chapters.
- Packet capture walkthrough được deferred vì cần authorization, privacy và artifact-handling policy riêng.

## Validation Constraints

Workspace hiện chạy Windows PowerShell. WSL chỉ có distro nội bộ của Docker Desktop và Docker daemon không active, nên không có general-purpose Linux runtime phù hợp để chạy các lab. Vì vậy:

- shell commands được review theo man-pages và giới hạn target;
- labs chỉ signal child PID vừa tạo hoặc thao tác trong mktemp sandbox;
- không đưa recursive chmod/chown/delete vào lab;
- complete .NET network example được đưa vào compile verification riêng;
- người học phải chạy Linux labs trên VM/WSL2/Linux tương đương target environment và lưu output làm evidence.

## Artifacts Produced

- [Filesystem, permissions and identities](../02-linux-git-networking/filesystem-permissions-and-identities.md)
- [Processes, signals and resource pressure](../02-linux-git-networking/process-signals-and-resource-pressure.md)
- [DNS, TCP, TLS and HTTP deep dive](../02-linux-git-networking/dns-tcp-tls-http-deep-dive.md)
- Updated [module index](../02-linux-git-networking/README.md)
- Updated [module references](../02-linux-git-networking/references.md)
- Updated [skills matrix](skills-matrix.md)

## Verification Metadata

- Research date: 2026-08-11.
- Primary source policy: official manuals, kernel docs, RFCs and Microsoft Learn.
- Context7 library: /dotnet/docs.
- Context7 query theme: HttpClient pooling, DNS refresh, PooledConnectionLifetime, connection timeout and safe client lifetime.
- Runtime validation: complete .NET network probe compiled with .NET SDK 10.0.301, 0 warnings and 0 errors; 86 unique external Markdown links checked with no confirmed 404/410; Linux command execution deferred with the constraint documented above.
