# Production Troubleshooting Foundations: Process → Socket → HTTP

## Mục tiêu / Learning Objectives

Sau chương này, người học có thể:

- chuyển symptom thành giả thuyết theo từng lớp thay vì đoán;
- chứng minh process có tồn tại, có listen đúng address/port và có nhận connection;
- phân biệt lỗi DNS, route, TCP, TLS, HTTP, application và dependency;
- dựng timeline từ process state, socket state, logs và request timing;
- giải thích graceful shutdown của ứng dụng .NET trên Linux;
- thực hiện failure experiment an toàn và ghi evidence có thể review.

## Tại sao cần học? / Why It Matters

Các câu “server down”, “network chậm” hoặc “API timeout” đều quá rộng. Một request có thể thất bại dù:

- process vẫn chạy nhưng không listen;
- socket chỉ bind loopback;
- DNS trả IP cũ;
- TCP bị firewall drop;
- TLS certificate/hostname không hợp lệ;
- reverse proxy không kết nối được upstream;
- application chờ SQL/Redis/model provider;
- response thành công nhưng vượt latency SLO.

Troubleshooting tốt rút ngắn thời gian từ symptom đến lớp lỗi, sau đó mới tìm root cause. Nó không bắt đầu bằng restart.

## Tổng quan / Overview

Phương pháp:

~~~text
Observe symptom
  ↓
Bound the failure by layer
  ↓
Collect the smallest discriminating evidence
  ↓
Form one falsifiable hypothesis
  ↓
Run a controlled experiment
  ↓
Mitigate, verify, then find root cause
  ↓
Record timeline and prevention action
~~~

“Discriminating evidence” là bằng chứng giúp loại ít nhất một nhánh. Ví dụ:

- ss cho biết socket đang LISTEN hay không;
- curl tách DNS/connect/TLS/first-byte/total time;
- HTTP status chứng minh đã đi xa hơn TCP/TLS;
- trace span chỉ ra dependency nào tiêu timeout budget.

## Mental Model

### Request path

~~~text
Client process
  │
  ├─ resolve name ─────── DNS
  │
  ├─ choose route ─────── routing table / NAT
  │
  ├─ establish session ── TCP or QUIC
  │
  ├─ secure channel ───── TLS
  │
  └─ exchange message ─── HTTP
                          │
                          ▼
                    proxy / load balancer
                          │
                          ▼
                  listening application
                          │
                          ▼
                    dependencies
~~~

### Evidence boundaries

| Evidence | Điều nó chứng minh | Điều nó không chứng minh |
| --- | --- | --- |
| Process có PID | Một process entry tồn tại | Process healthy hoặc listen |
| Port LISTEN | Kernel có listening socket | Business readiness |
| TCP connect thành công | Path đến socket hoạt động | TLS/HTTP/application đúng |
| TLS handshake thành công | Identity/crypto negotiation đạt | HTTP response đúng |
| HTTP 200 | Request cụ thể thành công | Toàn bộ dependency/tenant/workload healthy |
| Readiness 200 | Probe contract đang đạt | User journey/SLO chắc chắn đạt |

## Thuật ngữ / Terminology

| Thuật ngữ | Mental model |
| --- | --- |
| Process | Chương trình đang thực thi với PID, identity, memory và resource handles |
| Thread | Đơn vị execution được scheduler quản lý trong process |
| File descriptor | Handle số của process đến file, socket, pipe hoặc kernel object |
| Socket | Endpoint giao tiếp; listening socket nhận connection mới |
| Bind | Gắn socket với local address/port |
| Listen | Đưa stream socket vào trạng thái nhận connection |
| Loopback | Interface chỉ host cục bộ, thường 127.0.0.1 hoặc ::1 |
| Route | Quyết định interface/next hop cho destination |
| DNS resolution | Chuyển tên thành record/address; có cache và TTL |
| TCP handshake | Thiết lập connection stateful, không đảm bảo application response |
| TLS handshake | Thương lượng crypto và xác minh identity/certificate |
| HTTP semantics | Method, target, fields, status và representation độc lập transport version |
| Timeout budget | Tổng thời gian cho operation, phân bổ qua các hop/dependency |
| Graceful shutdown | Ngừng nhận việc mới, drain/cancel có kiểm soát, đóng resource và telemetry |

## Prerequisites

