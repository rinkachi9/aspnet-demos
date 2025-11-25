using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Demo.Common.Telemetry;

public static class Propagation
{
    private static readonly TextMapPropagator CompositePropagator = new CompositeTextMapPropagator(
        new TextMapPropagator[]
        {
            new TraceContextPropagator(),
            new BaggagePropagator()
        });

    public static void ConfigureDefaults() =>
        Sdk.SetDefaultTextMapPropagator(CompositePropagator);
}
