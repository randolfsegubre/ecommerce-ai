using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.Interfaces;
using ECommerce.AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.AI.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsFeatured && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= p.MinStockLevel && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && 
                       (p.Name.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm) ||
                        p.Brand != null && p.Brand.Contains(searchTerm) ||
                        p.Model != null && p.Model.Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsWithImagesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}