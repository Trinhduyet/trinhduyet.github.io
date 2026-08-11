# Linux, Git và Networking cho Production Engineering

> [← README tổng quan](../../README.md) · [Master roadmap](../00-roadmap/master-roadmap.md)

## Mục tiêu module

Sau module này, người học không chỉ biết chạy lệnh. Người học phải có thể lần theo một request thất bại từ process đến socket, route, DNS, TCP/TLS và HTTP; đồng thời dùng Git để điều tra và phục hồi thay đổi an toàn.

Module này đứng đầu vì nó mở khóa:

- ASP.NET Core/Kestrel troubleshooting;
- Docker process, filesystem, signals và networking;
- Kubernetes probes, Services, DNS và resource diagnosis;
- distributed systems failure reasoning;
- observability và incident response.

## Prerequisites

- Terminal và file/path cơ bản.
- Khái niệm client/server, process và IP address ở mức awareness.
- Một Linux VM, máy Linux hoặc WSL2 cho lab.
- Không yêu cầu Docker/Kubernetes.

## Mental model

~~~mermaid
flowchart TD
    INTENT["User intent"] --> GIT["Git revision + configuration"]
    GIT --> PROCESS["Linux process + identity + resources"]
    CLIENT["Client URL"] --> DNS["DNS → address"]
    DNS --> ROUTE["Namespace + route + NAT"]
    ROUTE --> TRANSPORT["TCP / UDP / QUIC"]
    TRANSPORT --> TLS["TLS identity + protected channel"]
    TLS --> HTTP["HTTP semantics"]
    HTTP --> PROXY["Proxy / load balancer"]
    PROXY --> SOCKET["Application listening socket"]
    PROCESS --> SOCKET
    SOCKET --> DEP["Application dependency"]
    DEP --> EVIDENCE["Logs · metrics · traces · incident evidence"]
~~~

Mỗi lớp có evidence riêng. HTTP 503 không chứng minh DNS lỗi; timeout không chứng minh server “down”; process tồn tại không chứng minh port đang listen.

## Roadmap theo layer

| Layer | Chủ đề | Priority | Artifact |
| --- | --- | --- | --- |
| L0 | Linux/network stack giải quyết vấn đề gì; Git lưu lịch sử thế nào | P0 | Module overview |
| L1 | Filesystem, permissions, users/groups | P0 | [Filesystem, permissions and identities](filesystem-permissions-and-identities.md) |
| L1 | Processes, signals and resource boundaries | P0 | [Processes, signals and resource pressure](process-signals-and-resource-pressure.md) |
| L1 | DNS, ports, sockets, TCP, TLS, HTTP | P0 | [DNS, TCP, TLS and HTTP deep dive](dns-tcp-tls-http-deep-dive.md) |
| L1 | Git working tree, index, commit, refs | P0 | [Git mental model and safe recovery](git-mental-model-and-safe-recovery.md) |
| L2 | curl, ss, ps, top, lsof, journalctl, grep, awk, ip, getent | P0 | [Production troubleshooting foundations](production-troubleshooting-foundations.md) |
| L2 | Git diff/log/restore/revert/branch/merge | P0 | [Git mental model and safe recovery](git-mental-model-and-safe-recovery.md) |
| L3 | CPU/memory/I/O pressure; service lifecycle | P0 | [Processes, signals and resource pressure](process-signals-and-resource-pressure.md) |
| L3 | NAT, reverse proxy, load balancer, WSL/container boundaries | P0 | [Proxy, NAT, load balancer and network boundaries](proxy-nat-load-balancer-and-network-boundaries.md) |
| L4 | Scheduling, virtual memory, filesystem/cache và TCP state | P1/P2 | Integrated into current deep dives |
| L5 | Failure domains, security boundary, capacity và architecture trade-offs | P0 | [.NET incident lab](incident-lab-dotnet-service.md) |

Không tạo file cho Planned topic cho đến khi nội dung đủ hữu ích.

## Learning slices

