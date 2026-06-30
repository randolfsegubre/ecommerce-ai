namespace ECommerce.AI.Domain.Events.Product;

/// <summary>
/// Domain event raised when a new product is created
/// </summary>
public record ProductCreated(
    Guid ProductId,
    string Name,
    string SKU,
    decimal Price,
    Guid CategoryId
) : DomainEvent;

/// <summary>
/// Domain event raised when product price is changed
/// </summary>
public record ProductPriceChanged(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    decimal? OldComparePrice,
    decimal? NewComparePrice
) : DomainEvent;

/// <summary>
/// Domain event raised when product stock changes
/// </summary>
public record ProductStockChanged(
    Guid ProductId,
    int OldQuantity,
    int NewQuantity,
    bool IsNowLowStock,
    bool IsNowOutOfStock
) : DomainEvent;

/// <summary>
/// Domain event raised when product becomes low on stock
/// </summary>
public record ProductLowStockAlert(
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    int MinStockLevel
) : DomainEvent;

/// <summary>
/// Domain event raised when product goes out of stock
/// </summary>
public record ProductOutOfStock(
    Guid ProductId,
    string ProductName
) : DomainEvent;

/// <summary>
/// Domain event raised when product is activated or deactivated
/// </summary>
public record ProductStatusChanged(
    Guid ProductId,
    bool IsActive
) : DomainEvent;