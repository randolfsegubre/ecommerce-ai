// src/main.js
// Entry point for React app with Redux Provider and React Router

import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import { store } from './app/store';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import ProductList from './components/ProductList.jsx';
import './index.css';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <Provider store={store}>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<ProductList />} />
        {/* Add more routes as needed */}
      </Routes>
    </BrowserRouter>
  </Provider>
);
