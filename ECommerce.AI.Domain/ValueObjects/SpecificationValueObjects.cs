namespace ECommerce.AI.Domain.ValueObjects;

/// <summary>
/// Value object representing a specification name
/// </summary>
public record SpecificationName
{
    public string Value { get; }

    public SpecificationName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Specification name cannot be null or empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Specification name cannot exceed 100 characters", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(SpecificationName name) => name.Value;
    public static implicit operator SpecificationName(string value) => new(value);
}

/// <summary>
/// Value object representing a specification value
/// </summary>
public record SpecificationValue
{
    public string Value { get; }

    public SpecificationValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Specification value cannot be null or empty", nameof(value));

        if (value.Length > 500)
            throw new ArgumentException("Specification value cannot exceed 500 characters", nameof(value));

        Value = value.Trim();
    }

    public bool IsNumeric()
    {
        return decimal.TryParse(Value, out _);
    }

    public decimal? GetNumericValue()
    {
        return decimal.TryParse(Value, out var result) ? result : null;
    }

    public bool IsBoolean()
    {
        return bool.TryParse(Value, out _) || 
               Value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
               Value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
               Value.Equals("1") || Value.Equals("0");
    }

    public bool? GetBooleanValue()
    {
        if (bool.TryParse(Value, out var result))
            return result;

        return Value.ToLowerInvariant() switch
        {
            "yes" or "1" => true,
            "no" or "0" => false,
            _ => null
        };
    }

    public bool IsColor()
    {
        // Check for hex color codes
        if (Value.StartsWith('#') && (Value.Length == 7 || Value.Length == 4))
        {
            return Value.Skip(1).All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }

        // Check for common color names
        var colorNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "red", "blue", "green", "yellow", "black", "white", "gray", "grey",
            "pink", "purple", "orange", "brown", "cyan", "magenta", "lime",
            "silver", "gold", "navy", "maroon", "olive", "teal", "aqua"
        };

        return colorNames.Contains(Value);
    }

    public bool IsUrl()
    {
        return Uri.TryCreate(Value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public override string ToString() => Value;

    public static implicit operator string(SpecificationValue value) => value.Value;
    public static implicit operator SpecificationValue(string value) => new(value);
}