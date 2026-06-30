# ECommerce.AI - Domain Driven Architecture Implementation Documentation

## Overview

This e-commerce application is built using **Domain Driven Architecture (DDA)** principles with a clean separation of concerns across multiple projects. The architecture follows SOLID principles, incorporates dependency injection, and uses modern .NET 9 patterns.

## Architecture Layers

### 1. Domain Layer (`ECommerce.AI.Domain`)
**Purpose**: Contains the core business logic, entities, and domain interfaces.

**Why this approach?**
- **Business Logic Isolation**: Domain entities contain business rules and invariants
- **Framework Independence**: No dependencies on external frameworks (pure C# objects)
- **Rich Domain Model**: Entities have behavior, not just data (following DDD principles)

#### Key Components:

##### BaseEntity (`Domain/Common/BaseEntity.cs`)
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    // ... audit fields
}
```

**Design Decisions:**
- **Protected Setters**: Prevents external modification of core properties
- **Guid IDs**: Better for distributed systems and security
- **Soft Delete**: `IsDeleted` flag instead of hard deletes for data integrity
- **Audit Trail**: Automatic tracking of creation and modification

##### Domain Entities
- **Product**: Core business entity with rich behavior
- **Category**: Hierarchical structure support
- **ProductImage**: Value objects for product imagery
- **ProductSpecification**: Technical specifications as domain concepts

**Why Private Constructors + Factory Methods?**
```csharp
private Product() { } // EF Constructor
public Product(string name, string description, ...) // Business constructor
```
- **Encapsulation**: Forces use of business constructors
- **Validation**: Ensures entities are always in valid state
- **EF Compatibility**: Private constructor for Entity Framework

##### Repository Interfaces (`Domain/Interfaces/`)
**Why Interfaces in Domain?**
- **Dependency Inversion**: High-level modules don't depend on low-level modules
- **Testability**: Easy to mock for unit tests
- **Framework Independence**: Domain doesn't know about EF Core

### 2. Application Layer (`ECommerce.AI.Application`)
**Purpose**: Contains use cases, DTOs, and application services.

#### CQRS Pattern Implementation
**Command Query Responsibility Segregation (CQRS)**

##### Commands (Write Operations)
```csharp
public record CreateProductCommand : IRequest<ProductDto>;
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
```

##### Queries (Read Operations)
```csharp
public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
```

**Why CQRS?**
- **Separation of Concerns**: Read and write operations have different requirements
- **Scalability**: Can optimize read and write operations separately
- **Maintainability**: Clear separation of business operations
- **MediatR Integration**: Clean request/response pattern

#### DTOs (Data Transfer Objects)
**Why DTOs?**
- **API Contract Stability**: Domain changes don't affect API consumers
- **Security**: Prevents over-posting attacks
- **Performance**: Only transfer needed data
- **Versioning**: API versioning without domain changes

#### AutoMapper Configuration
```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.IsInStock()))
```
**Benefits:**
- **Automatic Mapping**: Reduces boilerplate code
- **Type Safety**: Compile-time checking
- **Performance**: Optimized mapping

#### FluentValidation
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
}
```
**Why FluentValidation?**
- **Readable Validation Rules**: Business rules are clear
- **Testable**: Can unit test validation logic
- **Composable**: Rules can be combined and reused
- **Localization Support**: Error messages can be localized

### 3. Infrastructure Layer (`ECommerce.AI.Infrastructure`)
**Purpose**: Contains data access, external service implementations, and cross-cutting concerns.

#### Entity Framework Core Implementation
**Code-First Approach with Fluent API**

```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.Property(p => p.Name).IsRequired().HasMaxLength(300);
    entity.HasIndex(p => p.SKU).IsUnique();
});
```

**Why Code-First?**
- **Version Control**: Database schema in source control
- **Developer Productivity**: Database evolves with code
- **Type Safety**: Strongly typed queries
- **Migration Support**: Automatic database updates

#### Repository Pattern Implementation
**Why Repository Pattern?**
- **Abstraction**: Hides data access complexity
- **Testability**: Easy to unit test business logic
- **Flexibility**: Can switch data providers
- **Centralized Query Logic**: Reusable data access patterns

```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();
    }
}
```

