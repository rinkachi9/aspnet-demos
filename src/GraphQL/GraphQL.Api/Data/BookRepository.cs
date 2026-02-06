using GraphQL.Api.Models;

namespace GraphQL.Api.Data;

public interface IBookRepository
{
    IQueryable<Book> GetBooks();
    IQueryable<Author> GetAuthors();
    void AddBook(Book book);
}

public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Author> _authors;
    private readonly List<Book> _books;

    public InMemoryBookRepository()
    {
        _authors = new List<Author>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Robert C. Martin" },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Martin Fowler" }
        };

        _books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "Clean Code", AuthorId = _authors[0].Id, Author = _authors[0] },
            new() { Id = Guid.NewGuid(), Title = "Clean Architecture", AuthorId = _authors[0].Id, Author = _authors[0] },
            new() { Id = Guid.NewGuid(), Title = "Patterns of Enterprise Application Architecture", AuthorId = _authors[1].Id, Author = _authors[1] }
        };

        _authors[0].Books = _books.Where(b => b.AuthorId == _authors[0].Id).ToList();
        _authors[1].Books = _books.Where(b => b.AuthorId == _authors[1].Id).ToList();
    }

    public IQueryable<Book> GetBooks() => _books.AsQueryable();
    public IQueryable<Author> GetAuthors() => _authors.AsQueryable();
    
    public void AddBook(Book book)
    {
        _books.Add(book);
        
        // Update author relationship if exists in memory
        var author = _authors.FirstOrDefault(a => a.Id == book.AuthorId);
        author?.Books.Add(book);
    }
}
