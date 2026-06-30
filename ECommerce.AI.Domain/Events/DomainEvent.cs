namespace ECommerce.AI.Domain.Events;

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();
}