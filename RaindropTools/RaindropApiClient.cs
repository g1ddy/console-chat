using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaindropTools;

internal static class RaindropApiClient
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://api.raindrop.io/rest/v1/")
    };

    public static async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        return await _client.SendAsync(request);
    }
}
