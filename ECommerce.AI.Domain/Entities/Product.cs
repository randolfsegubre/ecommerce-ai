using ECommerce.AI.Domain.Common;
using ECommerce.AI.Domain.ValueObjects;
using ECommerce.AI.Domain.Events.Product;

namespace ECommerce.AI.Domain.Entities;

/// <summary>
/// Product aggregate root representing a sellable item in the e-commerce system
/// </summary>
public class Product : AggregateRoot
{
    // Private fields for value objects
    private Money _price;
    private Money? _comparePrice;
    private ProductSKU _sku;
    private ProductWeight _weight;
    private ProductDimensions? _dimensions;

    // Simple properties
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int StockQuantity { get; private set; }
    public int MinStockLevel { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; } = false;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public Guid CategoryId { get; private set; }
    
    // Value object properties
    public ProductSKU SKU => _sku;
    public Money Price => _price;
    public Money? ComparePrice => _comparePrice;
    public ProductWeight Weight => _weight;
    public ProductDimensions? Dimensions => _dimensions;
    
    // Navigation properties
    public virtual Category Category { get; private set; } = null!;
    public virtual ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
    public virtual ICollection<ProductSpecification> Specifications { get; private set; } = new List<ProductSpecification>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private Product() // EF Constructor
    {
        // EF Core will populate these fields
    }
#pragma warning restore CS8618

    public Product(string name, string description, ProductSKU sku, Money price, 
                  int stockQuantity, int minStockLevel, ProductWeight weight, Guid categoryId,
                  Money? comparePrice = null, ProductDimensions? dimensions = null, 
                  string? brand = null, string? model = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        _sku = sku ?? throw new ArgumentNullException(nameof(sku));
        SetPrice(price, comparePrice);
        SetStock(stockQuantity, minStockLevel);
        _weight = weight ?? throw new ArgumentNullException(nameof(weight));
        _dimensions = dimensions;
        Brand = brand;
        Model = model;
        CategoryId = categoryId;

        // Raise domain event
        RaiseDomainEvent(new ProductCreated(Id, Name, _sku.Value, _price.Amount, CategoryId));
    }

    public void UpdateBasicInfo(string name, string description, string? brand = null, string? model = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be null or empty", nameof(description));

        Name = name;
        Description = description;
        Brand = brand;
        Model = model;
        MarkAsUpdated();
    }

    public void SetPrice(Money price, Money? comparePrice = null)
    {
        if (price == null)
            throw new ArgumentNullException(nameof(price));

        if (comparePrice != null && comparePrice.Currency != price.Currency)
            throw new ArgumentException("Compare price must be in the same currency as the main price");

        var oldPrice = _price?.Amount ?? 0;
        var oldComparePrice = _comparePrice?.Amount;

        _price = price;
        _comparePrice = comparePrice;
        
        MarkAsUpdated();

        // Raise domain event for price change
        if (oldPrice != price.Amount || oldComparePrice != comparePrice?.Amount)
        {
            RaiseDomainEvent(new ProductPriceChanged(Id, oldPrice, price.Amount, oldComparePrice, comparePrice?.Amount));
        }
    }

    public void SetStock(int quantity, int minLevel = 0)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        if (minLevel < 0)
            throw new ArgumentException("Min stock level cannot be negative", nameof(minLevel));
        
        var oldQuantity = StockQuantity;
        var wasLowStock = IsLowStock();
        var wasOutOfStock = !IsInStock();

        StockQuantity = quantity;
        MinStockLevel = minLevel;
        MarkAsUpdated();

        // Raise domain events for stock changes
        var isNowLowStock = IsLowStock();
        var isNowOutOfStock = !IsInStock();

        if (oldQuantity != quantity)
        {
            RaiseDomainEvent(new ProductStockChanged(Id, oldQuantity, quantity, isNowLowStock, isNowOutOfStock));
        }

        // Raise specific alert events
        if (!wasLowStock && isNowLowStock && IsInStock())
        {
            RaiseDomainEvent(new ProductLowStockAlert(Id, Name, quantity, minLevel));
        }

        if (!wasOutOfStock && isNowOutOfStock)
        {
            RaiseDomainEvent(new ProductOutOfStock(Id, Name));
        }
    }

    public void UpdatePhysicalProperties(ProductWeight weight, ProductDimensions? dimensions = null)
    {
        _weight = weight ?? throw new ArgumentNullException(nameof(weight));
        _dimensions = dimensions;
        MarkAsUpdated();
    }

    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

        CategoryId = categoryId;
        MarkAsUpdated();
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            MarkAsUpdated();
            RaiseDomainEvent(new ProductStatusChanged(Id, true));
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            MarkAsUpdated();
            RaiseDomainEvent(new ProductStatusChanged(Id, false));
        }
    }

    public void SetFeatured(bool isFeatured)
    {
        if (IsFeatured != isFeatured)
        {
            IsFeatured = isFeatured;
            MarkAsUpdated();
        }
    }

    public void UpdateSKU(ProductSKU newSku)
    {
        _sku = newSku ?? throw new ArgumentNullException(nameof(newSku));
        MarkAsUpdated();
    }

    // Business logic methods
    public bool IsInStock() => StockQuantity > 0;
    public bool IsLowStock() => StockQuantity > 0 && StockQuantity <= MinStockLevel;
    public bool HasDiscount() => _comparePrice != null && _comparePrice.IsGreaterThan(_price);
    public decimal DiscountPercentage() => HasDiscount() ? 
        Math.Round(((_comparePrice!.Amount - _price.Amount) / _comparePrice.Amount) * 100, 2) : 0;
    
    public Money GetDiscountAmount() => HasDiscount() ? 
        _comparePrice!.Subtract(_price) : Money.Zero(_price.Currency);

    public bool RequiresShipping() => _weight.Value > 0;

    public bool FitsInBox(ProductDimensions boxDimensions)
    {
        if (_dimensions == null || boxDimensions == null)
            return true; // Assume it fits if dimensions are unknown

        return _dimensions.Length <= boxDimensions.Length &&
               _dimensions.Width <= boxDimensions.Width &&
               _dimensions.Height <= boxDimensions.Height;
    }
}