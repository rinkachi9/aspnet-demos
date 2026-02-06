using AdvancedDb.Elastic.Models;
using Elastic.Clients.Elasticsearch;

namespace AdvancedDb.Elastic.Services;

public class IndexingService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "products";

    public IndexingService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task SetupIndexAsync()
    {
        var exists = await _client.Indices.ExistsAsync(IndexName);
        
        if (!exists.Exists) // Fixed: ExistsAndIsValid -> Exists
        {
            Console.WriteLine($"Creating index {IndexName} with EdgeNGram...");
            await _client.Indices.CreateAsync<ProductDocument>(IndexName, c => c // Fixed: added generic type
                .Settings(s => s
                    .Analysis(a => a
                        .TokenFilters(tf => tf
                            .EdgeNGram("autocomplete_filter", eng => eng
                                .MinGram(2)
                                .MaxGram(10)
                            )
                        )
                        .Analyzers(an => an
                            .Custom("autocomplete", ca => ca
                                .Tokenizer("standard")
                                .Filter(new[] { "lowercase", "autocomplete_filter" })
                            )
                        )
                    )
                )
                .Mappings(m => m
                    .Properties(p => p // Fixed: Generic Properties<T> usage
                        .Text(t => t.Title, c => c.Analyzer("autocomplete").SearchAnalyzer("standard"))
                        .Text(t => t.Description, c => c.Analyzer("english"))
                        .Keyword(t => t.Category) 
                        .Double(t => t.Price) // Double is correct in v8 helpers usually, checking context
                        .Date(t => t.CreatedAt)
                    )
                )
            );
        }
    }

    public async Task IndexDataAsync(IEnumerable<ProductDocument> products)
    {
        Console.WriteLine($"Indexing {products.Count()} products...");
        
        var response = await _client.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(products)
        );

        if (response.Errors)
        {
            foreach (var item in response.ItemsWithErrors)
            {
                // Fixed: Error.Reason access
                Console.WriteLine($"Failed to index {item.Id}: {item.Error?.Reason}");
            }
        }
        else
        {
            Console.WriteLine("Indexing completed successfully.");
        }
    }
}
