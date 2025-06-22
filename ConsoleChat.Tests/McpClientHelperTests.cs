using System.IO;
using SemanticKernelChat.Helpers;

namespace ConsoleChat.Tests;

public class McpClientHelperTests
{
    [Theory]
    [InlineData("dotnet")]
    [InlineData("nvx")]
    public void ResolveCommandPath_Returns_System_Command_Unchanged(string command)
    {
        string result = McpClientHelper.ResolveCommandPath(command);
        Assert.Equal(command, result);
    }

    [Fact]
    public void ResolveCommandPath_Converts_Relative_Path()
    {
        string rel = $".{Path.DirectorySeparatorChar}tools{Path.DirectorySeparatorChar}server";
        string expected = Path.GetFullPath(rel, AppContext.BaseDirectory);
        string result = McpClientHelper.ResolveCommandPath(rel);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolveCommandPath_Returns_Absolute_Path_Unchanged()
    {
        string abs = Path.GetFullPath("server.exe");
        string result = McpClientHelper.ResolveCommandPath(abs);
        Assert.Equal(abs, result);
    }

    [Fact]
    public void ResolveCommandPath_Handles_Alternate_Separator()
    {
        char alt = Path.AltDirectorySeparatorChar;
        string rel = $"folder{alt}server";
        string expected = Path.GetFullPath(rel, AppContext.BaseDirectory);
        string result = McpClientHelper.ResolveCommandPath(rel);
        Assert.Equal(expected, result);
    }
}
