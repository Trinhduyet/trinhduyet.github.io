# AI Platform Architecture — Model Serving, Gateway, vLLM, llama.cpp & Evaluation

> [← AI Engineering](README.md) · [LLM Runtime](llm-runtime-messages-context-and-reasoning.md) · [RAG & Evaluation](rag-evaluation-and-observability.md) · [Architecture Modeling](../25-software-architecture/architecture-modeling-event-storming-and-c4.md)

**Priority P2** với Solution Architect tổng quát, **P0/P1** nếu hệ thống có AI trong critical business flow. Mục tiêu là thiết kế serving layer theo security, latency, cost, quality và governance; không chọn `vLLM`/`LangChain` chỉ vì được nhắc nhiều.

## 1. AI system boundary

```text
Authenticated Client
  → Product API (identity, business authorization, budget)
  → AI Orchestrator (prompts, retrieval, tools, validation)
  → AI Gateway (routing, quota, telemetry, policy)
  → Provider API OR self-hosted inference server
  → Structured validation + output policy
  → Product action (authorized, idempotent)
```

**Cần tách ba data planes:** (1) user/product API, (2) retrieval/context & tool calls, (3) model inference. Một response `200` từ model không chứng minh câu trả lời đúng, user có quyền truy cập nguồn, hoặc side effect an toàn.

## 2. Buy vs host vs hybrid

| Option | Pros | Costs / constraints |
|---|---|---|
| Managed model API | Fast to start, provider handles serving infrastructure | Data boundary, vendor availability/pricing, latency/quotas, egress |
| Self-host on GPU | Deployment/data/control flexibility, optimize specific workload | GPU capacity, batching, scheduling, updates, observability, security |
| Local CPU/GPU runtime | Edge/on-prem, low dependency | Smaller throughput/model constraints, device fleet and lifecycle |
| Hybrid routing | Multiple model/provider options, fallback | Quality drift, prompt/schema compatibility, evaluation complexity |

**Ràng buộc cần hỏi:** region/data residency, PII, tool execution rights, tenant isolation, max context, quality target, time-to-first-token, tokens/second, availability, burst behavior, cost per task và operator capability.

## 3. Model serving tools — khác nhau về vai trò

| Tool / approach | Vai trò chính | Cần kiểm chứng |
|---|---|---|
| **vLLM** | High-throughput inference serving, batching/scheduling trên supported hardware/models | Supported model/hardware, KV cache, latency under burst, deployment security |
| **llama.cpp** | Native local/edge inference runtime, quantization và deployment flexibility | Model accuracy after quantization, hardware constraints, memory/throughput |
| **Ollama** | Developer/local serving convenience | Prod HA, auth, observability, capacity và hardening requirements |
| **LangChain / LangGraph** | Python-side orchestration/chains/graphs, không phải inference server | State/retry/stream semantics, prompt safety, tool authz, code ownership |
| **Microsoft.Extensions.AI** | .NET abstractions for chat/embeddings/tools/middleware | Provider compatibility, structured output, cancellation, telemetry |
| **Model gateway** | Routing, quotas, policy, logging, budgets, provider failover | Sensitive content logging, tenant fairness, silent quality regression |

Chọn runtime và orchestrator riêng biệt. “Hỗ trợ OpenAI-compatible API” không mặc nhiên tương thích mọi parameter, tool calling, structured schema, streaming và error model.

## 4. Capacity planning khác API truyền thống

**Thông số quan trọng:**

- Input/output token distributions (p50/p95), context length, prompt cache hit rate.
- **TTFT** (Time To First Token) và output throughput, không chỉ HTTP latency.
- GPU memory theo model weights, KV cache, batch size và concurrency.
- Request queue length, admission control, max waiting time, streaming cancellation.
- Throughput theo token/s dưới workload thực tế, không chỉ req/s trung bình.
- Embedding/retrieval vector queries, document ingestion & reindex cost.

### Ví dụ capacity minh họa

```text
10 requests/second at peak
500 input tokens and 300 output tokens per request (mean)
≈ 3,000 generated output tokens/second aggregate demand
```

Đây chỉ là demand estimate trước overhead; **không** thể suy ra số GPU nếu không benchmark model, context, batching, hardware, TTFT SLO và quantization.

## 5. RAG, AI Gateway và multi-tenant security

1. Auth user/tenant ở product API; policy quyết định nguồn và tool người dùng được phép sử dụng.
2. Vector/keyword retrieval bắt buộc filter ACL/tenant trước hoặc trong truy vấn và kiểm tra lại kết quả; không chỉ filter sau khi đưa docs vào prompt.
3. Prompt/context từ web, PDF hay user nhập là **untrusted input**. Không để văn bản đó cấp quyền tool, ghi đè system policy hoặc đọc secret.
4. AI Gateway áp quota/cost per tenant, request ID, model routing và timeout. Redact PII và secret trước log/tracing.
5. Tool execution có schema validation, least privilege, authorization theo resource, approval cho side effects và idempotency/reconciliation khi gọi systems bên ngoài.
6. Mọi provider fallback phải có quality evaluation; không tự động đổi sang model khác và khẳng định output tương đương.

## 6. Production evaluation & rollout

| Evaluation | Metric / expectation |
|---|---|
| Correctness | Task success/groundedness trên golden dataset đã review |
| Safety | PII leakage, prompt injection success, unsafe tool-call rate |
| Retrieval | Recall@k, precision, ACL leakage = 0 theo test fixtures |
| Runtime | TTFT p95, tokens/sec, timeout/error rate, GPU utilization |
| Cost | Cost per successful task, GPU hours, inference + retrieval costs |
| Operations | Queue age, model cold start, crash recovery, version traceability |

**Release gate:** golden regression + adversarial tests + canary; rollback phải version cả prompt, retrieval pipeline, tool schema và model runtime, không chỉ image tag.

## 7. Failure scenarios

| Fault | Degradation & recovery |
|---|---|
| Provider 429/outage | Quota-aware admission, bounded retry, circuit breaker; fallback chỉ khi policy/eval cho phép |
| GPU OOM / KV cache pressure | Limit concurrency/context, queue/backpressure, graceful 503 và capacity planning |
| Prompt injection via retrieved text | Treat context as data; tool policy outside prompt; record blocked attempts |
| AI tool timeout after side effect | Stable idempotency reference, UNKNOWN state/status reconciliation |
| Model update changes structured output | Schema validation + golden regression + canary/rollback |
| Tenant retrieval leak | Resource ACL enforcement at retrieval and response boundaries; negative tests |

## 8. Lab / architecture artifacts

1. So sánh managed model API và một local serving runtime trên cùng 100 tasks; đo TTFT, tokens/s, latency, cost.
2. Áp AI Gateway quota theo tenant, bounded retries, request cancellation và tracing không log PII.
3. RAG với hai tenants có documents trùng tên; test user A không bao giờ retrieve tài liệu của B.
4. Inject prompt injection yêu cầu gọi privileged tool; verify tool authorization độc lập prompt.
5. Canary một model/prompt mới; chạy golden regression; rollback khi vượt budget/quality threshold.
6. ADR: managed vs self-host vs hybrid, gồm region/privacy, team ops, cost model và revisit trigger.

**Definition of Done:** chỉ có bản demo chat chưa chứng minh AI production. Phải có eval/safety/latency/cost evidence và hành vi failure đo được.

**Nguồn chính:** [vLLM Documentation](https://docs.vllm.ai/) · [llama.cpp](https://github.com/ggml-org/llama.cpp) · [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai) · [Awesome — AI Resources](https://github.com/mehdihadeli/awesome-software-architecture/tree/main/docs/ai).
