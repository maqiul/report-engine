# 集成示例：第三方 Vue 项目接入 ReportEngine 报表设计器

这个示例就是一个**普通业务项目**，演示如何把 `@reportengine/vue` 装进自己的工程。
设计器（编辑 + 所见即所得预览）**完全跑在浏览器里，零后端依赖**。

## 1. 安装

```bash
npm install @reportengine/vue
# 本地开发用 file: 引用本仓库源码
npm install file:../report-engine-vue
```

## 2. 注册插件

```ts
// src/main.ts
import { createApp } from 'vue'
import App from './App.vue'
import ReportEngineVue from '@reportengine/vue'
import '@reportengine/vue/style.css'

createApp(App)
  .use(ReportEngineVue, {
    apiBase: '/api/reports',   // 后端渲染服务地址（仅 ReportViewer 预览/导出需要）
    theme: 'dark',
  })
  .mount('#app')
```

`use()` 之后全局注册了两个组件：`<ReportViewer>`（预览）、`<ReportEditor>`（设计器）。

## 3. 用设计器

```vue
<script setup lang="ts">
import { ref } from 'vue'

const templateJson = ref(JSON.stringify({
  version: '1.0',
  page: { width: 210, height: 297, unit: 'mm' },
  dataSources: [{ name: 'items', type: 'json' }],
  bands: [
    {
      type: 'detail', height: 10, dataSource: 'items',
      elements: [
        { type: 'text', text: '{{currentRow.name}}', x: 10, y: 1, width: 100, height: 8 }
      ]
    }
  ]
}, null, 2))

const data = ref({ items: [{ name: '张三', amount: 100 }] })
</script>

<template>
  <!-- v-model 双向绑定模板 JSON（rptx 就是 JSON） -->
  <ReportEditor v-model="templateJson" :data="data" />
</template>
```

## 4. 保存模板（纯前端）

```ts
function saveTemplate() {
  const blob = new Blob([templateJson.value], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'my-report.rptx'
  a.click()
  URL.revokeObjectURL(url)
}
```

## 5. 可选：接后端出 PDF/Excel

设计器不产出 PDF。要出打印件，需要项目里有 report-engine 渲染服务
（.NET `ReportEngine.WebApi` 或 Java `report-engine-spring-boot-starter`），暴露三个接口：

| 接口 | 方法 | 说明 |
|------|------|------|
| `/api/reports/render/preview` | POST | 渲染预览（分页/元素） |
| `/api/reports/export/pdf` | POST | 导出 PDF（Blob） |
| `/api/reports/export/excel` | POST | 导出 Excel（Blob） |

请求体统一为：

```json
{ "templateJson": "{...}", "data": { "items": [ { "name": "张三" } ] } }
```

用库自带的客户端：

```ts
import { getClient, downloadBlob } from '@reportengine/vue'

const client = getClient()
const blob = await client.exportPdf({ templateJson: templateJson.value, data: data.value })
downloadBlob(blob, 'my-report.pdf')
```

## 设计器能力一览
- 13 种元素：静态框/字段框/统计框/系统变量框/线段/图形/图像/条形码/二维码/表格/交叉表/图表/子报表
- 画布：拖拽移动、8 向缩放、框选多选、吸附辅助线、真标尺、网格
- 编辑：撤销/重做、复制/粘贴/剪切、6 向对齐、组合/锁定、格式刷、每类型全属性、条件格式
- 文件：新建/打开 .rptx/下载 .rptx/最近文件
- 预览：浏览器内渲染（图表用 ECharts、条码 JsBarcode、二维码 qrcode）

## 跑起来

```bash
cd report-engine-vue-sample
npm install
npm run dev
```
