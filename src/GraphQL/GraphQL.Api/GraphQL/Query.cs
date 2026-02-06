using GraphQL.Api.Data;
using GraphQL.Api.Data;
using GraphQL.Api.Models;
using GraphQL.Api.Services;

namespace GraphQL.Api.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Book> GetBooks([Service] IBookRepository repository) => repository.GetBooks();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Author> GetAuthors([Service] IBookRepository repository) => repository.GetAuthors();

    public async Task<List<ExternalBook>> GetExternalBooks(
        [Service] IOpenLibraryClient client, 
        string term) => await client.SearchBooksAsync(term);
}
