import path from "path"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    // Output to project root dist/ for Tauri bundling
    outDir: '../../dist',
    emptyOutDir: true,
  },
})
// Trigger restart
