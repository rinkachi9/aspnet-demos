using AdvancedDb.Mongo.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AdvancedDb.Mongo.Services;

public class SalesAnalyticsService(IMongoDatabase db)
{
    private readonly IMongoCollection<Order> _orders = db.GetCollection<Order>("orders");

    public async Task<BsonDocument> GetComprehensiveReportAsync()
    {
        // Using BsonDocument builders to avoid strict generic typing issues with Facet in C# driver
        // which can be tricky with complex nested stages.
        
        var revenueStage = new BsonDocument("$group", new BsonDocument 
        { 
            { "_id", BsonNull.Value }, 
            { "TotalItems", new BsonDocument("$sum", "$Quantity") } 
        });

        var topProductsPipeline = new []
        {
            new BsonDocument("$group", new BsonDocument 
            { 
                { "_id", "$ProductId" }, 
                { "Sold", new BsonDocument("$sum", "$Quantity") } 
            }),
            new BsonDocument("$sort", new BsonDocument("Sold", -1)),
            new BsonDocument("$limit", 5)
        };

        var dailyActivityPipeline = new []
        {
            new BsonDocument("$group", new BsonDocument 
            { 
                { "_id", "$CreatedAt" }, 
                { "Count", new BsonDocument("$sum", 1) } 
            }),
            new BsonDocument("$sort", new BsonDocument("_id", -1)),
            new BsonDocument("$limit", 10)
        };

        var facetStage = new BsonDocument("$facet", new BsonDocument
        {
            { "TotalRevenue", new BsonArray { revenueStage } },
            { "TopProducts", new BsonArray(topProductsPipeline) },
            { "DailyActivity", new BsonArray(dailyActivityPipeline) }
        });

        var pipeline = new EmptyPipelineDefinition<Order>()
            .AppendStage<Order, BsonDocument, BsonDocument>(facetStage);

        var aggregation = await _orders.AggregateAsync(pipeline);
        return await aggregation.FirstOrDefaultAsync();
    }
}
