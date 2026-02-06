using AdvancedSecurity.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedSecurity.Authorization;

public class SameAuthorRequirement : IAuthorizationRequirement { }

public class DocumentAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Document>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   SameAuthorRequirement requirement,
                                                   Document resource)
    {
        // 1. Check if user has "sub" or "name" claim
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
             return Task.CompletedTask;
        }

        var userName = user.Identity.Name ?? user.FindFirst("sub")?.Value;

        // 2. Compare User Name with Document Author
        if (userName != null && resource.Author == userName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
