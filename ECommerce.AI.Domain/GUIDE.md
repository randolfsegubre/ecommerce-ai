# Domain Layer Guide — `ECommerce.AI.Domain`

This is the innermost layer with **zero dependencies** on any other project. All business rules, invariants, and domain logic live here.

---

## Dependency Rule

```
Domain ← (no dependencies)
     ↑
Application
     ↑
Infrastructure / API
```

---

## Base Classes

### `BaseEntity` — `Common/BaseEntity.cs`

Abstract base for every entity.

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Auto `Guid.NewGuid()` |
| `CreatedAt` | `DateTime` | UTC, set at construction |
| `UpdatedAt` | `DateTime?` | Null until first update |
| `CreatedBy` | `string?` | Audit — who created |
| `UpdatedBy` | `string?` | Audit — who last updated |
| `IsDeleted` | `bool` | Soft-delete flag |

**Methods:**
- `MarkAsUpdated(string? updatedBy = null)` — sets `UpdatedAt = DateTime.UtcNow` and optionally `UpdatedBy`
- `SoftDelete()` — sets `IsDeleted = true`, then calls `MarkAsUpdated()`

EF applies global query filter `!IsDeleted` on all entities.

---

### `AggregateRoot` — `Common/AggregateRoot.cs`

Extends `BaseEntity`. Used for entities that are the root of a consistency boundary. Holds domain events.

| Member | Type | Description |
|---|---|---|
| `_domainEvents` | `List<IDomainEvent>` | Private backing list |
| `DomainEvents` | `IReadOnlyCollection<IDomainEvent>` | Read-only accessor |
| `HasDomainEvents` | `bool` | True if any events are queued |

**Methods:**
- `RaiseDomainEvent(IDomainEvent)` — `protected` — appends to `_domainEvents`
- `ClearDomainEvents()` — `public` — call after dispatching events to a bus

**Aggregates in this solution:** `Category`, `Product`

---

## Entities

### `Category` — `Entities/Category.cs`

Aggregate root. Represents a hierarchical product category (unlimited depth).

#### Properties

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | Required, max 200 chars |
| `Description` | `string` | Required, max 1000 chars |
| `ImageUrl` | `string?` | Optional |
| `IsActive` | `bool` | Default `true` |
| `ParentCategoryId` | `Guid?` | `null` = root category |
| `ParentCategory` | `Category?` | Navigation — loaded by `CategoryRepository.GetByIdAsync` |
| `SubCategories` | `ICollection<Category>` | Navigation |
| `Products` | `ICollection<Product>` | Navigation |

#### Constructor
```csharp
Category(string name, string description, string? imageUrl, Guid? parentCategoryId)
```
- Validates name and description are non-null/non-empty (throws `ArgumentException`)
- Raises `CategoryCreated` domain event

#### Domain Methods

| Method | Signature | Raises Event? |
|---|---|---|
| `UpdateDetails` | `(string name, string description, string? imageUrl)` | No |
| `SetParentCategory` | `(Guid? parentCategoryId)` | `CategoryParentChanged` (only if value changes) |
| `Activate` | `()` | `CategoryStatusChanged(true)` |
| `Deactivate` | `()` | `CategoryStatusChanged(false)` |

#### Business Logic Methods

| Method | Returns | Description |
|---|---|---|
| `IsRootCategory()` | `bool` | True if `ParentCategoryId == null` |
| `HasSubCategories()` | `bool` | True if `SubCategories.Any()` |
| `HasProducts()` | `bool` | True if `Products.Any()` |
| `CanBeDeleted()` | `bool` | `!HasSubCategories() && !HasProducts()` |
| `GetTotalProductCount()` | `int` | Recursive count including all descendants |
| `GetAllAncestors()` | `IEnumerable<Category>` | Walks `ParentCategory` chain upward |
| `GetAllDescendants()` | `IEnumerable<Category>` | Recursive via `SubCategories` |
| `IsAncestorOf(Category other)` | `bool` | True if `this` appears in `other`'s ancestor chain |
| `IsDescendantOf(Category other)` | `bool` | True if `other` appears in `this`'s ancestor chain |
| `GetDepthLevel()` | `int` | 0 = root; 1 = direct child of root; etc. |

---

### `Product` — `Entities/Product.cs`

Aggregate root. Represents a saleable product with pricing, stock, and physical properties.