- [Module overview](README.md).
- Linux shell cơ bản.
- Quyền đọc process/socket/log phù hợp.
- curl, ps, ss; getent và openssl hữu ích; lsof là optional.

## How It Works

### 1. Xác định symptom và phạm vi

Ghi chính xác:

- ai/điểm nào quan sát lỗi;
- timestamp có timezone;
- request target và correlation/request ID;
- error/status nguyên văn;
- một tenant hay tất cả;
- một instance/zone hay toàn hệ thống;
- bắt đầu sau deploy/config/dependency change nào;
- tỷ lệ lỗi và latency distribution.

Không bắt đầu bằng “root cause là network”. Bắt đầu bằng: “Từ host A lúc 10:03 UTC, 83% GET /orders vượt 5s; DNS resolve dưới 10ms; TCP connect dưới 20ms; time-to-first-byte khoảng 5s.”

### 2. Process

~~~bash
ps -eo pid,ppid,user,stat,%cpu,%mem,etime,comm --sort=-pcpu | head -n 20
~~~

Đọc:

- PID/PPID: lifecycle/parent;
- USER: security identity;
- STAT: R, S, D, T, Z và modifiers;
- %CPU/%MEM: signal ban đầu, không phải kết luận;
- ELAPSED: process restart gần symptom không.

Process ở state D thường chờ I/O không ngắt được; zombie Z đã kết thúc nhưng chưa được parent reap. CPU phần trăm của ps có semantics riêng và không nhất thiết cộng đúng 100%; đọc man page của implementation đang dùng.

Nếu service dùng systemd:

~~~bash
systemctl status myapp.service --no-pager
journalctl -u myapp.service --since "15 minutes ago" --no-pager
~~~

Status cho lifecycle; journal cho timeline. Không coi “active (running)” là health check.

### 3. Listening socket

~~~bash
ss -lntp
ss -lntp 'sport = :8080'
~~~

Các điểm cần đọc:

- LISTEN có tồn tại không;
- local address là 127.0.0.1, ::1, một interface cụ thể hay wildcard;
- port có đúng contract không;
- process/PID có đúng binary/identity không;
- listen backlog có dấu hiệu áp lực không.

127.0.0.1:8080 chỉ nhận traffic loopback. 0.0.0.0:8080 nhận trên mọi IPv4 interface được route tới; điều đó mở rộng exposure và phải đi cùng firewall/policy.

lsof là cách đối chiếu:

~~~bash
lsof -nP -iTCP:8080 -sTCP:LISTEN
~~~

Quyền hạn có thể làm thiếu process details; thiếu output không luôn có nghĩa không tồn tại socket.

### 4. Address và route

~~~bash
ip address
ip route
ip route get 203.0.113.10
~~~

203.0.113.0/24 là TEST-NET-3 dùng trong tài liệu. Trong incident, thay bằng IP đã xác minh.

Kiểm tra:

- source address/interface được chọn;
- default route/next hop;
- policy route hoặc network namespace;
- host route khác container/pod route hay không.

### 5. DNS

~~~bash
getent ahosts api.example.com
~~~

getent dùng Name Service Switch của host nên gần hành vi application hơn chỉ đọc một DNS server. dig hữu ích để hỏi record/DNS server cụ thể, nhưng kết quả dig và application có thể khác vì cache, search domain, hosts file hoặc resolver configuration.

Ghi:

- tất cả address trả về;
- IPv4/IPv6;
- address nào client thực sự thử;
- TTL/cache behavior khi điều tra stale DNS;
- split-horizon/private DNS boundary.

### 6. TCP/TLS/HTTP bằng curl

~~~bash
curl --silent --show-error --output /dev/null \
  --connect-timeout 3 \
  --max-time 10 \
  --write-out 'remote_ip=%{remote_ip} code=%{http_code} dns=%{time_namelookup} connect=%{time_connect} tls=%{time_appconnect} first_byte=%{time_starttransfer} total=%{time_total}\n' \
  https://api.example.com/health
~~~

Diễn giải:

- time_namelookup tăng: resolver/DNS path;
- time_connect − time_namelookup tăng: route/TCP/listener/queue;
- time_appconnect − time_connect tăng: TLS;
- time_starttransfer − time_appconnect tăng: proxy/app/dependency;
- total − first_byte tăng: response body/stream/bandwidth/client consumption.

