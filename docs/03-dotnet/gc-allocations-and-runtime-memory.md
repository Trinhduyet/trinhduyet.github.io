# GC, Allocations và Runtime Memory

> [← ThreadPool và diagnostics](threadpool-concurrency-and-diagnostics.md) · [Module overview](README.md) · Nối với [memory model](../01-computer-science/memory-stack-heap-virtual-memory-and-cache.md)

## Mục tiêu / Learning Objectives

- phân biệt allocation rate, live/retained heap, working set, RSS và native memory;
- giải thích generations, LOH, POH, collection pause và promotion;
- đọc `GC.GetGCMemoryInfo`, runtime counters và dump theo hypothesis;
- chọn object lifetime, pooling, `Span<T>`, `Memory<T>` và `ArrayPool<T>` có lý do;
- tránh pinning, pool misuse, unbounded buffers và memory-sensitive diagnostics;
- đặt memory budget, failure mode và verification evidence cho service.

## Tại sao cần học? / Why It Matters

Một service có thể “memory leak” dù GC vẫn chạy: retained references giữ object sống, cache không có eviction, native buffer nằm ngoài managed heap, hoặc queue tăng vì downstream chậm. Ngược lại, allocation rate cao nhưng heap ổn định có thể chỉ là workload tạo rác ngắn hạn; tối ưu mù bằng pool làm code phức tạp và giữ dữ liệu nhạy cảm lâu hơn.

GC là cơ chế thu hồi managed objects, không phải một memory budget manager cho toàn process. Production review phải nối allocation shape với latency, container limit, native dependencies, shutdown và privacy.

## Tổng quan / Overview

~~~mermaid
flowchart LR
    W["Workload<br/>requests · files · messages"]
    A["Allocations<br/>objects · arrays · strings"]
    G["GC generations<br/>Gen0 · Gen1 · Gen2"]
    L["Large / pinned<br/>LOH · POH"]
    M["Memory evidence<br/>heap · native · working set"]
    D["Decision<br/>shape · pool · limit · alert"]
    W --> A --> G --> M --> D
    A --> L --> M
    M -. "hypothesis" .-> A
~~~

## Mental Model

Hãy tách bốn đại lượng:

| Đại lượng | Câu hỏi trả lời | Ví dụ evidence |
| --- | --- | --- |
| Allocation rate | Tạo object nhanh đến mức nào? | `GC.GetAllocatedBytesForCurrentThread`, allocation profiler |
| Live/retained heap | Sau collection còn giữ bao nhiêu? | heap dump, retained graph |
| Managed heap size | GC đang quản lý bao nhiêu? | `GC.GetGCMemoryInfo`, `System.Runtime` counters |
| Process memory | OS/container thấy bao nhiêu? | working set, RSS, cgroup/container metrics |

Allocation là flow; retained heap là stock; working set còn chịu ảnh hưởng của native code, JIT, stacks, mapped files và allocator. Đừng suy ra leak từ một snapshot duy nhất.

## Thuật ngữ / Terminology

| Thuật ngữ | Ý nghĩa |
| --- | --- |
| Managed heap | Vùng heap do CLR quản lý |
| Generation 0/1/2 | Các cohort theo tuổi object |
| Promotion | Object sống sót được chuyển lên generation cao hơn |
| LOH | Large Object Heap, thường cho allocation lớn |
| POH | Pinned Object Heap cho object pinned |
| Compaction | Di chuyển object để giảm fragmentation |
| Pinning | Cố định địa chỉ object, hạn chế compaction |
| Allocation rate | Số byte/object tạo theo thời gian |
| Retention | Reference graph khiến object không thể collect |
| ArrayPool | Pool dùng lại mảng theo bucket |
| Span/Memory | View/buffer API giảm copy, không tự quản ownership |
| Working set | Pages resident của process tại thời điểm đo |

## Prerequisites

- [Stack, heap, virtual memory và cache](../01-computer-science/memory-stack-heap-virtual-memory-and-cache.md).
- [Resource pressure](../02-linux-git-networking/process-signals-and-resource-pressure.md).
- [Ownership và disposal](exceptions-disposable-and-resource-ownership.md).
- [ThreadPool và diagnostics](threadpool-concurrency-and-diagnostics.md).

