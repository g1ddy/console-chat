using Microsoft.Extensions.AI;
using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Console;

public class ChatController : IChatController
{
    private readonly IChatConsole _console;
    private readonly IChatClient _chatClient;
    private readonly McpToolCollection _toolCollection;

    public McpToolCollection ToolCollection => _toolCollection;

    public ChatController(IChatConsole console, IChatClient chatClient, McpToolCollection toolCollection)
    {
        _console = console;
        _chatClient = chatClient;
        _toolCollection = toolCollection;
    }

    public async Task SendAndDisplayAsync(IChatHistoryService history)
    {
        ChatMessage[] responses = [];
        Exception? error = null;
        await _console.DisplayThinkingIndicator(async () =>
        {
            try
            {
                var response = await _chatClient.GetResponseAsync(
                    history.Messages,
                    new() { Tools = [.. _toolCollection.Tools] });
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
        IChatHistoryService history,
        Action<IReadOnlyList<ChatMessage>>? finalCallback = null)
    {
        var updates = _chatClient.GetStreamingResponseAsync(
            history.Messages,
            new() { Tools = [.. _toolCollection.Tools] });
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
