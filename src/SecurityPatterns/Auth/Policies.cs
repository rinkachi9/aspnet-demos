using Microsoft.AspNetCore.Authorization;

namespace SecurityPatterns.Auth;

public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge) => MinimumAge = minimumAge;
}

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        // Check if user has context
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask; // Not authenticated, so requirement not met (or let default handling verify auth)
        }

        // Simulating finding a claim. In real OIDC, "birthdate" or custom "age" claim might exist.
        // For verify, we'll check for a custom claim "custom_age"
        var ageClaim = context.User.FindFirst("custom_age");

        if (ageClaim != null && int.TryParse(ageClaim.Value, out var age))
        {
            if (age >= requirement.MinimumAge)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