#### Properties

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | Required, max 300 chars |
| `Description` | `string` | Required, max 2000 chars |
| `StockQuantity` | `int` | Current inventory level |
| `MinStockLevel` | `int` | Triggers low-stock alert |
| `IsActive` | `bool` | Default `true` |
| `IsFeatured` | `bool` | Default `false` — shown in featured lists |
| `Brand` | `string?` | |
| `Model` | `string?` | |
| `CategoryId` | `Guid` | FK → `Category` |
| `SKU` | `ProductSKU` | Value object — backed by private `_sku` |
| `Price` | `Money` | Value object — backed by private `_price` |
| `ComparePrice` | `Money?` | Strike-through price for discounts |
| `Weight` | `ProductWeight` | Value object |
| `Dimensions` | `ProductDimensions?` | Value object |
| `Category` | `Category` | Navigation |
| `Images` | `ICollection<ProductImage>` | Navigation |
| `Specifications` | `ICollection<ProductSpecification>` | Navigation |

#### Constructor
```csharp
Product(string name, string description, ProductSKU sku, Money price,
        int stockQuantity, int minStockLevel, ProductWeight weight,
        Guid categoryId, Money? comparePrice, ProductDimensions? dimensions,
        string? brand, string? model)
```
Raises `ProductCreated` domain event.

#### Domain Methods

| Method | Signature | Raises Event |
|---|---|---|
| `UpdateBasicInfo` | `(string name, string description, string? brand, string? model)` | No |
| `UpdateSKU` | `(ProductSKU newSku)` | No |
| `SetPrice` | `(Money price, Money? comparePrice)` | `ProductPriceChanged` (if price changes) |
| `SetStock` | `(int quantity, int minLevel)` | `ProductStockChanged` + `ProductLowStockAlert` or `ProductOutOfStock` |
| `UpdatePhysicalProperties` | `(ProductWeight weight, ProductDimensions? dimensions)` | No |
| `SetCategory` | `(Guid categoryId)` | No |
| `Activate` | `()` | `ProductStatusChanged(true)` |
| `Deactivate` | `()` | `ProductStatusChanged(false)` |
| `SetFeatured` | `(bool isFeatured)` | No |

#### Business Logic Methods

| Method | Returns | Description |
|---|---|---|
| `IsInStock()` | `bool` | `StockQuantity > 0` |
| `IsLowStock()` | `bool` | `StockQuantity > 0 && StockQuantity <= MinStockLevel` |
| `HasDiscount()` | `bool` | `ComparePrice != null && ComparePrice > Price` |
| `DiscountPercentage()` | `decimal` | `(ComparePrice - Price) / ComparePrice * 100` |
| `GetDiscountAmount()` | `Money` | `ComparePrice - Price` |
| `RequiresShipping()` | `bool` | `Weight.Value > 0` |
| `FitsInBox(ProductDimensions box)` | `bool` | True if all dimensions fit within box |

---

### `ProductImage` — `Entities/ProductImage.cs`

Extends `BaseEntity` (not an aggregate root — managed through `Product`).

| Property | Type | Notes |
|---|---|---|
| `ImageUrl` | `string` | Backed by `ImageUrl` value object |
| `AltText` | `string?` | For accessibility/SEO |
| `SortOrder` | `int` | Min 0; lower = shown first |
| `IsPrimary` | `bool` | Only one image per product should be primary |
| `ProductId` | `Guid` | FK → `Product` |

**Methods:** `UpdateImage(string url, string? altText)`, `SetSortOrder(int)`, `SetAsPrimary()`, `RemovePrimary()`, `UpdateAltText(string?)`  
**Queries:** `IsValidImage()` → delegates to `ImageUrl` value object, `HasAltText()` → `AltText != null`

---

### `ProductSpecification` — `Entities/ProductSpecification.cs`

Key-value pair describing a product attribute (e.g., `Color: Midnight Black`).

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | Backed by `SpecificationName` value object |
| `Value` | `string` | Backed by `SpecificationValue` value object |
| `SortOrder` | `int` | |
| `ProductId` | `Guid` | FK → `Product` |

**Methods:** `UpdateSpecification(name, value)`, `SetSortOrder(int)`, `UpdateName(string)`, `UpdateValue(string)`  
**Value queries (delegate to `SpecificationValue`):** `IsNumericValue()`, `GetNumericValue()`, `IsBooleanValue()`, `GetBooleanValue()`, `IsColorValue()`, `IsUrlValue()`

---

## Value Objects

Implemented as C# `record` types — **immutable** with **structural equality** (two instances with same data are equal).

### `Money` — `ValueObjects/Money.cs`

Represents a monetary amount with currency.

