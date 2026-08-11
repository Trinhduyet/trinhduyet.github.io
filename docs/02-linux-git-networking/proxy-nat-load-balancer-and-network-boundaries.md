# Proxy, NAT, Load Balancer, and Network Boundaries

## Mục tiêu / Learning Objectives

Sau chương này, người học có thể:

- vẽ request path qua network namespace, NAT, L4/L7 load balancer và reverse proxy;
- phân biệt bind address, reachable address, translated address và original client identity;
- giải thích TLS termination/pass-through, health check, draining và connection reuse;
- điều tra 502/503/504 theo từng hop thay vì gộp thành “proxy lỗi”;
- cấu hình ASP.NET Core forwarded headers chỉ từ proxy/network đáng tin;
- thiết kế timeout, retry, observability và security boundaries cho multi-hop topology.

## Tại sao cần học? / Why It Matters

Khi có proxy hoặc NAT, ứng dụng không còn nhìn request giống client. Remote IP có thể là proxy; scheme tại Kestrel có thể là http dù user dùng https; một port LISTEN trong container namespace chưa chắc exposed trên host; load balancer có thể trả 503 trước khi request tới application.

Nếu topology không được viết rõ, team dễ:

- tin header do client tự gửi;
- tạo HTTPS redirect loop;
- kiểm tra socket ở sai namespace;
- retry qua nhiều lớp và khuếch đại load;
- drain load balancer nhưng vẫn kill application trước khi request hoàn tất;
- log client IP/trace không nhất quán qua hops.

## Tổng quan / Overview

~~~mermaid
flowchart LR
    C["Client<br/>source address A"]
    NAT["NAT / gateway<br/>A → B"]
    L4["L4 load balancer<br/>connection routing"]
    L7["L7 reverse proxy<br/>TLS + HTTP routing"]
    APP["Application<br/>namespace + listener"]
    DEP["Dependency"]

    C -->|DNS + TCP/TLS| NAT
    NAT -->|translated flow| L4
    L4 -->|backend connection| L7
    L7 -->|new or reused upstream connection| APP
    APP --> DEP

    L7 -.->|Forwarded metadata from known hop only| APP
~~~

Mỗi mũi tên là một hop có thể có:

- connection pool riêng;
- DNS/address selection riêng;
- TLS identity riêng;
- connect/read/idle timeout riêng;
- retry riêng;
- metrics/logs riêng;
- source address và trust boundary riêng.

## Mental Model

### Namespace trước, address sau

Linux network namespace cô lập network devices, protocol stacks, routing tables, firewall rules và socket port space. localhost luôn có nghĩa “namespace hiện tại”, không phải “host vật lý” hay “mọi container”.

Một service LISTEN 0.0.0.0:8080 trong namespace A không tự xuất hiện ở namespace B. Cần veth/bridge/routing/NAT/port publishing hoặc proxy làm đường nối.

### NAT là stateful address translation

- SNAT thay source address/port, thường cho outbound connection.
- DNAT thay destination address/port, thường để publish/forward inbound traffic.
- NAPT/PAT dịch cả address và transport identifier như TCP/UDP port.
- Conntrack/state mapping phải tồn tại để return traffic được dịch ngược đúng.

NAT không tự là firewall, service discovery, authentication hoặc encryption. Nó làm mất end-to-end meaning của address và thêm state/failure mode vào network.

### Proxy tạo hop mới

Forward proxy đại diện client đi ra ngoài. Reverse proxy đại diện server/application trước client. Reverse proxy L7 thường terminate client connection, đọc HTTP, rồi tạo/reuse connection khác tới upstream.

Vì vậy “client connected” và “proxy connected upstream” là hai events khác nhau.

### L4 và L7 load balancing

| Layer | Quyết định dựa trên | Có thể làm | Không tự biết |
| --- | --- | --- | --- |
| L4 | IP, port, transport connection | Route connection, preserve/pass TLS | HTTP method/path/status nếu không terminate protocol |
| L7 | HTTP/TLS metadata sau termination | Host/path routing, headers, auth/rate policy | Business health nếu probe/contract không biểu diễn đúng |

