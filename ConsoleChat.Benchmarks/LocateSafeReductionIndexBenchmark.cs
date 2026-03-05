using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.AI;
using SemanticKernelChat;

namespace ConsoleChat.Benchmarks;

[MemoryDiagnoser]
public class LocateSafeReductionIndexBenchmark
{
    private List<ChatMessage> _chatHistory = null!;

    [GlobalSetup]
    public void Setup()
    {
        _chatHistory = new List<ChatMessage>();
        for (int i = 0; i < 1000; i++)
        {
            // Create a long sequence of function-related messages at the end
            if (i > 500)
            {
                if (i % 2 == 0)
                {
                    _chatHistory.Add(new ChatMessage(ChatRole.Assistant, [new FunctionCallContent("test", "test")]));
                }
                else
                {
                    _chatHistory.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent("test", "result")]));
                }
            }
            else
            {
                _chatHistory.Add(new ChatMessage(ChatRole.User, "Message " + i));
            }
        }
    }

    [Benchmark]
    public int LocateSafeReductionIndex()
    {
        // Force it to look back through many messages
        return _chatHistory.LocateSafeReductionIndex(10, thresholdCount: 900);
    }
}
