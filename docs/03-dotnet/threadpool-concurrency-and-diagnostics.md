# ThreadPool, Generic Host, Concurrency và Runtime Diagnostics

> [← Async/await](async-await-cancellation-and-task-lifecycle.md) · [Module overview](README.md) · Tiếp theo: [GC và allocations](gc-allocations-and-runtime-memory.md)

## Mục tiêu / Learning Objectives

- giải thích ThreadPool, TaskScheduler, queueing, work stealing và thread injection;
- phân biệt CPU saturation, ThreadPool starvation, lock contention và downstream wait;
- compose Generic Host với DI, configuration/options, logging, hosted services và shutdown;
- chọn concurrency/bulkhead/backpressure boundary;
- dùng dotnet-counters, dotnet-trace, dotnet-stack và dotnet-dump theo hypothesis;
- thiết kế diagnostic artifacts có permission, privacy, retention và runbook.

## Tại sao cần học? / Why It Matters

Service chậm không đồng nghĩa CPU cao. Synchronous blocking có thể làm ThreadPool queue tăng trong khi CPU thấp; quá nhiều worker làm lock contention/memory tăng; Generic Host có thể shutdown mà worker chưa drain; dump có thể chứa toàn bộ process memory.

Runtime diagnostics có giá trị khi bắt đầu bằng symptom và hypothesis. Chạy mọi tool liên tục hoặc tăng ThreadPool minimum theo cảm giác thường tạo thêm noise và cost.

## Tổng quan / Overview

~~~mermaid
flowchart LR
    S["Symptom<br/>latency · CPU · memory · hang"]
    H["Hypothesis<br/>starvation · lock · GC · dependency"]
    C["Counters<br/>rate · queue · resources"]
    T["Trace/stack<br/>timeline · waits · hot path"]
    D["Dump<br/>state · heap · locks"]
    F["Fix + verify<br/>workload · rollback · runbook"]
    S --> H --> C --> T --> D --> F
    C -. "narrow/stop" .-> H
    T -. "capture cost" .-> F
~~~

## Mental Model

ThreadPool là shared process resource. .NET có một pool cho mỗi process, được dùng bởi TPL tasks, async I/O completions, timers và callbacks. Work queue có thể tăng theo memory; active workers bị runtime điều tiết. Blocking workers làm continuation mới chờ.

Generic Host là lifecycle graph gom configuration, DI, logging, IHostedService và shutdown. Host là composition root, không tự làm dependency scope hoặc thread-safety đúng.

Diagnostics ladder: counters để xác định “có gì xấu”, trace để thấy timeline, stack để thấy threads đang chờ ở đâu, dump để inspect state/heap/locks.

## Thuật ngữ / Terminology

| Thuật ngữ | Ý nghĩa |
| --- | --- |
| ThreadPool worker | Runtime-managed worker xử lý queued work |
| Work stealing | Worker idle lấy work từ queue worker khác |
| Thread injection | Runtime tạo worker theo demand/throughput |
| Starvation | Work không có worker tiến triển đủ nhanh |
| Saturation | Resource gần hết capacity |
| Bulkhead | Tách capacity/work class |
| Backpressure | Consumer truyền capacity pressure về producer |
| Generic Host | DI/config/logging/lifetime composition |
| Scope | Lifetime boundary cho dependencies |
| Counter | Rate hoặc snapshot measurement |
| Trace | Timeline events/spans/profiles |
| Dump | Process snapshot, có thể nhạy cảm |
| SOS | Managed runtime/dump debugger extension |

## Prerequisites

- [Module 01 concurrency](../01-computer-science/process-thread-scheduling-and-concurrency.md).
- [Async/cancellation](async-await-cancellation-and-task-lifecycle.md).
- [Module 02 process/resource diagnosis](../02-linux-git-networking/process-signals-and-resource-pressure.md).

## How It Works

### ThreadPool và starvation

Default TaskScheduler chạy trên ThreadPool, hỗ trợ local/global queues, work stealing và thread injection/retirement. Task ngắn, CPU work và async continuation phù hợp pool; long-running blocking work có thể giữ workers.

