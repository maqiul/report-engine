import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import ReportEngineVue from '@reportengine/vue'
import '@reportengine/vue/style.css'

import OrderList from './views/OrderList.vue'
import OrderDetail from './views/OrderDetail.vue'
import ReportDashboard from './views/ReportDashboard.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: OrderList, name: 'list' },
    { path: '/orders/:id', component: OrderDetail, name: 'detail', props: true },
    { path: '/reports', component: ReportDashboard, name: 'reports' }
  ]
})

const app = createApp(App)
app.use(router)
// @reportengine/vue Plugin：提供 ReportClient
app.use(ReportEngineVue, { apiBase: '/api/reports' })
app.mount('#app')
