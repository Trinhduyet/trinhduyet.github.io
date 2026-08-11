# DNS, TCP, TLS, and HTTP Deep Dive

## Mục tiêu / Learning Objectives

Sau chương này, người học có thể:

- theo dấu một request từ name resolution đến application response;
- phân biệt DNS answer, route, TCP connection, TLS identity và HTTP semantics;
- dùng evidence để tách timeout, refusal, reset, certificate và HTTP failure;
- giải thích connection pooling, DNS refresh và timeout budget trong .NET HttpClient;
- điều tra đúng network namespace, proxy/load balancer và endpoint boundary;
- thiết kế deadline, retry, idempotency và observability cho production call path.

## Tại sao cần học? / Why It Matters

“Network error” không phải root cause. Cùng symptom timeout có thể do resolver không trả lời, SYN bị drop, connection pool giữ IP cũ, TLS SNI sai, proxy hết upstream connection, server chờ dependency hoặc client hết total deadline.

Mỗi lớp có contract và evidence riêng. Chẩn đoán nhanh nhất là xác định lớp cuối cùng đã thành công, rồi kiểm tra boundary kế tiếp.

## Tổng quan / Overview

~~~text
Application URL
  ↓ parse scheme, host, port, path
Resolver/cache ── DNS records and TTL
  ↓ addresses
Routing table / network namespace / NAT
  ↓ packets
TCP handshake or QUIC connection
  ↓ reliable byte stream for HTTP/1.1 and HTTP/2
TLS handshake ── server identity, trust, crypto, ALPN
  ↓ protected channel
HTTP request/response semantics
  ↓
Proxy / load balancer / application / dependency
~~~

HTTP/1.1 và HTTP/2 thường chạy trên TCP; HTTP/3 dùng QUIC trên UDP. HTTP semantics về method, status và fields được định nghĩa độc lập với version transport.

## Mental Model

### Chứng minh theo lớp

| Evidence | Chứng minh | Chưa chứng minh |
| --- | --- | --- |
| DNS có A/AAAA | Resolver trả address | Route/connect hoặc address còn phục vụ |
| Route tồn tại | Kernel chọn next hop/interface | Packet không bị drop |
| TCP connect thành công | Đã tới listening socket | TLS/application healthy |
| TLS handshake thành công | Crypto/identity negotiation đạt | HTTP response/business success |
| HTTP response headers | Server/proxy đã xử lý request tới HTTP | Body hoàn chỉnh hoặc dependency healthy |
| HTTP 2xx | Request cụ thể thành công | Toàn bộ user journey/SLO đạt |

### TCP là byte stream

TCP không bảo toàn message boundaries. Một write không tương ứng một read. Application protocol phải tự framing. TCP retransmission/order không cung cấp encryption hoặc peer identity, và connection idle không tự chứng minh peer còn healthy.

### DNS TTL không phải connection lifetime

Resolver cache có TTL, nhưng application có thể giữ connection đã mở qua lúc record đổi. DNS cutover vì vậy phụ thuộc cả cache behavior lẫn pool lifetime, load balancer draining và application retry/deadline.

## Thuật ngữ / Terminology

| Thuật ngữ | Mental model |
| --- | --- |
| Stub resolver | Library/local component nhận query từ application |
| Recursive resolver | Resolver tìm/cached answer thay client |
| Authoritative server | Nguồn authoritative cho zone/record |
| TTL | Thời gian cache record được phép giữ theo DNS contract |
| NXDOMAIN | Tên không tồn tại theo authoritative result |
| SERVFAIL | Resolver không hoàn thành query; khác NXDOMAIN |
| Socket address | IP address + transport port |
| SYN backlog | Queue/kernel state trong quá trình connection establishment |
| RST | TCP reset; endpoint/path chủ động từ chối hoặc hủy state |
| SNI | Hostname client gửi trong TLS handshake để chọn certificate/site |
| ALPN | Thương lượng application protocol, ví dụ h2 hoặc http/1.1 |
| Certificate chain | Leaf certificate và issuer chain đến trust anchor |
| Host header / :authority | HTTP authority dùng để route virtual host |
| Keep-alive / pooling | Tái sử dụng connection cho nhiều request |
| Deadline | Thời điểm operation phải kết thúc |
| Retry budget | Giới hạn retry theo time/attempt/load, không phải vòng lặp vô hạn |

