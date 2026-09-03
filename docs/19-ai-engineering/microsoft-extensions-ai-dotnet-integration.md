# Microsoft.Extensions.AI — .NET Integration Guide

> Mục tiêu: hiểu `Microsoft.Extensions.AI` như **application integration layer** của .NET cho generative AI — không phải model, không phải provider, không phải agent framework.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0 cho .NET AI Engineer</span>
  <span><strong>Baseline</strong>&nbsp;Microsoft.Extensions.AI 10.9.0</span>
  <span><strong>Mode</strong>&nbsp;mental model → API → pipeline → provider → failure → production</span>
</div>

## Hiểu trong 5 phút

Trong một .NET AI application, có thể tách các lớp như sau:

```text
Business capability
        ↓
Application service
        ↓
Microsoft.Extensions.AI abstractions
├─ IChatClient
├─ IEmbeddingGenerator<TInput,TEmbedding>
├─ ChatMessage / ChatOptions / ChatResponse
├─ AIFunction / tools
└─ middleware-style client pipeline
        ↓
Provider adapter
├─ OpenAI
├─ Azure OpenAI
├─ local/OpenAI-compatible provider
└─ other implementation
        ↓
Model / inference service
```

Key idea:

```text
Microsoft.Extensions.AI
!= OpenAI SDK
!= Azure OpenAI
!= Agent Framework
!= your business abstraction
```

Nó cung cấp **common contracts + composable pipeline** để application code bớt phụ thuộc provider-specific API.

---

# 1. Vì sao cần Microsoft.Extensions.AI?

Nếu application gọi provider SDK trực tiếp ở mọi service:

```text
OrderService → OpenAI SDK
SupportService → OpenAI SDK
KnowledgeService → OpenAI SDK
RiskService → OpenAI SDK
```

thì provider details lan vào application layer:

```text
provider request types
provider message types
provider response types
provider tool types
provider telemetry
provider-specific options
```

Khi đổi provider hoặc viết test, blast radius lớn.

`Microsoft.Extensions.AI` đưa vào một abstraction line:

```text
Application
    ↓
IChatClient
    ↓
provider adapter
```

Nhưng vẫn phải nhớ:

> `IChatClient` là **technical port**, không phải business port.

Business layer vẫn nên phụ thuộc capability-specific interface:

```csharp
public interface IOrderRiskClassifier
{
    Task<RiskDecision> ClassifyAsync(
        OrderRiskInput input,
        CancellationToken cancellationToken);
}
```

Implementation mới phụ thuộc `IChatClient`:

```text
Order domain/application
→ IOrderRiskClassifier
→ AiOrderRiskClassifier
→ IChatClient
→ Provider
```

---

# 2. Package layering

Microsoft tách package theo trách nhiệm.

## `Microsoft.Extensions.AI.Abstractions`

Core exchange contracts:

```text
IChatClient
IEmbeddingGenerator<TInput,TEmbedding>
ChatMessage
ChatOptions
ChatResponse
AIContent
```

Phù hợp cho **library/provider implementation** muốn expose common abstraction mà không kéo toàn bộ higher-level utilities.

## `Microsoft.Extensions.AI`

Application package, phụ thuộc `Abstractions`, bổ sung higher-level building blocks như:

```text
ChatClientBuilder
function invocation pipeline
structured output helpers
telemetry/OpenTelemetry integration
caching/decorators
AIFunction helpers
```

Rule thực tế:

```text
application/service
→ reference Microsoft.Extensions.AI

provider adapter/library
→ có thể chỉ cần Microsoft.Extensions.AI.Abstractions
```

Repo pin runnable lab ở `10.9.0`; production phải kiểm [Technology Baseline](../00-roadmap/technology-baseline.md) trước upgrade.

---

# 3. `IChatClient` là gì?

Mental model:

```text
IEnumerable<ChatMessage>
+ ChatOptions
+ CancellationToken
        ↓
IChatClient
        ↓
ChatResponse
```

Core contract:

```csharp
Task<ChatResponse> GetResponseAsync(
    IEnumerable<ChatMessage> messages,
    ChatOptions? options = null,
    CancellationToken cancellationToken = default);
```

và streaming:

```text
messages
→ IChatClient
→ IAsyncEnumerable<ChatResponseUpdate>
```

`IChatClient` không đảm bảo:

```text
AuthZ
prompt-injection defense
context-size policy
business validation
idempotency
human approval
```

Những boundary đó vẫn thuộc application.