| Property | Type | Invariant |
|---|---|---|
| `Amount` | `decimal` | ≥ 0, rounded to 2 decimal places |
| `Currency` | `string` | Non-empty, uppercased, default `"USD"` |

**Operations:**
- `Add(Money other)` — throws if currencies differ
- `Subtract(Money other)` — throws if currencies differ or result < 0
- `Multiply(decimal factor)` — returns new `Money`
- `IsGreaterThan(Money)` / `IsLessThan(Money)` — throws if currencies differ
- `Zero(string currency)` — static factory: `new Money(0, currency)`
- Implicit conversions: `decimal → Money` (uses "USD"), `Money → decimal`

---

### `ProductSKU` — `ValueObjects/ProductSKU.cs`

Stock Keeping Unit identifier.

| Property | Invariant |
|---|---|
| `Value` | Non-empty, uppercased/trimmed, matches regex `^[A-Z0-9\-_]{3,50}$` |

Implicit conversions: `string ↔ ProductSKU`

---

### `ProductWeight` — `ValueObjects/ProductWeight.cs`

| Property | Type | Invariant |
|---|---|---|
| `Value` | `double` | ≥ 0, rounded to 3 decimal places |
| `Unit` | `string` | Non-empty, lowercased, default `"kg"` |

**Methods:** `ConvertTo(string targetUnit, double conversionFactor)` → returns new `ProductWeight`  
Implicit conversions: `double ↔ ProductWeight`

---

### `ProductDimensions` — `ValueObjects/ProductDimensions.cs`

| Property | Type | Invariant |
|---|---|---|
| `Length` | `double` | > 0, rounded to 2 dp |
| `Width` | `double` | > 0, rounded to 2 dp |
| `Height` | `double` | > 0, rounded to 2 dp |
| `Unit` | `string` | Non-empty, lowercased, default `"cm"` |

**Computed:** `Volume` → `Length * Width * Height`  
**Methods:** `ConvertTo(string, double)`, `FromString(string, string)` — static, parses `"L x W x H"` format

---

### `ImageUrl` — `ValueObjects/ImageUrl.cs`

| Property | Invariant |
|---|---|
| `Value` | Valid HTTP/HTTPS URL, OR `data:image/...` base64 string |

**Methods:** `IsDataUrl()`, `IsHttpUrl()`, `GetFileExtension()` (from URL path)

---

### `SpecificationName` & `SpecificationValue` — `ValueObjects/SpecificationValueObjects.cs`

| Value Object | Invariants |
|---|---|
| `SpecificationName` | Non-empty, trimmed, max 100 chars |
| `SpecificationValue` | Non-empty, trimmed, max 500 chars |

`SpecificationValue` extra methods: `IsNumeric()`, `GetNumericValue()`, `IsBoolean()`, `GetBooleanValue()`, `IsColor()` (hex + named colors), `IsUrl()`

---

## Domain Events

All events are immutable `record` types extending `DomainEvent`.

### `DomainEvent` base & `IDomainEvent` interface

```csharp
interface IDomainEvent {
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}

abstract record DomainEvent : IDomainEvent {
    DateTime OccurredOn = DateTime.UtcNow
    Guid EventId = Guid.NewGuid()
}
```

### Category Events

| Event | When Raised | Properties |
|---|---|---|
| `CategoryCreated` | `Category` constructor | `CategoryId`, `Name`, `ParentCategoryId?` |
| `CategoryStatusChanged` | `Activate()` / `Deactivate()` | `CategoryId`, `IsActive` |
| `CategoryParentChanged` | `SetParentCategory()` (only when value changes) | `CategoryId`, `OldParentId?`, `NewParentId?` |

### Product Events

| Event | When Raised | Properties |
|---|---|---|
| `ProductCreated` | `Product` constructor | `ProductId`, `Name`, `SKU`, `Price`, `CategoryId` |
| `ProductPriceChanged` | `SetPrice()` when price actually changes | `ProductId`, `OldPrice`, `NewPrice`, `OldComparePrice?`, `NewComparePrice?` |
| `ProductStockChanged` | `SetStock()` when quantity changes | `ProductId`, `OldQuantity`, `NewQuantity`, `IsNowLowStock`, `IsNowOutOfStock` |
| `ProductLowStockAlert` | `SetStock()` when transitioning into low-stock (but not zero) | `ProductId`, `ProductName`, `CurrentStock`, `MinStockLevel` |
| `ProductOutOfStock` | `SetStock()` when quantity reaches 0 | `ProductId`, `ProductName` |
| `ProductStatusChanged` | `Activate()` / `Deactivate()` | `ProductId`, `IsActive` |

