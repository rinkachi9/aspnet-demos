using System.Diagnostics;
using OpenTelemetry;

namespace ObservabilityPatterns.Processors;

public class RedactionProcessor : BaseProcessor<Activity>
{
    private static readonly HashSet<string> SensitiveTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "user.email", "password", "credit_card"
    };

    public override void OnEnd(Activity activity)
    {
        // Iterate over tags and redact sensitive ones
        foreach (var tag in activity.Tags)
        {
            if (SensitiveTags.Contains(tag.Key))
            {
                activity.SetTag(tag.Key, "***REDACTED***");
            }
        }
    }
}
