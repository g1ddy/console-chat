using Microsoft.Extensions.AI;
using SemanticKernelChat;
using SemanticKernelChat.Console;

using Spectre.Console;

namespace ConsoleChat.Tests;

public class ChatConsoleHelpersTests
{
    [Fact]
    public void GetHeaderStyle_Returns_Values_For_User()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(ChatRole.User);
        Assert.StartsWith(":bust_in_silhouette: User", header);
        Assert.Equal(Justify.Left, j);
        Assert.Equal(Color.RoyalBlue1, style.Foreground);
    }

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_Assistant()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(ChatRole.Assistant);
        Assert.StartsWith(":robot: Assistant", header);
        Assert.Equal(Justify.Right, j);
        Assert.Equal(Color.DarkSeaGreen2, style.Foreground);
    }

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_Tool()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(ChatRole.Tool);
        Assert.StartsWith(":wrench: Tool", header);
        Assert.Equal(Justify.Center, j);
        Assert.Equal(Color.Grey37, style.Foreground);
    }
}
