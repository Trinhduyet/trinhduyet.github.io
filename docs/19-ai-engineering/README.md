# Module 19 — Production AI Engineering

> Mục tiêu: từ Software/Backend Engineer trở thành engineer có thể **design, build, evaluate, secure, deploy và operate một AI-powered product** — không dừng ở prompt hay một API call tới model.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Foundation</strong>&nbsp;Software Engineering first</span>
  <span><strong>Mode</strong>&nbsp;mental model → code → eval → failure → operations</span>
  <span><strong>Target</strong>&nbsp;Production AI Engineer</span>
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

Nó thường gần với:

```text
Authenticated User
      ↓
Product / API
      ↓
Application Contract
      ↓
AI Orchestration / Harness
 ├─ instructions + messages
 ├─ context management
 ├─ provider/model selection
 ├─ structured output
 ├─ retrieval
 ├─ tools + permissions
 ├─ workflow / agent loop
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

# 1. AI Engineer thực sự làm gì?

AI Engineer tập trung vào **application layer của AI**.

Một useful distinction:

| Role | Primary responsibility |
|---|---|
| ML researcher | algorithms/models/training research |
| ML engineer | training/inference/data/model platform lifecycle |
| **AI engineer** | **build product/application around modern models/APIs** |
| AI-assisted developer | dùng coding agents/copilots để build software nhanh hơn |

Một người có thể làm nhiều role, nhưng skill boundary khác nhau.

Production AI Engineer cần:

```text
Software Engineering
+ Backend/API
+ Data/Search
+ Security/AuthZ
+ Distributed Systems
+ Cloud/Deployment
+ Observability
+ LLM Runtime/Context
+ Tools/RAG/Agents
+ Evaluation
```

AI Hero nhấn mạnh đúng một điểm quan trọng: strong software-engineering fundamentals là lợi thế lớn của AI Engineer; role này không đồng nghĩa phải tự train foundation model. Xem [References](references.md).

---

# 2. Learning path mới — học đúng dependency

Module này không bắt đầu bằng RAG hay agent.

```text
Layer 1 — Role + vocabulary
        ↓
Layer 2 — LLM runtime / messages / context / tokens
        ↓
Layer 3 — Structured output + tool calling
        ↓
Layer 4 — Retrieval / RAG
        ↓
Layer 5 — Evaluation / observability / safety
        ↓
Layer 6 — Production architecture / deployment / lifecycle
```

| Chapter | Mục tiêu |
|---|---|
| [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md) | phân biệt model/provider/harness/agent/workflow/tool/memory/eval; chọn đúng abstraction |
| [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md) | hiểu model request thực sự chứa gì, context budget, reasoning trade-off, session/turn/tool loop |
| [Structured Output & Tool Calling](structured-output-and-tool-calling.md) | biến model output thành contract và expose capability có authorization |
| [RAG, Evaluation & Observability](rag-evaluation-and-observability.md) | retrieval lifecycle, ACL, eval regression, AI telemetry |
| [References](references.md) | official/provider sources + supplementary AI Hero learning sources |

Sau Module 19, học [AI Coding Agents](../21-ai-coding-agents/README.md) để hiểu agent hoạt động trên repository/shell/CI với trust boundary khác.

---

# 3. Model ≠ Agent

Đây là distinction quan trọng nhất đang thiếu ở nhiều AI roadmap.

## Model

```text
parameters
+ inference
→ output
```

Model tự nó không có:

```text
filesystem
database
browser
shell
memory across sessions
business authorization
agent loop
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

## Harness

Harness/application quản lý:

```text
system/developer instructions
conversation state
context selection
tool definitions
permission gates
tool execution
retry/timeout
memory/compaction
telemetry
```

Khi một coding agent “đọc file, chạy test, sửa code”, model không tự có những capability đó. Harness expose tools và environment cho model sử dụng.

→ [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md)

---

# 4. Messages và context là runtime input

Model không “nhớ” conversation theo nghĩa application database.

Conceptually mỗi provider request nhận một working set:

```text
instructions
+ user messages
+ selected history
+ retrieved context
+ tool schemas
+ tool results
→ model
```

Context window hữu hạn và có cost.

Bad mental model:

```text
More context = more intelligence
```

Better:

```text
Relevant, current, authorized context
> large noisy context
```

Context quá lớn có thể làm tăng:

- latency;
- cost;
- distraction;
- stale-data risk;
- prompt-injection surface.

→ [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md)

---

# 5. System prompt là instruction, không phải security boundary

