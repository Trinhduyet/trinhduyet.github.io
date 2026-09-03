# Module 19 — Production AI Engineering

> Mục tiêu: từ Software/Backend Engineer trở thành engineer có thể **design, build, evaluate, secure, deploy và operate AI-powered systems** — không dừng ở prompt engineering hay một API call tới model.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Foundation</strong>&nbsp;Software Engineering first</span>
  <span><strong>.NET path</strong>&nbsp;Microsoft.Extensions.AI</span>
  <span><strong>Evidence</strong>&nbsp;runnable lab + CI self-test</span>
</div>

<div class="key-takeaway" markdown>
<strong>AI Engineer = Software Engineer + AI runtime knowledge + evaluation + operations.</strong>

Model chỉ là một dependency. Product quality đến từ cách bạn quản lý **context, contracts, tools, authorization, retrieval, evaluation, latency, cost, failure và lifecycle** xung quanh model.
</div>

## Hiểu trong 5 phút

AI application production không phải:

```text
User
→ Prompt
→ LLM
→ Text
```

Nó gần với:

```text
Authenticated User
      ↓
ASP.NET Core / Product API
      ↓
Application Service
      ↓
Business AI Capability
      ↓
AI Orchestration / Harness
 ├─ instructions + messages
 ├─ context selection
 ├─ Microsoft.Extensions.AI / IChatClient
 ├─ structured output
 ├─ retrieval / RAG
 ├─ tools + permissions
 ├─ workflow / bounded agent loop
 └─ timeout / budget / fallback
      ↓
Provider / Model
      ↓
Deterministic validation + business systems
      ↓
Eval + Observability + Cost + Audit
      ↓
Deployment + Monitoring + Rollback
```

Nếu chỉ học prompt và model API, bạn mới học một phần nhỏ của AI Engineering.

---

# 1. Học theo dependency, không theo hype

Thứ tự khuyến nghị:

```text
1. Role + vocabulary
        ↓
2. LLM runtime
   messages / context / tokens / reasoning
        ↓
3. .NET integration
   Microsoft.Extensions.AI
        ↓
4. Structured output
        ↓
5. Tool calling + AuthZ
        ↓
6. Retrieval / RAG
        ↓
7. Evaluation + observability
        ↓
8. Workflow / bounded agents
        ↓
9. Deployment / failure / lifecycle
        ↓
10. Runnable evidence
```

| Chapter | Bạn cần hiểu |
|---|---|
| [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md) | model/provider/harness/agent/workflow/tool/memory/eval là gì |
| [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md) | request thực sự chứa gì, context window, session/turn, reasoning trade-off |
| [Microsoft.Extensions.AI — .NET Integration Guide](microsoft-extensions-ai-dotnet-integration.md) | `IChatClient`, pipeline, provider adapters, tools, structured output, telemetry/eval |
| [Structured Output & Tool Calling](structured-output-and-tool-calling.md) | typed contract + capability tools + AuthZ/idempotency |
| [RAG, Evaluation & Observability](rag-evaluation-and-observability.md) | retrieval lifecycle, ACL, eval regression, AI telemetry |
| [Runnable AI Engineering Lab](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/19-ai-engineering) | executable evidence, MEAI self-test, failure drills |
| [References](references.md) | official/provider sources + supplementary learning sources |

Sau Module 19, học [AI Coding Agents](../21-ai-coding-agents/README.md) để đi sâu vào coding agents, repository tools, permissions và sandbox.

---

# 2. Mười distinction phải thuộc

```text
Model != Agent
```

```text
Provider != Model
```

```text
Microsoft.Extensions.AI != Provider
```

```text
Tool request != Tool execution
```

```text
System instruction != Authorization
```

```text
Context window != Persistent memory
```

```text
Structured output != Business correctness
```

```text
RAG != Vector database
```

```text
Reasoning effort != Always better
```

```text
Agent != Default architecture
```

Nếu chưa giải thích được các câu này bằng một system cụ thể, chưa nên học agent framework sâu.

---

# 3. Model, provider, harness, workflow và agent

## Model

```text
parameters + inference
→ output
```

Model tự nó không có:

```text
filesystem
database
business AuthZ
persistent memory
shell
network permission
agent loop
```

