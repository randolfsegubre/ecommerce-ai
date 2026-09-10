// src/components/ProductCard.jsx
// ============================================================================
// WALKTHROUGH STEP 5 of 7 — Presentational component (pure UI, no data fetching)
// Rendered once per product by ProductList.jsx (STEP 4). Just receives a
// `product` object as a prop and displays it — no state, no API calls of
// its own. Card/CardContent/CardMedia/Typography are pre-built components
// from the Material UI (MUI) library, not defined in this project.
// Next: STEP 6 -> ../features/products/productsSlice.js
// ============================================================================

import React from 'react';
import { Card, CardContent, CardMedia, Typography } from '@mui/material'; // [library] MUI

// [component] Props destructuring: `{ product }` pulls `product` out of the
// props object React passes in (shape documented in src/types/Product.js).
const ProductCard = ({ product }) => (
  // sx={{ ... }} is MUI's way of passing inline styles.
  <Card sx={{ maxWidth: 345 }}>
    {/* Product photo */}
    <CardMedia
      component="img"
      height="140"
      image={product.imageUrl}
      alt={product.name}
    />
    <CardContent>
      <Typography gutterBottom variant="h5" component="div">
        {product.name}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {product.description}
      </Typography>
      <Typography variant="subtitle1" color="primary">
        ${product.price}
      </Typography>
    </CardContent>
  </Card>
);

export default ProductCard;
