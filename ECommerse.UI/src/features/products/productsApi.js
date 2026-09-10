// src/features/products/productsApi.js
// ============================================================================
// WALKTHROUGH STEP 3 of 7 — API layer ("RTK Query" data fetching)
// Defines HOW to fetch products/categories from the real E-Commerse.AI.API
// backend, and auto-generates React hooks (useGetProductsQuery etc.) that
// STEP 4 (ProductList.jsx) calls. Its reducer/middleware got registered
// into the store back in STEP 2. This is not a component — it's a
// data-fetching config object; baseUrl is relative because Vite's dev
// proxy (vite.config.js) forwards /api/* to the real backend, so this
// works both in dev and once built.
// Next: STEP 4 -> ../../components/ProductList.jsx
// ============================================================================

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'; // [library]

// The API "slice": bundles the base URL, endpoints, and auto-generated
// Redux reducer/middleware + React hooks (like useGetProductsQuery below).
export const productsApi = createApi({
  reducerPath: 'productsApi', // key this API's cache lives under in the store (used in store.js)
  baseQuery: fetchBaseQuery({ baseUrl: '/api/' }),
  // tagTypes + providesTags below: RTK Query's cache-invalidation
  // mechanism — a future mutation (e.g. "delete product") can invalidate
  // the 'Product' tag so every query that provided it automatically refetches.
  tagTypes: ['Product', 'Category'],
  endpoints: (builder) => ({
    // GET /api/products
    getProducts: builder.query({
      query: () => 'products',
      providesTags: ['Product'],
    }),
    // GET /api/products/:id
    getProduct: builder.query({
      query: (id) => `products/${id}`,
      providesTags: ['Product'],
    }),
    // GET /api/products/featured
    getFeaturedProducts: builder.query({
      query: () => 'products/featured',
      providesTags: ['Product'],
    }),
    // GET /api/categories
    getCategories: builder.query({
      query: () => 'categories',
      providesTags: ['Category'],
    }),
  }),
});

// Auto-generated hooks (`use<EndpointName>Query`): call one of these inside
// a component to trigger the matching fetch and get back { data, error, isLoading }.
export const {
  useGetProductsQuery,
  useGetProductQuery,
  useGetFeaturedProductsQuery,
  useGetCategoriesQuery,
} = productsApi;
