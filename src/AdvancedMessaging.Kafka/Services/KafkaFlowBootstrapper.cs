using KafkaFlow;

namespace AdvancedMessaging.Kafka.Services;

public class KafkaFlowBootstrapper : IHostedService
{
    private readonly IKafkaBus _bus;

    public KafkaFlowBootstrapper(IKafkaBus bus)
    {
        _bus = bus;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _bus.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _bus.StopAsync();
    }
}
