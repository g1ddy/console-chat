using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using ConsoleChat.Tests.TestUtilities;

namespace ConsoleChat.Tests;

public class ListPromptsCommandStrategyTests
{
    private const string List = "/list";
    private const string Prompts = "prompts";

    private static McpPromptCollection CreateCollection() =>
        McpCollectionFactory.CreatePromptCollection();

    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new ListPromptsCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("", "/l", "");

        Assert.NotNull(completions);
        Assert.Contains(List, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Prompts_Option()
    {
        var strategy = new ListPromptsCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("/list ", "p", "");

        Assert.NotNull(completions);
        Assert.Contains(Prompts, completions!);
    }

    [Fact]
    public void CanExecute_Returns_True_For_Command()
    {
        var strategy = new ListPromptsCommandStrategy(CreateCollection());
        Assert.True(strategy.CanExecute("/list prompts"));
    }

    [Fact]
    public void CanExecute_Returns_False_For_Invalid_Command()
    {
        var strategy = new ListPromptsCommandStrategy(CreateCollection());
        Assert.False(strategy.CanExecute("/list"));
    }
}
