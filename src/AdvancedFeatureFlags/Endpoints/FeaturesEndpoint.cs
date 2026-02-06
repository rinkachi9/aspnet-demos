using AdvancedFeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace AdvancedFeatureFlags.Endpoints;

public static class FeaturesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/features");

        // 1. Basic .NET Feature Management (appsettings.json)
        // Uses IFeatureManager
        group.MapGet("/discount", async (IFeatureManager featureManager) =>
        {
            if (await featureManager.IsEnabledAsync("DiscountFeature"))
            {
                return Results.Ok(new { message = "Discount Applied! 20% OFF", price = 80 });
            }
            return Results.Ok(new { message = "Standard Price", price = 100 });
        });

        // 2. Feature Gate Attribute (Advanced: requires Controller, or custom Filter for Minimal API)
        // For Minimal API, we use Conditionally maps or manual check. 
        // Showing manual check for "BetaEndpoint".

        group.MapGet("/beta", async (IFeatureManager featureManager) =>
        {
            if (!await featureManager.IsEnabledAsync("BetaEndpoint"))
            {
                return Results.NotFound();
            }
            return Results.Ok("Welcome to BETA functionality!");
        });

        // 3. Unleash (Enterprise / Docker)
        // Uses IUnleashService
        group.MapGet("/experimental", (IUnleashService unleash) =>
        {
            if (unleash.IsEnabled("experimental-feature"))
            {
                return Results.Ok("Experimental Logic V2 (Enabled via Unleash)");
            }
            return Results.Ok("Stable Logic V1");
        });
    }
}