#### Unit of Work Pattern
**Why Unit of Work?**
- **Transaction Management**: Ensures data consistency
- **Performance**: Single database connection per request
- **Atomicity**: All changes succeed or fail together

```csharp
public interface IUnitOfWork
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
}
```

### 4. API Layer (`E-Commerse.AI.API`)
**Purpose**: Web API controllers, middleware, and configuration.

#### Clean Controllers
Controllers are thin and delegate to MediatR:
```csharp
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        return Ok(result);
    }
}
```

**Benefits:**
- **Single Responsibility**: Controllers only handle HTTP concerns
- **Testability**: Business logic is in handlers
- **Consistency**: All operations follow same pattern

### 5. Shared Layer (`ECommerce.AI.Shared`)
**Purpose**: Common utilities, constants, and cross-cutting concerns.

## Design Patterns and Principles Applied

### SOLID Principles

#### Single Responsibility Principle (SRP)
- Each class has one reason to change
- Handlers do one specific operation
- Repositories handle data access for one aggregate

#### Open/Closed Principle (OCP)
- Easy to add new features without modifying existing code
- New handlers can be added without changing existing ones
- Repository pattern allows new data sources

#### Liskov Substitution Principle (LSP)
- Repository implementations can be substituted
- Base entity can be extended safely

#### Interface Segregation Principle (ISP)
- Specific repository interfaces (IProductRepository vs IRepository<T>)
- Focused DTOs for specific use cases

#### Dependency Inversion Principle (DIP)
- High-level modules depend on abstractions
- Infrastructure implements domain interfaces
- Dependency injection throughout

### Domain Driven Design (DDD) Concepts

#### Entities
- Have identity (ID)
- Contain business logic
- Maintain invariants

#### Value Objects
- ProductImage, ProductSpecification
- Immutable
- No identity, defined by values

#### Aggregates
- Product is aggregate root
- Category is aggregate root
- Consistency boundaries

#### Repositories
- One per aggregate root
- Encapsulate data access
- Return domain objects

#### Domain Services
- Cross-aggregate operations
- Business logic that doesn't fit in entities

## Technology Choices and Justifications

### .NET 9
- **Latest Framework**: Access to newest features and performance improvements
- **Minimal APIs**: Option for lightweight endpoints
- **AOT Compatibility**: Future performance optimizations

### Entity Framework Core 9
- **Mature ORM**: Proven in enterprise applications
- **Performance**: Query optimization and change tracking
- **Migrations**: Database evolution support
- **LINQ Support**: Type-safe queries

### MediatR
- **Decoupling**: Controllers don't depend on business logic
- **Pipeline Behaviors**: Cross-cutting concerns (logging, validation)
- **Testability**: Easy to unit test handlers

### AutoMapper
- **Productivity**: Reduces mapping boilerplate
- **Conventions**: Automatic property mapping
- **Performance**: Compiled expressions

### FluentValidation
- **Readability**: Business rules as code
- **Composability**: Rules can be combined
- **Integration**: Works with ASP.NET Core model binding

## Benefits of This Architecture

### Maintainability
- Clear separation of concerns
- Changes are localized
- Easy to understand and modify

### Testability
- Each layer can be tested independently
- Business logic is isolated
- Dependencies are easily mocked

### Scalability
- Layers can be scaled independently
- CQRS allows read/write optimization
- Microservices migration path

### Flexibility
- Easy to change data sources
- API can evolve independently
- Business rules are centralized

### Performance
- Optimized queries through repositories
- Lazy loading where appropriate
- Efficient mapping with AutoMapper

## Future Considerations

### AI Integration Module
The architecture is prepared for AI services:
- External service interfaces in Domain
- AI implementations in Infrastructure
- AI-powered features in Application layer

### Event Sourcing
The current architecture can evolve to event sourcing:
- Domain events can be added to entities
- Event handlers in Application layer
- Event store in Infrastructure

### Microservices
Each bounded context can become a microservice:
- Product management service
- Inventory service
- Category management service

## Conclusion

This Domain Driven Architecture provides a solid foundation for an e-commerce application that is maintainable, testable, and scalable. The clear separation of concerns and adherence to SOLID principles ensures the codebase remains manageable as it grows in complexity and features.

The architecture supports the current requirements while being flexible enough to accommodate future enhancements such as AI integration, event-driven architecture, and potential microservices decomposition.