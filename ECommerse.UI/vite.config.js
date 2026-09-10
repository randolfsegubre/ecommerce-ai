import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Forwards /api/* to the local E-Commerse.AI.API backend (dotnet run, https://localhost:7104)
      // so the frontend can call a relative '/api/...' URL with no CORS or hardcoded-port setup.
      // Matches the "http" launch profile in E-Commerse.AI.API/Properties/launchSettings.json,
      // which is what a plain `dotnet run` binds to.
      '/api': {
        target: 'http://localhost:5196',
        changeOrigin: true,
      },
    },
  },
})
