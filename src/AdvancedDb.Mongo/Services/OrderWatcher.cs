using AdvancedDb.Mongo.Entities;
using MongoDB.Driver;

namespace AdvancedDb.Mongo.Services;

// Uses CHANGE STREAMS to react to DB events in real-time
public class OrderWatcher : BackgroundService
{
    private readonly IMongoClient _client;
    private readonly ILogger<OrderWatcher> _logger;

    public OrderWatcher(IMongoClient client, ILogger<OrderWatcher> logger)
    {
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _client.GetDatabase("advanced_mongo_db"); // Hardcoded for demo simplicity
        var collection = db.GetCollection<Order>("orders");

        var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };

        // Standard cursor iteration for Mongo Drivers
        using var cursor = await collection.WatchAsync(options, stoppingToken);
        _logger.LogInformation("OrderWatcher started. Waiting for changes...");

        try
        {
            while (await cursor.MoveNextAsync(stoppingToken))
            {
                foreach (var change in cursor.Current)
                {
                    if (change.OperationType == ChangeStreamOperationType.Insert)
                    {
                        var order = change.FullDocument;
                        _logger.LogWarning("🚨 NEW ORDER DETECTED VIA OPLOG! ID: {Id}, Qty: {Qty}. Triggering Shipment...", 
                            order.Id, order.Quantity);
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Watcher died");
        }
    }
}
