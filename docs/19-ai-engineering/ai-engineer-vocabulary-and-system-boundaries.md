# AI Engineer Vocabulary và System Boundaries

> Mục tiêu: dùng đúng từ để design đúng system. Khi `model`, `agent`, `tool`, `context`, `memory`, `workflow`, `eval` bị trộn thành một chữ “AI”, architecture review rất dễ sai boundary.

## 1. AI Engineer là ai?

AI Engineer là software engineer xây **application có AI capability**.

Không đồng nhất với:

```text
ML Researcher
ML Engineer
Data Scientist
AI-assisted Developer
Prompt-only specialist
```

Một AI Engineer production thường làm việc ở giao điểm:

```text
Software Engineering
+ Product/Application Engineering
+ LLM/AI capabilities
+ Data/Retrieval
+ Evaluation
+ Security/Operations
```

Điểm quan trọng từ góc nhìn nghề nghiệp:

```text
You do not need to train a foundation model
but you do need to engineer the system around one.
```

Bạn cần đủ mạnh ở:

- backend/API;
- data/storage/search;
- authentication/authorization;
- distributed failure;
- deployment/cloud;
- observability;
- evaluation;
- AI-specific context/tool/retrieval behavior.

AI Hero mô tả vai trò này như software developer xây application powered by AI và nhấn mạnh khác biệt với AI-assisted developer — người dùng AI để code nhanh hơn. Module này giữ distinction đó vì hai role có objective và risk khác nhau.

---

# 2. Vocabulary map

## Model

Parameters/weights thực hiện inference.

```text
input tokens
→ model
→ next-token generation / structured output
```

Model không tự có filesystem, database, shell hay long-term memory.

## Provider

Service/runtime serve model.

Provider-specific concerns:

```text
model availability
rate limits
context limit
pricing
data policy
region
API semantics
tool/reasoning capability
```

## Harness

Software biến model thành usable application actor:

```text
instructions
context assembly
tool execution
permissions
session state
memory
retry/timeout
telemetry
```

## Agent

Một model được harness với context/tools/loop để thực hiện task qua nhiều bước.

```text
Agent
= model
+ harness
+ context
+ tools
+ control loop
```

Agent không phải “model mạnh hơn”. Nó là **runtime composition** khác.

## Workflow

Flow deterministic hoặc mostly deterministic do application điều khiển.

```text
step A
→ step B
→ step C
```

Nếu sequence đã biết trước, workflow thường đơn giản, dễ test và an toàn hơn để model tự chọn mọi bước.

## Tool

Capability mà harness expose cho model/agent đề xuất sử dụng.

Tool nên map tới business capability:

```text
get_order_status
create_support_draft
search_architecture_docs
```

không phải unrestricted primitive:

```text
run_any_sql
execute_shell_as_admin
```

## Tool call

Structured request do model tạo ra.

## Tool result

Kết quả application gửi lại cho model sau khi tool được execute.

## Context

Information model nhìn thấy trong current request.

## Context window

Finite working set chứa messages, instructions, retrieved data, tool definitions/results và output budget theo provider/model semantics.

## Memory

Mechanism application dùng để persist/reload selected information across sessions.

Không nên gọi transcript dài là “memory architecture” nếu không có lifecycle, selection, deletion và source-of-truth rules.

## RAG

Retrieval-Augmented Generation:

```text
query
→ retrieve relevant authorized data
→ inject context
→ generate
```

RAG là data/retrieval system, không chỉ vector DB.

## Embedding

Vector representation dùng cho similarity/search use cases. Không phải replacement cho relational data model hay authorization.

## Eval

Một cách đo AI behavior bằng dataset + metric/judgement + threshold.

```text
change
→ run eval
→ compare baseline
→ release decision
```

## Guardrail

Control giảm risk ở một boundary cụ thể.

Examples:

```text
schema validation
content filter
authorization
tool allowlist
rate limit
human approval
```

Không có “one guardrail solves AI safety”.

---

# 3. Parametric vs contextual knowledge

Đây là vocabulary cực quan trọng cho AI Engineer.

```text
Parametric knowledge
= what model learned during training

Contextual knowledge
= what application puts in front of model now
```

Production implication:

```text
current repository/API/business state
must come from current context/tool/source of truth
not from model memory assumptions
```

Nếu model không biết internal SDK:

```text
read current SDK docs/code
```

không phải:

```text
hope the model recalls it correctly
```

---

# 4. Agent vs workflow — chọn đúng abstraction

## Deterministic workflow phù hợp khi

```text
steps known
business invariants strict
failure handling explicit
same inputs should produce controlled behavior
```

Ví dụ:

```text
validate request
→ query order
→ calculate eligibility
→ persist decision
```

Không cần agent.

## Agent phù hợp hơn khi

```text
task open-ended
next action depends on semantic interpretation
multiple tools may be needed
path cannot be enumerated cheaply
human-like investigation is useful
```

Ví dụ:

```text
investigate production incident
→ inspect logs
→ inspect deploy diff
→ inspect metrics
→ choose next diagnostic step
```

Ngay cả agent vẫn cần bounded tools, permissions và stopping criteria.

---

# 5. Agent control loop

Một tool-using loop:

```text
Goal
 ↓
Model chooses next action
 ↓
Tool request
 ↓
Application validates/executes
 ↓
Tool result
 ↓
Model updates plan
 ↓
repeat or finish
```

Production limits:

```text
max turns
max tool calls
max cost
max wall-clock time
allowed tools
approval gates
cancellation
```

Nếu không có bounds, một seemingly simple user request có thể tạo runaway cost/latency hoặc repeated side effects.

