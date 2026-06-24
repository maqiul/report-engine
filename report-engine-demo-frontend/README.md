# ReportEngine Demo Frontend

销售订单业务 demo 的 Vue 3 前端。

## 第三方项目集成演示

```bash
npm install @reportengine/vue
```

```ts
import { createApp } from 'vue'
import ReportEngineVue from '@reportengine/vue'
import '@reportengine/vue/style.css'

createApp(App).use(ReportEngineVue, { apiBase: '/api/reports' }).mount('#app')
```

```vue
<ReportViewer :templateJson="tpl" :data="data" :scale="0.9" />
```

## 启动

```bash
npm install
npm run dev    # 启动 Vite dev server :5174（代理 /api → :8080 后端）
```

需要后端 `report-engine-demo-backend` 在 `:8080` 跑着。

## 构建

```bash
npm run type-check    # vue-tsc 0 错误
npm run build         # 输出到 dist/
```