| Signal | Starvation | CPU saturation |
| --- | --- | --- |
| CPU | Có thể thấp | Cao hoặc quota throttled |
| Pool queue | Tăng | Có thể bình thường |
| Stacks | Wait/Result/sync I/O | CPU hot methods |
| Fix | Async end-to-end, remove block | Giảm work/parallelism, optimize |

Không coi SetMinThreads là fix đầu tiên; tăng minimum không cần thiết có thể tăng contention.

### Concurrency policy

Limit theo constrained resource: DB connections, external quota, CPU, memory hoặc descriptors. Semaphore/Channel cần capacity/full behavior; per-tenant bulkhead ngăn noisy neighbor.

### Generic Host

~~~csharp
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<WorkerOptions>()
    .Bind(builder.Configuration.GetSection("Worker"))
    .ValidateOnStart();
builder.Services.AddHostedService<FileWorker>();

using IHost host = builder.Build();
await host.RunAsync();
~~~

Dùng Singleton cho process-wide thread-safe state, Scoped cho unit/request scope, Transient cho short-lived dependency. Singleton worker cần IServiceScopeFactory để tạo scope cho từng unit.

### Hosted service lifecycle

BackgroundService phải observe stopping token, drain/cancel trong grace period và dispose resource. Startup/config validation failure nên làm service không ready thay chạy degraded không quan sát.

### Diagnostics

dotnet-counters phù hợp first-level investigation; System.Diagnostics.Metrics là hướng mới hơn, EventCounters giữ compatibility. dotnet-trace dùng EventPipe profiling; dotnet-stack in managed stacks; dotnet-dump collect/analyze dump. Tool cần PID, bitness, user và permission đúng; container dump có thể cần ptrace.

### Hypothesis workflow

1. Ghi symptom, SLO, version, deployment và workload.
2. Chụp counters ngắn để xác định rate/queue/resource.
3. Trace một cửa sổ đủ đại diện.
4. Stack/dump chỉ khi cần state/roots/locks.
5. Sửa nhỏ nhất, replay workload, so before/after.
6. Cập nhật runbook/alert.

## Minimal Example

~~~csharp
public sealed class FileWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<FileWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IFileProcessor>();
            await processor.ProcessNextAsync(stoppingToken);
            logger.LogDebug("Processed one file unit");
        }
    }
}
~~~

Production cần idle delay/backpressure, error classification, metrics và queue ownership.

## Production Example

Symptom: p99 tăng, CPU 30%, queue length tăng.

~~~powershell
dotnet-counters monitor --process-id <PID> --refresh-interval 3 --counters System.Runtime
dotnet-stack report --process-id <PID>
dotnet-trace collect --process-id <PID> --duration 00:00:20
~~~

Counters xác nhận queue/thread/working set; stack tìm sync-over-async/blocked lock; trace phân biệt wait và CPU path. Chỉ collect dump khi hypothesis cần deep state:

~~~powershell
dotnet-dump collect --process-id <PID> --type Heap --output .\artifacts\starvation.dmp
~~~

PID phải được resolve/read-only từ đúng instance; dump path cần access/retention policy.

## .NET Integration

- Host.CreateApplicationBuilder, IHostedService, BackgroundService và IServiceScopeFactory là lifecycle primitives.
- ILogger<T> và source-generated logging tạo structured telemetry.
- IOptions<T>, IOptionsSnapshot<T>, IOptionsMonitor<T> có lifetime/change semantics khác nhau.
- dotnet-counters đọc System.Runtime metrics/counters; xác minh tool/version trước incident.
- dotnet-trace/dotnet-stack/dotnet-dump cần same-user/correct-bitness permissions.

## Internals

TaskScheduler có thể inline task; local/global/work-steal decisions ảnh hưởng ordering và cache nhưng không phải business ordering. ThreadPool heuristics không deterministic giữa runtime patches. EventPipe sampling/provider level ảnh hưởng overhead. Host stop signal chỉ có ý nghĩa khi hosted services observe token.

## Common Mistakes

