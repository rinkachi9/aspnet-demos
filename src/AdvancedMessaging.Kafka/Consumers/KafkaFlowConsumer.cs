using AdvancedMessaging.Kafka.Services;
using KafkaFlow;

namespace AdvancedMessaging.Kafka.Consumers;

public class KafkaFlowMessageHandler : IMessageHandler<string>
{
    private readonly BenchmarkMetrics _metrics;

    public KafkaFlowMessageHandler(BenchmarkMetrics metrics)
    {
        _metrics = metrics;
    }

    public Task Handle(IMessageContext context, string message)
    {
        _metrics.Increment("KafkaFlow");
        return Task.CompletedTask;
    }
}

public static class KafkaFlowExtensions
{
    public static IServiceCollection AddKafkaFlowSetup(this IServiceCollection services)
    {
        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { "localhost:9092" })
                .AddConsumer(consumer => consumer
                    .Topic("benchmark-topic")
                    .WithGroupId("benchmark-group-kafkaflow")
                    .WithBufferSize(100)
                    .WithWorkersCount(10)
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .AddMiddlewares(middlewares => middlewares
                        .AddTypedHandlers(h => h.AddHandler<KafkaFlowMessageHandler>())
                    )
                )
            )
        );
        return services;
    }
}
