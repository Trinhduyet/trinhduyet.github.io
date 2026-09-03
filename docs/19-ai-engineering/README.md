# Module 19 — Production AI Engineering

> Mục tiêu: từ Software/Backend Engineer trở thành engineer có thể **design, build, evaluate, secure, deploy và operate một AI-powered product** — không dừng ở prompt hay một API call tới model.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Foundation</strong>&nbsp;Software Engineering first</span>
  <span><strong>Mode</strong>&nbsp;mental model → code → eval → failure → operations</span>
  <span><strong>Evidence</strong>&nbsp;runnable lab + self-test</span>
</div>

<div class="key-takeaway" markdown>
<strong>AI Engineer ≠ người chỉ biết prompt.</strong>

AI Engineer là software engineer xây application dùng model như một capability, rồi chịu trách nhiệm cho **context, tools, retrieval, authorization, evaluation, latency, cost, failure và lifecycle** xung quanh capability đó.
</div>

## Hiểu trong 5 phút

Một AI product production không phải:

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
Product / API
      ↓
Application Contract
      ↓
AI Orchestration / Harness
 ├─ instructions + messages
 ├─ context selection
 ├─ provider/model
 ├─ structured output
 ├─ retrieval
 ├─ tools + permissions
 ├─ workflow / bounded agent loop
 └─ timeout / budget / fallback
      ↓
Business APIs / SQL / Search / Storage
      ↓
Deterministic validation + authorization
      ↓
Evaluation + Tracing + Cost + Audit
      ↓
Deployment + Monitoring + Rollback
```

AI chỉ là một capability trong system.

---

# 1. Learning path — học đúng dependency

Đừng bắt đầu bằng RAG hay agent framework.

```text
Role + vocabulary
      ↓
LLM runtime / messages / context / tokens
      ↓
Structured output + tool calling
      ↓
Retrieval / RAG
      ↓
Evaluation / observability / safety
      ↓
Runnable evidence
      ↓
Production architecture / lifecycle
```

| Chapter | Mục tiêu |
|---|---|
| [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md) | phân biệt model/provider/harness/agent/workflow/tool/memory/eval |
| [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md) | hiểu provider request, context budget, reasoning trade-off, session/turn/tool loop |
| [Structured Output & Tool Calling](structured-output-and-tool-calling.md) | biến model output thành contract và expose capability có authorization |
| [RAG, Evaluation & Observability](rag-evaluation-and-observability.md) | retrieval lifecycle, ACL, eval regression, AI telemetry |
| [Runnable AI Engineering Lab](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/19-ai-engineering) | chạy orchestration/tool/eval/failure semantics không cần API key |
| [References](references.md) | official/provider sources + supplementary learning sources |

Sau Module 19, học [AI Coding Agents](../21-ai-coding-agents/README.md) để hiểu agent trên repository/shell/CI với trust boundary khác.

---

# 2. Sáu distinction phải thuộc

```text
Model != Agent
```

```text
Tool request != Tool execution
```

```text
System instruction != Authorization
```

```text
Context window != Memory database
```

```text
Structured output != Business correctness
```

```text
Reasoning effort != Always better
```

Nếu chưa giải thích được sáu câu này bằng một system cụ thể, chưa nên deep dive agent framework.

---

# 3. Model, provider, harness và agent

## Model

```text
parameters + inference
→ output
```

Model tự nó không có filesystem, DB, shell, business AuthZ hay persistent memory.

## Provider

Provider chịu trách nhiệm host/inference/API surface:

```text
model selection
request format
usage
rate limits
availability
provider-specific features
```

## Harness

Harness/application quản lý:

```text
instructions
messages/history
context selection
tool definitions
tool execution
permissions
retry/timeout
budgets
telemetry
```

## Agent

```text
Model
+ Harness
+ Context
+ Tools
+ Control loop
= Agent
```

Agent là một system behavior, không phải chỉ một model name.

---

# 4. Messages và context là runtime input

Conceptually mỗi model request nhận một working set:

```text
instructions
+ user messages
+ selected history
+ retrieved context
+ tool schemas
+ tool results
→ model
```

Model không tự “nhớ” toàn session nếu application không đưa state/context trở lại request.

Rule:

```text
Relevant + current + authorized context
> large noisy context
```

Context nhiều hơn có thể tăng:

```text
latency
cost
distraction
stale-data risk
prompt-injection surface
```

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
current business data
current repo code
official docs
retrieved policy
tool result
```

Production rule:

```text
Current truth
→ current source/context/tool
```

