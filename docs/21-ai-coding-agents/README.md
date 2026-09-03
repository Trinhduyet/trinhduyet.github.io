# Module 21 — AI Coding Agents

> Mục tiêu: hiểu và vận hành coding agent như một **privileged software-engineering automation system có LLM planner**, không phải một chatbot viết code.

<div class="lesson-meta">
  <span><strong>Priority</strong>&nbsp;P0/P1</span>
  <span><strong>Prerequisite</strong>&nbsp;Module 19 foundations</span>
  <span><strong>Mode</strong>&nbsp;model → harness → context → tools → permissions → evidence</span>
  <span><strong>Outcome</strong>&nbsp;safe, verifiable coding workflow</span>
</div>

<div class="key-takeaway" markdown>
<strong>Coding agent không phải model có quyền developer.</strong>

Một coding agent là model được harness với repository context, filesystem/shell/Git tools, permissions, sandbox, session state và verification loop. Chất lượng cuối cùng phụ thuộc rất mạnh vào **context, environment, tests, permissions và review**, không chỉ model name.
</div>

## Hiểu trong 5 phút

```text
User task
   ↓
Coding Agent Harness
 ├─ system/repository instructions
 ├─ session + context-window management
 ├─ Read / Search / Edit / Shell / Git tools
 ├─ MCP / external tools
 ├─ permissions / approval mode
 ├─ sandbox / worktree
 ├─ hooks / policies
 └─ compaction / handoff / subagents
   ↓
Model Provider
   ↓
Model
   ↓
tool call / answer
   ↓
Harness validates permission and executes
   ↓
tool result
   ↓
Model continues
   ↓
Build / Test / Diff / Review evidence
```

Một agent coding tốt không chỉ generate code. Nó phải có **closed feedback loop với executable evidence**.

---

# 1. Phân biệt ba mức

## Code Completion

```text
cursor position
→ predict next code fragment
```

Scope nhỏ, thường không tự inspect repository/tool loop.

## Coding Assistant

```text
chat
→ answer/explain
→ suggest edit
```

Có thể hiểu code context nhưng user vẫn điều khiển hầu hết action.

## AI Coding Agent

```text
inspect repository
→ plan
→ read/search
→ edit
→ run commands/tests
→ inspect failures
→ iterate
→ produce diff/PR/evidence
```

Agent có **environment + tools + loop**, vì vậy risk và governance khác assistant thông thường.

---

# 2. Learning path

Học theo dependency:

```text
1. Model / provider / harness / agent
        ↓
2. Session / turn / context / context window
        ↓
3. Filesystem / tools / environment / MCP
        ↓
4. Permission mode / agent mode / sandbox
        ↓
5. Repository instructions / progressive disclosure
        ↓
6. Handoff / compaction / memory / subagents
        ↓
7. Task specification / planning
        ↓
8. Build / tests / automated checks
        ↓
9. Automated review / human review
        ↓
10. CI / PR / delivery governance
```

| Chapter | Mục tiêu |
|---|---|
| [AI Coding Agent Vocabulary, Context & Handoffs](ai-coding-agent-vocabulary-context-and-handoffs.md) | định nghĩa đầy đủ model/harness/session/context/tools/permissions/handoff/memory/AX |
| [Repository Context, Instructions & MCP](repository-context-mcp-and-instructions.md) | context discovery, `AGENTS.md`, MCP, filesystem/network/tool boundaries |
| [Safe Agentic Coding Workflow](safe-agentic-coding-workflow.md) | task → discovery → test → edit → CI → PR → human review |
| [References](references.md) | official agent/MCP/security sources + supplementary AI Hero vocabulary |

Prerequisite concepts: [Module 19 — Production AI Engineering](../19-ai-engineering/README.md).

---

# 3. Core vocabulary phải thuộc

| Term | Ý nghĩa |
|---|---|
| Model | parameters chạy inference; không tự có filesystem/tools |
| Provider | runtime/API serve model |
| Harness | software bọc model bằng context/tools/policy/session |
| Agent | model-in-harness hoạt động qua turns/tool loop |
| Provider request | một round-trip harness ↔ provider |
| Session | bounded interaction state |
| Turn | một user message + toàn bộ agent work trước khi yield |
| Context | information task-relevant agent có |
| Context window | literal token sequence model nhìn thấy mỗi request |
| Environment | world bên ngoài model: repo/shell/Git/network/CI |
| Tool | function harness expose để agent quan sát/thay đổi environment |
| Tool call | structured proposal do model tạo |
| Tool result | execution result đưa lại model |
| Permission mode | auto/ask/block policy cho tool calls |
| Agent mode | permission policy + behavioral steering preset |
| Sandbox | isolation giảm blast radius |
| Memory | selected state persisted/reloaded across sessions |
| Handoff | chuyển task context sang session khác |
| Compaction | lossy summary để lấy context headroom |
| Automated check | deterministic pass/fail evidence |
| Automated review | probabilistic agent judgement |
| Human review | human judgement trên artifact/diff |
| AX | Agent Experience — environment quality cho agent |

