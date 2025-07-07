using SemanticKernelChat.Console;
using SemanticKernelChat;
using ConsoleChat.Tests.TestUtilities;
using Xunit;
using NSubstitute;
using Spectre.Console.Testing;

namespace ConsoleChat.Tests;

public class DebugCommandStrategyTests
{
    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new DebugCommandStrategy();
        var completions = strategy.GetCompletions("", "/d", "");
        Assert.NotNull(completions);
        Assert.Contains(CliConstants.Commands.Debug, completions!);
    }

    [Fact]
    public void CanExecute_Returns_True_For_Command()
    {
        var strategy = new DebugCommandStrategy();
        Assert.True(strategy.CanExecute("/debug"));
    }

    [Fact]
    public async Task ExecuteAsync_Toggles_Debug_Mode()
    {
        var strategy = new DebugCommandStrategy();
        var console = new ChatConsole(new FakeLineEditor([]), new TestConsole());
        Assert.False(console.DebugEnabled);
        await strategy.ExecuteAsync("/debug", new ChatHistoryService(), Substitute.For<IChatController>(), console);
        Assert.True(console.DebugEnabled);
    }
}

internal sealed class FakeLineEditor : IChatLineEditor
{
    private readonly Queue<string?> _inputs;
    public FakeLineEditor(IEnumerable<string?> inputs) => _inputs = new Queue<string?>(inputs);
    public Task<string?> ReadLine(CancellationToken cancellationToken) => Task.FromResult(_inputs.Count > 0 ? _inputs.Dequeue() : null);
}
