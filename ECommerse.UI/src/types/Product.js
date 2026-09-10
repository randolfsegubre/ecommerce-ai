// src/types/Product.js
// ============================================================================
// WALKTHROUGH STEP 7 of 7 — Shared data shape (documentation only, no code runs)
// Plain JavaScript has no built-in "types". This JSDoc block just documents
// what a Product object looks like, so editors can autocomplete/check
// product.xxx fields. Used conceptually by ProductCard.jsx, ProductList.jsx
// and productsApi.js — nothing here is imported at runtime, though.
// That's the whole app! See WALKTHROUGH.md for the full guided tour.
// ============================================================================
/**
 * @typedef {Object} Product
 * @property {string} id            - Unique identifier for the product
 * @property {string} name          - Product name shown as the card title
 * @property {string} description   - Short description shown on the card
 * @property {number} price         - Price in whatever currency the API returns
 * @property {string} imageUrl      - URL of the product photo
 * @property {string} brand         - Manufacturer/brand name
 * @property {Object.<string, string>} specs - Free-form key/value spec sheet
 *   (e.g. { "CPU": "Intel i7", "RAM": "16GB" })
 */