## Provider

Provider host model và expose API/runtime:

```text
model/deployment selection
request protocol
rate limit
usage
availability
provider-specific capabilities
```

## Harness

Harness/application quyết định:

```text
instructions
messages/history
context selection
tool schemas
tool execution
permissions
budgets
retry/timeout
telemetry
```

## Workflow

Workflow có steps tương đối biết trước:

```text
validate
→ retrieve
→ classify
→ validate output
→ save
```

## Agent

```text
Model
+ Harness
+ Context
+ Tools
+ Control loop
= Agent behavior
```

Agent phù hợp khi next action thực sự cần semantic selection/exploration; không phải mọi AI feature đều cần agent.

---

# 4. LLM request mental model

Conceptually một request gồm:

```text
instructions
+ user message
+ selected conversation history
+ retrieved context
+ tool definitions
+ previous tool results
+ generation/reasoning options
        ↓
      model
        ↓
text / structured data / tool request
```

Model không tự “nhớ” toàn bộ session trừ khi provider/application lưu và đưa state trở lại theo API semantics.

Rule:

```text
Relevant + current + authorized context
> large noisy context
```

Nhiều context hơn có thể tăng:

```text
latency
cost
distraction
staleness
prompt-injection surface
```

→ [LLM Runtime](llm-runtime-messages-context-and-reasoning.md)

---

# 5. Parametric knowledge vs contextual knowledge

## Parametric knowledge

Knowledge nằm trong model parameters từ training.

```text
may be stale
may be incomplete
not controlled by your application
```

## Contextual knowledge

Knowledge application cung cấp ở runtime:

```text
current database result
current business policy
current source code
current official docs
retrieved document
tool result
```

Production rule:

```text
Current truth
→ current authorized source/context/tool
```

không dựa vào việc model “có vẻ biết”.

---

# 6. System prompt là instruction, không phải security

System/developer instruction có thể hướng dẫn:

```text
behavior
style
workflow
tool preference
response format
```

Nó không enforce:

```text
Authentication
Authorization
resource ownership
rate limit
idempotency
approval
sandbox
network policy
```

Correct boundary:

```text
Identity
→ Application AuthZ
→ Business validation
→ Capability
```

Model chỉ có thể đề xuất action.

---

# 7. Microsoft.Extensions.AI là integration layer chính của .NET track

Trong repository này, `.NET AI integration` được dạy qua `Microsoft.Extensions.AI`.

```text
Business capability
        ↓
Application implementation
        ↓
IChatClient / IEmbeddingGenerator
        ↓
MEAI client pipeline
        ↓
OpenAI / Azure OpenAI / other provider adapter
        ↓
Model
```

Core concepts:

```text
Microsoft.Extensions.AI.Abstractions
├─ IChatClient
├─ IEmbeddingGenerator<...>
├─ ChatMessage
├─ ChatOptions
└─ ChatResponse

Microsoft.Extensions.AI
├─ ChatClientBuilder
├─ structured output helpers
├─ AIFunction / function invocation
├─ telemetry
└─ caching/decorator utilities
```

Đọc sâu: [Microsoft.Extensions.AI — .NET Integration Guide](microsoft-extensions-ai-dotnet-integration.md).

---

# 8. Business abstraction trước technical abstraction

`IChatClient` rất hữu ích nhưng không nên trở thành dependency của toàn bộ domain.

Bad:

```text
OrderService
→ IChatClient
```

Better:

```csharp
public interface IOrderRiskClassifier
{
    Task<RiskDecision> ClassifyAsync(
        OrderRiskInput input,
        CancellationToken cancellationToken);
}
```

Architecture:

```text
Order Application
→ IOrderRiskClassifier
→ AiOrderRiskClassifier
→ IChatClient
→ Provider
```

Lợi ích:

- domain không biết vendor;
- test/fake dễ;
- AI implementation thay đổi không lan rộng;
- có thể fallback sang deterministic implementation.

---

# 9. Structured output — contract trước prose

Business system không nên parse text kiểu:

```text
Risk: HIGH
ManualReview: yes
```

bằng `Split`/regex tùy tiện.

Dùng typed contract:

```csharp
public sealed record RiskDecision(
    string Level,
    string Reason,
    bool RequiresHumanReview);
```

