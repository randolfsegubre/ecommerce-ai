# Frontend Guide — `ECommerse.UI`

**Location:** `ECommerse.UI/`  
**Stack:** React 18, Vite, Redux Toolkit + RTK Query, Tailwind CSS (or plain CSS)

---

## Folder Structure

```
ECommerse.UI/
├── src/
│   ├── app/
│   │   ├── store.js        ← Redux store setup
│   │   └── store.ts        ← TypeScript version (parallel)
│   ├── components/
│   │   ├── ProductCard.jsx  ← Product display card
│   │   └── ProductList.jsx  ← List of ProductCards
│   ├── features/
│   │   └── products/
│   │       ├── productsApi.js   ← RTK Query endpoints
│   │       └── productsSlice.js ← Redux state slice
│   ├── types/
│   │   └── Product.js      ← Product type/shape documentation
│   ├── main.jsx            ← React entry point
│   └── index.css
├── index.html
├── vite.config.js
└── package.json
```

---

## Redux Store — `app/store.js`

```js
import { configureStore } from '@reduxjs/toolkit';
import { productsApi } from '../features/products/productsApi';
import productsReducer from '../features/products/productsSlice';

export const store = configureStore({
  reducer: {
    [productsApi.reducerPath]: productsApi.reducer,
    products: productsReducer,
  },
  middleware: (getDefault) => getDefault().concat(productsApi.middleware),
});
```

**State shape:**
```
store
├── productsApi  ← RTK Query cache (auto-managed)
└── products     ← Local UI state (selected product, filters, etc.)
```

---

## RTK Query — `features/products/productsApi.js`

RTK Query auto-generates hooks for data fetching with caching and invalidation.

### Base URL
Configured to point at the API (e.g., `http://localhost:5000/api`).

### Endpoints

| Hook | Method | Route | Description |
|---|---|---|---|
| `useGetAllProductsQuery()` | `GET` | `/products` | Fetch all active products |
| `useGetProductByIdQuery(id)` | `GET` | `/products/{id}` | Fetch single product |
| `useGetFeaturedProductsQuery()` | `GET` | `/products/featured` | Fetch featured products |
| `useSearchProductsQuery(term)` | `GET` | `/products/search?q={term}` | Search products |
| `useCreateProductMutation()` | `POST` | `/products` | Create product |
| `useUpdateProductMutation()` | `PUT` | `/products/{id}` | Update product |
| `useDeleteProductMutation()` | `DELETE` | `/products/{id}` | Delete product |
| `useGetAllCategoriesQuery()` | `GET` | `/categories` | Fetch all categories |
| `useGetRootCategoriesQuery()` | `GET` | `/categories/roots` | Fetch root categories with children |

### Cache Invalidation Tags

| Tag | Invalidated By |
|---|---|
| `Product` | Create, Update, Delete mutations |
| `Category` | Category mutations |

---

## Products Slice — `features/products/productsSlice.js`

Local Redux state for UI concerns (not server data — that's in RTK Query cache).

| State | Type | Description |
|---|---|---|
| `selectedProduct` | `object \| null` | Currently selected/viewed product |
| `filters` | `object` | Active search/filter criteria |
| `viewMode` | `"grid" \| "list"` | Display preference |

**Actions:**
- `setSelectedProduct(product)` — set the active product
- `clearSelectedProduct()` — deselect
- `setFilters(filters)` — update search/filter state
- `setViewMode(mode)` — toggle grid/list

---

## Components

### `ProductCard` — `components/ProductCard.jsx`

Displays a single product in a card layout.

**Props:**
| Prop | Type | Description |
|---|---|---|
| `product` | `ProductDto` | Product data from API |
| `onSelect` | `function` | Called when card is clicked |
| `onAddToCart` | `function` | Called when "Add to Cart" is clicked |
| `showBadges` | `bool` | Show "Featured", "Low Stock", "Out of Stock" badges |

**Displays:**
- Primary product image (or placeholder)
- Name, Brand/Model
- Price + compare price (strikethrough) + discount badge
- Stock status indicator
- Category name

---

### `ProductList` — `components/ProductList.jsx`

Renders a grid or list of `ProductCard` components.

**Props:**
| Prop | Type | Description |
|---|---|---|
| `products` | `ProductDto[]` | Array of products |
| `viewMode` | `"grid" \| "list"` | Layout mode |
| `isLoading` | `bool` | Shows skeleton cards while loading |
| `onProductSelect` | `function` | Forwarded to each `ProductCard` |

**Usage with RTK Query:**

```jsx
import { useGetAllProductsQuery } from '../features/products/productsApi';
import ProductList from '../components/ProductList';

function ProductsPage() {
  const { data: products = [], isLoading } = useGetAllProductsQuery();
  return <ProductList products={products} isLoading={isLoading} viewMode="grid" />;
}
```

---

## Running the Frontend

```powershell
cd ECommerse.UI
npm install    # first time only
npm run dev    # http://localhost:5173
```

Make sure the API is running (`dotnet run` in `E-Commerse.AI.API/`) before starting the frontend.

---

## Adding a New Feature

1. Add RTK Query endpoint to `productsApi.js` (or create a new API slice file)
2. Add any local UI state to an appropriate slice
3. Create component in `components/`
4. Wire up in a page component using the generated hooks
5. Add tag-based cache invalidation for mutations
