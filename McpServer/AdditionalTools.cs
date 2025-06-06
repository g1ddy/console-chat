using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public static class UtilityTools
{
    [McpServerTool, Description("Returns the current server time in ISO 8601 format.")]
    public static string CurrentTime() => DateTime.UtcNow.ToString("o");

    [McpServerTool, Description("Converts the input to uppercase.")]
    public static string ToUpper(string input) => input.ToUpperInvariant();

    [McpServerTool, Description("Adds two numbers together.")]
    public static int Add(int a, int b) => a + b;
}
