using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AdvancedDb.Mongo.Entities;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Name { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    // KEY for Optimistic Concurrency
    [BsonElement("__v")]
    public int Version { get; set; }

    // POLYMORPHISM
    public ProductDetails? Details { get; set; }
}

[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(BookDetails), typeof(TechDetails))]
public abstract class ProductDetails
{
    public required string Type { get; set; }
}

public class BookDetails : ProductDetails
{
    public required string ISBN { get; set; }
    public required string Author { get; set; }
}

public class TechDetails : ProductDetails
{
    public required string Manufacturer { get; set; }
    public int WarrantyMonths { get; set; }
}

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public required string ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
