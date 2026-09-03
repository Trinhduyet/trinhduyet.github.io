# AI Engineering Vocabulary & System Boundaries

> Mục tiêu: có một vocabulary đủ rộng để đọc tài liệu AI hiện đại mà không trộn `model`, `provider`, `agent`, `context`, `memory`, `RAG`, `tool`, `eval` thành một box “AI”. Định nghĩa được ưu tiên theo **system boundary và production consequence**, không phải học thuộc buzzword.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Mode</strong>&nbsp;definition → boundary → production implication</span>
  <span><strong>.NET mapping</strong>&nbsp;Microsoft.Extensions.AI</span>
</div>

## Hiểu trong 5 phút

Một AI-powered application production:

```text
Business capability
      ↓
Application / Harness
 ├─ instructions
 ├─ messages
 ├─ context
 ├─ retrieval
 ├─ tools
 ├─ permissions
 ├─ workflow / agent loop
 ├─ eval
 └─ telemetry
      ↓
Provider
      ↓
Model
```

Các khái niệm quan trọng nhất:

```text
Model != Provider
Model != Agent
Context != Context Window
Session != Memory
Tool Call != Tool Execution
System Prompt != Authorization
RAG != Vector Database
Structured Output != Business Correctness
Eval != Unit Test only
Reasoning Effort != Always Better
```

---

# 1. AI Engineer là ai?

## AI Engineer

Software engineer xây **application có AI capability**.

Typical responsibilities:

```text
model/provider integration
application architecture
context engineering
structured output
tool calling
RAG/data retrieval
evaluation
security/privacy
latency/cost
observability
deployment/reliability
```

AI Engineer không nhất thiết train foundation model.

## ML Engineer

Tập trung nhiều hơn vào:

```text
training/fine-tuning pipelines
features/data pipelines
model serving
model optimization
ML infrastructure
```

Boundary hữu ích:

```text
ML Engineer
→ builds/operates model-serving capability

AI Engineer
→ builds products using model capability
```

Thực tế organization có thể overlap, nhưng mental model này giúp chọn learning path.

## AI-assisted Developer

Developer dùng coding agents/assistants để build software nhanh hơn.

```text
AI Engineer
→ builds AI-powered product

AI-assisted Developer
→ uses AI to build any product
```

Một người có thể là cả hai.

## Prompt Engineering

Thiết kế instructions/examples/input để influence model output.

Đây là **một kỹ năng** trong AI Engineering, không phải toàn bộ discipline.

Production AI còn cần:

```text
contracts
data
AuthZ
evals
failures
observability
operations
```

---

# 2. Model fundamentals

## Model

Tập parameters/weights dùng để thực hiện inference.

```text
input tokens
→ model
→ output token probabilities
→ generated tokens
```

Model tự nó không có:

```text
filesystem
HTTP API access
business database
persistent memory
agent loop
authorization
```

## Parameters / Weights

Các số được tối ưu trong training và giữ cố định trong inference thông thường.

Parametric knowledge được encode trong parameters theo cách distributed, không phải một table facts đơn giản.

## Training

Quá trình điều chỉnh parameters bằng data/objective trước khi model được serve.

```text
training data
→ optimization
→ parameters
```

Một user conversation không tự update weights của model.

## Pre-training

Large-scale training tạo general language/world/code capability.

## Post-training

Các bước sau pre-training nhằm cải thiện behavior như instruction following, preference alignment, safety hoặc specialized behavior.

Không cần deep ML math để trở thành AI Engineer, nhưng phải hiểu training != runtime context.

## Inference

Chạy trained model với input hiện tại để sinh output.

```text
parameters fixed
+ current context
→ generated output
```

## Next-token prediction

Core generation mechanism: dự đoán token tiếp theo dựa trên current sequence, append token, lặp lại.

Production implication:

```text
plausible output
!= guaranteed truth
```

## Token

Atomic unit model đọc/ghi.

Token không đồng nhất với word.

Token count ảnh hưởng:

```text
context capacity
latency
usage/cost
provider limits
```

## Tokenizer

Rule/model-specific mechanism biến text ↔ token IDs.

Không estimate chính xác cost bằng `word count` khi cần production accounting.

## Non-determinism

Same logical input có thể không luôn trả byte-identical output.

Nguyên nhân có thể liên quan sampling/runtime/provider behavior.

Engineering implication:

```text
assert invariant/contract
not exact prose
```

trừ khi exact output thật sự là requirement.

---

# 3. Provider và model-serving runtime

## Model Provider

Service/runtime serve model inference.

Examples conceptual:

```text
hosted provider
cloud provider AI service
local inference runtime
```

Provider concern:

```text
model IDs/deployments
context limits
pricing
rate limits
availability
regions
data retention
API semantics
reasoning/tool support
```

## Model Provider Request

Một round-trip từ application/harness tới provider.

```text
context/options
→ provider request
→ provider response
```

Một user turn có thể cần nhiều provider requests nếu có tools/agent loop.

## Input Tokens

Tokens gửi vào provider request.

Có thể gồm:

```text
instructions
history
retrieved context
tool definitions
tool results
current user message
```

## Output Tokens

Tokens model sinh ra.

Một số provider tính output/reasoning usage khác input.

## Cached Input Tokens

Provider có thể reuse/cache một prefix đã xử lý và report/bill riêng.

Không assume mọi provider/model có cùng cache semantics.

## Prefix Cache

Provider-side optimization cho shared input prefix.

Useful khi repeated requests share large stable prefix, nhưng architecture không nên phụ thuộc correctness vào cache hit.

## Rate Limit

Provider constraint trên request/token/concurrency/resource usage.

Production cần:

```text
backpressure
retry policy
queueing
fallback/degraded mode
usage budget
```

---

# 4. Reasoning concepts

## Reasoning Effort / Effort

Runtime configuration cho một số reasoning-capable models/providers, điều chỉnh amount of reasoning compute/usage.

Mental model:

```text
more effort
→ potentially higher quality on hard tasks
→ usually more latency/usage/cost
```

Không phải mọi task cần high effort.

## Reasoning Tokens / Reasoning Usage

Provider có thể report token/usage liên quan reasoning separately.

Không đồng nhất với visible explanation.

```text
hidden/internal reasoning work
!=
user-visible chain-of-thought
```

Application quan tâm observable contract:

```text
correctness
structured output
tool behavior
latency
usage/cost
```

## Reasoning Configuration

Một release variable giống:

```text
model
prompt
retrieval
chunking
```

nên phải eval trước khi rollout.

---

# 5. Harness, workflow và agent

## Harness

Software bao quanh model để tạo usable application behavior.

Harness có thể quản lý:

```text
system/developer instructions
message history
context assembly
retrieval
tools
tool execution
permissions
retry/timeout
memory
compaction
telemetry
```

## Workflow

Application-controlled sequence của steps.

```text
validate
→ retrieve
→ classify
→ validate output
→ save
```

Nếu path known, workflow thường dễ test và govern hơn autonomous agent loop.

## Agent

Model hoạt động trong harness với context/tools/control loop để chọn next action qua nhiều steps.

```text
Agent
= model
+ harness
+ context
+ tools
+ control loop
```

Agent là system behavior, không phải model tier.

## Agent Loop

```text
goal
→ model chooses action
→ tool/request
→ environment/result
→ model updates plan
→ repeat/finish
```

Production loop phải bounded:

```text
max turns
max tool calls
max provider requests
time budget
cost budget
allowed tools
approval gates
```

## Subagent

Agent phụ chạy subtask trong context/session riêng rồi trả result về parent workflow/agent.

Useful cho:

```text
parallel research
specialized review
bounded investigation
```

Nhưng tăng:

```text
cost
context-transfer loss
permission complexity
audit complexity
```

---

# 6. Message, instruction và conversation runtime

## Message

Typed item/conversation unit application gửi provider/model.

Common roles/concepts có thể gồm:

```text
system/developer instruction
user
assistant
function/tool call
function/tool result
```

Exact role names/precedence là provider/API-specific.

## System Prompt / System Instruction

Standing behavior instructions đặt ở high-priority application layer theo provider semantics.

Useful cho:

```text
role
style
workflow constraints
output rules
tool guidance
```

Không phải security boundary.

## Developer Instruction

Một số provider/API có explicit developer-level instruction role/surface.

Mental model:

```text
application-owned behavior instruction
```

Exact precedence phải đọc current official provider docs.

## User Message

Current user intent/input.

User-provided content là untrusted input đối với security boundaries.

## Assistant Message

Prior/current model-generated output đưa vào conversation state khi application/harness cần.