System/developer instructions có thể nói:

```text
how to behave
what style to use
which workflow to follow
when to call tools
```

Nhưng không thay được:

```text
Authentication
Authorization
Business validation
Rate limiting
Idempotency
Sandbox
Network policy
Human approval
```

Sai:

```text
System prompt:
"Only admins may refund."
→ model directly controls payment action
```

Đúng:

```text
User identity
→ application AuthZ
→ business rule
→ approval/idempotency
→ payment capability
```

Model có thể đề xuất action. Application quyết định authority.

---

# 6. Parametric knowledge vs contextual knowledge

## Parametric

Knowledge trong model parameters từ training.

```text
may be stale
may be incomplete
not under application control
```

## Contextual

Knowledge application đưa vào current request:

```text
current repo code
current API docs
current business data
current tool result
retrieved policy
```

Production rule:

```text
Current truth comes from current source/context/tool,
not from assuming model memory is current.
```

Ví dụ package/library mới release sau training cutoff: load official docs/source vào context thay vì để model đoán API.

---

# 7. Tokens, reasoning và cost

AI request cost/latency không chỉ tính “số câu”.

```text
Input tokens
+ generated output tokens
+ provider/model-specific reasoning compute/usage
+ extra requests from tool loops
= runtime cost surface
```

Một số reasoning-capable models có effort controls. Higher effort có thể giúp bài khó nhưng thường đổi lấy latency/cost.

Không dùng:

```text
max reasoning everywhere
```

Dùng:

```text
representative eval set
→ compare quality
→ compare P95 latency
→ compare cost
→ choose config
```

Reasoning là một **quality/latency/cost knob**, không phải badge chất lượng.

---

# 8. Business abstraction trước provider abstraction

Provider abstraction hữu ích:

```text
IChatClient
→ provider adapter
```

Nhưng business code không nên lan đầy concept “chat”.

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

Implementation AI đứng sau business port.

```text
Order Application
→ IOrderRiskClassifier
→ AI implementation
→ IChatClient/provider
```

Lợi ích:

- domain không biết model vendor;
- deterministic fake/test dễ hơn;
- model replacement không lan toàn codebase;
- AI capability có thể fallback sang non-AI implementation khi phù hợp.

---

# 9. Structured Output — model output phải vào contract

Business application không nên parse prose mơ hồ.

Fragile:

```csharp
string level = response.Text.Split(':')[1];
```

Better contract:

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

Vẫn cần deterministic validation.

Ví dụ:

```text
JSON schema says amount is decimal
but business rule says refund <= remaining balance
```

Business invariant phải enforce bằng application code.

→ [Structured Output & Tool Calling](structured-output-and-tool-calling.md)

---

# 10. Tool Calling — request ≠ execution

Mental model:

```text
Model
→ structured tool request
→ Harness/Application
→ validate + authorize
→ execute capability
→ tool result
→ next model request
```

Tool request chỉ là output của model.

Một tool production phải trả lời:

1. identity nào đang gọi?
2. permission nào cần?
3. input validate thế nào?
4. side effect gì?
5. retry/idempotency ra sao?
6. timeout = failed hay unknown outcome?
7. audit log ở đâu?
8. human approval có cần không?

Tool tốt:

```text
get_order_status(orderId)
create_support_draft(ticketId)
search_authorized_documents(query)
```

Tool nguy hiểm nếu expose trực tiếp:

```text
execute_arbitrary_sql(sql)
run_shell(command)
```

---

# 11. Workflow vs Agent

Đừng biến mọi flow thành agent.

## Workflow phù hợp khi

```text
steps known
invariants strict
same lifecycle repeats
failure/recovery can be enumerated
```

Ví dụ:

```text
validate
→ retrieve
→ classify
→ validate output
→ save
```

## Agent phù hợp hơn khi

```text
next step phụ thuộc semantic reasoning
path khó enumerate
multiple tools/resources may be explored
```

Ví dụ internal incident assistant:

```text
inspect alert
→ choose relevant logs
→ inspect deploy change
→ inspect dependency health
→ formulate hypothesis
```

Agent vẫn phải bounded bởi:

```text
max turns
tool allowlist
cost budget
time budget
permissions
approval
cancellation
```

---

# 12. RAG là data system

RAG không chỉ:

```text
PDF → embeddings → vector DB → model
```

Production pipeline:

```text
Source of Truth
  ↓
Extract / Normalize
  ↓
Chunk + Metadata
  ↓
ACL / tenant boundary
  ↓
Embed / Index
  ↓
Retrieve / Filter / Rerank
  ↓
Context
  ↓
Generate
  ↓
Citation / Provenance
```