không dựa vào việc model “có vẻ nhớ”.

---

# 6. System prompt không phải security boundary

Instruction có thể nói:

```text
how to behave
which workflow to follow
when to ask for tools
```

Nhưng không thay được:

```text
Authentication
Authorization
Business validation
Rate limiting
Idempotency
Sandbox
Human approval
```

Correct direction:

```text
Identity
→ Application AuthZ
→ Business rule
→ Capability execution
```

Model chỉ đề xuất action.

---

# 7. Business abstraction trước provider abstraction

Provider abstraction hữu ích:

```text
IChatClient / provider adapter
```

Nhưng business code nên phụ thuộc business capability:

```csharp
public interface IOrderRiskClassifier
{
    Task<RiskDecision> ClassifyAsync(
        OrderRiskInput input,
        CancellationToken cancellationToken);
}
```

Direction:

```text
Order Application
→ IOrderRiskClassifier
→ AI implementation
→ provider abstraction
```

Lợi ích:

- domain không biết vendor;
- fake/test dễ;
- fallback dễ hơn;
- provider replacement không lan toàn codebase.

---

# 8. Structured Output — schema là lớp đầu, không phải lớp cuối

Model output nên vào typed contract khi business code cần structure.

```csharp
public sealed record RiskDecision(
    string Level,
    string Reason,
    bool RequiresHumanReview);
```

Nhưng:

```text
Schema-valid
!=
Business-valid
```

Pipeline đúng:

```text
model output
→ schema/type validation
→ deterministic domain validation
→ business action
```

---

# 9. Tool Calling — model không có authority

Flow:

```text
Model
→ ToolRequest
→ Harness/Application
→ allowlist
→ input validation
→ AuthZ
→ execute capability
→ ToolResult
→ next model request
```

Tool production phải trả lời:

1. identity nào gọi;
2. permission nào cần;
3. side effect gì;
4. retry có an toàn không;
5. timeout là failed hay unknown outcome;
6. audit log ở đâu;
7. approval có cần không.

Expose capability-oriented tools:

```text
get_order_status(orderId)
search_authorized_documents(query)
create_support_draft(ticketId)
```

trước khi nghĩ tới generic `run_shell` hay `execute_sql`.

---

# 10. Workflow vs Agent

Dùng deterministic workflow khi steps đã biết:

```text
validate
→ retrieve
→ classify
→ validate output
→ save
```

Dùng agent khi next step thực sự cần semantic action selection:

```text
inspect alert
→ choose evidence source
→ inspect deploy
→ inspect dependency
→ formulate hypothesis
```

Agent phải bounded:

```text
max turns
max tool calls
time budget
cost budget
tool allowlist
permissions
approval
cancellation
```

---

# 11. RAG là data system

Production RAG:

```text
Source of Truth
→ extract/normalize
→ chunk + metadata
→ ACL/tenant filter
→ embed/index
→ retrieve/filter/rerank
→ context
→ generate
→ citation/provenance
```

Nó có lifecycle/failure riêng:

```text
freshness
deletion
versioning
index migration
stale derived data
tenant isolation
re-index
cost
```

ACL phải xảy ra **trước khi model nhìn thấy dữ liệu**.

---

# 12. Evaluation là test system của AI behavior

Không có eval thì change này:

```text
model
prompt
retrieval
chunking
tool description
reasoning effort
```

chỉ được review bằng cảm giác.

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

Dùng deterministic checks cho invariant deterministic:

```text
schema valid?
AuthZ denied?
expected source retrieved?
tool allowlist enforced?
```

Dùng judgement eval cho behavior như relevance/groundedness khi cần.

---

# 13. Reasoning là quality / latency / cost trade-off

Không dùng:

```text
max reasoning everywhere
```

Dùng:

```text
eval set
→ compare effort/configs
→ quality
→ P95 latency
→ usage/cost
→ choose
```

Provider-specific reasoning semantics thay đổi theo model/API, vì vậy official docs là canonical source.

---

# 14. AI observability

Backend telemetry vẫn cần:

```text
logs
metrics
traces
```

AI thêm:

```text
provider/model
prompt/config version
retrieval/index version
input/output usage
reasoning config
tool-call count
tool latency
provider latency
fallback rate
eval score
cost/request
```

Không log raw prompt/context/tool result mặc định nếu có PII, secrets hoặc internal data.

---

# 15. AI failure vẫn là Distributed Systems

```text
provider timeout
!=
provider definitely did nothing
```

Đặc biệt write tool:

