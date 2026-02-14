using SemanticKernelChat.Console;

namespace ConsoleChat.Tests;

public class CommandTokenizerTests
{
    [Theory]
    [InlineData("arg1 arg2", new[] { "arg1", "arg2" })]
    [InlineData("  arg1  arg2  ", new[] { "arg1", "arg2" })]
    [InlineData("arg1   arg2", new[] { "arg1", "arg2" })]
    [InlineData("", new string[] { })]
    [InlineData("   ", new string[] { })]
    [InlineData(null, new string[] { })]
    public void SplitArguments_Returns_Expected_Tokens(string? input, string[] expected)
    {
        var result = CommandTokenizer.SplitArguments(input!);
        Assert.Equal(expected, result);
    }
}
