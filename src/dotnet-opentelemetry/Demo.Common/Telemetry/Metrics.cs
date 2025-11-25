using System.Diagnostics.Metrics;

namespace Demo.Common.Telemetry;

public static class Metrics
{
    public static readonly Meter Meter = new("demo.meter");

    public static readonly Histogram<double> MessageProcessingDuration = Meter.CreateHistogram<double>(
        name: "demo.message.processing.duration",
        unit: "ms",
        description: "Time spent processing a message in the worker");
}
