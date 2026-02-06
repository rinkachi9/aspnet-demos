using FluentValidation;
using MediatR;

namespace AdvancedMediatR.Features.Orders;

public record CreateOrderCommand(string ProductName, decimal Price, int Quantity) : IRequest<Guid>;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(ILogger<CreateOrderHandler> logger)
    {
        _logger = logger;
    }

    public Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Order for {ProductName}", request.ProductName);
        // Simulate DB
        return Task.FromResult(Guid.NewGuid());
    }
}
