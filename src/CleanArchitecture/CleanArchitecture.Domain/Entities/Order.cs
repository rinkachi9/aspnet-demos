using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Events;
using CleanArchitecture.Domain.ValueObjects;

namespace CleanArchitecture.Domain.Entities;

public class Order : BaseEntity
{
    public int CustomerId { get; private set; }
    public Address ShippingAddress { get; private set; }
    public DateTime OrderDate { get; private set; }
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Price * i.Quantity);

    private Order() { } // EF

    public Order(int customerId, Address address)
    {
        CustomerId = customerId;
        ShippingAddress = address;
        OrderDate = DateTime.UtcNow;

        AddDomainEvent(new OrderCreatedEvent(this));
    }

    public void AddItem(string productName, decimal price, int quantity)
    {
        _items.Add(new OrderItem(productName, price, quantity));
    }
}
