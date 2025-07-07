using Microsoft.Extensions.AI;
using System;
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
    private readonly IChatHistoryReducer _reducer;
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

        int targetCount = _summaryKeepLast + 1;
        int thresholdCount = _summaryThreshold - targetCount;
        _reducer = new ChatHistorySummarizationReducer(chatClient, targetCount, thresholdCount)
        {
            SummarizationInstructions = SummarizationPrompt,
            UseSingleSummary = true,
            FailOnError = false
        };
    }

    public async Task SummarizeAsync(IChatHistoryService history)
    {
        IEnumerable<ChatMessage>? reduced = await _reducer.ReduceAsync(history.Messages, CancellationToken.None);
        if (reduced is null)
        {
            return;
        }

        var newHistory = reduced.ToList();
        int desiredCount = _summaryKeepLast + 1;
        if (newHistory.Count > desiredCount)
        {
            bool hasSystem = newHistory[0].Role == ChatRole.System;
            int prefixCount = hasSystem ? 2 : 1; // keep system and summary

            var trimmed = new List<ChatMessage>(desiredCount);
            trimmed.AddRange(newHistory.Take(prefixCount));
            trimmed.AddRange(newHistory.Skip(newHistory.Count - _summaryKeepLast));
            newHistory = trimmed;
        }

        history.Replace(newHistory);
    }

    public async Task SendAndDisplayAsync(IChatHistoryService history)
    {
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