---

# 7. Context, context window, session và turn

## Context

Thông tin relevant model/agent có cho task hiện tại.

Context quality là về relevance/correctness/currentness.

## Context Window

Literal finite token sequence model nhìn thấy trong **mỗi provider request**.

Có thể chứa:

```text
instructions
history
retrieved docs
tool schemas
tool results
examples
current request
```

## Context Budget

Application policy phân bổ finite context capacity cho từng category.

Ví dụ:

```text
instructions        5%
history            20%
retrieval          50%
tool definitions   10%
output headroom    15%
```

Tỷ lệ thực tế phụ thuộc use case.

## Session

Bounded interaction state do application/harness quản lý qua nhiều turns.

Session policy có thể gồm:

```text
retention
expiry
history selection
compaction
privacy/deletion
```

## Turn

Một user input cộng toàn bộ system work trước khi yield response/control lại user.

Một turn có thể chứa nhiều provider requests/tool calls.

## Stateless

Không carry information forward tự động.

Model inference thường được mental-model như stateless across requests; harness/provider API có thể offer stateful convenience, nhưng application phải hiểu state ownership.

## Stateful

System giữ information qua requests/turns/sessions.

Stateful agent behavior đến từ harness/provider state/memory, không phải weights tự thay đổi.

---

# 8. Context quality và attention degradation

## Attention

Mechanism giúp model relate tokens/context trong inference.

AI Engineer không nhất thiết học toàn transformer math để ship app, nhưng phải hiểu practical consequence:

```text
more tokens
!= unlimited focus
```

## Attention Degradation

Long/noisy context có thể làm model sử dụng relevant information kém hơn trước khi hard context limit bị chạm.

Symptoms:

```text
forgets earlier constraint
confuses versions
misses relevant retrieved chunk
repeats stale plan
```

## Smart Zone / Dumb Zone

AI Hero dùng mental model:

```text
early focused context
→ smart zone

long/noisy/degraded session
→ dumb zone
```

Đây là explanatory metaphor, không phải formal provider metric.

Mitigation:

```text
progressive disclosure
context pruning
fresh retrieval
compaction
handoff
fresh session
```

---

# 9. Knowledge vocabulary

## Parametric Knowledge

Knowledge encoded in model parameters từ training.

Properties:

```text
frozen for deployed model
may be stale
not application-controlled
```

## Contextual Knowledge

Knowledge application đặt trong current context:

```text
current policy
current code
DB/tool result
retrieved docs
current API docs
```

Đây là lever AI Engineer kiểm soát trực tiếp.

## Knowledge Cutoff

Temporal boundary của parametric knowledge.

Current/recent truth phải đến từ contextual source/tool.

## Primary Source

Artifact gốc/authoritative:

```text
actual code
current DB result
official docs
raw event
system-of-record record
```

## Secondary Source

Summary/account của primary source:

```text
summary
handoff note
compiled knowledge article
```

Secondary source tiết kiệm context nhưng lossy.

Rule:

```text
navigate with secondary source
verify critical truth with primary source
```

---

# 10. Retrieval & RAG vocabulary

## Retrieval

Process tìm relevant source/data cho current query/task.

Không nhất thiết dùng vector search; có thể là:

```text
SQL lookup
keyword/BM25
vector search
hybrid search
graph lookup
API/tool call
```

## RAG — Retrieval-Augmented Generation

```text
query
→ retrieve authorized relevant data
→ add to context
→ generate answer/action
```

RAG là data/retrieval lifecycle, không chỉ vector database.

## Chunk

Unit tài liệu được index/retrieved.

Chunk design ảnh hưởng:

```text
retrieval precision
context size
citation granularity
```

## Embedding

Vector representation của input dùng cho similarity/retrieval.

## Embedding Model

Model tạo embedding vectors.

Change embedding model có thể yêu cầu:

```text
re-embed
index migration
retrieval re-evaluation
```

## Vector Store / Vector Index

Storage/index hỗ trợ nearest-neighbor/vector retrieval.

Không thay:

```text
source of truth
ACL
metadata filtering
freshness/deletion lifecycle
```

## Hybrid Search

Kết hợp lexical + semantic/vector retrieval.

## Reranking

Scoring/reordering candidate results sau initial retrieval để chọn context tốt hơn.

## Grounding

Model answer dựa trên supplied/verified sources thay vì chỉ parametric recall.

