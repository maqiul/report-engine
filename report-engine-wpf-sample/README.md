# ReportEngine WPF Sample

> WPF 桌面端集成 ReportEngine 的完整示例：**模板读取 + 替换 + 预览**。

## 一键运行

```bash
cd D:\report-engine\report-engine-wpf-sample
dotnet run -c Release
```

启动后自动加载 `Templates\order-summary.rptx` 并渲染 3 条订单数据。

## 集成 5 步走

### 1. 项目引用

```xml
<!-- ReportEngine.WpfSample.csproj -->
<ProjectReference Include="..\ReportEngine.Core\ReportEngine.Core.csproj" />
<ProjectReference Include="..\ReportEngine.Viewer.Wpf\ReportEngine.Viewer.Wpf.csproj" />
```

### 2. XAML 嵌入预览控件

```xml
<Window xmlns:viewer="clr-namespace:ReportEngine.Viewer.Wpf;assembly=ReportEngine.Viewer.Wpf">
    <viewer:ReportViewerControl x:Name="Viewer" />
</Window>
```

### 3. 加载模板（读取 .rptx JSON）

```csharp
using ReportEngine.Core.Parsing;

var parser = new TemplateParser();
ReportTemplate template = parser.ParseFile(@"Templates\order-summary.rptx");
// 或从字符串：
ReportTemplate template2 = parser.Parse(jsonString);
```

### 4. 渲染（数据替换表达式 `{{orders.xxx}}`）

```csharp
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;

var resolver = new FileSystemTemplateResolver(@"Templates\");
var renderer = new ReportRenderer(resolver);

var data = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["orders"] = new List<Dictionary<string, object>>
    {
        new() { ["id"] = "SO-001", ["customer"] = "Acme", ["total"] = 1990m }
    }
};

var rendered = await renderer.RenderAsync(template, data);
```

### 5. 显示预览 + 替换保存

```csharp
// 显示预览
Viewer.SetReport(rendered);

// 替换标题后重新渲染（演示动态修改）
if (template.Bands[0].Elements[0] is TextElement titleEl)
{
    titleEl.Text = "新标题";
}
var newRendered = await renderer.RenderAsync(template, newData);
Viewer.SetReport(newRendered);

// 序列化回 JSON
string json = parser.Serialize(template);
File.WriteAllText("output.rptx", json);
```

## 关键 API 一览

| 类/方法 | 作用 |
|---------|------|
| `TemplateParser.ParseFile(path)` | 读取 .rptx → `ReportTemplate` |
| `TemplateParser.Parse(json)` | 解析 JSON 字符串 |
| `TemplateParser.Serialize(template)` | 序列化回 JSON |
| `ReportRenderer.RenderAsync(template, data)` | 渲染 → `RenderedReport` |
| `FileSystemTemplateResolver(dir)` | 文件系统模板源 |
| `ReportViewerControl.SetReport(report)` | WPF 预览 |
| `ReportViewerControl.Zoom` / `CurrentPage` | 缩放/翻页 |

## 项目结构

```
report-engine-wpf-sample/
├── ReportEngine.WpfSample.csproj       # 项目文件
├── App.xaml / App.xaml.cs              # 应用入口
├── MainWindow.xaml / .cs               # 主窗体 + 演示逻辑
└── Templates/
    └── order-summary.rptx              # 演示模板
```

## 演示功能

| 按钮 | 演示 |
|------|------|
| **1. 加载模板 + 渲染预览** | `ParseFile` → `RenderAsync` → `Viewer.SetReport` |
| **2. 替换标题后重新渲染** | 改 `TextElement.Text` + 换数据 + 重新渲染 |
| **3. 保存当前模板为 JSON** | `Serialize` → `File.WriteAllText` + 重新 `Parse` 验证 |
| **上一页 / 下一页** | 翻页 |
| **缩放滑块** | 0.5× ~ 2.0× |

## 模板格式说明

`order-summary.rptx` 是标准 JSON：

```json
{
  "version": "1.0",
  "page": { "width": 210, "height": 297, "margin": {...} },
  "dataSources": [{ "name": "orders", "type": "json" }],
  "bands": [
    { "type": "title", "height": 12, "elements": [...] },
    { "type": "detail", "dataSource": "orders", "height": 8, "elements": [...] }
  ]
}
```

表达式 `{{orders.id}}` 在 `{{dataSourceName.fieldName}}` 命名空间下求值。