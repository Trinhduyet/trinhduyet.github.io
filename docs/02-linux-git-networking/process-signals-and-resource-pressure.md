# Linux Processes, Signals, and Resource Pressure

## Mục tiêu / Learning Objectives

Sau chương này, người học có thể:

- đọc process tree, thread state, CPU, memory, file descriptor và elapsed time;
- phân biệt utilization, saturation, pressure và hard limit;
- giải thích signal delivery, graceful shutdown, timeout và forced termination;
- điều tra service chậm ở đúng host/cgroup/container boundary;
- dùng PSI, /proc, systemd và .NET diagnostics để tạo giả thuyết có thể kiểm chứng;
- thiết kế resource budget và shutdown contract cho production service.

## Tại sao cần học? / Why It Matters

Một process RUNNING không có nghĩa là nó đang phục vụ traffic. CPU trung bình thấp không loại trừ throttling hoặc một core bị bão hòa. Memory còn free không đảm bảo không có reclaim pressure. Restart có thể xóa evidence và che một leak, deadlock hoặc dependency stall.

Production diagnosis cần trả lời bốn câu riêng:

1. Work đang ở trạng thái nào?
2. Resource nào được dùng, tại boundary nào?
3. Work có đang phải chờ resource không?
4. Limit, supervisor hoặc signal nào làm process chuyển trạng thái?

## Tổng quan / Overview

~~~text
Service manager / orchestrator
  │ creates, limits, restarts, signals
  ▼
Process ── threads ── scheduler states
  │
  ├─ CPU time / runnable queue / throttling
  ├─ virtual memory / RSS / reclaim / OOM
  ├─ file descriptors / sockets / files
  ├─ block I/O and uninterruptible waits
  └─ cgroup limits and pressure accounting
~~~

Tách ba loại số liệu:

- Utilization: đã dùng bao nhiêu resource.
- Saturation/pressure: work phải chờ resource bao lâu.
- Limit/event: boundary nào throttle, reject hoặc kill work.

## Mental Model

### Process không phải service

Service là lifecycle contract do systemd/orchestrator quản lý. Một service có thể tạo nhiều process. Main PID có thể sống trong khi worker lỗi; PID có thể đổi sau restart. Luôn nối evidence với service unit, cgroup và deployment revision.

### Thread states là clue, không phải diagnosis

| State thường gặp | Ý nghĩa sơ bộ |
| --- | --- |
| R | Running hoặc runnable, đang dùng/chờ CPU |
| S | Interruptible sleep, thường đang đợi event/timer/I/O |
| D | Uninterruptible sleep, thường liên quan kernel I/O path |
| T/t | Stopped hoặc traced |
| Z | Zombie: đã exit, parent chưa reap |

Một snapshot không đủ. Cần trend, rate và workload context.

### Host metric có thể sai boundary

Container có thể bị giới hạn 1 CPU trên host 32 CPU. Host chỉ 10% CPU vẫn có thể đồng nghĩa container liên tục chạm quota. Tương tự, host còn nhiều RAM nhưng cgroup chạm memory.max. Bắt đầu tại boundary đang chịu SLO rồi mới mở rộng ra host.

## Thuật ngữ / Terminology

| Thuật ngữ | Mental model |
| --- | --- |
| PID / PPID | Process ID và parent process ID |
| TID | Thread ID; Linux scheduler quản lý task/thread |
| RSS | Resident pages hiện ở RAM; không đồng nghĩa private memory |
| VSZ | Virtual address space, có thể lớn hơn RAM rất nhiều |
| Context switch | CPU chuyển execution context |
| Load average | Số task runnable hoặc uninterruptible trung bình, không phải CPU percent |
| PSI | Pressure Stall Information: thời gian work bị stall do CPU/memory/I/O contention |
| rlimit | Soft/hard per-process resource limit được kế thừa qua fork/exec |
| cgroup v2 | Hierarchy phân nhóm, đo và giới hạn resource |
| Throttling | Trì hoãn work để giữ trong limit, không nhất thiết kill process |
| OOM kill | Kernel/cgroup chọn kill process khi không thể thỏa memory allocation |
| Signal | Async notification với default action hoặc handler |
| Grace period | Khoảng supervisor cho app drain trước forced termination |

## Prerequisites

- [Production Troubleshooting Foundations](production-troubleshooting-foundations.md).
- Quyền đọc /proc và service metadata phù hợp.
- ps, top hoặc htop, vmstat; pidstat và systemd tools hữu ích.
- Với .NET diagnostics: dotnet SDK/tool có version tương thích runtime.

## How It Works

### 1. Xác định đúng process và lifecycle owner

