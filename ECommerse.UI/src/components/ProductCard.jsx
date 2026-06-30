// src/components/ProductCard.js
// Card component for displaying a single computer product

import React from 'react';
import { Card, CardContent, CardMedia, Typography } from '@mui/material';

const ProductCard = ({ product }) => (
  <Card sx={{ maxWidth: 345 }}>
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
