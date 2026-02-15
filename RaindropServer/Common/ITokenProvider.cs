using System.Net.Http.Headers;

namespace RaindropServer.Common;

/// <summary>
/// Provides an access token for Raindrop API requests.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Gets the current access token.
    /// </summary>
    string? GetToken();
}
