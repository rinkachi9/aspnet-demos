using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ObservabilityPatterns.Diagnostics;

namespace ObservabilityPatterns.Health;

public class PrometheusHealthCheckPublisher : IHealthCheckPublisher
{
    // Gauge: 0 = Unhealthy, 1 = Degraded, 2 = Healthy
    private static readonly ObservableGauge<int> HealthStatusGauge = AppDiagnostics.Meter.CreateObservableGauge(
        "aspnetcore_healthcheck_status",
        () => _currentHealthStatus,
        description: "Overall Health Status: 0=Unhealthy, 1=Degraded, 2=Healthy");

    private static int _currentHealthStatus = 2; // Start as Healthy

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        _currentHealthStatus = report.Status switch
        {
            HealthStatus.Healthy => 2,
            HealthStatus.Degraded => 1,
            HealthStatus.Unhealthy => 0,
            _ => 0
        };

        return Task.CompletedTask;
    }
}