Đọc định nghĩa sâu: [AI Coding Agent Vocabulary, Context & Handoffs](ai-coding-agent-vocabulary-context-and-handoffs.md).

---

# 4. Coding Agent khác Business AI Agent

| Business AI Agent | AI Coding Agent |
|---|---|
| tools là business capabilities | tools là filesystem/shell/Git/CI/GitHub/browser |
| state là business/conversation workflow | state thường là repo/worktree/task/PR/session |
| output có thể là business action | output chủ yếu code/diff/tests/PR |
| risk: data/tool abuse | risk: arbitrary code, secrets, supply chain, CI/IAM |
| tool AuthZ theo business resource | tool permission theo filesystem/network/repo/CI |
| eval tập trung product behavior | eval tập trung task success + regressions + scope |

Hai loại dùng chung agent concepts nhưng **trust boundary khác nhau**.

---

# 5. Model != Agent

Model chỉ nhận tokens và sinh output.

```text
Model
!=
Read repo
Run test
Commit code
Browse docs
Remember last week
```

Những capability này đến từ harness/tools/environment.

```text
Model
+ Harness
+ Context
+ Tools
+ Permissions
+ Control loop
= Coding Agent
```

Điều này rất quan trọng khi debug:

```text
bad answer
? model capability
? missing context
? stale context
? wrong tool
? tool error
? permission block
? bad harness instruction
? attention degradation
```

Không đổ mọi failure cho model.

---

# 6. Session, turn và provider request

Một user message có thể tạo nhiều provider requests:

```text
User: fix failing test
  ↓
request #1
→ model asks Search
  ↓
Search result
  ↓
request #2
→ model asks Read
  ↓
Read result
  ↓
request #3
→ model asks Edit
  ↓
Edit result
  ↓
request #4
→ model asks Test
  ↓
Test output
  ↓
request #5
→ final response
```

Đây là **một turn**, nhưng có 5 provider requests.

Session chứa nhiều turns và history/tool outputs có thể accumulate.

Cost/latency của coding agent vì vậy thường đến từ **tool loop + repeated context**, không chỉ final answer tokens.

---

# 7. Context != Context Window

## Context

Relevant information cho task:

```text
issue
relevant implementation
matching test
current package version
architecture constraint
current error
```

## Context window

Tất cả tokens model thấy trong một request:

```text
system prompt
history
AGENTS.md
file contents
tool schemas
tool results
current task
```

Rule:

```text
large context window
!=
good context
```

Context quality cao khi:

```text
relevant
current
primary-source-backed
minimal noise
correct authority
```

→ [Repository Context, Instructions & MCP](repository-context-mcp-and-instructions.md)

---

# 8. Parametric vs contextual knowledge

## Parametric knowledge

Model nhớ từ training.

Risk:

```text
stale SDK
invented API
old framework behavior
wrong repository assumption
```

## Contextual knowledge

Model đọc trực tiếp từ current context:

```text
actual csproj
current source
actual compiler error
current official docs
```

Coding rule:

```text
Current primary source
> parametric memory
```

Khi API/version-sensitive, agent phải inspect project version + current docs trước khi edit.

---

# 9. Tools và environment

Coding-agent environment có thể gồm:

```text
filesystem
shell
Git
compiler/test runner
package manager
network
browser
GitHub/CI
cloud CLI
MCP servers
```

Tools là cách agent perceive/act:

```text
Read
Search
Edit/Write
Bash/Shell
Git
Browser
MCP calls
```

Critical distinction:

```text
Tool call
= model output/proposal

Tool execution
= harness/application side effect
```

Tool call không tự có authority.

---

# 10. Permission mode, agent mode và sandbox

## Permission mode

Quyết định action:

```text
auto-run
ask user
block
```

## Agent mode

Thường bundle:

```text
permission policy
+
behavioral instructions
```

Ví dụ:

