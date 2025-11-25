using System.Diagnostics;

namespace Demo.Common.Telemetry;

public static class Telemetry
{
    public const string ServiceNameApi = "demo.api";
    public const string ServiceNameWorker = "demo.worker";
    public static readonly ActivitySource ActivitySource = new("demo");

    public static string ServiceVersion =>
        typeof(Telemetry).Assembly.GetName().Version?.ToString() ?? "1.0.0";
}
