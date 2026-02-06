namespace AdvancedCAP.Domain;

public class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
}
