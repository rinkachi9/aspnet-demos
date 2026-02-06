using AdvancedFeatureManagement.Attributes;
using AdvancedFeatureManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Unleash;

namespace AdvancedFeatureManagement.Controllers;

[ApiController]
[Route("features")]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureService _featureService;

    public FeaturesController(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    [HttpGet("status/{featureName}")]
    public IActionResult GetStatus(string featureName)
    {
        // Simple check
        var enabled = _featureService.IsEnabled(featureName);
        return Ok(new { Feature = featureName, Enabled = enabled });
    }

    [HttpGet("beta-feature")]
    [FeatureGate("BetaFeature")] // PROTECTED ENDPOINT
    public IActionResult GetBetaFeature()
    {
        return Ok(new { Message = "You have access to the Beta Feature!" });
    }

    [HttpGet("gradual-rollout")]
    public IActionResult GetGradualRollout([FromQuery] string userId)
    {
        // Contextual check for Gradual Rollout (Stickiness)
        var context = new UnleashContext
        {
            UserId = userId
        };

        var enabled = _featureService.IsEnabled("NewAlgorithm", context);

        if (enabled)
        {
            return Ok(new { Algorithm = "v2 (Enhanced)", UserId = userId });
        }
        
        return Ok(new { Algorithm = "v1 (Legacy)", UserId = userId });
    }
}
