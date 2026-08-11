# References — Module 01 Computer Science Essentials

> [← Module overview](README.md)

Official English documentation là source of truth. Danh sách này ưu tiên nguồn mô tả trực tiếp behavior và giữ các deep dive ổn định tách khỏi API phụ thuộc phiên bản.

## Official — .NET

- [Collections and data structures — C# reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/collections)
- [Dictionary<TKey,TValue> — .NET 10](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-10.0)
- [HashSet<T> — .NET 10](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-10.0)
- [PriorityQueue<TElement,TPriority> — .NET 10](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2?view=net-10.0)
- [Thread-safe collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [Managed thread pool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool)
- [Task Parallel Library](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)
- [Task-based asynchronous programming](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming)
- [TaskScheduler supplementary remarks](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-threading-tasks-taskscheduler)
- [Overview of synchronization primitives](https://learn.microsoft.com/en-us/dotnet/standard/threading/overview-of-synchronization-primitives)
- [Synchronizing data for multithreading](https://learn.microsoft.com/en-us/dotnet/standard/threading/synchronizing-data-for-multithreading)
- [Interlocked — .NET 10](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked?view=net-10.0)
- [Fundamentals of garbage collection](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals)
- [Garbage collection and performance](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/performance)
- [Large object heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap)
- [Stopwatch — .NET 10](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-10.0)

## Official — Linux và OS behavior

- [EEVDF Scheduler](https://docs.kernel.org/scheduler/sched-eevdf.html)
- [CFS Scheduler Design — historical mental model](https://docs.kernel.org/6.14/scheduler/sched-design-CFS.html)
- [sched(7) — Linux scheduling policies](https://man7.org/linux/man-pages/man7/sched.7.html)
- [Linux Memory Management](https://docs.kernel.org/admin-guide/mm/)
- [Page Tables, MMU, TLB and Page Faults](https://docs.kernel.org/mm/page_tables.html)
- [proc_pid_status(5)](https://man7.org/linux/man-pages/man5/proc_pid_status.5.html)
- [proc_meminfo(5)](https://man7.org/linux/man-pages/man5/proc_meminfo.5.html)
- [getrusage(2)](https://man7.org/linux/man-pages/man2/getrusage.2.html)

## Specifications và language memory model

- [C# language specification — variables](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/variables)
- [ECMA-335 — Common Language Infrastructure](https://ecma-international.org/publications-and-standards/standards/ecma-335/)
- [POSIX.1-2024 — System Interfaces](https://pubs.opengroup.org/onlinepubs/9799919799/)

## Academic và foundational deep dive

- [MIT 6.006 — Asymptotic Complexity notes](https://ocw.mit.edu/courses/6-006-introduction-to-algorithms-fall-2011/ce8348ec64dce3841ced6a9d0c9e48f2_MIT6_006F11_rec01.pdf)
- [MIT 6.006 — Introduction to Algorithms course](https://ocw.mit.edu/courses/6-006-introduction-to-algorithms-fall-2011/)
- [Operating Systems: Three Easy Pieces](https://pages.cs.wisc.edu/~remzi/OSTEP/) — textbook do các tác giả phát hành miễn phí; dùng cho conceptual deep dive, không thay official runtime/kernel docs.

## Roadmap / scope discovery

- [roadmap.sh Computer Science](https://roadmap.sh/computer-science)
- [roadmap.sh Data Structures and Algorithms](https://roadmap.sh/datastructures-and-algorithms)

Roadmap sources chỉ giúp phát hiện scope. Chúng không quyết định behavior của .NET runtime hay Linux kernel.

## Vietnamese

Không chọn community tutorial tiếng Việt làm nguồn canonical cho module này. Khi Microsoft Learn cung cấp bản địa hóa, dùng language switch trên chính trang official nhưng vẫn đối chiếu bản English vì localization có thể trễ hơn.

## Source decisions

| Câu hỏi | Nguồn quyết định | Ghi chú |
| --- | --- | --- |
| API/behavior của collection .NET | Microsoft Learn với view `net-10.0` | Implementation detail có thể đổi; không hard-code internal capacity/load factor |
| Scheduler Linux hiện đại | Linux EEVDF docs | CFS page chỉ dùng để giải thích lịch sử và `vruntime` |
| Process scheduling policy | Linux man-pages và kernel docs | Không suy diễn real-time guarantee từ nice value |
| Virtual memory/page fault | Linux kernel docs | Phân biệt stable concept với kiến trúc phần cứng cụ thể |
| GC generations/LOH | Microsoft Learn | Segment size và heuristic là implementation-specific |
| Asymptotic notation | MIT OCW | Dùng cho định nghĩa; production decision vẫn cần workload evidence |

## Verification metadata

- Verified: 2026-08-11.
- Technology version: .NET 10; Linux documentation current at verification time, with CFS explicitly marked historical.
- Official sources: links above.
- Context7 queries used: không có Context7 tool khả dụng trong run này.
- Notes: link health được kiểm tra ở bước validation cuối module.