## Prerequisites

- [Production Troubleshooting Foundations](production-troubleshooting-foundations.md).
- curl và getent; ss, ip, openssl, dig/resolvectl là hữu ích.
- Hiểu process/socket và quyền chạy diagnostic trong đúng namespace.

## How It Works

### 1. Parse endpoint trước khi đo

Ghi rõ:

- scheme: http hoặc https;
- hostname, port effective và path;
- proxy configuration;
- IPv4/IPv6 preference;
- caller namespace/host/pod;
- expected virtual host/SNI;
- total deadline và retry policy.

Một URL thiếu scheme/port context khiến evidence dễ bị gắn sai lớp.

### 2. DNS resolution

~~~bash
getent ahosts api.example.com
resolvectl query api.example.com
dig api.example.com A
dig api.example.com AAAA
~~~

getent gần với system resolver path hơn dig trực tiếp. dig hữu ích để xem DNS protocol/record và chọn resolver cụ thể, nhưng không phải lúc nào cũng phản ánh nsswitch, hosts file hoặc application resolver behavior.

Ghi answer, resolver, TTL, query latency và error category. NXDOMAIN, timeout và SERVFAIL là các failure khác nhau.

### 3. Route và reachability

~~~bash
ip route get 203.0.113.10
ip address show
ip rule show
~~~

Chạy trong cùng network namespace với caller. Route tồn tại chỉ chứng minh kernel có quyết định; firewall/security group/NAT vẫn có thể drop.

### 4. TCP

~~~bash
ss -lntp
ss -ntp state established
curl --connect-timeout 2 --max-time 10 -v http://api.example.com/health
~~~

Phân loại:

- connection refused thường cho thấy RST/no listener ở endpoint được chạm tới;
- connect timeout thường gợi ý drop, unreachable path hoặc silent endpoint;
- reset after connect cho thấy connection state bị hủy trong/giữa exchange;
- read timeout nghĩa là đã đi xa hơn connect nhưng response không đến trong budget.

Đây là clues, không phải quy luật tuyệt đối; proxy/NAT có thể tạo lại error behavior.

### 5. TLS

~~~bash
openssl s_client \
  -connect api.example.com:443 \
  -servername api.example.com \
  -verify_hostname api.example.com \
  -showcerts
~~~

Kiểm tra:

- SNI/hostname có đúng virtual host;
- certificate validity window;
- subject alternative names;
- chain và trust anchor;
- negotiated TLS version/cipher/ALPN;
- clock của client/server;
- mTLS client certificate nếu contract yêu cầu.

Không dùng tùy chọn bỏ certificate verification làm “fix” production.

### 6. HTTP

~~~bash
curl --silent --show-error --output /dev/null \
  --write-out 'code=%{http_code} dns=%{time_namelookup} connect=%{time_connect} tls=%{time_appconnect} first_byte=%{time_starttransfer} total=%{time_total}\n' \
  https://api.example.com/health
~~~

Kết hợp status, headers, timing và trace/request ID. 502/503/504 có thể do proxy tạo; cần biết response producer và upstream evidence. TTFB cao có thể ở app/dependency, không chỉ network.

### 7. Bypass có kiểm soát để phân biệt layer

curl --resolve giữ hostname cho Host/SNI nhưng ép một IP:

~~~bash
curl --resolve api.example.com:443:203.0.113.10 \
  --connect-timeout 2 --max-time 10 \
  https://api.example.com/health
~~~

Nếu ép IP thành công còn normal resolution thất bại, DNS/cache/address selection là hướng mạnh. Nếu cả hai thất bại giống nhau, tiếp tục ở route/TCP/TLS/app. Không dùng kết quả một lần để kết luận permanent root cause.

## Minimal Example

Terminal 1:

~~~bash
python3 -m http.server 8080 --bind 127.0.0.1
~~~

Terminal 2:

