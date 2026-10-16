# .NET Foundations — C#, Runtime, ASP.NET Core

**Goal:** Giải thích tại sao code hoạt động, làm được bài lab và điều tra được lỗi; không chỉ học thuộc định nghĩa.

## Các chủ đề phải nắm

- Value/reference semantics, record/class/struct, generic collections, nullable reference types.
- async/await, Task vs ValueTask, cancellation, ThreadPool starvation và deadlocks.
- Garbage collection, allocation, IDisposable, memory leak, diagnostics.
- ASP.NET Core pipeline, dependency injection lifetimes, middleware, options, authentication/authorization.
- API contracts: REST, GraphQL, pagination, rate limiting, versioning và error behavior.

## Phỏng vấn theo chiều sâu

**Q: `async` có tạo thread mới không?** Không. `async/await` biểu diễn state machine; bất đồng bộ I/O thường giải phóng thread trong thời gian chờ. `Task.Run` chủ yếu dành cho CPU-bound work khi có lý do, không dùng để bọc SQL asynchronous.

**Follow-up:** CancellationToken phải được truyền xuống những layer nào? Vì sao `.Result`/blocking trên ASP.NET có thể gây thread pool starvation? Cách đo bằng `dotnet-counters`/trace?

**Q: Scoped service có thể inject vào singleton không?** Không nên giữ scoped dependency vượt scope request; cần tạo scope đúng và quản lý lifetime phù hợp. Kiểm tra ValidateScopes trong development.

## Đọc và làm

[C# types](../../03-dotnet/csharp-types-generics-and-collections.md) · [Async](../../03-dotnet/async-await-cancellation-and-task-lifecycle.md) · [GC](../../03-dotnet/gc-allocations-and-runtime-memory.md) · [ASP.NET Core](../../07-aspnet-core/README.md).
