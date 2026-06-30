namespace ECommerce.AI.Domain.Events;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When the domain event occurred
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }
}