---

# 4. `ChatMessage` và message roles

Một request thường là sequence:

```text
System / instruction
User
Assistant
Tool result
User
...
```

Ví dụ:

```csharp
using Microsoft.Extensions.AI;

ChatMessage[] messages =
[
    new(ChatRole.System,
        "You are a support assistant. Prefer current authorized context."),
    new(ChatRole.User,
        "What is order 100 status?")
];
```

Mental model:

```text
message history
!= server-side memory guarantee
```

Application phải quyết định history nào được gửi lại cho provider request tiếp theo.

Không nên append toàn bộ history vô hạn.

---

# 5. `ChatOptions` — request policy, không phải business policy

`ChatOptions` mang các provider-agnostic request hints/options như:

```text
ModelId
MaxOutputTokens
Temperature
StopSequences
Tools
ToolMode
ResponseFormat
Reasoning
Instructions
```

Ví dụ:

```csharp
ChatOptions options = new()
{
    MaxOutputTokens = 500,
    Temperature = 0.2f
};
```

Nhưng provider/model support khác nhau.

Rule:

```text
ChatOptions expresses intent
provider adapter maps best effort
provider/model capability decides actual support
```

Không assume mọi option hoạt động giống nhau trên mọi model.

---

# 6. Reasoning options

MEAI hiện có provider-agnostic `ReasoningOptions` và `ReasoningEffort`.

Conceptually:

```csharp
ChatOptions options = new()
{
    Reasoning = new ReasoningOptions
    {
        Effort = Microsoft.Extensions.AI.ReasoningEffort.Medium
    }
};
```

Các mức abstraction hiện gồm:

```text
None
Low
Medium
High
ExtraHigh
```

Nhưng đây là **request abstraction**, không phải guarantee provider support.

Correct production process:

```text
representative eval
→ compare effort
→ correctness
→ latency
→ usage/cost
→ select config
```

Không default `High/ExtraHigh` cho mọi request.

---

# 7. `ChatResponse` và usage

Response không chỉ có text.

Conceptually:

```text
ChatResponse
├─ Messages / Contents
├─ Text convenience view
├─ UsageDetails
├─ response metadata
└─ provider raw representation when available
```

`UsageDetails` có thể chứa:

```text
InputTokenCount
OutputTokenCount
ReasoningTokenCount
CachedInputTokenCount
TotalTokenCount
additional provider counts
```

Telemetry nên lưu metadata phù hợp policy:

```text
provider
model
request id
input tokens
output tokens
reasoning tokens
latency
tool count
status
```

Không mặc định log raw context/prompt.

---

# 8. Provider adapter — OpenAI

Provider construction ở composition root, không nằm trong domain service.

```csharp
using Microsoft.Extensions.AI;
using OpenAI;

string apiKey = configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("Missing OpenAI key");

string model = configuration["OpenAI:Model"]
    ?? throw new InvalidOperationException("Missing model");

IChatClient providerClient =
    new OpenAIClient(apiKey)
        .GetChatClient(model)
        .AsIChatClient();
```

Application service chỉ nhận:

```csharp
public sealed class ArchitectureTutor(IChatClient chatClient)
{
    // ...
}
```

Không truyền API key/model client xuống domain.

---

# 9. Provider adapter — Azure OpenAI

Azure mapping vẫn đi qua `IChatClient`.

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

AzureOpenAIClient azureClient = new(
    new Uri(endpoint),
    new DefaultAzureCredential());

IChatClient chatClient = azureClient
    .GetChatClient(deploymentName)
    .AsIChatClient();
```

Production ưu tiên workload identity / Microsoft Entra authentication khi architecture cho phép thay vì hard-code key.

Boundary:

```text
Application
→ IChatClient
→ AzureOpenAIClient / OpenAI ChatClient
→ Azure OpenAI deployment
```

Provider deployment name/model availability vẫn là Azure configuration concern.

---

# 10. `ChatClientBuilder` — middleware mindset

Một điểm mạnh của MEAI là decorate client bằng pipeline quen thuộc với .NET.

```text
Application
    ↓
Telemetry decorator
    ↓
Caching / custom decorator
    ↓
Function invocation
    ↓
Provider IChatClient
```

Ví dụ:

```csharp
IChatClient client = new ChatClientBuilder(providerClient)
    .UseFunctionInvocation()
    .Build();
