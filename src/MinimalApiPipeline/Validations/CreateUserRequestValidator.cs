using FluentValidation;
using MinimalApiPipeline.Models;

namespace MinimalApiPipeline.Validations;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThan(18);
    }
}
