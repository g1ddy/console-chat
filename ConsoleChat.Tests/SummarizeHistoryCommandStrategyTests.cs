using System.Threading.Tasks;
using NSubstitute;
using SemanticKernelChat.Console;
using SemanticKernelChat;

namespace ConsoleChat.Tests;

public class SummarizeHistoryCommandStrategyTests
{
    private const string Summarize = "/summarize";

    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new SummarizeHistoryCommandStrategy();
        var completions = strategy.GetCompletions("", "/s", "");

        Assert.NotNull(completions);
        Assert.Contains(Summarize, completions!);
    }

    [Fact]
    public void CanExecute_Returns_True_For_Command()
    {
        var strategy = new SummarizeHistoryCommandStrategy();
        Assert.True(strategy.CanExecute("/summarize"));
    }

    [Fact]
    public async Task ExecuteAsync_Calls_Controller()
    {
        var strategy = new SummarizeHistoryCommandStrategy();
        var history = new ChatHistoryService();
        var controller = Substitute.For<IChatController>();
        var console = Substitute.For<IChatConsole>();

        var result = await strategy.ExecuteAsync("/summarize", history, controller, console);

        await controller.Received(1).SummarizeAsync(history);
        Assert.True(result);
    }
}
