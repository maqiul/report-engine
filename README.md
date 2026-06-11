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
│   ├── MainWindow.cs              ← 3855 行（已从 5050 拆出 1195 行）
│   ├── UiFactory.cs               ←   工具栏/菜单/窗口静态构造
│   ├── ElementFactory.cs          ←   元素类型 → 控件/图标映射
│   ├── ElementIcons.cs            ←   内联 SVG 图标资源
│   ├── BrushParser.cs             ←   #RRGGBB → Brush
│   ├── CanvasRenderContext.cs     ←   画布渲染上下文（缩放/选中/拖拽）
│   ├── BandStyle.cs               ←   模板元素命名/类型映射
│   ├── CanvasRenderer.cs          ←   模板画布绘制
│   ├── PreviewRenderer.cs         ←   预览数据值解析
│   ├── PreviewJsonParser.cs       ←   简易 JSON 解析
│   ├── ExportDataBuilder.cs       ←   导出数据打包
│   ├── EnumCnMap.cs               ←   枚举 ↔ 中文显示名映射
│   ├── ColorPickerDialog.cs       ←   颜色选择弹窗
│   └── PageSetupDialog.cs         ←   页面设置弹窗
├── ReportEngine.Designer.WinForms/ ← WinForms 可视化设计器（占位）
├── ReportEngine.Desktop/         ← 桌面启动器（exe，net8.0）
├── tests/
│   └── ReportEngine.Core.Tests/  ← xUnit + FluentAssertions（net8.0，40/0/0）
│       ├── TemplateParserTests         (4)
│       ├── ExpressionEngineTests       (8)
│       ├── BarcodeGeneratorTests       (9)
│       ├── SubReportResolverTests      (7)
│       ├── ClosedXmlExporterTests      (6)
│       └── ReportRendererTests         (6)
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

## ✅ 单元测试

两个独立测试项目，目标框架锁 `net8.0`：

| 项目 | 覆盖范围 | 测试数 |
|------|----------|--------|
| `ReportEngine.Core.Tests` | 模板解析 / 表达式引擎（系统变量/聚合/DotPath/FieldFormat）/ ZXing 条码 / 子报表解析（FileSystem + Composite）/ ClosedXML 导出 / 渲染主路径 + ReportRenderer 边界（ReportHeader / PageHeader / SubBands / 多栏 / 极小页面） | **70** |
| `ReportEngine.Export.Pdf.Tests`（v0.1.7 新增）| `PdfSharpExporter` 主路径 — text / line / shape / image 断源 / barcode / border / 多页 / 磁盘文件 | **10** |

**总计 80 个测试**，全绿。运行：

```bash
dotnet test ReportEngine.slnx
```

> 设计器侧（WPF 预览 / WinForms 设计器）暂未独立测试，跨项目引用复杂，待 v0.4.x 加 net8.0 Designer 测试项目后再补。

---

## 🗓️ 版本与路线图

当前版本：**v0.1.13**（34 commits / 13 tags；详细里程碑见 `git tag -l`）

| 阶段 | 内容 | 状态 |
|------|------|------|
| 0.1.x | Core 引擎（解析 / 渲染 / 表达式 / 子报表） | ✅ |
| 0.2.x | Excel/PDF 导出 + ZXing 条码 | ✅ |
| 0.3.x | WPF 运行时预览（分页 / 缩放 / 打印） | 🚧 |
| 0.4.x | WPF 可视化设计器 | 🚧（MainWindow 已从 5050 → 3335 行） |
| 0.5.x | WinForms 端对齐 | ⏳ |
| 1.0   | 文档、单元测试、首个稳定 NuGet 发布 | 🚧（已 80 个测试） |

### 🗺️ v0.2 详细路线图

**v0.2 目标**：把 `MainWindow.cs` 进一步拆到 < 3000 行；把测试覆盖扩到 100 条；把文档化补齐。

#### 已识别候选（按 ROI 排序）

| 优先级 | 任务 | 预估影响 | 备注 |
|--------|------|----------|------|
| 🥇 P0 | **Step4.A** 抽 8 个 `AddProp*` 工厂方法 | MainWindow -175 行 | 需把 `_propertyStack` / `_currentExpander` / `_propRowIndex` 打包进 `PropertyRowContext` |
| 🥇 P0 | **D2** 补 ClosedXmlExporter 边界用例（条件格式 / 嵌套表） | +6 测试 | `ConditionalFormatRule` 覆盖 |
| 🥈 P1 | **A1** 补 SubReport/Barcode 集成测试（端到端） | +4 测试 | 跨模块的 `RenderedReport` 集成 |
| 🥈 P1 | **Step4.B** 抽 `LoadPreviewData` + `ShowShortcutsDialog` + 杂项弹窗 | MainWindow -120 行 | 单调用方，走 Step3.B 模式 |
| 🥉 P2 | **D3** 补 `ReportRenderer` 边界用例（空模板 / 嵌套子报表 / 异常输入） | +5 测试 | 提高 Core 健壮性 |
| 🥉 P2 | **D4** 补 `ExpressionEngine` 边界用例（嵌套表达式 / 类型推断） | +4 测试 | 表达式语法边界 |
| ⏳ P3 | **D5** 补 Designer.Wpf 测试项目（需先加 net8.0 Designer 测试项目） | +N 测试 | 跨项目引用复杂，等 v0.4.x |
| ⏳ P3 | **v0.4.x** MainWindow 拆解 Stage 2（事件处理 / 拖拽 / 选中） | MainWindow -1000 行 | 深度耦合 WPF 状态，ROI 中等 |

