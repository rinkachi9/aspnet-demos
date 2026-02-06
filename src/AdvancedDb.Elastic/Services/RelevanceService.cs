using AdvancedDb.Elastic.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace AdvancedDb.Elastic.Services;

public class RelevanceService
{
    private readonly ElasticsearchClient _client;

    public RelevanceService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task SearchWithDecayAsync(string term)
    {
        Console.WriteLine($"Searching '{term}' with Gaussian Decay (Newer = Better)...");

        var response = await _client.SearchAsync<ProductDocument>(s => s
            .Index("products")
            .Query(q => q
                .FunctionScore(fs => fs
                    .Query(baseQuery => baseQuery
                        .Match(m => m.Field(f => f.Title).Query(term))
                    )
                    .Functions(funcs => funcs
                        .Add(f => f.Gaussian(g => g
                            .Field(f => f.CreatedAt)
                            .Origin(DateTime.UtcNow) // Center = Now
                            .Scale(TimeSpan.FromDays(30))            // -50% score every 30 days
                            .Offset(TimeSpan.FromDays(1))            // First day perfect score
                            .Decay(0.5)
                        ))
                    )
                    .BoostMode(FunctionScoreBoostMode.Multiply)
                )
            )
        );

        foreach (var hit in response.Hits)
        {
            Console.WriteLine($"[{hit.Score:F2}] {hit.Source.Title} ({hit.Source.CreatedAt.ToShortDateString()})");
        }
    }
}
