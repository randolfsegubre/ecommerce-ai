# E-Commerce AI — Product & Category Management API

A .NET 9 Clean Architecture e-commerce backend with AI-integration capabilities. Handles hierarchical product categories, product lifecycle management (pricing, stock, specifications, images), and a React frontend for browsing.

---

## What This Application Does

| Feature | Description |
|---|---|
| **Product Catalog** | CRUD for products with SKU, pricing, stock levels, weight, and dimensions |
| **Category Hierarchy** | Unlimited-depth tree of categories; root → subcategory → sub-subcategory |
| **Stock Management** | Low-stock alerts and out-of-stock events via domain events |
| **Price Management** | Regular price + compare-at price with discount calculation |
| **Product Images** | Multiple images per product with sort order and primary image designation |
| **Specifications** | Key-value specification pairs (e.g. Color: Red, RAM: 16GB) |
| **Search** | Full-text product search across name, description, and SKU |
| **Domain Events** | Rich event model — CategoryCreated, ProductPriceChanged, ProductLowStockAlert, etc. |

---

## Technology Stack

| Layer | Technology |
|---|---|
| Backend framework | ASP.NET Core (.NET 9) |
| Architecture | Clean Architecture |
| ORM | EF Core (SQL Server) |
| Mediator | MediatR 12.4.1 |
| Validation | FluentValidation 11.11.0 |
| Mapping | AutoMapper 12.0.1 |
| Frontend | React + Vite (JavaScript) |
| State | Redux Toolkit + RTK Query |

---

## Solution Structure

```
E-Commerse.AI.API/
├── ECommerce.AI.sln
├── ECommerce.AI.Domain/          ← Entities, value objects, events, interfaces (no dependencies)
├── ECommerce.AI.Application/     ← DTOs, commands, MediatR handlers, AutoMapper
├── ECommerce.AI.Infrastructure/  ← EF Core DbContext, repository implementations
├── ECommerce.AI.Shared/          ← Shared utilities (currently minimal)
├── E-Commerse.AI.API/            ← ASP.NET Core Web API controllers
└── ECommerse.UI/                 ← React + Vite SPA (Redux Toolkit)
```

Each layer has a `GUIDE.md` with full object, method, and relationship documentation.

---

## Local Development Setup

### Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 9.0.x |
| Node.js | 18+ LTS |
| SQL Server / LocalDB | Any |
| Git | Any |

---

### Step-by-Step First-Time Setup

**1. Clone the repository**

```powershell
git clone https://github.com/randolfsegubre/ecommerce-ai.git
cd ecommerce-ai
```

**2. Configure the database**

Edit `E-Commerse.AI.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerceAI_Dev;Trusted_Connection=True;"
  }
}
```

**3. Create the database**

```powershell
dotnet ef database update \
  --project ECommerce.AI.Infrastructure \
  --startup-project E-Commerse.AI.API
```

**4. Restore and build**

```powershell
dotnet restore
dotnet build
```

**5. Install frontend dependencies**

```powershell
cd ECommerse.UI
npm install
```

---

### Running the Application

**Terminal 1 — API:**

```powershell
cd E-Commerse.AI.API
dotnet run
# API: http://localhost:5000 (or check launchSettings.json)
# Swagger: http://localhost:5000/swagger
```

**Terminal 2 — Frontend:**

```powershell
cd ECommerse.UI
npm run dev
# Frontend: http://localhost:5173
```

---

## Architecture Flow

```
Browser (React + Redux Toolkit)
  └── RTK Query → axios
        └── /api/* → API

ASP.NET Core API
  └── MediatR pipeline
        └── Command/Query Handler
              └── IUnitOfWork → Repository
                    └── EF Core → SQL Server
                          └── Domain entity (raises DomainEvents)
```

### Request lifecycle (command):
1. React dispatches an RTK Query mutation (e.g., create product)
2. Controller receives `CreateProductCommand`, sends to `IMediator`
3. `CreateProductHandler` validates via FluentValidation
4. Handler constructs domain aggregate `Product` (raises `ProductCreated` event)
5. Persists via `IUnitOfWork.SaveChangesAsync()`
6. `DomainEvents` are cleared and can be dispatched to event bus (not yet wired)
7. AutoMapper maps `Product` → `ProductDto` and returns to client

---

## Layer-by-Layer Developer Guides

| Layer | Guide File |
|---|---|
| Domain (entities, value objects, events, specs) | [ECommerce.AI.Domain/GUIDE.md](ECommerce.AI.Domain/GUIDE.md) |
| Application (DTOs, commands, MediatR handlers) | [ECommerce.AI.Application/GUIDE.md](ECommerce.AI.Application/GUIDE.md) |
| Infrastructure (repositories, DbContext) | [ECommerce.AI.Infrastructure/GUIDE.md](ECommerce.AI.Infrastructure/GUIDE.md) |
| API (controllers, endpoints) | [E-Commerse.AI.API/GUIDE.md](E-Commerse.AI.API/GUIDE.md) |
| Frontend (React components, Redux store) | [ECommerse.UI/GUIDE.md](ECommerse.UI/GUIDE.md) |
