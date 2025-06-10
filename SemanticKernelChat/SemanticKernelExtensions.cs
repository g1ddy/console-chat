using Amazon.BedrockRuntime;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
namespace SemanticKernelChat;

/// <summary>
/// Extension methods for registering the Semantic Kernel chat client
/// </summary>
public static class SemanticKernelExtensions
{
    /// <summary>
    /// Registers the Semantic Kernel chat client and related services.
    /// </summary>
    public static IServiceCollection AddSemanticKernelChatClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Create the chat client using configuration
        IChatClient chatClient = CreateChatClient(configuration);

        // Register the chat client with function invocation support
        _ = services.AddChatClient(_ =>
            chatClient
            .AsBuilder()
            .UseFunctionInvocation()
            .Build());

        return services;
    }

    /// <summary>
    /// Creates an IChatClient based on the configured provider (AwsBedrock or OpenAI).
    /// </summary>
    private static IChatClient CreateChatClient(IConfiguration configuration)
    {
        // Get provider and model configuration
        var provider = configuration["Provider"] ?? throw new ArgumentNullException("PROVIDER", "Semantic Kernel provider is not configured.");
        var providerSection = configuration.GetSection(provider);

        // Get the model ID from the configuration
        var modelId = providerSection["ModelId"] ?? throw new ArgumentNullException("MODEL", "Semantic Kernel model is not configured.");

        switch (provider)
        {
            case "AwsBedrock":
                // Create Bedrock runtime client and wrap as IChatClient
                var bedrockRuntime = Helpers.BedrockHelper.GetBedrockRuntimeAsync(providerSection).GetAwaiter().GetResult();
                return bedrockRuntime.AsIChatClient(
                    modelId
                );
            case "OpenAI":
                // Create OpenAI chat client
                return new OpenAI.Chat.ChatClient(
                    modelId ?? "gpt-3.5-turbo",
                    configuration["OPENAI_API_KEY"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ).AsIChatClient();
            default:
                throw new NotSupportedException($"Provider '{provider}' is not supported.");
        }
    }
}