HTTP/3 dùng QUIC/UDP nên TCP-specific assumptions không áp dụng nguyên vẹn. HTTP semantics vẫn theo RFC 9110.

TLS inspection:

~~~bash
openssl s_client \
  -connect api.example.com:443 \
  -servername api.example.com \
  -brief </dev/null
~~~

SNI/hostname phải phản ánh hostname client dùng. Không dùng curl -k như “fix”; nó bỏ certificate verification và che security failure.

### 7. Application và dependency

Khi HTTP đã tới application:

- dùng request/trace ID tìm log;
- xem route/handler/status;
- theo trace span để tìm SQL, Redis, broker, external API hoặc model call;
- so timeout budget với retry count;
- xem connection/thread pool, queue và resource saturation;
- kiểm tra deploy/config/secret/schema change trong timeline.

Đừng dừng ở “database timeout”. Đó là failure location; root cause có thể là blocking, pool exhaustion, DNS, CPU, lock, plan regression hoặc network.

## Minimal Example

Điều tra local endpoint:

~~~bash
ps -eo pid,user,stat,etime,comm | grep -E 'dotnet|myapp'
ss -lntp 'sport = :8080'
curl --verbose --connect-timeout 2 --max-time 5 http://127.0.0.1:8080/health
~~~

Decision tree:

~~~text
No process?
  → lifecycle/deploy/crash

Process, no LISTEN?
  → startup/config/bind failure

LISTEN on loopback only, remote client?
  → address binding / topology

TCP connects, TLS fails?
  → certificate/SNI/protocol/time

TLS succeeds, HTTP 5xx?
  → proxy/application/dependency

HTTP succeeds, latency bad?
  → timing split + saturation + dependency trace
~~~

## Production Example

### Symptom

Load balancer trả 502 cho 40% request sau rolling deployment.

### Evidence plan

| Layer | Check | Branch bị loại/xác nhận |
| --- | --- | --- |
| Client/LB | status, backend ID, timestamp | Instance-specific hay global |
| DNS/TLS client-facing | curl timings/certificate | Edge path có ổn không |
| LB target | target health/reason | LB không reach, refused, timeout hay bad response |
| Host/process | service status, restart time, PID | Crash/startup loop |
| Socket | ss address/port/PID | Wrong bind hoặc no listener |
| App | startup/readiness logs | Migration/config/dependency |
| Dependency | trace spans/pool/latency | App chậm vì downstream |

### Một kết luận có bằng chứng

~~~text
Observed:
- Old instances healthy.
- New instances process active.
- New instances listen on 127.0.0.1:8080.
- Load balancer connects to the host interface address.
- Local curl to 127.0.0.1 succeeds; curl to host interface is refused.

Conclusion:
Deployment changed endpoint binding to loopback. The application is alive but
unreachable from the load balancer.

Mitigation:
Roll back endpoint configuration, verify the intended interface binding and
firewall, then redeploy one canary.

Prevention:
Add a deployment smoke test from the same network path as the load balancer
and validate effective endpoint configuration at startup.
~~~

“Bind wildcard” không tự động là fix an toàn. Production decision phải giới hạn exposure bằng explicit address, security group/firewall, NetworkPolicy hoặc proxy topology.

## .NET Integration

### Host và signal

Từ .NET 10, runtime thuần không tự cài default termination handler; application model phải sở hữu lifecycle. Ứng dụng dùng Generic Host/ASP.NET Core thường dùng ConsoleLifetime, xử lý SIGINT/SIGQUIT/SIGTERM để bắt đầu graceful shutdown.

Ví dụ Worker hoàn chỉnh ở mức file Program.cs:

~~~csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHostedService<HeartbeatWorker>();

await builder.Build().RunAsync();

sealed class HeartbeatWorker(ILogger<HeartbeatWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Heartbeat at {Timestamp}", DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping: no new work will be accepted.");
        await base.StopAsync(cancellationToken);
        logger.LogInformation("Stopped.");
    }
}
~~~

Điểm production:

- truyền stoppingToken đến mọi async I/O;
- không swallow OperationCanceledException theo cách biến shutdown bình thường thành error noise;
- bounded cleanup; shutdown timeout không phải vô hạn;
- queue consumer phải quyết định ack/nack/requeue boundary;
- flush telemetry có deadline;
- SIGKILL không thể graceful; dữ liệu đúng phải dựa vào durable/transactional design.

