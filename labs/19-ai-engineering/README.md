# Module 19 Runnable Lab — AI Engineering without Magic

Lab này biến các mental model của Module 19 thành **executable evidence** mà không cần API key, quota hay network.

Nó cố ý dùng một `DeterministicModel` thay vì provider thật để bạn quan sát rõ:

```text
Application / Harness
├─ messages + system instruction
├─ authorized context
├─ model request
├─ structured output
├─ tool request
├─ application AuthZ
├─ bounded tool loop
├─ usage telemetry
└─ eval gate
```

Sau khi hiểu loop này, bạn có thể thay `IAiModel` bằng OpenAI/Azure OpenAI/`IChatClient` adapter mà không đổi business boundary.

---

## Prerequisite

- .NET 10 SDK.

Check:

```bash
dotnet --version
```

---

## Run

Từ repository root:

```bash
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -- demo
```

Expected shape:

```text
== Authorized RAG-style answer ==
Refund period is 30 days.
...

== Read-only tool call ==
Order 100 status: Shipped.
...

== Structured output + deterministic validation ==
{
  "Level": "HIGH",
  ...
}
```

---

# 1. Model != Agent

Lab expose một interface rất nhỏ:

```csharp
public interface IAiModel
{
    Task<ModelResponse> CompleteAsync(
        ModelRequest request,
        CancellationToken cancellationToken);
}
```

Model nhận messages và trả một trong các dạng:

```text
text
structured JSON
tool request
```

Nó **không** trực tiếp:

```text
query database
read another tenant
execute arbitrary action
control loop forever
```

Các capability đó nằm ở application/harness.

---

# 2. Context được chọn trước khi model nhìn thấy

`KnowledgeRetriever` filter theo tenant trước:

```text
UserContext.TenantId
      ↓
filter documents
      ↓
rank/select
      ↓
ContextBuilder budget
      ↓
model
```

Không làm:

```text
retrieve all tenants
→ model sees everything
→ filter answer later
```

Lab còn có context budget để chứng minh:

```text
context window != database
more context != automatically better
```

Run failure drill:

```bash
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -- failure-noisy-context
```

Một retrieved document chứa prompt-injection text. Expected:

```text
malicious text stays data
no tool authority is granted
context budget is respected
```

---

# 3. Tool request != tool execution

Khi model muốn đọc order:

```text
Model
→ ToolRequest(get_order_status, orderId=100)
→ ToolHost
→ allowlist
→ tenant authorization
→ execute
→ tool result
→ model
```

Tool host mới là nơi enforce authority.

Run:

```bash
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -- failure-unauthorized-tool
```

Expected:

```text
EXPECTED DENY: Tenant 'tenant-b' cannot read order '100'.
```

Điểm cần nhớ:

```text
system prompt says "do not leak data"
!=
application authorization
```

---

# 4. Structured output != business correctness

`ClassifyRiskAsync` yêu cầu structured JSON rồi deserialize thành:

```csharp
public sealed record RiskDecision(
    string Level,
    string Reason,
    bool RequiresHumanReview);
```

Sau đó application vẫn validate allowed domain values.

Mental model:

```text
Model output
→ schema/type parsing
→ deterministic business validation
→ business flow
```

Không parse prose kiểu:

```text
"Risk: HIGH"
```

bằng `Split(':')` rồi coi đó là contract.

---

# 5. Reasoning effort phải đi qua eval

Run:

```bash
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -- eval
```

Lab cố ý làm một case khó fail ở `Low` nhưng pass ở `High`.

Bạn sẽ thấy cùng lúc:

```text
quality score
input usage
output usage
reasoning usage
```

Đây là mental model cần giữ khi dùng provider thật:

```text
representative eval set
→ compare configs
→ quality
→ latency
→ cost
→ release decision
```

Các token numbers trong fake model chỉ là **deterministic simulation để học telemetry shape**. Không coi `characters / 4` là tokenizer production. Với provider thật, dùng usage do provider/SDK report.

---

# 6. Agent/tool loop phải bounded

Run:

```bash
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -- failure-runaway-loop
```

Fake model cố request cùng tool mãi.

Harness chặn bằng:

```text
maxToolCalls = 2
```

Expected:

```text
EXPECTED STOP: Tool loop exceeded maxToolCalls=2.
```

Production agent nên có nhiều budget hơn một counter đơn:

```text
max turns
max tool calls
time budget
cost budget
permission boundary
human approval where required
cancellation
```

---

# 7. Self-test — executable evidence

Run toàn bộ deterministic acceptance checks:

```bash
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -- self-test
```

Self-test kiểm:

```text
[ ] tenant-aware retrieval
[ ] context budget
[ ] read-only tool execution
[ ] unauthorized tool denial
[ ] structured output validation
[ ] prompt-injection text does not gain authority
[ ] runaway tool loop is stopped
[ ] high-effort eval beats low-effort eval on the hard case
[ ] usage trade-off is observable
```

Expected final line:

```text
SELF-TEST PASS
```

---

# 8. Files

```text
labs/19-ai-engineering/
├── AiEngineeringLab.csproj
├── Program.cs
├── LabRuntime.cs
└── README.md
```

`Program.cs` chỉ là CLI entry point.

`LabRuntime.cs` chứa intentionally-small abstractions để flow đọc được trong một file:

```text
IAiModel
DeterministicModel
KnowledgeRetriever
ContextBuilder
ToolHost
KnowledgeAssistant
EvalRunner
LabCli
```

Không thêm framework trước khi abstraction thật sự cần.

---

# 9. Thay fake model bằng provider thật

Extension exercise:

```text
IAiModel
  ↓
OpenAI/Azure OpenAI/Microsoft.Extensions.AI adapter
```

Giữ nguyên các boundary khác:

```text
KnowledgeRetriever
ToolHost/AuthZ
RiskDecision validation
EvalRunner
failure drills
```

Provider adapter phải map:

```text
AiMessage
→ provider message format

ToolRequest
← provider tool/function request

AiUsage
← provider-reported usage
```

Không để provider SDK lan vào domain/application code chỉ vì demo nhanh hơn.

---

# 10. Bài tập mở rộng

## A — real provider adapter

Thêm một adapter thật và giữ `DeterministicModel` cho CI/self-test.

## B — persistence

Lưu:

```text
prompt/config version
request id
provider/model
usage
latency
result status
```

không lưu raw sensitive context mặc định.

## C — eval regression

Thêm candidate config rồi fail process nếu score thấp hơn threshold.

## D — write tool

Thêm một low-risk write tool và bắt buộc:

```text
AuthZ
idempotency key
approval policy
unknown-outcome handling
```

## E — real retrieval

Thay in-memory documents bằng SQL/Search/vector backend nhưng giữ rule:

```text
ACL before model
```

---

# Definition of Done

Lab hoàn thành khi bạn có thể giải thích bằng code:

```text
Model != Agent
Tool request != execution
System instruction != authorization
Context quality > context quantity
Structured output != business correctness
Reasoning config requires eval
Agent loop requires budgets
AI failures still follow distributed-systems rules
```

Và chạy được:

```bash
dotnet build labs/19-ai-engineering/AiEngineeringLab.csproj -c Release
dotnet run --project labs/19-ai-engineering/AiEngineeringLab.csproj -c Release --no-build -- self-test
```
