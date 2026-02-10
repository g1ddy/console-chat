using ModelContextProtocol.Server;

using System.ComponentModel;

namespace McpServer;

[McpServerToolType]
public static class EchoTools
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Hello from C#: {message}";

    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return string.Create(message.Length, message, (span, state) =>
        {
            state.AsSpan().CopyTo(span);
            span.Reverse();
        });
    }
}
