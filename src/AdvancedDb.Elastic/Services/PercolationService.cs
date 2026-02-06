using AdvancedDb.Elastic.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace AdvancedDb.Elastic.Services;

public class PercolationService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "queries"; // Index storing QUERIES, not documents

    public PercolationService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task SetupPercolationIndexAsync()
    {
        // Define an index that holds queries
        await _client.Indices.CreateAsync(IndexName, c => c
            .Mappings(m => m
                .Properties(p => p
                    .Percolator("query") // Special field type
                    .Keyword("userId")
                    .Double("targetPrice")
                )
            )
        );
    }

    public async Task RegisterAlertAsync(string userId, decimal targetPrice, string term)
    {
        // User says: "Notify me if 'iPhone' is < $1000"
        var query = new BoolQuery
        {
            Must = new List<Query>
            {
                new TermQuery("title") { Value = term },
                new RangeQuery("price") { Lt = (double)targetPrice }
            }
        };

        // Index the QUERY as a document
        await _client.IndexAsync(new 
        { 
            query = query, // Store the logic
            userId,
            targetPrice 
        }, IndexName);
        
        Console.WriteLine($"Alert registered for {userId}: {term} < {targetPrice}");
    }

    public async Task CheckAlertsAsync(ProductDocument product)
    {
        Console.WriteLine($"Checking alerts for: {product.Title} (${product.Price})");

        // "Percolate" the document against the stored queries
        var response = await _client.SearchAsync<dynamic>(s => s
            .Index(IndexName)
            .Query(q => q
                .Percolate(p => p
                    .Field("query")
                    .Document(product) // Pass the document in memory
                )
            )
        );

        foreach (var hit in response.Hits)
        {
            // hit.Source is dynamic/json element
            Console.WriteLine($"MATCH! Notify user about deal!");
        }
    }
}
