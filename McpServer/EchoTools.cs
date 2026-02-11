using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;

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
            int sourceIndex = 0;
            int writeIndex = span.Length;

            while (sourceIndex < state.Length)
            {
                int count = StringInfo.GetNextTextElementLength(state, sourceIndex);
                writeIndex -= count;
                state.AsSpan(sourceIndex, count).CopyTo(span.Slice(writeIndex));
                sourceIndex += count;
            }
        });
    }
}
