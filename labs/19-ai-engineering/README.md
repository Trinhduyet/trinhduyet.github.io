# Module 19 Runnable Lab — Core AI + Microsoft.Extensions.AI

Lab này có hai mục tiêu:

1. học **AI application semantics** mà không phụ thuộc provider;
2. chạy **Microsoft.Extensions.AI integration thật** trên .NET 10.

Không cần API key cho hai self-test chính.

```text
Lane A — Core architecture
IAiModel
→ deterministic harness
→ context / tools / AuthZ / eval

Lane B — .NET integration
IChatClient
→ ChatClientBuilder
→ structured output / AIFunction
→ function invocation pipeline
```

Provider thật là optional.

---

## Prerequisite

- .NET 10 SDK.

```bash
dotnet --version
```

Packages được pin trong project:

```text
Microsoft.Extensions.AI          10.9.0
Microsoft.Extensions.AI.OpenAI   10.9.0
```

Không dùng floating `latest` trong executable evidence.

---

# 1. Core demo

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- demo
```

Bạn sẽ thấy:

```text
authorized RAG-style context
read-only tool call
structured output
usage estimate
```

Core lane cố ý dùng `DeterministicModel` để failure semantics không drift theo provider/model.

---

# 2. Core self-test

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- self-test
```

Expected:

```text
SELF-TEST PASS
```

Self-test chứng minh:

```text
tenant-aware retrieval
context budget
structured output validation
read-only tool execution
application AuthZ
prompt-injection text remains untrusted
bounded tool loop
reasoning/eval trade-off
```

---

# 3. Microsoft.Extensions.AI demo

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- meai-demo
```

Lane này dùng trực tiếp:

```text
IChatClient
ChatMessage
ChatOptions
ChatResponse
ChatResponse<T>
ChatClientBuilder
AIFunction
AIFunctionFactory
UseFunctionInvocation
FunctionCallContent
FunctionResultContent
UsageDetails
ReasoningOptions
```

Deterministic `IChatClient` đóng vai provider để test abstraction/pipeline mà không gọi network.

---

# 4. Microsoft.Extensions.AI self-test

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- meai-self-test
```

Expected:

```text
MEAI SELF-TEST PASS
```

Nó verify ba behavior quan trọng.

## Typed structured output

```text
IChatClient
→ GetResponseAsync<RiskDecision>
→ JSON/schema conversion
→ RiskDecision
```

Application vẫn phải validate domain invariant sau đó.

## Automatic function invocation

```text
User message
→ deterministic IChatClient
→ FunctionCallContent(get_order_status)
→ FunctionInvokingChatClient
→ AIFunction executes
→ FunctionResultContent
→ provider client again
→ final ChatResponse
```

Đây là executable proof cho distinction:

```text
Tool request
!=
Tool implementation
```

Trong production, tool implementation phải tự enforce AuthZ/business rules.

## Reasoning usage shape

Self-test gửi `ReasoningOptions` ở low/high effort và chứng minh usage shape khác nhau.

Mục tiêu không phải mô phỏng provider hoàn hảo, mà verify application hiểu reasoning như một configurable trade-off.

---

# 5. Optional live OpenAI adapter

Chỉ chạy khi bạn chủ động cấu hình credentials.

```bash
export OPENAI_API_KEY='...'
export OPENAI_MODEL='...'

dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- meai-live
```

Code path:

```text
OpenAIClient
→ GetChatClient(model)
→ AsIChatClient()
→ ChatClientBuilder
→ UseFunctionInvocation()
→ application
```

`OPENAI_MODEL` không được hard-code vào lab vì model availability/lifecycle thay đổi nhanh.

Không commit API key.

---

# 6. Azure OpenAI extension exercise

Thay composition root bằng Azure OpenAI provider, còn application layer giữ `IChatClient`.

Conceptual mapping:

```text
AzureOpenAIClient
→ GetChatClient(deploymentName)
→ AsIChatClient()
→ same MEAI pipeline
```

Production ưu tiên Microsoft Entra/workload identity khi phù hợp.

Không đổi:

```text
business ports
AuthZ
eval dataset
context boundary
failure drills
```

---

# 7. Vì sao vẫn giữ `IAiModel` khi đã có `IChatClient`?

Hai abstraction có scope khác nhau.

```text
IAiModel
= teaching/application harness boundary của lab

IChatClient
= .NET ecosystem technical integration abstraction
```

Trong production business code, tốt hơn nữa là capability-specific port:

```text
IOrderRiskClassifier
IKnowledgeAssistant
IIncidentSummarizer
```

Direction tốt:

```text
Business Port
→ AI Implementation
→ IChatClient
→ provider adapter
```

Không dùng `IChatClient` như domain language ở mọi nơi.

---

# 8. Failure drills — core lane

## Unauthorized tool

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- failure-unauthorized-tool
```

Expected:

```text
EXPECTED DENY
```

## Runaway loop

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- failure-runaway-loop
```

Expected:

```text
EXPECTED STOP
```

## Noisy/malicious context

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- failure-noisy-context
```

Expected:

```text
malicious retrieved text stays data
no tool authority is gained
```

---

# 9. Eval

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- eval
```

Lab cố ý tạo một hard case để minh họa:

```text
Low effort
→ lower synthetic reasoning usage
→ one eval miss

High effort
→ higher synthetic reasoning usage
→ eval passes
```

Không suy ra rằng production luôn nên dùng high reasoning.

Production phải đo:

```text
quality
latency
real provider usage/cost
```

---

# 10. CI evidence

GitHub Pages workflow chạy:

```text
restore/build .NET 10 lab
↓
core self-test
↓
MEAI self-test
↓
learning quality audit
↓
MkDocs strict build
↓
Pages deploy
```

Do đó `Runnable` ở Module 19 có nghĩa code thực sự compile và assertions chạy trong CI.

---

# 11. Files

```text
AiEngineeringLab.csproj
├─ exact MEAI package pins

Program.cs
├─ routes core vs meai commands

LabRuntime.cs
├─ IAiModel
├─ DeterministicModel
├─ retrieval/context
├─ ToolHost/AuthZ
├─ eval
└─ core failure drills

MeaiIntegration.cs
├─ deterministic IChatClient
├─ ChatClientBuilder
├─ typed structured output
├─ AIFunction/function invocation
├─ reasoning/usage check
└─ optional OpenAI adapter
```

---

# 12. Production extension order

Không nhảy thẳng sang autonomous agent.

```text
1. Replace deterministic provider with real IChatClient adapter
2. Keep business port
3. Add real AuthN/AuthZ
4. Add real RAG/search
5. Add OpenTelemetry
6. Add Microsoft.Extensions.AI.Evaluation
7. Add provider/model eval matrix
8. Add timeout/fallback/degraded mode
9. Add write tool only with idempotency/approval
10. Add agent loop only when deterministic workflow is insufficient
```

---

# Exit Criteria

- [ ] `self-test` pass;
- [ ] `meai-self-test` pass;
- [ ] giải thích `IAiModel` vs `IChatClient`;
- [ ] giải thích `ChatClientBuilder` pipeline;
- [ ] đọc được `ChatMessage`, `ChatOptions`, `ChatResponse`;
- [ ] implement typed structured output;
- [ ] hiểu `FunctionCallContent → AIFunction → FunctionResultContent`;
- [ ] biết automatic function invocation không thay AuthZ;
- [ ] biết optional provider adapter nằm ở composition root;
- [ ] biết provider/model drift không được làm CI baseline mất reproducibility.

Đọc cùng [Microsoft.Extensions.AI — .NET Integration Guide](../../docs/19-ai-engineering/microsoft-extensions-ai-dotnet-integration.md).