- Tăng ThreadPool min/max để che blocking.
- Một queue cho tất cả tenants/work classes.
- Inject scoped dependency vào singleton.
- Worker loop không cancellation/backpressure.
- Log mỗi poll thành high-cardinality noise.
- Collect dump chứa PII không policy.
- Attach tool vào PID sai/sidecar.
- Alert task/thread count mà bỏ queue age/SLO.
- Validate config quá muộn tới first request.

## Performance Considerations

Concurrency limit theo downstream capacity, không chỉ logical CPU. Worker unit phải đủ lớn để không scheduler-overhead dominated nhưng đủ nhỏ để fairness. Trace có bounded window; counters phù hợp ongoing monitoring. Tách CPU-heavy và I/O-heavy work để giảm starvation.

## Security Considerations

DI/config có thể chứa secrets; không log toàn bộ options. Diagnostic tools cần process access và least privilege. Dump chứa memory/credentials nên encrypt, access-control, expiry và delete. ptrace capability mở rộng attack surface. Per-tenant queues/limits ngăn resource exhaustion và data cross-talk.

## Reliability / Failure Modes

| Failure | Signal | Response |
| --- | --- | --- |
| ThreadPool starvation | Queue/latency cao, CPU thấp | Async chain, remove block, bound work |
| CPU saturation | CPU/run queue cao | Giảm parallelism, optimize, scale |
| Scope leak | Memory/connection tăng | Scope per unit, dispose, diagnostics |
| Worker crash | Restart loop | Propagate error, supervisor, DLQ |
| Shutdown timeout | Token bị bỏ qua | Cooperative drain, forced policy |
| Queue overload | Age/memory/reject tăng | Backpressure, shed, durable spill |
| Diagnostic failure | Permission/bitness/ptrace | Preflight, fallback, runbook |

## Observability

- System.Runtime: CPU/time, working set, GC, ThreadPool queue/thread/work items, lock contention;
- host lifecycle: startup, readiness, stop/drain duration, hosted-service failures;
- queue: depth, age, enqueue/dequeue/reject, tenant share;
- cancellation/failure reason và dependency wait;
- diagnostic artifact inventory/access audit.

## Operational Considerations

- Pin diagnostic tool version/architecture trong runbook.
- Capture baseline counters; stop trace/dump khi đủ evidence.
- Kiểm tra disk capacity và sensitive-artifact retention.
- Test invalid config, shutdown under backlog và worker restart.
- Chỉ cấp diagnostic container permissions on demand.
- Canary thay đổi concurrency/ThreadPool; so queue age/SLO.

## Architect Perspective

- Work nên chạy trên shared pool, bounded worker, dedicated thread hay process?
- Host owns lifecycle nào; DI scope map với unit of work ra sao?
- Queue full thì reject/drop/persist/degrade thế nào?
- Counter/trace/dump nào đủ chứng minh hypothesis?
- Ai sở hữu permission và retention của artifact?

Ở 10x cần bulkhead, per-tenant budget, worker partition và counters tốt hơn. Ở 100x cần external queue, multiple processes/nodes, durable state và fleet observability; tăng ThreadPool không đổi external capacity.

## Trade-offs

| Choice | Lợi ích | Giá phải trả |
| --- | --- | --- |
| Shared ThreadPool | Reuse, simple | Starvation coupling |
| Dedicated worker | Isolation | Threads/memory/lifecycle |
| Generic Host | Standard lifecycle/DI/logging | Composition complexity |
| Bounded queue | Protect memory/downstream | Reject/latency policy |
| Counters | Low overhead trend | Limited causal detail |
| Trace | Timeline/cause | Capture/storage cost |
| Dump | Deep state/heap | Sensitive, heavy snapshot |

## When NOT to Use It

- Không custom TaskScheduler trước khi default + bounded design chứng minh thiếu.
- Không chạy dump định kỳ như metrics.
- Không dùng Generic Host làm service locator.
- Không register mọi service Singleton để “giảm allocation”.
- Không grant ptrace permanently nếu on-demand đủ.
- Không đặt một concurrency limit chung cho work cost khác nhau.

## Alternatives

