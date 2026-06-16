# ReportEngine Web

Web 版本的报表引擎，支持在线预览、打印、导出 PDF/Excel。

## 📁 项目结构

```
web/
├── ReportEngine.WebApi/    # ASP.NET Core Web API 后端
│   ├── Controllers/
│   │   ├── RenderController.cs   # 渲染预览 API
│   │   └── ExportController.cs   # 导出 PDF/Excel API
│   └── Models/
│       └── RenderModels.cs       # 请求/响应模型
└── frontend/               # Vue 3 + TypeScript 前端
    └── src/
        ├── api/report.ts         # API 调用封装
        └── components/
            └── ReportViewer.vue  # 报表预览组件
```

## 🚀 快速开始

### 1. 启动后端 API

```bash
cd web/ReportEngine.WebApi
dotnet run
```

后端将在 `http://localhost:5000` 启动，Swagger 文档：`http://localhost:5000/swagger`

### 2. 启动前端

```bash
cd web/frontend
npm install
npm run dev
```

前端将在 `http://localhost:5173` 启动。

## 📡 API 接口

### 预览报表

```http
POST /api/render/preview
Content-Type: application/json

{
  "templateJson": "{...}",
  "data": {
    "orders": [
      { "orderNo": "SO-001", "customer": "张三", "amount": 1234.56 }
    ]
  }
}
```

### 导出 PDF

```http
POST /api/export/pdf
Content-Type: application/json

{ "templateJson": "{...}", "data": {...} }
```

返回 `application/pdf` 文件。

### 导出 Excel

```http
POST /api/export/excel
Content-Type: application/json

{ "templateJson": "{...}", "data": {...} }
```

返回 `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` 文件。

## 🎨 前端组件

### ReportViewer

报表预览组件，支持：
- ✅ 在线预览（分页显示）
- ✅ 翻页导航
- ✅ 缩放控制
- ✅ 浏览器打印
- ✅ 导出 PDF
- ✅ 导出 Excel

### 使用示例

```vue
<template>
  <ReportViewer
    :template-json="templateJson"
    :data="data"
    :scale="0.8"
  />
</template>

<script setup lang="ts">
import ReportViewer from './components/ReportViewer.vue'

const templateJson = `{...}`
const data = {
  orders: [
    { orderNo: 'SO-001', customer: '张三', amount: 1234.56 }
  ]
}
</script>
```

### 组件方法

```ts
const viewerRef = ref<InstanceType<typeof ReportViewer> | null>(null)

// 手动刷新
viewerRef.value?.loadReport()

// 打印
viewerRef.value?.print()

// 导出
viewerRef.value?.exportPdf()
viewerRef.value?.exportExcel()
```

## 📋 模板格式

模板是 JSON 格式，与桌面版 `.rptx` 文件相同：

```json
{
  "version": "1.0",
  "page": {
    "width": 210,
    "height": 297,
    "margin": { "top": 15, "bottom": 15, "left": 10, "right": 10 }
  },
  "dataSources": [{ "name": "orders" }],
  "bands": [
    {
      "type": "reportHeader",
      "height": 30,
      "elements": [{
        "type": "text",
        "text": "销售订单报表",
        "x": 10, "y": 10, "width": 190, "height": 15,
        "font": { "size": 20, "bold": true },
        "alignment": "center"
      }]
    },
    {
      "type": "detail",
      "height": 8,
      "dataSource": "orders",
      "elements": [
        { "type": "text", "text": "{{currentRow.orderNo}}", "x": 10, "y": 0, "width": 40, "height": 6 },
        { "type": "text", "text": "{{currentRow.amount}}", "x": 130, "y": 0, "width": 60, "height": 6, "alignment": "right" }
      ]
    }
  ]
}
```

## 🔧 开发

### 后端开发

```bash
cd web/ReportEngine.WebApi
dotnet build
dotnet test
```

### 前端开发

```bash
cd web/frontend
npm run dev          # 开发服务器
npm run build        # 生产构建
npm run type-check   # 类型检查
npm run lint         # 代码检查
```

## 📦 部署

### 后端

```bash
cd web/ReportEngine.WebApi
dotnet publish -c Release -o ./publish
```

### 前端

```bash
cd web/frontend
npm run build
# 将 dist/ 目录部署到静态文件服务器
```

### Docker（待实现）

```dockerfile
# TODO: 添加 Dockerfile
```

## 🗺️ 路线图

- [x] 基础预览功能
- [x] PDF 导出
- [x] Excel 导出
- [x] 浏览器打印
- [ ] 模板在线编辑
- [ ] 数据源配置界面
- [ ] 模板管理（上传/下载/列表）
- [ ] 用户认证与权限
- [ ] Docker 部署支持