```text
tool timeout
→ side effect may already exist
```

Cần áp dụng:

```text
idempotency
status query
reconciliation
bounded retry
```

→ [Distributed Systems](../17-distributed-systems/README.md)

---

# 16. Runnable Lab — executable evidence

Module này có lab chạy được không cần API key:

**[labs/19-ai-engineering](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/19-ai-engineering)**

Lab dùng deterministic fake model có chủ đích để orchestration semantics không phụ thuộc network/quota/model drift.

Run:

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- demo
```

Eval:

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- eval
```

Self-test:

```bash
dotnet run \
  --project labs/19-ai-engineering/AiEngineeringLab.csproj \
  -- self-test
```

Lab chứng minh bằng executable checks:

```text
Tenant-aware context selection
Context budget
Structured output validation
Read-only tool execution
Application-level authorization
Prompt-injection text does not gain authority
Bounded tool loop
Reasoning/eval trade-off
Usage telemetry shape
```

CI build và chạy `self-test` trên mỗi Pages deployment.

---

# 17. Tại sao lab mặc định không gọi provider thật?

Một runnable learning artifact phải reproducible.

Nếu baseline phụ thuộc:

```text
API key
network
quota
model availability
provider behavior drift
```

thì CI không còn deterministic.

Vì vậy architecture của lab là:

```text
IAiModel
├─ DeterministicModel   ← CI / learning baseline
└─ RealProviderAdapter  ← extension exercise
```

Khi thay bằng provider thật, giữ nguyên:

```text
ToolHost/AuthZ
Context boundary
Structured validation
Eval suite
Failure drills
```

---

# 18. Production project spine

Sau core lab, build **Enterprise Knowledge & Operations Assistant**.

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
- business ports thay vì provider SDK trong domain;
- auth/AuthZ;
- ACL-aware retrieval;
- prompt/context budget;
- structured output;
- read-only tools first;
- bounded agent loop only when justified;
- evaluation dataset;
- model/prompt/retrieval versioning;
- tracing + usage/cost metrics;
- Docker/Azure deployment;
- CI/CD + rollback;
- PII-safe logging.

Failure drills:

```text
model timeout
large/noisy context
stale knowledge
retrieval unavailable
unauthorized retrieval
direct/indirect prompt injection
malformed structured output
wrong tool selected
tool timeout
provider outage
cost spike
runaway tool loop
```

Evidence:

```text
architecture diagram
trust-boundary diagram
OpenAPI
AuthZ negative tests
context/usage report
eval report
latency/cost dashboard
failure drill report
ADR: provider/search/agent-vs-workflow
```

---

# 19. Exit Criteria

Bạn hoàn thành Module 19 khi có thể:

- [ ] giải thích model/provider/harness/agent bằng một architecture cụ thể;
- [ ] giải thích messages/context/session/turn/provider-request flow;
- [ ] phân biệt parametric và contextual knowledge;
- [ ] thiết kế context/usage budget;
- [ ] chọn workflow vs agent và giải thích trade-off;
- [ ] implement structured output + deterministic validation;
- [ ] implement tool có AuthZ/idempotency phù hợp;
- [ ] thiết kế RAG có ACL/deletion/versioning;
- [ ] xây eval suite;
- [ ] benchmark quality/latency/cost giữa configs;
- [ ] xử lý provider/tool timeout theo distributed-systems semantics;
- [ ] chạy `labs/19-ai-engineering` và có `SELF-TEST PASS`;
- [ ] deploy/observe AI feature như production subsystem.

## Học tiếp

1. [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md)
2. [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md)
3. [Structured Output & Tool Calling](structured-output-and-tool-calling.md)
4. [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)
5. [Runnable AI Engineering Lab](https://github.com/Trinhduyet/trinhduyet.github.io/tree/main/labs/19-ai-engineering)
6. [AI Coding Agents](../21-ai-coding-agents/README.md)
7. [System Design](../24-system-design/README.md)
8. [Software Architecture](../25-software-architecture/README.md)
9. [References](references.md)

## Verification metadata

- Rebuilt foundations: 2026-09-03.
- Runnable lab added: 2026-09-03.
- CI evidence: `.NET 10 build + self-test` runs in the Pages workflow.
- Supplementary learning sources: AI Hero — AI Engineer Roadmap, AI Coding Dictionary, LLM Fundamentals.
- Provider/version-specific semantics: current official docs are canonical; see [References](references.md) and [Technology Baseline](../00-roadmap/technology-baseline.md).
