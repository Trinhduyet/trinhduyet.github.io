# C# / .NET / ASP.NET Core — câu hỏi có đáp án

<div class="lesson-meta"><span>Track: Backend Core</span><span>Foundation → Architecture</span><span>9 câu có giải thích</span></div>

**Cách luyện:** đọc câu hỏi trước, tự trả lời trong 90 giây; mở đáp án, tìm sự khác biệt; chạy ví dụ nếu có. Một câu trả lời tốt nêu **cơ chế → rủi ro → khi nào áp dụng → cách kiểm chứng**.

## NET-01. `async/await` có tạo thread mới?

**Trả lời:** Không. Compiler chuyển method `async` thành state machine. Với async I/O thật, thread có thể trả về ThreadPool trong lúc chờ completion; continuation chạy khi operation hoàn thành. `Task.Run` thường dùng để offload CPU-bound work, không biến synchronous I/O thành nonblocking I/O.

**Ví dụ:**
```csharp
app.MapGet("/orders/{id}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.AsNoTracking()
        .Where(o => o.Id == id)
        .Select(o => new { o.Id, o.Status })
        .SingleOrDefaultAsync(ct);

    return order is null ? Results.NotFound() : Results.Ok(order);
});
```

**Follow-up:** Gọi `.Result` dưới tải có thể gây gì? ThreadPool starvation; đo số thread, queue length, p95 và traces. **Đọc sâu:** [Task lifecycle](../../03-dotnet/async-await-cancellation-and-task-lifecycle.md).

## NET-02. DI `Singleton`, `Scoped`, `Transient` khác nhau thế nào?

**Trả lời:** Singleton tồn tại trong service provider lifetime; Scoped thường một HTTP request scope; Transient tạo khi resolve. Không giữ scoped DbContext trong singleton: DbContext không thread-safe, scope đã dispose hoặc dữ liệu chia sẻ sai.

**Thiết kế:** BackgroundService là singleton, cần `IServiceScopeFactory.CreateAsyncScope()` bên trong từng iteration/job khi cần DbContext. Không dùng scope xuyên các job song song.

**Follow-up:** `IHttpClientFactory` quản lý lifetime gì? Factory quản lý handler pooling/DNS; typed clients thường transient, không nên capture lâu dài trong singleton.

## NET-03. `CancellationToken` phải truyền tới đâu?

**Trả lời:** Truyền từ request/worker qua mọi async I/O có thể hủy: DB, HTTP và delay. Hủy request nhằm tránh lãng phí tài nguyên, nhưng **không** tự động hoàn tác thao tác đã commit hoặc external payment đã xử lý.

```csharp
using var timeout = CancellationTokenSource.CreateLinkedTokenSource(requestCt);
timeout.CancelAfter(TimeSpan.FromSeconds(3));
var response = await httpClient.GetAsync(uri, timeout.Token);
```

**Follow-up:** Client timeout sau khi lệnh payment được gửi: outcome phải xem là UNKNOWN cho đến khi reconciliation/provider status xác nhận, không retry bằng payment reference mới.

## NET-04. `record`, `class`, `struct` và value equality?

**Trả lời:** `record class` là reference type, mặc định value-based equality trên thành viên; `class` mặc định reference equality; `record struct` là value type. `with` copy nông, nên nested mutable lists có thể bị chia sẻ. Entity EF Core có identity và tracking, không tự chuyển tất cả sang record.

**Follow-up:** Tại sao “struct luôn nằm stack” sai? Struct có thể inline trong object/array hoặc tồn tại trong state machine, placement phụ thuộc context/runtime.

## NET-05. Middleware order và exception handling trong ASP.NET Core?

**Trả lời:** Middleware chạy theo thứ tự đăng ký, response đi ngược pipeline. Exception handler cần bao các middleware cần bắt lỗi; `UseAuthentication` trước `UseAuthorization`; routing, CORS, rate limiting phải theo contract từng ứng dụng.

**Follow-up:** Lỗi 401 khác 403? 401 là chưa xác thực hợp lệ, 403 là đã xác thực nhưng không đủ quyền. 403 không nên fallback trả data của tenant khác.

**Đọc sâu:** [Hosting and pipeline](../../07-aspnet-core/pipeline-hosting-and-configuration.md).

## NET-06. GraphQL N+1 giải quyết bằng DataLoader đã đủ chưa?

**Trả lời:** DataLoader batch/cache per operation giúp tránh gọi DB một lần trên mỗi parent, nhưng không tự cứu query thiếu filter, projection quá lớn hay query depth/cost quá cao. Phải giới hạn query complexity, pagination và đo SQL thật.

**Follow-up:** Nếu 1 field gọi 2 hệ thống downstream không sẵn sàng, gateway trả partial data hay fail toàn bộ? Cần thống nhất schema/API contract, error propagation, latency budget và observability.

## NET-07. `IAsyncDisposable` và resource lifetime?

**Trả lời:** Dùng `await using` khi resource cần async cleanup (flush/close I/O). Không tự gọi Dispose DbContext do DI đang quản lý. Exception trong cleanup có thể che exception gốc; đo leaking connections/sockets, tránh tạo `HttpClient` liên tục.

**Follow-up:** Factory-created DbContext khác scoped-injected DbContext về ai sở hữu Dispose?

## NET-08. API idempotency và HTTP methods?

**Trả lời:** Idempotency nghĩa là cùng một operation lặp lại có cùng business effect theo contract, không phải cùng response byte-for-byte. GET/PUT/DELETE có semantic idempotent theo HTTP; POST thanh toán có thể hỗ trợ bằng Idempotency-Key + unique DB constraint + hash payload trong transaction.

**Failure:** Lưu key vào Redis trước nhưng business SQL commit lỗi gây trạng thái mơ hồ. Phải xác định authoritative operation store và expiry/reuse semantics.

**Đọc sâu:** [HTTP contracts](../../06-api-design/http-resource-contracts-and-semantics.md).

## NET-09. Làm sao review một API production chậm?

**Trả lời theo thứ tự:** xác định workload/percentiles → trace request → tách DB/external/network/serialization → query plan/allocations/ThreadPool → thử một thay đổi → regression + load test + canary. Không đo p95 từ vài requests đơn lẻ.

**Architecture follow-up:** Nếu DB là bottleneck, tăng số API replicas có thể làm contention nặng hơn; so scale-up, index/query fix, read model, cache có freshness contract.

---

**Tiếp theo:** [SQL & EF Core](sql-data.md) · [Distributed/Azure](distributed-cloud.md) · [Mock Interview](mock-interview.md).
