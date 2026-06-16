# ReportEngine

> 一个轻量级、跨平台的 .NET 报表引擎：JSON 模板 + 核心渲染 + Excel/PDF 导出 + WPF/WinForms 预览与设计器。

![status](https://img.shields.io/badge/status-alpha-orange)
![.net](https://img.shields.io/badge/.NET-net462%20%7C%20netstandard2.0%20%7C%20net8.0-blue)
![license](https://img.shields.io/badge/license-MIT-blue)
![tests](https://img.shields.io/badge/tests-4693%2F4693-brightgreen)
![ci](https://github.com/maqiul/report-engine/actions/workflows/ci.yml/badge.svg)

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

## 📦 NuGet 包（v0.2.0-alpha）

| 包 | 版本 | 目标框架 | 说明 |
|----|------|----------|------|
| `ReportEngine.Core` | `0.2.0-alpha` | net462 / netstandard2.0 / net8.0 | 核心引擎：模板解析、渲染、表达式、子报表、条码 |
| `ReportEngine.Export.Pdf` | `0.2.0-alpha` | net462 / netstandard2.0 / net8.0 | PDF 导出器（基于 PdfSharpCore） |
| `ReportEngine.Export.Excel` | `0.2.0-alpha` | net462 / netstandard2.0 / net8.0 | Excel 导出器（基于 ClosedXML） |

本地打包：

```bash
dotnet pack ReportEngine.Core/ReportEngine.Core.csproj -c Release -o ./nupkgs
dotnet pack ReportEngine.Export.Pdf/ReportEngine.Export.Pdf.csproj -c Release -o ./nupkgs
dotnet pack ReportEngine.Export.Excel/ReportEngine.Export.Excel.csproj -c Release -o ./nupkgs
```

> 版本号统一由 `Directory.Build.props` 管理，修改一处即可全局生效。

---

## 📄 许可证

本项目基于 [MIT License](./LICENSE) 开源。

---

## 📦 项目结构

```
ReportEngine.slnx
├── Directory.Build.props          ← 统一构建属性（版本号/NuGet元数据/编译选项）
├── ReportEngine.Core/             ← 跨平台核心：模板解析、渲染、表达式、子报表、条码
│   ├── Parsing/        TemplateParser
│   ├── Rendering/      ReportRenderer
│   ├── Data/           ExpressionEngine
│   ├── SubReports/     SubReportResolver / FileSystemTemplateResolver
│   ├── Barcodes/       BarcodeGenerator
│   └── Export/         IExcelExporter / IPdfExporter（接口）
├── ReportEngine.Export.Excel/    ← ClosedXML 实现
├── ReportEngine.Export.Pdf/      ← PdfSharpCore 实现
├── ReportEngine.Viewer.Wpf/      ← WPF 运行时预览控件
├── ReportEngine.Viewer.WinForms/ ← WinForms 运行时预览控件
├── ReportEngine.Designer.Wpf/    ← WPF 可视化设计器
│   ├── MainWindow.cs              ← 180 行（从 5050 行拆分，-96.4%）
│   ├── MainWindow.*.cs            ← 15 个 partial class 文件
│   ├── UiFactory / ElementFactory / ElementIcons
│   ├── CanvasRenderer / BandStyle / BrushParser
│   ├── PreviewRenderer / PreviewJsonParser / ExportDataBuilder
│   ├── PropertyRowFactory / EnumCnMap
│   ├── ColorPickerDialog / PageSetupDialog / FontDialog / ExpressionEditorDialog
│   └── ...
├── ReportEngine.Designer.WinForms/ ← WinForms 可视化设计器（占位）
├── ReportEngine.Desktop/         ← 桌面启动器（exe，net8.0）
├── tests/
│   ├── ReportEngine.Core.Tests/          ← 4575 测试
│   ├── ReportEngine.Export.Pdf.Tests/    ← 67 测试（含 17 个端到端）
│   └── ReportEngine.Export.Excel.Tests/  ← 51 测试（含 16 个端到端）
└── SampleTemplates/              ← 示例 .rptx 模板
```

---

## 🚀 快速开始

### 环境要求

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
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
var parser = new TemplateParser();
var template = parser.Parse(templateJson);

// 2. 准备数据
var data = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["orders"] = new()
    {
        new() { ["orderNo"] = "SO-2026-0001", ["customer"] = "ACME", ["totalAmount"] = 12345.67 },
        // ...
    }
};

// 3. 渲染
var resolver = new FileSystemTemplateResolver("./templates");
var renderer = new ReportRenderer(resolver);
var rendered = await renderer.RenderAsync(template, data);

// 4. 导出
new PdfSharpExporter().ExportToFile(rendered, "output.pdf");
new ClosedXmlExporter().ExportToFile(rendered, "output.xlsx");
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
        { "type": "text", "text": "{{currentRow.orderNo}}",     "x": 10, "y": 1, "width": 35,  "height": 8 },
        { "type": "text", "text": "{{currentRow.totalAmount}}", "x": 130, "y": 1, "width": 30,  "height": 8,
          "alignment": "right" }
      ]
    }
  ]
}
```

完整示例见 [`SampleTemplates/sales_order.rptx`](./SampleTemplates/sales_order.rptx)。

---

## ✅ 测试

**4693 个测试，全部通过。**

| 项目 | 测试数 | 覆盖范围 |
|------|--------|----------|
| `ReportEngine.Core.Tests` | 4575 | 模板解析、表达式引擎、条码生成、子报表、渲染路径、ClosedXML 导出 |
| `ReportEngine.Export.Pdf.Tests` | 67 | PdfSharpExporter + 17 个端到端集成测试 |
| `ReportEngine.Export.Excel.Tests` | 51 | ClosedXmlExporter + 16 个端到端集成测试 |

端到端测试覆盖完整流程：JSON 模板解析 → 渲染 → PDF/Excel 导出，包括多 Band 类型、多元素类型、大数据量、分组报表、表达式格式化、多数据源、子报表嵌套等场景。

运行测试：

```bash
dotnet test ReportEngine.slnx
```

---

## 🗓️ 版本与路线图

当前版本：**v0.1.245**

| 阶段 | 内容 | 状态 |
|------|------|------|
| 0.1.x | Core 引擎 + Excel/PDF 导出 + ZXing 条码 + NuGet pack + 代码重构 | ✅ |
| 0.2.x | WPF 运行时预览（分页 / 缩放 / 打印） | 🚧 |
| 0.3.x | WPF 可视化设计器完善 | 🚧 |
| 0.4.x | WinForms 端对齐 | ⏳ |
| 1.0   | 文档、首个稳定 NuGet 发布 | 🚧 |

### v0.1.x 已完成里程碑

| 指标 | 成果 |
|------|------|
| **MainWindow.cs 拆分** | 5050 → **180 行**（-96.4%），提取 15 个 partial class 文件 |
| **单元测试** | 100 → **4693**（+4593），含 33 个端到端集成测试 |
| **Nullable 警告** | 全部消除，**0 警告** |
| **NuGet 打包** | 3 个包统一版本 0.2.0-alpha |
| **CI/CD** | GitHub Actions 自动构建 + 测试 + 打包 |
| **代码规范** | `.editorconfig` + `Directory.Build.props` 统一管理 |
| **文档** | `LICENSE` / `README.md` / `CONTRIBUTING.md` 三件套 |

### v0.2+ 展望

- **v0.2.x** — WPF 运行时预览补完（分页 / 缩放 / 打印对话框）
- **v0.3.x** — WPF 可视化设计器完善 + Designer 单元测试
- **v0.4.x** — WinForms 端对齐
- **v1.0** — 首版稳定 NuGet 发布 + 完整文档站

---

## 🤝 贡献

欢迎 PR 和 Issue！详细开发流程、代码规范、提 PR 步骤请见 [`CONTRIBUTING.md`](./CONTRIBUTING.md)。

---

<sub>Built with ❤️ using .NET 8 + ClosedXML + PdfSharpCore + ZXing.Net</sub>
