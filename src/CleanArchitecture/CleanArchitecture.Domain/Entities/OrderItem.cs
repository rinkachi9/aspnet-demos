using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.Entities;

public class OrderItem : BaseEntity
{
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public OrderItem(string productName, decimal price, int quantity)
    {
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }
}
