using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using RaindropServer.Collections;
using RaindropServer.Common;
using RaindropServer.Tests.Common;
using Xunit;
using System.Net.Http;
using System.Reflection;

namespace RaindropServer.Tests;

public class HttpClientConfigurationTests
{
    [Fact]
    public void DefaultTimeout_Is30Seconds()
    {
        // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder();

        var myConfig = new Dictionary<string, string>
        {
            {"Raindrop:BaseUrl", "https://api.raindrop.io"},
            {"Raindrop:ApiToken", "dummy-token"}
        };

        configBuilder.AddInMemoryCollection(myConfig!);
        var configuration = configBuilder.Build();

        services.AddRaindropApiClient(configuration);
        services.AddSingleton<ITokenProvider>(new StaticTokenProvider("dummy-token"));

        var provider = services.BuildServiceProvider();

        // Act
        var api = provider.GetRequiredService<ICollectionsApi>();
        var httpClient = GetHttpClientFromRefitClient(api);

        // Assert
        Assert.NotNull(httpClient);
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }

    [Fact]
    public void CustomTimeout_IsApplied()
    {
        // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder();

        var myConfig = new Dictionary<string, string>
        {
            {"Raindrop:BaseUrl", "https://api.raindrop.io"},
            {"Raindrop:ApiToken", "test-token"},
            {"Raindrop:TimeoutSeconds", "45"}
        };

        configBuilder.AddInMemoryCollection(myConfig!);
        var configuration = configBuilder.Build();

        services.AddRaindropApiClient(configuration);
        services.AddSingleton<ITokenProvider>(new StaticTokenProvider("test-token"));

        var provider = services.BuildServiceProvider();

        // Act
        var api = provider.GetRequiredService<ICollectionsApi>();
        var httpClient = GetHttpClientFromRefitClient(api);

        // Assert
        Assert.NotNull(httpClient);
        Assert.Equal(TimeSpan.FromSeconds(45), httpClient.Timeout);
    }

    [Fact]
    public void InvalidTimeout_ThrowsException()
    {
         // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder();

        var myConfig = new Dictionary<string, string>
        {
            {"Raindrop:BaseUrl", "https://api.raindrop.io"},
            {"Raindrop:ApiToken", "test-token"},
            {"Raindrop:TimeoutSeconds", "-1"}
        };

        configBuilder.AddInMemoryCollection(myConfig!);
        var configuration = configBuilder.Build();

        services.AddRaindropApiClient(configuration);
        services.AddSingleton<ITokenProvider>(new StaticTokenProvider("test-token"));

        var provider = services.BuildServiceProvider();

        // Act & Assert
        // Resolving the client triggers creation and configuration
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var api = provider.GetRequiredService<ICollectionsApi>();
            // Just resolving might not trigger HttpClient creation if lazy?
            // But usually it does. If not, the reflection helper will access it.
            // If the creation fails inside constructor, it throws.
            // If creation is lazy, accessing it will throw.
            // But GetHttpClientFromRefitClient might need to be called to trigger.
            // However, InvalidOperationException from Configure should happen during HttpClient creation.
            // Refit proxies usually create HttpClient in constructor or use a factory.

            // If GetRequiredService succeeds, try accessing client.
            if (api != null)
            {
                 // Try to force creation if lazy
                 var client = GetHttpClientFromRefitClient(api);
            }
        });

        Assert.Contains("Raindrop TimeoutSeconds must be greater than 0", exception.Message);
    }

    private HttpClient? GetHttpClientFromRefitClient(object client)
    {
        // Refit generated proxy usually has a field for HttpClient.
        // It might be named "Client", "client", "httpClient", or similar.
        // Or it might be inside a RequestBuilder.

        // Let's inspect fields.
        var type = client.GetType();

        // Look for HttpClient field
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(HttpClient))
            {
                return (HttpClient?)field.GetValue(client);
            }
        }

        // Look for properties
        var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in properties)
        {
             if (prop.PropertyType == typeof(HttpClient))
             {
                 return (HttpClient?)prop.GetValue(client);
             }
        }

        return null;
    }
}
