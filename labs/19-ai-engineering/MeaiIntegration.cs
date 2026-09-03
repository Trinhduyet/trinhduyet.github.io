using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AiEngineeringLab;

/// <summary>
/// Runnable Microsoft.Extensions.AI integration exercises.
/// The default verification path is deterministic and does not require an API key.
/// </summary>
public static class MeaiLab
{
    public static async Task<int> RunAsync(string[] args)
    {
        string command = args.FirstOrDefault()?.Trim().ToLowerInvariant() ?? "meai-self-test";

        try
        {
            return command switch
            {
                "meai-demo" => await DemoAsync(),
                "meai-self-test" => await SelfTestAsync(),
                "meai-live" => await LiveProviderAsync(),
                _ => 2
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"MEAI ERROR: {exception.GetType().Name}: {exception.Message}");
            return 1;
        }
    }

    private static async Task<int> DemoAsync()
    {
        using IChatClient client = BuildDeterministicPipeline();

        Console.WriteLine("== Microsoft.Extensions.AI typed structured output ==");
        ChatResponse<RiskDecision> typed = await client.GetResponseAsync<RiskDecision>(
            "Classify risk for a large cross-border transfer.",
            useJsonSchemaResponseFormat: true);

        Console.WriteLine($"level={typed.Result.Level} humanReview={typed.Result.RequiresHumanReview}");

        Console.WriteLine("\n== Microsoft.Extensions.AI automatic function invocation ==");
        AIFunction getOrderStatus = AIFunctionFactory.Create(
            (string orderId) => orderId == "100" ? "Shipped" : "NotFound",
            name: "get_order_status",
            description: "Read the current status for an order ID.");

        ChatResponse toolResponse = await client.GetResponseAsync(
            "What is order 100 status?",
            new ChatOptions { Tools = [getOrderStatus] });

        Console.WriteLine(toolResponse.Text);
        Console.WriteLine("\nRun 'meai-self-test' next.");
        return 0;
    }

    private static async Task<int> SelfTestAsync()
    {
        using IChatClient client = BuildDeterministicPipeline();

        ChatResponse<RiskDecision> typed = await client.GetResponseAsync<RiskDecision>(
            "Classify risk for a large cross-border transfer.",
            useJsonSchemaResponseFormat: true);

        Assert(typed.Result.Level == "HIGH", "MEAI typed structured output level");
        Assert(typed.Result.RequiresHumanReview, "MEAI typed structured output business field");

        AIFunction getOrderStatus = AIFunctionFactory.Create(
            (string orderId) => orderId == "100" ? "Shipped" : "NotFound",
            name: "get_order_status",
            description: "Read the current status for an order ID.");

        ChatResponse toolResponse = await client.GetResponseAsync(
            "What is order 100 status?",
            new ChatOptions
            {
                Tools = [getOrderStatus],
                MaxOutputTokens = 200
            });

        Assert(
            toolResponse.Text.Contains("Shipped", StringComparison.OrdinalIgnoreCase),
            "MEAI automatic function invocation");

        ChatResponse low = await client.GetResponseAsync(
            "Explain the exception window.",
            new ChatOptions
            {
                Reasoning = new ReasoningOptions
                {
                    Effort = Microsoft.Extensions.AI.ReasoningEffort.Low
                }
            });

        ChatResponse high = await client.GetResponseAsync(
            "Explain the exception window.",
            new ChatOptions
            {
                Reasoning = new ReasoningOptions
                {
                    Effort = Microsoft.Extensions.AI.ReasoningEffort.High
                }
            });

        long lowReasoning = low.Usage?.ReasoningTokenCount ?? 0;
        long highReasoning = high.Usage?.ReasoningTokenCount ?? 0;
        Assert(highReasoning > lowReasoning, "MEAI reasoning usage shape");

        Console.WriteLine("MEAI SELF-TEST PASS");
        return 0;
    }