## Citation / Provenance

Metadata cho biết statement/result dựa trên source nào.

Citation không tự chứng minh source đúng; nó hỗ trợ traceability.

---

# 11. Tool vocabulary

## Tool

Capability harness expose cho model/agent.

Examples:

```text
get_order_status
search_authorized_documents
create_support_draft
```

## Tool Schema

Machine-readable contract cho:

```text
name
description
arguments/types
```

Schema tốt giúp model chọn/gọi tool đúng nhưng không enforce business authorization.

## Tool Call

Structured model output đề xuất gọi tool.

```text
tool name + arguments
```

## Tool Execution

Application/harness thực sự invoke capability.

Đây là side-effect/security boundary.

## Tool Result

Execution output đưa lại model/harness.

Tool result là untrusted data nếu source không trusted.

## Tool Host

Application component quản lý:

```text
registry
validation
AuthZ
execution
logging
budgets
```

## Read Tool

Không intentional side effect ngoài observation.

Vẫn có data-access/AuthZ risk.

## Write Tool

Có side effect:

```text
send email
create ticket
refund payment
change config
```

Cần:

```text
idempotency
approval
audit
unknown-outcome strategy
```

## MCP

Protocol để AI application/harness kết nối external tool/data servers.

MCP là integration protocol, không phải authorization/safety guarantee.

---

# 12. Structured output vocabulary

## Structured Output

Model response constrained/parsed vào defined data shape/schema.

```text
prose
→ unreliable parsing

schema/typed output
→ stronger application contract
```

## Schema Validation

Output đúng structural constraints.

## Business Validation

Output đúng domain invariants.

```text
schema valid
!=
business valid
```

## Function Calling / Tool Calling

Provider/model mechanism để model emit structured tool requests thay vì trực tiếp execute external action.

---

# 13. Memory, clearing, compaction và handoff

## Memory System

Mechanism persist selected state/knowledge across sessions rồi reload when relevant.

Memory cần lifecycle:

```text
selection
source
expiry
update
deletion
privacy
```

## Conversation History

Prior messages/tool results. History không tự là long-term memory architecture.

## Clearing

Start fresh session/context state.

## Compaction

Summarize old session/history để free context headroom.

Compaction là lossy.

## Autocompact

Harness automatically triggers compaction near its context policy threshold.

## Handoff

Transfer task context sang session/agent khác.

## Handoff Artifact

Durable summary/spec/ticket dùng để carry task state.

Critical facts nên link tới primary source.

## Progressive Disclosure

Load only context needed now; keep pointers to deeper detail.

## Context Pointer

Reference từ high-level context tới detail source/load-on-demand material.

---

# 14. Evaluation vocabulary

## Eval / Evaluation

System đo AI behavior trên representative dataset/tasks.

```text
input cases
→ run candidate
→ metrics/judgement
→ compare baseline
→ release decision
```

## Eval Dataset

Representative cases phản ánh production behavior/risk.

## Baseline

Current/reference system result để candidate so sánh.

## Candidate

New model/prompt/retrieval/tool/config cần evaluate.

## Deterministic Check

Pass/fail bằng code:

```text
schema valid
AuthZ denied
expected document retrieved
tool not called
```

## LLM-as-Judge / AI Evaluator

Model được dùng để judge semantic quality.

Useful cho:

```text
relevance
groundedness
style/completeness
```

Nhưng probabilistic.

Không dùng AI judge để thay deterministic check khi invariant có thể encode bằng code.

## Human Evaluation

Human judgement trên sample/behavior.

## Regression Eval

Eval suite chạy trước release/change để phát hiện quality/safety degradation.

---

# 15. Observability vocabulary

## Trace

Causal/request execution path qua application/provider/tools/retrieval.

## Span

Timed operation trong trace.

## AI Telemetry

Metadata mở rộng cho AI calls:

```text
provider/model
prompt/config version
input/output/reasoning usage
retrieval/index version
tool count/latency
eval score
cost
```

## Usage

Provider/application counters như token counts/request counts.

## Cost per Request / Task

Economic metric cần theo dõi cùng quality/latency.

## Prompt Version

Identity/version của instruction template/system prompt để correlate behavior/regression.

## Model Version / Deployment

Identity của model/deployment config được release.

## Retrieval Version

Identity của index/chunking/embedding/reranking configuration.

---

