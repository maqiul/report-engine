# ReportEngine

> 一个轻量级、跨平台的 .NET 报表引擎：JSON 模板 + 核心渲染 + Excel/PDF 导出 + WPF/WinForms 预览与设计器。

![status](https://img.shields.io/badge/status-alpha-orange)
![.net](https://img.shields.io/badge/.NET-net462%20%7C%20netstandard2.0%20%7C%20net8.0-blue)
![license](https://img.shields.io/badge/license-MIT-blue)

---

## ✨ 特性

- **跨平台核心库**：`ReportEngine.Core` 同时支持 `net462` / `netstandard2.0` / `net8.0`，可直接打包为 NuGet 包嵌入到现有 WinForms/WPF/控制台业务系统中。
- **JSON 模板格式 `.rptx`**：纯文本、易 diff、易版本管理。包含 Header/Detail/Footer、变量绑定 `{{...}}`、聚合函数、子报表、条码/二维码、字体/颜色/对齐等。
- **多端渲染**：
  - 桌面运行时预览 —— WPF / WinForms
  - 模板设计器 —— WPF（基础）
- **多格式导出**：
  - Excel —— 基于 [ClosedXML](https://github.com/ClosedXML/ClosedXML)
  - PDF —— 基于 [PdfSharpCore](https://github.com/ststeiger/PdfSharpCore)
- **条码/二维码** —— 基于 [ZXing.Net](https://github.com/micjahn/ZXing.Net)

---

## 📄 许可证

本项目基于 [MIT License](./LICENSE) 开源。

---

## 📦 项目结构

```
ReportEngine.slnx                  ← 根解决方案（新建）
├── ReportEngine.Core/             ← 跨平台核心：模板解析、渲染、表达式、子报表、条码
│   ├── Parsing/        TemplateParser
│   ├── Rendering/      ReportRenderer
│   ├── Data/           ExpressionEngine
│   ├── SubReports/     SubReportResolver
│   ├── Barcodes/       BarcodeGenerator
│   └── Export/         IExcelExporter / IPdfExporter（接口）
├── ReportEngine.Export.Excel/    ← ClosedXML 实现
├── ReportEngine.Export.Pdf/      ← PdfSharpCore 实现
├── ReportEngine.Viewer.Wpf/      ← WPF 运行时预览控件
├── ReportEngine.Viewer.WinForms/ ← WinForms 运行时预览控件
├── ReportEngine.Designer.Wpf/    ← WPF 可视化设计器（开发中）
├── ReportEngine.Designer.WinForms/ ← WinForms 可视化设计器（占位）
├── ReportEngine.Desktop/         ← 桌面启动器（exe，net8.0）
└── SampleTemplates/              ← 示例 .rptx 模板
    ├── sales_order.rptx
    └── order_detail.rptx
```

---

## 🚀 快速开始

### 环境要求

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)（含 net8.0 桌面运行时）
- Windows 10/11（运行 WPF/WinForms 端需要）
- Visual Studio 2022 17.8+ 或 JetBrains Rider 2023.3+

### 克隆与构建

```bash
git clone <repo-url> report-engine
cd report-engine
dotnet restore ReportEngine.slnx
dotnet build   ReportEngine.slnx -c Debug
```

### 运行桌面启动器

```bash
dotnet run --project ReportEngine.Desktop
```

### 在自己的项目里使用 Core

```csharp
// 1. 加载模板
var templateJson = File.ReadAllText("sales_order.rptx");
var template = new TemplateParser().Parse(templateJson);

// 2. 准备数据
var orders = new List<Dictionary<string, object?>>
{
    new() { ["orderNo"] = "SO-2026-0001", ["customer"] = "ACME", ... },
    // ...
};

// 3. 渲染并导出
var exporter = new ClosedXmlExporter();
exporter.Export(template, orders, "output.xlsx");
```

---

## 📝 模板格式 (.rptx) 示例

模板是 JSON，结构示意：

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
    {
      "name": "orders", "type": "json",
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
        { "type": "text", "text": "{{orders.orderNo}}",     "x": 10, "y": 1, "width": 35,  "height": 8 },
        { "type": "text", "text": "{{orders.totalAmount}}", "x": 130, "y": 1, "width": 30,  "height": 8,
          "alignment": "right" }
      ]
    }
  ]
}
```

完整示例见 [`SampleTemplates/sales_order.rptx`](./SampleTemplates/sales_order.rptx)。

---

## 🗓️ 版本与路线图

当前版本：**0.2.0-alpha**（仅 Core / Export 模块带包元数据；其他模块版本号未统一）

| 阶段 | 内容 | 状态 |
|------|------|------|
| 0.1.x | Core 引擎（解析 / 渲染 / 表达式 / 子报表） | ✅ |
| 0.2.x | Excel/PDF 导出 + ZXing 条码 | ✅ |
| 0.3.x | WPF 运行时预览（分页 / 缩放 / 打印） | 🚧 |
| 0.4.x | WPF 可视化设计器 | 🚧 |
| 0.5.x | WinForms 端对齐 | ⏳ |
| 1.0   | 文档、单元测试、首个稳定 NuGet 发布 | ⏳ |

---

## 🤝 贡献

欢迎 PR 和 Issue。当前最需要帮忙的方向：

1. 单元测试覆盖（特别是 `TemplateParser`、`ExpressionEngine`）
2. WPF 设计器的属性面板与拖拽画布
3. 文档：模板字段参考、API 参考、最佳实践
4. 多语言资源与 .editorconfig（统一 UTF-8）

---

<sub>Built with ❤️ using .NET 8 + ClosedXML + PdfSharpCore + ZXing.Net</sub>