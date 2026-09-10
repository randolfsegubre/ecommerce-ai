# Code Walkthrough (for people new to React)

This document walks through the app in the **order things actually happen**
when you open it in a browser — one step per file. Each source file also has
a matching `WALKTHROUGH STEP N of 7` comment block at the top, so you can
jump back and forth between this doc and the real code.

If you're new to React, here are the four vocabulary words you'll see over
and over below:

- **Component** — a JavaScript function that returns JSX (HTML-like syntax)
  describing what should appear on screen. React calls it to render UI.
- **Props** — data passed *into* a component from its parent, e.g.
  `<ProductCard product={someProduct} />` — inside ProductCard, `product` is
  a prop.
- **State** — data a component (or the whole app) remembers and can change
  over time, which causes the UI to re-render when it changes.
- **Hook** — a special function (always starting with `use`, e.g. `useState`,
  `useGetProductsQuery`) that lets a component tap into React features like
  state or data fetching.

---

## The map

```
index.html
  -> src/main.jsx                (STEP 1 — boot the app)
       -> src/app/store.js       (STEP 2 — global state container)
            -> src/features/products/productsApi.js   (STEP 3 — how to fetch products)
       -> src/components/ProductList.jsx               (STEP 4 — the "/" page)
            -> src/components/ProductCard.jsx           (STEP 5 — one product's UI)

Not wired into the running app yet, but part of the codebase:
  src/features/products/productsSlice.js  (STEP 6 — future local state)
  src/types/Product.js                    (STEP 7 — shape of a Product)
```

---

## STEP 1 — `src/main.jsx`: the app boots

The browser loads `index.html`, which has:
```html
<script type="module" src="/src/main.jsx"></script>
```
That makes `main.jsx` the very first JavaScript to run. It does three things:

1. Finds `<div id="root">` in `index.html` and tells React "render into here."
2. Wraps everything in `<Provider store={store}>` — this is what makes the
   Redux store (STEP 2) available to *any* component anywhere in the tree,
   without having to manually pass it down as a prop.
3. Wraps everything in `<BrowserRouter>` / `<Routes>` / `<Route>` — this is
   **React Router**, a library for showing different components depending on
   the URL. Right now there's only one route: visiting `/` renders
   `<ProductList />`.

**Key idea:** `main.jsx` doesn't contain any of the app's real UI — it's just
wiring: "here's where global state comes from, here's how pages are chosen."

---

## STEP 2 — `src/app/store.js`: the global state container

Redux apps have exactly **one store** — a single object holding all shared
state. This file builds it with `configureStore`.

Right now the store only holds one thing: the cache that `productsApi`
(STEP 3) uses to remember fetched products, avoid duplicate network
requests, and know when to show loading/error states.

```js
reducer: {
  [productsApi.reducerPath]: productsApi.reducer,
}
```

`productsApi.reducerPath` is just the string `'productsApi'` — this line
means "store `productsApi`'s data under `state.productsApi`."

**Key idea:** a "reducer" is a function that says how a slice of state
changes in response to an action. You don't call reducers directly; Redux
calls them for you when something is `dispatch`ed.

---

## STEP 3 — `src/features/products/productsApi.js`: how to fetch products

This uses **RTK Query** (a data-fetching toolkit built into Redux Toolkit)
instead of writing `fetch()` + `useState()` + `useEffect()` by hand.

```js
export const productsApi = createApi({
  reducerPath: 'productsApi',
  baseQuery: fetchBaseQuery({ baseUrl: 'https://api.example.com/' }),
  endpoints: (builder) => ({
    getProducts: builder.query({ query: () => 'products' }),
  }),
});

export const { useGetProductsQuery } = productsApi;
```

- `baseQuery` says every request starts from `https://api.example.com/`
  (a placeholder — replace with the real backend URL).
- `endpoints` lists the available operations. `getProducts` means "GET
  `https://api.example.com/products`".
- RTK Query then **auto-generates** a React hook named `useGetProductsQuery`
  — you never write this hook yourself, it's created from the endpoint name.