- Process supervisor/container restart cho isolation.
- Durable broker/queue cho replayable work.
- OpenTelemetry metrics/traces cho continuous distributed evidence.
- OS tools ps/top/perf/proc để correlate host boundary.
- Single-owner loop thay shared concurrent workers.
- Microsoft.Diagnostics.NETCore.Client khi CLI không đủ.

## Review Questions

1. ThreadPool starvation khác CPU saturation thế nào?
2. Vì sao SetMinThreads không phải fix đầu tiên?
3. Generic Host giải quyết lifecycle nào?
4. Vì sao singleton worker cần tạo scope?
5. Counters, trace, stack và dump khác evidence nào?
6. Dump artifact tạo security risk gì?
7. Queue full behavior nên quyết định ở đâu?
8. Khi nào custom scheduler/dedicated process hợp lý?

## Hands-on Lab

### Problem

Đọc runtime snapshot và diễn giải cancellation/allocation evidence, sau đó thử diagnostics tool nếu cài sẵn.

### Constraints

- Chỉ attach PID RuntimeLab do learner khởi chạy.
- Không collect dump vào repo hoặc upload artifact nhạy cảm.
- Tool unavailable thì ghi rõ, không giả output.

### Implementation steps

~~~powershell
cd E:\Documents\Dev\labs\03-dotnet\runtime-lab
dotnet run -c Release --no-build -- diagnostics
dotnet run -c Release --no-build -- allocation 100000
~~~

Nếu có dotnet-counters:

~~~powershell
dotnet-counters monitor --counters System.Runtime -- dotnet run --project .\RuntimeLab.csproj -- diagnostics
~~~

### Expected outcome

Diagnostics in runtime/GC/ThreadPool snapshot; allocation reports deltas. Counter tool có thể kết thúc nhanh vì lab bounded.

### Verification

Ghi tool version, command, permission, output và hypothesis. Đối chiếu WorkingSet/GC info với counter output khi có.

### Failure experiment

Chạy tool vào PID không tồn tại hoặc sai bitness; ghi error và runbook response. Không thử privilege escalation.

### Questions

- Counter nào chỉ là symptom, counter nào giúp narrow hypothesis?
- Khi nào trace/dump overkill?
- Artifact retention/access policy của team là gì?

## Exit Criteria

- Chẩn đoán starvation/saturation/lock/dependency bằng metric plan.
- Viết Generic Host worker có DI scope, cancellation và shutdown.
- Có RuntimeLab diagnostics output và tool availability record.
- Thiết kế diagnostic runbook gồm permission, privacy, storage và rollback.
- Giải thích lúc nào không nên tune ThreadPool/custom scheduler.

## Related Topics

- [Async/await/cancellation](async-await-cancellation-and-task-lifecycle.md)
- [Exceptions/ownership](exceptions-disposable-and-resource-ownership.md)
- [GC/allocations](gc-allocations-and-runtime-memory.md)
- [Module 01 scheduling](../01-computer-science/process-thread-scheduling-and-concurrency.md)
- [Module 02 process/resource diagnosis](../02-linux-git-networking/process-signals-and-resource-pressure.md)
- Module 16 — Observability (planned)

## Official English Sources

- [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [Dependency injection usage](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/usage)
- [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [Logging in C#](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/overview)
- [Managed ThreadPool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool)
- [TaskScheduler](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-threading-tasks-taskscheduler)
- [.NET diagnostics overview](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [dotnet-stack](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-stack)
- [dotnet-dump](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-dump)
- [Module references](references.md)

## Vietnamese Resources

Không dùng tutorial community làm canonical cho ThreadPool/Host/diagnostics. Đối chiếu behavior, permissions và tool syntax với Microsoft Learn current.

## Verification Metadata

- Verified: 2026-08-11.
- Technology version: .NET 10 target; diagnostic tool syntax/metrics may evolve and must be refreshed before incident use.
- Official sources: Microsoft Learn links trên và [references.md](references.md).
- Context7 queries used: none; tool unavailable in this run.
- Notes: diagnostic evidence must be hypothesis-driven and artifacts treated as sensitive operational data.
