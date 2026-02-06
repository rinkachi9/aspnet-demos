using Microsoft.AspNetCore.Authorization;

namespace SecurityPatterns.Auth;

// 1. The Requirement (Standard)
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

// 2. The Handler (Logic)
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Strategy 1: Check "permissions" claim directly (if IdP sends them)
        var hasClaim = context.User.Claims.Any(c => 
            c.Type == "permissions" && 
            c.Value == requirement.Permission);

        // Strategy 2: Check "roles" and map to permissions (if legacy RBAC)
        // In a real app, this might query a database or cache
        var isAdmin = context.User.IsInRole("Admin");

        if (hasClaim || (isAdmin)) 
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
