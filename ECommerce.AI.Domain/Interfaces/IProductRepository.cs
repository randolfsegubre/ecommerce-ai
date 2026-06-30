using ECommerce.AI.Domain.Entities;

namespace ECommerce.AI.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsWithImagesAsync(CancellationToken cancellationToken = default);
}