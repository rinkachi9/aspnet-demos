namespace GraphQL.Api.Models;

public class Author
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
