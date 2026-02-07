using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddRaindropApiClient(config);
        foreach (var reg in registrations) reg(services);
        Provider = services.BuildServiceProvider();
    }
}
