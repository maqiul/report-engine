import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// 接入本地 @reportengine/vue 包
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5174,
    host: '127.0.0.1',
    proxy: {
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: true
  }
})
