using System.Text.Json.Serialization;

namespace RaindropServer.Common;

public class IdRef
{
    [JsonPropertyName("$id")]
    public int Id { get; set; }
}
