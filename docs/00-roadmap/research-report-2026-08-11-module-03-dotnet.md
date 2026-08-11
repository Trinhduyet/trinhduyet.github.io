# Research report — Module 03 C#/.NET Runtime

## Topic researched

- C# types, generics, constraints, exceptions và filters;
- deterministic disposal, `IDisposable`, `IAsyncDisposable` và ownership cascade;
- TAP, `async`/`await`, state-machine mental model, cancellation và deadlines;
- ThreadPool/TaskScheduler, sync-over-async, concurrency limits và diagnostics;
- Generic Host, DI, configuration/options, logging và graceful shutdown;
- GC generations, allocation rate, retained heap, LOH, dumps, counters và traces.

## Official sources found

- Microsoft Learn C# guides/specification cho generics, exceptions và async.
- Microsoft Learn .NET runtime docs cho Task cancellation, ThreadPool, TaskScheduler, disposal và GC.
- Microsoft Learn Generic Host, DI, Options, logging và Worker Services.
- Microsoft Learn diagnostics tools: `dotnet-counters`, `dotnet-trace`, `dotnet-stack`, `dotnet-dump`, EventPipe/Metrics.

Danh sách canonical nằm tại [Module 03 references](../03-dotnet/references.md).

## Versions checked

- Repository target: .NET 10 LTS; local SDK was verified separately before lab build.
- Current host API docs recommend `HostApplicationBuilder`/`Host.CreateApplicationBuilder` for new apps while retaining `IHostBuilder` for compatibility scenarios.
- Runtime diagnostics pages distinguish modern `System.Diagnostics.Metrics` from older EventCounters; module teaches both boundary and migration nuance.

## Nuance and decisions

1. Async is an execution model, not a promise of parallelism. The module keeps I/O-bound awaits, CPU-bound `Task.Run`, ThreadPool scheduling and cancellation as separate concepts.
2. Cancellation is cooperative. A task may return normally after noticing a token, or transition Canceled when it throws `OperationCanceledException` with the relevant token.
3. GC manages managed memory but does not know `Dispose` semantics. Resource ownership remains explicit and deterministic.
4. Generic Host is a lifecycle composition root, not an excuse to put domain logic into `Program.cs` or register every object as a singleton.
5. Diagnostics artifacts may contain process memory and secrets; capture, access, retention and container permission are part of the design.

## Context7

Context7 did not expose a callable tool in this run. No fictional library IDs or queries were recorded. Version-sensitive APIs were checked against direct Microsoft Learn pages and the repository baseline.

## Files delivered

- `docs/03-dotnet/README.md`
- `docs/03-dotnet/references.md`
- five Module 03 chapters using the 28-heading template
- `labs/03-dotnet/runtime-lab/RuntimeLab.csproj`
- `labs/03-dotnet/runtime-lab/Program.cs`
- root README, roadmap status, prerequisites, learning path and skills matrix.

## Verification evidence

- `dotnet build -c Release`: succeeded with 0 warnings and 0 errors.
- `cancellation 10000 25`: bounded channel produced 512 and consumed 511 before cooperative cancellation; both tasks completed without an unhandled exception.
- `allocation 100000`: checksums matched; naive path allocated 28,000,000 bytes with two Gen0 collections, pooled path allocated 0 measured bytes with zero Gen0 collections after warm-up.
- `diagnostics`: emitted .NET 10.0.9 runtime, x64/Windows context, GC mode, heap snapshot, collection counts, working set and ThreadPool min/max/available values.
- Invalid bounds and unknown commands return usage exit code 64; static validation found five chapters with 28 headings each and no placeholder markers.

## Multi-role review

| Role | Review question | Module 03 answer |
| --- | --- | --- |
| Senior Engineer | Có thể implement đúng không? | Examples và RuntimeLab cover ownership, cancellation, pooling và bounded input. |
| Security Engineer | Secrets/blast radius ở đâu? | Pooled buffers, dumps, dumps/trace access, memory exhaustion và cancellation boundaries được nêu rõ. |
| Performance Engineer | Đo gì trước khi tune? | Allocation flow, retained heap, GC counters, ThreadPool evidence, workload bounds và tail latency. |
| Operations Engineer | Diagnose/recover thế nào? | Counters → trace → stack → dump ladder, graceful shutdown và memory/diagnostic runbook. |
| Software Architect | Boundary/trade-off nào? | Type/resource/execution/host boundaries, capacity ownership và simpler alternative trước pooling/parallelism. |

## Verification metadata

- Verified: 2026-08-11.
- Scope: source and design decisions for Module 03.
- Official sources: [Module 03 references](../03-dotnet/references.md).
- Context7 queries used: none; tool unavailable.
- Notes: learner level remains unchanged until RuntimeLab and project evidence are produced.