Một sản phẩm có thể kết hợp L4 và L7; tên sản phẩm không đủ để suy ra boundary.

## Thuật ngữ / Terminology

| Thuật ngữ | Mental model |
| --- | --- |
| Network namespace | Instance tách biệt của network stack/resources |
| veth pair | Hai virtual interfaces nối hai namespace giống pipe hai đầu |
| Bridge | L2 forwarding domain nối interfaces |
| NAT | Dịch address giữa realms |
| SNAT / DNAT | Dịch source / destination |
| Conntrack | State theo flow dùng cho NAT/firewall behavior |
| Forward proxy | Proxy phía client/egress |
| Reverse proxy | Proxy phía server/ingress |
| TLS termination | Proxy giải mã TLS và tạo security boundary mới |
| TLS pass-through | Proxy route encrypted connection mà không terminate TLS |
| Health probe | Request/check do balancer dùng để chọn backend |
| Draining | Ngừng nhận connection/request mới, cho in-flight work hoàn tất |
| Stickiness | Cố gắng route cùng client/session về backend nhất định |
| Forwarded metadata | for/by/host/proto hoặc X-Forwarded-* do proxy thêm |
| Hairpin NAT | Traffic quay vào published address qua NAT từ internal side |

## Prerequisites

- [DNS, TCP, TLS, and HTTP Deep Dive](dns-tcp-tls-http-deep-dive.md).
- [Processes, Signals, and Resource Pressure](process-signals-and-resource-pressure.md).
- Có thể dùng ss, ip, curl và đọc process PID.
- .NET Integration cần hiểu ASP.NET Core middleware ordering.

## How It Works

### 1. Chụp topology thật

Trước khi chạy lệnh, ghi:

- client host/namespace;
- DNS answer và destination client chọn;
- public/private IP, port và protocol của từng hop;
- nơi TLS terminate và certificate identity;
- L4/L7 routing key;
- proxy-to-upstream address/discovery;
- health probe source/path/contract;
- timeout/retry/drain owner từng hop.

Không dùng một box “network” bao trùm mọi thứ.

### 2. Chứng minh namespace của process

Sau khi xác minh PID:

~~~bash
target_pid=12345
case "$target_pid" in
  ''|*[!0-9]*) printf 'invalid pid\n' >&2; exit 64 ;;
esac

readlink "/proc/$target_pid/ns/net"
ip netns identify "$target_pid"
nsenter --target "$target_pid" --net -- ip address show
nsenter --target "$target_pid" --net -- ip route show
nsenter --target "$target_pid" --net -- ss -lntp
~~~

nsenter thường cần privilege phù hợp. Named netns có thể không tồn tại cho container namespace; /proc/PID/ns/net identity vẫn là evidence. /proc/net phản ánh namespace của process đọc nó.

### 3. Theo connection ở từng hop

Tại client:

~~~bash
curl --connect-timeout 2 --max-time 10 -v https://api.example.com/health
ss -ntp state established
~~~

Tại proxy:

- client-side accept/connect state;
- selected upstream/address;
- upstream connect/TLS/TTFB timings;
- route/cluster name và retry count;
- response flag/status producer.

Tại application:

- local/remote socket address thực tế;
- request scheme/host/path sau trusted middleware;
- trace ID, route và response status;
- dependency calls và total deadline còn lại.

### 4. Phân biệt status producer

| Status/symptom | Câu hỏi đầu tiên |
| --- | --- |
| 502 Bad Gateway | Proxy có connect/đọc được upstream protocol hợp lệ không? |
| 503 Service Unavailable | Proxy không có healthy backend, rate/capacity policy hay app chủ động trả? |
| 504 Gateway Timeout | Hop nào hết deadline; upstream đã nhận request chưa? |
| Redirect loop | Scheme/host/path base có bị mất hoặc trusted sai không? |
| Client IP luôn là proxy | Đây là expected transport identity hay thiếu trusted forwarding? |
| Works on host, fails in container | DNS/route/listener nằm ở namespace nào? |

Status chỉ là starting clue. Response headers/logs cần cho biết component tạo response.

### 5. Align lifecycle

Safe rollout thường cần:

