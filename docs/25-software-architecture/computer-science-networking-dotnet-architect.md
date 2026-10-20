# Computer Science & Networking cho .NET Solution Architect

> [← Danh mục kiến thức](solution-architect-technical-catalog.md) · [Lộ trình](solution-architect-dotnet-roadmap.md) · [Lab bắt buộc L01](solution-architect-mandatory-labs.md#l01)

Tài liệu này đi từ nguyên lý máy tính/mạng tới cách điều tra ASP.NET Core chậm trong production. Ví dụ được thiết kế để học và kiểm chứng; các số liệu latency, concurrency hoặc lỗi bên dưới là **giả lập**, không phải số đo production của một hệ thống cụ thể.

## 1. Mục tiêu và cách học

Sau chương này, bạn phải có thể:

- Vẽ đường đi của một HTTP request: client → DNS → TCP/TLS → proxy/load balancer → Kestrel → middleware → application → connection pool → SQL Server/dependency.
- Phân biệt latency, throughput, concurrency, saturation và tail latency.
- Phân biệt CPU-bound, I/O-bound, ThreadPool starvation, connection pool exhaustion, SQL blocking và dependency timeout.
- Giải thích connection reuse, timeout budget, cancellation, retry và graceful shutdown.
- Tái hiện ít nhất hai lỗi có kiểm soát, thu thập trace/metrics, đưa ra nguyên nhân có bằng chứng và chứng minh bản sửa không tạo lỗi mới.

**Quy trình học:** hiểu khái niệm → vẽ request path → dự đoán failure → chạy thí nghiệm → đọc telemetry → so sánh trước/sau → ghi ADR ngắn.

## 2. Từ process đến thread và Task

### Process, thread, Task không phải một khái niệm

- **Process** là vùng thực thi được hệ điều hành cô lập tương đối, có không gian địa chỉ và tài nguyên riêng.
- **Thread** là luồng thực thi được scheduler lập lịch. Thread có stack và chi phí chuyển ngữ cảnh.
- **Task** trong .NET biểu diễn một thao tác bất đồng bộ có thể hoàn tất trong tương lai. Một Task không đồng nghĩa với một thread.
- **ThreadPool** tái sử dụng worker thread để tránh tạo/hủy thread cho từng công việc ngắn.
- **Async I/O** cho phép thread trả lại cho pool khi đang chờ I/O bất đồng bộ; nó không làm database hay mạng tự nhiên nhanh hơn.

### Khi nào dùng và không dùng

| Cách làm | Dùng khi | Tránh khi |
|---|---|---|
| `async/await` | Chờ HTTP, SQL, file hoặc I/O hỗ trợ async | Không biến CPU-heavy loop thành nhanh hơn |
| `Task.Run` | Đưa CPU work phù hợp ra khỏi caller trong ứng dụng có lý do rõ ràng | Không bọc mọi I/O call trong `Task.Run` ở ASP.NET Core |
| Dedicated/background worker | Công việc dài, có giới hạn concurrency và vòng đời riêng | Không tạo thread thủ công cho từng request |
| Bounded Channel/queue | Cần backpressure và giới hạn bộ nhớ | Không dùng unbounded queue cho tải không giới hạn |

### Ví dụ: I/O-bound đúng cách

```csharp
app.MapGet("/customers/{id:int}", async (
    int id,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var customer = await db.Customers
        .AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    return customer is null ? Results.NotFound() : Results.Ok(customer);
});
```

Không viết `db.Customers...SingleOrDefaultAsync(...).Result` hoặc `.Wait()` trong request path. Blocking thread trong lúc chờ I/O có thể làm giảm số request mà process phục vụ được và góp phần gây ThreadPool starvation khi tải cao.

**Lưu ý:** `async` không sửa được query chậm, lock lâu, thiếu index hoặc dependency không có timeout. Async làm thread được sử dụng hiệu quả hơn trong lúc chờ; bottleneck vẫn phải được đo.

## 3. CPU-bound, I/O-bound và ThreadPool starvation

- **CPU-bound:** công việc bị giới hạn bởi thời gian CPU, ví dụ tính toán mã hóa hoặc xử lý ảnh. Tối ưu thuật toán, allocation và concurrency trước khi tăng thread.
- **I/O-bound:** công việc chờ database, HTTP, filesystem hoặc broker. Dùng async API, timeout và cancellation; điều tra dependency latency.
- **ThreadPool starvation:** công việc cần worker nhưng các worker hiện hữu bị block hoặc không theo kịp tốc độ đến. Request queue và latency có thể tăng đột biến.
- **Lock contention:** nhiều thread tranh cùng lock; thêm thread có thể làm contention nặng hơn.
- **Deadlock:** các tác vụ giữ tài nguyên và chờ lẫn nhau theo chu trình. Starvation và deadlock là hai vấn đề khác nhau.

### Dấu hiệu và cách phân biệt

| Dấu hiệu | Giả thuyết cần kiểm tra | Bằng chứng tiếp theo |
|---|---|---|
| CPU cao liên tục | CPU-bound hoặc retry storm | CPU profile, call tree, request rate |
| CPU thấp nhưng latency cao | I/O wait, lock, queue hoặc dependency chậm | Distributed trace, SQL waits, ThreadPool counters |
| ThreadPool thread/queue tăng cùng latency | Blocking/starvation có thể xảy ra | dotnet-counters, dump/trace, tìm sync-over-async |
| SQL duration cao | Query, blocking, network hoặc pool wait | Query Store/actual plan, wait stats, connection pool telemetry |
| HTTP dependency duration cao | Provider, DNS, TLS, network hoặc timeout policy | Span HTTP, DNS/connection metrics, provider status |

Không kết luận ThreadPool starvation chỉ từ một biểu đồ. Phải đối chiếu request rate, ThreadPool queue/thread count, stack trace và latency của dependency.

## 4. Latency, throughput, concurrency và Little's Law

- **Latency:** thời gian một request hoàn thành.
- **Throughput:** số request/operation hoàn thành trong một đơn vị thời gian.
- **Concurrency:** số operation đang diễn ra đồng thời.
- **Saturation:** mức tài nguyên đang tiến gần giới hạn hữu dụng.
- **p50/p95/p99:** phân vị latency; p99 là ngưỡng mà 99% mẫu không vượt qua trong tập đo, không phải latency tối đa tuyệt đối.

Little's Law ở trạng thái ổn định:

`L = λ × W`

Trong đó `L` là số request trung bình đang ở trong hệ thống, `λ` là throughput trung bình (request/giây), và `W` là thời gian trung bình trong hệ thống (giây).

Ví dụ giả lập: 200 request/giây, thời gian trung bình 0,25 giây ⇒ trung bình khoảng 50 request đang xử lý/đợi. Nếu latency tăng lên 2 giây trong khi throughput vẫn 200 request/giây, số request đang tồn tại có thể tăng lên khoảng 400. Queue dài có thể làm latency tăng thêm, tạo vòng lặp xấu.

**Không dùng Little's Law để dự báo chính xác hệ thống chưa ổn định hoặc tải thay đổi nhanh.** Nó là công cụ suy luận và kiểm tra tính nhất quán, không thay load test/capacity model.

### Vì sao phải theo dõi p95/p99?

Average có thể che khuất một nhóm request cực chậm do lock, cache miss, retry hoặc một dependency. Đặt mục tiêu dựa trên SLO và phân bố latency; so sánh cùng workload, thời gian warm-up và môi trường.

## 5. TCP, DNS, TLS và HTTP: request thực sự đi qua đâu?

Một request HTTPS thường bao gồm một số bước sau (thứ tự chính xác phụ thuộc cache, connection reuse, proxy và giao thức):

1. Resolve hostname qua DNS hoặc cache DNS.
2. Thiết lập TCP connection, trừ khi đã có connection phù hợp được tái sử dụng.
3. Thực hiện TLS handshake khi tạo kết nối TLS mới.
4. Gửi HTTP request qua connection.
5. Proxy/load balancer chuyển tiếp request tới ứng dụng.
6. Kestrel và middleware xử lý request; application gọi SQL/dependency.
7. Response quay lại client.

Không phải request nào cũng trả chi phí DNS + TCP + TLS mới. Connection pooling, keep-alive, HTTP/2 multiplexing và cache có thể giảm overhead. Vì vậy hãy đo riêng **time to first byte**, connection setup và server processing khi có telemetry phù hợp.

### DNS

DNS biến hostname thành địa chỉ IP; kết quả có thể được cache ở nhiều lớp. Khi endpoint đổi IP, cache và connection đang mở có thể khiến chuyển đổi không tức thời.

**Lỗi thường gặp:** đổi DNS rồi cho rằng mọi process sẽ lập tức dùng endpoint mới. Cần biết DNS TTL, resolver behavior, connection lifetime và khả năng retry.

### TCP

TCP cung cấp byte stream có thứ tự và retransmission, nhưng không cung cấp ranh giới message ở tầng ứng dụng. Packet loss, congestion và retransmission có thể làm tail latency tăng. Kết nối TCP thành công không có nghĩa ứng dụng đích khỏe.

### TLS

TLS bảo vệ tính bí mật/toàn vẹn và xác thực peer dựa trên certificate/trust chain. Lỗi production có thể do certificate hết hạn, hostname không khớp, trust store, proxy TLS inspection hoặc cấu hình protocol/cipher. Không tắt certificate validation để “sửa nhanh”.

### HTTP/1.1, HTTP/2, HTTP/3

- **HTTP/1.1:** thường dựa vào nhiều connection song song để đạt concurrency; connection reuse vẫn quan trọng.
- **HTTP/2:** multiplex nhiều stream trên một TCP connection; mất packet ở TCP có thể ảnh hưởng nhiều stream trên cùng connection.
- **HTTP/3:** chạy HTTP trên QUIC/UDP, giảm một số vấn đề head-of-line blocking ở transport giữa các stream, nhưng có yêu cầu hạ tầng/network khác và không tự động tốt hơn ở mọi môi trường.

Chọn giao thức theo client, proxy, observability, compatibility và số đo. Không đổi sang HTTP/3 chỉ vì phiên bản mới hơn.

## 6. HttpClient, connection pooling và timeout budget

Trong .NET, `IHttpClientFactory` giúp quản lý cấu hình và vòng đời handler, đồng thời hỗ trợ logging/resilience integration. Tránh tạo và dispose một `HttpClient` mới cho mỗi request như một quy tắc chung, vì quản lý connection sai có thể gây socket/resource pressure; cũng tránh giữ handler vô hạn khi DNS/endpoint cần cập nhật.

```csharp
builder.Services.AddHttpClient<PaymentProviderClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["PaymentProvider:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(3);
});
```

Đây chỉ là ví dụ cấu hình timeout. Trong hệ thống thật, timeout phải xuất phát từ end-to-end budget, không chọn số tùy ý.

### Timeout, cancellation và retry

- **Timeout:** giới hạn thời gian chờ của một thao tác.
- **CancellationToken:** truyền yêu cầu hủy xuống các API hỗ trợ hủy; không bảo đảm external side effect đã bị hủy.
- **Retry:** có thể giúp vượt lỗi tạm thời, nhưng làm tăng tải và có thể lặp side effect.
- **Circuit breaker:** tạm ngừng gọi dependency có dấu hiệu lỗi để tránh tiếp tục gây tải; không phải cơ chế sửa dữ liệu.
- **Bulkhead:** giới hạn tài nguyên/concurrency cho một dependency để tránh chiếm hết tài nguyên của hệ thống.

Nếu API có tổng budget 2 giây mà gọi ba dependency tuần tự, mỗi dependency có timeout 2 giây, request có thể vượt budget nhiều lần. Phải chia timeout budget, tính overhead và truyền cancellation xuyên suốt.

**Không retry mù** với thao tác charge payment, submit order hoặc issue voucher nếu không có idempotency/reconciliation. Timeout chỉ cho biết caller không nhận được kết quả kịp thời; nó không chứng minh operation chưa xảy ra.

## 7. SQL connection pool không phải ThreadPool

SQL client duy trì pool các kết nối vật lý để tái sử dụng. Khi toàn bộ connection trong pool đang bận, request khác có thể phải chờ dù ThreadPool vẫn còn worker.

Các nguyên nhân thường gặp:

- Giữ connection/transaction lâu trong lúc gọi HTTP provider.
- Quên dispose resource đúng cách.
- Query chậm hoặc blocking khiến connection không được trả về sớm.
- Tạo quá nhiều pool do connection string khác nhau.
- Đặt concurrency cao hơn khả năng xử lý của database.

**Nguyên tắc:** không giữ transaction SQL mở trong khi chờ external HTTP/broker nếu không thực sự cần và đã đánh giá hệ quả. Thiết kế transaction ngắn, đặt timeout, điều tra pool wait và SQL waits riêng biệt.

## 8. Reverse proxy, load balancer, NAT và health checks

- **Reverse proxy:** nhận request và chuyển tiếp tới upstream; có thể kết thúc TLS, sửa header và áp dụng giới hạn.
- **Load balancer:** phân phối traffic theo thuật toán/health state; không thay thế thiết kế stateless hoặc xử lý consistency.
- **NAT/SNAT:** chuyển đổi địa chỉ/port. Hệ thống tạo quá nhiều outbound connection có thể cạn port khả dụng hoặc gặp giới hạn nền tảng.
- **Liveness:** tiến trình có còn hoạt động không.
- **Readiness:** instance có sẵn sàng nhận traffic không.
- **Startup probe:** cho phép ứng dụng khởi động lâu mà không bị liveness đánh dấu lỗi quá sớm.

Không đưa dependency ngoài vào liveness một cách máy móc: nếu SQL tạm mất và tất cả instance đều restart liên tục, hệ thống có thể tự khuếch đại sự cố. Readiness có thể phản ánh khả năng phục vụ, nhưng cần tránh cascading removal. Thiết kế probe theo hành vi phục hồi thực tế của ứng dụng và nền tảng.

## 9. Backpressure và giới hạn concurrency

Nếu request đến nhanh hơn khả năng xử lý, queue sẽ tăng. Tăng concurrency không phải lúc nào cũng tăng throughput: nó có thể gây contention, tăng memory, tăng connection pool pressure và làm p99 xấu hơn.

Các biện pháp:

- Bounded queue thay vì unbounded in-memory queue.
- Giới hạn concurrency theo dependency.
- Admission control/rate limit khi quá tải.
- Timeout budget và load shedding cho công việc không thiết yếu.
- Durable broker cho công việc cần sống sót qua process restart.
- Retry có backoff/jitter và giới hạn số lần; DLQ/reconciliation cho message không thể xử lý.

**Trade-off:** từ chối sớm một phần tải có thể giữ SLO cho traffic quan trọng hơn là nhận mọi request rồi để tất cả timeout.

## 10. Bài lab L01 — điều tra request chậm

### Thiết kế thí nghiệm

Tạo ASP.NET Core API với ba endpoint tách biệt:

1. `/io`: gọi một dependency I/O giả lập có delay và cancellation.
2. `/cpu`: thực hiện CPU work có giới hạn để quan sát CPU saturation.
3. `/blocking`: chỉ trong môi trường lab, cố ý block bằng sync-over-async để so sánh; không deploy endpoint này lên môi trường công khai.

Sau đó chạy tải tăng dần và so sánh:

- Request rate, throughput, error rate, p50/p95/p99.
- CPU và memory.
- ThreadPool thread count/queue.
- HTTP connection reuse và dependency duration.
- SQL query duration/pool wait nếu kết nối SQL Server.

Dùng `dotnet-counters`, OpenTelemetry traces và công cụ load test như k6 hoặc NBomber. Không cần giả vờ rằng DNS/TLS lỗi thật được tái hiện chỉ bằng delay trong code; nếu chưa dựng được proxy/network fault, hãy ghi rõ đó là simulated latency, không phải DNS/TLS failure.

### Tiêu chí pass/fail

- [ ] Có lệnh chạy và môi trường tái lập.
- [ ] Có baseline trước khi gây lỗi.
- [ ] Tái hiện tối thiểu một vấn đề blocking/starvation và một dependency latency/timeout.
- [ ] Có metrics + trace hoặc stack evidence hỗ trợ kết luận.
- [ ] Thay đổi một biến mỗi lần; so sánh cùng workload trước/sau.
- [ ] Nêu được giới hạn thí nghiệm và không tuyên bố kết quả giả lập là production benchmark.

## 11. Lỗi production thường gặp và hướng xử lý

| Triệu chứng | Đừng vội kết luận | Điều tra |
|---|---|---|
| API p99 tăng, CPU thấp | “Cần thêm CPU” | Trace, SQL waits, pool waits, dependency latency |
| ThreadPool tăng mạnh | “Tăng MinThreads là fix” | Tìm blocking, sync-over-async, lock contention |
| Socket/connection lỗi sau deploy | “Provider đang down” | Handler lifetime, DNS change, SNAT, connection reuse |
| Request timeout nhưng payment/order có thể đã chạy | “Request fail nên retry được” | Idempotency status, provider query, reconciliation |
| Restart liên tục khi database lỗi | “Liveness phải check tất cả dependency” | Probe semantics, restart loop, readiness policy |
| Queue/memory tăng không giới hạn | “Thêm instance là đủ” | Arrival vs service rate, bounded queue, backpressure |

## 12. Trade-off và câu hỏi phỏng vấn

1. **Async có làm request nhanh hơn không?** Không nhất thiết; nó thường giải phóng thread trong lúc chờ I/O, nhưng không loại bỏ latency của dependency.
2. **Task có phải thread không?** Không. Task biểu diễn công việc/kết quả bất đồng bộ; continuation có thể chạy trên thread khác.
3. **ThreadPool starvation khác deadlock thế nào?** Starvation là thiếu worker khả dụng để tiến triển; deadlock là chu trình chờ tài nguyên/tác vụ.
4. **Vì sao CPU thấp nhưng API vẫn chậm?** Có thể đang chờ I/O, lock, queue hoặc connection pool.
5. **Vì sao HTTP timeout không chứng minh payment thất bại?** Server có thể đã xử lý nhưng response bị trễ/mất; cần idempotency và reconciliation.
6. **HTTP/2 luôn tốt hơn HTTP/1.1?** Không; phụ thuộc proxy/client/workload, connection behavior và vấn đề multiplexing trên TCP.
7. **Tăng concurrency có luôn tăng throughput?** Không; khi saturated, contention và queueing có thể làm latency xấu hơn.
8. **Khi nào không retry?** Khi operation có side effect và chưa có idempotency/khả năng xác định trạng thái.
9. **Vì sao không gọi external API trong SQL transaction dài?** Giữ lock/connection lâu, làm tăng contention; SQL rollback không hoàn tác external side effect.
10. **Làm sao chứng minh nguyên nhân?** Kết hợp workload tái lập, trace/metrics/profile, thay một biến mỗi lần và đo lại.

## 13. Liên kết học tiếp

- [Danh mục 16 nhóm kiến thức](solution-architect-technical-catalog.md)
- [Bài thực hành bắt buộc](solution-architect-mandatory-labs.md)
- [System Design — NFR và capacity estimation](../24-system-design/requirements-nfr-and-capacity-estimation.md)
- [.NET Runtime](../03-dotnet/README.md)
- [Networking](../02-linux-git-networking/README.md)
- [SQL Server](../05-sql/README.md)
- [Distributed Systems](../17-distributed-systems/README.md)