~~~bash
systemctl status myapp.service --no-pager
systemctl show myapp.service -p MainPID -p ControlGroup -p ActiveState -p SubState -p NRestarts
ps -eo pid,ppid,user,stat,ni,psr,pcpu,pmem,rss,vsz,etime,comm,args --sort=-pcpu
~~~

Không chọn PID chỉ theo tên ngắn. Ghi command line, start/elapsed time, parent và cgroup để tránh attach nhầm instance.

### 2. Đọc process state và limits

Sau khi xác minh PID là số và thuộc service cần điều tra:

~~~bash
target_pid=12345
case "$target_pid" in
  ''|*[!0-9]*) printf 'invalid pid\n' >&2; exit 64 ;;
esac

cat "/proc/$target_pid/status"
cat "/proc/$target_pid/limits"
ls -1 "/proc/$target_pid/fd" | wc -l
~~~

/proc/PID/status cho identity, thread count, memory fields, capabilities, signal masks và context-switch counters. /proc/PID/limits cho soft/hard limits thực tế.

### 3. Quan sát trend và pressure

~~~bash
uptime
free -h
vmstat 1 5
cat /proc/pressure/cpu
cat /proc/pressure/memory
cat /proc/pressure/io
~~~

PSI some biểu diễn thời gian ít nhất một task bị stall; full biểu diễn thời gian mọi non-idle task trong scope bị stall đồng thời. avg10/60/300 là tỷ lệ theo cửa sổ; total là thời gian tích lũy microseconds. Đọc delta/trend, không so một total tuyệt đối giữa máy có uptime khác nhau.

### 4. Đọc cgroup/service boundary

~~~bash
systemctl show myapp.service \
  -p CPUUsageNSec -p MemoryCurrent -p MemoryPeak -p MemoryMax \
  -p TasksCurrent -p TasksMax -p IOReadBytes -p IOWriteBytes

systemd-cgtop
~~~

Trên cgroup v2, memory.high là throttle/reclaim boundary phù hợp để kiểm soát dần; memory.max là hard boundary. Khi chạm hard boundary và reclaim thất bại, cgroup có thể OOM kill. Đừng chỉ tăng limit; xác định working set, concurrency và leak behavior.

### 5. Correlate với application

Đặt process/cgroup evidence cạnh:

- request rate, latency percentiles, error rate;
- queue depth và active concurrency;
- dependency latency/timeouts;
- GC allocation/heap/pause;
- thread pool queue/thread count;
- deployment, autoscaling, limit và configuration events.

## Minimal Example

Quan sát một process sleep có PID đã biết:

~~~bash
sleep 30 &
lab_pid=$!
printf 'pid=%s\n' "$lab_pid"
ps -o pid,ppid,stat,etime,comm -p "$lab_pid"
cat "/proc/$lab_pid/status" | sed -n '1,25p'
wait "$lab_pid"
~~~

Lệnh có thời hạn 30 giây và không cần kill process lạ. State thường là S vì process chờ timer.

## Production Example

Symptom: API latency tăng, dashboard host CPU chỉ 35%, không có error rõ ràng.

Quy trình evidence:

1. Xác nhận SLO regression và deployment/change timeline.
2. Map request đến đúng replica, service unit và cgroup.
3. Đọc CPU quota/throttling và PSI tại cgroup nếu có, không chỉ host CPU.
4. Kiểm tra runnable threads, context-switch rate và per-thread CPU.
5. Dùng runtime counters để tách CPU work, GC pressure, thread-pool queue hoặc lock contention.
6. Chỉ thu trace/profiler sau khi có giả thuyết và trong duration bounded.

Ví dụ commands:

~~~bash
pidstat -p 12345 -t -u -r -d 1 10
cat /proc/pressure/cpu
cat /proc/pressure/memory
journalctl -u myapp.service --since '30 minutes ago' --no-pager
~~~

Nếu một container bị 1 CPU quota và continuously throttled, scale CPU hoặc giảm concurrency có thể là mitigation. Root cause vẫn có thể là regression làm CPU/request tăng; cần so profile/counter theo build trước và sau.

## .NET Integration

### Shutdown contract

.NET Generic Host ConsoleLifetime xử lý SIGINT, SIGQUIT và SIGTERM để bắt đầu graceful shutdown. Hosted services cần:

- ngừng nhận work mới;
- honor CancellationToken;
- drain có deadline;
- dispose connection/producer/consumer;
- flush telemetry trong time budget;
- exit non-zero khi startup hoặc unrecoverable failure thực sự thất bại.

