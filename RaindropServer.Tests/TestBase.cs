using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaindropServer.Common;
using RaindropServer.Tests.Common;

namespace RaindropServer.Tests;

public abstract class TestBase
{
    protected IServiceProvider Provider { get; }

    protected TestBase(params Action<IServiceCollection>[] registrations)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var token = config["Raindrop:ApiToken"];
        var baseUrl = config["Raindrop:BaseUrl"];
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Set Raindrop:ApiToken and Raindrop:BaseUrl to run tests");

        var services = new ServiceCollection();

        // Register client services (this registers HttpContextTokenProvider by default)
        services.AddRaindropApiClient(config);

        // Override with StaticTokenProvider for tests
        services.AddSingleton<ITokenProvider>(new StaticTokenProvider(token));

        foreach (var reg in registrations) reg(services);
        Provider = services.BuildServiceProvider();
    }
}
