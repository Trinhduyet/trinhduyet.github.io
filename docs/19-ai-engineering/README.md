# Module 19 — AI Engineering cho Software / Backend Engineer

> Mục tiêu: xây AI application **có thể chạy như một sản phẩm thật** — có backend, data, API, auth, cloud/infra, deployment, observability, evaluation và recovery; không dừng ở việc gọi model API.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Foundation</strong>&nbsp;Software Engineering first</span>
  <span><strong>Target</strong>&nbsp;Production AI Engineer</span>
</div>

<div class="key-takeaway" markdown>
<strong>Triết lý của track AI Engineering</strong>

Hãy học cách **build hệ thống**:

- làm backend;
- hiểu database;
- biết API;
- hiểu cloud/infra;
- biết triển khai sản phẩm;
- có project thật để chứng minh mình build được.

**AI Engineer trong thực tế không chỉ là người “biết AI”.** Đó là người đủ kỹ năng engineering để đưa AI vào một hệ thống hoạt động thật.
</div>

## 1. AI Engineer production cần stack gì?

```text
Software Engineering foundation
│
├── Programming / .NET / Python as needed
├── Backend Engineering
├── Database / SQL / Search
├── API / Auth / Security
├── Distributed Systems
├── Cloud / Infra
├── CI/CD / Deployment
└── Observability / Operations
          ↓
AI Engineering layer
│
├── Models / providers
├── Prompt / context
├── Structured output
├── Tool calling
├── Embeddings / Retrieval / RAG
├── Evaluation
├── Safety / authorization
└── Model / prompt / index lifecycle
          ↓
Production product
```

Nếu chỉ biết:

```text
prompt → model → string
```

thì mới biết một phần rất nhỏ của AI application.

## 2. Một AI feature thực tế trông như thế nào?

Ví dụ: internal architecture assistant đọc dữ liệu dự án và gọi read-only tools.

```text
Authenticated User
      ↓
Web / App
      ↓
ASP.NET Core API
      ↓
Authentication + Authorization
      ↓
AI Application Service
 ├─ model/provider abstraction
 ├─ context builder
 ├─ retrieval
 ├─ tool calling
 └─ structured output
      ↓
Business APIs / SQL / Search / Vector Index
      ↓
Evaluation + Tracing + Cost + Audit
      ↓
Cloud deployment + monitoring + rollback
```

AI chỉ là một capability trong system.

## 3. Engineering dependency map

Trước khi deep dive AI, cần tương đối chắc:

### Backend

```text
HTTP request lifecycle
async/cancellation
dependency injection
background jobs
rate limiting
resilience
```

→ [Backend Engineering](../04-backend/README.md) · [ASP.NET Core](../07-aspnet-core/README.md)

### Database

```text
schema
indexes
transactions
concurrency
query plans
```

→ [SQL](../05-sql/README.md)

### API / Security

```text
contracts
authentication
authorization
OAuth/OIDC
idempotency
errors
```

→ [API Design](../06-api-design/README.md)

### Distributed Systems

```text
timeout
retry
duplicate
queue
consistency
reconciliation
```

→ [Distributed Systems](../17-distributed-systems/README.md)

### Cloud / Infrastructure

```text
identity
network
compute
data
messaging
observability
DR
cost
```

→ [Cloud & Microsoft Azure](../14-cloud/README.md)

### System Design / Architecture

```text
requirements
capacity
failure
boundaries
trade-offs
evolution
```

→ [System Design](../24-system-design/README.md) · [Software Architecture](../25-software-architecture/README.md)

## 4. AI Engineering mental model

```text
Business capability
        ↓
Application contract
        ↓
AI orchestration boundary
        ↓
Model + Retrieval + Tools
        ↓
Deterministic business validation
        ↓
Evaluation + Security + Observability
        ↓
Production operation
```

Một AI Engineer production phải kiểm soát ít nhất 9 biến:

1. **Business outcome** — AI đang cải thiện workflow nào?
2. **Model** — quality, latency, cost, availability.
3. **Context** — system instruction, history, business data.
4. **Output contract** — text hay structured schema.
5. **Retrieval** — data nào được retrieve, freshness/ACL ra sao.
6. **Tools** — model được phép đề xuất/gọi action nào.
7. **Authorization** — application có enforce quyền thật không.
8. **Evaluation** — release mới tốt hơn hay regress?
9. **Operations** — timeout, rate limit, tracing, cost, fallback, rollback.

