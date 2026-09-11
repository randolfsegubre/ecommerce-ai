// src/components/NotFound.jsx
// ============================================================================
// Catch-all 404 page - wired to a `path="*"` Route in main.jsx, which
// React Router matches when no other route matches the current URL.
// ============================================================================
import React from 'react';
import { Box, Typography, Button } from '@mui/material';

const NotFound = () => (
  <Box
    sx={{
      minHeight: '100vh',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      textAlign: 'center',
      px: 3,
    }}
  >
    <Typography variant="overline" color="text.secondary">
      404
    </Typography>
    <Typography variant="h5" sx={{ mt: 1, fontWeight: 600 }}>
      We couldn&apos;t find that page
    </Typography>
    <Typography variant="body1" color="text.secondary" sx={{ mt: 1, maxWidth: 420 }}>
      The page you&apos;re looking for doesn&apos;t exist or may have moved.
    </Typography>
    <Button variant="contained" sx={{ mt: 4 }} href="/">
      Back to Home
    </Button>
  </Box>
);

export default NotFound;
