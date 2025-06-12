namespace ConsoleChat.Tests;

public class McpFunctionTests
{
    [Fact]
    public void Add_Returns_Sum()
    {
        int result = UtilityTools.Add(2, 3);
        Assert.Equal(5, result);
    }

    [Fact]
    public void ToUpper_Converts_To_Uppercase()
    {
        string result = UtilityTools.ToUpper("hello");
        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void CurrentTime_Returns_Parsable_Iso8601()
    {
        string isoTime = UtilityTools.CurrentTime();
        bool parsed = DateTime.TryParse(isoTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out _);
        Assert.True(parsed);
    }

    [Fact]
    public void Echo_Returns_Prefixed_Message()
    {
        string result = EchoTool.Echo("test");
        Assert.Equal("Hello from C#: test", result);
    }

    [Fact]
    public void ReverseEcho_Returns_Reversed_Message()
    {
        string result = EchoTool.ReverseEcho("abc");
        Assert.Equal("cba", result);
    }
}
