using GraphQL.Api.Models;
using System.Text.Json;

namespace GraphQL.Api.Services;

public interface IOpenLibraryClient
{
    Task<List<ExternalBook>> SearchBooksAsync(string term);
}

public class OpenLibraryClient : IOpenLibraryClient
{
    private readonly HttpClient _httpClient;

    public OpenLibraryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ExternalBook>> SearchBooksAsync(string term)
    {
        var response = await _httpClient.GetAsync($"https://openlibrary.org/search.json?q={term}&limit=5");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OpenLibraryResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Docs is null) return new List<ExternalBook>();

        return result.Docs.Select(d => new ExternalBook(
            d.Title,
            d.Author_Name?.FirstOrDefault() ?? "Unknown",
            d.Cover_I > 0 ? $"https://covers.openlibrary.org/b/id/{d.Cover_I}-M.jpg" : ""
        )).ToList();
    }
}
