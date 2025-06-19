using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Console;

public class ChatController : IChatController
{
    private readonly IChatConsole _console;

    public ChatController(IChatConsole console)
    {
        _console = console;
    }

    public async Task SendAndDisplayAsync(
        IChatClient chatClient,
        IChatHistoryService history,
        IReadOnlyList<McpClientTool> tools)
    {
        ChatMessage[] responses = [];
        Exception? error = null;
        await _console.DisplayThinkingIndicator(async () =>
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
            _console.DisplayError(error);
            return;
        }

        history.Add(responses);
        _console.WriteChatMessages(responses);
    }

    public async Task SendAndDisplayStreamingAsync(
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
            messages = await _console.DisplayStreamingUpdatesAsync(updates);
        }
        catch (Exception ex)
        {
            error = ex;
        }

        if (error is not null)
        {
            _console.DisplayError(error);
            return;
        }

        history.Add([..messages]);

        finalCallback?.Invoke(messages);
    }
}