## 5. Code đầu tiên — provider abstraction với `IChatClient`

`Microsoft.Extensions.AI` cung cấp abstraction để application code không gắn chặt vào một provider.

```csharp
using Microsoft.Extensions.AI;

public sealed class ArchitectureTutor(IChatClient chatClient)
{
    public async Task<string> ExplainAsync(
        string topic,
        CancellationToken cancellationToken)
    {
        ChatResponse response = await chatClient.GetResponseAsync(
            [
                new(ChatRole.System,
                    "Bạn là software architect. Giải thích ngắn, có ví dụ production."),
                new(ChatRole.User, topic)
            ],
            cancellationToken: cancellationToken);

        return response.Text;
    }
}
```

Boundary:

```text
ArchitectureTutor
      ↓
IChatClient
      ↓
OpenAI / Azure OpenAI / local/other provider adapter
```

Business/application code có thể test/fake mà không phụ thuộc trực tiếp SDK provider.

## 6. Business capability abstraction tốt hơn model abstraction thuần túy

Bad:

```csharp
public sealed class OrderService
{
    private readonly ChatClient _provider;
}
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

Implementation AI đứng sau business port:

```csharp
public sealed class AiOrderRiskClassifier(IChatClient chatClient)
    : IOrderRiskClassifier
{
    public async Task<RiskDecision> ClassifyAsync(
        OrderRiskInput input,
        CancellationToken cancellationToken)
    {
        ChatResponse<RiskDecision> result =
            await chatClient.GetResponseAsync<RiskDecision>(
                $"""
                Evaluate the order risk.
                CustomerId: {input.CustomerId}
                Amount: {input.Amount}
                Country: {input.Country}
                """,
                cancellationToken: cancellationToken);

        return result.Result;
    }
}
```

Ứng dụng phụ thuộc `IOrderRiskClassifier`, không phụ thuộc concept “chat” ở mọi nơi.

## 7. Structured output

Business systems cần contract.

Fragile:

```csharp
string level = response.Text.Split(':')[1];
```

Better:

```json
{
  "level": "HIGH",
  "reason": "...",
  "requiresHumanReview": true
}
```

Nhưng structured output từ model vẫn phải validate business constraints. Schema-valid không đồng nghĩa business-correct.

## 8. Tool Calling — model không phải authorization boundary

Sai:

```text
System prompt:
"Only refund if user is admin."
```

Đúng hơn:

```text
User identity
      ↓
Application authorization
      ↓
RefundTool
      ↓
Business validation / idempotency
      ↓
Payment system
```

Model có thể **request** tool; application quyết định tool có được thực thi không.

## 9. RAG — retrieval là data system

RAG không chỉ:

```text
PDF → embeddings → vector DB → model
```

Production RAG cần:

```text
ingestion
chunking
metadata
versioning
ACL/security trimming
index lifecycle
freshness
deletion
retrieval evaluation
citation/provenance
```

Nếu user không được đọc document X, retrieval layer không được dựa vào prompt để “nhắc model đừng dùng X”.

## 10. Evaluation

Không có eval thì đổi model/prompt/retrieval chỉ là cảm giác.

Eval pipeline:

```text
golden / representative dataset
      ↓
old config vs new config
      ↓
quality metrics
+ latency
+ cost
+ safety / authorization regressions
      ↓
