using Microsoft.Extensions.AI;
using System.Linq;
using System.Collections.Generic;
using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Console;

public class ChatController : IChatController
{
    private readonly IChatConsole _console;
    private readonly IChatClient _chatClient;
    private readonly McpToolCollection _toolCollection;
    private readonly IReadOnlyList<AIFunction> _functions;

    private ChatOptions CreateChatOptions() => new() { Tools = [.._toolCollection.Tools, .._functions] };
    private const int DefaultSummaryThreshold = 20;
    private const int DefaultSummaryKeepLast = 5;

    private readonly int _summaryThreshold;
    private readonly int _summaryKeepLast;
    private const string SummarizationPrompt = "Summarize the previous conversation in a concise form.";

    public McpToolCollection ToolCollection => _toolCollection;

    public ChatController(
        IChatConsole console,
        IChatClient chatClient,
        McpToolCollection toolCollection, IReadOnlyList<AIFunction> functions,
        int summaryThreshold = DefaultSummaryThreshold,
        int summaryKeepLast = DefaultSummaryKeepLast)
    {
        if (summaryThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(summaryThreshold), "Must be positive.");
        }

        if (summaryKeepLast <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(summaryKeepLast), "Must be positive.");
        }

        if (summaryKeepLast >= summaryThreshold)
        {
            throw new ArgumentException($"{nameof(summaryKeepLast)} must be less than {nameof(summaryThreshold)}.", nameof(summaryKeepLast));
        }

        _console = console;
        _chatClient = chatClient;
        _toolCollection = toolCollection;
        _functions = functions;
        _summaryThreshold = summaryThreshold;
        _summaryKeepLast = summaryKeepLast;
    }

    private async Task MaybeSummarizeAsync(IChatHistoryService history)
    {
        if (history.Messages.Count <= _summaryThreshold)
        {
            return;
        }

        int summarizeCount = history.Messages.Count - _summaryKeepLast;
        if (summarizeCount <= 0)
        {
            return;
        }

        var toSummarize = history.Messages.Take(summarizeCount).ToList();
        toSummarize.Add(new ChatMessage(ChatRole.User, SummarizationPrompt));

        var response = await _chatClient.GetResponseAsync(toSummarize);
        var summaryMessage = response.Messages.Last();

        var newHistory = new List<ChatMessage> { summaryMessage };
        newHistory.AddRange(history.Messages.Skip(summarizeCount));

        history.Replace(newHistory);
    }

    public async Task SendAndDisplayAsync(IChatHistoryService history)
    {
        await MaybeSummarizeAsync(history);

        ChatMessage[] responses = [];
        Exception? error = null;
        await _console.DisplayThinkingIndicator(async () =>
        {
            try
            {
                var response = await _chatClient.GetResponseAsync(history.Messages, CreateChatOptions());
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
        await MaybeSummarizeAsync(history);

        var updates = _chatClient.GetStreamingResponseAsync(history.Messages, CreateChatOptions());
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
