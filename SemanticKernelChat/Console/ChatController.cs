using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Console;

internal static class ChatController
{
    public static async Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools)
    {
        ChatMessage[] responses = [];
        Exception? error = null;
        await ChatConsole.DisplayThinkingIndicator(async () =>
        {
            try
            {
                var response = await chatClient.GetResponseAsync(
                    history.Messages,
                    new() { Tools = [.. tools] });
                responses = [.. response.Messages];
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        if (error is not null)
        {
            ChatConsole.DisplayError(error);
            return;
        }

        ChatConsole.WriteChatMessages(history, responses);
    }

    public static async Task SendAndDisplayStreamingAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools,
        Action<IReadOnlyList<ChatMessage>>? finalCallback = null)
    {
        var updates = chatClient.GetStreamingResponseAsync(
            history.Messages,
            new() { Tools = [.. tools] });
        Exception? error = null;
        IReadOnlyList<ChatMessage> messages = [];

        try
        {
            messages = await ChatConsole.DisplayStreamingUpdatesAsync(updates);
        }
        catch (Exception ex)
        {
            error = ex;
        }

        if (error is not null)
        {
            ChatConsole.DisplayError(error);
            return;
        }

        history.Add([..messages]);

        finalCallback?.Invoke(messages);
    }
}
