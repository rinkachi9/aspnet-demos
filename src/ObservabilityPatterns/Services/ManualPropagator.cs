using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using ObservabilityPatterns.Diagnostics;

namespace ObservabilityPatterns.Services;

public class ManualPropagatorDemo
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public void InjectContext(Activity activity, Dictionary<string, string> carrier)
    {
        // Inject current context into the dictionary (e.g., to send via a custom queue)
        Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), carrier, (c, key, value) =>
        {
            c[key] = value;
        });
        
        Console.WriteLine($"[ManualPropagator] Injected TraceId: {activity.TraceId}");
    }

    public Activity? ExtractContextAndStartActivity(Dictionary<string, string> carrier, string activityName)
    {
        // Extract context from the dictionary
        var parentContext = Propagator.Extract(default, carrier, (c, key) =>
        {
            return c.TryGetValue(key, out var value) ? new[] { value } : Enumerable.Empty<string>();
        });

        // Start a new activity as a child of the extracted context
        Baggage.Current = parentContext.Baggage; // Restore Baggage
        
        var activity = AppDiagnostics.ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
        
        if (activity != null)
        {
             Console.WriteLine($"[ManualPropagator] Started Activity {activity.OperationName} with Parent {activity.ParentId}");
        }

        return activity;
    }
}
