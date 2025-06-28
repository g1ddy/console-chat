using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using ConsoleChat.Tests.TestUtilities;

namespace ConsoleChat.Tests;

public class UsePromptCommandStrategyTests
{
    private const string Use = "/use";

    private static McpPromptCollection CreateCollection() =>
        McpCollectionFactory.CreatePromptCollection();

    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("", "/u", "");

        Assert.NotNull(completions);
        Assert.Contains(Use, completions!);
    }

    [Fact]
    public void CanExecute_Returns_False_For_Invalid_Command()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        Assert.False(strategy.CanExecute("/use missing"));
    }
}
