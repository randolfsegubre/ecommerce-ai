// src/features/products/productsSlice.js
// ============================================================================
// WALKTHROUGH STEP 6 of 7 — Extra local-state slice (defined but NOT wired in)
// A Redux "slice" (one reducer + its actions) for remembering which product
// a user has selected. NOT currently registered in store.js (STEP 2) —
// nothing dispatches these actions yet. It's scaffolding for a future
// "product details" feature, kept here so you can see the pattern.
// Next: STEP 7 -> ../../types/Product.js
// ============================================================================

import { createSlice } from '@reduxjs/toolkit'; // [library]

// Starting state: no product selected yet.
const initialState = {
  selectedProduct: null, // holds a Product object (see src/types/Product.js) or null
};

export const productsSlice = createSlice({
  name: 'products',
  initialState,
  reducers: {
    // dispatch(selectProduct(someProduct)) -> action.payload = someProduct
    selectProduct(state, action) {
      state.selectedProduct = action.payload;
    },
    // dispatch(clearSelectedProduct()) -> resets selection to null
    clearSelectedProduct(state) {
      state.selectedProduct = null;
    },
  },
});

// Action creators — pass these to dispatch(...) to trigger the reducers above.
export const { selectProduct, clearSelectedProduct } = productsSlice.actions;

// The reducer itself; would be registered in store.js if/when this feature is wired in.
export default productsSlice.reducer;
