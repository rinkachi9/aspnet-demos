using AdvancedMessaging.Kafka.Consumers;
using AdvancedMessaging.Kafka.Services;
using KafkaFlow;

var builder = Host.CreateApplicationBuilder(args);

// 1. Shared Infrastructure
builder.Services.AddSingleton<BenchmarkMetrics>();

// 2. Producers (Performance vs Reliability)
builder.Services.AddSingleton<PerformanceKafkaProducer>();
builder.Services.AddSingleton<ReliableKafkaProducer>();
builder.Services.AddSingleton<HighVolumeSeeder>();

// 3. Consumers (ALL REGISTERED)
// They will run simultaneously with different Consumer Groups
builder.Services.AddHostedService<RawBasicConsumer>();
builder.Services.AddHostedService<RawOptimizedConsumer>();

// KafkaFlow Setup
builder.Services.AddKafkaFlowSetup();
builder.Services.AddHostedService<KafkaFlowBootstrapper>();

// 4. Seeder Trigger 
// Runs automatically after startup
builder.Services.AddHostedService<BenchmarkRunner>();

var host = builder.Build();
host.Run();

class BenchmarkRunner(
    ILogger<BenchmarkRunner> logger, 
    HighVolumeSeeder seeder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);
        
        // Scenario 1: Performance Flood
        logger.LogWarning(">>> STARTING BENCHMARK: PERFORMANCE FLOOD (100k messages) <<<");
        await seeder.SeedAsync(100000, reliableMode: false, stoppingToken);

        await Task.Delay(15000, stoppingToken); // Let consumers catch up

        // Scenario 2: Reliable Flood
        logger.LogWarning(">>> STARTING BENCHMARK: RELIABLE FLOOD (10k messages) <<<");
        await seeder.SeedAsync(10000, reliableMode: true, stoppingToken);
    }
}
