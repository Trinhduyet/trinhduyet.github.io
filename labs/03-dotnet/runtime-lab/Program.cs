using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace RuntimeLab;

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
                "cancellation" => await RunCancellationAsync(args).ConfigureAwait(false),
                "allocation" => RunAllocation(args),
                "diagnostics" => RunDiagnostics(args),
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

    private static async Task<int> RunCancellationAsync(string[] args)
    {
        var items = ReadBoundedInt(args, 1, "items", 1, 200_000, 10_000);
        var cancelAfterMs = ReadBoundedInt(args, 2, "cancelAfterMs", 1, 10_000, 25);
        if (args.Length > 3)
            throw new ArgumentException("Too many positional arguments.");
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(256)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = true
        });

        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(cancelAfterMs);
        var token = cancellation.Token;
        var stopwatch = Stopwatch.StartNew();
        var producer = ProduceAsync(channel.Writer, items, token);
        var consumer = ConsumeAsync(channel.Reader, token);
        var results = await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        stopwatch.Stop();

        Console.WriteLine("command=cancellation");
        Console.WriteLine($"requestedItems={items}");
        Console.WriteLine($"cancelAfterMs={cancelAfterMs}");
        Console.WriteLine($"produced={results[0]}");
        Console.WriteLine($"consumed={results[1]}");
        Console.WriteLine($"cancellationRequested={token.IsCancellationRequested}");
        Console.WriteLine($"producerStatus={producer.Status}");
        Console.WriteLine($"consumerStatus={consumer.Status}");
        Console.WriteLine($"elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F2}");
        return 0;
    }

    private static async Task<int> ProduceAsync(ChannelWriter<int> writer, int items, CancellationToken token)
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
            // Cancellation is expected for this experiment.
        }
        finally
        {
            writer.TryComplete();
        }

        return produced;
    }

    private static async Task<int> ConsumeAsync(ChannelReader<int> reader, CancellationToken token)
    {
        var consumed = 0;
        try
        {
            await foreach (var item in reader.ReadAllAsync(token).ConfigureAwait(false))
            {
                consumed++;
                _ = item;
                if ((consumed & 255) == 255)
                    await Task.Delay(1, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Cancellation is expected for this experiment.
        }

        return consumed;
    }

    private static int RunAllocation(string[] args)
    {
        var items = ReadBoundedInt(args, 1, "items", 1, 1_000_000, 100_000);
        if (args.Length > 2)
            throw new ArgumentException("Too many positional arguments.");
        WarmUpAllocationPath();

        var naive = Measure(() => AllocateByteArrays(items));
        var pooled = Measure(() => AllocatePooledByteArrays(items));
        var checksumMatches = naive.Checksum == pooled.Checksum;

        Console.WriteLine("command=allocation");
        Console.WriteLine($"items={items}");
        Console.WriteLine($"checksumNaive={naive.Checksum}");
        Console.WriteLine($"checksumPooled={pooled.Checksum}");
        Console.WriteLine($"checksumMatches={checksumMatches}");
        Console.WriteLine($"naiveAllocatedBytes={naive.AllocatedBytes}");
        Console.WriteLine($"pooledAllocatedBytes={pooled.AllocatedBytes}");
        Console.WriteLine($"naiveGen0Delta={naive.Gen0Collections}");
        Console.WriteLine($"pooledGen0Delta={pooled.Gen0Collections}");
        return checksumMatches ? 0 : 1;
    }

    private static void WarmUpAllocationPath()
    {
        _ = AllocateByteArrays(32);
        _ = AllocatePooledByteArrays(32);
    }

    private static AllocationResult AllocateByteArrays(int items)
    {
        long checksum = 0;
        for (var i = 0; i < items; i++)
        {
            var buffer = new byte[256];
            buffer[0] = (byte)(i & byte.MaxValue);
            buffer[^1] = (byte)((i * 31) & byte.MaxValue);
            checksum += buffer[0] + buffer[^1];
        }

        return new AllocationResult(checksum);
    }

    private static AllocationResult AllocatePooledByteArrays(int items)
    {
        long checksum = 0;
        var pool = ArrayPool<byte>.Shared;
        for (var i = 0; i < items; i++)
        {
            var buffer = pool.Rent(256);
            try
            {
                buffer[0] = (byte)(i & byte.MaxValue);
                buffer[255] = (byte)((i * 31) & byte.MaxValue);
                checksum += buffer[0] + buffer[255];
            }
            finally
            {
                pool.Return(buffer, clearArray: false);
            }
        }

        return new AllocationResult(checksum);
    }

    private static AllocationResult Measure(Func<AllocationResult> workload)
    {
        var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
        var beforeGen0 = GC.CollectionCount(0);
        var result = workload();
        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - beforeBytes;
        var gen0Collections = GC.CollectionCount(0) - beforeGen0;
        return result with { AllocatedBytes = allocatedBytes, Gen0Collections = gen0Collections };
    }

    private static int RunDiagnostics(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("diagnostics does not accept positional arguments.");

        using var process = Process.GetCurrentProcess();
        ThreadPool.GetMinThreads(out var minWorker, out var minIo);
        ThreadPool.GetMaxThreads(out var maxWorker, out var maxIo);
        ThreadPool.GetAvailableThreads(out var availableWorker, out var availableIo);
        var memory = GC.GetGCMemoryInfo();

        Console.WriteLine("command=diagnostics");
        Console.WriteLine($"framework={RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"runtimeVersion={Environment.Version}");
        Console.WriteLine($"os={RuntimeInformation.OSDescription}");
        Console.WriteLine($"architecture={RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"logicalProcessors={Environment.ProcessorCount}");
        Console.WriteLine($"serverGc={GCSettings.IsServerGC}");
        Console.WriteLine($"gcLatencyMode={GCSettings.LatencyMode}");
        Console.WriteLine($"heapSizeBytes={memory.HeapSizeBytes}");
        Console.WriteLine($"fragmentedBytes={memory.FragmentedBytes}");
        Console.WriteLine($"memoryLoadBytes={memory.MemoryLoadBytes}");
        Console.WriteLine($"totalAvailableMemoryBytes={memory.TotalAvailableMemoryBytes}");
        Console.WriteLine($"highMemoryLoadThresholdBytes={memory.HighMemoryLoadThresholdBytes}");
        Console.WriteLine($"pauseTimePercentage={memory.PauseTimePercentage:F2}");
        Console.WriteLine($"pinnedObjectsCount={memory.PinnedObjectsCount}");
        Console.WriteLine($"finalizationPendingCount={memory.FinalizationPendingCount}");
        Console.WriteLine($"gen0Collections={GC.CollectionCount(0)}");
        Console.WriteLine($"gen1Collections={GC.CollectionCount(1)}");
        Console.WriteLine($"gen2Collections={GC.CollectionCount(2)}");
        Console.WriteLine($"allocatedBytesCurrentThread={GC.GetAllocatedBytesForCurrentThread()}");
        Console.WriteLine($"workingSetBytes={process.WorkingSet64}");
        Console.WriteLine($"threadPoolMinWorker={minWorker}");
        Console.WriteLine($"threadPoolMinIo={minIo}");
        Console.WriteLine($"threadPoolMaxWorker={maxWorker}");
        Console.WriteLine($"threadPoolMaxIo={maxIo}");
        Console.WriteLine($"threadPoolAvailableWorker={availableWorker}");
        Console.WriteLine($"threadPoolAvailableIo={availableIo}");
        return 0;
    }

    private static int ReadBoundedInt(string[] args, int index, string name, int min, int max, int defaultValue)
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
        Console.WriteLine("RuntimeLab commands:");
        Console.WriteLine("  cancellation [items=10000] [cancelAfterMs=25]");
        Console.WriteLine("  allocation [items=100000]");
        Console.WriteLine("  diagnostics");
        Console.WriteLine("Bounds: cancellation items 1..200000, delay 1..10000; allocation items 1..1000000.");
    }

    private readonly record struct AllocationResult(
        long Checksum,
        long AllocatedBytes = 0,
        int Gen0Collections = 0);
}
