namespace ECommerce.AI.Domain.Events.Category;

/// <summary>
/// Domain event raised when a new category is created
/// </summary>
public record CategoryCreated(
    Guid CategoryId,
    string Name,
    Guid? ParentCategoryId
) : DomainEvent;

/// <summary>
/// Domain event raised when category status changes
/// </summary>
public record CategoryStatusChanged(
    Guid CategoryId,
    bool IsActive
) : DomainEvent;

/// <summary>
/// Domain event raised when category hierarchy changes
/// </summary>
public record CategoryParentChanged(
    Guid CategoryId,
    Guid? OldParentId,
    Guid? NewParentId
) : DomainEvent;