    private static async Task<int> LiveProviderAsync()
    {
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("Set OPENAI_API_KEY before running meai-live.");

        string model = Environment.GetEnvironmentVariable("OPENAI_MODEL")
            ?? throw new InvalidOperationException("Set OPENAI_MODEL before running meai-live.");

        using IChatClient client = CreateOpenAiClient(apiKey, model);

        ChatResponse response = await client.GetResponseAsync(
            [
                new ChatMessage(
                    ChatRole.System,
                    "You are a production AI engineering tutor. Be concise and technically precise."),
                new ChatMessage(
                    ChatRole.User,
                    "Explain why Microsoft.Extensions.AI is an integration abstraction rather than a security boundary.")
            ]);

        Console.WriteLine(response.Text);
        Console.WriteLine(
            $"usage input={response.Usage?.InputTokenCount ?? 0} output={response.Usage?.OutputTokenCount ?? 0} reasoning={response.Usage?.ReasoningTokenCount ?? 0}");
        return 0;
    }

    public static IChatClient CreateOpenAiClient(string apiKey, string model)
    {
        IChatClient providerClient = new OpenAIClient(apiKey)
            .GetChatClient(model)
            .AsIChatClient();

        return new ChatClientBuilder(providerClient)
            .UseFunctionInvocation()
            .Build();
    }

    private static IChatClient BuildDeterministicPipeline()
    {
        IChatClient providerClient = new DeterministicMeaiChatClient();

        return new ChatClientBuilder(providerClient)
            .UseFunctionInvocation()
            .Build();
    }

    private static void Assert(bool condition, string name)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Assertion failed: {name}");
        }
    }
}

/// <summary>
/// A deterministic provider implementation of IChatClient used to verify MEAI behavior in CI.
/// It intentionally models provider output; application safety still lives outside this class.
/// </summary>
public sealed class DeterministicMeaiChatClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ChatMessage[] history = messages.ToArray();

        FunctionResultContent? functionResult = history
            .SelectMany(message => message.Contents)
            .OfType<FunctionResultContent>()
            .LastOrDefault();

        if (functionResult is not null)
        {
            return Task.FromResult(Response(
                $"Order 100 status: {functionResult.Result}.",
                options));
        }

        string userText = history
            .LastOrDefault(message => message.Role == ChatRole.User)?.Text
            ?? string.Empty;

        if (userText.Contains("order 100", StringComparison.OrdinalIgnoreCase))
        {
            ChatMessage toolRequest = new(
                ChatRole.Assistant,
                [
                    new FunctionCallContent(
                        "call-order-100",
                        "get_order_status",
                        new Dictionary<string, object?>
                        {
                            ["orderId"] = "100"
                        })
                ]);

            return Task.FromResult(new ChatResponse(toolRequest)
            {
                Usage = Usage(options, input: 30, output: 8)
            });
        }

        if (userText.Contains("classify risk", StringComparison.OrdinalIgnoreCase))
        {
            const string json = """
                {
                  "level": "HIGH",
                  "reason": "Large cross-border transfer requires review.",
                  "requiresHumanReview": true
                }
                """;

            return Task.FromResult(Response(json, options));
        }

        if (userText.Contains("exception window", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Response(
                "Exception requests must be submitted within 7 days after denial.",
                options));
        }

        return Task.FromResult(Response(
            "Insufficient authorized context. Escalate instead of guessing.",
            options));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = messages;
        _ = options;
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        yield break;
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public void Dispose()
    {
    }

    private static ChatResponse Response(string text, ChatOptions? options) =>
        new(new ChatMessage(ChatRole.Assistant, text))
        {
            Usage = Usage(options, input: 24, output: Math.Max(1, text.Length / 4))
        };

    private static UsageDetails Usage(ChatOptions? options, long input, long output)
    {
        long reasoning = options?.Reasoning?.Effort switch
        {
            Microsoft.Extensions.AI.ReasoningEffort.Low => 16,
            Microsoft.Extensions.AI.ReasoningEffort.Medium => 64,
            Microsoft.Extensions.AI.ReasoningEffort.High => 256,
            Microsoft.Extensions.AI.ReasoningEffort.ExtraHigh => 512,
            _ => 0
        };

        return new UsageDetails
        {
            InputTokenCount = input,
            OutputTokenCount = output + reasoning,
            ReasoningTokenCount = reasoning,
            TotalTokenCount = input + output + reasoning
        };
    }
}
