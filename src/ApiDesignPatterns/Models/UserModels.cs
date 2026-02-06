using FluentValidation;

namespace ApiDesignPatterns.Models;

public class CreateUserRequest
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(5).MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18).WithMessage("User must be an adult.");
    }
}
