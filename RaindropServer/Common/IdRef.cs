using System.Text.Json.Serialization;

namespace RaindropServer.Common;

public record IdRef
{
    [JsonPropertyName("$id")]
    public int Id { get; init; }
}