MEAI hỗ trợ strongly typed response helpers.

Nhưng:

```text
Schema-valid
!= Business-valid
```

Pipeline phải là:

```text
model output
→ schema/type parse
→ deterministic validation
→ domain action
```

→ [Structured Output & Tool Calling](structured-output-and-tool-calling.md)

---

# 10. Tool calling — model không có authority

Flow đúng:

```text
Model
→ tool request
→ MEAI/Application harness
→ allowlist
→ input validation
→ AuthZ
→ execute business capability
→ tool result
→ model again if needed
```

Tool tốt:

```text
get_order_status(orderId)
search_authorized_documents(query)
create_support_draft(ticketId)
```

Tool nguy hiểm nếu expose vô điều kiện:

```text
execute_sql(sql)
run_shell(command)
delete_any_resource(id)
```

Read/write tools có risk khác nhau.

Write tool cần nghĩ thêm:

```text
approval
idempotency
unknown outcome
audit
reconciliation
```

---

# 11. Bounded tool / agent loop

Một loop production cần budget:

```text
max model iterations
max tool calls
max wall-clock time
max cost
tool allowlist
write-tool policy
approval threshold
cancellation
```

MEAI có function invocation pipeline và iteration stopping mechanisms; application vẫn cần business-level budget.

Không có budget:

```text
model
→ tool
→ model
→ tool
→ ...
```

có thể thành runaway loop.

---

# 12. RAG là distributed data system

RAG production:

```text
Source of Truth
→ extract / normalize
→ chunk + metadata
→ ACL / tenant boundary
→ embeddings / index
→ retrieve
→ filter / rerank
→ context
→ model
→ citation / provenance
```

Nó có lifecycle:

```text
freshness
deletion
versioning
index migration
embedding migration
stale derived data
ACL changes
re-index
cost
```

Critical security rule:

```text
Unauthorized data
must be filtered
before model sees it
```

Không retrieve mọi thứ rồi “nhắc model đừng leak”.

→ [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)

---

# 13. Embeddings không phải magic semantic database

Embedding:

```text
input
→ embedding model
→ vector representation
```

Vector index/search vẫn cần:

```text
metadata
filters
ACL
ranking
hybrid search
freshness
evaluation
```

MEAI có `IEmbeddingGenerator<TInput,TEmbedding>` để giảm provider coupling.

Khi đổi embedding model:

```text
model switch
→ vector dimensions/behavior may change
→ index migration/rebuild
→ retrieval eval
```

Không coi nó là một config-only deploy.

---

# 14. Evaluation là test system của AI behavior

Không có eval, thay đổi này:

```text
model
prompt
system instruction
reasoning effort
chunking
embedding
reranker
tool description
```

chỉ được đánh giá bằng cảm giác.

Eval loop:

```text
Representative dataset
      ↓
Baseline vs Candidate
      ↓
Quality + Safety
+ Latency + Cost
      ↓
Release decision
```

## Deterministic assertions

Dùng code/test cho:

```text
schema valid?
AuthZ denied?
expected source retrieved?
tool allowlist enforced?
side effect exactly once?
```

## Judgement metrics

Dùng evaluator/human review khi cần:

```text
relevance
groundedness
completeness
helpfulness
```

`Microsoft.Extensions.AI.Evaluation` cung cấp .NET evaluation libraries trên MEAI abstractions.

---

# 15. Reasoning = quality / latency / cost trade-off

MEAI có provider-agnostic reasoning options, nhưng support thực tế phụ thuộc provider/model.

Không:

```text
High reasoning everywhere
```

Nên:

```text
representative eval
→ Low / Medium / High where supported
→ correctness
→ P95 latency
→ token/usage cost
→ choose config
```

Reasoning là runtime policy, không phải badge chất lượng.

---

# 16. AI observability

Backend vẫn cần:

```text
logs
metrics
traces
```

AI thêm dimensions:

```text
provider/model
prompt/config version
retrieval/index version
input tokens
output tokens
reasoning tokens
tool calls
tool latency
provider latency
fallback rate
eval score
cost/request
```

MEAI có client pipeline/OpenTelemetry integration, nhưng data policy vẫn thuộc application.

Không mặc định log raw prompt/context/tool output nếu có:

