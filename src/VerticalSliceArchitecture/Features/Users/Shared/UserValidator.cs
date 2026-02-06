using FluentValidation;

namespace VerticalSliceArchitecture.Features.Users.Shared;

// Tier 3: Feature-Specific Shared Logic
// Used by both CreateUser and UpdateUser, but NOT by other features.
// Lives strictly inside Features/Users/Shared.

public class UserValidator : AbstractValidator<UserDto>
{
    public UserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Username too short.");
    }
}

public record UserDto(string Username, string Email);
