using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.ValueObjects;
using MediatR;

namespace CleanArchitecture.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;

    public CreateOrderHandler(IOrderRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(request.Street, request.City, request.Country, request.ZipCode);
        var order = new Order(request.CustomerId, address);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductName, item.Price, item.Quantity);
        }

        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync(cancellationToken);

        // Simple Side Effect (In a real app, prefer Domain Events handler for this)
        // But here we show simple clean architecture separation, handler coordinates domain & infrastructure services.
        await _emailService.SendOrderConfirmationAsync(order);

        return order.Id;
    }
}
