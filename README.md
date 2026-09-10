# E-Commerce AI — Product & Category Management API

A .NET 9 Clean Architecture e-commerce backend, plus a React frontend for browsing. "AI" in the name is aspirational only — there is no AI feature implemented; `Azure.AI.OpenAI` is referenced but unused.

**Status (2026-09-07): dev-complete, verified end-to-end locally.** Controllers are wired through MediatR to real EF Core-backed handlers (this was previously stubbed — see "Known limitations" below for what that fix uncovered and what's still deliberately out of scope), against a real SQL Server LocalDB database with an EF Core migration and local seed data. Verified in this pass: `dotnet build` on the full solution, a live `dotnet run` with automatic migration + seeding, full CRUD through Swagger and curl (including FluentValidation returning structured 400s instead of raw 500s), and the React app rendering real backend data end-to-end in a browser via the Vite dev proxy.

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
| Mapping | AutoMapper 13.0.1 |
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

**2. Database connection**

Already configured for a zero-setup local run: `E-Commerse.AI.API/appsettings.json` ships a default connection string pointing at SQL Server LocalDB (`(localdb)\mssqllocaldb`, database `ECommerceAI_Dev`, Windows-integrated auth — no password, so safe to commit), which ships with Visual Studio / SQL Server Express and needs no separate install or Docker container on most dev machines. Override it in a gitignored `appsettings.Development.json` if you want a different SQL Server instance.

**3. Restore and build**

```powershell
dotnet restore
dotnet build ECommerce.AI.sln
```

**4. Install frontend dependencies**

```powershell
cd ECommerse.UI
npm install
```

There's no separate "create the database" step — `Program.cs` calls `Database.MigrateAsync()` and seeds three sample products on every startup (skipped if data already exists), so the first `dotnet run` creates and populates `ECommerceAI_Dev` automatically.

---

### Running the Application

**Terminal 1 — API:**

```powershell
cd E-Commerse.AI.API
dotnet run
# API + Swagger UI: http://localhost:5196  (also https://localhost:7104 — see launchSettings.json)
```

**Terminal 2 — Frontend:**

```powershell
cd ECommerse.UI
npm run dev
# Frontend: http://localhost:5173
```

The frontend talks to the API through Vite's dev proxy (`vite.config.js` forwards `/api/*` to `http://localhost:5196`), so no CORS setup or hardcoded API URL is needed for local dev — just make sure the API is running on its default `http` launch profile port before starting the frontend.

---

## Architecture Flow

```
Browser (React + Redux Toolkit)
  └── RTK Query (fetch-based) → Vite dev proxy
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

---

## Known limitations (as of 2026-09-07)

Real gaps surfaced while wiring the controllers end-to-end, kept here instead of silently fixed so the history is honest:

- **Category hierarchy endpoints (subcategories, ancestor/descendant "hierarchy" view) were removed, not implemented.** They previously returned hardcoded fake data; rather than ship another fake response, they were cut. `ICategoryRepository.GetSubCategoriesAsync` exists and works — a real subcategories endpoint is a small, well-scoped addition if needed.
- **`AutoMapper` 13.0.1 still shows a `NU1903` advisory (`GHSA-rvv3-g6hj-g44x`) on `dotnet build`/`dotnet list package --vulnerable`.** This advisory is about AutoMapper's post-12.x commercial licensing change, not an exploitable code vulnerability — there is no newer free version that clears it. Confirm current licensing terms before using this in anything commercial.
- **No authentication/authorization.** Swagger's Bearer scheme is defined but not enforced anywhere — every endpoint is open. Needed before any real deployment.
- **CORS is wide open in Development** (`AllowAnyOrigin/Method/Header`) and Production has no CORS policy configured at all. Needs a real allow-list before deploying.
- **Seed images are external `picsum.photos` URLs** — fine for local demo, but a production deployment should use real product imagery (blob storage/CDN), not a third-party placeholder service.
- **No automated tests.** Verification in this pass was manual (`dotnet build`, live `dotnet run`, curl/Swagger, and a browser check of the React app) — there is no test project yet. Worth adding before treating this as production-track rather than a portfolio piece.
- **`ECommerse.UI/src` has unused duplicate `.ts`/`.tsx` files sitting next to the real `.jsx` ones** (`store.ts`, `productsApi.ts`, `productsSlice.ts`, `ProductCard.tsx`, `ProductList.tsx`, `Product.ts`) left over from an abandoned TypeScript migration. There's no `tsconfig.json` and nothing imports them, so they're inert, not a build risk — but worth deleting for a clean portfolio checkout.
