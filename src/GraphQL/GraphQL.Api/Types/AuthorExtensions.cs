using GraphQL.Api.DataLoaders;
using GraphQL.Api.Models;

namespace GraphQL.Api.Types;

[ExtendObjectType(typeof(Author))]
public class AuthorExtensions
{
    // This overrides the 'books' field on the Author type
    public async Task<List<Book>> GetBooksAsync(
        [Parent] Author author,
        BookBatchDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        // This call will be batched!
        return await dataLoader.LoadAsync(author.Id, cancellationToken) ?? new List<Book>();
    }
}
