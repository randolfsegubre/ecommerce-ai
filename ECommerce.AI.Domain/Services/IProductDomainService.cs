using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.ValueObjects;

namespace ECommerce.AI.Domain.Services;

/// <summary>
/// Domain service for product-related business logic
/// </summary>
public interface IProductDomainService
{
    /// <summary>
    /// Validates if a SKU is unique across all products
    /// </summary>
    Task<bool> IsSkuUniqueAsync(ProductSKU sku, Guid? excludeProductId = null);

    /// <summary>
    /// Calculates discount price based on business rules
    /// </summary>
    Money CalculateDiscountPrice(Money originalPrice, decimal discountPercentage);

    /// <summary>
    /// Determines if a product should trigger low stock alerts
    /// </summary>
    bool ShouldTriggerLowStockAlert(Product product);

    /// <summary>
    /// Calculates shipping weight including packaging
    /// </summary>
    ProductWeight CalculateShippingWeight(ProductWeight productWeight);

    /// <summary>
    /// Validates business rules for product creation
    /// </summary>
    Task<ProductValidationResult> ValidateProductForCreationAsync(Product product);

    /// <summary>
    /// Validates business rules for product updates
    /// </summary>
    Task<ProductValidationResult> ValidateProductForUpdateAsync(Product product);
}

/// <summary>
/// Result of product validation
/// </summary>
public record ProductValidationResult(
    bool IsValid,
    List<string> Errors
)
{
    public static ProductValidationResult Success() => new(true, new List<string>());
    
    public static ProductValidationResult Failure(params string[] errors) => 
        new(false, errors.ToList());

    public static ProductValidationResult Failure(List<string> errors) => 
        new(false, errors);
}