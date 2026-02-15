namespace RaindropServer;

/// <summary>
/// Options for configuring the Raindrop API client.
/// </summary>
public class RaindropOptions
{
    /// <summary>
    /// Base URL of the Raindrop API.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// The timeout for HTTP requests in seconds. Defaults to 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
