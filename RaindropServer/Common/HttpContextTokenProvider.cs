namespace RaindropServer.Common;

/// <summary>
/// Retrieves the token from the current HTTP request context.
/// </summary>
public class HttpContextTokenProvider : ITokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetToken()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            // Just return the raw header value, let the caller handle scheme parsing if needed.
            // But since we want the token for the downstream request, and Refit's AuthorizationHeaderValue expects scheme + parameter,
            // we should probably return just the parameter (token) if it's Bearer, or the whole thing if generic.
            // Let's assume standard Bearer usage: "Bearer <token>"

            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring("Bearer ".Length).Trim();
            }

            return headerValue;
        }

        return null;
    }
}
