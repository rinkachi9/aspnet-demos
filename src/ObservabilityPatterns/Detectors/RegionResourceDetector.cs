using OpenTelemetry.Resources;

namespace ObservabilityPatterns.Detectors;

public class RegionResourceDetector : IResourceDetector
{
    private readonly string _region;

    public RegionResourceDetector(IConfiguration configuration)
    {
        _region = configuration["Region"] ?? "unknown";
    }

    public Resource Detect()
    {
        return ResourceBuilder.CreateEmpty()
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("cloud.region", _region),
                new KeyValuePair<string, object>("deployment.environment", "production-sim")
            })
            .Build();
    }
}
