namespace ECommerce.AI.Domain.ValueObjects;

/// <summary>
/// Value object representing product dimensions
/// </summary>
public record ProductDimensions
{
    public double Length { get; }
    public double Width { get; }
    public double Height { get; }
    public string Unit { get; }

    public ProductDimensions(double length, double width, double height, string unit = "cm")
    {
        if (length <= 0 || width <= 0 || height <= 0)
            throw new ArgumentException("All dimensions must be positive");

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be null or empty", nameof(unit));

        Length = Math.Round(length, 2);
        Width = Math.Round(width, 2);
        Height = Math.Round(height, 2);
        Unit = unit.ToLowerInvariant();
    }

    public double Volume => Length * Width * Height;

    public ProductDimensions ConvertTo(string targetUnit, double conversionFactor)
    {
        if (string.IsNullOrWhiteSpace(targetUnit))
            throw new ArgumentException("Target unit cannot be null or empty", nameof(targetUnit));

        if (conversionFactor <= 0)
            throw new ArgumentException("Conversion factor must be positive", nameof(conversionFactor));

        return new ProductDimensions(
            Length * conversionFactor,
            Width * conversionFactor,
            Height * conversionFactor,
            targetUnit
        );
    }

    public override string ToString() => $"{Length} x {Width} x {Height} {Unit}";

    public static ProductDimensions FromString(string dimensionsString, string unit = "cm")
    {
        if (string.IsNullOrWhiteSpace(dimensionsString))
            throw new ArgumentException("Dimensions string cannot be null or empty", nameof(dimensionsString));

        var parts = dimensionsString.Split(new[] { 'x', 'X', '*', '×' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 3)
            throw new ArgumentException("Dimensions string must contain exactly 3 measurements separated by 'x'", nameof(dimensionsString));

        if (!double.TryParse(parts[0].Trim(), out var length) ||
            !double.TryParse(parts[1].Trim(), out var width) ||
            !double.TryParse(parts[2].Trim(), out var height))
        {
            throw new ArgumentException("All dimensions must be valid numbers", nameof(dimensionsString));
        }

        return new ProductDimensions(length, width, height, unit);
    }
}