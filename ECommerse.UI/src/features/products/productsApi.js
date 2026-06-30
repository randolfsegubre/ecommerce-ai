// src/features/products/productsApi.js
// RTK Query API slice for fetching computer products from backend
// Replace 'BASE_URL' with your actual API endpoint

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export const productsApi = createApi({
  reducerPath: 'productsApi',
  baseQuery: fetchBaseQuery({ baseUrl: 'https://api.example.com/' }), // TODO: Replace with your API URL
  endpoints: (builder) => ({
    getProducts: builder.query({
      query: () => 'products',
    }),
  }),
});

export const { useGetProductsQuery } = productsApi;