**Key idea:** this file doesn't fetch anything by itself. It only *defines*
how fetching would work. The fetch actually happens when a component calls
`useGetProductsQuery()` — which is STEP 4.

---

## STEP 4 — `src/components/ProductList.jsx`: the page that fetches + lists

This is the component shown at the `/` route (wired up in STEP 1). It's what
some people call a "container" or "smart" component: it fetches data and
decides what to render based on that data's status.

```jsx
const { data: products, error, isLoading } = useGetProductsQuery();

if (isLoading) return <div>Loading products...</div>;
if (error) return <div>Error loading products.</div>;

return (
  <Grid container spacing={2}>
    {products?.map((product) => (
      <Grid item ... key={product.id}>
        <ProductCard product={product} />
      </Grid>
    ))}
  </Grid>
);
```

- Calling the hook `useGetProductsQuery()` is what actually triggers the
  network request from STEP 3. React re-runs this component automatically
  as the request moves from "loading" to "done" or "error."
- `.map()` turns the array of products into an array of `<ProductCard>`
  elements — one per product. The `key={product.id}` prop is required by
  React so it can efficiently tell list items apart when re-rendering.

**Key idea:** `products?.map(...)` — the `?.` (optional chaining) means "only
call `.map` if `products` isn't `undefined`/`null`" — important because while
the query is still loading, `products` starts out as `undefined`.

---

## STEP 5 — `src/components/ProductCard.jsx`: one product's UI

This is a **presentational component** — it has no state and makes no API
calls. It just receives a `product` object as a prop and displays it.

```jsx
const ProductCard = ({ product }) => (
  <Card sx={{ maxWidth: 345 }}>
    <CardMedia image={product.imageUrl} alt={product.name} />
    <CardContent>
      <Typography variant="h5">{product.name}</Typography>
      <Typography variant="body2">{product.description}</Typography>
      <Typography variant="subtitle1">${product.price}</Typography>
    </CardContent>
  </Card>
);
```

`Card`, `CardMedia`, `CardContent`, `Typography` all come from **Material UI
(MUI)**, a third-party component library — they're not custom to this
project, just pre-built, styled building blocks.

**Key idea:** `{ product }` in the function signature is called *props
destructuring*. `ProductCard` technically receives one `props` object
(`{ product: {...} }`); this syntax pulls `product` straight out of it.

---

## STEP 6 — `src/features/products/productsSlice.js`: state for later

This is a second Redux "slice," separate from `productsApi`. It's meant to
remember which product a user has clicked on (`selectedProduct`), for a
future feature like a product-detail view.

**Important:** unlike `productsApi`, this slice is **not yet registered** in
`store.js` — so right now `selectProduct` / `clearSelectedProduct` exist as
code but nothing in the running app calls `dispatch()` on them. It's written
ahead of time as a pattern to follow when that feature gets built.

---

## STEP 7 — `src/types/Product.js`: what a Product looks like

Plain JavaScript has no built-in type system (unlike TypeScript). This file
uses a **JSDoc comment** to describe a Product's shape anyway, purely so
editors like VS Code can offer autocomplete when you type `product.` in the
other files:

```js
/**
 * @typedef {Object} Product
 * @property {string} id
 * @property {string} name
 * @property {string} description
 * @property {number} price
 * @property {string} imageUrl
 * @property {string} brand
 * @property {Object.<string, string>} specs
 */
```

Nothing here executes — it's documentation, not logic.

---

## A note on the `.ts`/`.tsx` files next to each `.js`/`.jsx` file

You'll notice files like `ProductCard.tsx` sitting next to `ProductCard.jsx`.
Those `.ts`/`.tsx` files are **empty (0 bytes)** and the project has no
`tsconfig.json` — the app actually runs on the `.jsx`/`.js` files only
(confirmed by `index.html`, which loads `/src/main.jsx`). They look like
leftovers from an abandoned TypeScript migration attempt; this walkthrough
and the inline comments only cover the real, active files.