~~~bash
getent ahosts localhost
ss -lntp '( sport = :8080 )'
curl --resolve local.test:8080:127.0.0.1 \
  --connect-timeout 1 --max-time 3 -v \
  http://local.test:8080/
~~~

--resolve bypass DNS answer nhưng vẫn gửi authority local.test. Lab chứng minh name, address, listening socket và HTTP response là các boundary riêng.

## Production Example

Symptom: sau DNS cutover, một số .NET replicas tiếp tục gọi IP cũ; curl mới trên cùng host thấy IP mới.

Giả thuyết cần tách:

- resolver cache chưa hết TTL;
- process/application dùng resolver path khác curl;
- connection pool vẫn tái sử dụng connection cũ;
- proxy/service mesh cache upstream;
- IPv4 và IPv6 answers khác nhau;
- rollout chỉ tác động một phần replicas;
- endpoint cũ vẫn accept nhưng trả chậm/lỗi.

Evidence:

1. Ghi DNS answers/TTL từ caller namespace.
2. Ghi actual remote address của active sockets.
3. Kiểm tra HttpClient handler lifetime/pool configuration.
4. Correlate replica start time, deployment và DNS change time.
5. Test --resolve với cả IP cũ/mới, giữ đúng Host/SNI.
6. Kiểm tra proxy/mesh/LB logs trước khi ép restart toàn fleet.

Mitigation có thể là drain connection pool hoặc rolling restart có kiểm soát, nhưng prevention là đặt connection lifetime phù hợp DNS/failover contract và test cutover.

## .NET Integration

### HttpClient lifetime

HttpClient dùng connection pool. Tạo/dispose client cho từng request gây churn, TIME_WAIT và có thể cạn port. Hai pattern được tài liệu .NET khuyến nghị:

- long-lived HttpClient với SocketsHttpHandler.PooledConnectionLifetime;
- short-lived clients được tạo bởi IHttpClientFactory, nơi handler được pool theo lifetime.

HttpClient resolve DNS khi tạo connection và không tự theo dõi TTL DNS. PooledConnectionLifetime giới hạn tuổi connection trong pool để connection mới resolve lại DNS; giá trị cần dựa trên TTL, cutover và traffic behavior, không copy máy móc.

### Complete minimal client

~~~csharp
using System.Net;

if (args.Length != 1 || !Uri.TryCreate(args[0], UriKind.Absolute, out var endpoint))
{
    Console.Error.WriteLine("Usage: NetworkProbe https://host/path");
    return 64;
}

var handler = new SocketsHttpHandler
{
    AutomaticDecompression = DecompressionMethods.All,
    ConnectTimeout = TimeSpan.FromSeconds(2),
    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
};

using var client = new HttpClient(handler)
{
    Timeout = TimeSpan.FromSeconds(10)
};

using var requestDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(3));

try
{
    using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
    using var response = await client.SendAsync(
        request,
        HttpCompletionOption.ResponseHeadersRead,
        requestDeadline.Token);

    Console.WriteLine(
        $"status={(int)response.StatusCode} version={response.Version} endpoint={endpoint.Host}");
    return response.IsSuccessStatusCode ? 0 : 1;
}
catch (OperationCanceledException) when (requestDeadline.IsCancellationRequested)
{
    Console.Error.WriteLine("Per-request deadline exceeded.");
    return 28;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"HTTP transport failure: {exception.Message}");
    return 69;
}
~~~

Trong ví dụ, per-request deadline 3 giây ngắn hơn HttpClient.Timeout 10 giây nên thường thắng. Production code cần phân biệt caller cancellation, request deadline và client-wide timeout bằng structured telemetry; không log token/header nhạy cảm.

### IHttpClientFactory caveats

- Không cache typed client lâu hơn intended lifetime vì có thể giữ handler/address cũ.
- Không đưa CookieContainer semantics vào shared pooled handler nếu cookie isolation quan trọng.
- Factory quản lý handler pooling, không tự thiết kế timeout/retry/idempotency cho application.
- Retry cần nhận biết method/body replayability và downstream load.

## Internals

### DNS hierarchy và caching

