# AI Coding Agent Vocabulary, Context & Handoffs

> Mục tiêu: hiểu đúng các thành phần tạo nên một coding agent. Khi `model`, `agent`, `harness`, `session`, `context`, `tool`, `permission`, `sandbox`, `memory` và `handoff` bị gọi chung là “AI”, bạn rất khó debug hoặc governance đúng.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0</span>
  <span><strong>Mode</strong>&nbsp;definition → mental model → coding example → failure</span>
  <span><strong>Scope</strong>&nbsp;coding agents, not business agents</span>
</div>

## Hiểu trong 5 phút

Một coding agent không phải chỉ là một model.

```text
Coding Agent
=
Model
+ Harness
+ System / repository instructions
+ Context window
+ Tools
+ Environment
+ Permission policy
+ Session state
+ Verification loop
```

Ví dụ:

```text
User task
  ↓
Coding-agent harness
 ├─ AGENTS.md / instructions
 ├─ session/history
 ├─ Read / Search / Write / Shell / Git tools
 ├─ permission mode
 ├─ sandbox
 └─ context management
  ↓
Model provider request
  ↓
Model
  ↓
tool call / answer
  ↓
Harness executes or asks permission
  ↓
tool result
  ↓
next model provider request
```

Điểm quan trọng:

```text
Model does not read files.
Tool does not decide policy.
Agent does not own unlimited authority.
Context is not the same as context window.
Session is not durable memory.
Automated check is not automated review.
```

---

# 1. Model layer

Các từ này thuộc nền chung với Module 19 nhưng coding agent phải hiểu để debug đúng.

## Model

Tập parameters/weights dùng trong inference.

```text
input tokens
→ model
→ next-token prediction
→ output tokens
```

Model tự nó không có:

```text
filesystem
shell
Git
browser
memory across sessions
permission system
```

## Parameters / weights

Các số được học trong training và giữ cố định trong inference.

Nếu model chưa biết repo của bạn:

```text
load repo information into context
```

không phải:

```text
assume session edits model weights
```

## Training

Quá trình tạo/điều chỉnh parameters trước khi model được serve.

Coding session thông thường **không phải training**.

## Inference

Một lần chạy model với input hiện tại để sinh output.

## Next-token prediction

Cơ chế cơ bản: model dự đoán token tiếp theo từ context hiện có.

Điều này giải thích vì sao:

```text
missing context
→ guessing

bad context
→ plausible wrong output
```

## Non-determinism

Cùng logical input có thể tạo output khác nhau.

Coding workflow do đó phải verify bằng:

```text
compiler
tests
linters
policy checks
```

không chỉ dựa vào “agent nói đã xong”.

## Effort / reasoning effort

Runtime knob cho một số model/provider để tăng reasoning compute.

Coding implication:

```text
hard architecture/debugging
→ maybe higher effort

mechanical rename/format
→ lower effort often enough
```

Đánh giá bằng task eval, latency và cost.

## Input / output / cached tokens

- **Input tokens**: context gửi tới provider.
- **Output tokens**: model sinh ra.
- **Cached input tokens**: provider có thể tính riêng phần prefix đã cache.

Token economics ảnh hưởng trực tiếp đến coding-agent cost vì một turn có thể có nhiều provider requests.

---

# 2. Model provider, harness và agent

## Model provider

Runtime/service phục vụ model inference.

Provider concern:

```text
model availability
context limits
rate limits
pricing
reasoning/tool support
retention/data policy
```

## Model provider request

**Một** round-trip từ harness tới provider.

Một user turn có thể chứa nhiều request:

```text
User asks fix
→ request #1: model asks Read
→ tool result
→ request #2: model asks Search
→ tool result
→ request #3: model proposes edit
→ tool result
→ request #4: model inspects test failure
→ ...
```

## Harness

Software bao quanh model và biến nó thành usable coding system.

Harness sở hữu:

```text
system prompt
context-window assembly
tools
tool execution
permissions
hooks
session/history
compaction
subagents
UI/CLI
```

## Agent

Runtime actor mà user tương tác:

```text
Agent
= model + harness + context + tools + loop
```

Cùng một model trong hai harness khác nhau có thể có behavior rất khác vì:

```text
tools differ
instructions differ
permissions differ
context loading differs
verification loop differs
```

---

# 3. Session, turn, context và context window

Đây là nhóm dễ bị trộn nhất.

## Session

Một bounded run của interaction giữa user và agent.

Session chứa nhiều turns và harness có thể lưu history/state trong session.

Session kết thúc khi:

```text
user clears/closes
harness starts fresh session
compaction/handoff creates new session
```

## Turn

