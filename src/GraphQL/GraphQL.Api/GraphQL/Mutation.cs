using GraphQL.Api.Data;
using GraphQL.Api.Models;

namespace GraphQL.Api.GraphQL;

public class Mutation
{
    public async Task<Book> AddBook(
        [Service] IBookRepository repository,
        string title,
        Guid authorId)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            AuthorId = authorId
        };
        
        // In a real app, this would be async
        repository.AddBook(book);
        
        return await Task.FromResult(book);
    }
}