> **Note:** Domain events are collected in `AggregateRoot._domainEvents` and must be dispatched/cleared after `SaveChangesAsync`. An event bus integration is not yet wired in this solution.

---

## Interfaces

### `IRepository<T>` — `Interfaces/IRepository.cs`

Generic repository contract. All `T` must extend `BaseEntity`.

```csharp
Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
Task<T> AddAsync(T entity, CancellationToken ct = default)
Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
Task<T> UpdateAsync(T entity, CancellationToken ct = default)
Task DeleteAsync(T entity, CancellationToken ct = default)          // soft-delete
Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
```

### `ICategoryRepository` — extends `IRepository<Category>`

```csharp
Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken ct = default)
Task<Category?> GetWithProductsAsync(Guid categoryId, CancellationToken ct = default)
Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken ct = default)
```

### `IProductRepository` — extends `IRepository<Product>`

```csharp
Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken ct = default)
Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken ct = default)
Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken ct = default)
Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
Task<IEnumerable<Product>> GetProductsWithImagesAsync(CancellationToken ct = default)
```

### `IUnitOfWork`

```csharp
IProductRepository Products { get; }
ICategoryRepository Categories { get; }
IRepository<T> Repository<T>() where T : BaseEntity
Task<int> SaveChangesAsync(CancellationToken ct = default)
Task BeginTransactionAsync(CancellationToken ct = default)
Task CommitTransactionAsync(CancellationToken ct = default)
Task RollbackTransactionAsync(CancellationToken ct = default)
```

---

## Specifications Pattern

**Base:** `Specifications/Specification.cs`

`ISpecification<T>`: `Expression<Func<T, bool>> ToExpression()`, `bool IsSatisfiedBy(T entity)`

`Specification<T>` abstract class with combinators:
- `.And(ISpecification<T>)` → `AndSpecification<T>`
- `.Or(ISpecification<T>)` → `OrSpecification<T>`
- `.Not()` → `NotSpecification<T>`

### Product Specifications — `Specifications/Product/ProductSpecifications.cs`

| Class | Expression |
|---|---|
| `ActiveProductSpecification` | `p.IsActive` |
| `FeaturedProductSpecification` | `p.IsFeatured` |
| `InStockProductSpecification` | `p.StockQuantity > 0` |
| `LowStockProductSpecification` | `p.StockQuantity > 0 && p.StockQuantity <= p.MinStockLevel` |
| `OutOfStockProductSpecification` | `p.StockQuantity <= 0` |
| `ProductByCategorySpecification(Guid id)` | `p.CategoryId == id` |
| `ProductWithDiscountSpecification` | `p.ComparePrice != null && p.ComparePrice > p.Price` |
| `ProductNameSearchSpecification(string term)` | Name / Description / SKU contains term (case-insensitive) |
| `ProductPriceRangeSpecification(decimal min, decimal max)` | `p.Price >= min && p.Price <= max` |

**Composing specifications:**
```csharp
var spec = new ActiveProductSpecification()
    .And(new InStockProductSpecification())
    .And(new ProductByCategorySpecification(categoryId));

var results = products.Where(spec.IsSatisfiedBy);
// or pass spec.ToExpression() to EF
```

---

## Domain Service Interfaces (Not Yet Implemented)

### `ICategoryDomainService`

```csharp
Task<bool> CanMoveToParentAsync(Guid categoryId, Guid? newParentId)
Task<List<Category>> GetCategoryHierarchyAsync(Guid categoryId)
Task<CategoryDeletionResult> ValidateForDeletionAsync(Guid categoryId)
Task<List<Category>> GetAllDescendantsAsync(Guid categoryId)
```

`CategoryDeletionResult` record: `(bool CanDelete, string Reason, int ProductCount, int SubcategoryCount)`

### `IProductDomainService`

```csharp
Task<bool> IsSkuUniqueAsync(ProductSKU sku, Guid? excludeProductId)
Money CalculateDiscountPrice(Money originalPrice, decimal discountPercentage)
bool ShouldTriggerLowStockAlert(Product product)
ProductWeight CalculateShippingWeight(ProductWeight productWeight)
Task<ProductValidationResult> ValidateProductForCreationAsync(Product product)
Task<ProductValidationResult> ValidateProductForUpdateAsync(Product product)
```

`ProductValidationResult` record: `(bool IsValid, List<string> Errors)`