Một user message cộng toàn bộ work agent làm cho tới khi trả quyền điều khiển lại user.

Một turn có thể gồm:

```text
multiple provider requests
multiple tool calls
multiple test runs
```

## Context

Thông tin **relevant** agent đang có cho task.

Ví dụ:

```text
bug description
relevant class
matching tests
architecture rule
current CI error
```

Context là khái niệm về **information quality**.

## Context window

Literal token sequence model nhìn thấy trong **mỗi provider request**.

Có thể chứa:

```text
system prompt
conversation history
repository instructions
file contents
tool definitions
tool results
current task
```

Phân biệt:

```text
Context
= information that matters

Context window
= all tokens model currently sees

Session
= running interaction history/state
```

Một context window có thể rất lớn nhưng context quality vẫn tệ nếu phần lớn là noise.

## Stateful vs stateless

- Model thường stateless across provider requests.
- Session có thể stateful vì harness lưu history.
- Agent có thể được làm stateful across sessions bằng memory system/artifacts.

Đừng nói “model nhớ repo” nếu harness chỉ reload file/instructions.

---

# 4. Context engineering cho coding agent

## Parametric knowledge

Thông tin model nhớ từ training.

```text
may be stale
may invent recent APIs
may not know internal conventions
```

## Contextual knowledge

Thông tin model đọc được từ current context:

```text
actual source code
current package version
AGENTS.md
CI error
current official docs
```

Coding rule:

```text
Primary source in context
> model memory
```

## Knowledge cutoff

Temporal boundary của parametric knowledge.

Version-sensitive code là vùng dễ hallucinate:

```text
new .NET API
new Kubernetes version
new SDK
new provider model
```

Agent phải inspect current source/version/docs.

## Primary source

Artifact gốc, authoritative nhất:

```text
source code
actual test failure
compiler output
schema
provider docs
```

## Secondary source

Summary/account của primary source:

```text
handoff summary
README explanation
compaction summary
PR summary
```

Secondary source rẻ token hơn nhưng lossy.

Rule:

```text
Use secondary source to navigate.
Use primary source to decide when correctness matters.
```

## Progressive disclosure

Load đúng information khi cần thay vì preload mọi thứ.

```text
AGENTS.md
→ context pointer
→ architecture doc
→ service runbook
→ only load when task touches that area
```

## Context pointer

Reference dẫn agent tới detail khác.

Ví dụ:

```markdown
For database migration rules, see docs/architecture/database-migrations.md.
```

## Skill

Một capability/instruction bundle được load khi task cần.

Ví dụ:

```text
PDF handling skill
release skill
incident-debug skill
```

Skill tốt hỗ trợ progressive disclosure thay vì nhét mọi procedure vào global instructions.

---

# 5. Attention degradation và context quality

Long session có thể degrade trước khi full context window.

Common symptoms:

```text
forgets earlier constraint
re-opens already settled decision
edits unrelated file
misreads tool result
duplicates work
```

Useful mental model:

```text
more context
→ more competing signals
→ relevant relationship may get weaker
```

AI Hero gọi vùng đầu session có focus tốt là “smart zone” và vùng session dài/noisy là “dumb zone”. Đây là mental model hữu ích, không phải provider specification.

Mitigation:

```text
small task scope
progressive disclosure
clear stale outputs
fresh session
handoff artifact
re-read primary source
```

---

# 6. Tools và environment

## Environment

World bên ngoài model mà agent có thể quan sát/thay đổi qua tools.

Coding-agent environment thường gồm:

```text
filesystem
Git worktree
shell
toolchain
network
GitHub/CI
issue tracker
browser/docs
```

## Filesystem

Tree file agent đọc/ghi. Đây là default environment quan trọng nhất của coding agent.

## Tool

Function harness expose cho agent.

Ví dụ:

```text
Read
Search
Write/Edit
Shell/Bash
Git
Browser
MCP tool
```

## Tool call

Structured model output:

```text
tool name
+ arguments
```

Nó chưa phải side effect.

## Tool result

Output harness đưa lại model sau khi execute tool.

```text
file content
shell stdout/stderr
search result
API response
error
```

Tool result là cách agent “nhìn” environment sau action.

## MCP

Protocol để harness kết nối external tool/data servers.

```text
Agent harness
→ MCP client
→ MCP server
→ external capability
```

MCP không tự cung cấp AuthZ/sandbox/governance. MCP server là một trust boundary mới.

---

# 7. Permission model

## Permission request

Prompt/approval gate harness đưa cho user trước tool call chưa được pre-approved.

## Permission mode

Policy quyết định action nào:

```text
auto-run
ask
block
```

## Agent mode

Preset rộng hơn permission mode, thường kết hợp:

```text
permission policy
+
behavioral instructions
```

Ví dụ conceptual:

```text
Plan mode
= read/research instructions + writes blocked

Edit mode
= scoped writes allowed

Autonomous sandbox mode
= broader auto permissions but isolated environment
```

## Sandbox

Isolation boundary giới hạn blast radius:

```text
container
VM
restricted shell
isolated worktree
network-limited runner
```

Sandbox không thay AuthZ nhưng giảm impact khi agent/tool sai.

---

# 8. Handoffs và context reset

Big task thường dài hơn một useful session.

## Clearing

Kết thúc session hiện tại và bắt đầu fresh session.

## Handoff

Chuyển context cần thiết sang session khác.

## Handoff artifact

Durable document dùng làm carry mechanism.

Nên chứa:

```text
goal
current state
accepted decisions
changed files
verification performed
known failures
next step
```

Không nên chứa toàn chain-of-thought.

## Spec

Artifact mô tả multi-session work:

```text
what is being built
constraints
acceptance criteria
decisions
tickets
```

## Ticket

Một bounded unit đủ nhỏ cho một session/agent task.

Good ticket:

```text
one outcome
known scope
acceptance criteria
verification
```

## Compaction

History cũ được summarize để tạo headroom cho continuation/fresh context.

Compaction là lossy.

Do đó critical decision phải sống trong durable source:

```text
ADR
spec
issue
code/tests
```

## Autocompact

Harness tự trigger compaction khi context gần threshold.

Bạn không nên coi summary sau autocompact là primary source cho subtle code facts.

---

# 9. Memory và repository instructions

## Memory system

Mechanism persist selected information qua sessions rồi reload sau.

Coding-agent memory có thể chứa:

```text
user preference
repository convention
persistent task note
known tool configuration
```

Không nên dùng memory làm source of truth cho:

```text
current code
current branch
current CI status
current package version
```

Những thứ đó phải đọc lại environment.

## AGENTS.md

Repository instruction artifact mà một số harness có thể load/discover để steer agent.

Tốt cho:

```text
build/test commands
architecture constraints
safety rules
repository-specific definition of done
```

Không nên biến `AGENTS.md` thành encyclopedia 1,000 dòng.

## Subagent

Agent phụ chạy trong session/context riêng để xử lý bounded subtask rồi trả result về parent/harness.

Use cases:

```text
parallel research
focused code search
independent review
specialized analysis
```

Risk:

```text
more provider calls
context transfer loss
permission propagation
harder audit
```

Subagent output là secondary source cho parent; critical claim nên verify lại bằng primary source.

---

# 10. Verification vocabulary

## Automated check

Deterministic pass/fail:

```text
compiler
test
lint
type-check
security scan
policy check
```

## Automated review

Một agent/model review work và đưa judgement.

```text
probabilistic
useful as second opinion
not a deterministic gate by itself
```

## Human review

Human đọc artifact/diff và đánh giá.

Đọc summary của agent không tương đương đọc diff khi risk cao.

## Human-in-the-loop

Human tham gia trong session/workflow để:

```text
clarify
approve
redirect
review
resolve ambiguity
```

## AFK / unattended work

Agent chạy không có human theo dõi liên tục.

Chỉ hợp khi:

```text
scope bounded
sandboxed
permissions limited
verification automated
stop conditions clear
```

## Vibe coding

Pattern chấp nhận generated code mà không hiểu/review đủ.

Không phù hợp với production-critical repository.

---

# 11. Design and collaboration vocabulary

## Design concept

Shared understanding của user và agent về thứ đang build, độc lập với implementation artifact cụ thể.

## Grilling / Socratic clarification

Agent hỏi từng decision để làm rõ design concept trước khi implement.

Useful khi requirement ambiguous.

## Prototyping

Build rough artifact nhanh để học/clarify thay vì tranh luận abstract quá lâu.

Prototype không tự trở thành production code.

## DX — Developer Experience

Environment quality cho human engineer:

```text
fast feedback
clear docs
simple commands
good errors
reproducible setup
```

## AX — Agent Experience

Environment quality cho coding agent:

```text
fast tests
clear repository instructions
searchable architecture
small modules
good errors
safe tools
free context headroom
```

DX và AX thường reinforce nhau.

---

# 12. Failure modes

## Hallucination

Confident but wrong output.

Hai dạng hữu ích khi debug coding agent:

```text
factual hallucination
→ invents API/file/fact

faithfulness failure
→ loaded source says A, agent outputs B
```

## Sycophancy

Agent đồng ý với user assumption dù evidence khác.

Prompt/task nên yêu cầu:

```text
verify assumption
show counterevidence
state uncertainty
```

