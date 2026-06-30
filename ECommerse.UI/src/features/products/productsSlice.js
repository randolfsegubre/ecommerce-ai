// src/features/products/productsSlice.js
// Redux slice for managing product state (optional, for local state)

import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  selectedProduct: null,
};

export const productsSlice = createSlice({
  name: 'products',
  initialState,
  reducers: {
    selectProduct(state, action) {
      state.selectedProduct = action.payload;
    },
    clearSelectedProduct(state) {
      state.selectedProduct = null;
    },
  },
});

export const { selectProduct, clearSelectedProduct } = productsSlice.actions;
export default productsSlice.reducer;
