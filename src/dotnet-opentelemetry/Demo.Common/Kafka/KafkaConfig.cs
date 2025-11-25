using KafkaFlow.Configuration;

namespace Demo.Common.Kafka;

public class KafkaConfig
{
    public required string[] Brokers { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required SaslMechanism SaslMechanism { get; set; }
    public required SecurityProtocol SecurityProtocol { get; set; }
    public required string ClientId { get; set; }

    public string ConsumerGroupId { get; set; }
    public int WorkersCount { get; set; }

    public bool EnableSslCertificateVerification { get; set; } = true;
}
