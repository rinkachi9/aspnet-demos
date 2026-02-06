using ProtoBuf;

namespace ApiDesignPatterns.Models;

[ProtoContract]
public class ProductDto
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public string Name { get; set; } = string.Empty;

    [ProtoMember(3)]
    public decimal Price { get; set; }

    [ProtoMember(4)]
    public string Category { get; set; } = string.Empty;
}
