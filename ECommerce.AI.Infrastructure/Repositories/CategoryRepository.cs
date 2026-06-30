using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.Interfaces;
using ECommerce.AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.AI.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetWithProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Products.Where(p => p.IsActive))
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}