SIGKILL không thể được catch hoặc cleanup. systemd thường gửi SIGTERM trước, chờ TimeoutStopSec rồi mới forced termination theo policy. Grace period phải lớn hơn drain budget có chủ đích, không phải vô hạn.

### Runtime counters trước, trace sau

Với .NET 10, one-shot tool execution có thể dùng dnx khi môi trường cho phép:

~~~bash
dnx dotnet-counters monitor --process-id 12345
dnx dotnet-counters collect --process-id 12345 --duration 00:00:30 --output counters.csv --format csv
dnx dotnet-trace collect --process-id 12345 --duration 00:00:30 --output myapp.nettrace
~~~

Hoặc cài tool có version pin và dùng dotnet-counters/dotnet-trace trực tiếp. Luôn:

- xác minh PID và runtime tương thích;
- giới hạn duration/output path;
- đánh giá overhead và dữ liệu nhạy cảm;
- lưu deployment revision, UTC interval và workload rate cùng artifact.

dotnet-counters phù hợp health investigation ban đầu. Trace/profiler cho call stacks, contention và timeline sâu hơn nhưng có overhead/storage cao hơn. dotnet-trace collect-linux là preview, cần root/kernel phù hợp và không phải lựa chọn mặc định.

### Counter interpretation

| Dấu hiệu | Giả thuyết cần kiểm tra tiếp |
| --- | --- |
| CPU cao, allocation thấp | Compute loop, serialization, crypto, regex hoặc user code |
| Allocation/GC cao | Object churn, payload lớn, cache miss hoặc buffering |
| ThreadPool queue tăng | Blocking, downstream latency hoặc insufficient throughput |
| Exception rate tăng | Retry storm, invalid input hoặc dependency failure |
| Working set tăng liên tục | Managed/native leak, cache không bound hoặc backlog |

Counter không tự chứng minh root cause; nó chọn bước đo tiếp theo.

## Internals

### Signal delivery

Mỗi signal có default disposition: terminate, ignore, stop, continue hoặc core dump. Process có thể install handler cho phần lớn signal, block signal theo thread và nhận pending signal sau đó. SIGKILL và SIGSTOP không thể catch, block hoặc ignore.

Handler phải tránh unsafe work. Trong managed host, framework biến signal thành cancellation/lifecycle callback; code ứng dụng nên xử lý qua lifecycle abstraction thay vì tự viết low-level handler tràn lan.

### rlimit inheritance

Resource limits là per-process và được các thread chia sẻ. Child kế thừa limits qua fork, và limits được giữ qua exec. Supervisor/service config vì vậy có thể làm app chạm NOFILE dù interactive shell có limit khác. /proc/PID/limits là evidence của process thật.

### Memory không chỉ là heap

RSS có thể gồm managed heap, native allocations, JIT code, thread stacks, mapped files và shared pages. .NET GC heap metric nhỏ hơn RSS không tự chứng minh leak ngoài managed heap, nhưng là một hướng điều tra. Cgroup memory accounting và host page cache semantics cũng cần xem cùng nhau.

## Common Mistakes

- Dùng một snapshot top rồi kết luận root cause.
- So container utilization với tổng host capacity.
- Nhầm load average với CPU percent.
- Nhầm VSZ là RAM đang dùng.
- Chỉ nhìn free memory, bỏ qua reclaim, PSI và cgroup limit.
- Gửi kill -9 trước khi thu thread dump/trace/log và cho app grace period.
- Tăng TimeoutStopSec vô hạn thay vì giới hạn drain.
- Attach profiler production không duration/overhead plan.
- Restart loop che startup failure và làm dependency quá tải.
- Tăng file-descriptor limit mà không sửa leak/connection lifecycle.

## Performance Considerations

- CPU saturation cần per-core/cgroup view; lock contention có thể giảm CPU nhưng tăng latency.
- Context-switch storm và quá nhiều threads làm cache locality kém.
- Memory reclaim và swap có thể làm tail latency tăng trước khi OOM.
- Unbounded queues chuyển backpressure thành memory growth và recovery time dài.
- Trace/counter frequency quá cao có observer overhead.
- Cgroup CPU quota giúp isolation nhưng có thể gây throttling bursty workload.

## Security Considerations

- /proc, traces, dumps và command line có thể chứa secrets, tokens hoặc PII.
- Chỉ cấp ptrace/diagnostic permission cho operator cần thiết; bảo vệ artifacts như production data.
- Không chạy diagnostic tool bằng root nếu scope thấp hơn đủ dùng.
- Giới hạn core dumps, storage retention và access policy.
- Signal permission dựa trên identity/capability; xác minh PID để tránh tác động process khác.
- Diagnostic port của .NET không nên exposed rộng.