## How It Works

### Generational GC

Object mới thường bắt đầu ở Gen0. Collection Gen0 nhỏ và thường xuyên; object sống sót được promote. Gen1 là vùng đệm giữa short-lived và long-lived. Gen2 chứa object sống lâu hơn và collection thường đắt hơn. Đây là heuristic, không phải guarantee về thời gian sống business.

GC có thể chạy workstation hoặc server mode tùy runtime/host. Concurrent/background collection giảm pause foreground nhưng không làm mất cost CPU, memory và synchronization. Hãy đo trên deployment target, không chỉ máy developer.

### LOH, POH và fragmentation

Allocation lớn thường vào LOH; LOH không nên được dùng như cách “né” Gen0. Object pinned cần địa chỉ ổn định; pinning lâu có thể gây fragmentation, còn POH tách một số pinned object khỏi heap thường. Kích thước ngưỡng và chi tiết collector phụ thuộc runtime; xem tài liệu/runtime counters thay vì hard-code assumption.

### Collection lifecycle

Một collection xác định root, mark object reachable, sweep/reclaim và có thể compact. Finalizer queue, weak references, handles, interop và pinned buffers làm graph phức tạp hơn. `GC.Collect()` ép collection chỉ phù hợp experiment/controlled boundary; không dùng như production leak fix.

## Minimal Example

~~~csharp
static (long Allocated, int Gen0Collections) Measure(Func<byte[]> work)
{
    var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
    var beforeGen0 = GC.CollectionCount(0);
    var result = work();
    GC.KeepAlive(result);
    return (
        GC.GetAllocatedBytesForCurrentThread() - beforeBytes,
        GC.CollectionCount(0) - beforeGen0);
}

var sample = Measure(() => new byte[1024]);
Console.WriteLine($"allocated={sample.Allocated} gen0={sample.Gen0Collections}");
~~~

Đây là allocation của current thread, không phải toàn process. Dùng workload lặp đủ lớn và warm-up trước khi kết luận.

## Production Example

~~~csharp
public sealed class PooledPayloadFormatter
{
    private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

    public int Format(ReadOnlySpan<byte> input, Span<byte> destination)
    {
        if (destination.Length < input.Length)
            throw new ArgumentException("Destination is too small", nameof(destination));

        input.CopyTo(destination);
        return input.Length;
    }

    public byte[] RentAndCopy(ReadOnlySpan<byte> input)
    {
        var buffer = _pool.Rent(input.Length);
        input.CopyTo(buffer);
        return buffer;
    }

    public void Return(byte[] buffer, int clearLength)
    {
        if ((uint)clearLength > (uint)buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(clearLength));

        _pool.Return(buffer, clearArray: clearLength > 0);
    }
}
~~~

Pool chỉ an toàn khi ownership được encode rõ: caller biết capacity thực, return đúng pool, không dùng buffer sau return, và clear dữ liệu khi buffer có secret. Với payload nhỏ/không nóng, allocation đơn giản thường dễ review hơn.

## .NET Integration

- `GC.GetGCMemoryInfo()` cung cấp snapshot GC memory state phù hợp cho diagnostics, không thay memory profiler.
- `GC.GetAllocatedBytesForCurrentThread()` hữu ích cho micro/experiment; phân tán work qua ThreadPool cần đo thêm process-level evidence.
- `ArrayPool<T>` giúp giảm allocation/copy trong hot path nhưng cần `try/finally` để return.
- `Span<T>` chỉ sống trên stack và không thể cross `await`; `Memory<T>`/`IMemoryOwner<T>` phù hợp lifetime async dài hơn.
- Generic Host nên expose runtime metrics qua logging/metrics, không nhồi full heap snapshot vào health endpoint.

## Internals

GC roots gồm stack references, static fields, handles và runtime structures. Reference graph quyết định retention; một singleton cache có thể giữ cả object graph dù request đã kết thúc. JIT có thể làm lifetime local ngắn/dài hơn dự đoán source-level.

Object header, alignment, reference fields, array length và padding tạo overhead; nhiều object nhỏ có thể đắt hơn một buffer có layout tốt. Boxing biến value type thành object managed; closure và iterator/state machine có thể tạo allocation tùy code shape/runtime.

