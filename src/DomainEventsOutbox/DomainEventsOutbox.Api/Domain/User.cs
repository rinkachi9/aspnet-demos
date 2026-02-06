using MediatR;

namespace DomainEventsOutbox.Api.Domain;

public class User : BaseEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public User(string email, string fullName)
    {
        Id = Guid.NewGuid();
        Email = email;
        FullName = fullName;

        AddDomainEvent(new UserRegisteredEvent(Id, Email));
    }

    // EF Core
    private User() { } 
}

public record UserRegisteredEvent(Guid UserId, string Email) : INotification;
