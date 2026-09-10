// src/components/ProductCard.jsx
// ============================================================================
// WALKTHROUGH STEP 5 of 7 — Presentational component (pure UI, no data fetching)
// Rendered once per product by ProductList.jsx (STEP 4). Just receives a
// `product` object (shaped like the backend's ProductDto — see
// src/types/Product.js) as a prop and displays it — no state, no API calls
// of its own. Card/CardContent/CardMedia/Chip/Typography are pre-built
// components from the Material UI (MUI) library, not defined in this project.
// Next: STEP 6 -> ../features/products/productsSlice.js
// ============================================================================

import React from 'react';
import { Card, CardContent, CardMedia, Chip, Typography } from '@mui/material'; // [library] MUI

// [component] Props destructuring: `{ product }` pulls `product` out of the
// props object React passes in.
const ProductCard = ({ product }) => {
  // ProductDto's images are an array (a product can have several photos);
  // prefer whichever one is flagged isPrimary, falling back to the first
  // image if none is flagged (or to undefined if there are no images at all).
  const primaryImage = product.images?.find((img) => img.isPrimary) ?? product.images?.[0];

  return (
    // sx={{ ... }} is MUI's way of passing inline styles.
    <Card sx={{ maxWidth: 345, height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Product photo — only rendered if the product actually has one */}
      {primaryImage && (
        <CardMedia
          component="img"
          height="160"
          image={primaryImage.imageUrl}
          alt={primaryImage.altText ?? product.name}
        />
      )}
      <CardContent>
        <Typography gutterBottom variant="h6" component="div">
          {product.name}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {product.description}
        </Typography>
        <Typography variant="subtitle1" color="primary" sx={{ mt: 1 }}>
          ${product.price.toFixed(2)}
        </Typography>
        {/* Out-of-stock badge — only shown when isInStock is false, never a "In stock" badge for the common case */}
        {!product.isInStock && <Chip label="Out of stock" size="small" color="warning" sx={{ mt: 1 }} />}
      </CardContent>
    </Card>
  );
};

export default ProductCard;
