using AdvancedDb.Mongo.Entities;
using MongoDB.Driver;

namespace AdvancedDb.Mongo.Services;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Order> _orders;
    private readonly IMongoClient _client;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IMongoDatabase db, IMongoClient client, ILogger<ProductService> logger)
    {
        _products = db.GetCollection<Product>("products");
        _orders = db.GetCollection<Order>("orders");
        _client = client;
        _logger = logger;
    }

    // 1. INDEX MANAGEMENT
    public async Task InitializeIndexesAsync()
    {
        // Simple Index on Name
        await _products.Indexes.CreateOneAsync(
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(x => x.Name)));
        
        // Compound Index
        await _products.Indexes.CreateOneAsync(
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(x => x.Category).Ascending(x => x.Price)));
            
        _logger.LogInformation("Indexes ensured.");
    }

    // 2. BULK OPERATIONS
    public async Task BulkInsertAsync(int count)
    {
        var models = new List<WriteModel<Product>>(count);
        for (int i = 0; i < count; i++)
        {
            models.Add(new InsertOneModel<Product>(new Product
            {
                Name = $"Product-{Guid.NewGuid()}",
                Category = i % 2 == 0 ? "Electronics" : "Books",
                Price = i * 1.5m,
                Stock = 100,
                Version = 1,
                Details = i % 2 == 0 
                    ? new TechDetails { Manufacturer = "Sony", WarrantyMonths = 24, Type = "Tech" }
                    : new BookDetails { Author = "Rowling", ISBN = "123-456", Type = "Book" }
            }));
        }

        // Unordered is faster because it continues even if one fails and sends in parallel batches
        await _products.BulkWriteAsync(models, new BulkWriteOptions { IsOrdered = false });
        _logger.LogInformation("Bulk inserted {Count} products.", count);
    }

    // 3. ACID TRANSACTIONS (Multi-Document)
    public async Task PurchaseAsync(string productId, int quantity)
    {
        using var session = await _client.StartSessionAsync();
        
        // Wrap in transaction
        session.StartTransaction();

        try
        {
            // A. Decrement Stock
            var updateResult = await _products.UpdateOneAsync(session,
                p => p.Id == productId && p.Stock >= quantity, // Check constraint inside update
                Builders<Product>.Update.Inc(p => p.Stock, -quantity));

            if (updateResult.ModifiedCount == 0)
            {
                throw new Exception("Insufficient stock or product not found.");
            }

            // B. Create Order
            await _orders.InsertOneAsync(session, new Order
            {
                ProductId = productId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            });

            // Commit
            await session.CommitTransactionAsync();
            _logger.LogInformation("Purchase Transaction Data committed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction aborted.");
            await session.AbortTransactionAsync();
            throw;
        }
    }

    // 4. OPTIMISTIC CONCURRENCY CONTROL (OCC)
    public async Task UpdateDetailsAsync(string id, string newName, int expectedVersion)
    {
        // ReplaceOne where Id matches AND Version matches
        // Increment version manually
        
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) throw new Exception("Not found");
        
        product.Name = newName;
        product.Version++; // New Version

        var result = await _products.ReplaceOneAsync(
            p => p.Id == id && p.Version == expectedVersion, // OCC Check
            product);

        if (result.ModifiedCount == 0)
        {
            // This means someone else updated the version in mean time
            throw new Exception($"Concurrency Conflict! Expected v{expectedVersion} but DB has newer.");
        }
        
        _logger.LogInformation("Product updated successfully to v{Version}", product.Version);
    }
    
    public async Task<List<Product>> GetAllAsync() => await _products.Find(_ => true).Limit(50).ToListAsync();
}
