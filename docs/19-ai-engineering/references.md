# Module 19 — References

> Policy: **official/provider documentation là source of truth cho API/model/version semantics**. AI Hero được dùng như supplementary learning source cho vocabulary và mental model.

## Official — Microsoft.Extensions.AI / .NET AI

- Microsoft Learn — AI apps for .NET developers: https://learn.microsoft.com/en-us/dotnet/ai/
- Microsoft Learn — Develop .NET apps with AI features: https://learn.microsoft.com/en-us/dotnet/ai/overview
- Microsoft Learn — Microsoft.Extensions.AI libraries: https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai
- Microsoft Learn — `IChatClient` API: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient
- Microsoft Learn — `ChatOptions` API: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.chatoptions
- Microsoft Learn — `UsageDetails` API: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.usagedetails
- Microsoft Learn — AI tool calling: https://learn.microsoft.com/en-us/dotnet/ai/conceptual/ai-tools
- Microsoft Learn — `FunctionInvokingChatClient`: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.functioninvokingchatclient
- Microsoft Learn — structured output quickstart: https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/structured-output
- Microsoft Learn — prompt an AI model with .NET: https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/prompt-model
- Microsoft Learn — RAG concepts: https://learn.microsoft.com/en-us/dotnet/ai/conceptual/rag
- Microsoft Learn — AI Evaluation libraries: https://learn.microsoft.com/en-us/dotnet/ai/evaluation/libraries
- Microsoft Learn — evaluate with reporting: https://learn.microsoft.com/en-us/dotnet/ai/evaluation/evaluate-with-reporting
- Microsoft Learn — accessing data in AI functions: https://learn.microsoft.com/en-us/dotnet/ai/how-to/access-data-in-functions
- NuGet — `Microsoft.Extensions.AI`: https://www.nuget.org/packages/Microsoft.Extensions.AI
- NuGet — `Microsoft.Extensions.AI.OpenAI`: https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI
- GitHub — dotnet/extensions: https://github.com/dotnet/extensions
- GitHub — .NET AI samples: https://github.com/dotnet/ai-samples

### Repository baseline

```text
Microsoft.Extensions.AI          10.9.0
Microsoft.Extensions.AI.OpenAI   10.9.0
```

The executable lab pins exact package versions. Documentation concepts are kept separate from provider/model-specific behavior.

## Official — Azure OpenAI

- Microsoft Learn — Azure OpenAI .NET client: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai-readme
- Microsoft Learn — .NET AI quickstarts include Azure OpenAI `AsIChatClient()` integration: https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/prompt-model
- Azure Identity — `DefaultAzureCredential`: https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential

Production Azure deployments should prefer identity-based authentication when architecture/support allows and must check current deployment/model availability separately.

## Official — OpenAI

- OpenAI API — Models: https://platform.openai.com/docs/models
- OpenAI API — Text generation / message and instruction semantics: https://platform.openai.com/docs/guides/text
- OpenAI API — Reasoning: https://platform.openai.com/docs/guides/reasoning
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

- [Microsoft.Extensions.AI — .NET Integration Guide](microsoft-extensions-ai-dotnet-integration.md) — MEAI package layering, `IChatClient`, pipeline, tools, telemetry and evaluation integration.
- [AI Coding Agents](../21-ai-coding-agents/README.md) — repository agents, permissions, context and safe coding workflow.
- [Distributed Systems](../17-distributed-systems/README.md) — timeout, unknown outcome, retry, idempotency and reconciliation for tool side effects.
- [Security & DevSecOps](../09-security-devsecops/README.md) — trust boundaries, identity, secrets and supply chain.
- [System Design](../24-system-design/README.md) — latency, capacity, failure and cost.
- [Software Architecture](../25-software-architecture/README.md) — boundaries, quality attributes and evolution.

## Verification metadata

- Verified: 2026-09-03.
- `Microsoft.Extensions.AI` / `Microsoft.Extensions.AI.OpenAI` stable package line verified at `10.9.0`.
- Supplementary AI Hero pages reviewed on 2026-09-03.
- Version-sensitive behavior follows current official documentation and the repository [Technology Baseline](../00-roadmap/technology-baseline.md).
