using System.Diagnostics;

internal static class Program
{
    private const int LookupMaxSize = 200_000;
    private const int LookupMaxQueries = 20_000;
    private const long LookupMaxComparisons = 500_000_000;
    private const int RaceMaxWorkers = 64;
    private const long RaceMaxOperations = 20_000_000;
    private const int LocalityMaxSize = 5_000_000;
    private const long LocalityMaxVisits = 50_000_000;

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            PrintUsage();
            return args.Length == 0 ? 64 : 0;
        }

        try
        {
            PrintRuntimeContext();

            return args[0].ToLowerInvariant() switch
            {
                "lookup" => RunLookup(args),
                "race" => RunRace(args),
                "locality" => RunLocality(args),
                _ => Fail($"Unknown command: {args[0]}")
            };
        }
        catch (ArgumentException exception)
        {
            return Fail(exception.Message);
        }
    }

    private static int RunLookup(string[] args)
    {
        EnsureArgumentCount(args, 3);
        var size = ReadPositiveInt(args, 1, 20_000, LookupMaxSize, "size");
        var queryCount = ReadPositiveInt(args, 2, 5_000, LookupMaxQueries, "queries");
        var comparisonBudget = (long)size * queryCount;

        if (comparisonBudget > LookupMaxComparisons)
        {
            throw new ArgumentException(
                $"size * queries must be <= {LookupMaxComparisons:N0}; requested {comparisonBudget:N0}.");
        }

        var values = new List<int>(size);
        for (var value = 0; value < size; value++)
        {
            values.Add(value * 2);
        }

        var queries = BuildQueries(size, queryCount);
        _ = CountListHits(values, queries.AsSpan(0, Math.Min(128, queries.Length)));

        var listElapsed = Measure(() => CountListHits(values, queries), out var listHits);

        var buildElapsed = Measure(() => new HashSet<int>(values), out var index);
        var setElapsed = Measure(() => CountSetHits(index, queries), out var setHits);

        Console.WriteLine("experiment=lookup");
        Console.WriteLine($"size={size:N0} queries={queryCount:N0} theoretical_list_comparison_ceiling={comparisonBudget:N0}");
        Console.WriteLine($"list_hits={listHits:N0} list_lookup_ms={listElapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"hashset_build_ms={buildElapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"hashset_hits={setHits:N0} hashset_lookup_ms={setElapsed.TotalMilliseconds:F3}");
        Console.WriteLine("interpretation=Separate index construction cost from query cost; repeat with your real cardinality and distribution.");

        return listHits == setHits ? 0 : 1;
    }

    private static int RunRace(string[] args)
    {
        EnsureArgumentCount(args, 3);
        var workers = ReadPositiveInt(args, 1, Math.Min(Environment.ProcessorCount, 8), RaceMaxWorkers, "workers");
        var iterations = ReadPositiveInt(args, 2, 100_000, int.MaxValue, "iterations");
        var expected = (long)workers * iterations;

        if (expected > RaceMaxOperations)
        {
            throw new ArgumentException(
                $"workers * iterations must be <= {RaceMaxOperations:N0}; requested {expected:N0}.");
        }

        var unsafeCounter = 0;
        var unsafeElapsed = Measure(
            () => Parallel.For(0, workers, _ =>
            {
                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    var snapshot = unsafeCounter;
                    if ((iteration & 127) == 0)
                    {
                        Thread.Yield();
                    }

                    unsafeCounter = snapshot + 1;
                }
            }),
            out _);

        var safeCounter = 0;
        var safeElapsed = Measure(
            () => Parallel.For(0, workers, _ =>
            {
                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    Interlocked.Increment(ref safeCounter);
                }
            }),
            out _);

        Console.WriteLine("experiment=race");
        Console.WriteLine($"workers={workers} iterations_per_worker={iterations:N0} expected={expected:N0}");
        Console.WriteLine($"unsafe_actual={unsafeCounter:N0} unsafe_lost_updates={expected - unsafeCounter:N0} unsafe_ms={unsafeElapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"interlocked_actual={safeCounter:N0} interlocked_ms={safeElapsed.TotalMilliseconds:F3}");
        Console.WriteLine("interpretation=counter++ is read-modify-write, not an atomic cross-thread operation.");

        return safeCounter == expected ? 0 : 1;
    }

    private static int RunLocality(string[] args)
    {
        EnsureArgumentCount(args, 3);
        var size = ReadPositiveInt(args, 1, 2_000_000, LocalityMaxSize, "size");
        var passes = ReadPositiveInt(args, 2, 5, 20, "passes");
        var visits = (long)size * passes;

        if (visits > LocalityMaxVisits)
        {
            throw new ArgumentException(
                $"size * passes must be <= {LocalityMaxVisits:N0}; requested {visits:N0}.");
        }

        var data = new int[size];
        var indices = new int[size];
        for (var index = 0; index < size; index++)
        {
            data[index] = index & 1023;
            indices[index] = index;
        }

        Shuffle(indices, seed: 42);
        _ = SumSequential(data, 1);
        _ = SumRandom(data, indices, 1);

        var sequentialElapsed = Measure(() => SumSequential(data, passes), out var sequentialChecksum);
        var randomElapsed = Measure(() => SumRandom(data, indices, passes), out var randomChecksum);
        var ratio = sequentialElapsed.TotalMilliseconds == 0
            ? double.NaN
            : randomElapsed.TotalMilliseconds / sequentialElapsed.TotalMilliseconds;

        Console.WriteLine("experiment=locality");
        Console.WriteLine($"elements={size:N0} passes={passes} visits={visits:N0} approximate_array_bytes={(long)size * sizeof(int) * 2:N0}");
        Console.WriteLine($"sequential_checksum={sequentialChecksum} sequential_ms={sequentialElapsed.TotalMilliseconds:F3}");
        Console.WriteLine($"random_checksum={randomChecksum} random_ms={randomElapsed.TotalMilliseconds:F3} random_over_sequential={ratio:F2}x");
        Console.WriteLine("interpretation=Same Big-O and same values can produce different wall time because access locality differs.");

        GC.KeepAlive(data);
        GC.KeepAlive(indices);
        return sequentialChecksum == randomChecksum ? 0 : 1;
    }

    private static int[] BuildQueries(int size, int queryCount)
    {
        var queries = new int[queryCount];
        var random = new Random(42);

        for (var index = 0; index < queryCount; index++)
        {
            queries[index] = index % 2 == 0
                ? random.Next(size) * 2
                : (random.Next(size) * 2) + 1;
        }

        return queries;
    }

    private static int CountListHits(List<int> values, ReadOnlySpan<int> queries)
    {
        var hits = 0;
        foreach (var query in queries)
        {
            if (values.Contains(query))
            {
                hits++;
            }
        }

        return hits;
    }

    private static int CountSetHits(HashSet<int> values, ReadOnlySpan<int> queries)
    {
        var hits = 0;
        foreach (var query in queries)
        {
            if (values.Contains(query))
            {
                hits++;
            }
        }

        return hits;
    }

    private static long SumSequential(int[] data, int passes)
    {
        long sum = 0;
        for (var pass = 0; pass < passes; pass++)
        {
            for (var index = 0; index < data.Length; index++)
            {
                sum += data[index];
            }
        }

        return sum;
    }

    private static long SumRandom(int[] data, int[] indices, int passes)
    {
        long sum = 0;
        for (var pass = 0; pass < passes; pass++)
        {
            for (var index = 0; index < indices.Length; index++)
            {
                sum += data[indices[index]];
            }
        }

        return sum;
    }

    private static void Shuffle(int[] values, int seed)
    {
        var random = new Random(seed);
        for (var index = values.Length - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }

    private static TimeSpan Measure<T>(Func<T> operation, out T result)
    {
        var stopwatch = Stopwatch.StartNew();
        result = operation();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private static int ReadPositiveInt(string[] args, int index, int defaultValue, int maximum, string name)
    {
        if (index >= args.Length)
        {
            return defaultValue;
        }

        if (!int.TryParse(args[index], out var value) || value <= 0 || value > maximum)
        {
            throw new ArgumentException($"{name} must be an integer in range 1..{maximum:N0}.");
        }

        return value;
    }

    private static void EnsureArgumentCount(string[] args, int maximum)
    {
        if (args.Length > maximum)
        {
            throw new ArgumentException($"Too many arguments for command '{args[0]}'.");
        }
    }

    private static void PrintRuntimeContext()
    {
        Console.WriteLine($"runtime={Environment.Version} os={Environment.OSVersion.Platform} architecture={System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture} logical_processors={Environment.ProcessorCount}");
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"error: {message}");
        PrintUsage();
        return 64;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("WorkloadLab - bounded educational experiments for .NET 10");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project WorkloadLab.csproj -- lookup [size] [queries]");
        Console.WriteLine("  dotnet run --project WorkloadLab.csproj -- race [workers] [iterations]");
        Console.WriteLine("  dotnet run --project WorkloadLab.csproj -- locality [size] [passes]");
        Console.WriteLine();
        Console.WriteLine("Defaults:");
        Console.WriteLine("  lookup   size=20000, queries=5000");
        Console.WriteLine("  race     workers=min(logical processors, 8), iterations=100000");
        Console.WriteLine("  locality size=2000000, passes=5");
    }
}
