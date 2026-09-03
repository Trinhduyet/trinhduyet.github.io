# Module 21 — References

> Policy: official product/protocol documentation quyết định exact capability, permissions, modes và security behavior. AI Hero AI Coding Dictionary được dùng như **supplementary vocabulary/mental-model source**.

## Supplementary — AI Hero vocabulary

- AI Coding Dictionary: https://www.aihero.dev/ai-coding-dictionary
- What Is An AI Engineer?: https://www.aihero.dev/what-is-an-ai-engineer
- Messages, System Prompts and Reasoning Tokens: https://www.aihero.dev/messages-system-prompts-and-reasoning-tokens

Dictionary categories được dùng để audit vocabulary breadth:

```text
The Model
Sessions / Context Windows / Turns
Tools / Environment
Failure Modes
Handoffs
Memory / Steering
Patterns of Work
```

Module 21 không copy nguyên dictionary. Các term được tổ chức lại theo software-engineering ownership:

```text
model/runtime fundamentals
→ Module 19

coding-agent environment / permissions / handoffs / review / AX
→ Module 21
```

Useful supplementary concepts:

```text
model vs harness vs agent
provider request vs turn
context vs context window vs session
tool call vs tool result
permission mode vs agent mode
sandbox
attention degradation
primary vs secondary source
handoff / spec / ticket / compaction
AGENTS.md / progressive disclosure / context pointer / skill / subagent
automated check vs automated review vs human review
human-in-the-loop / AFK / vibe coding
DX / AX
```

Terms such as “smart zone”, “dumb zone”, “grilling” or other explanatory labels are treated as useful teaching language, not provider standards.

---

## Official — OpenAI Codex

- OpenAI Help — Codex collection: https://help.openai.com/en/collections/14937394-codex
- OpenAI Help — Using Codex with your ChatGPT plan: https://help.openai.com/en/articles/11369540
- OpenAI — Codex: https://openai.com/codex/
- OpenAI — Running Codex safely: https://openai.com/index/running-codex-safely/
- OpenAI Codex repository: https://github.com/openai/codex

Exact approval/sandbox/model/product behavior changes over time; verify current docs before relying on UI-specific behavior.

---

## Official — GitHub Copilot Agents

- GitHub Docs — What is GitHub Copilot?: https://docs.github.com/en/copilot/get-started/what-is-github-copilot
- GitHub Docs — Concepts for Copilot agents: https://docs.github.com/en/copilot/concepts/agents
- GitHub Docs — Use Copilot agents: https://docs.github.com/en/copilot/how-tos/use-copilot-agents
- GitHub Docs — Review output from Copilot agents: https://docs.github.com/en/copilot/how-tos/copilot-on-github/use-copilot-agents
- GitHub Docs — Third-party coding agents: https://docs.github.com/en/copilot/concepts/agents/about-third-party-coding-agents
- GitHub Docs — Copilot documentation index: https://docs.github.com/en/copilot

Current GitHub docs include concepts for agent sessions, cloud/local sandboxes, agent skills, memory, hooks and third-party coding agents. Treat exact availability as plan/product-policy dependent.

---

## Official — Anthropic Claude Code

- Claude Code docs: https://docs.anthropic.com/en/docs/claude-code/overview
- Claude Code security: https://docs.anthropic.com/en/docs/claude-code/security

Product-specific instruction files, permission modes, sandbox/network behavior and hooks should be verified against current Anthropic docs.

---

## Official — Model Context Protocol

- MCP documentation: https://modelcontextprotocol.io/docs/getting-started/intro
- MCP specification: https://modelcontextprotocol.io/specification/latest

MCP is an integration protocol. Tool authorization, identity, sandboxing, data governance and audit remain system responsibilities.

---

## Repository cross-links

- [Module 19 — Production AI Engineering](../19-ai-engineering/README.md) — model/runtime/context/reasoning/MEAI/RAG/eval foundations.
- [AI Engineering Vocabulary](../19-ai-engineering/ai-engineer-vocabulary-and-system-boundaries.md) — shared model/provider/runtime vocabulary.
- [Testing & Code Review](../08-testing-code-review/README.md) — executable quality evidence and human review.
- [Security & DevSecOps](../09-security-devsecops/README.md) — trust boundaries, credentials, supply chain.
- [DevOps & Delivery](../13-devops-iac/README.md) — CI/CD and artifact/release governance.
- [Software Architecture](../25-software-architecture/README.md) — boundaries and change governance.

## Verification metadata

- Updated: 2026-09-03.
- AI Hero pages reviewed as supplementary conceptual sources.
- Product-specific details remain time-sensitive and should be checked in current official docs before operational use.