```text
Plan mode
→ research/read instructions
→ write blocked

Scoped edit mode
→ writes in repo allowed
→ network/secret/merge blocked
```

## Sandbox

Giảm blast radius nếu command/edit sai.

```text
container
VM
isolated worktree
restricted shell
network-limited runner
```

Rule:

```text
broader autonomy
requires
stronger isolation + better automated verification
```

---

# 11. Permission ladder cho repository work

```text
Read source                     low
Search source                   low
Write working tree              medium
Run compiler/tests              medium
Install dependency              medium/high
Network access                  high
Read secrets                    very high
Push branch                     high
Modify CI permissions           very high
Merge PR                        very high
Production deploy               critical
```

Default production posture:

| Action | Agent default | Human/pipeline |
|---|---|---|
| read/search repo | allow | — |
| edit scoped branch/worktree | allow/controlled | review |
| run unit/build tests | allow | — |
| add dependency | propose/verify | review when material |
| arbitrary network | deny/allowlist | policy |
| read secrets | deny | narrow mechanism only |
| push branch | explicit | policy |
| create PR | controlled | review |
| merge | no default | reviewer/branch protection |
| deploy prod | no default | delivery pipeline |

---

# 12. Repository instructions

Coding agent cần standing brief ngắn, executable.

`AGENTS.md`/equivalent nên chứa:

```text
build command
test command
docs command
architecture constraints
security constraints
git permissions
definition of done
```

Không nên chứa:

```text
huge tutorial
duplicated domain docs
stale package docs
secrets
```

Dùng progressive disclosure:

```text
AGENTS.md
→ architecture pointer
→ module-specific docs
→ load only when needed
```

---

# 13. Progressive disclosure, skills và context pointers

Repo lớn không nên preload toàn bộ knowledge.

```text
small standing instructions
+ context pointers
+ task-specific skills/docs
+ primary source reads
```

Example:

```text
AGENTS.md
→ "Database migration rules: docs/db-migrations.md"
→ agent only loads file when task touches migrations
```

Một `skill`/playbook tốt bundle procedure cụ thể và chỉ load khi relevant.

Benefits:

```text
less context noise
less token cost
less stale information
better attention
```

---

# 14. Context degradation

Long session có thể tệ dần dù chưa chạm hard context limit.

Symptoms:

```text
forgets constraints
repeats work
changes settled design
mixes old/new file state
scope drifts
```

Mitigation:

```text
smaller task
fresh primary-source reads
clear stale tool output
compact
fresh session
handoff artifact
```

AI Hero gọi giai đoạn early focused là “smart zone” và long/noisy region là “dumb zone”; dùng như mental model, không phải formal provider metric.

---

# 15. Handoff, compaction và memory

## Handoff

Chuyển work sang session mới.

Good handoff artifact:

```text
goal
constraints
current state
decisions
changed files
test evidence
known failures
next step
links to primary sources
```

## Compaction

Summarize previous history để lấy context headroom.

Compaction **lossy**.

Critical decisions phải persist ở:

```text
code/tests
ADR
spec
issue
PR
```

## Memory

Cross-session persisted information.

Không dùng memory làm source of truth cho:

```text
current branch
current source
current CI
current package version
```

Agent phải re-read environment.

---

# 16. Spec, ticket và multi-session work

Task lớn hơn một useful session cần durable planning artifact.

## Spec

```text
outcome
requirements
constraints
architecture decisions
acceptance criteria
tickets/status
```

## Ticket

Một bounded chunk đủ nhỏ cho một session/agent.

Good ticket:

```text
one outcome
known scope
explicit constraints
verification
```

Bad:

```text
"modernize architecture"
```

Good:

```text
"Add idempotent persistence for duplicate webhook event IDs, with concurrent regression test; preserve public API."
```

---

# 17. Task contract trước khi agent edit

Một coding task production nên có:

```markdown
## Problem
What is wrong?

## Expected behavior
What must become true?

## Constraints
What must not change?

## Acceptance criteria
What executable evidence proves success?

## Permissions
What may the agent read/write/run/push?
```

Nếu objective mơ hồ, agent có thể optimize nhầm thứ.

---

# 18. Good task vs poor task

Good:

```text
fix scoped bug có reproduction
add regression test
upgrade dependency với acceptance criteria
refactor có strong test suite
implement endpoint theo contract
migrate repetitive code
sync docs with code
```

Poor/risky:

```text
rewrite everything
improve security everywhere
make it faster
change production IAM freely
run production migrations without rollback
```