~~~text
Mark backend unready
→ load balancer ngừng route work mới
→ chờ propagation / connection drain
→ gửi graceful termination cho app
→ app ngừng nhận work, drain trong budget
→ forced kill chỉ sau deadline
~~~

Nếu termination bắt đầu trước health state propagation, in-flight/new requests có thể rơi vào connection reset hoặc 502/503.

## Minimal Example

Giả sử local diagnostic service listen 127.0.0.1:5080:

~~~bash
ss -lntp '( sport = :5080 )'
curl --silent --show-error http://127.0.0.1:5080/diagnostics/request
curl --silent --show-error \
  -H 'X-Forwarded-For: 203.0.113.50' \
  -H 'X-Forwarded-Proto: https' \
  http://127.0.0.1:5080/diagnostics/request
~~~

Client trực tiếp tự gửi được X-Forwarded-* headers. Vì vậy raw header không phải evidence về client identity hoặc original scheme. Application chỉ được apply metadata từ exact trusted proxy chain.

## Production Example

Symptom: OAuth redirect URI bị tạo thành http, browser gặp redirect loop sau khi đưa ASP.NET Core sau reverse proxy terminate TLS.

Topology:

~~~text
Browser -- HTTPS --> proxy -- HTTP --> Kestrel
~~~

Evidence:

1. Browser/proxy access log chứng minh client dùng HTTPS.
2. Kestrel thấy direct request scheme HTTP từ proxy, đúng với upstream hop.
3. Proxy có gửi X-Forwarded-Proto=https hoặc Forwarded proto=https không?
4. ASP.NET Core có UseForwardedHeaders trước HTTPS redirection/auth không?
5. Direct remote address có nằm trong KnownProxies/KnownNetworks và ForwardLimit đúng số hop không?
6. Host/path-base có được preserve/validate đúng không?

Không sửa bằng cách hard-code Request.Scheme cho mọi request hoặc trust all networks. Fix là mô tả exact proxy chain, cấu hình proxy metadata và trust list, đặt middleware đúng thứ tự, sau đó test direct-bypass path bị từ chối/không được tin.

## .NET Integration

### Trusted forwarded headers

Ví dụ một proxy duy nhất có IP lấy từ configuration:

~~~csharp
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var proxyAddressText = builder.Configuration["TrustedProxy:Address"]
    ?? throw new InvalidOperationException("TrustedProxy:Address is required.");

if (!IPAddress.TryParse(proxyAddressText, out var proxyAddress))
{
    throw new InvalidOperationException("TrustedProxy:Address must be an IP address.");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 1;
    options.KnownProxies.Add(proxyAddress);
    options.AllowedHosts.Add("api.example.com");
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/diagnostics/request", (HttpContext context) => Results.Ok(new
{
    remoteIp = context.Connection.RemoteIpAddress?.ToString(),
    scheme = context.Request.Scheme,
    host = context.Request.Host.Value,
    pathBase = context.Request.PathBase.Value
}));

app.Run();
~~~

Production requirements:

- proxyAddress là direct peer mà app thực sự thấy, không mặc định là public LB address;
- ForwardLimit phản ánh số trusted forwarder entries cần consume;
- KnownProxies/KnownNetworks không được clear để trust mọi source;
- AllowedHosts/host validation phù hợp tenant/domain contract;
- UseForwardedHeaders chạy trước middleware dùng scheme/host/client IP;
- firewall/network policy chặn client bypass proxy nếu architecture yêu cầu.

ASP.NET Core servicing đã harden behavior để bỏ qua X-Forwarded-* từ unknown proxies. Cấu hình topology đúng là bắt buộc; biến môi trường bật broad cloud defaults không thay thế explicit trust design.

## Internals

### Connection identity qua proxy

Application kernel chỉ biết direct TCP peer. Original client address là application metadata do intermediary chuyển tiếp, nên trust phụ thuộc:

- client có thể bypass trusted proxy hay không;
- proxy có xóa header do client gửi trước khi append giá trị authoritative không;
- app chỉ consume entries từ trusted side của chain;
- proxy-to-app channel có integrity/network isolation phù hợp.

