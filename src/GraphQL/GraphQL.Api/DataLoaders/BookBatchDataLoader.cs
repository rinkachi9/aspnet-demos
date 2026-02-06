using GraphQL.Api.Data;
using GraphQL.Api.Models;

namespace GraphQL.Api.DataLoaders;

public class BookBatchDataLoader : BatchDataLoader<Guid, List<Book>>
{
    private readonly IBookRepository _repository;

    public BookBatchDataLoader(
        IBookRepository repository,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _repository = repository;
    }

    protected override async Task<IReadOnlyDictionary<Guid, List<Book>>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        // Simulate DB Latency
        // await Task.Delay(10, cancellationToken);
        
        // In a real DB, this would be:
        // await _context.Books.Where(b => keys.Contains(b.AuthorId)).ToListAsync();
        
        var books = _repository.GetBooks()
            .Where(b => keys.Contains(b.AuthorId))
            .ToList();

        return books
            .GroupBy(b => b.AuthorId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
