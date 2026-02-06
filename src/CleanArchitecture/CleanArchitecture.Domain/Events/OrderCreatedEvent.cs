using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Events;

public class OrderCreatedEvent : BaseEvent
{
    public Order Order { get; }

    public OrderCreatedEvent(Order order)
    {
        Order = order;
    }
}
