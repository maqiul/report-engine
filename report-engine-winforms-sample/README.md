# ReportEngine WinForms Sample

> WinForms 桌面端集成 ReportEngine 的完整示例：**模板读取 + 替换 + 预览 + 导出**（对称 WPF sample）。

## 一键运行

```bash
cd D:\report-engine\report-engine-winforms-sample
dotnet run -c Release
```

启动后自动加载 `Templates\order-summary.rptx` 并渲染 3 条订单数据。

## 与 WPF sample 的差异

| 项 | WPF | WinForms |
|----|-----|----------|
| viewer 控件 | `ReportViewerControl`（WPF） | `ReportViewerControl`（WinForms） |
| `Zoom` 类型 | `double` | **`float`** |
| 翻页/缩放工具栏 | 示例自己搭 | **控件内建**（首页/上下页/末页/缩放/适合宽度/打印） |
| 打印 | 无 | **`PrintReport()` 打印预览** |
| 窗体 | `MainWindow.xaml` + 后台 | 纯代码 `MainForm.cs`（无 designer/resx） |
| 入口 | `App.xaml` | `Program.cs` `[STAThread] Main` |

## 集成 5 步走

### 1. 项目引用

```xml
<!-- ReportEngine.WinFormsSample.csproj -->
<ProjectReference Include="..\ReportEngine.Core\ReportEngine.Core.csproj" />
<ProjectReference Include="..\ReportEngine.Viewer.WinForms\ReportEngine.Viewer.WinForms.csproj" />
<ProjectReference Include="..\ReportEngine.Export.Pdf\ReportEngine.Export.Pdf.csproj" />
<ProjectReference Include="..\ReportEngine.Export.Excel\ReportEngine.Export.Excel.csproj" />
```

> WinForms viewer 多目标 `net462;net8.0-windows`，本示例锁 `net8.0-windows`。

### 2. 放置预览控件

```csharp
using ReportEngine.Viewer.WinForms;

var viewer = new ReportViewerControl { Dock = DockStyle.Fill };
this.Controls.Add(viewer);
// 控件自带顶部工具栏：翻页 / 缩放 / 适合宽度 / 打印
```

### 3. 加载模板

```csharp
using ReportEngine.Core.Parsing;

var parser = new TemplateParser();
ReportTemplate template = parser.ParseFile(@"Templates\order-summary.rptx");
// 或：parser.Parse(jsonString)
```

### 4. 渲染

```csharp
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;

var resolver = new FileSystemTemplateResolver(@"Templates\");
var renderer = new ReportRenderer(resolver);

var data = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["orders"] = new List<Dictionary<string, object>>
    {
        new() { ["id"] = "SO-001", ["customer"] = "Acme Corp", ["total"] = 1990.00 }
    }
};

RenderedReport rendered = await renderer.RenderAsync(template, data);
viewer.SetReport(rendered);
```

### 5. 导出

```csharp
// PDF
new PdfSharpExporter().ExportToFile(rendered, @"out\order.pdf");
// Excel
new ClosedXmlExporter().ExportToFile(rendered, @"out\order.xlsx");
// 打印预览（控件内建）
viewer.PrintReport();
```

## 关键 API 一览

| 类/方法 | 作用 |
|---------|------|
| `TemplateParser.ParseFile(path)` | 读取 .rptx → `ReportTemplate` |
| `TemplateParser.Serialize(template)` | 序列化回 JSON |
| `ReportRenderer.RenderAsync(template, data)` | 渲染 → `RenderedReport` |
| `FileSystemTemplateResolver(dir)` | 文件系统模板源 |
| `ReportViewerControl.SetReport(report)` | 喂入预览 |
| `ReportViewerControl.Zoom` (`float`) | 缩放 0.25~4.0 |
| `ReportViewerControl.CurrentPage` / `TotalPages` | 翻页 |
| `ReportViewerControl.FitWidth()` | 适应宽度 |
| `ReportViewerControl.PrintReport()` | 打印预览 |

## 演示功能（5 个按钮）

| 按钮 | 演示 |
|------|------|
| **1. 加载模板 + 渲染预览** | `ParseFile` → `RenderAsync` → `SetReport` |
| **2. 替换标题后重新渲染** | 强转 `TextElement.Text` 改标题 + 重新渲染 |
| **3. 保存当前模板为 JSON** | `Serialize` → `WriteAllText` + 重新 `Parse` 验证 |
| **4. 导出 PDF** | `SaveFileDialog` + `PdfSharpExporter.ExportToFile` |
| **5. 导出 Excel** | `SaveFileDialog` + `ClosedXmlExporter.ExportToFile` |

> 翻页、缩放、打印由 viewer 内建工具栏提供，无需自己实现。

## 测试

```bash
cd D:\report-engine\report-engine-winforms-sample.Tests
dotnet test -c Release
```

10 个 xUnit 测试全通过（`Xunit.StaFact 1.1.11` 的 `[WinFormsFact]` 提供 UI 线程）：

- 模板解析 / JSON 往返
- 渲染产页 / detail 展开
- PDF / Excel 导出魔数校验（`%PDF` / `PK`）
- viewer 接收渲染结果 / Zoom 钳制（float 0.25~4.0）
- `MainForm` 实例化（纯代码窗体，无 XAML 加载顺序问题，可直接测）

## 模板格式

`order-summary.rptx` 是标准 JSON（与 .NET / Java / Vue 端通用）：

```json
{
  "version": "1.0",
  "page": { "width": 210, "height": 297, "margin": {...} },
  "dataSources": [{ "name": "orders", "type": "json" }],
  "bands": [
    { "type": "reportHeader", "height": 12, "elements": [ {...} ] },
    { "type": "detail", "dataSource": "orders", "height": 8, "elements": [ {...} ] }
  ]
}
```

表达式 `{{orders.id}}` 按 `{{dataSourceName.fieldName}}` 命名空间求值。
