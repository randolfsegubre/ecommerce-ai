# Infrastructure Layer Guide — `ECommerce.AI.Infrastructure`

This layer implements data persistence via EF Core. It depends only on the Domain layer (through its interfaces). No business logic lives here.

---

## `ECommerceDbContext` — `Data/ECommerceDbContext.cs`

**Inherits:** `DbContext`

### DbSets

| DbSet | Entity |
|---|---|
| `Products` | `Product` |
| `Categories` | `Category` |
| `ProductImages` | `ProductImage` |
| `ProductSpecifications` | `ProductSpecification` |

---

## EF Core Configuration (`OnModelCreating`)

### `Category`

| Config | Detail |
|---|---|
| PK | `Id` |
| `Name` | `max 200`, required |
| `Description` | `max 1000`, required |
| `ImageUrl` | `max 500`, optional |
| Index | `Name` |
| Self-reference | `HasOne(c => c.ParentCategory).WithMany(c => c.SubCategories).HasForeignKey(c => c.ParentCategoryId).OnDelete(DeleteBehavior.Restrict)` |
| Soft-delete filter | `!IsDeleted` |

### `Product`

| Config | Detail |
|---|---|
| PK | `Id` |
| `Name` | `max 300`, required |
| `Description` | `max 2000`, required |
| `SKU` | `max 50`, required, **unique index** |
| `Price`, `ComparePrice` | `decimal(18,2)` |
| Indexes | SKU (unique), Name, `{CategoryId, IsActive}` composite |
| FK | `HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict)` |
| Soft-delete filter | `!IsDeleted` |

### `ProductImage`

| Config | Detail |
|---|---|
| `ImageUrl` | `max 500` |
| `AltText` | `max 200`, optional |
| Index | `{ProductId, SortOrder}` composite |
| FK | `HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade)` |
| Soft-delete filter | `!IsDeleted` |

### `ProductSpecification`

| Config | Detail |
|---|---|
| `Name` | `max 100` |
| `Value` | `max 500` |
| Index | `{ProductId, SortOrder}` composite |
| FK | `HasOne(s => s.Product).WithMany(p => p.Specifications).HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Cascade)` |
| Soft-delete filter | `!IsDeleted` |

---

## `SaveChangesAsync` Override

Automatically calls `entity.MarkAsUpdated()` on any modified `BaseEntity` before saving:

```csharp
foreach (var entry in ChangeTracker.Entries<BaseEntity>())
{
    if (entry.State == EntityState.Modified)
        entry.Entity.MarkAsUpdated();
}
```

---

## Repository Implementations

### `Repository<T>` — `Repositories/Repository.cs`

Base generic implementation of `IRepository<T>`. All methods operate on `_dbSet` (EF `DbSet<T>`).

**Soft-delete:** `DeleteAsync` and `DeleteRangeAsync` call `entity.SoftDelete()` — they do **not** issue a SQL `DELETE`. The entity is marked `IsDeleted = true` and updated.

**Update:** Calls `_dbSet.Update(entity)` — EF tracks all properties as modified.

---

### `CategoryRepository` — `Repositories/CategoryRepository.cs`

Extends `Repository<Category>`. Implements `ICategoryRepository`.

| Method | EF Query Detail |
|---|---|
| `GetRootCategoriesAsync()` | `.Include(SubCategories).Where(ParentCategoryId == null && IsActive)` |
| `GetSubCategoriesAsync(parentId)` | `.Include(SubCategories).Where(ParentCategoryId == parentId && IsActive)` |
| `GetWithProductsAsync(id)` | `.Include(c => c.Products.Where(p => p.IsActive)).ThenInclude(p => p.Images).FirstOrDefault(id)` |
| `GetActiveCategoriesAsync()` | `.Include(SubCategories.Where(s => s.IsActive)).Where(IsActive)` |
| `GetByIdAsync(id)` *(override)* | `.Include(ParentCategory).Include(SubCategories).FirstOrDefault(id)` — loads full parent+children tree |

---

### `ProductRepository` — `Repositories/ProductRepository.cs`

Extends `Repository<Product>`. Implements `IProductRepository`.

| Method | EF Query Detail |
|---|---|
| `GetByCategoryAsync(categoryId)` | `.Include(Images).Include(Specifications).Where(CategoryId == id && IsActive)` |
| `GetFeaturedProductsAsync()` | `.Include(Images).Include(Category).Where(IsFeatured && IsActive)` |
| `GetLowStockProductsAsync()` | `.Where(StockQuantity > 0 && StockQuantity <= MinStockLevel && IsActive)` |
| `SearchProductsAsync(term)` | `.Include(Images).Where(Name/Description/SKU contains term, case-insensitive)` |
| `GetBySkuAsync(sku)` | `.Include(Category).FirstOrDefault(SKU == sku)` |
| `GetProductsWithImagesAsync()` | `.Include(Images).Include(Category).Where(IsActive)` |

---

### `UnitOfWork` — `Repositories/UnitOfWork.cs`

Implements `IUnitOfWork`. Wraps `ECommerceDbContext` and exposes repositories.

```csharp
IProductRepository Products { get; }       // lazily instantiated
ICategoryRepository Categories { get; }    // lazily instantiated

IRepository<T> Repository<T>()             // generic repository for any BaseEntity
Task<int> SaveChangesAsync(CancellationToken ct)  // delegates to context

Task BeginTransactionAsync(ct)    // context.Database.BeginTransactionAsync()
Task CommitTransactionAsync(ct)   // context.Database.CommitTransactionAsync()
Task RollbackTransactionAsync(ct) // context.Database.RollbackTransactionAsync()
```

**Usage pattern in handlers:**

```csharp
// Read
var product = await _unitOfWork.Products.GetByIdAsync(id, ct);

// Write
await _unitOfWork.Products.AddAsync(newProduct, ct);
await _unitOfWork.SaveChangesAsync(ct);  // single unit of work boundary

// Transaction
await _unitOfWork.BeginTransactionAsync(ct);
try {
    await _unitOfWork.Products.AddAsync(product, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    await _unitOfWork.CommitTransactionAsync(ct);
} catch {
    await _unitOfWork.RollbackTransactionAsync(ct);
}
```

---

## Running Migrations

```powershell
# From solution root (E-Commerse.AI.API/)

dotnet ef migrations add <Name> \
  --project ECommerce.AI.Infrastructure \
  --startup-project E-Commerse.AI.API

dotnet ef database update \
  --project ECommerce.AI.Infrastructure \
  --startup-project E-Commerse.AI.API
```