## Reliability / Failure Modes

| Failure mode | Evidence | Mitigation trước mắt | Prevention |
| --- | --- | --- | --- |
| CPU throttling | cgroup cpu.stat, PSI, latency | Scale/giảm load | Right-size và performance budget |
| Memory pressure | PSI, memory.events, reclaim | Giảm concurrency/load | Bound cache/queue, memory.high alert |
| OOM kill | kernel/cgroup event, exit status | Restore capacity | Leak test, hard/soft boundary |
| FD exhaustion | /proc/PID/fd, limits, EMFILE | Shed load/restart có kiểm soát | Pool lifecycle, alert headroom |
| Thread-pool starvation | queue/thread counters, trace | Giảm blocking/load | Async end-to-end, bounded concurrency |
| D-state I/O stall | ps state, I/O PSI, storage metrics | Isolate/fail over | Storage SLO/timeouts |
| Zombie accumulation | Z state, PPID | Fix/restart parent | Reap child processes |
| Restart storm | NRestarts, journal, dependency load | Stop loop/circuit dependency | Backoff, start limits, readiness |
| Forced shutdown | SIGKILL/timeout evidence | Restore safely | Align drain and grace budgets |

## Observability

Một production capture nên có:

- UTC start/end, host/pod/unit, PID và deployment revision;
- request rate, latency/error/SLO impact;
- CPU utilization, quota/throttling và PSI;
- memory current/peak/max, PSI, events và OOM evidence;
- tasks/threads, FD count và limits;
- service restart/exit/signal timeline;
- .NET runtime counters và artifact metadata nếu đã collect.

Biểu đồ luôn cần unit, aggregation, scope và sample interval.

## Operational Considerations

- systemd Restart=on-failure thường phù hợp long-running service hơn restart vô điều kiện, nhưng phải có start-rate limiting/backoff.
- Startup, readiness, liveness và graceful shutdown là các contract khác nhau.
- Autoscaling metric phải phản ánh bottleneck: CPU không cứu được workload đang chờ I/O; queue length cần biết service rate.
- Limit nên có headroom và alert trước hard boundary.
- Runbook phải nêu evidence cần thu trước restart và khi nào impact buộc phải ưu tiên mitigation.
- Clock sync và UTC correlation là bắt buộc khi ghép kernel, systemd, app và platform logs.

## Architect Perspective

Kiến trúc production cần định nghĩa:

- resource envelope mỗi replica và workload assumption;
- bounded concurrency/queue và backpressure;
- termination grace, request deadline và retry budget;
- process supervision/restart policy;
- horizontal/vertical scaling signal;
- diagnostic access và artifact retention;
- degradation mode khi CPU, memory, I/O hoặc dependency bị pressure.

Không có resource budget nghĩa là capacity planning và failure isolation chưa hoàn chỉnh.

## Trade-offs

| Quyết định | Lợi ích | Đổi lại |
| --- | --- | --- |
| Hard low limit | Isolation chặt | OOM/throttling dễ xảy ra khi burst |
| Nhiều worker/threads | Tăng concurrency | Context switch, memory và contention |
| Long grace period | Drain nhiều work | Deploy/eviction chậm, stuck shutdown |
| Auto-restart | Khôi phục transient fault | Có thể tạo restart storm/che root cause |
| Rich production tracing | Diagnosis nhanh | Overhead, storage, privacy risk |
| Unbounded cache | Hit rate tốt lúc đầu | Memory risk và unpredictable eviction |

## When NOT to Use It

- Không dùng kill -9 như shutdown thông thường.
- Không dùng host average làm capacity signal duy nhất cho cgroup-isolated service.
- Không chạy full profiler lâu dài khi counters/traces bounded đủ trả lời câu hỏi.
- Không tăng limit thay cho việc sửa leak hoặc unbounded queue.
- Không restart để xác nhận mọi giả thuyết; restart làm thay đổi state và mất evidence.

## Alternatives

- eBPF/kernel profiling khi userspace trace không thấy scheduler/I/O path, với governance phù hợp.
- Continuous profiler khi tổ chức chấp nhận overhead/privacy contract.
- Core dump hoặc dump triage cho crash/hang khó tái hiện.
- Synthetic load test để tái tạo pressure ngoài production.
- Queueing/capacity model để dự báo trước khi thay limit.

## Review Questions

