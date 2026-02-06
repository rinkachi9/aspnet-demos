using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace DomainEventsOutbox.Api.Domain;

public abstract class BaseEntity
{
    private readonly List<INotification> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
