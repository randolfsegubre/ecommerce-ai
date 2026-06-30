using ECommerce.AI.Domain.Common;
using ECommerce.AI.Domain.ValueObjects;

namespace ECommerce.AI.Domain.Entities;

/// <summary>
/// Product image entity representing images associated with a product
/// </summary>
public class ProductImage : BaseEntity
{
    private ImageUrl _imageUrl;
    
    public string ImageUrl => _imageUrl.Value;
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public Guid ProductId { get; private set; }
    
    // Navigation properties
    public virtual Product Product { get; private set; } = null!;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private ProductImage() { } // EF Constructor
#pragma warning restore CS8618

    public ProductImage(string imageUrl, Guid productId, string? altText = null, 
                       int sortOrder = 0, bool isPrimary = false)
    {
        _imageUrl = new ImageUrl(imageUrl);
        ProductId = productId;
        AltText = altText;
        SortOrder = Math.Max(0, sortOrder);
        IsPrimary = isPrimary;
    }

    public void UpdateImage(string imageUrl, string? altText = null)
    {
        _imageUrl = new ImageUrl(imageUrl);
        AltText = altText;
        MarkAsUpdated();
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentException("Sort order cannot be negative", nameof(sortOrder));

        SortOrder = sortOrder;
        MarkAsUpdated();
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        MarkAsUpdated();
    }

    public void RemovePrimary()
    {
        IsPrimary = false;
        MarkAsUpdated();
    }

    public void UpdateAltText(string? altText)
    {
        AltText = altText;
        MarkAsUpdated();
    }

    // Business logic methods
    public bool IsValidImage() => !string.IsNullOrWhiteSpace(_imageUrl.Value);
    
    public bool HasAltText() => !string.IsNullOrWhiteSpace(AltText);
}