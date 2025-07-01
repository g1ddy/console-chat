using System.Text.Json.Serialization;

namespace RaindropTools.Common;

public class ParentRef
{
    [JsonPropertyName("$id")]
    public int Id { get; set; }
}
