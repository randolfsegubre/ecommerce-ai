// src/components/ProductList.jsx
// ============================================================================
// WALKTHROUGH STEP 4 of 7 — Page component ("container": fetches + lists data)
// Rendered by the "/" route in main.jsx. Calls the data-fetching hook from
// STEP 3, then renders one ProductCard (STEP 5) per product returned.
// Next: STEP 5 -> ./ProductCard.jsx
// ============================================================================

import React from 'react';
import Grid from '@mui/material/Grid'; // [library] MUI layout grid
import { useGetProductsQuery } from '../features/products/productsApi'; // STEP 3's hook
import ProductCard from './ProductCard.jsx'; // STEP 5

// [component] ProductList — no props; manages its own data via the hook below.
const ProductList = () => {
  // [React hook] Calling this triggers the fetch and re-renders this
  // component automatically as the request goes from loading -> done/error.
  const { data: products, error, isLoading } = useGetProductsQuery();

  // Conditional rendering: show a message instead of the grid while busy/failed.
  if (isLoading) return <div>Loading products...</div>;
  if (error) return <div>Error loading products.</div>;

  return (
    <Grid container spacing={2}>
      {/* .map(...) turns each product into a <ProductCard>; `key` is required
          by React so it can efficiently track/update list items. */}
      {products?.map((product) => (
        <Grid item xs={12} sm={6} md={4} lg={3} key={product.id}>
          <ProductCard product={product} />
        </Grid>
      ))}
    </Grid>
  );
};

export default ProductList;
