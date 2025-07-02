using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System.Net.Http.Headers;
using RaindropTools.Collections;
using RaindropTools.Raindrops;
using RaindropTools.Highlights;
using RaindropTools.Tags;
using RaindropTools.User;

namespace RaindropTools;

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

        var settings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            })
        };

        void Configure(HttpClient client, IServiceProvider sp)
        {
            var options = sp.GetRequiredService<IOptions<RaindropOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("Raindrop BaseUrl is required");
            }

            if (string.IsNullOrWhiteSpace(options.ApiToken))
            {
                throw new InvalidOperationException("Raindrop ApiToken is required");
            }

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
        }

        services.AddRefitClient<ICollectionsApi>(settings).ConfigureHttpClient((sp, client) => Configure(client, sp));
        services.AddRefitClient<IRaindropsApi>(settings).ConfigureHttpClient((sp, client) => Configure(client, sp));
        services.AddRefitClient<IHighlightsApi>(settings).ConfigureHttpClient((sp, client) => Configure(client, sp));
        services.AddRefitClient<ITagsApi>(settings).ConfigureHttpClient((sp, client) => Configure(client, sp));
        services.AddRefitClient<IUserApi>(settings).ConfigureHttpClient((sp, client) => Configure(client, sp));

        return services;
    }
}