1. [Production troubleshooting foundations](production-troubleshooting-foundations.md): symptom → layer → evidence → hypothesis → experiment.
2. [Filesystem, permissions and identities](filesystem-permissions-and-identities.md): path resolution, ownership, modes, ACL/capabilities, mounts và secret exposure.
3. [Processes, signals and resource pressure](process-signals-and-resource-pressure.md): lifecycle, graceful shutdown, cgroups, PSI và .NET diagnostics.
4. [DNS, TCP, TLS and HTTP deep dive](dns-tcp-tls-http-deep-dive.md): protocol path, connection pooling và timeout/error interpretation.
5. [Git mental model and safe recovery](git-mental-model-and-safe-recovery.md): working tree/index/objects/refs, revert, reflog và provenance.
6. [Proxy, NAT, load balancer and network boundaries](proxy-nat-load-balancer-and-network-boundaries.md): namespace, address translation, trusted forwarding và draining.
7. [.NET incident lab](incident-lab-dotnet-service.md): ứng dụng không reachable/not-ready, dependency failure, header spoof và graceful shutdown.

Toàn bộ 7/7 learning slices đã có content v1. Trạng thái năng lực của người học vẫn phụ thuộc lab output/evidence, không tự hoàn thành theo trạng thái tài liệu.

## Exit criteria

Người học phải chứng minh có thể:

- xác định process nào listen một port và dưới identity nào;
- phân biệt “bind loopback”, “không listen”, “DNS sai”, “TCP bị chặn”, “TLS fail”, “HTTP fail”;
- đọc process state và resource evidence mà không suy diễn từ một metric;
- giải thích SIGTERM khác SIGKILL và tác động đến graceful shutdown;
- dùng journal/system logs và correlation time để dựng timeline;
- dùng Git status/diff/log/revert/restore đúng mục đích;
- vẽ traffic path qua proxy/load balancer và chỉ ra trust/failure boundaries;
- thực hiện failure experiment, ghi hypothesis, evidence, kết luận và rollback.

## Architect perspective

Linux/networking knowledge giúp đặt câu hỏi đúng:

- boundary nào thực sự cô lập lỗi hoặc privilege;
- health check đo process, transport hay business readiness;
- timeout budget bị tiêu ở lớp nào;
- scale tăng connection, bandwidth, file descriptor, queue và log volume ra sao;
- proxy/orchestrator thêm failure mode và operational cost gì;
- team có đủ telemetry/runbook để sở hữu topology hay không.

## References

Nguồn module được quản lý tại [references.md](references.md). Technology baseline chung nằm tại [../00-roadmap/technology-baseline.md](../00-roadmap/technology-baseline.md).

## Verification metadata

- Verified: 2026-08-11
- Technology version: Linux concepts; .NET 10 integration; HTTP semantics RFC 9110
- Official sources: [references.md](references.md)
- Context7 queries used: /dotnet/docs cho HttpClient pooling/DNS refresh và .NET diagnostics; .NET baseline dùng /dotnet/core ở roadmap baseline
- Notes: content v1 hoàn thành 7/7 slices. Command output có thể khác theo distro, init system, cgroup version, network namespace và privilege; Linux signal/namespace lab evidence vẫn cần người học thực hiện trên môi trường phù hợp.

<!-- Mermaid.js Script CDN hỗ trợ tự động render sơ đồ Mermaid trên GitHub Pages (Jekyll) -->
<script type="module">
  import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
  mermaid.initialize({ startOnLoad: true, theme: 'default' });

  document.addEventListener("DOMContentLoaded", function () {
    const elements = document.querySelectorAll("pre.language-mermaid, code.language-mermaid, .language-mermaid pre, pre code.language-mermaid");
    elements.forEach((el) => {
      const container = el.tagName.toLowerCase() === "code" ? el.parentElement : el;
      const div = document.createElement("div");
      div.className = "mermaid";
      div.textContent = el.textContent;
      if (container && container.parentNode) {
        container.parentNode.replaceChild(div, container);
      }
    });
    mermaid.run({ querySelector: '.mermaid' });
  });
</script>
