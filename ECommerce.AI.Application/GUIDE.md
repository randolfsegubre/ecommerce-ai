# Application Layer Guide — `ECommerce.AI.Application`

This layer holds the use-case logic: DTOs, MediatR command/query handlers, FluentValidation validators, and AutoMapper mappings. Depends only on the Domain layer.

---

## What Lives Here

| Folder | Contents |
|---|---|
| `DTOs/` | Response data contracts (read models) |
| `DTOs/Commands/` | Request data contracts (write models) + FluentValidation validators |
| `Handlers/Products/` | MediatR command and query handlers |
| `Mappings/MappingProfile.cs` | AutoMapper source → destination mappings |

---

## DTOs

### `ProductDto` — response

| Property | Type | Source |
|---|---|---|
| `Id` | `Guid` | `Product.Id` |
| `Name` | `string` | |
| `Description` | `string` | |
| `SKU` | `string` | `Product.SKU.Value` (unwrapped from value object) |
| `Price` | `decimal` | `Product.Price.Amount` |
| `ComparePrice` | `decimal?` | `Product.ComparePrice?.Amount` |
| `StockQuantity` | `int` | |
| `MinStockLevel` | `int` | |
| `IsActive` | `bool` | |
| `IsFeatured` | `bool` | |
| `Weight` | `double` | `Product.Weight.Value` |
| `Dimensions` | `string?` | `Product.Dimensions.ToString()` → `"L x W x H cm"` |
| `Brand` | `string?` | |
| `Model` | `string?` | |
| `CategoryId` | `Guid` | |
| `Category` | `CategoryDto?` | Navigation — loaded when include is used |
| `Images` | `List<ProductImageDto>` | |
| `Specifications` | `List<ProductSpecificationDto>` | |
| `IsInStock` | `bool` | `Product.IsInStock()` |
| `IsLowStock` | `bool` | `Product.IsLowStock()` |
| `HasDiscount` | `bool` | `Product.HasDiscount()` |
| `CreatedAt` | `DateTime` | |
| `UpdatedAt` | `DateTime?` | |

---

### `CategoryDto` — response

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | |
| `Name` | `string` | |
| `Description` | `string` | |
| `ImageUrl` | `string?` | |
| `IsActive` | `bool` | |
| `ParentCategoryId` | `Guid?` | |
| `ParentCategory` | `CategoryDto?` | Recursive — parent loaded when available |
| `SubCategories` | `List<CategoryDto>` | Children |
| `Products` | `List<ProductDto>` | Products in this category |
| `CreatedAt` | `DateTime` | |
| `UpdatedAt` | `DateTime?` | |

---

### `ProductImageDto`

`Id`, `ImageUrl` (string), `AltText?`, `SortOrder`, `IsPrimary`, `ProductId`

### `ProductSpecificationDto`

`Id`, `Name`, `Value`, `SortOrder`, `ProductId`

---

## Commands (Write Requests)

### `CreateProductCommand` — `DTOs/Commands/CreateProductCommand.cs`

Used for both creating (`POST /products`) and updating (`PUT /products/{id}`) a product.

| Property | Type | Validation |
|---|---|---|
| `Name` | `string` | Required, max 300 chars |
| `Description` | `string` | Required, max 2000 chars |
| `SKU` | `string` | Required, max 50 chars |
| `Price` | `decimal` | > 0 |
| `ComparePrice` | `decimal?` | If present: must be > `Price` |
| `StockQuantity` | `int` | ≥ 0 |
| `MinStockLevel` | `int` | ≥ 0 |
| `Weight` | `double` | > 0 |
| `Dimensions` | `string?` | Optional, e.g. `"30 x 20 x 10"` |
| `Brand` | `string?` | Optional |
| `Model` | `string?` | Optional |
| `CategoryId` | `Guid` | Not empty |
| `Images` | `List<CreateProductImageCommand>` | Nested validation |
| `Specifications` | `List<CreateProductSpecificationCommand>` | Nested validation |

**`CreateProductImageCommand`:**
- `ImageUrl`: required, max 500 chars
- `AltText?`: max 200 chars
- `SortOrder`: int
- `IsPrimary`: bool

**`CreateProductSpecificationCommand`:**
- `Name`: required, max 100 chars
- `Value`: required, max 500 chars
- `SortOrder`: int

---

### `CreateCategoryCommand` & `UpdateCategoryCommand` — `DTOs/Commands/CategoryCommands.cs`

**Create:** `Name` (required, max 200), `Description` (required, max 1000), `ImageUrl?` (max 500), `ParentCategoryId?`

