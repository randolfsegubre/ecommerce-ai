# API Layer Guide — `E-Commerse.AI.API`

The presentation layer. Thin controllers that delegate all business logic to MediatR handlers in the Application layer.

---

## Configuration — `Program.cs`

| Concern | Detail |
|---|---|
| Routing | Conventional controller routing |
| Swagger | Available at `/swagger` |
| MediatR | Registered via `AddMediatR(ApplicationAssembly)` |
| AutoMapper | Registered via `AddAutoMapper(ApplicationAssembly)` |
| EF Core | `ECommerceDbContext` with SQL Server |
| Repository DI | `IUnitOfWork` → `UnitOfWork`, `ICategoryRepository` → `CategoryRepository`, `IProductRepository` → `ProductRepository` |

---

## All Endpoints

### `ProductsController` — `/api/products`

| Method | Route | MediatR Request | Description |
|---|---|---|---|
| `GET` | `/` | `GetAllProductsQuery` | List all active products with images and category |
| `GET` | `/{id}` | `GetProductByIdQuery(id)` | Get product by ID |
| `GET` | `/sku/{sku}` | `GetProductBySkuQuery(sku)` | Get product by SKU |
| `GET` | `/featured` | `GetFeaturedProductsQuery` | List featured products |
| `GET` | `/search?q={term}` | `SearchProductsQuery(term)` | Search by name/description/SKU |
| `POST` | `/` | `CreateProductRequest(command)` | Create product |
| `PUT` | `/{id}` | `UpdateProductRequest(id, command)` | Update product |
| `DELETE` | `/{id}` | `DeleteProductRequest(id)` | Soft-delete product |

**Create/Update request body (`CreateProductCommand`):**
```json
{
  "name": "iPhone 15 Pro",
  "description": "Apple smartphone with titanium frame",
  "sku": "APPL-IP15PRO-128",
  "price": 89999.00,
  "comparePrice": 94999.00,
  "stockQuantity": 50,
  "minStockLevel": 5,
  "weight": 0.187,
  "dimensions": "14.67 x 7.09 x 0.83",
  "brand": "Apple",
  "model": "iPhone 15 Pro",
  "categoryId": "...",
  "images": [
    { "imageUrl": "https://...", "altText": "Front view", "sortOrder": 0, "isPrimary": true }
  ],
  "specifications": [
    { "name": "Storage", "value": "128GB", "sortOrder": 0 },
    { "name": "Color", "value": "Natural Titanium", "sortOrder": 1 }
  ]
}
```

---

### `CategoriesController` — `/api/categories`

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | Get all active categories (flat list) |
| `GET` | `/roots` | Get root categories with their immediate subcategories |
| `GET` | `/{id}/subcategories` | Get subcategories of a specific category |
| `GET` | `/{id}/products` | Get category with its products |
| `GET` | `/{id}` | Get category by ID (includes parent and children) |
| `POST` | `/` | Create category |
| `PUT` | `/{id}` | Update category |
| `DELETE` | `/{id}` | Soft-delete category (fails if it has subcategories or products) |

**Create/Update request body (`CreateCategoryCommand`):**
```json
{
  "name": "Smartphones",
  "description": "Mobile phones and accessories",
  "imageUrl": "https://...",
  "parentCategoryId": "..."  // null for root category
}
```

---

### `ProductImagesController` — `/api/product-images`

| Method | Route | Description |
|---|---|---|
| `GET` | `/{productId}` | Get all images for a product |
| `POST` | `/` | Add image to product |
| `PUT` | `/{id}` | Update image |
| `DELETE` | `/{id}` | Remove image |
| `PUT` | `/{id}/primary` | Set image as primary |

---

## Error Responses

| Scenario | Status |
|---|---|
| Validation failure (FluentValidation) | `400 Bad Request` with field errors |
| Entity not found | `404 Not Found` |
| SKU already exists | `409 Conflict` |
| Unhandled exception | `500 Internal Server Error` |

---

## Controller Pattern

All controllers follow this pattern — delegate to MediatR:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await mediator.Send(new GetAllProductsQuery(), ct);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var product = await mediator.Send(new CreateProductRequest(command), ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
```
