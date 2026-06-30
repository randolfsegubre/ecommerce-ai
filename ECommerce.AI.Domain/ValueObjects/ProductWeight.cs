namespace ECommerce.AI.Domain.ValueObjects;

/// <summary>
/// Value object representing product weight
/// </summary>
public record ProductWeight
{
    public double Value { get; }
    public string Unit { get; }

    public ProductWeight(double value, string unit = "kg")
    {
        if (value < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be null or empty", nameof(unit));

        Value = Math.Round(value, 3);
        Unit = unit.ToLowerInvariant();
    }

    public ProductWeight ConvertTo(string targetUnit, double conversionFactor)
    {
        if (string.IsNullOrWhiteSpace(targetUnit))
            throw new ArgumentException("Target unit cannot be null or empty", nameof(targetUnit));

        if (conversionFactor <= 0)
            throw new ArgumentException("Conversion factor must be positive", nameof(conversionFactor));

        return new ProductWeight(Value * conversionFactor, targetUnit);
    }

    public override string ToString() => $"{Value} {Unit}";

    public static implicit operator double(ProductWeight weight) => weight.Value;
    public static implicit operator ProductWeight(double value) => new(value);
}