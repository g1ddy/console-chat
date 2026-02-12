using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace ConsoleChat.Benchmarks;

[MemoryDiagnoser]
public class JsonValidationBenchmark
{
    // Scenarios:
    // 1. Valid JSON (Small)
    // 2. Valid JSON (Large - simulated by just repetition, though simple is enough to test parsing overhead)
    // 3. Invalid JSON (Plain text - very common in chat output)
    // 4. Invalid JSON (Looks like JSON but malformed)

    [Params("{\"foo\":\"bar\"}", "Plain text message", "{\"broken\":")]
    public string? Json { get; set; }

    [Benchmark(Baseline = true)]
    public bool Original()
    {
        // Simulate original implementation (buggy, no dispose)
        if (string.IsNullOrWhiteSpace(Json))
        {
            return false;
        }

        try
        {
            _ = JsonDocument.Parse(Json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    [Benchmark]
    public bool WithHeuristicAndDispose()
    {
        if (string.IsNullOrWhiteSpace(Json)) return false;

        var text = Json.AsSpan().Trim();
        if ((text.StartsWith("{") && text.EndsWith("}")) || (text.StartsWith("[") && text.EndsWith("]")))
        {
            try
            {
                using var doc = JsonDocument.Parse(Json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        return false;
    }
}
