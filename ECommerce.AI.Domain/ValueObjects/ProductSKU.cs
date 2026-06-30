using System.Text.RegularExpressions;

namespace ECommerce.AI.Domain.ValueObjects;

/// <summary>
/// Value object representing a product SKU
/// </summary>
public record ProductSKU
{
    private static readonly Regex SkuValidationRegex = new(@"^[A-Z0-9\-_]{3,50}$", RegexOptions.Compiled);
    
    public string Value { get; }

    public ProductSKU(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be null or empty", nameof(value));

        var normalizedValue = value.Trim().ToUpperInvariant();
        
        if (!SkuValidationRegex.IsMatch(normalizedValue))
            throw new ArgumentException("SKU must be 3-50 characters long and contain only letters, numbers, hyphens, and underscores", nameof(value));

        Value = normalizedValue;
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSKU sku) => sku.Value;
    public static implicit operator ProductSKU(string value) => new(value);
}