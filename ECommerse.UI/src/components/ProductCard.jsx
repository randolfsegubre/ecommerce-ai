// src/components/ProductCard.jsx
// Card component for displaying a single product returned by the backend's ProductDto

import React from 'react';
import { Card, CardContent, CardMedia, Chip, Typography } from '@mui/material';

const ProductCard = ({ product }) => {
  const primaryImage = product.images?.find((img) => img.isPrimary) ?? product.images?.[0];

  return (
    <Card sx={{ maxWidth: 345, height: '100%', display: 'flex', flexDirection: 'column' }}>
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
        {!product.isInStock && <Chip label="Out of stock" size="small" color="warning" sx={{ mt: 1 }} />}
      </CardContent>
    </Card>
  );
};

export default ProductCard;