### Kestrel và traffic drain

Trình tự an toàn sau load balancer/orchestrator:

~~~text
New instance ready
  ↓
Add new instance to traffic
  ↓
Mark old instance not ready / remove from load balancing
  ↓
Wait for propagation and drain
  ↓
Send SIGTERM
  ↓
Kestrel stops accepting new work and waits up to shutdown timeout
  ↓
Force termination only after deadline
~~~

Nếu SIGTERM đến trước khi traffic removal có hiệu lực, client có thể nhận reset/5xx. Readiness và shutdown timing là một distributed coordination problem.

## Internals

### /proc

/proc là pseudo-filesystem cung cấp interface đến kernel process/system data. Nhiều tool như ps đọc dữ liệu từ đây. Một số mount option như hidepid thay đổi visibility; trong container/PID namespace, process view khác host.

### Socket state

Listening socket khác accepted connection. Kernel giữ queues cho connection đang handshake/đã hoàn tất chờ application accept. Saturation có thể xảy ra dù process chạy và port LISTEN.

Một số TCP states hữu ích:

- SYN-SENT: client đã gửi SYN, chờ response;
- SYN-RECV: server nhận SYN, handshake chưa hoàn tất;
- ESTAB: transport connection established;
- TIME-WAIT: endpoint giữ state sau close để xử lý delayed segments;
- CLOSE-WAIT: peer đã close nhưng local application chưa close.

Nhiều CLOSE-WAIT có thể gợi ý application/resource cleanup issue; không kết luận chỉ từ một snapshot.

### DNS không phải một lookup đơn

Application có thể đi qua:

- hosts file;
- local cache/stub resolver;
- search suffix;
- recursive resolver;
- authoritative servers;
- client/library cache.

Do đó “dig đúng” không luôn chứng minh application resolve đúng.

## Common Mistakes

- Restart trước khi lấy PID, logs, socket/resource snapshot.
- Dùng ping để kết luận HTTP reachable; ICMP và application path khác nhau.
- Thấy process running rồi kết luận service healthy.
- Thấy timeout rồi thêm retry, làm tăng tải và duplicate side effect.
- Dùng curl -k hoặc tắt TLS verification.
- Bind 0.0.0.0 mà không review exposure.
- Chỉ nhìn average latency thay vì error rate/P95/P99 và per-instance split.
- So log timestamp khác timezone.
- Grep secret, token hoặc full request body vào incident notes.
- Dùng sudo cho mọi lệnh, làm mờ least-privilege boundary.
- Chẩn đoán host namespace trong khi failure nằm trong container/pod namespace.

## Performance Considerations

Đi theo resource model:

| Resource | Evidence đầu tiên | Câu hỏi tiếp theo |
| --- | --- | --- |
| CPU | utilization, run queue, per-process/thread | Work tăng, spin, GC hay steal/throttle? |
| Memory | RSS, working set, pressure, OOM events | Leak, cache, load, limit hay reclaim? |
| Disk/I/O | latency, queue, throughput, errors | Database/log/temp/file nào? sync hay capacity? |
| Network | connect/RTT/retransmit, throughput, drops | Link, route, queue, peer hay application? |
| Sockets | states, accept backlog, file descriptors | Leak, burst, slow peer hay limit? |
| Thread/connection pools | queue/wait/utilization | Blocking, slow dependency hay undersizing? |

Không optimize theo một snapshot. Dùng time series quanh symptom và deployment/event timeline.

Ở 10x scale:

- connection count, file descriptor và ephemeral port pressure tăng;
- logging volume có thể thành bottleneck/cost;
- DNS/cache/proxy behavior trở nên đáng kể;
- tail latency và retry amplification chi phối user experience.

Ở 100x scale:

- cần aggregation, fleet-wide telemetry và per-cell isolation;
- ad-hoc SSH không còn là primary operating model;
- load shedding, capacity automation và failure-domain design trở thành bắt buộc.

## Security Considerations

- Process identity và file/socket permission là security boundary.
- /proc có thể lộ command line/environment; không truyền secret qua CLI nếu có lựa chọn an toàn hơn.
- Chỉ thu thập packet/process data trong scope được phép; tcpdump/strace có thể lộ sensitive data.
- Không log Authorization, cookie, API key, connection string hoặc PII.
- TLS failure là control hoạt động, không phải friction cần bỏ qua.
- Wildcard bind, port publishing và reverse proxy thay đổi attack surface.
- Diagnostic endpoint cần authentication/authorization và data minimization.
- Tooling chạy privileged tạo blast radius; dùng least privilege và audit.

