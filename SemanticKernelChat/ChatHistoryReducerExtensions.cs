using Microsoft.Extensions.AI;

namespace SemanticKernelChat;

internal static class ChatHistoryReducerExtensions
{
    public static IEnumerable<ChatMessage> Extract(
        this IReadOnlyList<ChatMessage> chatHistory,
        int startIndex,
        int? finalIndex = null,
        ChatMessage? systemMessage = null,
        Func<ChatMessage, bool>? filter = null)
    {
        int maxIndex = chatHistory.Count - 1;
        if (startIndex > maxIndex)
        {
            yield break;
        }

        if (systemMessage is not null)
        {
            yield return systemMessage;
        }

        finalIndex ??= maxIndex;
        finalIndex = Math.Min(finalIndex.Value, maxIndex);

        for (int index = startIndex; index <= finalIndex; ++index)
        {
            if (filter?.Invoke(chatHistory[index]) ?? false)
            {
                continue;
            }

            yield return chatHistory[index];
        }
    }

    public static int LocateSummarizationBoundary(this IReadOnlyList<ChatMessage> chatHistory, string summaryKey)
    {
        for (int index = 0; index < chatHistory.Count; ++index)
        {
            ChatMessage message = chatHistory[index];

            if (!(message.AdditionalProperties?.ContainsKey(summaryKey) ?? false))
            {
                return index;
            }
        }

        return chatHistory.Count;
    }

    public static int LocateSafeReductionIndex(
        this IReadOnlyList<ChatMessage> chatHistory,
        int targetCount,
        int? thresholdCount = null,
        int offsetCount = 0,
        bool hasSystemMessage = false)
    {
        targetCount -= hasSystemMessage ? 1 : 0;

        int thresholdIndex = chatHistory.Count - (thresholdCount ?? 0) - targetCount;
        if (thresholdIndex <= offsetCount)
        {
            return -1;
        }

        int messageIndex = chatHistory.Count - targetCount;

        while (messageIndex >= 0)
        {
            if (!chatHistory[messageIndex].Contents.Any(i => i is FunctionCallContent || i is FunctionResultContent))
            {
                break;
            }
            --messageIndex;
        }

        int targetIndex = messageIndex;

        while (messageIndex >= thresholdIndex)
        {
            if (chatHistory[messageIndex].Role == ChatRole.User)
            {
                return messageIndex;
            }
            --messageIndex;
        }

        return targetIndex;
    }
}
