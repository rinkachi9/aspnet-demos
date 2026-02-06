using FluentValidation;

namespace CleanArchitecture.Application.Orders.Commands.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductName).NotEmpty();
            items.RuleFor(i => i.Price).GreaterThan(0);
            items.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