#### v0.2 验收标准

- [ ] MainWindow.cs ≤ 3000 行
- [ ] 单元测试 ≥ 100 条（全绿）
- [ ] `ReportRenderer` / `ExpressionEngine` 边界用例覆盖
- [ ] `CONTRIBUTING.md` 已发布
- [ ] `LICENSE` / `README.md` / `CHANGELOG.md` 三件套到位
- [ ] 首次 `dotnet pack` 能产出 NuGet 包（`ReportEngine.Core` 0.3.x + `ReportEngine.Export.Excel` + `ReportEngine.Export.Pdf`）

#### v0.3+ 展望（暂不排期）

- **v0.3.x** — WPF 运行时预览补完（分页 / 缩放 / 打印对话框）
- **v0.4.x** — WPF 可视化设计器剩余拆解 + Designer 单元测试
- **v0.5.x** — WinForms 端对齐
- **v1.0** — 首版稳定 NuGet 发布 + 完整文档站（GitHub Pages / DocFX）

### v0.1.1 拆 `MainWindow.cs` 进度

`ReportEngine.Designer.Wpf/MainWindow.cs` 在 v0.1.0 起步时是个 271 KB / 5050 行的"巨无霸"，
XAML 设计器自动生成的代码 + 业务逻辑全堆在一起。v0.1.9 已按职责拆出 16 个独立文件：

| 步骤 | 拆出 | 文件 | 行数变化 |
|------|------|------|----------|
| Step 1 | 工具栏/菜单/元素工厂/图标 | `UiFactory.cs` / `ElementFactory.cs` / `ElementIcons.cs` | 5050 → 4948（-102） |
| Step 2.A | 画布渲染 + 预览值解析 | `BrushParser.cs` / `CanvasRenderContext.cs` / `BandStyle.cs` / `CanvasRenderer.cs` / `PreviewRenderer.cs` | 4948 → 4277（-671） |
| Step 2.B | JSON 解析 + 导出数据打包 | `PreviewJsonParser.cs` / `ExportDataBuilder.cs` | 4277 → 4234（-43） |
| Step 3.A | 枚举/字符串 ↔ 中文显示名映射 | `EnumCnMap.cs` | 4234 → 4162（-72） |
| Step 3.B | 颜色选择弹窗 | `ColorPickerDialog.cs` | 4162 → 4109（-53） |
| Step 3.C | 页面设置弹窗 | `PageSetupDialog.cs` | 4109 → 3855（-254） |
| Step 3.D | 字体设置弹窗 | `FontDialog.cs` | 3855 → 3697（-158） |
| Step 3.E | 表达式编辑器弹窗 | `ExpressionEditorDialog.cs` | 3697 → 3599（-98） |
| Step 4.A | 属性面板 9 个 `AddProp*` 工厂 + state 打包进 `PropertyRowContext` | `PropertyRowFactory.cs` | 3599 → 3402（-197） |

**累计**：**-1648 行 / -32.6%**。剩余 3402 行主要是 WPF 控件事件处理（拖拽/选中/缩放/快捷键），
与 DispatcherTimer / 剪贴板等运行时状态深度耦合，ROI 较低，暂留至 v0.4.x 重构。
候选续拆：Step4.B 抽 `LoadPreviewData` + `ShowShortcutsDialog` 等杂项弹窗（~120 行）。

> **关于 `chkStrike` 删除线**：FontDef 字段集只有 `Bold/Italic/Underline/Color`，未提供 `Strikeout`。
> 原 `ShowFontDialog` 的 `删除线` 复选框是 dead UI，弹窗里勾选后不会回写到 `FontDef`——这是**历史遗留**，
> 不在本次抽离的修复范围内（严格"等价抽离"）。FontDialog.cs 内已用注释标注，待后续 v0.4.x 与
> `FontDef.Strikeout` 字段扩展一起处理。

---

## ✅ 测试

```bash
# 当前：40/0/0 全绿（net8.0）
dotnet test tests/ReportEngine.Core.Tests/ReportEngine.Core.Tests.csproj
```

测试只针对跨平台 `ReportEngine.Core` 模块（解析、表达式、子报表、条码、Excel 导出、基础渲染路径）。
WPF/WinForms 端暂无测试项目（涉及 Windows 桌面依赖，等以后加 net8.0-windows 测试项目）。

---

## 🤝 贡献

欢迎 PR 和 Issue！详细开发流程、代码规范、提 PR 步骤请见 [`CONTRIBUTING.md`](./CONTRIBUTING.md)。

当前最需要帮忙的方向（按 ROI 排序，详见上节"🗺️ v0.2 详细路线图"）：

1. **P0** Step4.A 抽 8 个 `AddProp*` 工厂方法（属性面板重构，最大单点收益）
2. **P0** D2 补 ClosedXmlExporter 边界用例（条件格式 / 嵌套表）
3. **P1** A1 补 SubReport/Barcode 集成测试
4. **P1** Step4.B 抽杂项弹窗（`LoadPreviewData` / `ShowShortcutsDialog`）
5. **P2** D3/D4 补 `ReportRenderer` / `ExpressionEngine` 边界用例
6. 文档：模板字段参考、API 参考、最佳实践
7. 多语言资源与 .editorconfig（统一 UTF-8 + 风格）

---

<sub>Built with ❤️ using .NET 8 + ClosedXML + PdfSharpCore + ZXing.Net</sub>