Pinned interop buffer cần ownership và thời gian pin ngắn. Native allocator có thể không xuất hiện trong managed heap nhưng vẫn đẩy process vượt limit.

## Common Mistakes

- gọi `GC.Collect()` sau mỗi request;
- kết luận leak từ `GC.GetTotalMemory` hoặc working set một lần;
- dùng `ArrayPool` nhưng quên return khi exception;
- return buffer rồi vẫn giữ span/reference đến buffer;
- pool object có mutable state mà không reset;
- giữ toàn bộ request body trong memory trước khi stream;
- dùng `ToList`, `ToArray`, `string.Concat` lặp trong hot loop mà không đo;
- pin một array trong thời gian dài;
- coi `Span<T>` là ownership hoặc thread-safe buffer;
- dump production chứa secrets mà không có access control/retention.

## Performance Considerations

Đo cả throughput và tail latency: allocation rate có thể làm Gen0 pressure; retention/LOH làm Gen2 hoặc fragmentation; pool contention có thể tệ hơn allocation. Benchmark phải có warm-up, realistic payload distribution, concurrency và steady-state.

Giảm copy trước khi giảm object count. Stream payload lớn, dùng `ReadOnlySpan<T>` cho synchronous slice, `Memory<T>` cho async ownership, và partition work để tránh một queue giữ mọi dữ liệu. Pool theo capacity classes; tránh pool vô hạn khiến memory floor tăng.

## Security Considerations

Pooled buffers có thể tái sử dụng bytes của tenant/request trước. Clear dữ liệu nhạy cảm trước return khi threat model yêu cầu, đồng thời giới hạn `clearArray` cost và buffer lifetime. Không log raw payload để “debug allocation”.

Heap dump và trace có thể chứa token, PII, request body và secrets. Chỉ capture với authorization, mã hóa at rest/in transit, retention ngắn và documented deletion. Memory limit phải coi là security boundary chống memory exhaustion.

## Reliability / Failure Modes

OOM là process-fatal hoặc gần fatal tùy allocation/native pressure; không trông chờ exception để graceful recovery mọi lúc. Bounded queue, request-size limit, streaming, cancellation và load shedding giảm blast radius.

Pool misuse gây data corruption khó tái hiện. Dùng `try/finally`, encapsulate owner, test double-return/use-after-return, và fail fast khi capacity invalid. Khi memory pressure tăng, giảm concurrency hoặc shed load thay vì tạo thêm worker.

## Observability

Hypothesis-driven ladder:

1. `System.Runtime` counters: allocation rate, GC heap size, Gen0/1/2 collections, working set, threadpool queue;
2. trace khi cần timeline pause/allocation/CPU;
3. dump/heap analysis khi cần retention graph hoặc state snapshot.

Ghi workload version, runtime version, GC mode, container limit, process id và capture window. Alert theo trend và SLO (p95/p99 latency, OOM/restart, heap after full GC), không chỉ một absolute heap value.

## Operational Considerations

- đặt memory request/limit và headroom cho runtime/native/JIT;
- canary một thay đổi allocation trước khi rollout rộng;
- runbook nêu khi nào dùng counters, trace, dump và ai được phép xem artifact;
- kiểm tra dump storage không nằm trên volume đầy hoặc public;
- `dotnet-counters`/`dotnet-trace` phải pin tool version và thời lượng;
- test graceful shutdown để queue không giữ payload vô hạn.

## Architect Perspective

Memory architecture là contract giữa API boundary, domain lifetime, queue capacity, serializer, cache, persistence và deployment. Chọn streaming hay buffering là quyết định latency/backpressure/privacy, không chỉ micro-optimization.

Review hỏi: dữ liệu được tạo ở đâu, owner là ai, retention kết thúc khi nào, giới hạn capacity ở đâu, metric nào chứng minh, và failure nào xảy ra khi downstream chậm hoặc container gần limit?

## Trade-offs

