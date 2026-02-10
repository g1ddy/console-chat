using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.AI;
using SemanticKernelChat.Console;
using Spectre.Console;
using Spectre.Console.Testing;

namespace ConsoleChat.Benchmarks;

[MemoryDiagnoser]
public class DisplayStreamingUpdatesBenchmark
{
    private ChatConsole _console;
    private TestConsole _testConsole;
    private List<ChatResponseUpdate> _updates;

    [Params(100)]
    public int UpdatesCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testConsole = new TestConsole();
        _testConsole.Profile.Capabilities.Interactive = true;
        _testConsole.Profile.Width = 80;

        _console = new ChatConsole(new DummyChatLineEditor(), _testConsole);

        _updates = new List<ChatResponseUpdate>();
        for (int i = 0; i < UpdatesCount; i++)
        {
            _updates.Add(new ChatResponseUpdate(ChatRole.Assistant, $"Token {i} "));
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Re-create console to avoid infinite memory growth during long benchmark runs
        _testConsole = new TestConsole();
        _testConsole.Profile.Capabilities.Interactive = true;
        _testConsole.Profile.Width = 80;
        _console = new ChatConsole(new DummyChatLineEditor(), _testConsole);
    }

    [Benchmark]
    public async Task DisplayStreamingUpdates()
    {
        await _console.DisplayStreamingUpdatesAsync(ToAsyncEnumerable(_updates));
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private static async IAsyncEnumerable<ChatResponseUpdate> ToAsyncEnumerable(IEnumerable<ChatResponseUpdate> updates)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var update in updates)
        {
            yield return update;
        }
    }
}

public class DummyChatLineEditor : IChatLineEditor
{
    public Task<string?> ReadLine(CancellationToken cancellationToken) => Task.FromResult<string?>(null);
}