```

Mental model giống HTTP middleware/decorator:

```text
outer behavior
→ inner client
→ provider
→ response flows back outward
```

Thứ tự decorator có thể ảnh hưởng behavior/telemetry/caching, nên treat pipeline order như architecture decision.

---

# 11. Structured output với typed contract

Không parse prose bằng `Split`, regex tùy tiện hoặc string convention.

```csharp
public sealed record RiskDecision(
    string Level,
    string Reason,
    bool RequiresHumanReview);
```

MEAI có typed response helpers:

```csharp
ChatResponse<RiskDecision> response =
    await chatClient.GetResponseAsync<RiskDecision>(
        "Classify this transaction risk.",
        useJsonSchemaResponseFormat: true,
        cancellationToken: cancellationToken);

RiskDecision decision = response.Result;
```

Tuy nhiên:

```text
valid JSON
→ valid schema/type
→ still may violate business rule
```

Do đó:

```text
model
→ structured response
→ deterministic validation
→ domain action
```

Nếu parse/deserialization có thể thất bại, dùng API `TryGetResult`/error handling phù hợp thay vì assume luôn valid.

→ [Structured Output & Tool Calling](structured-output-and-tool-calling.md)

---

# 12. `AIFunction` và tool calling

MEAI mô hình hóa function/tool bằng `AIFunction`.

```csharp
AIFunction getOrderStatus = AIFunctionFactory.Create(
    (string orderId) => orderId == "100" ? "Shipped" : "NotFound",
    name: "get_order_status",
    description: "Read the status of an order the caller is authorized to view.");
```

Attach vào request:

```csharp
ChatOptions options = new()
{
    Tools = [getOrderStatus]
};
```

Core semantics:

```text
model
→ FunctionCallContent
→ application/client pipeline
→ invoke AIFunction
→ FunctionResultContent
→ model again
→ final answer
```

`FunctionInvokingChatClient`/`UseFunctionInvocation()` có thể automate loop này.

Nhưng **automatic invocation không đồng nghĩa automatic authorization**.

Tool implementation vẫn phải enforce:

```text
identity
resource ownership
permission
input validation
side-effect policy
idempotency
approval
```

---

# 13. Tool loop phải bounded

Function invocation loop không được chạy vô hạn.

MEAI function invocation pipeline có stop conditions/iteration controls; application còn phải có business-level budgets.

Think in two layers:

```text
MEAI invocation iteration guard
+
application agent/workflow budget
```

Budget có thể gồm:

```text
max model round-trips
max tool calls
max wall-clock time
max cost
max write tools
approval requirement
```

Nếu tool có external side effect:

```text
timeout
!= definitely failed
```

vẫn cần idempotency/reconciliation.

---

# 14. Streaming

`IChatClient` hỗ trợ streaming qua `GetStreamingResponseAsync`.

Use case:

```text
provider starts generating
→ chunks arrive
→ UI renders progressively
```

Nhưng streaming làm lifecycle phức tạp hơn:

```text
client disconnect
cancellation propagation
partial output
moderation/validation timing
tool call interruption
usage finalization
```

Đừng chuyển sang streaming chỉ vì UX trông “AI-like”.

Dùng khi time-to-first-token thực sự quan trọng.

---

# 15. Cancellation và timeout

`CancellationToken` phải propagate:

```text
HTTP RequestAborted
→ application
→ IChatClient
→ retrieval
→ tools where safe
```

Ví dụ:

```csharp
using CancellationTokenSource timeout =
    CancellationTokenSource.CreateLinkedTokenSource(requestAborted);

timeout.CancelAfter(TimeSpan.FromSeconds(10));

ChatResponse response = await chatClient.GetResponseAsync(
    messages,
    options,
    timeout.Token);
```

Nhưng cancel local wait không chứng minh remote side effect chưa xảy ra.

---

# 16. OpenTelemetry integration

`Microsoft.Extensions.AI` hỗ trợ telemetry pipeline bằng familiar .NET patterns.

Architecture:

```text
IChatClient
→ OpenTelemetry decorator
→ provider
```

Telemetry nên answer:

```text
request nào chậm?
model/provider nào lỗi?
token usage thay đổi thế nào?
tool latency ở đâu?
release mới có cost regression không?
```

Metadata logging tốt hơn raw-content logging mặc định.

PII/confidential context cần policy riêng trước khi capture prompts/responses.

---

# 17. Caching

MEAI có thể compose caching/decorator behavior, nhưng cache AI response không phải default universal optimization.

Trước khi cache, hỏi:

```text
input có deterministic enough không?
user/tenant identity có ảnh hưởng không?
authorized context có thay đổi không?
source freshness bao lâu?
model/prompt version có nằm trong cache key không?
```

Danger:

```text
same question text
but different tenant/user/context
→ shared cached answer
→ data leak
```

Cache key phải include security/data/version dimensions khi relevant.

---

# 18. Embeddings — `IEmbeddingGenerator`

MEAI không chỉ có chat.

`IEmbeddingGenerator<TInput,TEmbedding>` là abstraction cho embedding generation.

Conceptually:

```text
text/chunk
→ IEmbeddingGenerator
→ vector
→ index/search
```

Application architecture:

```text
Ingestion
→ embedding abstraction
→ vector index

