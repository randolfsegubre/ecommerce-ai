namespace ECommerce.AI.Application.DTOs;

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public CategoryDto? ParentCategory { get; init; }
    public List<CategoryDto> SubCategories { get; init; } = new();
    public List<ProductDto> Products { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}