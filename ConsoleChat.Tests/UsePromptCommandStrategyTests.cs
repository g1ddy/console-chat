using System.Linq;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using ConsoleChat.Tests.TestUtilities;

namespace ConsoleChat.Tests;

public class UsePromptCommandStrategyTests
{
    private const string Use = "/use";

    private static string SamplePromptName => "Sample";

    private static McpPromptCollection CreateCollection() =>
        PromptFactory.CreateCollectionWithPrompt(SamplePromptName);

    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("", "/u", "");

        Assert.NotNull(completions);
        Assert.Contains(Use, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Prompt_Names()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("/use ", "s", "");

        Assert.NotNull(completions);
        Assert.Contains(SamplePromptName, completions!);
    }

    [Fact]
    public void CanExecute_Returns_False_For_Invalid_Command()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        Assert.False(strategy.CanExecute("/use missing"));
    }

    [Fact]
    public void CanExecute_Returns_False_For_Incomplete_Command()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        Assert.False(strategy.CanExecute("/use"));
    }

    [Fact]
    public void CanExecute_Returns_True_For_Valid_Command()
    {
        var strategy = new UsePromptCommandStrategy(CreateCollection());
        Assert.True(strategy.CanExecute($"/use {SamplePromptName}"));
    }
}
