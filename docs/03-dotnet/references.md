# References — Module 03 C#/.NET Runtime

> [← Module overview](README.md)

## Official — C# language

- [Generic types and methods](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [C# type system](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/)
- [C# exception handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/exception-handling)
- [Creating and throwing exceptions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)
- [Asynchronous programming with async and await](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [Asynchronous programming scenarios](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios)
- [C# language specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/)

## Official — .NET execution and ownership

- [Task-based asynchronous programming](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming)
- [Task cancellation](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation)
- [Consuming the Task-based Asynchronous Pattern](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern)
- [Managed thread pool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool)
- [TaskScheduler](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-threading-tasks-taskscheduler)
- [Overview of synchronization primitives](https://learn.microsoft.com/en-us/dotnet/standard/threading/overview-of-synchronization-primitives)
- [Using objects that implement IDisposable](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/using-objects)
- [Implement a Dispose method](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
- [Implement a DisposeAsync method](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync)
- [Garbage collection fundamentals](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals)
- [Garbage collection and performance](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/performance)
- [Large object heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap)

## Official — Host, DI, configuration and logging

- [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [Dependency injection usage](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/usage)
- [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [Logging in C#](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/overview)
- [Worker services](https://learn.microsoft.com/en-us/dotnet/core/extensions/workers)

## Official — Diagnostics

- [.NET diagnostics overview](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/)
- [Diagnostics tools overview](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/tools-overview)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [dotnet-stack](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-stack)
- [dotnet-dump](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-dump)
- [Dumps and sensitive data](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dumps)
- [Creating metrics](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation)
- [EventCounters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/event-counters)

## Roadmap / scope discovery

- [roadmap.sh C#](https://roadmap.sh/csharp)
- [roadmap.sh ASP.NET Core](https://roadmap.sh/aspnet-core)
- [roadmap.sh Backend](https://roadmap.sh/backend)

Roadmap sources discover scope; Microsoft Learn and C# specifications decide behavior.

## Vietnamese Resources

Không chọn community tutorial tiếng Việt làm canonical. Có thể dùng language switch trên Microsoft Learn để hỗ trợ tiếp thu, nhưng bản English là source of truth cho API, lifecycle và version-sensitive behavior.

## Source decisions

| Câu hỏi | Nguồn quyết định | Ghi chú |
| --- | --- | --- |
| Generic type/constraint semantics | C# guide/specification | Stable language behavior; JIT representation là implementation detail |
| Exception/cancellation behavior | C# guide + Task cancellation docs | Cancellation cooperative; `OperationCanceledException` state semantics cần giữ |
| Host lifetime/config/logging | Generic Host docs | `HostApplicationBuilder` recommended cho app mới; `IHostBuilder` giữ cho compatibility |
| Runtime diagnostics | Diagnostics tools docs | Tool access, bitness, ptrace/container permissions là operational constraints |
| GC/allocations | GC fundamentals/performance | Allocation rate, survival và retained graph quan trọng hơn một heap snapshot |

## Verification metadata

- Verified: 2026-08-11.
- Technology version: .NET 10 target; current local SDK/runtime recorded in lab evidence.
- Official sources: links above.
- Context7 queries used: none; callable Context7 tool unavailable in this run.
- Notes: package-specific examples are kept minimal; executable lab uses BCL only to avoid hiding runtime behavior behind external dependencies.