DNS là distributed hierarchical database được chia thành zones. Recursive resolver cache positive và negative answers theo protocol/policy. Record change không tức thời đồng nhất vì caches, propagation workflow và connection reuse.

### TCP establishment và close

TCP dùng sequence/acknowledgment/retransmission để cung cấp ordered reliable byte stream. Connection identity gồm local/remote address và ports. Active close có thể để socket ở TIME_WAIT nhằm xử lý delayed segments; churn connection cao làm tăng ephemeral-port pressure.

### TLS identity

TLS bảo vệ channel khi peer identity được xác minh đúng. Certificate chain hợp lệ nhưng hostname không khớp vẫn thất bại. SNI giúp server chọn certificate; HTTP Host/:authority giúp route request sau handshake. Hai giá trị thường giống nhưng thuộc hai lớp khác nhau.

### HTTP semantics

Safe method không có nghĩa request không tiêu resource. Idempotent method nghĩa lặp lại intended effect tương đương, nhưng implementation hoặc external effects có thể phá giả định. Retry POST/payment cần idempotency key/operation state, không chỉ method-based rule.

## Common Mistakes

- Ping thất bại rồi kết luận service down, hoặc ping thành công rồi kết luận HTTP healthy.
- Dùng dig làm bằng chứng duy nhất cho resolver path của application.
- Curl bằng IP làm mất đúng SNI/Host và tạo lỗi giả.
- Bỏ certificate verification để vượt TLS error.
- Tạo HttpClient mới cho mỗi request.
- Giữ pooled connection vô hạn trong môi trường DNS failover.
- Retry mọi timeout/5xx mà không xét idempotency và remaining deadline.
- Cộng độc lập các timeout khiến một user request sống lâu hơn SLO nhiều lần.
- Chỉ đo average latency, bỏ p95/p99 và phase timing.
- Capture packet chứa secrets mà không có authorization/retention policy.

## Performance Considerations

- DNS cache giảm latency/query load nhưng tăng cutover lag.
- Connection pooling tránh handshake và port churn; pool quá lâu giữ stale endpoint.
- TLS session resumption giảm handshake cost nhưng vẫn cần lifetime/security policy.
- HTTP/2 multiplexing giảm connection count nhưng một connection có thể trở thành shared blast radius.
- Large headers/body buffering tăng allocation và time-to-first-byte/total time.
- Compression giảm bandwidth nhưng dùng CPU và có security considerations với secrets.
- Retry khuếch đại load đúng lúc dependency đang yếu; thêm backoff, jitter và retry budget.

## Security Considerations

- Luôn xác minh hostname và certificate chain; pinning chỉ dùng khi có rotation/recovery plan.
- DNS không mặc nhiên cung cấp end-to-end authenticity; TLS identity vẫn cần thiết.
- Không log Authorization, cookies, client certificates hoặc full sensitive URLs.
- SSRF defense cần validate scheme, resolved addresses, redirect và rebinding ở đúng boundary.
- Proxy trust configuration phải giới hạn trusted hops trước khi dùng forwarded headers.
- mTLS xác thực channel peer, không thay authorization/business identity.
- Packet capture là dữ liệu nhạy cảm; giới hạn interface/filter/duration và access.

## Reliability / Failure Modes

| Failure mode | Evidence phân biệt | Response |
| --- | --- | --- |
| NXDOMAIN | Resolver trả name error | Sửa name/zone/deploy order |
| SERVFAIL/timeout | Resolver-specific result/timing | Kiểm tra resolver/authority/network |
| Stale address | DNS answer mới nhưng active socket IP cũ | Drain/pool lifetime/cutover plan |
| Refused | Fast connect failure/RST | Kiểm tra listener/address/port |
| Silent drop | Connect timeout/retransmission | Firewall/route/NAT/security policy |
| TLS hostname mismatch | Verify error, SAN/SNI evidence | Sửa endpoint/certificate routing |
| Expired/untrusted chain | Validity/chain error | Renew/deploy trust chain |
| HTTP 502 | Proxy log + upstream connect/reset | Điều tra proxy-to-upstream hop |
| HTTP 503 | Capacity/readiness/rate policy | Shed load/restore healthy capacity |
| HTTP 504 | Proxy deadline, upstream timeline | Align budgets, fix slow dependency |
| Port exhaustion | Socket states, ephemeral range, churn | Reuse pools, reduce concurrency/churn |
| Retry storm | Attempts/request và dependency load | Stop retries, backoff/budget/circuit |

