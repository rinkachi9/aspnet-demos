using RabbitMQ.Client;
using System.Text;
using AdvancedMessaging.RabbitMQ.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<RawRabbitMqWorker>();
builder.Services.AddHostedService<RabbitSimulationWorker>();

var host = builder.Build();
host.Run();

// Simple Publisher Simulation
public class RabbitSimulationWorker(ILogger<RabbitSimulationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "raw-tasks-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        for (int i = 1; i <= 5; i++)
        {
            string message = $"Task #{i}";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: "raw-tasks-queue", basicProperties: null, body: body);
            logger.LogInformation($"[Publisher] Sent {message}");
        }
    }
}