## Reliability / Failure Modes

| Lớp | Failure mode | Symptom thường thấy | Bằng chứng phân biệt |
| --- | --- | --- | --- |
| Process | crash/restart/OOM | refused/5xx/gap | lifecycle logs, restart count, OOM event |
| Bind/listen | wrong port/address | local works, remote fails | ss local address |
| DNS | NXDOMAIN/stale/wrong view | resolve error/wrong backend | application resolver output, record/TTL |
| Route/firewall | drop/reject/wrong interface | timeout/refused/unreachable | route, connect behavior, network policy |
| TCP | backlog/retransmit/reset | connect slow/reset | socket states, kernel/network metrics |
| TLS | expiry/hostname/SNI/protocol/time | handshake error | verified s_client/curl output |
| Proxy/LB | unhealthy target/timeout/config | 502/503/504 | target/backend reason and logs |
| Application | exception/saturation | 5xx/slow | structured logs, runtime metrics, traces |
| Dependency | slow/unavailable/pool exhausted | app timeout/5xx | child span, pool/queue, dependency metric |

Recovery phải nêu:

- retry có an toàn/idempotent không;
- retry budget và backoff/jitter;
- fallback có trả stale/incorrect data không;
- traffic có drain/reroute được không;
- partial deployment rollback thế nào;
- dữ liệu/side effect có cần reconcile không.

## Observability

Một request production nên nối được:

~~~text
request ID / trace ID
  → edge/load balancer
  → proxy
  → ASP.NET Core server span
  → SQL/Redis/broker/model spans
  → status/error
  → latency/tokens/cost where relevant
~~~

Thu thập:

- Logs: structured, timestamp UTC, instance/version/config identity.
- Metrics: request rate, errors, duration, saturation; dependency/pool/queue.
- Traces: critical path và retry/fan-out.
- Events: deploy, config/secret/schema change, autoscale, failover.

Telemetry cũng có failure modes: mất exporter, sampling sai, cardinality explosion, clock skew, PII leak và chi phí quá cao.

## Operational Considerations

Runbook tối thiểu:

1. scope và severity;
2. safe read-only commands;
3. expected output và interpretation;
4. mitigation choices + risk;
5. rollback;
6. verification từ user path;
7. escalation owner;
8. evidence retention/redaction.

Production access nên ưu tiên central telemetry và controlled tooling. SSH thủ công là break-glass hoặc deep diagnosis, không phải automation strategy.

## Architect Perspective

### Vì sao các lớp tồn tại?

- DNS decouples logical name khỏi address nhưng thêm cache/staleness.
- Proxy/load balancer decouples client khỏi instances nhưng thêm hop/config/failure.
- TLS tạo confidentiality/integrity/identity nhưng cần certificate/time/trust operations.
- Orchestrator quản lý desired state nhưng health/lifecycle contract phải đúng.

### Security boundary

Mỗi hop có identity:

- user/client identity;
- edge/service identity;
- workload identity;
- dependency identity.

Network reachability không thay authorization. “Inside the cluster/VPC” không phải trust proof.

### Cost

- Extra hop tăng compute/network/latency và telemetry.
- High-cardinality logs/traces tăng storage/ingestion cost.
- HA topology tăng idle capacity và operational burden.
- Chẩn đoán thủ công không scale và tạo human cost.

### Câu hỏi quyết định

- Có cần proxy mới hay endpoint trực tiếp đủ?
- Failure domain có thực sự được cô lập?
- Probe đo đúng readiness contract không?
- Timeout budget có nhất quán qua hops?
- Team nào sở hữu DNS/cert/proxy/app/dependency?
- Có thể rollback config độc lập với code không?

## Trade-offs

