using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AdvancedMessaging.RabbitMQ.Services;

public class RawRabbitMqWorker : BackgroundService
{
    private readonly ILogger<RawRabbitMqWorker> _logger;
    private IConnection _connection;
    private IModel _channel;
    private const string QueueName = "raw-tasks-queue";

    public RawRabbitMqWorker(ILogger<RawRabbitMqWorker> logger)
    {
        _logger = logger;
        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Ensure Queue Exists
        _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        // QOS (Quality of Service) - PREFETCH COUNT
        // Critical for worker fairness. Tell RabbitMQ not to give more than 1 message to this worker at a time.
        // This prevents one worker from hogging all messages if it's slow.
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            _logger.LogInformation($"[RawRabbit] Received: {message}");

            try
            {
                // Simulate Heavy Processing
                await Task.Delay(1000, stoppingToken); 
                
                // MANUAL ACK
                // Acknowledge ONLY after processing is complete.
                // If this line isn't reached, RabbitMQ will redeliver the message to another worker.
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                _logger.LogInformation($"[RawRabbit] Acked: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message");
                // Application specific: BasicNack or BasicReject can be used here.
                // Requeue = true if temporary failure.
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer); // autoAck: false is KEY

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
