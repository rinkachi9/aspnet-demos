using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class BaseEvent : INotification
{
}

public interface INotification { } // Minimal MediatR-like interface if we don't want dep in Domain, but usually we add MediatR.Contracts or just empty marker. 
// Actually to keep Domain clean of MediatR dependency, we just define BaseEvent. 
// Application layer will map this to MediatR notifications or we implement INotification from MediatR if we allow dependency.
// Rule: Clean Architecture Domain should have NO dependencies. So I will NOT depend on MediatR here.
