import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Base path for production (served from .NET wwwroot)
  base: '/',
  build: {
    outDir: 'dist',
    sourcemap: false,
    // Asset optimization
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom']
        }
      }
    }
  },
  server: {
    port: 5173,
    proxy: {
      // Proxy API requests to the .NET backend (development only)
      '/api': {
        target: 'http://localhost:5180',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
