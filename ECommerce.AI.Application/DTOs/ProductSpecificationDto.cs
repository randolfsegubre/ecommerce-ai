namespace ECommerce.AI.Application.DTOs;

public record ProductSpecificationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public Guid ProductId { get; init; }
}