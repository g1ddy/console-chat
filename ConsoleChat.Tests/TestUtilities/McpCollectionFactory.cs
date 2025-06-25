using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests.TestUtilities;

internal static class McpCollectionFactory
{
    public static McpToolCollection CreateToolCollection()
        => (McpToolCollection)Activator.CreateInstance(typeof(McpToolCollection), nonPublic: true)!;
}
