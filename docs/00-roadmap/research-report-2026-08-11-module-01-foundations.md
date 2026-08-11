# Research report — Module 01 Computer Science Foundations

## Topic researched

- asymptotic complexity và workload reasoning cho backend;
- .NET collection selection và implementation contracts;
- process/thread/Task, Linux scheduling và .NET ThreadPool;
- synchronization primitives và race conditions;
- virtual memory, page tables, TLB/page faults, .NET GC/LOH và cache locality;
- phương pháp tạo một lab .NET 10 có workload giới hạn và output tái lập.

## Official sources found

- Microsoft Learn: collections, `Dictionary`, `HashSet`, `PriorityQueue`, ThreadPool, TPL, `TaskScheduler`, synchronization, `Interlocked`, GC fundamentals/performance/LOH và `Stopwatch`.
- Linux Kernel documentation: EEVDF, historical CFS design, memory management và page tables.
- Linux man-pages: scheduling policies, process memory/status và resource usage.
- MIT OpenCourseWare 6.006: asymptotic notation và algorithm analysis.

Danh mục có link trực tiếp nằm tại [Module 01 references](../01-computer-science/references.md).

## Versions checked

- Lab target: .NET 10 theo repository technology baseline.
- .NET API pages: `view=net-10.0` khi có version selector.
- Linux scheduler: EEVDF là hướng scheduler fair-class hiện đại; CFS page được dùng như historical mental model, không mô tả nó là implementation duy nhất hiện tại.
- Stable CS concepts không gắn với patch version.

## Conflicting or nuanced information

1. Nhiều tài liệu cũ nói fair scheduler Linux đồng nghĩa CFS. Kernel documentation hiện nói CFS đang nhường chỗ cho EEVDF; module tách current direction và historical mental model.
2. Big-O tables thường ghi hash lookup là `O(1)`. Microsoft ghi retrieval của `Dictionary` gần `O(1)` và phụ thuộc chất lượng hashing; module diễn đạt là expected/average under assumptions, không phải guarantee.
3. “Reference type ở heap, value type ở stack” là shorthand sai trong nhiều trường hợp. Module dùng storage context, lifetime và boxing/capture thay cho quy tắc loại đơn giản.
4. `Task` thường chạy trên ThreadPool nhưng không đồng nghĩa thread. Module giữ ba abstraction riêng.
5. `Stopwatch` đo elapsed time đủ cho lab định hướng, nhưng một lần chạy không phải benchmark; output luôn kèm runtime context và yêu cầu lặp lại.

## Decisions

- Giữ Module 01 thành bốn chương production-relevant thay vì triển khai toàn bộ curriculum DSA.
- Dùng một lab console .NET 10 không có external package cho ba concept: lookup cost, lost update và memory locality.
- Đặt hard bounds trên input để lab không vô tình tiêu thụ tài nguyên lớn.
- Không tăng current level trong Skills Matrix; content và buildable lab chỉ chuyển status sang In progress, learner evidence vẫn pending.
- Không dùng internal constant của .NET collection/GC làm contract kiến trúc.

## Context7

Context7 không có callable tool trong run này. Các claim/API .NET phụ thuộc version được kiểm tra trực tiếp bằng Microsoft Learn và source links do Microsoft công bố. Không ghi nhận query giả.

## Verification performed

- `dotnet build -c Release`: succeeded, 0 warnings, 0 errors.
- `lookup` default: 2.500 hits ở cả hai structures; list lookup 6,707 ms, HashSet build 0,502 ms và lookup 0,354 ms trên runtime local 10.0.9.
- `race` default: expected 800.000; unsafe 292.969; `Interlocked` 800.000.
- `locality` default: checksum bằng nhau; sequential 5,246 ms, randomized 69,643 ms, ratio 13,27×.
- Ba workload vượt comparison/operation/visit budgets đều bị từ chối với exit code 64.

Các timings chỉ là smoke evidence của implementation trên máy hiện tại, không phải benchmark dùng để so hardware hay hứa production performance.

## Files updated

- `docs/01-computer-science/README.md`
- `docs/01-computer-science/references.md`
- bốn chương Module 01
- `labs/01-computer-science/workload-lab/WorkloadLab.csproj`
- `labs/01-computer-science/workload-lab/Program.cs`
- repository README, roadmap overview, master roadmap và skills matrix.

## Verification metadata

- Verified: 2026-08-11.
- Scope: research and source decisions for Module 01.
- Official sources: [Module references](../01-computer-science/references.md).
- Context7 queries used: none; tool unavailable.
- Notes: lab build, smoke output, Markdown, Mermaid và link checks are recorded in the completion report for this run.
