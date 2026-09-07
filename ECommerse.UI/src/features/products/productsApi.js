// src/features/products/productsApi.js
// RTK Query API slice for the E-Commerse.AI.API backend.
// baseUrl is relative - Vite's dev proxy (vite.config.js) forwards /api/* to the real backend.

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export const productsApi = createApi({
  reducerPath: 'productsApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/api/' }),
  tagTypes: ['Product', 'Category'],
  endpoints: (builder) => ({
    getProducts: builder.query({
      query: () => 'products',
      providesTags: ['Product'],
    }),
    getProduct: builder.query({
      query: (id) => `products/${id}`,
      providesTags: ['Product'],
    }),
    getFeaturedProducts: builder.query({
      query: () => 'products/featured',
      providesTags: ['Product'],
    }),
    getCategories: builder.query({
      query: () => 'categories',
      providesTags: ['Category'],
    }),
  }),
});

export const {
  useGetProductsQuery,
  useGetProductQuery,
  useGetFeaturedProductsQuery,
  useGetCategoriesQuery,
} = productsApi;