### Forwarded list direction

Nhiều proxy append entries tạo list. ASP.NET Core xử lý từ phải sang trái và mặc định ForwardLimit 1. Tăng limit mà không định nghĩa exact chain có thể làm app consume attacker-controlled prefix.

### NAT state và asymmetric path

NAT mapping gắn với flow. Khi return path đi qua thiết bị không có cùng state, connection có thể fail dù routing tới subnet tồn tại. Failover NAT cần state strategy hoặc chấp nhận existing flows reset.

### TLS termination

TLS termination chia security channel thành hai hop. Client-to-proxy certificate không bảo vệ proxy-to-app hop. Backend TLS/mTLS, private network và workload identity là các control độc lập cần threat model.

## Common Mistakes

- Tin X-Forwarded-For từ mọi source để rate limit/authorize.
- Nghĩ NAT tự tạo security boundary hoặc service health.
- Chạy ss/curl ở host trong khi process ở namespace khác.
- Dùng localhost để gọi service ở container/VM khác.
- Gộp LB, ingress, service mesh sidecar và app thành một hop.
- Tăng mọi timeout độc lập, tạo total latency lớn hơn user SLO.
- Retry ở client, proxy và app cùng lúc.
- Health probe chỉ kiểm tra process nhưng route user vào app chưa ready.
- Kill app ngay sau khi remove backend khỏi pool, không chờ propagation/drain.
- Log raw forwarded chain như verified client identity.

## Performance Considerations

- Mỗi proxy hop thêm queue, connection pool, buffer, serialization và timeout.
- TLS termination/resumption dùng CPU; pass-through giảm L7 control nhưng giữ end-to-end encrypted hop.
- Upstream pool quá nhỏ gây queue; quá lớn tăng FD, backend load và connection churn.
- HTTP/2 multiplexing giảm connection count nhưng có shared failure/flow-control behavior.
- Buffering bảo vệ slow clients nhưng tăng memory/disk và delay streaming.
- Load balancing least-connections/round-robin/hash phản ứng khác với request duration và heterogeneous backends.
- Cross-zone/region routing tăng latency, egress cost và failure coupling.

## Security Considerations

- Forwarded/Host headers chỉ đáng tin khi direct proxy và chain được whitelist/secured.
- Original client IP thường là personal data; có retention/access/minimization policy.
- Host header spoofing có thể ảnh hưởng absolute URL, reset link và tenant routing.
- Chặn direct access tới application port nếu authorization/rate/TLS policy nằm ở proxy.
- TLS termination cần certificate/key lifecycle và secure backend hop.
- mTLS xác thực peer hop, không tự chứng minh end-user authorization.
- Proxy admin/status endpoints không public; config dump có thể lộ topology/secrets.
- Request smuggling/desync prevention cần proxy/app parse alignment và supported protocol versions.

## Reliability / Failure Modes

| Failure mode | Evidence phân biệt | Mitigation/prevention |
| --- | --- | --- |
| No healthy upstream | LB health state, app readiness | Sửa probe/capacity/dependency |
| Upstream connect refused | Proxy connect error, app listener | Sửa port/bind/readiness order |
| Upstream connect timeout | route/firewall/namespace | Kiểm tra exact hop policy |
| Idle timeout mismatch | reset sau idle, hop timeout configs | Align keepalive/idle budgets |
| Redirect loop | scheme before/after middleware | Trusted forwarded proto + order |
| Client IP spoof | direct peer + raw header mismatch | Trust exact proxies; block bypass |
| NAT state loss | failover/conntrack timeline | Drain/reconnect/state strategy |
| Uneven load | per-backend RPS/concurrency/latency | Chọn algorithm, capacity weights |
| Retry amplification | attempts ở nhiều hops | Một retry owner/budget |
| Shutdown resets | LB drain vs SIGTERM timeline | Pre-stop/readiness/grace alignment |

## Observability

Mỗi hop nên ghi hoặc export:

- direct peer và selected upstream;
- route/cluster/backend identity;
- DNS/connect/TLS/queue/TTFB/total timing;
- protocol/version và connection reuse;
- response status producer/response flags;
- retry count/reason và remaining deadline;
- health state/drain transitions;
- trace context được propagate, không tự tạo lại ở mọi hop.

