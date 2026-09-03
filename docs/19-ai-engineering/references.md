# Module 19 — References

> Policy: **official/provider documentation là source of truth cho API/model/version semantics**. AI Hero được dùng như supplementary learning source cho vocabulary và mental model.

## Official — .NET AI

- Microsoft Learn — AI apps for .NET developers: https://learn.microsoft.com/en-us/dotnet/ai/
- Microsoft Learn — `IChatClient`: https://learn.microsoft.com/en-us/dotnet/ai/ichatclient
- Microsoft Learn — RAG concepts: https://learn.microsoft.com/en-us/dotnet/ai/conceptual/rag
- Microsoft Learn — AI Evaluation libraries: https://learn.microsoft.com/en-us/dotnet/ai/evaluation/libraries
- Microsoft Learn — accessing data in AI functions: https://learn.microsoft.com/en-us/dotnet/ai/how-to/access-data-in-functions

## Official — OpenAI

- OpenAI API — Models: https://platform.openai.com/docs/models
- OpenAI API — Text generation / message and instruction semantics: https://platform.openai.com/docs/guides/text
- OpenAI API — Reasoning models and effort: https://platform.openai.com/docs/guides/reasoning
- OpenAI API — Function calling / tools: https://platform.openai.com/docs/guides/function-calling
- OpenAI API — Official .NET SDK: https://github.com/openai/openai-dotnet

Provider-specific details such as message roles, reasoning controls, context limits, token accounting, model IDs and tool capabilities can change. Check current official documentation before production implementation.

## Supplementary — AI Hero

### AI Engineer role

- What Is An AI Engineer?: https://www.aihero.dev/what-is-an-ai-engineer

Useful for:

```text
AI Engineer vs ML Engineer
AI Engineer vs prompt-only role
AI Engineer vs AI-assisted developer
software-engineering-first framing
```

### LLM fundamentals

- Messages, System Prompts and Reasoning Tokens: https://www.aihero.dev/messages-system-prompts-and-reasoning-tokens
- LLM Fundamentals series: https://www.aihero.dev/llm-fundamentals

Useful for:

```text
message/context mental model
system prompt
reasoning effort/tokens
model-provider request loop
tools/agents/context window
```

### AI coding vocabulary

- AI Coding Dictionary: https://www.aihero.dev/ai-coding-dictionary

Useful vocabulary categories:

```text
model/provider/harness
session/context/turn
tools/environment
failure modes
handoffs
memory/steering
patterns of work
AX/DX
```

The dictionary is intentionally treated as a terminology/mental-model source, not a normative provider specification.

## Cross-module references

- [AI Coding Agents](../21-ai-coding-agents/README.md) — repository agents, permissions, context and safe coding workflow.
- [Distributed Systems](../17-distributed-systems/README.md) — timeout, unknown outcome, retry, idempotency and reconciliation for tool side effects.
- [Security & DevSecOps](../09-security-devsecops/README.md) — trust boundaries, identity, secrets and supply chain.
- [System Design](../24-system-design/README.md) — latency, capacity, failure and cost.
- [Software Architecture](../25-software-architecture/README.md) — boundaries, quality attributes and evolution.

## Verification metadata

- Verified: 2026-09-03.
- Supplementary AI Hero pages reviewed on 2026-09-03.
- Version-sensitive behavior follows current official documentation and the repository [Technology Baseline](../00-roadmap/technology-baseline.md).
