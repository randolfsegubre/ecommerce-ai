// src/components/ErrorBoundary.jsx
// ============================================================================
// A React error boundary - the standard React pattern for catching a
// render-time error anywhere in the component tree below it. Must be a
// class component: componentDidCatch/getDerivedStateFromError have no
// hook equivalent, so this can't be written as a function component like
// every other component in this app.
// ============================================================================
import React from 'react';
import { Box, Typography, Button } from '@mui/material'; // [library] MUI

class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  // React calls this during the render phase when a descendant throws -
  // its only job is to flip state so the next render shows the fallback UI.
  static getDerivedStateFromError() {
    return { hasError: true };
  }

  // Called after the error, safe for side effects like logging - real
  // errors always go to the console, never swallowed silently.
  componentDidCatch(error, info) {
    console.error('ErrorBoundary caught an error:', error, info);
  }

  handleBackToHome = () => {
    // A full page reload (not React Router's navigate()) is deliberate:
    // navigate() would just re-render the same broken component tree,
    // since this.state.hasError only resets on unmount. Reloading at "/"
    // restarts the whole app fresh - a new Redux store, no stale state -
    // which is what "restart the session" actually means here.
    window.location.href = '/';
  };

  render() {
    if (this.state.hasError) {
      return (
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
            Something went wrong
          </Typography>
          <Typography variant="h5" sx={{ mt: 1, fontWeight: 600 }}>
            We hit a snag loading this page
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mt: 1, maxWidth: 420 }}>
            This is on us, not you. Going back to the homepage usually clears it up.
          </Typography>
          <Button variant="contained" sx={{ mt: 4 }} onClick={this.handleBackToHome}>
            Back to Home
          </Button>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