Application log phân biệt remoteIp sau trusted middleware và rawForwardedHeader debug field. Raw header không dùng làm primary security/audit identity.

## Operational Considerations

- Topology diagram/runbook phải có IP/port/protocol, TLS owner, DNS owner và health path từng hop.
- Cấu hình proxy/LB được version/review/test/rollback như application code.
- Certificate, DNS, health, drain và idle timeouts có canary/synthetic tests.
- Capacity plan tính cả concurrent connections, handshakes, buffers và cross-zone traffic.
- Change one hop at a time khi có thể; correlate config revision với metrics.
- Namespace diagnostic cần least privilege và exact PID; không dùng privileged shell thường trực.

## Architect Perspective

Trước khi thêm proxy/LB, trả lời:

- requirement là availability, routing, TLS offload, policy hay observability?
- L4 có đủ hay thực sự cần L7?
- hop mới sở hữu retry, rate limit, auth hay chỉ transport?
- client identity được xác lập ở đâu?
- backend có thể bị bypass không?
- health contract đại diện process, readiness hay user journey?
- state/connection bị ảnh hưởng thế nào khi scale/failover/deploy?
- team nào on-call và có đủ telemetry/config knowledge?

Một managed load balancer giảm vận hành thiết bị nhưng không xóa responsibility về timeout, health, trust và cost.

## Trade-offs

| Quyết định | Lợi ích | Đổi lại |
| --- | --- | --- |
| L4 balancing | Ít parse/overhead, pass-through dễ hơn | Ít HTTP-aware routing/policy |
| L7 reverse proxy | Routing/policy/telemetry phong phú | Thêm latency, state và attack surface |
| TLS termination ở edge | Central certificate/policy | Backend hop là security boundary mới |
| End-to-end TLS | Bảo vệ backend hop | Identity/cert/inspection phức tạp hơn |
| Source-IP preservation | Audit/routing trực tiếp | Topology/platform constraints |
| Forwarded headers | Giữ metadata qua L7 hop | Spoof/privacy/trust-chain risk |
| Session stickiness | Hỗ trợ stateful legacy app | Uneven load và failover coupling |
| Multi-zone balancing | Availability | Latency/egress/state coordination |

## When NOT to Use It

- Không thêm L7 proxy chỉ vì “production luôn cần ingress” nếu một managed L4/service endpoint đã đủ.
- Không dùng client IP làm user identity hoặc authorization chính.
- Không dùng NAT thay authentication/network policy.
- Không bật trust-all forwarded headers để chữa redirect loop.
- Không thêm retry ở proxy khi operation side-effect không có idempotency contract.
- Không dùng stickiness để che in-memory session state nếu scale/recovery requirements không cho phép.

## Alternatives

- Direct service endpoint khi topology đơn giản và policy đủ ở application/platform.
- DNS-based load distribution, chấp nhận cache/failover semantics.
- Client-side discovery/balancing khi client ecosystem và ownership cho phép.
- Service mesh khi mTLS/traffic policy/telemetry cross-service đủ giá trị so với complexity.
- API gateway khi cần product/API policy ở edge; không đồng nghĩa mọi internal call đi qua gateway.
- Unix domain socket cho same-host proxy-to-app hop.

## Review Questions

1. Vì sao localhost khác nhau giữa host và container?
2. NAT thay đổi gì và không cung cấp những security guarantees nào?
3. L4 và L7 load balancer quan sát/route dựa trên dữ liệu gì?
4. Vì sao application thấy proxy IP là behavior đúng?
5. X-Forwarded-For trở thành trusted data dưới điều kiện nào?
6. TLS termination tạo thêm trust boundary ra sao?
7. 502, 503 và 504 có thể được tạo ở những component nào?
8. Drain ordering liên quan readiness và SIGTERM thế nào?
9. Retry ở ba hop gây amplification ra sao?
10. Bạn cần fields nào để trace một request qua nhiều proxies?

## Hands-on Lab

### Mục tiêu

Vẽ exact topology và chứng minh raw forwarded headers có thể bị client spoof bằng local incident service.

