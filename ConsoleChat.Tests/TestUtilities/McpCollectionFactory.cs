using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class McpCollectionFactory
{
    public static McpToolCollection CreateToolCollection()
        => new McpToolCollection();

    public static McpPromptCollection CreatePromptCollection()
        => new McpPromptCollection();
}
