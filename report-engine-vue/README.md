# @reportengine/vue

Vue 3 组件包：报表**预览** + 可视化**设计器** + 模板编辑器。

> 设计器（编辑 + 所见即所得预览）**完全跑在浏览器里，零后端依赖**。
> 需要 PDF/Excel 打印件时，再接一个 report-engine 渲染服务（.NET 或 Java）。

## 安装

```bash
npm install @reportengine/vue
```

本地开发引用本仓库源码：

```bash
npm install file:../report-engine-vue
```

## 快速开始

```ts
// src/main.ts
import { createApp } from 'vue'
import App from './App.vue'
import ReportEngineVue from '@reportengine/vue'
import '@reportengine/vue/style.css'

createApp(App)
  .use(ReportEngineVue, {
    apiBase: '/api/reports',  // 后端渲染服务地址（仅 ReportViewer 需要）
    theme: 'light',
    defaultFont: 'Microsoft YaHei',
  })
  .mount('#app')
```

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
  <!-- 设计器：v-model 绑定模板 JSON（.rptx 就是 JSON） -->
  <ReportEditor v-model="templateJson" :data="data" />
</template>
```

## 组件

### `<ReportEditor>` — 模板设计器
| Prop | 类型 | 说明 |
|------|------|------|
| `modelValue` | `string` | 模板 JSON（v-model 双向绑定） |
| `data` | `Record<string, any[]>` | 业务数据，用于预览与字段下拉 |

三种模式：**代码**（JSON 编辑）/ **可视化**（画布编辑）/ **预览**（所见即所得）。

### `<ReportViewer>` — 后端渲染预览（可选）
| Prop | 类型 | 说明 |
|------|------|------|
| `templateJson` | `string` | 模板 JSON |
| `data` | `Record<string, any[]>` | 业务数据 |
| `scale` | `number` | 缩放 |

调后端 `/api/reports/render/preview`，并支持导出 PDF/Excel。

### 子组件（按需引入）
`Toolbox` / `BandTree` / `PropertyPanel` / `TemplateEditor` / `PreviewCanvas` —— 想自己搭编辑器布局时用。

## 设计器能力

**13 种元素**（`type` 与后端 `ReportElementConverter` 完全对齐）
静态框 / 字段框 / 统计框 / 系统变量框 / 线段 / 图形框 / 图像框 / 条形码 / 二维码 / 表格 / 交叉表 / 图表 / 子报表

> 字段框/统计框/系统变量框都是 `type: "text"`，靠 `dataField` / `summaryFunction` / `systemVariable` 区分。

**画布**
- 拖拽移动、8 向缩放手柄、框选多选（Ctrl 切换 / Shift 范围选）
- 多选整体拖动、6 向对齐（按整体包围盒）
- 吸附（1.5mm 阈值，自动对齐其他元素边缘/中心/页边距 + 辅助线）
- 真标尺（顶/左 10mm 刻度，随滚动同步）、网格显示

**编辑**
- 撤销/重做（Ctrl+Z / Ctrl+Y）、复制/粘贴/剪切（Ctrl+C/V/X）
- 组合 / 取消组合（`groupId`）、锁定 / 解锁（`locked`）
- 格式刷（捕获样式后连续套用到多个目标）
- 方向键微调位置（1mm，Shift 为 5mm）、Delete 删除、Ctrl+D 复制
- 右键菜单：复制 / 删除 / 锁定 / 组合 / 层级调整

**属性**
- 每类型全属性（边框 `BorderDef` 四边、背景色、可见性、超链接、表格列宽行高、图表系列…）
- 条件格式规则（表达式 / 背景色 / 粗体）
- 表达式编辑器：字段下拉自动从实际数据提取

**文件**
- 新建 / 打开 `.rptx` / 下载 `.rptx` / 最近文件（localStorage）

**预览渲染（浏览器内）**
- 图表 → ECharts（柱/折/饼/面积/散点）
- 条形码 → JsBarcode，二维码 → qrcode
- 表格真网格、图像真图
- 表达式求值、数据绑定、统计汇总（Sum/Count/Avg/Max/Min）、detail 行展开

## 后端对接（可选，用于导出 PDF/Excel）

前端不产出 PDF。需要打印件时，你的项目里跑一个 report-engine 渲染服务
（.NET `ReportEngine.WebApi` 或 Java `report-engine-spring-boot-starter`），暴露：

| 接口 | 方法 | 说明 |
|------|------|------|
| `/api/reports/render/preview` | POST | 渲染预览（分页/元素） |
| `/api/reports/export/pdf` | POST | 导出 PDF（Blob） |
| `/api/reports/export/excel` | POST | 导出 Excel（Blob） |

请求体：`{ "templateJson": "{...}", "data": { "items": [...] } }`

```ts
import { getClient, downloadBlob } from '@reportengine/vue'

const client = getClient()
const blob = await client.exportPdf({ templateJson, data })
downloadBlob(blob, 'report.pdf')
```

## 表达式语法

| 语法 | .NET | Java | 说明 |
|------|:----:|:----:|------|
| `{{currentRow.field}}` | ✅ | ✅ | **跨端通用**，跨端模板请用这个 |
| `{{dataSource.field}}` | ✅ | ❌ | 仅 .NET |
| `{{field}}` | ✅ | ❌ | 仅 .NET（裸字段） |

系统变量：`PageNumber` / `TotalPages` / `PrintDate` / `PrintTime` / `ReportTitle`

## 开发

```bash
npm install
npm run type-check   # vue-tsc --noEmit
npm run build        # vite build && vue-tsc --emitDeclarationOnly
npm run dev          # 本地调试
```

> build 顺序不能反：`vite build` 会清空 `dist`（emptyOutDir），会把先前生成的 `.d.ts` 删掉。

## License

MIT
