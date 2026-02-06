using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ObservabilityPatterns.Diagnostics;

public static class AppDiagnostics
{
    public const string ServiceName = "ObservabilityPatterns";
    public const string ServiceVersion = "1.0.0";

    // ActivitySource for manual Tracing
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    // Meter for manual Metrics
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);

    // Custom Counters
    public static readonly Counter<long> OrdersPlaced = Meter.CreateCounter<long>("orders_placed_total", description: "Total number of orders placed");
    public static readonly Histogram<double> OrderProcessingTime = Meter.CreateHistogram<double>("order_processing_seconds", unit: "s", description: "Time taken to process an order");
}
