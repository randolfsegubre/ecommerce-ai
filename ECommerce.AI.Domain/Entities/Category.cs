using ECommerce.AI.Domain.Common;
using ECommerce.AI.Domain.Events.Category;

namespace ECommerce.AI.Domain.Entities;

/// <summary>
/// Category aggregate root representing product categories in the e-commerce system
/// </summary>
public class Category : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? ParentCategoryId { get; private set; }
    
    // Navigation properties
    public virtual Category? ParentCategory { get; private set; }
    public virtual ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private Category() { } // EF Constructor
#pragma warning restore CS8618

    public Category(string name, string description, string? imageUrl = null, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be null or empty", nameof(description));

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        ParentCategoryId = parentCategoryId;

        // Raise domain event
        RaiseDomainEvent(new CategoryCreated(Id, Name, ParentCategoryId));
    }

    public void UpdateDetails(string name, string description, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be null or empty", nameof(description));

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        MarkAsUpdated();
    }

    public void SetParentCategory(Guid? parentCategoryId)
    {
        // Prevent setting self as parent
        if (parentCategoryId == Id)
            throw new InvalidOperationException("A category cannot be its own parent");

        var oldParentId = ParentCategoryId;
        ParentCategoryId = parentCategoryId;
        MarkAsUpdated();

        // Raise domain event
        if (oldParentId != parentCategoryId)
        {
            RaiseDomainEvent(new CategoryParentChanged(Id, oldParentId, parentCategoryId));
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            MarkAsUpdated();
            RaiseDomainEvent(new CategoryStatusChanged(Id, true));
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            MarkAsUpdated();
            RaiseDomainEvent(new CategoryStatusChanged(Id, false));
        }
    }

    // Business logic methods
    public bool IsRootCategory() => ParentCategoryId == null;
    
    public bool HasSubCategories() => SubCategories.Any();
    
    public bool HasProducts() => Products.Any();
    
    public bool CanBeDeleted() => !HasSubCategories() && !HasProducts();
    
    public int GetTotalProductCount() => Products.Count + SubCategories.Sum(sc => sc.GetTotalProductCount());
    
    public IEnumerable<Category> GetAllAncestors()
    {
        var ancestors = new List<Category>();
        var current = ParentCategory;
        
        while (current != null)
        {
            ancestors.Add(current);
            current = current.ParentCategory;
        }
        
        return ancestors;
    }
    
    public IEnumerable<Category> GetAllDescendants()
    {
        var descendants = new List<Category>();
        
        foreach (var subCategory in SubCategories)
        {
            descendants.Add(subCategory);
            descendants.AddRange(subCategory.GetAllDescendants());
        }
        
        return descendants;
    }
    
    public bool IsAncestorOf(Category category)
    {
        return category.GetAllAncestors().Contains(this);
    }
    
    public bool IsDescendantOf(Category category)
    {
        return GetAllAncestors().Contains(category);
    }
    
    public int GetDepthLevel()
    {
        return GetAllAncestors().Count();
    }
}