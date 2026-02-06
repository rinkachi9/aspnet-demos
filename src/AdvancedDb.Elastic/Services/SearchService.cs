using AdvancedDb.Elastic.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace AdvancedDb.Elastic.Services;

public class SearchService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "products";

    public SearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task SearchProductsAsync(string searchTerm)
    {
        Console.WriteLine($"--- Searching for: '{searchTerm}' ---");

        var response = await _client.SearchAsync<ProductDocument>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Should(should => should
                        .MultiMatch(m => m
                            .Query(searchTerm)
                            .Fields(f => f.Field(p => p.Title).Field(p => p.Description))
                            .Fuzziness(new Fuzziness("AUTO")) // Typo tolerance
                        )
                    )
                )
                )
            )
            .Highlight(h => h
                .Fields(f => f
                    .Add(new Field("description"), new HighlightField
                    {
                        PreTags = new[] { "<em>" },
                        PostTags = new[] { "</em>" }
                    })
                )
            )
            .Aggregations(a => a
                .Terms("by_category", t => t.Field(p => p.Category)) // Facets
                .Range("price_ranges", r => r
                    .Field(p => p.Price)
                    .Ranges(
                        range => range.To(100).Key("Cheap"),
                        range => range.From(100).To(500).Key("Medium"),
                        range => range.From(500).Key("Expensive")
                    )
                )
            )
        );

        if (!response.IsValidResponse)
        {
            Console.WriteLine("Search failed.");
            return;
        }

        Console.WriteLine($"Found {response.Total} documents.");

        foreach (var hit in response.Hits)
        {
            var product = hit.Source;
            Console.WriteLine($"[{hit.Score:F2}] {product.Title} - ${product.Price}");
            
            if (hit.Highlight?.ContainsKey("description") == true)
            {
                foreach (var snippet in hit.Highlight["description"])
                {
                    Console.WriteLine($"   Highlight: {snippet}");
                }
            }
        }

        Console.WriteLine("\n--- Aggregations ---");
        
        if (response.Aggregations.TryGetValue("by_category", out var categoryAgg) && categoryAgg is StringTermsAggregate terms)
        {
            Console.WriteLine("Categories:");
            foreach (var bucket in terms.Buckets)
            {
                Console.WriteLine($"  {bucket.Key}: {bucket.DocCount}");
            }
        }
    }
}
