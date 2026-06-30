// src/components/ProductList.js
// Displays a list of computer products using ProductCard

import React from 'react';
import Grid from '@mui/material/Grid';
import { useGetProductsQuery } from '../features/products/productsApi';
import ProductCard from './ProductCard.jsx';

const ProductList = () => {
  const { data: products, error, isLoading } = useGetProductsQuery();

  if (isLoading) return <div>Loading products...</div>;
  if (error) return <div>Error loading products.</div>;

  return (
    <Grid container spacing={2}>
      {products?.map((product) => (
        <Grid item xs={12} sm={6} md={4} lg={3} key={product.id}>
          <ProductCard product={product} />
        </Grid>
      ))}
    </Grid>
  );
};

export default ProductList;
