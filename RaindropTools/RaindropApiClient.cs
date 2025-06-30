using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaindropTools;

/// <summary>
/// Typed HTTP client used to call the Raindrop API.
/// </summary>
public class RaindropApiClient
{
    private readonly HttpClient _client;

    public RaindropApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);

        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        return await _client.SendAsync(request);
    }
}
