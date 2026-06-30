using ECommerce.AI.Domain.Entities;

namespace ECommerce.AI.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default);
    Task<Category?> GetWithProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
}