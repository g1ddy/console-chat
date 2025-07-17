using Microsoft.Extensions.DependencyInjection;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using System.Linq;

namespace ConsoleChat.Tests;

public class ChatCommandServiceCollectionExtensionsTests
{
    [Fact]
    public void AddChatCommandStrategies_Registers_All_Strategies()
    {
        var services = new ServiceCollection();

        services.AddChatCommandStrategies();

        var strategyType = typeof(IChatCommandStrategy);
        var expected = strategyType.Assembly.GetTypes()
            .Where(t => strategyType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .OrderBy(t => t.FullName)
            .ToArray();

        var actual = services
            .Where(d => d.ServiceType == strategyType)
            .Select(d => d.ImplementationType)
            .OrderBy(t => t!.FullName)
            .ToArray();

        Assert.Equal(expected, actual);
    }
}
