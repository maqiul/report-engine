# ReportEngine Web

Web 版本的报表引擎，支持在线预览、打印、导出 PDF/Excel。

##  Java 库 (可复用)

核心渲染逻辑已抽成独立库 `java-lib`，其他项目可直接依赖：

```groovy
// Gradle
implementation 'com.reportengine:java-lib:0.2.0'
```

```xml
<!-- Maven -->
<dependency>
    <groupId>com.reportengine</groupId>
    <artifactId>java-lib</artifactId>
    <version>0.2.0</version>
</dependency>
```

### 使用示例

```java
// 渲染报表
ReportRenderer renderer = new ReportRenderer();
RenderResponse response = renderer.render(request);

// 导出 PDF
PdfExporter pdfExporter = new PdfExporter();
byte[] pdfBytes = pdfExporter.export(request);

// 导出 Excel
ExcelExporter excelExporter = new ExcelExporter();
byte[] excelBytes = excelExporter.export(request);
```

### 发布到本地

```bash
cd java-lib
../web/backend/gradlew.bat publishToMavenLocal
```

## 📁 项目结构

```
report-engine/
├── java-lib/               # Java 核心库 (可复用)
│   └── src/main/java/com/reportengine/lib/
│       ├── renderer/ReportRenderer.java
│       ├── exporter/{Pdf,Excel}Exporter.java
│       └── model/{RenderRequest,RenderResponse}.java
├── web/
│   ├── ReportEngine.WebApi/    # .NET 后端
│   ├── backend/                # Java 后端 (依赖 java-lib)
│   └── frontend/               # Vue 3 前端
```

##  快速开始

### 选择后端

项目提供两种后端实现，任选其一：

| 后端 | 技术栈 | 启动命令 |
|------|--------|----------|
| .NET | ASP.NET Core 8.0 | `dotnet run` |
| Java | Spring Boot 4.1 | `./gradlew bootRun` |

### 1. 启动后端 API

#### 选项 A：.NET 后端

```bash
cd web/ReportEngine.WebApi
dotnet run
```

#### 选项 B：Java 后端

```bash
cd web/backend
./gradlew bootRun
# 或
gradlew.bat bootRun  # Windows
```

后端将在 `http://localhost:5000` 启动。

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

##  开发

### .NET 后端开发

```bash
cd web/ReportEngine.WebApi
dotnet build
dotnet test
```

### Java 后端开发

```bash
cd web/backend
./gradlew build -x test    # 构建（跳过测试）
./gradlew bootRun          # 运行
./gradlew clean            # 清理
```

**Java 后端依赖：**
- Spring Boot 4.1.0
- com.reportengine:java-lib:0.2.0

### 前端开发

```bash
cd web/frontend
npm run dev          # 开发服务器
npm run build        # 生产构建
npm run type-check   # 类型检查
npm run lint         # 代码检查
```

##  部署

### .NET 后端

```bash
cd web/ReportEngine.WebApi
dotnet publish -c Release -o ./publish
```

### Java 后端

```bash
cd web/backend
./gradlew bootJar
# 生成 build/libs/backend-0.2.0.jar
java -jar build/libs/backend-0.2.0.jar
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
- [x] Java Spring Boot 后端
- [x] Java 核心库 (java-lib)
- [ ] 模板在线编辑
- [ ] 数据源配置界面
- [ ] 模板管理（上传/下载/列表）
- [ ] 用户认证与权限
- [ ] Docker 部署支持
