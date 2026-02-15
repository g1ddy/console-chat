using System.Linq;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace SemanticKernelChat;

/// <summary>
/// Reduce the chat history by summarizing messages past the target message count.
/// </summary>
/// <remarks>
/// Adapted from Semantic Kernel's ChatHistorySummarizationReducer for ChatMessage.
/// </remarks>
public class ChatHistorySummarizationReducer : IChatHistoryReducer
{
    public const string SummaryMetadataKey = "__summary__";

    public const string DefaultSummarizationPrompt =
        """
        Provide a concise and complete summarization of the entire dialog that does not exceed 5 sentences

        This summary must always:
        - Consider both user and assistant interactions
        - Maintain continuity for the purpose of further dialog
        - Include details from any existing summary
        - Focus on the most significant aspects of the dialog

        This summary must never:
        - Critique, correct, interpret, presume, or assume
        - Identify faults, mistakes, misunderstanding, or correctness
        - Analyze what has not occurred
        - Exclude details from any existing summary
        """;

    public string SummarizationInstructions { get; init; } = DefaultSummarizationPrompt;

    public bool FailOnError { get; init; } = true;

    public bool UseSingleSummary { get; init; } = true;

    private readonly IChatClient _chatClient;
    private readonly int _thresholdCount;
    private readonly int _targetCount;
    private readonly ILogger? _logger;

    public ChatHistorySummarizationReducer(IChatClient chatClient, int targetCount, int? thresholdCount = null, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        if (targetCount <= 0) throw new ArgumentOutOfRangeException(nameof(targetCount));
        if (thresholdCount.HasValue && thresholdCount <= 0) throw new ArgumentOutOfRangeException(nameof(thresholdCount));

        _chatClient = chatClient;
        _targetCount = targetCount;
        _thresholdCount = thresholdCount ?? 0;
        _logger = logger;
    }

    public async Task<IEnumerable<ChatMessage>?> ReduceAsync(
        IReadOnlyList<ChatMessage> chatHistory,
        CancellationToken cancellationToken = default)
    {
        var systemMessage = chatHistory.FirstOrDefault(l => l.Role == ChatRole.System);

        int insertionPoint = chatHistory.LocateSummarizationBoundary(SummaryMetadataKey);

        int truncationIndex = chatHistory.LocateSafeReductionIndex(
            _targetCount,
            _thresholdCount,
            insertionPoint,
            hasSystemMessage: systemMessage is not null);

        IEnumerable<ChatMessage>? truncatedHistory = null;

        if (truncationIndex >= 0)
        {
            IEnumerable<ChatMessage> summarizedHistory =
                chatHistory.Extract(
                    UseSingleSummary ? 0 : insertionPoint,
                    truncationIndex,
                    filter: m => m.Contents.Any(i => i is FunctionCallContent || i is FunctionResultContent));

            try
            {
                IEnumerable<ChatMessage> summarizationRequest = summarizedHistory.Append(new ChatMessage(ChatRole.System, SummarizationInstructions));
                ChatResponse response = await _chatClient.GetResponseAsync(summarizationRequest, options: null, cancellationToken).ConfigureAwait(false);
                ChatMessage summaryMessage = response.Messages.Last();
                summaryMessage.AdditionalProperties ??= new();
                summaryMessage.AdditionalProperties[SummaryMetadataKey] = true;

                truncatedHistory = AssemblySummarizedHistory(summaryMessage, systemMessage);
            }
            catch (Exception ex)
            {
                if (FailOnError)
                {
                    throw;
                }

                _logger?.LogError(ex, "Summarization failed");
            }
        }

        return truncatedHistory;

        IEnumerable<ChatMessage> AssemblySummarizedHistory(ChatMessage? summaryMessage, ChatMessage? systemMsg)
        {
            if (systemMsg is not null)
            {
                yield return systemMsg;
            }

            if (insertionPoint > 0 && !UseSingleSummary)
            {
                for (int index = 0; index <= insertionPoint - 1; ++index)
                {
                    yield return chatHistory[index];
                }
            }

            if (summaryMessage is not null)
            {
                yield return summaryMessage;
            }

            for (int index = truncationIndex; index < chatHistory.Count; ++index)
            {
                yield return chatHistory[index];
            }
        }
    }

    public override bool Equals(object? obj)
    {
        ChatHistorySummarizationReducer? other = obj as ChatHistorySummarizationReducer;
        return other != null &&
               _thresholdCount == other._thresholdCount &&
               _targetCount == other._targetCount &&
               UseSingleSummary == other.UseSingleSummary &&
               string.Equals(SummarizationInstructions, other.SummarizationInstructions, StringComparison.Ordinal);
    }

    public override int GetHashCode() => HashCode.Combine(nameof(ChatHistorySummarizationReducer), _thresholdCount, _targetCount, SummarizationInstructions, UseSingleSummary);
}
