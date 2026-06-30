// src/app/store.js
// Redux store configuration using Redux Toolkit and RTK Query
// RTK Query is included for API integration

import { configureStore } from '@reduxjs/toolkit';
import { productsApi } from '../features/products/productsApi';

export const store = configureStore({
  reducer: {
    [productsApi.reducerPath]: productsApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(productsApi.middleware),
});