| Lựa chọn | Lợi ích | Chi phí/rủi ro |
| --- | --- | --- |
| Allocate object mới | Dễ đọc, ownership rõ | Allocation/GC pressure |
| `ArrayPool<T>` | Giảm allocation/copy | Return/reset/pooling complexity |
| Buffering | Dễ retry/parse | Peak memory, latency |
| Streaming | Bounded memory | State/error/retry phức tạp |
| Copy | Isolation, thread-safety rõ | CPU/memory bandwidth |
| Zero-copy view | Nhanh trong hot path | Lifetime/pinning/aliasing khó |

## When NOT to Use It

- Không dùng pool cho code không chứng minh allocation hot path.
- Không dùng `unsafe`/pinning để sửa một latency outlier chưa có profile.
- Không ép `Span<T>` vào API async hoặc lưu nó qua `await`.
- Không bật heap dump liên tục trong production.
- Không dùng static cache nếu không có eviction, size bound và ownership policy.

## Alternatives

- `ImmutableArray<T>`/record cho snapshot immutable thay vì shared mutable pool.
- `MemoryPool<T>` hoặc `IMemoryOwner<T>` khi ownership cần được encode trong type.
- Streaming `PipeReader`/`Stream` khi payload lớn.
- External cache/queue khi process memory không phải nơi giữ durable state.
- Native allocator/interop chỉ khi profile chứng minh và có security/cleanup contract.

## Review Questions

1. Allocation rate cao nhưng heap sau Gen2 ổn định nói gì?
2. Vì sao working set có thể tăng khi managed heap không tăng?
3. Khi nào `ArrayPool<T>` tạo bug bảo mật?
4. Tại sao `GC.Collect()` không phải leak fix?
5. Queue capacity và memory budget liên hệ thế nào?
6. Evidence nào phân biệt leak retained graph với native memory pressure?

## Hands-on Lab

Trong [RuntimeLab](../../labs/03-dotnet/runtime-lab/Program.cs), chạy:

~~~powershell
dotnet run -c Release --no-build -- allocation 100000
dotnet run -c Release --no-build -- diagnostics
~~~

Ghi lại checksum, allocated bytes, Gen0/1/2 delta, heap size, working set và runtime/GC mode. Lặp với payload khác nhau; không kết luận từ một lần chạy. Tạo decision note: allocate hay pool, bound nào, metric nào và rollback ra sao.

## Exit Criteria

- giải thích được flow/stock của allocation và retention;
- đọc được GC/working-set evidence mà không nhầm managed với native memory;
- viết được buffer ownership có return/clear/error path;
- chọn streaming, pooling hoặc allocation đơn giản dựa trên profile;
- đặt memory limit, queue bound, diagnostics privacy và OOM runbook;
- chạy RuntimeLab và lưu output reproducible.

## Related Topics

- [C# types, generics và collections](csharp-types-generics-and-collections.md).
- [Exceptions, IDisposable và ownership](exceptions-disposable-and-resource-ownership.md).
- [Async/cancellation](async-await-cancellation-and-task-lifecycle.md).
- [ThreadPool và diagnostics](threadpool-concurrency-and-diagnostics.md).
- [Module 01 memory model](../01-computer-science/memory-stack-heap-virtual-memory-and-cache.md).
- [Module 02 resource pressure](../02-linux-git-networking/process-signals-and-resource-pressure.md).

## Official English Sources

- [GC fundamentals](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals).
- [GC performance](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/performance).
- [Large Object Heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap).
- [Using objects / IDisposable](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/using-objects).
- [Diagnostics overview](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/).
- [Metrics instrumentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation).

## Vietnamese Resources

- Đọc [glossary](../00-roadmap/glossary.md) để thống nhất allocation, retention, RSS, backpressure.
- Viết decision record bằng tiếng Việt, giữ canonical English term trong ngoặc.
- Đối chiếu output RuntimeLab với memory model của Module 01 và resource pressure của Module 02.

## Verification Metadata

- Verified: 2026-08-11.
- Technology version: .NET 10 target; runtime details phải lấy từ command `diagnostics` trên máy chạy.
- Source policy: Microsoft Learn primary sources; không dùng benchmark blog làm normative claim.
- Context7 queries used: không có Context7 callable tool trong run này.
- Notes: GC behavior phụ thuộc runtime, OS, GC mode, workload và container limit; mọi tuning cần evidence.