Query
→ embedding abstraction
→ retrieve
→ authorized/reranked context
→ IChatClient
```

Embedding model change là **index migration**, không chỉ config switch.

Cần track:

```text
embedding model/version
vector dimensions
index version
re-index status
retrieval eval
```

---

# 19. Evaluation — `Microsoft.Extensions.AI.Evaluation`

Evaluation packages build trên MEAI abstractions.

Có thể tách:

```text
Microsoft.Extensions.AI.Evaluation
→ core evaluation abstractions

...Evaluation.NLP
→ deterministic/traditional NLP similarity metrics

...Evaluation.Quality
→ quality evaluators

...Evaluation.Safety
→ safety evaluators/services

...Evaluation.Reporting
→ caching/results/reporting
```

Điều quan trọng không phải package count mà là release loop:

```text
model/prompt/tool/retrieval change
→ evaluation suite
→ quality/safety
→ latency/cost
→ release gate
```

Dùng deterministic assertion cho invariant deterministic; đừng dùng LLM judge để kiểm `AuthZ == denied`.

→ [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)

---

# 20. Dependency Injection và lifetime

`IChatClient` implementation thường được tạo ở composition root và injected vào application service.

Conceptual DI:

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>
{
    IChatClient provider = CreateProviderClient();

    return new ChatClientBuilder(provider)
        .UseFunctionInvocation()
        .Build();
});
```

Nhưng lifetime phải follow implementation documentation.

`IChatClient` contract được thiết kế để concurrent use, nhưng:

- đừng dispose khi request đang dùng;
- đừng reuse mutable `ChatOptions` instance giữa concurrent requests nếu pipeline/provider có thể mutate nó;
- user/session-specific state không nên nằm trong singleton client instance.

Session state nên nằm ở request/session storage/context builder.

---

# 21. MEAI vs Agent Framework vs Semantic Kernel

## Microsoft.Extensions.AI

Dùng khi cần:

```text
provider-agnostic chat/embedding abstractions
structured output
tool primitives
client middleware/decorators
telemetry/caching integration
```

## Agent Framework

Dùng khi cần higher-level agent runtime/orchestration abstractions và lifecycle vượt quá một simple application workflow.

## Semantic Kernel

Có ecosystem/plugin/orchestration capabilities riêng; có thể coexist/interoperate depending architecture.

Rule của track này:

```text
Start with MEAI + normal application code.
Add agent framework only when agent behavior is actually required.
```

Không học framework trước khi hiểu model/context/tool loop.

---

# 22. MEAI không giải quyết gì cho bạn?

Không outsource các vấn đề này cho library:

```text
business boundaries
AuthN/AuthZ
prompt injection risk
retrieval ACL
source freshness
PII policy
idempotency
unknown outcome
human approval
eval dataset quality
cost budget
SLO/incident response
```

MEAI làm integration cleaner; nó không biến probabilistic system thành deterministic safe system.

---

# 23. Production reference architecture

```text
ASP.NET Core Endpoint
        ↓
Application Service
        ↓
Business AI Port
        ↓
AI Implementation
 ├─ ContextBuilder
 ├─ Retriever
 ├─ deterministic validation
 ├─ tool policies
 └─ IChatClient
        ↓
MEAI client pipeline
 ├─ telemetry
 ├─ function invocation when allowed
 └─ provider adapter
        ↓
OpenAI / Azure OpenAI / other provider
```

Important direction:

```text
provider SDK
must not become
business architecture
```

---

# 24. Runnable lab integration

`labs/19-ai-engineering` giữ deterministic domain/harness lab và thêm MEAI layer.

Hai verification paths:

```bash
# core architecture semantics
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- self-test

# Microsoft.Extensions.AI abstraction/function pipeline
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- meai-self-test
```

