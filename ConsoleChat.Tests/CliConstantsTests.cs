using SemanticKernelChat.Console;

namespace ConsoleChat.Tests;

public class CliConstantsTests
{
    [Fact]
    public void TopLevelConstants_HaveExpectedValues()
    {
        Assert.Equal("[grey]Welcome to ConsoleChat! Use Shift+Enter for a new line. Type '/exit' on an empty line to quit.[/]", CliConstants.WelcomeMessage);
        Assert.Equal("[red]Exiting ConsoleChat...[/]", CliConstants.ExitMessage);
        Assert.Equal("[grey]You: [/]", CliConstants.UserPrompt);
        Assert.Equal("Thinking...", CliConstants.ThinkingMessage);
        Assert.Equal("An unexpected error occurred. Please try again.", CliConstants.GenericErrorMessage);
        Assert.Equal("Summarize the previous conversation in a concise form.", CliConstants.SummarizationPrompt);
    }

    [Fact]
    public void Commands_HaveExpectedValues()
    {
        Assert.Equal("/debug", CliConstants.Commands.Debug);
        Assert.Equal("/disable", CliConstants.Commands.Disable);
        Assert.Equal("/enable", CliConstants.Commands.Enable);
        Assert.Equal("/exit", CliConstants.Commands.Exit);
        Assert.Equal("/help", CliConstants.Commands.Help);
        Assert.Equal("/list", CliConstants.Commands.List);
        Assert.Equal("/toggle", CliConstants.Commands.Toggle);
        Assert.Equal("/use", CliConstants.Commands.Use);
        Assert.Equal("/summarize", CliConstants.Commands.Summarize);
        Assert.Equal("/suggest", CliConstants.Commands.Suggest);
    }

    [Fact]
    public void Commands_All_ContainsAllCommands()
    {
        var expectedCommands = new HashSet<string>
        {
            "/debug", "/disable", "/enable", "/exit", "/help",
            "/list", "/toggle", "/use", "/summarize", "/suggest"
        };

        Assert.Equal(expectedCommands.Count, CliConstants.Commands.All.Count);
        foreach (var command in expectedCommands)
        {
            Assert.Contains(command, CliConstants.Commands.All);
        }
    }

    [Fact]
    public void Options_HaveExpectedValues()
    {
        Assert.Equal("mcp", CliConstants.Options.Mcp);
        Assert.Equal("tools", CliConstants.Options.Tools);
        Assert.Equal("prompts", CliConstants.Options.Prompts);
    }

    [Fact]
    public void MultiSelection_HaveExpectedValues()
    {
        Assert.Equal("[grey](Press <space> to toggle, <enter> to accept)[/]", CliConstants.MultiSelection.Instructions);
        Assert.Equal("[green](enabled)[/]", CliConstants.MultiSelection.Enabled);
        Assert.Equal("[red](disabled)[/]", CliConstants.MultiSelection.Disabled);
        Assert.Equal("[bold]Selected[/]", CliConstants.MultiSelection.SelectedHeader);
        Assert.Equal("[yellow]*[/]", CliConstants.MultiSelection.SelectionMarker);
        Assert.Equal("[grey]No selections made.[/]", CliConstants.MultiSelection.NoSelections);
    }

    [Theory]
    [InlineData("MyTool", "[grey]:wrench: MyTool Parameters...[/]")]
    [InlineData("AnotherTool", "[grey]:wrench: AnotherTool Parameters...[/]")]
    public void Tool_ParametersFormat_ReturnsExpectedString(string toolName, string expected)
    {
        Assert.Equal(expected, CliConstants.Tool.ParametersFormat(toolName));
    }

    [Theory]
    [InlineData("MyTool", "[grey]:wrench: MyTool Result...[/]")]
    [InlineData("AnotherTool", "[grey]:wrench: AnotherTool Result...[/]")]
    public void Tool_ResultFormat_ReturnsExpectedString(string toolName, string expected)
    {
        Assert.Equal(expected, CliConstants.Tool.ResultFormat(toolName));
    }

    [Theory]
    [InlineData("Something happened", "[orange1]:warning: Something happened[/]")]
    public void Tool_WarningFormat_ReturnsExpectedString(string message, string expected)
    {
        Assert.Equal(expected, CliConstants.Tool.WarningFormat(message));
    }

    [Fact]
    public void Roles_HaveExpectedValues()
    {
        Assert.Equal(":bust_in_silhouette: User", CliConstants.Roles.UserHeader);
        Assert.Equal(":robot: Assistant", CliConstants.Roles.AssistantHeader);
        Assert.Equal(":wrench: Tool", CliConstants.Roles.ToolHeader);
    }
}