```text
PII
secret
internal document
credential
regulated data
```

---

# 17. Timeout, retry và unknown outcome vẫn là Distributed Systems

```text
provider timeout
!= provider definitely did nothing
```

Với read-only model call, retry có thể tương đối đơn giản hơn.

Với side-effect tool:

```text
tool timeout
→ action may already have happened
```

Cần:

```text
idempotency key
status query
reconciliation
bounded retry
```

→ [Distributed Systems](../17-distributed-systems/README.md)

---

# 18. Provider fallback không phải đổi model name

```text
Provider A fails
→ Provider B
```

có thể khác:

```text
message semantics
structured-output support
tool behavior
reasoning support
context limits
latency
cost
safety filters
data residency
```

Fallback config phải chạy cùng acceptance/eval suite.

MEAI giảm coupling nhưng không làm provider semantics giống hệt nhau.

---

# 19. Khi nào dùng workflow, khi nào dùng agent?

## Workflow trước

Dùng khi steps known:

```text
validate
→ retrieve
→ model classify
→ deterministic validate
→ save
```

Ưu điểm:

```text
predictable
easier test
smaller blast radius
clear failure path
```

## Agent sau

Dùng khi next step cần semantic reasoning:

```text
inspect alert
→ choose evidence
→ inspect logs/deploy/dependency
→ formulate hypothesis
```

Agent vẫn phải bounded và authorized.

---

# 20. MEAI vs Agent Framework

Start with:

```text
normal .NET application code
+ Microsoft.Extensions.AI
```

Chỉ thêm higher-level agent framework khi thật sự cần:

```text
agent lifecycle
multi-step autonomous orchestration
higher-level agent abstractions
```

Không cần agent framework để:

```text
single model call
structured output
RAG answer
typed classifier
simple tool-enabled workflow
```

---

# 21. Production architecture trên Azure

Reference shape:

```text
Front Door / APIM
      ↓
ASP.NET Core API
      ↓
Application / Business AI Ports
      ↓
Microsoft.Extensions.AI
      ↓
Azure OpenAI / provider
      ↓
Azure AI Search / SQL / Storage / APIs
      ↓
Managed Identity + Key Vault
      ↓
Azure Monitor / OpenTelemetry
```

Service names không phải architecture.

Bắt đầu từ:

```text
business outcome
latency
availability
data boundary
security
cost
```

rồi mới chọn Azure services.

→ [Azure & Platform](../14-cloud/README.md)

---

# 22. Runnable Lab — từ theory sang evidence

Module có runnable `.NET 10` lab:

**[labs/19-ai-engineering](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/19-ai-engineering)**

## Core self-test

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- self-test
```

Kiểm:

```text
tenant-aware retrieval
context budget
structured validation
read-only tool
AuthZ
prompt-injection boundary
bounded loop
eval trade-off
```

## Microsoft.Extensions.AI self-test

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- meai-self-test
```

Kiểm trực tiếp:

```text
IChatClient
ChatMessage / ChatOptions / ChatResponse
ChatClientBuilder
GetResponseAsync<T>
AIFunction
UseFunctionInvocation
FunctionCallContent → FunctionResultContent
UsageDetails
```

Hai self-test đều không cần API key nên chạy ổn định trong CI.

Provider thật là optional adapter path, không phải CI dependency.

---

# 23. Project spine — Enterprise Knowledge & Operations Assistant

Functional:

```text
login
ask question
retrieve authorized docs
generate cited answer
read business status through tool
return structured result
```

Engineering requirements:

- ASP.NET Core API;
- business ports;
- `Microsoft.Extensions.AI` integration;
- OpenAI/Azure OpenAI adapter;
- authentication + authorization;
- ACL-aware RAG;
- context/token budget;
- structured output;
- read-only tools first;
- bounded agent loop only when justified;
- eval dataset;
- prompt/model/index versioning;
- OpenTelemetry + usage/cost;
- Docker/Azure deployment;
- CI/CD + rollback;
- PII-safe logging.

Failure drills:

```text
model timeout
large/noisy context
stale model knowledge
retrieval outage
unauthorized retrieval
prompt injection
malformed structured output
wrong tool
unauthorized tool
tool timeout
provider outage
cost spike
runaway loop
```