Nó có distributed-data problems:

```text
freshness
deletion
versioning
index migration
tenant isolation
stale derived data
replay/re-index
cost
```

Nếu user không được xem document X, retrieval boundary phải loại X **trước khi model nhìn thấy**.

→ [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)

---

# 13. Evaluation là test system của AI behavior

Không có eval thì thay đổi:

```text
model
prompt
system instruction
retrieval
chunking
reranker
tool description
reasoning effort
```

chỉ được đánh giá bằng cảm giác.

Eval loop:

```text
Representative dataset
      ↓
Baseline configuration
vs
Candidate configuration
      ↓
Quality + Safety
+ Latency + Cost
      ↓
Release decision
```

Phân biệt:

## Deterministic check

```text
schema valid?
AuthZ denied correctly?
expected source retrieved?
API contract valid?
```

## Probabilistic/judgement eval

```text
answer relevance
completeness
groundedness
helpfulness
```

Không dùng LLM judge cho invariant có thể kiểm chắc chắn bằng code.

---

# 14. AI observability

Normal backend vẫn cần:

```text
logs
metrics
traces
```

AI thêm dimensions:

```text
provider/model
request count
input/output usage
reasoning configuration
prompt/config version
retrieval/index version
tool-call count
tool latency
provider latency
time-to-first-token
fallback rate
eval score
cost/request
```

Không log raw prompt/context/tool result mặc định nếu chứa:

```text
PII
secrets
internal documents
credentials
regulated data
```

Telemetry design phải follow data policy.

---

# 15. Timeout, retry và unknown outcome vẫn là Distributed Systems

AI/tool workflows không thoát khỏi distributed-systems rules.

```text
provider timeout
!=
provider definitely did nothing
```

Đặc biệt với write tool:

```text
tool timeout
→ side effect may already have happened
```

Do đó áp dụng:

```text
idempotency
status query
reconciliation
bounded retry
```

Không để agent retry blindly một financial/external side effect.

→ [Distributed Systems](../17-distributed-systems/README.md)

---

# 16. Provider fallback không phải chỉ đổi model name

```text
Provider A fails
→ Provider B
```

nghe đơn giản nhưng có thể khác:

- message/instruction semantics;
- structured-output support;
- tool behavior;
- reasoning controls;
- context window;
- latency/cost;
- safety behavior;
- data residency/compliance.

Fallback path phải có eval riêng.

```text
Primary config
vs
Fallback config
→ same acceptance suite
```

---

# 17. Production architecture trên Azure

Một reference shape:

```text
Front Door / APIM
      ↓
ASP.NET Core API
(App Service / Container Apps / AKS when justified)
      ↓
AI Application Layer
 ├─ Azure OpenAI / provider
 ├─ Azure AI Search / retrieval
 ├─ Azure SQL / state
 ├─ Blob / source documents
 └─ Service Bus / async ingestion/jobs
      ↓
Managed Identity + Key Vault
      ↓
Azure Monitor / App Insights
```

Nhưng service names không phải architecture.

Phải bắt đầu từ:

```text
requirements
latency
availability
data boundary
security
cost
```

rồi mới map sang Azure.

→ [Azure & Platform](../14-cloud/README.md)

---

# 18. Project spine — Enterprise Knowledge & Operations Assistant

Build một assistant có **read-only default**, có AuthZ và eval.

## Functional

```text
login
ask question
retrieve authorized docs
generate cited answer
read order/project status through tool
return structured result
```

## Engineering requirements

- ASP.NET Core API;
- business ports thay vì provider SDK trong domain;
- SQL cho configuration/audit/workflow state;
- authentication + authorization;
- document ingestion/index lifecycle;
- ACL-aware retrieval;
- context budget;
- structured output;
- read-only tools;
- bounded agent/tool loop nếu cần;
- evaluation dataset;
- model/prompt/retrieval versioning;
- tracing + token/cost metrics;
- Docker + Azure deployment;
- CI/CD + rollback;
- PII-safe logging.

## Failure drills

```text
model timeout
large/noisy context
stale model knowledge
retrieval unavailable
stale index after document deletion
unauthorized document retrieval attempt
prompt injection in retrieved content
malformed structured output
wrong tool selected
tool API timeout
provider outage
reasoning/cost spike
runaway tool loop
```

## Evidence

