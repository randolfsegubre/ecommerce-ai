// src/features/products/productsApi.js
// ============================================================================
// WALKTHROUGH STEP 3 of 7 — API layer ("RTK Query" data fetching)
// Defines HOW to fetch products from the backend, and auto-generates a React
// hook (useGetProductsQuery) that STEP 4 (ProductList.jsx) will call. Its
// reducer/middleware got registered into the store back in STEP 2.
// This is not a component — it's a data-fetching config object.
// Next: STEP 4 -> ../../components/ProductList.jsx
// ============================================================================

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'; // [library]

// The API "slice": bundles the base URL, endpoints, and auto-generated
// Redux reducer/middleware + React hooks (like useGetProductsQuery below).
export const productsApi = createApi({
  reducerPath: 'productsApi', // key this API's cache lives under in the store (used in store.js)
  baseQuery: fetchBaseQuery({ baseUrl: 'https://api.example.com/' }), // TODO: Replace with your API URL
  endpoints: (builder) => ({
    // GET https://api.example.com/products
    getProducts: builder.query({
      query: () => 'products',
    }),
  }),
});

// Auto-generated hook (`use<EndpointName>Query`): call this inside a
// component to trigger the fetch and get back { data, error, isLoading }.
export const { useGetProductsQuery } = productsApi;
