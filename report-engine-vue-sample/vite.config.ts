import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      // 代理 starter 后端（别人项目假设后端跑在 5000 端口）
      '/api/reports': 'http://localhost:5000',
    },
  },
})