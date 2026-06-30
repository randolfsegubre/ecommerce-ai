namespace ECommerce.AI.Application.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? ComparePrice { get; init; }
    public int StockQuantity { get; init; }
    public int MinStockLevel { get; init; }
    public bool IsActive { get; init; }
    public bool IsFeatured { get; init; }
    public double Weight { get; init; }
    public string? Dimensions { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public Guid CategoryId { get; init; }
    public CategoryDto? Category { get; init; }
    public List<ProductImageDto> Images { get; init; } = new();
    public List<ProductSpecificationDto> Specifications { get; init; } = new();
    public bool IsInStock => StockQuantity > 0;
    public bool IsLowStock => StockQuantity <= MinStockLevel;
    public bool HasDiscount => ComparePrice.HasValue && ComparePrice > Price;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}