| Lựa chọn | Lợi ích | Chi phí/rủi ro |
| --- | --- | --- |
| Bind loopback | Giảm exposure | Không reachable từ remote proxy/container boundary |
| Bind wildcard | Đơn giản trong container | Mở rộng attack surface; phụ thuộc firewall/policy |
| Nhiều retry | Có thể vượt transient fault | Amplification, latency, duplicates |
| Verbose logs | Điều tra dễ hơn | PII, cost, I/O và noise |
| Deep tracing | Critical path rõ | Overhead, sampling/cardinality/retention |
| Short timeout | Fail fast, giải phóng resource | False failures nếu budget thiếu |
| Long timeout | Tolerate slow dependency | Resource retention và tail latency |

## When NOT to Use It

Không dùng ad-hoc shell diagnosis làm giải pháp chính khi:

- fleet lớn và sự cố cần aggregate theo instance/zone/version;
- production access bị hạn chế đúng lý do;
- failure quá ngắn để snapshot thủ công;
- cần audit/repeatability;
- packet/process data nhạy cảm;
- root cause cần profiler, runtime trace, database plan hoặc distributed trace chuyên biệt.

Shell tools vẫn hữu ích để kiểm chứng cục bộ và xây mental model.

## Alternatives

| Nhu cầu | Công cụ đầu tiên | Khi cần sâu hơn |
| --- | --- | --- |
| Process | ps/systemctl | top, pidstat, profiler/runtime diagnostics |
| Socket | ss | lsof, namespace-aware tooling, packet capture |
| DNS | getent | dig/drill, resolver logs |
| HTTP timing | curl | synthetic monitor, load test, distributed trace |
| TLS | curl/openssl s_client | certificate inventory/scanner |
| Logs | journalctl | centralized log platform |
| Network path | ip route/curl | tracepath, tcpdump, eBPF with authorization |

strace, tcpdump và eBPF là powerful nhưng có overhead/privilege/data risks; dùng sau khi câu hỏi hẹp và được phép.

## Review Questions

1. Vì sao process “running” chưa chứng minh service available?
2. 127.0.0.1:8080 khác 0.0.0.0:8080 ở boundary nào?
3. curl connect nhanh nhưng first byte chậm gợi ý lớp nào?
4. dig trả đúng IP nhưng application vẫn resolve sai có thể do đâu?
5. HTTP 503 khác TCP connection refused về evidence path thế nào?
6. Vì sao thêm retry có thể làm outage nặng hơn?
7. SIGTERM và SIGKILL khác nhau ra sao với Generic Host?
8. Readiness probe nên đo gì và không nên tạo side effect gì?
9. Telemetry có những failure/security/cost modes nào?
10. Điều gì phải đổi khi từ 10 instances lên 1,000 instances?

## Hands-on Lab

### Problem

Xây một local HTTP service tạm thời, chứng minh từng lớp hoạt động, sau đó tạo bốn failure khác nhau và phân loại đúng.

### Constraints

- Chạy trên Linux/WSL2.
- Không dùng sudo.
- Không tắt firewall hoặc TLS verification.
- Không dùng port production.
- Dùng .invalid/TEST-NET cho negative tests ngoài local.

### Expected outcome

Một incident worksheet chứa:

- command và timestamp;
- hypothesis trước experiment;
- output liên quan đã redact;
- layer được xác nhận/loại;
- mitigation và cleanup.

### Implementation steps

Chạy trong cùng một shell:

~~~bash
LAB_DIR="$(mktemp -d)"
python3 -m http.server 8080 \
  --bind 127.0.0.1 \
  --directory "$LAB_DIR" \
  >"$LAB_DIR/server.log" 2>&1 &
SERVER_PID=$!

ps -p "$SERVER_PID" -o pid,ppid,user,stat,etime,comm
ss -lntp 'sport = :8080'
curl --silent --show-error --output /dev/null \
  --write-out 'code=%{http_code} connect=%{time_connect} first_byte=%{time_starttransfer} total=%{time_total}\n' \
  http://127.0.0.1:8080/
~~~

Ghi LAB_DIR và SERVER_PID. Không tái sử dụng biến nếu mở shell khác.

### Verification

- ps hiển thị đúng PID.
- ss hiển thị LISTEN trên 127.0.0.1:8080.
- curl trả 200.
- server.log có request tương ứng.

### Failure experiment 1 — HTTP application response

~~~bash
curl --include --max-time 5 http://127.0.0.1:8080/missing
~~~

Expected: TCP thành công, HTTP 404. Không gọi đây là network failure.

### Failure experiment 2 — Protocol mismatch

~~~bash
curl --verbose --max-time 5 https://127.0.0.1:8080/
~~~

