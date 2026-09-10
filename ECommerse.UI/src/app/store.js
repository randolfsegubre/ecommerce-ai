// src/app/store.js
// ============================================================================
// WALKTHROUGH STEP 2 of 7 — Global state container (the Redux "store")
// Imported by main.jsx before anything renders. A Redux app has exactly ONE
// store; any component under <Provider> (main.jsx) can read from it. This
// store currently just holds RTK Query's product-fetching cache (STEP 3).
// Next: STEP 3 -> ../features/products/productsApi.js
// ============================================================================

import { configureStore } from '@reduxjs/toolkit'; // [library] Redux Toolkit
import { productsApi } from '../features/products/productsApi'; // STEP 3

export const store = configureStore({
  reducer: {
    // Registers productsApi's auto-generated reducer under state.productsApi
    [productsApi.reducerPath]: productsApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    // RTK Query needs its middleware added for caching/refetching to work.
    getDefaultMiddleware().concat(productsApi.middleware),
});
