import { createApp } from 'vue'
import App from './App.vue'

// 关键：第三方项目集成方式
import ReportEngineVue from '@reportengine/vue'
import '@reportengine/vue/style.css'

createApp(App)
  .use(ReportEngineVue, {
    apiBase: '/api/reports',  // 后端 starter 自动暴露
    theme: 'dark',
  })
  .mount('#app')