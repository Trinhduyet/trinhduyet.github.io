# Module 01 — Computer Science Essentials cho Backend và System Design

> [Roadmap](../00-roadmap/README.md) · [Module 02](../02-linux-git-networking/README.md)

Module này xây nền để giải thích vì sao một hệ thống nhanh, chậm, sai hoặc không ổn định. Phạm vi cố ý chọn lọc: chỉ học Computer Science có tác động trực tiếp đến backend, .NET, production troubleshooting và system design.

## Module trong một hình

![Sơ đồ Readme — diagram 1](../assets/diagrams/01-computer-science-readme-1.svg)

## Phạm vi

| Learning slice | Priority | Evidence |
| --- | --- | --- |
| [Complexity và workload reasoning](complexity-and-workload-reasoning.md) | P1 | workload experiment |
| [Data structures cho backend](data-structures-for-backend-systems.md) | P1/P2 | structure choice + trade-off |
| [Process, thread, scheduling và concurrency](process-thread-scheduling-and-concurrency.md) | P0 | race/concurrency experiment |
| [Memory, virtual memory, GC và cache](memory-stack-heap-virtual-memory-and-cache.md) | P0 | memory/runtime evidence |
| [WorkloadLab source](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/01-computer-science/workload-lab) | P0/P1 | build + run report |

## Dependency map

![Sơ đồ Readme — diagram 2](../assets/diagrams/01-computer-science-readme-2.svg)

## Bốn mental model phải giữ lại

### 1. Complexity là mô hình tăng trưởng, không phải đồng hồ

`O(1)` không bảo đảm nhanh; `O(n)` không mặc định chậm. Phải định nghĩa `n`, operation, distribution và constraint trước. Wall time còn chịu constant factor, cache locality, allocation, vectorization, I/O, contention và runtime state.

### 2. Data structure là tập hợp invariant và cost

Không chọn `Dictionary` chỉ vì “O(1)”. Hãy hỏi cần lookup, order, duplicate, priority, range query, boundedness hay concurrent access. Mỗi invariant đổi lấy memory, maintenance cost hoặc semantics ở operation khác.

### 3. Task không phải thread

Process là isolation/resource boundary; thread là execution context được OS schedule; `Task` biểu diễn một asynchronous operation. `async` giúp không giữ thread khi chờ I/O, nhưng không làm CPU work miễn phí và không tự làm shared state an toàn.

### 4. Allocated, resident và retained là ba câu hỏi khác nhau

Virtual address space không phải RAM. Managed heap không phải toàn bộ process memory. GC có thể thu hồi object nhưng process working set chưa giảm ngay. Memory diagnosis phải nối allocation rate, retained heap, RSS, page faults, native memory và container limit.

## Cách chạy lab — portable path

Không dùng absolute path theo máy cá nhân. Clone repo một lần:

```powershell
git clone https://github.com/Trinhduyet/trinhduyet.github.io.git
cd trinhduyet.github.io
```

Từ **repository root**:

```powershell
cd labs/01-computer-science/workload-lab
dotnet build -c Release

dotnet run -c Release --no-build -- lookup
dotnet run -c Release --no-build -- race
dotnet run -c Release --no-build -- locality
```

Path trên dùng forward slash nên chạy được trong PowerShell, Bash và các shell phổ biến khi current directory là repository root.

Ba experiment có seed cố định và hard limit để tránh vô tình tạo tải quá lớn. `Stopwatch` phù hợp để quan sát định hướng trong lab, nhưng một lần chạy không phải benchmark có thể công bố.

![Sơ đồ Readme — diagram 3](../assets/diagrams/01-computer-science-readme-3.svg)

## Evidence tối thiểu

1. Runtime, OS, architecture và logical processor count.
2. Dự đoán trước mỗi experiment.
3. Output với ít nhất hai workload sizes.
4. Giải thích vì sao cùng Big-O vẫn có thể khác wall time.
5. Một failure observation: lost update, memory limit rejection hoặc safety bound.
6. Một quyết định production và điều kiện xem xét lại.

## Exit criteria

Người học hoàn thành Module 01 khi có thể:

- định nghĩa input size và cost model cho backend operation cụ thể;
- chọn data structure theo dominant operations và invariant;
- phân biệt process, thread, `Task`, concurrency và parallelism;
- tái hiện race condition rồi sửa bằng primitive phù hợp;
- giải thích virtual memory, RSS, managed heap, GC và cache locality;
- đo workload có context và không kết luận từ một sample;
- nối quyết định local với performance, reliability, security và cost.

## Tiếp tục

Học tiếp [Module 02 — Linux, Git và Networking](../02-linux-git-networking/README.md), sau đó [Module 03 — .NET Runtime](../03-dotnet/README.md).