MEAI self-test không cần API key. Nó dùng deterministic `IChatClient` implementation để CI verify:

```text
IChatClient contract
ChatClientBuilder
structured typed response
AIFunction
UseFunctionInvocation
FunctionCallContent → FunctionResultContent loop
usage shape
```

Provider thật là optional command/extension path, không phải CI dependency.

---

# 25. Migration path từ provider SDK trực tiếp

Nếu code hiện tại:

```text
BusinessService
→ provider-specific ChatClient
```

migrate từng bước:

```text
1. Tạo business port
2. Move provider call vào AI implementation
3. Expose provider client as IChatClient
4. Move common concerns vào ChatClientBuilder pipeline
5. Convert text parsing → structured output
6. Convert arbitrary actions → AIFunction capability tools
7. Add eval + telemetry
8. Keep provider-specific escape hatch only where justified
```

Không cần rewrite toàn application một lần.

---

# 26. Failure drills nên chạy

## Provider unavailable

Expected:

```text
timeout/failure classified
fallback/degraded mode follows policy
no infinite retry
```

## Malformed structured output

Expected:

```text
deserialization/validation fails safely
no business action
```

## Unauthorized tool

Expected:

```text
model requests tool
AuthZ denies execution
```

## Runaway function loop

Expected:

```text
iteration/tool budget stops loop
```

## Context leak

Expected:

```text
tenant filter occurs before model
```

## Cancellation

Expected:

```text
request cancellation propagates
safe work stops
side-effect unknown outcome reconciled
```

---

# 27. Production checklist

- [ ] application references `Microsoft.Extensions.AI` intentionally;
- [ ] business code depends on business capability, not raw `IChatClient` everywhere;
- [ ] provider construction lives in composition root;
- [ ] package/provider versions pinned and reviewed;
- [ ] `ChatMessage` history/context is explicitly constructed;
- [ ] `ChatOptions` not shared mutably across concurrent requests;
- [ ] reasoning options benchmarked, not maximized by default;
- [ ] structured output gets deterministic validation;
- [ ] tools are capability-oriented;
- [ ] tool AuthZ occurs in application/backend;
- [ ] function/agent loop bounded;
- [ ] write tools have idempotency/unknown-outcome strategy;
- [ ] cancellation propagates;
- [ ] telemetry captures metadata/usage without unsafe raw logging;
- [ ] cache keys respect identity/context/version;
- [ ] embedding/index versions tracked if RAG is used;
- [ ] eval gate runs before model/prompt/retrieval changes ship;
- [ ] provider-specific behavior remains behind a narrow adapter/escape hatch.

---

# Exit Criteria

Bạn đạt chapter này khi có thể:

1. giải thích `Microsoft.Extensions.AI.Abstractions` vs `Microsoft.Extensions.AI`;
2. giải thích vai trò của `IChatClient`, `ChatMessage`, `ChatOptions`, `ChatResponse`;
3. tạo provider client rồi expose thành `IChatClient`;
4. build `ChatClientBuilder` pipeline;
5. dùng typed structured output;
6. tạo `AIFunction` và giải thích function invocation loop;
7. chứng minh model/tool request không bypass AuthZ;
8. giải thích `ReasoningOptions` là provider-agnostic hint;
9. đo usage/latency qua telemetry;
10. giải thích embedding/evaluation integration;
11. chạy `meai-self-test` thành công;
12. biết khi nào MEAI đủ và khi nào mới cần agent framework.

## Official sources

- Microsoft Learn — Microsoft.Extensions.AI libraries: https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai
- Microsoft Learn — AI apps for .NET developers: https://learn.microsoft.com/en-us/dotnet/ai/
- Microsoft Learn — `IChatClient` API: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient
- Microsoft Learn — AI tool calling: https://learn.microsoft.com/en-us/dotnet/ai/conceptual/ai-tools
- Microsoft Learn — structured output quickstart: https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/structured-output
- Microsoft Learn — AI Evaluation libraries: https://learn.microsoft.com/en-us/dotnet/ai/evaluation/libraries
- NuGet — Microsoft.Extensions.AI: https://www.nuget.org/packages/Microsoft.Extensions.AI

## Verification metadata

- Updated: 2026-09-03.
- Documentation baseline: `Microsoft.Extensions.AI` 10.9.0.
- Stable mechanisms are taught separately from provider/model-specific behavior.
- Exact runnable package versions are pinned in `labs/19-ai-engineering/AiEngineeringLab.csproj`.