### Bước 1: chạy incident service

Theo [Incident Lab](incident-lab-dotnet-service.md), chạy service trên loopback port 5080.

### Bước 2: lấy transport identity

~~~bash
curl --silent --show-error http://127.0.0.1:5080/diagnostics/request
~~~

Ghi remoteIp, localIp, scheme, host và raw forwarded headers.

### Bước 3: giả mạo metadata

~~~bash
curl --silent --show-error \
  -H 'X-Forwarded-For: 203.0.113.50' \
  -H 'X-Forwarded-Proto: https' \
  -H 'X-Forwarded-Host: victim.example' \
  http://127.0.0.1:5080/diagnostics/request
~~~

Xác nhận direct remoteIp vẫn là loopback nhưng raw headers mang giá trị client tự chọn. Giải thích vì sao application không được authorize/rate-limit theo raw value.

### Bước 4: topology worksheet

Vẽ một production-like path và điền:

| Hop | Namespace/address | TLS | Health | Timeout | Retry owner | Trusted metadata |
| --- | --- | --- | --- | --- | --- | --- |
| Client → edge | | | | | | |
| Edge → proxy | | | | | | |
| Proxy → app | | | | | | |
| App → dependency | | | | | | |

### Bước 5: failure reasoning

Với các symptom 502, redirect loop và shutdown reset, nêu:

- lớp cuối đã thành công;
- evidence cần lấy ở từng hop;
- một mitigation bounded;
- root-cause/prevention action;
- security impact nếu trust-all header được dùng như workaround.

## Exit Criteria

Hoàn thành khi người học có thể:

- vẽ namespace/NAT/L4/L7/application path không gộp hop;
- giải thích address translation và direct peer identity;
- kiểm tra socket/route trong đúng network namespace;
- cấu hình trusted forwarded headers và middleware order trong ASP.NET Core;
- phân loại proxy status bằng upstream evidence;
- thiết kế health/drain/timeout/retry contract;
- chỉ ra privacy/security boundary của forwarded client metadata.

## Related Topics

- [DNS, TCP, TLS, and HTTP Deep Dive](dns-tcp-tls-http-deep-dive.md)
- [Processes, Signals, and Resource Pressure](process-signals-and-resource-pressure.md)
- [Incident Lab: Diagnose a .NET Service](incident-lab-dotnet-service.md)
- Docker and Kubernetes networking
- API gateways and service mesh
- Zero trust and workload identity

## Official English Sources

- [Linux network namespaces](https://man7.org/linux/man-pages/man7/network_namespaces.7.html)
- [ip-netns(8)](https://man7.org/linux/man-pages/man8/ip-netns.8.html)
- [/proc/PID/net](https://www.man7.org/linux/man-pages/man5/proc_net.5.html)
- [RFC 3022: Traditional NAT](https://www.rfc-editor.org/rfc/rfc3022.html)
- [RFC 7239: Forwarded HTTP Extension](https://www.rfc-editor.org/rfc/rfc7239.html)
- [RFC 9110: HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html)
- [ASP.NET Core proxy and load balancer configuration](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)
- [ASP.NET Core forwarded-header hardening](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/8/forwarded-headers-unknown-proxies?view=aspnetcore-10.0)

## Vietnamese Resources

- [Truy cập ứng dụng mạng với WSL](https://learn.microsoft.com/vi-vn/windows/wsl/networking)

Tài liệu WSL tiếng Việt hỗ trợ nhận biết localhost/NAT/mirrored networking ở môi trường local. Production behavior vẫn phải đối chiếu platform, kernel và proxy documentation thực tế.

## Verification Metadata

- Last verified: 2026-08-11.
- Versions/scope: current iproute2/man-pages accessed on verification date; ASP.NET Core 10 guidance; RFC 3022, 7239 and 9110.
- Version-sensitive note: ASP.NET Core forwarded-header trust behavior includes servicing hardening introduced in 8.0.17/9.0.6 and carried into current guidance.
- Runtime note: unprivileged raw-header experiment is verified with the module incident service; privileged namespace mutation is intentionally not required by the lab.