# 16. Safety, security và governance vocabulary

## Guardrail

Control giảm risk ở một boundary cụ thể.

Examples:

```text
schema validation
content filtering
AuthZ
tool allowlist
rate limit
sandbox
human approval
```

Không có single universal guardrail.

## Prompt Injection

Untrusted content cố chuyển data thành instructions để steer model/harness.

Mitigation không chỉ prompt wording; cần:

```text
authority separation
AuthZ
tool restrictions
sandbox
context policy
```

## Direct Prompt Injection

Malicious instruction đến trực tiếp từ user input.

## Indirect Prompt Injection

Malicious instruction nằm trong retrieved/web/tool content mà model đọc.

## Context Poisoning

Bad/stale/malicious context làm behavior sai.

## Least Privilege

Mỗi tool/identity chỉ có permissions cần thiết.

## Human Approval

Human authorization step trước high-risk action.

Approval không thay deterministic business rules.

## Audit Log

Record traceable về:

```text
who
what request
which model/tool
what action
what result
when
```

---

# 17. Failure modes

## Hallucination

Confidently wrong/unfounded output.

Useful split:

```text
factual hallucination
→ invented fact/API

faithfulness failure
→ contradicts supplied context/source
```

## Sycophancy

Model agrees with user assumption despite contrary evidence.

## Stale Knowledge

Parametric knowledge outdated relative to current world/system.

## Attention Degradation

Long/noisy context reduces effective use of relevant information.

## Tool Misuse

Model chooses/calls a tool inappropriately.

Even schema-valid call can violate business intent.

## Runaway Loop

Repeated reasoning/tool calls without proportional progress.

## Unknown Outcome

Timeout/disconnect xảy ra nhưng side effect có thể đã happened.

Common với write tools/external APIs.

Mitigation:

```text
idempotency
status query
reconciliation
```

## Provider Failure

```text
timeout
rate limit
service unavailable
invalid response
capability mismatch
```

Need classified retry/fallback/degraded behavior.

---

# 18. Prompt and context patterns

## Few-shot Example

Example input/output included in context để steer behavior.

## Zero-shot

Task instruction không có explicit examples.

## Prompt Template

Parameterized instruction/messages built by application.

## Context Engineering

Discipline chọn, structure, order và manage runtime information cho model/agent.

Includes:

```text
instructions
retrieval
history
examples
tool schemas
summaries
context budget
```

Context engineering thường có leverage lớn hơn việc chỉ “viết prompt dài hơn”.

---

# 19. Production patterns

## Fallback

Alternative path khi primary provider/model/tool unavailable hoặc quality threshold fail.

Could be:

```text
secondary model
simpler deterministic workflow
human escalation
cached safe answer
```

## Degraded Mode

Reduced capability nhưng system vẫn usable/safe.

## Human-in-the-loop

Human participates in decision/review/approval loop.

## Idempotency

Repeated same logical write request does not create duplicate effect.

## Reconciliation

Later process checks actual state và repairs/settles unknown/inconsistent outcome.

## Backpressure

System limits/adapts incoming work khi downstream/provider capacity saturated.

---

# 20. .NET mapping — Microsoft.Extensions.AI

Trong .NET track:

| Concept | Microsoft.Extensions.AI mapping |
|---|---|
| provider-neutral chat client | `IChatClient` |
| messages | `ChatMessage` |
| request options | `ChatOptions` |
| response | `ChatResponse` |
| typed output | `ChatResponse<T>` helpers |
| tool/function | `AIFunction` |
| function creation | `AIFunctionFactory` |
| tool loop | function-invocation client/pipeline |
| client composition | `ChatClientBuilder` |
| usage | `UsageDetails` |
| reasoning config | `ReasoningOptions` |
| embeddings | `IEmbeddingGenerator<...>` |
| eval integration | `Microsoft.Extensions.AI.Evaluation` packages |

MEAI là **technical integration abstraction**, không phải:

```text
business domain interface
provider itself
security boundary
agent framework by default
```

→ [Microsoft.Extensions.AI — .NET Integration Guide](microsoft-extensions-ai-dotnet-integration.md)

---

# 21. Một architecture được label đúng

```text
Customer Support Assistant
```

Map:

```text
Business Port
= ICustomerSupportAssistant

Harness
= ASP.NET Core orchestration/application service

Technical AI abstraction
= Microsoft.Extensions.AI IChatClient

Provider
= Azure OpenAI / OpenAI / other provider

Model
= selected model deployment

Context
= system instructions + current user message + authorized policy docs

RAG
= policy retrieval pipeline

Tool
= get_order_status

Tool Call
= model requests get_order_status(orderId)

Tool Execution
= backend validates AuthZ and calls order service

Tool Result
= authorized order status DTO/text

Memory
= selected durable user preference/session summary if needed

Eval
= support QA dataset + groundedness/accuracy/safety metrics

Workflow
= deterministic escalation path

Agent
= optional bounded open-ended tool loop
```

Nếu diagram chỉ có một box `AI`, chưa đủ architecture detail.

---

# 22. Vocabulary ownership giữa Module 19 và 21

## Module 19 owns

```text
AI Engineer role
model/provider/runtime
messages/context/tokens/reasoning
Microsoft.Extensions.AI
structured output/tools
RAG/embeddings
evals/observability
business AI security/reliability
```

## Module 21 owns

```text
coding-agent environment
filesystem/shell/Git tools
permission/agent modes
sandbox
repository instructions
progressive disclosure
handoffs/compaction
skills/subagents
coding-agent verification/review
AX/DX
```

→ [AI Coding Agents](../21-ai-coding-agents/README.md)

---

# 23. Quick definition table

| Term | Short definition |
|---|---|
| AI Engineer | software engineer building AI-powered applications |
| Model | trained parameters performing inference |
| Provider | service/runtime serving model |
| Inference | running model on current input |
| Token | atomic model input/output unit |
| Harness | software around model managing context/tools/policy |
| Agent | model + harness + tools + loop |
| Workflow | application-controlled sequence of steps |
| Context | relevant information available now |
| Context window | literal finite tokens model sees per request |
| Session | bounded interaction state |
| Turn | one user interaction including multiple model/tool steps |
| System prompt | high-priority behavioral instruction, not AuthZ |
| Parametric knowledge | knowledge encoded in model weights |
| Contextual knowledge | knowledge supplied at runtime |
| RAG | retrieve authorized data then generate with it |
| Embedding | vector representation for similarity/search |
| Tool | capability exposed to model/agent |
| Tool call | model proposal to invoke tool |
| Tool result | execution result returned into context |
| Structured output | model output constrained/parsed to schema/type |
| Memory | persisted selected state across sessions |
| Eval | dataset + metrics/judgement + threshold/release decision |
| Guardrail | control reducing risk at one boundary |
| Hallucination | confidently wrong/unfounded output |
| Sycophancy | unjustified agreement with user assumption |
| Prompt injection | untrusted content trying to become instruction |
| Context poisoning | harmful/stale/noisy context degrading behavior |
| Compaction | lossy history summary to free context capacity |
| Handoff | transfer task context to another session/agent |

---

# Exit Criteria

Bạn đạt chapter này khi có thể:

- [ ] phân biệt AI Engineer / ML Engineer / AI-assisted Developer;
- [ ] giải thích parameters/training/inference/next-token prediction;
- [ ] phân biệt provider/model/provider request;
- [ ] giải thích input/output/cached tokens và reasoning usage;
- [ ] phân biệt harness/workflow/agent/subagent;
- [ ] phân biệt message/system instruction/context/context window/session/turn;
- [ ] phân biệt parametric/contextual knowledge + knowledge cutoff;
- [ ] giải thích RAG/retrieval/embedding/vector index/reranking;
- [ ] phân biệt tool/tool call/tool execution/tool result/MCP;
- [ ] phân biệt structured/schema/business validation;
- [ ] giải thích memory/compaction/handoff/progressive disclosure;
- [ ] thiết kế eval với deterministic checks + semantic evaluation;
- [ ] nhận diện hallucination/sycophancy/prompt injection/context poisoning/unknown outcome;
- [ ] map concepts sang `Microsoft.Extensions.AI`;
- [ ] vẽ architecture mà không dùng một box “AI” mơ hồ.

## Sources

Xem [References](references.md). AI Hero được dùng như supplementary vocabulary/mental-model source; official Microsoft/provider documentation quyết định version-sensitive runtime/API semantics.

## Verification metadata

- Expanded: 2026-09-03.
- Vocabulary split intentionally between Module 19 (AI application engineering) and Module 21 (coding-agent operating environment).