## Observability

Client span/metric nên có, theo policy cardinality:

- logical destination/service, scheme và protocol version;
- DNS/connect/TLS/TTFB/total duration khi stack hỗ trợ;
- selected remote address ở debug sampling phù hợp;
- status/error category và timeout phase;
- retry attempt, remaining deadline và result;
- trace/request ID đi qua proxy/application;
- pool/connection counters và ephemeral-port health.

.NET networking phát EventSource events cho DNS, sockets, TLS, HTTP và connection pooling. Dùng dotnet-counters/EventPipe/OpenTelemetry để correlate, tránh enable verbose payload logging mặc định.

## Operational Considerations

- DNS cutover runbook phải nêu TTL, pool lifetime, old-endpoint drain và rollback window.
- Certificate renewal cần alert trước expiry, chain validation và canary handshake.
- Timeout budget phải giảm dần theo call tree; downstream timeout ngắn hơn caller deadline đủ để cleanup/retry có kiểm soát.
- Load balancer health, application readiness và dependency readiness không nên bị gộp thành một probe mơ hồ.
- Dual-stack cần test IPv4/IPv6 và fallback behavior từ production-like namespace.
- Khi service mesh/proxy tồn tại, caller-to-proxy và proxy-to-upstream là hai hop riêng.

## Architect Perspective

Một network call contract production cần ghi:

- authoritative service identity/name và discovery mechanism;
- protocol/version, TLS/mTLS và trust ownership;
- connection/pool lifetime và DNS/failover expectations;
- total deadline, per-hop timeout, retry/idempotency policy;
- concurrency, connection và rate limits;
- proxy/LB topology cùng failure ownership;
- telemetry fields và incident evidence path.

Nếu những điều này chỉ nằm trong default của library, architecture đang phụ thuộc hành vi khó kiểm soát.

## Trade-offs

| Quyết định | Lợi ích | Đổi lại |
| --- | --- | --- |
| TTL thấp | Cutover nhanh hơn | Query load/cache miss nhiều hơn |
| Pool lifetime dài | Ít handshake, throughput tốt | Stale endpoint/failover chậm |
| Pool lifetime ngắn | DNS refresh nhanh | Handshake/port/CPU cost |
| Retry nhiều | Che transient fault | Load amplification và latency tail |
| mTLS | Strong workload channel identity | Certificate lifecycle/rotation complexity |
| HTTP/2 multiplexing | Ít connection, concurrency cao | Shared connection failure/flow-control coupling |
| Proxy trung gian | Central policy/observability | Thêm hop, config và failure mode |

## When NOT to Use It

- Không dùng raw TCP nếu cần HTTP semantics, ecosystem và observability đã có sẵn.
- Không dùng IP cố định thay service discovery chỉ để né DNS issue.
- Không tắt TLS verification để xử lý certificate lifecycle kém.
- Không retry operation non-idempotent nếu chưa có idempotency/reconciliation design.
- Không capture toàn bộ traffic production khi targeted counters/logs đủ trả lời.

## Alternatives

- Unix domain sockets cho local same-host IPC.
- Message broker cho asynchronous decoupling và controlled redelivery.
- Service mesh cho centralized transport policy khi tổ chức chấp nhận complexity.
- gRPC/HTTP/2 cho typed RPC/streaming, vẫn cần deadline và pooling design.
- QUIC/HTTP/3 khi network path/use case có lợi và operational stack hỗ trợ.

## Review Questions

