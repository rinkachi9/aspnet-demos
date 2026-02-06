using System.Text.Json.Serialization;

namespace AdvancedDb.Elastic.Models;

public class ProductDocument
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
