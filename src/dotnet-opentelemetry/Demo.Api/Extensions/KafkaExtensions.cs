using Demo.Api.Services;
using Demo.Common.Kafka;
using KafkaFlow;
using Microsoft.Extensions.Options;
using Acks = KafkaFlow.Acks;
using SaslMechanism = KafkaFlow.Configuration.SaslMechanism;
using SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol;

namespace Demo.Api.Extensions;

internal static class KafkaExtensions
{
    private const string DefaultBootstrapServers = "redpanda:9092";

    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<KafkaConfig>()
            .Bind(configuration.GetSection("Kafka"))
            .Validate(config =>
                    config.Brokers is { Length: > 0 } &&
                    config.Brokers.All(b => !string.IsNullOrWhiteSpace(b)) &&
                    !string.IsNullOrWhiteSpace(config.Username) &&
                    !string.IsNullOrWhiteSpace(config.Password) &&
                    !string.IsNullOrWhiteSpace(config.ClientId) &&
                    Enum.IsDefined(typeof(SaslMechanism), config.SaslMechanism) &&
                    Enum.IsDefined(typeof(SecurityProtocol), config.SecurityProtocol),
                "Invalid KafkaConfiguration")
            .ValidateOnStart();

        services
            .AddOptions<KafkaTopics>()
            .Bind(configuration.GetSection("KafkaTopics"))
            .Validate(config =>
                    !string.IsNullOrWhiteSpace(config.DemoTopic),
                "Invalid KafkaTopics configuration")
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<KafkaConfig>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<KafkaTopics>>().Value);

        var kafkaSettings = configuration.GetSection("Kafka").Get<KafkaConfig>()!;
        var kafkaTopics = configuration.GetSection("KafkaTopics").Get<KafkaTopics>()!;

        services.AddTransient<IMessagePublisher, KafkaMessagePublisher>();

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(kafkaSettings.Brokers)
                .WithSecurityInformation(security =>
                {
                    security.SaslMechanism = kafkaSettings.SaslMechanism;
                    security.SecurityProtocol = kafkaSettings.SecurityProtocol;
                    security.SaslUsername = kafkaSettings.Username;
                    security.SaslPassword = kafkaSettings.Password;
                })
                .AddProducer(nameof(IMessagePublisher), producer => producer
                    .DefaultTopic(kafkaTopics.DemoTopic)
                    .WithAcks(Acks.Leader)
                    .AddMiddlewares(m =>
                    {
                        m.AddSerializer<JsonMessageSerializer>(_ => new JsonMessageSerializer());
                    }))
            )
        );

        return services;
    }
}
