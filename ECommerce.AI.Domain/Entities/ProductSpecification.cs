using ECommerce.AI.Domain.Common;
using ECommerce.AI.Domain.ValueObjects;

namespace ECommerce.AI.Domain.Entities;

/// <summary>
/// Product specification entity representing key-value specifications for a product
/// </summary>
public class ProductSpecification : BaseEntity
{
    private SpecificationName _name;
    private SpecificationValue _value;
    
    public string Name => _name.Value;
    public string Value => _value.Value;
    public int SortOrder { get; private set; }
    public Guid ProductId { get; private set; }
    
    // Navigation properties
    public virtual Product Product { get; private set; } = null!;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private ProductSpecification() { } // EF Constructor
#pragma warning restore CS8618

    public ProductSpecification(string name, string value, Guid productId, int sortOrder = 0)
    {
        _name = new SpecificationName(name);
        _value = new SpecificationValue(value);
        ProductId = productId;
        SortOrder = Math.Max(0, sortOrder);
    }

    public void UpdateSpecification(string name, string value)
    {
        _name = new SpecificationName(name);
        _value = new SpecificationValue(value);
        MarkAsUpdated();
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentException("Sort order cannot be negative", nameof(sortOrder));

        SortOrder = sortOrder;
        MarkAsUpdated();
    }

    public void UpdateName(string name)
    {
        _name = new SpecificationName(name);
        MarkAsUpdated();
    }

    public void UpdateValue(string value)
    {
        _value = new SpecificationValue(value);
        MarkAsUpdated();
    }

    // Business logic methods
    public bool IsNumericValue() => _value.IsNumeric();
    
    public decimal? GetNumericValue() => _value.GetNumericValue();
    
    public bool IsBooleanValue() => _value.IsBoolean();
    
    public bool? GetBooleanValue() => _value.GetBooleanValue();
    
    public bool IsColorValue() => _value.IsColor();
    
    public bool IsUrlValue() => _value.IsUrl();
}