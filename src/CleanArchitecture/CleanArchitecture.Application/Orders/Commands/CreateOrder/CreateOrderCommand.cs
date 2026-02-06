using MediatR;

namespace CleanArchitecture.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    int CustomerId,
    string Street,
    string City,
    string Country,
    string ZipCode,
    List<OrderItemDto> Items) : IRequest<int>;

public record OrderItemDto(string ProductName, decimal Price, int Quantity);
