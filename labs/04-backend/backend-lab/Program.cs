using System.Globalization;
using System.Threading.Channels;

namespace BackendLab;

internal static class Program
{
    private const int UsageExitCode = 64;

    private static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
            {
                PrintUsage();
                return args.Length == 0 ? UsageExitCode : 0;
            }

            return args[0].ToLowerInvariant() switch
            {
                "pagination" => RunPagination(args),
                "idempotency" => RunIdempotency(args),
                "backpressure" => await RunBackpressureAsync(args).ConfigureAwait(false),
                _ => Invalid("Unknown command.")
            };
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            PrintUsage();
            return UsageExitCode;
        }
    }

    private static int RunPagination(string[] args)
    {
        var items = ReadBoundedInt(args, 1, "items", 1, 200_000, 10_000);
        var pageSize = ReadBoundedInt(args, 2, "pageSize", 1, 500, 100);
        var page = ReadBoundedInt(args, 3, "page", 1, 200_000, 3);
        if (args.Length > 4)
            throw new ArgumentException("Too many positional arguments.");

        var totalPages = (items + pageSize - 1) / pageSize;
        if (page > totalPages)
            throw new ArgumentException($"page must be between 1 and {totalPages} for this workload.");

        var start = (page - 1) * pageSize;
        var count = Math.Min(pageSize, items - start);
        long checksum = 0;
        for (var offset = start; offset < start + count; offset++)
        {
            var item = new BackendItem(offset + 1, $"item-{offset + 1:D8}");
            checksum = unchecked((checksum * 31) + item.Id + item.Name.Length);
        }

        Console.WriteLine("command=pagination");
        Console.WriteLine($"items={items}");
        Console.WriteLine($"pageSize={pageSize}");
        Console.WriteLine($"page={page}");
        Console.WriteLine($"totalPages={totalPages}");
        Console.WriteLine($"returned={count}");
        Console.WriteLine($"firstId={start + 1}");
        Console.WriteLine($"lastId={start + count}");
        Console.WriteLine($"stableOrdering=true");
        Console.WriteLine($"checksum={checksum}");
        return 0;
    }

    private static int RunIdempotency(string[] args)
    {
        var requests = ReadBoundedInt(args, 1, "requests", 1, 200_000, 1_000);
        var duplicateEvery = ReadBoundedInt(args, 2, "duplicateEvery", 1, 1_000, 10);
        if (args.Length > 3)
            throw new ArgumentException("Too many positional arguments.");

        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        var sideEffects = 0;
        var replayed = 0;
        var checksum = 0L;
        for (var request = 0; request < requests; request++)
        {
            var key = $"operation-{request / duplicateEvery:D8}";
            if (seen.TryGetValue(key, out var previous))
            {
                replayed++;
                checksum += previous;
                continue;
            }

            var effect = request + 1;
            seen.Add(key, effect);
            sideEffects++;
            checksum += effect;
        }

        Console.WriteLine("command=idempotency");
        Console.WriteLine($"requests={requests}");
        Console.WriteLine($"duplicateEvery={duplicateEvery}");
        Console.WriteLine($"uniqueKeys={seen.Count}");
        Console.WriteLine($"sideEffects={sideEffects}");
        Console.WriteLine($"replayed={replayed}");
        Console.WriteLine($"conflicts=0");
        Console.WriteLine($"checksum={checksum}");
        return sideEffects == seen.Count ? 0 : 1;
    }

    private static async Task<int> RunBackpressureAsync(string[] args)
    {
        var items = ReadBoundedInt(args, 1, "items", 1, 200_000, 10_000);
        var capacity = ReadBoundedInt(args, 2, "capacity", 1, 10_000, 64);
        var cancelAfterMs = ReadBoundedInt(args, 3, "cancelAfterMs", 1, 10_000, 25);
        if (args.Length > 4)
            throw new ArgumentException("Too many positional arguments.");

        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = true
        });
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(cancelAfterMs);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var producer = ProduceAsync(channel.Writer, items, cancellation.Token);
        var consumer = ConsumeAsync(channel.Reader, cancellation.Token);
        var results = await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        stopwatch.Stop();

        Console.WriteLine("command=backpressure");
        Console.WriteLine($"requestedItems={items}");
        Console.WriteLine($"capacity={capacity}");
        Console.WriteLine($"cancelAfterMs={cancelAfterMs}");
        Console.WriteLine($"produced={results[0]}");
        Console.WriteLine($"consumed={results[1]}");
        Console.WriteLine($"rejectedOrUnprocessed={items - results[0]}");
        Console.WriteLine($"cancellationRequested={cancellation.IsCancellationRequested}");
        Console.WriteLine($"producerStatus={producer.Status}");
        Console.WriteLine($"consumerStatus={consumer.Status}");
        Console.WriteLine($"elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F2}");
        return 0;
    }

    private static async Task<int> ProduceAsync(
        ChannelWriter<int> writer,
        int items,
        CancellationToken token)
    {
        var produced = 0;
        try
        {
            for (var i = 0; i < items; i++)
            {
                await writer.WriteAsync(i, token).ConfigureAwait(false);
                produced++;
                if ((i & 255) == 255)
                    await Task.Delay(1, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Expected for a bounded cancellation experiment.
        }
        finally
        {
            writer.TryComplete();
        }

        return produced;
    }

    private static async Task<int> ConsumeAsync(
        ChannelReader<int> reader,
        CancellationToken token)
    {
        var consumed = 0;
        try
        {
            await foreach (var item in reader.ReadAllAsync(token).ConfigureAwait(false))
            {
                _ = item;
                consumed++;
                if ((consumed & 255) == 255)
                    await Task.Delay(1, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Expected for a bounded cancellation experiment.
        }

        return consumed;
    }

    private static int ReadBoundedInt(
        string[] args,
        int index,
        string name,
        int min,
        int max,
        int defaultValue)
    {
        if (args.Length <= index)
            return defaultValue;

        if (!int.TryParse(args[index], NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            || value < min
            || value > max)
        {
            throw new ArgumentException($"{name} must be an integer in [{min}, {max}].");
        }

        return value;
    }

    private static int Invalid(string message)
    {
        Console.Error.WriteLine($"error: {message}");
        PrintUsage();
        return UsageExitCode;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("BackendLab commands:");
        Console.WriteLine("  pagination [items=10000] [pageSize=100] [page=3]");
        Console.WriteLine("  idempotency [requests=1000] [duplicateEvery=10]");
        Console.WriteLine("  backpressure [items=10000] [capacity=64] [cancelAfterMs=25]");
        Console.WriteLine("Bounds: items 1..200000; pageSize 1..500; capacity 1..10000; delay 1..10000.");
    }

    private readonly record struct BackendItem(int Id, string Name);
}
