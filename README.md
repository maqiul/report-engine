# ReportEngine

> 一个跨平台、可视化、可打包分发的报表引擎：JSON 模板 + 核心渲染 + Excel/PDF 导出 + WPF/WinForms/Web 多端预览与设计器。

![status](https://img.shields.io/badge/status-v0.3.0-brightgreen)
![.net](https://img.shields.io/badge/.NET-net462%20%7C%20netstandard2.0%20%7C%20net8.0-blue)
![java](https://img.shields.io/badge/Java-Spring%20Boot%204.1-orange)
![npm](https://img.shields.io/badge/npm-%40reportengine%2Fvue-red)
![docker](https://img.shields.io/badge/docker-compose-blue)
![license](https://img.shields.io/badge/license-MIT-blue)
![tests](https://img.shields.io/badge/tests-4875%2F4875-brightgreen)

---

## ✨ 特性

- **三件套 npm/Maven 生态**（让别人项目 4 步集成）：
  - ☕ **后端 starter** `com.reportengine:report-engine-spring-boot-starter:0.3.0` —— Spring Boot 3 一行 `@Autowired ReportEngine`
  - 📦 **前端组件包** `@reportengine/vue@0.3.0` —— Vue 3 一行 `app.use(ReportEngineVue)`，`<ReportViewer>` + `<ReportEditor>` 即用
  - 🔧 **核心库** `com.reportengine:java-lib:0.3.0` —— 无 Spring 依赖的纯 JVM 库
- **跨平台核心库**：
  - .NET 端：`ReportEngine.Core` 同时支持 `net462` / `netstandard2.0` / `net8.0`，可直接打包为 NuGet 包嵌入到现有 WinForms/WPF/控制台业务系统中。
  - Java 端：`com.reportengine:java-lib` 独立库，发布到 Maven Local，可被任意 JVM 项目依赖。
- **JSON 模板格式 `.rptx`**：纯文本、易 diff、易版本管理。包含 Header/Detail/Footer、变量绑定 `{{...}}`、聚合函数、子报表、条码/二维码、字体/颜色/对齐等。
- **多端渲染**：
  - 桌面运行时预览 —— WPF / WinForms
  - Web 端运行时预览 —— Vue 3 + Spring Boot
  - 模板设计器 —— WPF（基础）/ Web（可视化拖拽）
- **多格式导出**：
  - Excel —— .NET 端用 [ClosedXML](https://github.com/ClosedXML/ClosedXML)，Java 端用 [Apache POI](https://poi.apache.org/)
  - PDF —— .NET 端用 [PdfSharpCore](https://github.com/ststeiger/PdfSharpCore)，Java 端用 [iText 7](https://itextpdf.com/)
- **条码/二维码** —— .NET 端用 [ZXing.Net](https://github.com/micjahn/ZXing.Net)
- **容器化部署** —— `docker compose up -d` 一键启动前后端
- **OpenAPI 文档** —— Springdoc OpenAPI 自动生成 API 契约
- **数据库持久化** —— Spring Data JPA + H2 嵌入式数据库，模板 CRUD

---

## 🐳 一键启动（最快路径）

环境要求：Docker Desktop 4.0+（Windows / macOS / Linux）

```bash
git clone https://github.com/maqiul/report-engine.git
cd report-engine/docker
docker compose up -d
```

启动后访问：

- 前端（含可视化编辑器）：http://localhost:3000
- 后端 API：http://localhost:5000
- Swagger UI：http://localhost:5000/swagger-ui.html
- OpenAPI JSON：http://localhost:5000/v3/api-docs

停止：

```bash
docker compose down
```

详见 [`docker/README.md`](./docker/README.md)。

---

## 📦 包与产物（v0.3.0）

### ☕ Java Maven 库（发布到 Maven Local）

| 包 | 版本 | 说明 |
|----|------|------|
| `com.reportengine:java-lib` | `0.3.0` | 核心库（无 Spring 依赖），含 ReportEngine 门面 + TemplateStore SPI |
| `com.reportengine:report-engine-spring-boot-starter` | `0.3.0` | Spring Boot 3 starter，自动配置 + ReportEngine Bean + REST 端点 |

### 📦 前端 npm 包

| 包 | 版本 | 说明 |
|----|------|------|
| `@reportengine/vue` | `0.3.0` | Vue 3 组件包，Plugin install + `<ReportViewer>` + `<ReportEditor>` |

### 🟦 .NET NuGet 包（0.2.0-alpha）

| 包 | 版本 | 目标框架 | 说明 |
|----|------|----------|------|
| `ReportEngine.Core` | `0.2.0-alpha` | net462 / netstandard2.0 / net8.0 | 核心引擎：模板解析、渲染、表达式、子报表、条码 |
| `ReportEngine.Export.Pdf` | `0.2.0-alpha` | net462 / netstandard2.0 / net8.0 | PDF 导出器（基于 PdfSharpCore） |
| `ReportEngine.Export.Excel` | `0.2.0-alpha` | net462 / netstandard2.0 / net8.0 | Excel 导出器（基于 ClosedXML） |

### 发布到 Maven Local

```bash
cd java-lib && gradlew publishToMavenLocal
cd ../report-engine-starter && gradlew publishToMavenLocal
```

### Docker 镜像

| 镜像 | 标签 | 大小 |
|------|------|------|
| `reportengine-backend` | `0.2.0` | ~280MB（Eclipse Temurin 17 JRE） |
| `reportengine-frontend` | `0.2.0` | ~50MB（nginx alpine） |

---

## 🏗️ 项目结构

```
report-engine/
├── Directory.Build.props          ← .NET 统一构建属性（版本号/NuGet 元数据）
├── ReportEngine.Core/             ← .NET 跨平台核心
├── ReportEngine.Export.Pdf/       ← .NET PDF 导出
├── ReportEngine.Export.Excel/     ← .NET Excel 导出
├── ReportEngine.Viewer.Wpf/       ← WPF 运行时预览
├── ReportEngine.Viewer.WinForms/  ← WinForms 运行时预览
├── ReportEngine.Designer.Wpf/     ← WPF 可视化设计器（180 行 MainWindow）
├── ReportEngine.Desktop/          ← 桌面启动器
├── tests/                         ← .NET 测试（4693 个）
├── java-lib/                      ← Java 核心库（无 Spring 依赖）
│   ├── src/main/java/com/reportengine/lib/
│   │   ├── model/        RenderRequest / RenderResponse / PageInfo / ElementInfo
│   │   ├── renderer/     ReportRenderer
│   │   ├── exporter/     PdfExporter / ExcelExporter
│   │   ├── store/        TemplateStore SPI
│   │   └── ReportEngine  ← 门面类
│   └── src/test/java/    ← JUnit 5 + Mockito + AssertJ（64 个测试）
├── report-engine-starter/         ← ☕ Spring Boot 3 starter（让别人 @Autowired）
│   ├── src/main/java/com/reportengine/starter/
│   │   ├── ReportEngineAutoConfiguration
│   │   ├── ReportRestController  ← /api/reports/**
│   │   ├── ReportEngineProperties
│   │   └── ClasspathTemplateStore
│   ├── src/main/resources/META-INF/spring/
│   │   └── org.springframework.boot.autoconfigure.AutoConfiguration.imports
│   └── src/test/java/    ← 10 个 starter 集成测试
├── report-engine-starter-sample/  ← 模拟'第三方 Spring Boot 项目'集成验证
├── report-engine-vue/             ← 📦 Vue 3 前端组件包（npm @reportengine/vue）
│   ├── src/index.ts     ← Vue Plugin install
│   ├── src/api/report.ts  ← ReportClient
│   ├── src/components/   ← ReportViewer + ReportEditor + editor/*
│   └── src/types.ts
├── report-engine-vue-sample/      ← 模拟'第三方 Vue 项目'集成验证
├── web/                           ← Web 版本（Docker 一键部署）
│   ├── frontend/         ← Vue 3 + Vite + TypeScript（开发版）
│   ├── backend/          ← Java Spring Boot 4.1 + Gradle（开发版）
│   └── ReportEngine.WebApi/  ← .NET 端 Web API（备选实现）
├── docker/                        ← Docker 编排
│   ├── docker-compose.yml
│   ├── backend/Dockerfile
│   ├── frontend/Dockerfile
│   ├── frontend/nginx.conf
│   └── fonts/STSONG.TTF           ← 11MB 中文字体
└── SampleTemplates/               ← 示例 .rptx 模板
```

---

## 🚀 快速开始

### Web 版本（Java 后端 + Vue 前端）

```bash
# 1. 启动容器
cd docker && docker compose up -d

# 2. 打开浏览器
#    前端：http://localhost:3000
#    Swagger：http://localhost:5000/swagger-ui.html
```

### Web 版本（.NET 后端，本地开发）

```bash
# 前端
cd web/frontend
npm install
npm run dev          # localhost:3000

# .NET 后端
dotnet run --project web/ReportEngine.WebApi   # localhost:5000
```

### 桌面版本（Windows）

```bash
dotnet run --project ReportEngine.Desktop
```

### 在自己的项目里使用 Core（.NET）

```csharp
// 1. 加载模板
var templateJson = File.ReadAllText("sales_order.rptx");
var parser = new TemplateParser();
var template = parser.Parse(templateJson);

// 2. 准备数据
var data = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["orders"] = new()
    {
        new() { ["orderNo"] = "SO-2026-0001", ["customer"] = "ACME", ["totalAmount"] = 12345.67 }
    }
};

// 3. 渲染 + 导出
var resolver = new FileSystemTemplateResolver("./templates");
var renderer = new ReportRenderer(resolver);
var rendered = await renderer.RenderAsync(template, data);
new PdfSharpExporter().ExportToFile(rendered, "output.pdf");
new ClosedXmlExporter().ExportToFile(rendered, "output.xlsx");
```

### 在自己的项目里使用 java-lib（Java）

```java
// 1. 构造请求
RenderRequest request = new RenderRequest();
request.setTemplateJson("{\"bands\":[...]}");
Map<String, List<Map<String, Object>>> data = new HashMap<>();
data.put("orders", List.of(Map.of("id", "1", "name", "张三")));
request.setData(data);

// 2. 渲染
ReportRenderer renderer = new ReportRenderer();
RenderResponse response = renderer.render(request);

// 3. 导出
byte[] pdfBytes = new PdfExporter().export(request);
byte[] excelBytes = new ExcelExporter().export(request);
```

### 集成 Spring Boot Starter（Java 后端项目）

```xml
<!-- pom.xml -->
<dependency>
    <groupId>com.reportengine</groupId>
    <artifactId>report-engine-spring-boot-starter</artifactId>
    <version>0.3.0</version>
</dependency>
```

```java
// 业务代码
@RestController
public class ReportController {
    @Autowired
    private ReportEngine engine;  // 自动注入，无需任何配置

    @PostMapping("/my-report")
    public byte[] export(@RequestBody RenderRequest req) throws Exception {
        return engine.exportPdf(req);
    }
}
```

自动获得：
- Bean：`ReportEngine` / `ReportRenderer` / `PdfExporter` / `ExcelExporter` / `TemplateStore`
- REST 端点：`/api/reports/{render/preview, export/pdf, export/excel, render-by-id/{id}, health}`

### 集成 @reportengine/vue（Vue 3 前端项目）

```bash
npm install @reportengine/vue
```

```ts
// main.ts
import { createApp } from 'vue'
import App from './App.vue'
import ReportEngineVue from '@reportengine/vue'
import '@reportengine/vue/style.css'

createApp(App)
  .use(ReportEngineVue, { apiBase: '/api/reports' })
  .mount('#app')
```

```vue
<!-- 任何 .vue 文件 -->
<template>
  <ReportViewer :templateJson="tpl" :data="data" :scale="0.8" />
  <ReportEditor v-model:template="tpl" :data="data" />
</template>
```

---

## 📝 模板格式 (.rptx) 示例

```jsonc
{
  "version": "1.0",
  "page": {
    "width": 210, "height": 297,
    "unit": "mm",
    "orientation": "portrait",
    "margin": { "top": 15, "bottom": 15, "left": 10, "right": 10 }
  },
  "dataSources": [
    { "name": "orders", "type": "json",
      "fields": [
        { "name": "orderNo", "type": "string" },
        { "name": "totalAmount", "type": "number" }
      ]
    }
  ],
  "bands": [
    {
      "type": "reportHeader", "height": 35,
      "elements": [
        { "type": "text", "text": "销售订单报表", "x": 10, "y": 5,
          "width": 190, "height": 12,
          "font": { "family": "Microsoft YaHei", "size": 18, "bold": true } }
      ]
    },
    {
      "type": "detail", "height": 10, "dataSource": "orders",
      "elements": [
        { "type": "text", "text": "{{currentRow.orderNo}}",     "x": 10, "y": 1, "width": 35, "height": 8 },
        { "type": "text", "text": "{{currentRow.totalAmount}}", "x": 130, "y": 1, "width": 30, "height": 8,
          "alignment": "right" }
      ]
    },
    {
      "type": "pageFooter", "height": 15,
      "elements": [
        { "type": "text", "text": "第 {{page}} / {{totalPages}} 页", "x": 0, "y": 2, "width": 210, "height": 8,
          "alignment": "center" }
      ]
    }
  ]
}
```

完整示例见 [`SampleTemplates/sales_order.rptx`](./SampleTemplates/sales_order.rptx)。

---

## ✅ 测试

**4875+ 个测试 + 集成验证，全部通过。**

| 项目 | 数量 | 说明 |
|------|------|------|
| `ReportEngine.Core.Tests` | 4575 | .NET 核心 |
| `ReportEngine.Export.Pdf.Tests` | 67 | .NET PDF（含 17 个 E2E） |
| `ReportEngine.Export.Excel.Tests` | 51 | .NET Excel（含 16 个 E2E） |
| `java-lib` (JUnit 5) | 64 | Java 核心（模型/渲染/PDF/Excel） |
| `report-engine-starter` (Spring Boot Test) | 10 | starter 集成测试（自动配置 + REST 端点） |
| `report-engine-starter-sample` | 5 | 模拟第三方 Spring Boot 项目集成 |
| `web/backend` (Spring Boot Test) | 22 | Java 后端 E2E（11 render + 11 template CRUD） |

覆盖率（java-lib JaCoCo）：**90%**（model 100% / renderer 97% / exporter 86%）

### .NET 测试

```bash
dotnet test ReportEngine.slnx
```

### Java 测试

```bash
# java-lib
cd java-lib
gradlew test jacocoTestReport
# 报告：build/reports/jacoco/test/html/index.html

# starter
cd report-engine-starter
gradlew test

# web/backend
cd web/backend
gradlew test
```

### 前端包 build 验证

```bash
# 前端组件包
cd report-engine-vue
npm install && npm run build   # vue-tsc + vite build

# 模拟第三方 Vue 项目
cd ../report-engine-vue-sample
npm install && npx vite build   # 验证组件可被外部项目正常导入
```

---

## 🗓️ 版本与路线图

当前版本：**v0.3.0**

| 阶段 | 内容 | 状态 |
|------|------|------|
| 0.1.x | .NET 核心 + Excel/PDF 导出 + ZXing 条码 + NuGet pack + 代码重构 | ✅ |
| 0.2.x | Java 后端 + Web 前端 + Docker + 可视化编辑器 + OpenAPI | ✅ |
| **0.3.x** | **数据库持久化 + Spring Boot Starter（@Autowired ReportEngine）+ 前端 npm 包（app.use）** | ✅ |
| 0.4.x | 用户/权限/多租户（spring-security）+ 登录页 | ⏳ |
| 1.0   | 文档站 + 首版稳定 NuGet/Maven/npm 发布 | 🚧 |

### v0.3 里程碑

| 指标 | 成果 |
|------|------|
| **数据库持久化** | Spring Data JPA + H2 嵌入式，Template CRUD + 版本号自增 + 11 个 E2E 测试 |
| **后端 starter** | `com.reportengine:report-engine-spring-boot-starter:0.3.0` 自动配置 + 自动 REST 端点 + ReportEngine 门面 Bean |
| **前端 npm 包** | `@reportengine/vue@0.3.0` Vue Plugin + `<ReportViewer>` + `<ReportEditor>` + ReportClient + 完整 .d.ts |
| **核心库升级** | `com.reportengine:java-lib:0.3.0` 新增 ReportEngine 门面 + TemplateStore SPI |
| **别人集成方式** | 后端 `implementation` + 前端 `npm install` + `app.use(ReportEngineVue)` 4 步搞定 |
| **测试** | java-lib 64 + starter 10 + starter-sample 5 + 后端 E2E 22 = **101 Java 全绿**（.NET 4693 + Web E2E 11 + 新增 sample 验证共 **4875+**） |

### v0.2 里程碑

| 指标 | 成果 |
|------|------|
| **Java 后端** | Spring Boot 4.1 + Gradle 9.5 + iText 7 + Apache POI，3 端点（preview/pdf/excel） |
| **Java 库** | `com.reportengine:java-lib:0.2.0` 发布到 Maven Local，可被任意 JVM 项目依赖 |
| **Web 前端** | Vue 3 + Vite + TypeScript，3 模式（代码/可视化/预览） |
| **可视化编辑器** | Toolbox + BandTree + PropertyPanel + TemplateEditor，4 元素/4 band，原生拖拽 + 8 方向缩放 |
| **Docker** | `docker-compose.yml` 一键编排前后端，含 healthcheck + depends_on + 中文字体 |
| **OpenAPI** | springdoc 2.6，3 端点 + 5 schema，UI 自动生成 |
| **E2E 测试** | 11 个 Spring Boot MockMvc 测试，零回归 |
| **跨平台字体** | `PdfExporter.resolveFontPath()` 支持 env / cwd / classpath / Windows / Linux 五级 fallback |

---

## 🛠️ 技术栈

### .NET 端
- .NET 8 / netstandard2.0 / net462
- ClosedXML 0.104（Excel）
- PdfSharpCore 1.3.65（PDF）
- ZXing.Net 0.16.10（条码）
- WPF / WinForms

### Java 端
- JDK 17+
- Spring Boot 4.1.0
- iText 7（PDF）
- Apache POI（Excel）
- JUnit 5 + Mockito + AssertJ（测试）
- springdoc-openapi 2.6（API 文档）
- JaCoCo（覆盖率）

### Web 前端
- Vue 3 + Vite + TypeScript
- 原生 HTML5 鼠标事件（拖拽/缩放）
- 无第三方 UI 框架

### 部署
- Docker 29 + Docker Compose v2
- nginx 1.27 alpine
- Eclipse Temurin 17 JRE

---

## 🤝 贡献

欢迎 PR 和 Issue！详细开发流程、代码规范、提 PR 步骤请见 [`CONTRIBUTING.md`](./CONTRIBUTING.md)。

---

## 📄 许可证

本项目基于 [MIT License](./LICENSE) 开源。

---

<sub>Built with ❤️ using .NET 8 + Java 17 + Vue 3 + Docker · v0.2.0-web</sub>
