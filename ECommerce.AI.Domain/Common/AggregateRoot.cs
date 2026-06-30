using ECommerce.AI.Domain.Events;

namespace ECommerce.AI.Domain.Common;

/// <summary>
/// Base class for aggregate roots in the domain
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the collection of domain events raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to raise</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (typically called after they've been published)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Gets whether this aggregate has any pending domain events
    /// </summary>
    public bool HasDomainEvents => _domainEvents.Count > 0;
}