1. Vì sao host CPU 30% vẫn có thể là CPU saturation của service?
2. PSI khác utilization ở điểm nào?
3. RSS và VSZ chứng minh điều gì, không chứng minh điều gì?
4. memory.high và memory.max khác nhau về hành vi?
5. Vì sao /proc/PID/limits đáng tin hơn ulimit trong shell của operator?
6. Khi nào SIGTERM trở thành forced kill và ai quyết định grace period?
7. Evidence nào phân biệt CPU work với thread-pool starvation?
8. Tại sao restart loop có thể làm incident nặng hơn?

## Hands-on Lab

### Lab A: state và signal an toàn

~~~bash
sleep 60 &
lab_pid=$!
printf 'lab_pid=%s\n' "$lab_pid"

ps -o pid,ppid,stat,etime,comm -p "$lab_pid"
kill -STOP "$lab_pid"
ps -o pid,ppid,stat,etime,comm -p "$lab_pid"
kill -CONT "$lab_pid"
kill -TERM "$lab_pid"
wait "$lab_pid"
printf 'exit=%s\n' "$?"
~~~

Chỉ signal PID vừa tạo trong cùng shell. Xác nhận state chuyển sang T sau SIGSTOP và process kết thúc sau SIGTERM.

### Lab B: CPU work có thời hạn

Terminal 1:

~~~bash
timeout 15s sh -c 'while :; do :; done' &
cpu_pid=$!
printf 'cpu_pid=%s\n' "$cpu_pid"
wait "$cpu_pid"
~~~

Terminal 2, trong 15 giây:

~~~bash
ps -o pid,ppid,stat,psr,pcpu,etime,comm -p PID_FROM_TERMINAL_1
cat /proc/pressure/cpu
~~~

Workload bị timeout tự dừng; không chạy memory stress hoặc fork bomb. So sánh CPU percent và PSI, rồi giải thích vì sao một lab nhỏ có thể không tạo pressure trên máy nhiều core.

### Lab C: .NET bounded capture

Trên một local .NET process không chứa dữ liệu nhạy cảm:

1. Ghi PID, build/version và request workload.
2. Monitor counters trong 30 giây.
3. Tạo một trace 30 giây chỉ khi có câu hỏi cụ thể.
4. Ghi artifact size, overhead quan sát được và kết luận.

## Exit Criteria

Hoàn thành khi người học có thể:

- map symptom đến process, thread, cgroup và service manager;
- phân biệt utilization với pressure/limit event;
- đọc /proc/PID/status và /proc/PID/limits an toàn;
- giải thích SIGTERM, SIGKILL và graceful shutdown của .NET host;
- chọn counters trước trace/profiler và giới hạn capture;
- đề xuất resource, backpressure và termination budgets cho một service.

## Related Topics

- [Linux Filesystem, Permissions, and Identities](filesystem-permissions-and-identities.md)
- [DNS, TCP, TLS, and HTTP Deep Dive](dns-tcp-tls-http-deep-dive.md)
- [Production Troubleshooting Foundations](production-troubleshooting-foundations.md)
- Containers, cgroups and orchestration
- .NET performance diagnostics
- SLOs, capacity planning and incident response

## Official English Sources

- [signal(7)](https://man7.org/linux/man-pages/man7/signal.7.html)
- [proc_pid_status(5)](https://man7.org/linux/man-pages/man5/proc_pid_status.5.html)
- [getrlimit(2)](https://man7.org/linux/man-pages/man2/getrlimit.2.html)
- [Linux Pressure Stall Information](https://docs.kernel.org/accounting/psi.html)
- [Linux cgroup v2](https://www.kernel.org/doc/html/latest/admin-guide/cgroup-v2.html)
- [systemd.service source documentation](https://github.com/systemd/systemd/blob/main/man/systemd.service.xml)
- [systemd.resource-control source documentation](https://github.com/systemd/systemd/blob/main/man/systemd.resource-control.xml)
- [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [.NET diagnostic tools overview](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/profilers)

## Vietnamese Resources

- [Tổng quan Windows Subsystem for Linux](https://learn.microsoft.com/vi-vn/windows/wsl/about)

Tài liệu vận hành sâu bằng tiếng Việt còn phân mảnh. Dùng bản tiếng Việt để nhập môn và đối chiếu kernel/man-pages cùng tài liệu .NET tiếng Anh khi quyết định production.

## Verification Metadata

- Last verified: 2026-08-11.
- Versions/scope: current Linux kernel documentation and Linux man-pages accessed on verification date; systemd main branch source docs; .NET 10 diagnostics guidance.
- Context7: /dotnet/docs used to cross-check .NET diagnostic and lifecycle documentation.
- Runtime note: shell labs are bounded and target only child processes created by the learner. They were source-reviewed; no active Linux runtime was available in this Windows workspace for execution verification.
