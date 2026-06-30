namespace ECommerce.AI.Application.DTOs;

public record ProductImageDto
{
    public Guid Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? AltText { get; init; }
    public int SortOrder { get; init; }
    public bool IsPrimary { get; init; }
    public Guid ProductId { get; init; }
}