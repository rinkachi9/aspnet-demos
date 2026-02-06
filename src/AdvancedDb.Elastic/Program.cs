using AdvancedDb.Elastic.Models;
using AdvancedDb.Elastic.Services;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .Authentication(new BasicAuthentication("elastic", "changeme")) // If security enabled
    .DefaultIndex("products");

// For local dev without security/certs (as per docker-compose config)
// We might not need auth if xpack.security.enabled=false
var client = new ElasticsearchClient(new Uri("http://localhost:9200"));

var indexer = new IndexingService(client);
var searcher = new SearchService(client);

// 1. SETUP
Console.WriteLine("Waiting for Elastic...");
await Task.Delay(3000); 

// Reset Indices
var exists = await client.Indices.ExistsAsync("products");
if (exists.Exists) await client.Indices.DeleteAsync("products");

await indexer.SetupIndexAsync();

// Percolator Setup
var percolator = new PercolationService(client);
await percolator.SetupPercolationIndexAsync();
await percolator.RegisterAlertAsync("user_123", 200.0m, "chair"); // Alert if Chair < $200

// 2. SEED DATA
var products = new List<ProductDocument>
{
    new() { Id = Guid.NewGuid(), Title = "iPhone 15 Pro", Description = "Titanium finish.", Category = "Electronics", Price = 999, CreatedAt = DateTime.UtcNow },
    new() { Id = Guid.NewGuid(), Title = "Samsung Galaxy S24", Description = "AI powered.", Category = "Electronics", Price = 899, CreatedAt = DateTime.UtcNow.AddDays(-10) },
    new() { Id = Guid.NewGuid(), Title = "Ergonomic Chair", Description = "Office chair.", Category = "Furniture", Price = 150, CreatedAt = DateTime.UtcNow.AddDays(-20) }, // Matches Alert!
    new() { Id = Guid.NewGuid(), Title = "Gaming Mouse", Description = "Precision mouse.", Category = "Electronics", Price = 50, CreatedAt = DateTime.UtcNow.AddDays(-30) },
    new() { Id = Guid.NewGuid(), Title = "Wooden Desk", Description = "Oak desk.", Category = "Furniture", Price = 450, CreatedAt = DateTime.UtcNow.AddDays(-5) },
    new() { Id = Guid.NewGuid(), Title = "Coffee Maker", Description = "Brew coffee.", Category = "Kitchen", Price = 80, CreatedAt = DateTime.UtcNow }
};

await indexer.IndexDataAsync(products);

// 3. CHECK ALERTS (Percolation)
Console.WriteLine("\n--- Demo 1: Percolation (Alerts) ---");
foreach (var p in products)
{
    await percolator.CheckAlertsAsync(p);
}

// 4. AUTOCOMPLETE
Console.WriteLine("\n--- Demo 2: Autocomplete (EdgeNGram) ---");
// Searching 'iph' should find 'iPhone'
await searcher.SearchProductsAsync("iph"); 

// 5. RELEVANCE (Function Score)
Console.WriteLine("\n--- Demo 3: Relevance (Gaussian Decay) ---");
var relevance = new RelevanceService(client);
await relevance.SearchWithDecayAsync("electronics"); // Should boost newer items

// 6. DEEP PAGINATION
var pager = new PaginationService(client);
await pager.ScrollAllProductsAsync();