Evidence:

```text
architecture diagram
trust-boundary diagram
OpenAPI
AuthZ negative tests
context/token report
MEAI pipeline diagram
eval report
latency/cost dashboard
failure drill report
ADR: provider / RAG / workflow-vs-agent
```

---

# 24. Common mistakes

## “AI Engineer = prompt engineer”

Thiếu backend/data/security/operations.

## “Model = agent”

Sai system boundary.

## “IChatClient = business abstraction”

Technical abstraction vẫn không thay business port.

## “System prompt = security”

Instruction không enforce permission.

## “More context is always better”

Noise/cost/attack surface tăng.

## “Structured output = correct output”

Schema không enforce domain invariant.

## “RAG = vector DB”

Bỏ qua ingestion/ACL/deletion/eval.

## “Automatic function invocation = safe tools”

Library có thể execute tool loop; AuthZ vẫn phải nằm trong tool/backend.

## “High reasoning = always better”

Có thể chỉ tăng latency/cost.

## “MEAI makes providers identical”

Abstraction giảm coupling, không xóa capability/semantic differences.

## “Agent for every workflow”

Deterministic code thường tốt hơn khi steps đã biết.

---

# 25. Production checklist

- [ ] business outcome rõ;
- [ ] model/provider/harness/agent boundary rõ;
- [ ] .NET layer dùng `Microsoft.Extensions.AI` có chủ đích;
- [ ] business code không phụ thuộc raw provider SDK;
- [ ] context construction explicit;
- [ ] tenant/ACL filter trước model;
- [ ] system instruction không dùng làm AuthZ;
- [ ] structured output + deterministic validation;
- [ ] tools capability-oriented;
- [ ] tool AuthZ/idempotency/approval phù hợp;
- [ ] tool/agent loop bounded;
- [ ] reasoning config benchmarked;
- [ ] eval regression tồn tại;
- [ ] embeddings/index lifecycle có version nếu dùng RAG;
- [ ] cancellation/timeout rõ;
- [ ] unknown side-effect outcome có reconciliation;
- [ ] telemetry PII-safe;
- [ ] usage/latency/cost/fallback metrics;
- [ ] provider fallback tested;
- [ ] deployment/rollback/failure drills;
- [ ] `self-test` + `meai-self-test` pass.

---

# 26. Exit Criteria

Bạn hoàn thành Module 19 khi có thể:

1. giải thích AI Engineer khác ML Engineer ở application responsibility;
2. giải thích model/provider/harness/workflow/agent;
3. giải thích messages/context/session/turn/provider-request;
4. phân biệt parametric/contextual knowledge;
5. giải thích `Microsoft.Extensions.AI.Abstractions` vs `Microsoft.Extensions.AI`;
6. dùng `IChatClient` qua provider adapter;
7. build `ChatClientBuilder` pipeline;
8. implement typed structured output + deterministic validation;
9. implement `AIFunction` tool có AuthZ;
10. thiết kế RAG có ACL/deletion/versioning;
11. hiểu `IEmbeddingGenerator` và embedding migration;
12. xây eval suite + release gate;
13. benchmark reasoning quality/latency/cost;
14. xử lý timeout/cancellation/unknown outcome;
15. giải thích workflow vs agent;
16. chạy `labs/19-ai-engineering` với `SELF-TEST PASS` và `MEAI SELF-TEST PASS`;
17. deploy/observe AI capability như một production subsystem.

## Học tiếp

1. [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md)
2. [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md)
3. [Microsoft.Extensions.AI — .NET Integration Guide](microsoft-extensions-ai-dotnet-integration.md)
4. [Structured Output & Tool Calling](structured-output-and-tool-calling.md)
5. [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)
6. [AI Coding Agents](../21-ai-coding-agents/README.md)
7. [System Design](../24-system-design/README.md)
8. [Software Architecture](../25-software-architecture/README.md)
9. [References](references.md)

## Verification metadata

- Reworked: 2026-09-03.
- .NET AI integration baseline: `Microsoft.Extensions.AI` 10.9.0.
- Official Microsoft/.NET docs are canonical for MEAI APIs and package semantics.
- Provider/model-specific details remain version-sensitive and must be re-checked before production release.
