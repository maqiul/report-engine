import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  build: {
    lib: {
      entry: resolve(__dirname, 'src/index.ts'),
      name: 'ReportEngineVue',
      fileName: (format) => `report-engine-vue.${format}.js`,
      formats: ['es', 'umd'],
    },
    rollupOptions: {
      // Vue 是 peerDep，不要打进去
      external: ['vue'],
      output: {
        globals: {
          vue: 'Vue',
        },
        assetFileNames: (assetInfo) => {
          if (assetInfo.name === 'style.css') return 'style.css'
          return 'assets/[name][extname]'
        },
      },
    },
    sourcemap: true,
    cssCodeSplit: false,  // 全部 CSS 合成一个
  },
})