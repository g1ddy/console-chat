using Microsoft.Extensions.AI;
using SemanticKernelChat;
using SemanticKernelChat.Console;

using Spectre.Console;
using Spectre.Console.Rendering;

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

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_Null_Role()
    {
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(null);
        Assert.Contains("|", header);
        Assert.Equal(Justify.Left, j);
        Assert.Equal(Style.Plain, style);
    }

    [Fact]
    public void GetHeaderStyle_Returns_Values_For_Unknown_Role()
    {
        var customRole = new ChatRole("custom");
        var (header, j, style) = ChatConsoleHelpers.GetHeaderStyle(customRole);
        Assert.StartsWith("custom", header);
        Assert.Equal(Justify.Left, j);
        Assert.Equal(Style.Plain, style);
    }

    [Fact]
    public void CollectFunctionCallNames_With_Contents_Populates_Dictionary()
    {
        var contents = new List<AIContent>
        {
            new TextContent("Hello"),
            new FunctionCallContent("call1", "func1"),
            new FunctionCallContent("call2", "func2"),
            new FunctionCallContent("", "func3"), // Should be ignored
            new FunctionCallContent("call4", "")  // Should be ignored
        };
        var callNames = new Dictionary<string, string>();

        ChatConsoleHelpers.CollectFunctionCallNames(contents, callNames);

        Assert.Equal(2, callNames.Count);
        Assert.Equal("func1", callNames["call1"]);
        Assert.Equal("func2", callNames["call2"]);
    }

    [Fact]
    public void CollectFunctionCallNames_With_Messages_Returns_Dictionary()
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.Assistant, [new FunctionCallContent("call1", "func1")]),
            new ChatMessage(ChatRole.User, "Hello"),
            new ChatMessage(ChatRole.Assistant, [new FunctionCallContent("call2", "func2")])
        };

        var result = ChatConsoleHelpers.CollectFunctionCallNames(messages);

        Assert.Equal(2, result.Count);
        Assert.Equal("func1", result["call1"]);
        Assert.Equal("func2", result["call2"]);
    }

    [Fact]
    public void GetToolName_Returns_Name_From_Dictionary_If_Found()
    {
        var callNames = new Dictionary<string, string> { { "id1", "MyTool" } };
        var result = ChatConsoleHelpers.GetToolName(callNames, "id1", "Author", ChatRole.Assistant);
        Assert.Equal("MyTool", result);
    }

    [Fact]
    public void GetToolName_Returns_AuthorName_If_Not_Found_In_Dictionary()
    {
        var callNames = new Dictionary<string, string>();
        var result = ChatConsoleHelpers.GetToolName(callNames, "id1", "Author", ChatRole.Assistant);
        Assert.Equal("Author", result);
    }

    [Fact]
    public void GetToolName_Returns_TitleCase_Role_If_Author_And_Id_Missing()
    {
        var callNames = new Dictionary<string, string>();
        var result = ChatConsoleHelpers.GetToolName(callNames, null, null, ChatRole.Assistant);
        Assert.Equal("Assistant", result);
    }

    [Fact]
    public void GetToolName_Returns_Empty_String_If_Everything_Is_Missing()
    {
        var callNames = new Dictionary<string, string>();
        var result = ChatConsoleHelpers.GetToolName(callNames, null, null, null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void IsValidJson_Returns_Expected_Results()
    {
        Assert.True(ChatConsoleHelpers.IsValidJson("{\"key\": \"value\"}"));
        Assert.True(ChatConsoleHelpers.IsValidJson("[1, 2, 3]"));
        Assert.False(ChatConsoleHelpers.IsValidJson("not json"));
        Assert.False(ChatConsoleHelpers.IsValidJson(""));
        Assert.False(ChatConsoleHelpers.IsValidJson(null));
    }

    [Fact]
    public void SerializeArguments_Returns_Expected_Results()
    {
        Assert.Null(ChatConsoleHelpers.SerializeArguments(null));
        Assert.Null(ChatConsoleHelpers.SerializeArguments(new Dictionary<string, object?>()));

        var args = new Dictionary<string, object?> { { "key", "value" } };
        var result = ChatConsoleHelpers.SerializeArguments(args);
        Assert.NotNull(result);
        Assert.Contains("\"key\":\"value\"", result);
    }

    [Fact]
    public void FormatPanelContent_Returns_Expected_Number_Of_Items()
    {
        // Debug disabled
        var result1 = ChatConsoleHelpers.FormatPanelContent("markup", "{\"a\":1}", false);
        Assert.Single(result1);

        // Debug enabled, valid JSON
        var result2 = ChatConsoleHelpers.FormatPanelContent("markup", "{\"a\":1}", true);
        Assert.Equal(2, result2.Count());

        // Debug enabled, invalid JSON
        var result3 = ChatConsoleHelpers.FormatPanelContent("markup", "not json", true);
        Assert.Equal(2, result3.Count());

        // Debug enabled, null JSON
        var result4 = ChatConsoleHelpers.FormatPanelContent("markup", null, true);
        Assert.Single(result4);
    }

    [Fact]
    public void CreatePanel_Returns_Configured_Panel()
    {
        var content = new Markup("hello");
        var style = new Style(Color.Red);
        var header = new PanelHeader("header");

        var panel = ChatConsoleHelpers.CreatePanel(content, style, header);

        Assert.NotNull(panel);
        Assert.Equal(style, panel.BorderStyle);
        Assert.Equal(header.Text, panel.Header!.Text);
    }
}
