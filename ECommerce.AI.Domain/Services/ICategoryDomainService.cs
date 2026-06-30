using ECommerce.AI.Domain.Entities;

namespace ECommerce.AI.Domain.Services;

/// <summary>
/// Domain service for category-related business logic
/// </summary>
public interface ICategoryDomainService
{
    /// <summary>
    /// Validates if a category can be moved to a new parent without creating cycles
    /// </summary>
    Task<bool> CanMoveToParentAsync(Guid categoryId, Guid? newParentId);

    /// <summary>
    /// Gets the complete category hierarchy path for a category
    /// </summary>
    Task<List<Category>> GetCategoryHierarchyAsync(Guid categoryId);

    /// <summary>
    /// Validates if a category can be safely deleted (no products or subcategories)
    /// </summary>
    Task<CategoryDeletionResult> ValidateForDeletionAsync(Guid categoryId);

    /// <summary>
    /// Gets all descendant categories of a given category
    /// </summary>
    Task<List<Category>> GetAllDescendantsAsync(Guid categoryId);
}

/// <summary>
/// Result of category deletion validation
/// </summary>
public record CategoryDeletionResult(
    bool CanDelete,
    string Reason,
    int ProductCount,
    int SubcategoryCount
)
{
    public static CategoryDeletionResult CannotDelete(string reason, int productCount = 0, int subcategoryCount = 0) =>
        new(false, reason, productCount, subcategoryCount);

    public static CategoryDeletionResult Success() =>
        new(true, "Category can be safely deleted", 0, 0);
}