release gate
```

Có thể cần human evaluation cho subjective output, deterministic tests cho structured/tool/business rules.

## 11. AI observability

Cần quan sát ít nhất:

```text
request latency
model latency
retrieval latency
tool latency
input/output token or usage cost
model/provider errors
structured-output failures
tool-call rate
eval score
fallback rate
```

Không log raw prompt/context nếu có PII/secret/confidential data mà chưa có data policy.

## 12. Timeout / cancellation

AI call thường chậm hơn DB/API thông thường.

HTTP cancellation nên propagate:

```text
HTTP request aborted
→ application CancellationToken
→ retrieval/model/tool calls cancelled where safe
```

Nhưng external tool có side effect:

```text
tool timeout
!=
action definitely failed
```

Vẫn áp distributed-systems mental model `UNKNOWN + reconcile` khi phù hợp.

## 13. Provider failure / fallback

Fallback không đơn giản:

```text
Provider A fails
→ call Provider B
```

vì:

- models khác behavior/schema/tool semantics;
- data residency/compliance khác;
- latency/cost khác;
- same request có side-effect tool context.

Fallback strategy phải được evaluated, not assumed.

## 14. Cloud deployment

Một production AI app trên Azure có thể có:

```text
Front Door / APIM
→ .NET API (App Service / Container Apps / AKS when justified)
→ Azure OpenAI / provider
→ Azure SQL / Search / Storage
→ Service Bus for async jobs
→ Managed Identity + Key Vault
→ Azure Monitor / App Insights
```

Service names không phải architecture; [Module 14](../14-cloud/README.md) dạy cách chọn theo workload.

## 15. AI Project thật để chứng minh build được

Build một **Enterprise Read-Only Knowledge Assistant**:

### Functional

```text
login
ask question
retrieve authorized docs
generate cited answer
call read-only business tool
show structured result
```

### Engineering requirements

- ASP.NET Core API;
- SQL for conversation/audit/config state;
- API auth/AuthZ;
- document ingestion + index;
- ACL-aware retrieval;
- provider abstraction;
- tool contracts;
- structured output;
- evaluation dataset;
- telemetry/cost;
- Docker + Azure deployment;
- IaC/CI/CD;
- rate limiting;
- fallback/degradation;
- PII-safe logging.

### Failure drills

```text
model timeout
retrieval service unavailable
unauthorized document retrieved attempt
prompt injection asks destructive tool
provider returns malformed output
tool API timeout
index stale after document deletion
provider outage
cost spike
```

### Portfolio evidence

```text
architecture diagram
OpenAPI
schema/index design
AuthZ tests
RAG eval report
latency/cost dashboard
failure drill report
ADR: provider/search/hosting choices
deployment URL/demo
```

Đây chứng minh anh **build được AI product**, không chỉ demo prompt.

## 16. Learning priorities

### P0 — engineering + AI application core

- backend/API/data/cloud foundation;
- model/provider abstraction;
- structured output;
- tool calling + authorization;
- cancellation/timeouts;
- evaluation/regression;
- AI security basics;
- telemetry/latency/cost;
- deploy/operate.

### P1

- embeddings/vector/semantic search;
- RAG lifecycle;
- model fallback/routing;
- prompt/model/config versioning;
- caching/rate limiting;
- async AI jobs;
- human-in-the-loop.

### P2

- fine-tuning lifecycle;
- custom model hosting;
- training infrastructure;
- advanced MLOps when role/project requires.

## 17. Common mistakes

### “AI-first, engineering-later”

Chat demo chạy local nhưng không có auth, data model, deploy, observability.

### Authorization in prompt

Security boundary sai.

### No eval

Không biết release tốt hơn hay tệ hơn.

### Raw logs

Prompt/context chứa PII/secrets bị đẩy vào telemetry.

### Provider SDK in domain code

Vendor details lan khắp business system.

### No degraded mode

Model provider outage → whole core product unavailable dù AI feature không critical.

## 18. Production checklist

- [ ] AI feature gắn với business outcome rõ;
- [ ] backend/API/data foundation production-ready;
- [ ] auth/AuthZ ngoài prompt;
- [ ] secrets không nằm source;
- [ ] model/provider config versioned;
- [ ] timeout/cancellation rõ;
- [ ] structured output validated;
- [ ] tools enforce authorization + idempotency;
- [ ] retrieval respects ACL/deletion/versioning;
- [ ] PII-safe telemetry;
- [ ] quality/latency/cost metrics;
- [ ] eval regression before release;
- [ ] fallback/degradation strategy tested;
- [ ] cloud deployment + rollback;
- [ ] failure drills documented;
- [ ] project runnable/demoable.

## Học tiếp

1. [Structured Output & Tool Calling](structured-output-and-tool-calling.md)
2. [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)
3. [Cloud & Azure](../14-cloud/README.md)
4. [System Design](../24-system-design/README.md)
5. [Software Architecture](../25-software-architecture/README.md)
6. [AI Coding Agents](../21-ai-coding-agents/README.md)

## Official English Sources

- Microsoft Learn — AI apps for .NET developers: https://learn.microsoft.com/en-us/dotnet/ai/
- Microsoft Learn — `IChatClient`: https://learn.microsoft.com/en-us/dotnet/ai/ichatclient
- OpenAI official .NET SDK: https://github.com/openai/openai-dotnet
- roadmap.sh AI Engineer: https://roadmap.sh/ai-engineer

## Verification metadata

- Updated: 2026-08-19.
- Target: production AI engineering on top of software/backend foundations.
- Version-specific APIs/packages must follow current official docs and `technology-baseline.md`.
