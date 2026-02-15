using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System.Net.Http.Headers;
using RaindropServer.Collections;
using RaindropServer.Raindrops;
using RaindropServer.Highlights;
using RaindropServer.Filters;
using RaindropServer.Tags;
using RaindropServer.User;
using RaindropServer.Common;

namespace RaindropServer;

/// <summary>
/// Extension methods for registering Raindrop API services.
/// </summary>
public static class RaindropServiceCollectionExtensions
{
    /// <summary>
    /// Registers Raindrop API clients using configuration from the
    /// "Raindrop" section of <see cref="IConfiguration"/>.
    /// </summary>
    public static IServiceCollection AddRaindropApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RaindropOptions>(configuration.GetSection("Raindrop"));

        // Register core authentication services for clients
        services.AddHttpContextAccessor();
        services.AddTransient<AuthHeaderHandler>();

        // Register default token provider if not already registered (allows tests to override)
        services.AddSingleton<ITokenProvider, HttpContextTokenProvider>();

        var settings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            })
        };

        void Configure(IServiceProvider sp, HttpClient client)
        {
            var options = sp.GetRequiredService<IOptions<RaindropOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("Raindrop BaseUrl is required");
            }

            client.BaseAddress = new Uri(options.BaseUrl);
        }

        services.AddRefitClient<ICollectionsApi>(settings)
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IRaindropsApi>(settings)
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IHighlightsApi>(settings)
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IFiltersApi>(settings)
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ITagsApi>(settings)
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IUserApi>(settings)
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
