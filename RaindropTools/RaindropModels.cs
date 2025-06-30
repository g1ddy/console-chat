using System.Text.Json.Serialization;

namespace RaindropTools;

/// <summary>
/// Generic API response containing a single item.
/// </summary>
public record ItemResponse<T>(bool Result, T Item);

/// <summary>
/// Generic API response containing a list of items.
/// </summary>
public record ItemsResponse<T>(bool Result, List<T> Items);

/// <summary>
/// Response for operations that only return success flag.
/// </summary>
public record SuccessResponse(bool Result);

public class Collection
{
    [JsonPropertyName("_id")]
    public int Id { get; set; }

    public string? Title { get; set; }

    public ParentRef? Parent { get; set; }

    public string? Color { get; set; }

    public List<string>? Cover { get; set; }

    [JsonPropertyName("public")]
    public bool? Public { get; set; }
}

public class ParentRef
{
    [JsonPropertyName("$id")]
    public int Id { get; set; }
}

public class Raindrop
{
    [JsonPropertyName("_id")]
    public long Id { get; set; }

    public string? Title { get; set; }

    public string? Link { get; set; }

    public string? Excerpt { get; set; }

    public List<string>? Tags { get; set; }

    public bool? Important { get; set; }

    public ParentRef? Collection { get; set; }
}
