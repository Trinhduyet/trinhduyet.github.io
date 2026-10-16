# C# / .NET Backend — học đến đâu trả lời được đến đó

<div class="lesson-meta"><span>Runtime · ASP.NET Core · API</span><span>9 câu có lời giải</span></div>

[Bắt đầu với bộ câu hỏi .NET →](../05-interview-preparation/dotnet-api.md){ .hero-button .hero-button-primary }

## Bản đồ kiến thức

| Chủ đề | Cần hiểu thực sự | Bài chuyên sâu |
| --- | --- | --- |
| Type system | Value vs reference, boxing, record, nullable | [C# types](../../03-dotnet/csharp-types-generics-and-collections.md) |
| Async/Concurrency | I/O vs CPU-bound, cancellation, ThreadPool | [Async](../../03-dotnet/async-await-cancellation-and-task-lifecycle.md) · [ThreadPool](../../03-dotnet/threadpool-concurrency-and-diagnostics.md) |
| Memory/Resource | Allocation, GC, IDisposable, leaks | [GC](../../03-dotnet/gc-allocations-and-runtime-memory.md) |
| ASP.NET Core | DI, middleware, hosting, auth, error handling | [Pipeline](../../07-aspnet-core/pipeline-hosting-and-configuration.md) |
| API Design | HTTP semantics, idempotency, pagination, caching | [API contracts](../../06-api-design/http-resource-contracts-and-semantics.md) |
| Quality | Unit/integration/contract/load tests | [Test strategy](../../08-testing-code-review/test-strategy-and-boundaries.md) |

## Một câu hỏi mẫu

**`async/await` có tạo thread mới?** Không. Async I/O cho phép thread phục vụ request khác khi chờ I/O, thay vì blocking; task continuation chạy khi có kết quả. `Task.Run` dành cho workload CPU-bound được đo đạc, không phải cách cải thiện mọi SQL/HTTP call.

**Follow-up:** Dưới tải cao, sync-over-async có thể khiến ThreadPool queue tăng thế nào? Cần kiểm chứng bằng metrics/tracing thay vì suy đoán.

## Học trong một buổi 45 phút

1. Chọn [NET-01, NET-02 và NET-03](../05-interview-preparation/dotnet-api.md), tự nói đáp án.
2. Đọc bài async/DI, viết API có CancellationToken và Scoped DbContext.
3. Cố tình timeout dependency; quan sát trace và phản ứng của API.
4. Ghi một failure path và điều kiện để cải thiện.

**Tiếp theo:** [SQL & EF Core](../02-data-and-distributed-systems/index.md) · [Mock Interview](../05-interview-preparation/mock-interview.md).