1. Vì sao DNS answer mới không đảm bảo process đã gọi IP mới?
2. getent và dig trả lời hai câu hỏi khác nhau thế nào?
3. TCP connect thành công không chứng minh những lớp nào?
4. SNI và HTTP Host/:authority khác nhau ở đâu?
5. Tại sao curl trực tiếp IP có thể tạo TLS failure giả?
6. PooledConnectionLifetime giải quyết boundary nào và không giải quyết gì?
7. Retry timeout có thể làm incident nặng hơn như thế nào?
8. Evidence nào tách 502 do proxy không connect upstream với 500 do application?

## Hands-on Lab

### Lab A: đi qua từng boundary local

1. Chạy local server từ Minimal Example.
2. Ghi getent, ss và curl verbose output.
3. Dừng server bằng Ctrl+C.
4. Chạy lại curl cùng timeout và ghi connection-refused evidence.
5. Chạy:

~~~bash
getent ahosts intentionally-missing.invalid
printf 'resolver_exit=%s\n' "$?"
~~~

.invalid được dành riêng cho tên chắc chắn invalid; không phụ thuộc domain ngẫu nhiên.

### Lab B: tách name khỏi address

Khởi động lại server rồi so sánh:

~~~bash
curl --resolve blue.local:8080:127.0.0.1 http://blue.local:8080/
curl --resolve green.local:8080:127.0.0.1 http://green.local:8080/
~~~

Quan sát cả hai name đi cùng address/socket. Nếu server có virtual-host routing, Host có thể làm response khác dù IP giống nhau.

### Lab C: .NET pool/deadline

1. Tạo console project net10.0 và đặt complete minimal client vào Program.cs.
2. Chạy với local server và một endpoint không listen.
3. Ghi exit code, elapsed time và exception category.
4. Giải thích timeout nào ngắn nhất và tại sao client không được tạo per request.
5. Không dùng endpoint production hoặc certificate bypass trong lab.

## Exit Criteria

Hoàn thành khi người học có thể:

- xác định lớp cuối đã thành công bằng evidence;
- phân biệt NXDOMAIN, resolver failure, refusal, timeout, reset, TLS và HTTP error;
- dùng --resolve mà vẫn giữ đúng hostname semantics;
- thiết kế HttpClient lifetime, DNS refresh và deadline có chủ đích;
- mô tả retry/idempotency/load amplification;
- vẽ topology có namespace, proxy/LB, application và dependencies.

## Related Topics

- [Production Troubleshooting Foundations](production-troubleshooting-foundations.md)
- [Process, Signals, and Resource Pressure](process-signals-and-resource-pressure.md)
- Reverse proxy, NAT and load balancing
- Distributed tracing and OpenTelemetry
- Resilience patterns and SLO engineering
- Secure service-to-service communication

## Official English Sources

- [RFC 1034: Domain Names - Concepts and Facilities](https://www.rfc-editor.org/rfc/rfc1034.html)
- [RFC 1035: Domain Names - Implementation and Specification](https://www.rfc-editor.org/info/rfc1035/)
- [RFC 9293: Transmission Control Protocol](https://www.rfc-editor.org/info/rfc9293/)
- [RFC 8446: TLS 1.3](https://www.rfc-editor.org/info/rfc8446/)
- [RFC 9110: HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html)
- [.NET HttpClient guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [IHttpClientFactory troubleshooting](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory-troubleshooting)
- [.NET networking events](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/telemetry/events)

## Vietnamese Resources

- [Truy cập ứng dụng mạng với WSL](https://learn.microsoft.com/vi-vn/windows/wsl/networking)

RFC và tài liệu .NET tiếng Anh là nguồn chuẩn. Tài liệu WSL tiếng Việt hữu ích cho local lab nhưng WSL/NAT behavior không được mặc định đại diện production Linux/container network.

## Verification Metadata

- Last verified: 2026-08-11.
- Versions/scope: RFC 1034/1035 DNS foundations, RFC 9293 current TCP Internet Standard, TLS 1.3 RFC 8446, HTTP semantics RFC 9110, .NET 10 HttpClient guidance.
- Context7: /dotnet/docs; query covered HttpClient pooling, DNS refresh and PooledConnectionLifetime.
- Runtime note: local Linux labs were source-reviewed but not executed because no active general-purpose Linux runtime was available. The C# example is validated separately in the module verification pass.
