using AdvancedDb.Elastic.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;

namespace AdvancedDb.Elastic.Services;

public class PaginationService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "products";

    public PaginationService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task ScrollAllProductsAsync()
    {
        Console.WriteLine("\n--- Deep Pagination (SearchAfter) ---");

        // 1. First Page (Sorted is mandatory for SearchAfter)
        // Ideally use PIT (Point In Time) for consistency, but simple SearchAfter works for demo.
        var response = await _client.SearchAsync<ProductDocument>(s => s
            .Index(IndexName)
            .Size(2) // Small size to force pagination
            .Sort(sort => sort
                .Field(f => f.CreatedAt, new FieldSort { Order = SortOrder.Desc })
                .Field(f => f.Id, new FieldSort { Order = SortOrder.Asc }) // Tie-breaker
            )
        );

        int pageCount = 1;
        ProcessPage(response, pageCount);

        // 2. Loop until no more results
        while (response.Hits.Count > 0)
        {
            var lastHit = response.Hits.Last();
            var sortValues = lastHit.Sort; // Cursor

            if (sortValues == null || sortValues.Count == 0) break;

            response = await _client.SearchAsync<ProductDocument>(s => s
                .Index(IndexName)
                .Size(2)
                .Sort(sort => sort
                    .Field(f => f.CreatedAt, new FieldSort { Order = SortOrder.Desc })
                    .Field(f => f.Id, new FieldSort { Order = SortOrder.Asc })
                )
                .SearchAfter(sortValues.ToArray()) // The Magic
            );

            if (response.Hits.Count == 0) break;

            pageCount++;
            ProcessPage(response, pageCount);
        }
    }

    private void ProcessPage(SearchResponse<ProductDocument> response, int page)
    {
        Console.WriteLine($"Page {page}:");
        foreach (var hit in response.Hits)
        {
            Console.WriteLine($" - {hit.Source.Title} ({hit.Source.CreatedAt})");
        }
    }
}