## Context poisoning

Untrusted repo/web/tool content cố steer agent trái policy.

## Attention degradation

Long/noisy session làm relevant constraints yếu đi.

## Scope drift

Agent sửa ngoài task boundary.

## Verification gaming

Agent làm check xanh bằng cách:

```text
delete/skip test
weaken assertion
suppress error
broaden ignore rule
```

## Runaway tool loop

Agent tiếp tục gọi tools mà không tạo progress tương xứng.

---

# 13. Một coding-agent turn hoàn chỉnh

```text
User task
  ↓
Harness loads repository instructions
  ↓
Model provider request #1
  ↓
Model asks Search
  ↓
Harness executes Search
  ↓
Tool result enters context window
  ↓
Model provider request #2
  ↓
Model asks Read
  ↓
Read result
  ↓
request #3
  ↓
Model asks Edit
  ↓
permission policy evaluates
  ↓
write happens in sandbox/worktree
  ↓
request #4
  ↓
Model asks Test
  ↓
test output
  ↓
request #5
  ↓
Agent summarizes evidence and yields
```

Một `turn` có thể tốn nhiều provider requests, tool executions và tokens.

---

# 14. Khi session dài: quyết định clear, compact hay handoff

Use **continue** khi:

```text
context still focused
recent tool output relevant
no major drift
```

Use **compact** khi:

```text
same task continues
history large
critical state already durable elsewhere
```

Use **fresh session + handoff** khi:

```text
subtask changes
attention degraded
context polluted
multi-session project
```

Handoff artifact nên point tới primary source, không copy cả repo vào summary.

---

# 15. Repository architecture cho AX tốt

```text
repo/
├── AGENTS.md
├── docs/
│   ├── architecture.md
│   └── runbooks/
├── src/
├── tests/
├── scripts/
│   ├── verify.sh
│   └── smoke-test.sh
└── .github/workflows/
```

Good AX characteristics:

```text
one command builds
one command tests
failures readable
architecture searchable
scope boundaries visible
unsafe commands documented/blocked
CI deterministic
```

Agent prompt quality không thể bù mãi cho repo có AX tệ.

---

# 16. Definition table — nhớ nhanh

| Term | Định nghĩa ngắn |
|---|---|
| Model | parameters chạy inference |
| Provider | runtime/API serve model |
| Harness | software bọc model bằng context/tools/policy |
| Agent | model-in-harness hoạt động qua turns |
| Provider request | một round-trip harness ↔ model provider |
| Session | bounded interaction state |
| Turn | một user message + toàn bộ agent work cho tới khi yield |
| Context | thông tin task-relevant agent có |
| Context window | literal token sequence model thấy mỗi request |
| Tool | function harness expose |
| Tool call | model proposal gọi tool |
| Tool result | execution result đưa lại model |
| Environment | world agent tác động qua tools |
| Permission mode | policy auto/ask/block tool calls |
| Agent mode | permission mode + behavior steering |
| Sandbox | isolation giảm blast radius |
| Memory | state persisted/reloaded across sessions |
| Handoff | chuyển task context sang session khác |
| Compaction | lossy summary để lấy context headroom |
| AGENTS.md | repository standing instructions |
| Progressive disclosure | load detail only when needed |
| Skill | capability/instruction bundle loaded on demand |
| Subagent | bounded child agent/session |
| Automated check | deterministic pass/fail evidence |
| Automated review | probabilistic agent judgement |
| Human review | human judgement on artifact/diff |
| AX | environment quality cho agent |

---

# Exit Criteria

Bạn đạt chapter này khi có thể:

- [ ] phân biệt model/provider/harness/agent;
- [ ] phân biệt session/turn/provider request;
- [ ] phân biệt context/context window;
- [ ] giải thích tool call khác tool execution;
- [ ] giải thích environment/permission mode/agent mode/sandbox;
- [ ] giải thích parametric vs contextual knowledge;
- [ ] chọn primary source thay vì secondary summary khi correctness critical;
- [ ] giải thích clearing/handoff/compaction;
- [ ] thiết kế `AGENTS.md` + progressive disclosure/context pointers;
- [ ] phân biệt automated check/review/human review;
- [ ] giải thích AX và vì sao AX tốt thường đi cùng DX tốt;
- [ ] nhận diện hallucination, sycophancy, context poisoning, scope drift và verification gaming.

## Sources

AI Hero AI Coding Dictionary được dùng như supplementary vocabulary/mental-model source. Product-specific behavior, context limits, permissions và agent capabilities phải đối chiếu official product documentation.

→ [Module 19 Vocabulary](../19-ai-engineering/ai-engineer-vocabulary-and-system-boundaries.md)
