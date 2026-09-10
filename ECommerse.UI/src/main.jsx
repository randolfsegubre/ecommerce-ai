// src/main.jsx
// ============================================================================
// WALKTHROUGH STEP 1 of 7 — App entry point
// Loaded first (index.html has <script src="/src/main.jsx">). Boots React,
// plugs in Redux (global state) and React Router (page navigation), renders
// the app. Full guided tour: see WALKTHROUGH.md in the project root.
// Next: STEP 2 -> ./app/store.js
// ============================================================================

import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux'; // [library] connects React to the Redux store
import { store } from './app/store'; // STEP 2 — global state
import { BrowserRouter, Routes, Route } from 'react-router-dom'; // [library] page routing
import ProductList from './components/ProductList.jsx'; // STEP 4 — main page component
import './index.css';

// Turn the <div id="root"> from index.html into a React render target.
const root = ReactDOM.createRoot(document.getElementById('root'));

root.render(
  // <Provider> [component] makes the Redux `store` readable by every
  // component nested inside it (that's how ProductList.jsx gets data later).
  <Provider store={store}>
    {/* <BrowserRouter> [component] enables URL-based navigation */}
    <BrowserRouter>
      {/* <Routes>/<Route> [components]: "when URL is X, render component Y" */}
      <Routes>
        <Route path="/" element={<ProductList />} />
        {/* Add more routes as needed */}
      </Routes>
    </BrowserRouter>
  </Provider>
);
