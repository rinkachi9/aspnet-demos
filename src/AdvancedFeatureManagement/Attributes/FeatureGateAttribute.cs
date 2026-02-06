using AdvancedFeatureManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdvancedFeatureManagement.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FeatureGateAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _toggleName;

    public FeatureGateAttribute(string toggleName)
    {
        _toggleName = toggleName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureService = context.HttpContext.RequestServices.GetRequiredService<IFeatureService>();
        
        if (!featureService.IsEnabled(_toggleName))
        {
            context.Result = new NotFoundObjectResult(new { Message = $"Feature '{_toggleName}' is disabled." });
            return;
        }

        await next();
    }
}
