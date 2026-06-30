using System.Linq.Expressions;
using ECommerce.AI.Domain.Entities;

namespace ECommerce.AI.Domain.Specifications.Product;

/// <summary>
/// Specification for active products
/// </summary>
public class ActiveProductSpecification : Specification<Entities.Product>
{
    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.IsActive;
    }
}

/// <summary>
/// Specification for featured products
/// </summary>
public class FeaturedProductSpecification : Specification<Entities.Product>
{
    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.IsFeatured;
    }
}

/// <summary>
/// Specification for products in stock
/// </summary>
public class InStockProductSpecification : Specification<Entities.Product>
{
    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.StockQuantity > 0;
    }
}

/// <summary>
/// Specification for low stock products
/// </summary>
public class LowStockProductSpecification : Specification<Entities.Product>
{
    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.StockQuantity > 0 && product.StockQuantity <= product.MinStockLevel;
    }
}

/// <summary>
/// Specification for out of stock products
/// </summary>
public class OutOfStockProductSpecification : Specification<Entities.Product>
{
    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.StockQuantity <= 0;
    }
}

/// <summary>
/// Specification for products in a specific category
/// </summary>
public class ProductByCategorySpecification : Specification<Entities.Product>
{
    private readonly Guid _categoryId;

    public ProductByCategorySpecification(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.CategoryId == _categoryId;
    }
}

/// <summary>
/// Specification for products with discount
/// </summary>
public class ProductWithDiscountSpecification : Specification<Entities.Product>
{
    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.ComparePrice != null && product.ComparePrice.IsGreaterThan(product.Price);
    }
}

/// <summary>
/// Specification for products by name search
/// </summary>
public class ProductNameSearchSpecification : Specification<Entities.Product>
{
    private readonly string _searchTerm;

    public ProductNameSearchSpecification(string searchTerm)
    {
        _searchTerm = searchTerm?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(searchTerm));
    }

    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.Name.ToLowerInvariant().Contains(_searchTerm) || 
                         product.Description.ToLowerInvariant().Contains(_searchTerm) ||
                         product.SKU.Value.ToLowerInvariant().Contains(_searchTerm);
    }
}

/// <summary>
/// Specification for products by price range
/// </summary>
public class ProductPriceRangeSpecification : Specification<Entities.Product>
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public ProductPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        if (minPrice < 0) throw new ArgumentException("Min price cannot be negative", nameof(minPrice));
        if (maxPrice < minPrice) throw new ArgumentException("Max price cannot be less than min price", nameof(maxPrice));
        
        _minPrice = minPrice;
        _maxPrice = maxPrice;
    }

    public override Expression<Func<Entities.Product, bool>> ToExpression()
    {
        return product => product.Price >= _minPrice && product.Price <= _maxPrice;
    }
}