Agent autonomy phải tỷ lệ nghịch với ambiguity + blast radius.

---

# 19. Discovery before edit

Agent nên bắt đầu read-only:

```text
repo status
branch/worktree
project structure
relevant symbols
tests
CI/build commands
architecture constraints
```

Questions cần answer trước edit:

```text
Where is behavior owned?
What proves current bug?
Which public contract must remain?
Which layer owns invariant?
What files should not change?
```

Không bắt đầu bằng “create 12 files” khi chưa inspect code.

---

# 20. Plan phải map sang evidence

Bad:

```text
1. Analyze
2. Implement
3. Test
```

Better:

```text
1. Reproduce duplicate behavior.
   Evidence: failing regression test.
2. Locate persistence invariant.
   Evidence: schema/model inspection.
3. Implement smallest concurrency-safe fix.
   Evidence: focused diff.
4. Run targeted tests.
5. Run project/full tests.
6. Inspect diff scope.
```

Plan tốt làm verification path rõ từ đầu.

---

# 21. Verification ladder

Coding agent không được tự xác nhận bằng prose.

```text
Static/type/compiler check
      ↓
Targeted regression test
      ↓
Relevant project tests
      ↓
Solution build
      ↓
Full test suite
      ↓
Integration/security checks
      ↓
Diff inspection
      ↓
Human review
```

Tùy task không phải lúc nào cũng cần mọi tầng, nhưng evidence phải tương xứng risk.

---

# 22. Automated check vs automated review vs human review

## Automated check

Deterministic:

```text
build
unit test
type check
lint
policy scan
```

## Automated review

Agent/model thứ hai đưa judgement:

```text
architecture critique
security review
scope check
```

Useful nhưng probabilistic.

## Human review

Human đọc artifact/diff và quyết định.

```text
Reading agent summary
!=
reviewing the diff
```

đặc biệt với security/data/infra changes.

---

# 23. Human-in-the-loop và AFK work

## Human-in-the-loop

Human tham gia để:

```text
clarify requirement
approve risky action
review diff
resolve ambiguity
change direction
```

## AFK / unattended

Agent chạy khi user không theo dõi.

Chỉ nên khi:

```text
task bounded
sandbox strong
permissions limited
checks automated
stop criteria explicit
```

Không AFK với full prod credentials.

---

# 24. Shell, network và secrets là trust boundaries

Shell mở ra:

```text
arbitrary process execution
package install
filesystem mutation
cloud CLI
Git commands
```

Network thêm:

```text
external downloads
package registries
arbitrary URLs
internal endpoints
data exfiltration path
```

Secrets thêm authority.

Rule:

```text
agent reads untrusted content
+
has network/secrets/write capability
=
high prompt-injection blast radius
```

Use:

```text
sandbox
network allowlist
secret minimization
short-lived credentials
path scope
tool allowlist
audit logs
```

---

# 25. MCP là integration protocol, không phải safety system

```text
Agent Harness
→ MCP Client
→ MCP Server
→ External Tool/Data
```

MCP server có thể expose:

```text
GitHub
issue tracker
browser/docs
DB metadata
internal systems
```

Mỗi server cần review:

```text
identity
permissions
data exposed
write capability
network path
audit
prompt-injection surface
```

Protocol không tự làm tool safe.

---

# 26. Coding-agent failure modes

## Hallucination

Invented API/file/fact.

Fix: primary source + compiler/test + current docs.

## Faithfulness failure

Source đã load nhưng output drift khỏi source.

Fix: smaller/focused context + explicit verification.

## Sycophancy

Agent tin assumption của user thay vì evidence.

Fix: require verification/counterevidence.

## Attention degradation

Long session quên constraints.

Fix: compact/handoff/fresh session.

## Context poisoning / prompt injection

Repo/web/tool result chứa malicious instruction.

Fix: authority hierarchy + sandbox + permission gate.

## Scope drift

Sửa unrelated files.

Fix: task/file allowlist + diff audit.

## Verification gaming

Agent làm CI xanh bằng:

```text
weaken test
disable analyzer
skip failure
broaden ignore
```

Fix: review test changes + preserve behavior contract.

## Runaway loop

Repeated tools without progress.

Fix: iteration/time/cost budgets.

---

# 27. Vibe coding vs engineering with agents

Vibe coding:

```text
agent writes
→ user accepts without understanding/review
```

Production agentic engineering:

```text
clear task
→ scoped permissions
→ agent work
→ executable checks
→ diff review
→ CI
→ controlled merge/deploy
```

Agent throughput không thay accountability.

---

# 28. DX và AX

## DX

Developer Experience:

```text
fast feedback
clear docs
good errors
simple build/test
reproducible environment
```

## AX

Agent Experience:

```text
clear instructions
searchable architecture
small modules
fast tests
machine-readable errors
safe tooling
context headroom
```

Một repo có AX tốt thường cũng tốt hơn cho human developers.

```text
Better engineering environment
→ better human work
→ better agent work
```

---

# 29. Coding-agent evaluation

Không chỉ benchmark model chung. Eval theo repo tasks.

Measure:

```text
task completion rate
build pass rate
test pass rate
regression rate
scope violation rate
security finding rate
human rework time
time-to-PR
provider requests/tokens/cost
```

Dataset nên chứa:

```text
task
expected behavior
required checks
forbidden changes
permission expectations
```

Agent mới/model mới/harness mới phải chạy cùng eval set trước rollout rộng.

---

# 30. Safe production workflow

```text
Issue / Task Contract
      ↓
Read-only Discovery
      ↓
Plan + Evidence Map
      ↓
Scoped Worktree / Sandbox
      ↓
Targeted Edit
      ↓
Targeted Tests
      ↓
Build / Full Tests
      ↓
Diff / Dependency / Security Review
      ↓
Draft PR
      ↓
CI with Least Privilege
      ↓
Human Review
      ↓
Normal Merge / Deploy Pipeline
```

Agent không bypass software delivery system hiện có.

→ [Safe Agentic Coding Workflow](safe-agentic-coding-workflow.md)

---

# 31. Architecture checklist

Một coding-agent setup tốt phải trả lời được:

1. model/provider nào đang dùng và vì sao;
2. harness load context/instructions thế nào;
3. session/compaction/handoff policy ra sao;
4. tools nào có read/write/network capability;
5. permission mode/approval policy là gì;
6. sandbox boundary ở đâu;
7. repo path/network/secrets scope thế nào;
8. MCP servers nào được trust;
9. task acceptance criteria có executable không;
10. automated checks nào bắt buộc;
11. agent-generated test changes được review thế nào;
12. human review/merge/deploy gate nằm ở đâu;
13. audit trail chứa session/task/tool/commit evidence gì;
14. agent eval đo task success và scope/security ra sao.

---

# 32. Exit Criteria

Bạn hoàn thành Module 21 khi có thể:

- [ ] phân biệt model/provider/harness/agent;
- [ ] phân biệt session/turn/provider request;
- [ ] phân biệt context/context window/memory;
- [ ] giải thích primary vs secondary source;
- [ ] giải thích tool/tool call/tool result/environment;
- [ ] thiết kế permission mode + sandbox;
- [ ] viết repository instructions có progressive disclosure;
- [ ] giải thích handoff/compaction/subagent;
- [ ] viết task spec có problem/constraints/acceptance/permissions;
- [ ] discovery trước edit;
- [ ] plan map sang executable evidence;
- [ ] bắt agent chạy targeted/full verification;
- [ ] phân biệt automated check/review/human review;
- [ ] nhận diện hallucination/sycophancy/context poisoning/scope drift/verification gaming;
- [ ] thiết kế CI least privilege;
- [ ] review agent-generated PR như untrusted change;
- [ ] xây coding-agent eval dataset;
- [ ] giải thích AX và cải thiện repo cho cả human lẫn agent;
- [ ] không cho coding agent merge/deploy production theo mặc định.

## Học tiếp

1. [AI Coding Agent Vocabulary, Context & Handoffs](ai-coding-agent-vocabulary-context-and-handoffs.md)
2. [Repository Context, Instructions & MCP](repository-context-mcp-and-instructions.md)
3. [Safe Agentic Coding Workflow](safe-agentic-coding-workflow.md)
4. [Production AI Engineering](../19-ai-engineering/README.md)
5. [Testing & Code Review](../08-testing-code-review/README.md)
6. [Security & DevSecOps](../09-security-devsecops/README.md)
7. [DevOps & Delivery](../13-devops-iac/README.md)
8. [References](references.md)

## Verification metadata

- Rebuilt: 2026-09-03.
- AI Hero AI Coding Dictionary is used as supplementary vocabulary/mental-model input, not a normative product specification.
- Product-specific permissions, agent modes, context handling and capabilities must be checked against current official documentation.