---

# 6. Human-in-the-loop không chỉ là nút Approve

Có nhiều mức human involvement:

```text
review before execution
approve sensitive tool
review generated artifact
resolve ambiguity
review low-confidence result
manual reconciliation after unknown outcome
```

Human review phù hợp khi judgement/value at risk cao.

Nhưng đừng dùng human approval để che một architecture thiếu deterministic validation.

Ví dụ amount limit vẫn phải enforce bằng code dù có human review.

---

# 7. Automated check vs AI review

Hai loại evidence khác nhau.

## Automated check

Deterministic:

```text
unit tests
schema validation
compiler
lint
policy check
AuthZ test
```

## AI/LLM review

Probabilistic judgement:

```text
answer quality
style
semantic relevance
architecture critique
```

AI Engineer cần ưu tiên deterministic check cho invariant có thể encode.

Không dùng LLM judge để kiểm thứ mà code/test có thể xác định chắc chắn.

---

# 8. AI Engineer vs AI-assisted Developer

## AI Engineer

Builds:

```text
AI-powered product capability
model integration
RAG
tools/agents
evals
AI operations
```

## AI-assisted Developer

Uses:

```text
Codex
Claude Code
Copilot
Cursor
other coding agents
```

để build software nhanh hơn.

Một người có thể là cả hai, nhưng competency khác nhau.

```text
AI Engineer risk
→ model behavior, retrieval, tool authority, eval, cost

AI-assisted developer risk
→ wrong code, repo permissions, shell/secrets, CI/supply chain
```

Module 21 tập trung vế thứ hai.

---

# 9. Failure vocabulary

## Hallucination

Output tự tin nhưng sai/không grounded.

## Non-determinism

Same logical input có thể không luôn sinh byte-identical output.

Implication:

```text
assert contract/invariant
not exact prose unless required
```

## Knowledge cutoff / stale knowledge

Model parametric knowledge có temporal boundary.

## Prompt injection

Untrusted content cố tác động model thành instruction.

## Context poisoning

Bad/stale/malicious context làm decision degrade.

## Tool misuse

Model chọn đúng schema nhưng action không phù hợp business/security.

## Sycophancy

Model đồng ý với assumption thay vì challenge evidence.

## Runaway loop

Agent tiếp tục tool/reasoning iterations không tạo progress tương xứng.

---

# 10. AX — Agent Experience

AI coding dictionary dùng khái niệm Agent Experience để nói về environment giúp agent làm việc tốt.

Đây là một insight hữu ích hơn “prompt dài hơn”.

Một codebase có AX tốt thường có:

```text
fast tests
clear errors
small modules
repository instructions
reproducible commands
searchable docs
explicit architecture constraints
safe sandbox
small diffs
```

Những thứ này đồng thời làm **DX cho human engineer tốt hơn**.

Rule:

```text
Better engineering environment
→ better human output
→ better agent output
```

---

# 11. Progressive disclosure

Không load toàn bộ knowledge vào context ngay từ đầu.

Better:

```text
small standing instructions
+ pointers/index
+ retrieve detail only when needed
```

Ví dụ repo agent:

```text
AGENTS.md
→ points to architecture.md
→ points to service-specific runbook
→ only relevant files loaded for task
```

Tương tự RAG:

```text
query intent
→ narrow candidate corpus
→ retrieve relevant chunks
→ model
```

Progressive disclosure giúp giảm noise, cost và stale-context risk.

---

# 12. Vocabulary review exercise

Cho architecture:

```text
Customer Support Assistant
```

Hãy label chính xác:

```text
Model        = hosted LLM
Provider     = inference service
Harness      = ASP.NET AI orchestration service
Context      = system rules + conversation + retrieved policy
Tool         = get_order_status
Tool call    = model requests get_order_status(orderId)
Tool result  = authorized order DTO
RAG          = retrieve policy docs by tenant/ACL
Memory       = selected persisted user preference/history
Eval         = support QA dataset + metrics
Workflow     = deterministic escalation path
Agent        = optional open-ended diagnostic/tool loop
```

Nếu diagram chỉ có một box “AI”, rewrite diagram.

---

# 13. Architecture quality gate

Một AI design phải trả lời được:

1. model nào chỉ là implementation detail và business port nào stable?
2. context được lấy từ đâu và ai được phép nhìn?
3. session state khác durable business state thế nào?
4. agent có thực sự cần thiết hay workflow đủ?
5. tool nào read, tool nào write?
6. authorization diễn ra trước side effect ở đâu?
7. eval nào chứng minh release tốt hơn?
8. context/token/tool-loop cost được bound thế nào?
9. failure nào degrade gracefully?
10. human review nằm ở judgement boundary nào?

---

# Exit Criteria

Bạn đạt chapter này khi có thể:

- [ ] dùng đúng model/provider/harness/agent/workflow/tool terminology;
- [ ] phân biệt AI Engineer với AI-assisted Developer;
- [ ] phân biệt parametric/contextual knowledge;
- [ ] chọn agent vs deterministic workflow bằng requirement;
- [ ] thiết kế bounded agent loop;
- [ ] phân biệt deterministic check và LLM review;
- [ ] giải thích AX/progressive disclosure;
- [ ] vẽ AI architecture mà không dùng một box “AI” mơ hồ.

## Sources

Xem [References](references.md). AI Hero dictionary được dùng để cải thiện vocabulary/mental model; terminology provider-specific vẫn phải đối chiếu official docs.

## Verification metadata

- Verified: 2026-09-03.
- Focus: vocabulary as architecture boundary, not terminology memorization.