**Update:** Same as Create + `Id` (required, not empty)

---

## MediatR Handlers

### Architecture

```
Controller → IMediator.Send(request) → Handler → IUnitOfWork → Repository → EF Core
```

All handlers are registered in DI via MediatR's assembly scanning.

---

### Product Command Handlers — `Handlers/Products/ProductCommandHandlers.cs`

#### `CreateProductHandler` handles `CreateProductRequest(CreateProductCommand Command) → ProductDto`

Steps:
1. Validate command via `IValidator<CreateProductCommand>` (FluentValidation)
2. Verify `CategoryId` exists via `IUnitOfWork.Categories.GetByIdAsync()`
3. Check SKU uniqueness via `IUnitOfWork.Products.GetBySkuAsync(sku)` — throws if already exists
4. Construct value objects: `new ProductSKU(sku)`, `new Money(price, "USD")`, `new ProductWeight(weight)`, `ProductDimensions.FromString(dimensions)` (if provided)
5. Instantiate `new Product(...)` — raises `ProductCreated` event
6. Add `ProductImage` children to `product.Images`
7. Add `ProductSpecification` children to `product.Specifications`
8. `await _unitOfWork.Products.AddAsync(product)`
9. `await _unitOfWork.SaveChangesAsync()`
10. Re-fetch product with relations → map to `ProductDto` → return

#### `UpdateProductHandler` handles `UpdateProductRequest(Guid Id, CreateProductCommand Command) → ProductDto`

Steps:
1. Validate command
2. Load existing product — throws `NotFoundException` if not found
3. Verify category exists
4. Check SKU uniqueness (excluding the product being updated)
5. Call domain methods: `UpdateBasicInfo()`, `UpdateSKU()`, `SetPrice()`, `SetStock()`, `UpdatePhysicalProperties()`, `SetCategory()`
6. Save and re-map

#### `DeleteProductHandler` handles `DeleteProductRequest(Guid Id) → bool`

Loads product → `_unitOfWork.Products.DeleteAsync(product)` (soft-delete) → `SaveChangesAsync()` → returns `true`

---

### Product Query Handlers — `Handlers/Products/ProductQueryHandlers.cs`

| Request | Handler | Repository Method |
|---|---|---|
| `GetAllProductsQuery` | `GetAllProductsHandler` | `Products.GetProductsWithImagesAsync()` |
| `GetProductByIdQuery(Guid Id)` | `GetProductByIdHandler` | `Products.GetByIdAsync(id)` |
| `GetProductBySkuQuery(string Sku)` | `GetProductBySkuHandler` | `Products.GetBySkuAsync(sku)` |
| `GetFeaturedProductsQuery` | `GetFeaturedProductsHandler` | `Products.GetFeaturedProductsAsync()` |
| `SearchProductsQuery(string SearchTerm)` | `SearchProductsHandler` | `Products.SearchProductsAsync(term)` |

All query handlers map results to `ProductDto` or `List<ProductDto>` via AutoMapper.

---

## AutoMapper Mappings — `MappingProfile.cs`

| Source | Destination | Notable custom mappings |
|---|---|---|
| `Product` | `ProductDto` | `SKU ← SKU.Value`; `Price ← Price.Amount`; `ComparePrice ← ComparePrice?.Amount`; `Weight ← Weight.Value`; `Dimensions ← Dimensions.ToString()`; `IsInStock/IsLowStock/HasDiscount` from domain methods |
| `Category` | `CategoryDto` | All name-matches (default) |
| `CreateCategoryCommand` | `Category` | `ConstructUsing(src => new Category(src.Name, src.Description, src.ImageUrl, src.ParentCategoryId))` |
| `ProductImage` | `ProductImageDto` | `ImageUrl ← ImageUrl` (string property backed by value object) |
| `ProductSpecification` | `ProductSpecificationDto` | `Name ← Name`, `Value ← Value` |

> **Important:** `Product` value objects expose their underlying value via string/decimal properties (e.g., `ProductSKU` has a `Value` property). AutoMapper unwraps these in the mapping profile.

---

## Adding a New Feature — Checklist

1. Define request record (command or query) in `DTOs/Commands/` or a new `DTOs/Queries/` file
2. Add FluentValidation validator if it's a command (write operation)
3. Create handler class implementing `IRequestHandler<TRequest, TResponse>`
4. Register nothing — MediatR scans assemblies automatically
5. Add response DTO if needed
6. Add AutoMapper mapping in `MappingProfile.cs`
7. Wire up the controller action in the API layer
