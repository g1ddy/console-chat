using System.Net.Http.Headers;

namespace RaindropServer.Common;

/// <summary>
/// Injects the Authorization header from the token provider into outgoing requests.
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public AuthHeaderHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenProvider.GetToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            // Assume Bearer token for Raindrop API
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
