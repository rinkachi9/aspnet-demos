using Demo.Common.Kafka;
using Demo.Worker.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using KafkaFlow;
using KafkaFlow.TypedHandler;
using AutoOffsetReset = KafkaFlow.AutoOffsetReset;
using SaslMechanism = KafkaFlow.Configuration.SaslMechanism;
using SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol;

namespace Demo.Worker.Extensions;

internal static class KafkaExtensions
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<SampleMessageHandler>();

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

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(kafkaSettings.Brokers)
                .WithName(kafkaSettings.ClientId)
                .WithSecurityInformation(security =>
                {
                    security.SaslMechanism = kafkaSettings.SaslMechanism;
                    security.SecurityProtocol = kafkaSettings.SecurityProtocol;
                    security.SaslUsername = kafkaSettings.Username;
                    security.SaslPassword = kafkaSettings.Password;
                })
                .AddConsumer(consumer => consumer
                    .Topic(kafkaTopics.DemoTopic)
                    .WithGroupId(kafkaSettings.ConsumerGroupId)
                    .WithName(nameof(SampleMessageHandler))
                    .WithBufferSize(100)
                    .WithWorkersCount(kafkaSettings.WorkersCount)
                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                    .AddMiddlewares(m => m
                        .AddDeserializer<JsonMessageDeserializer>()
                        .Add<Middlewares.TracingMiddleware>()
                        .AddTypedHandlers(h => h.AddHandler<SampleMessageHandler>())
                    )
                )
            )
        );

        return services;
    }
}