```text
architecture diagram
trust-boundary diagram
OpenAPI
schema/index design
AuthZ negative tests
context/token budget report
RAG eval report
reasoning config comparison
latency/cost dashboard
failure drill report
ADR: provider/search/hosting/agent-vs-workflow
```

---

# 19. Learning priorities

## P0 — foundation

- AI Engineer role/system boundary;
- model vs provider vs harness vs agent;
- messages/context/tokens;
- parametric vs contextual knowledge;
- system prompt limitations;
- structured output;
- tool calling + AuthZ;
- evaluation;
- timeout/cost/observability;
- deploy/operate.

## P1 — production capability

- embeddings/search/RAG lifecycle;
- reranking;
- context management/compaction;
- agent loops and stopping conditions;
- human-in-the-loop;
- fallback/routing;
- prompt/model/index versioning;
- async AI jobs;
- semantic caching where justified.

## P2 — role/project dependent

- fine-tuning;
- custom model hosting;
- training infrastructure;
- advanced MLOps;
- GPU capacity planning.

---

# 20. Common mistakes

## “Model = Agent”

Sai boundary; model không tự có tools/memory/environment.

## “System prompt = security”

Instruction không enforce authorization.

## “More context is always better”

Noise, latency, cost và attack surface tăng.

## “Agent for every workflow”

Deterministic code thường tốt hơn khi steps đã biết.

## “RAG = vector DB”

Bỏ quên source lifecycle, ACL, deletion và eval.

## “Structured output = correct output”

Schema validation không enforce business invariant.

## “Retry fixes timeout”

Write side effect có thể bị duplicate/unknown outcome.

## “Prompt tweak without eval”

Không biết regression.

## “Raw prompt logging”

Có thể leak PII/secrets/internal data.

## “Max reasoning by default”

Đốt latency/cost mà không chứng minh quality gain.

---

# 21. Production checklist

- [ ] AI capability gắn với business outcome rõ;
- [ ] business/application boundary không phụ thuộc trực tiếp provider SDK;
- [ ] phân biệt model/provider/harness/agent/workflow;
- [ ] messages/context construction rõ;
- [ ] token/context budget có metric;
- [ ] system prompt không dùng làm AuthZ boundary;
- [ ] structured output + business validation;
- [ ] tools capability-oriented, có AuthZ/idempotency;
- [ ] agent loop bounded nếu dùng;
- [ ] retrieval respects ACL/deletion/versioning;
- [ ] representative eval suite tồn tại;
- [ ] reasoning/model config được benchmark;
- [ ] PII-safe telemetry;
- [ ] latency/usage/cost/fallback metrics;
- [ ] timeout/cancellation/reconciliation rõ;
- [ ] provider fallback tested;
- [ ] deployment + rollback;
- [ ] failure drills documented;
- [ ] project runnable/demoable.

---

# 22. Exit Criteria

Bạn hoàn thành Module 19 khi có thể:

1. giải thích model/provider/harness/agent bằng một architecture cụ thể;
2. giải thích messages/context/session/turn/provider-request flow;
3. phân biệt parametric và contextual knowledge;
4. thiết kế token/context budget;
5. chọn workflow vs agent và giải thích trade-off;
6. implement structured output có deterministic validation;
7. implement tool có AuthZ và idempotency phù hợp;
8. thiết kế RAG có ACL/deletion/versioning;
9. xây eval suite phân biệt deterministic checks và judgement metrics;
10. đo quality/latency/cost giữa model/reasoning configs;
11. xử lý provider/tool timeout theo distributed-systems semantics;
12. deploy và observe AI feature như một production subsystem.

## Học tiếp

1. [AI Engineer Vocabulary & System Boundaries](ai-engineer-vocabulary-and-system-boundaries.md)
2. [LLM Runtime — Messages, Context, Tokens & Reasoning](llm-runtime-messages-context-and-reasoning.md)
3. [Structured Output & Tool Calling](structured-output-and-tool-calling.md)
4. [RAG, Evaluation & Observability](rag-evaluation-and-observability.md)
5. [AI Coding Agents](../21-ai-coding-agents/README.md)
6. [System Design](../24-system-design/README.md)
7. [Software Architecture](../25-software-architecture/README.md)
8. [References](references.md)

## Verification metadata

- Rebuilt: 2026-09-03.
- Supplementary learning sources: AI Hero — AI Engineer Roadmap, AI Coding Dictionary, LLM Fundamentals.
- Provider/version-specific semantics: current official docs are canonical; see [References](references.md) and [Technology Baseline](../00-roadmap/technology-baseline.md).