Expected: TCP có thể kết nối nhưng TLS handshake thất bại vì endpoint chỉ nói plain HTTP.

### Failure experiment 3 — DNS

~~~bash
getent ahosts service-does-not-exist.invalid
curl --verbose --connect-timeout 2 --max-time 3 \
  https://service-does-not-exist.invalid/
~~~

.invalid là reserved top-level domain cho trường hợp chắc chắn không resolve. Expected: failure trước TCP.

### Failure experiment 4 — Process/listener

Trước khi gửi signal, xác minh PID vẫn là process lab:

~~~bash
ps -p "$SERVER_PID" -o pid,user,stat,etime,comm
kill -TERM "$SERVER_PID"
wait "$SERVER_PID"
ss -lntp 'sport = :8080'
curl --verbose --connect-timeout 2 --max-time 3 http://127.0.0.1:8080/
~~~

Expected: listener biến mất; curl thường nhận connection refused. Nếu output khác, ghi evidence thay vì ép kết quả.

### Optional route timeout experiment

~~~bash
curl --verbose --connect-timeout 2 --max-time 3 http://192.0.2.1:81/
~~~

192.0.2.0/24 là TEST-NET-1. Corporate proxy/VPN có thể intercept và thay đổi symptom; đó chính là một topology finding cần ghi lại.

### Questions

- Failure nào xảy ra trước khi server application nhận request?
- Output nào chứng minh bind loopback?
- Refused và timed out khác nhau thế nào nhưng vì sao vẫn chưa đủ kết luận root cause?
- Nếu đổi sang .NET Worker/Kestrel, telemetry nào thêm vào?
- Làm sao production smoke test đi từ cùng network path với load balancer?

### Lab exit criteria

- Phân loại đúng cả bốn failure.
- Không dùng restart/disable verification như mitigation.
- Có cleanup: process lab đã dừng; ghi temp directory để xóa có chủ đích sau khi xem log.
- Có một đề xuất automation/observability giúp phát hiện sớm hơn.

## Exit Criteria

Chương hoàn thành khi người học có thể, không nhìn đáp án:

- lập decision tree process/socket/DNS/TCP/TLS/HTTP/app;
- dùng ps, ss, getent, curl timing và journal/system logs đúng câu hỏi;
- giải thích output và uncertainty;
- thực hiện lab/failure experiment an toàn;
- mô tả .NET 10 Generic Host shutdown và traffic drain;
- viết một incident conclusion tách observed facts, inference và decision;
- nêu security, reliability, observability, performance và cost implications.

## Related Topics

- Prerequisite graph: [../00-roadmap/prerequisites.md](../00-roadmap/prerequisites.md)
- Module overview: [README.md](README.md)
- Next: filesystem/permissions, process/resources, DNS/TCP/TLS/HTTP deep dives (Planned)
- Later: Docker lifecycle, Kubernetes probes, OpenTelemetry, distributed timeouts/retries

## Official English Sources

- [proc(5)](https://man7.org/linux/man-pages/man5/procfs.5.html)
- [ps(1)](https://man7.org/linux/man-pages/man1/ps.1.html)
- [ss(8)](https://man7.org/linux/man-pages/man8/ss.8.html)
- [signal(7)](https://man7.org/linux/man-pages/man7/signal.7.html)
- [Git reference](https://git-scm.com/docs)
- [RFC 9110 — HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html)
- [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [.NET 10 termination signal change](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/10.0/sigterm-signal-handler)
- [Kestrel security considerations](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/security-considerations?view=aspnetcore-10.0)

## Vietnamese Resources

- [Microsoft Learn — truy cập ứng dụng mạng với WSL](https://learn.microsoft.com/vi-vn/windows/wsl/networking)

Không tìm thấy bản dịch Việt chính thức đầy đủ cho Linux man-pages/RFC được dùng. Bản English ở trên là source of truth.

## Verification Metadata

- Verified: 2026-08-11
- Technology version: Linux man-pages current at verification; HTTP RFC 9110; .NET 10
- Official sources: danh sách trên và [references.md](references.md)
- Context7 queries used: /dotnet/core query cho .NET 10 baseline; Generic Host behavior đối chiếu trực tiếp Microsoft Learn
- Notes: command flags đã đối chiếu với current man pages; output/permissions khác theo distro, init system, namespace và security policy.
