using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Pdf;
using ReportEngine.Export.Excel;

// ============================================================
// 演示程序：验证模板解析 + 表达式引擎 + 子报表嵌套渲染
// 运行: dotnet run
// ============================================================

Console.WriteLine("=== ReportEngine 测试 ===\n");

// 1. 创建模板解析器
var parser = new TemplateParser();

// 2. 解析主模板（销售订单报表，包含子报表引用）
Console.WriteLine("📄 解析主模板: sales_order.rptx");
var mainTemplate = parser.ParseFile("SampleTemplates/sales_order.rptx");
Console.WriteLine($"   版本: {mainTemplate.Version}");
Console.WriteLine($"   页面: {mainTemplate.Page.Width}x{mainTemplate.Page.Height}mm");
Console.WriteLine($"   数据源: {string.Join(", ", mainTemplate.DataSources.Select(ds => ds.Name))}");
Console.WriteLine($"   Band 数量: {mainTemplate.Bands.Count}");
Console.WriteLine($"   Band 类型: {string.Join(", ", mainTemplate.Bands.Select(b => b.Type))}");

// 3. 解析子模板（订单明细）
Console.WriteLine("\n📄 解析子模板: order_detail.rptx");
var detailTemplate = parser.ParseFile("SampleTemplates/order_detail.rptx");
Console.WriteLine($"   页面: {detailTemplate.Page.Width}x{detailTemplate.Page.Height}mm");
Console.WriteLine($"   数据源: {string.Join(", ", detailTemplate.DataSources.Select(ds => ds.Name))}");

// 4. 验证主模板中的子报表元素
Console.WriteLine("\n🔗 检查子报表引用:");
foreach (var band in mainTemplate.Bands)
{
    var subReports = band.Elements.OfType<SubReportElement>().ToList();
    foreach (var sub in subReports)
    {
        Console.WriteLine($"   Band [{band.Type}] → 子模板: {sub.TemplateRef}");
        Console.WriteLine($"         数据源: {sub.DataBinding.Source}");
        Console.WriteLine($"         参数: {string.Join(", ", sub.DataBinding.ParamMap.Select(kv => $"{kv.Key} = {kv.Value}"))}");
        Console.WriteLine($"         高度模式: {sub.HeightMode}");
        Console.WriteLine($"         每行重复: {sub.RepeatPerRow}");
    }
}

// 5. 创建模板解析器（用于渲染引擎的子报表加载）
var resolver = new FileSystemTemplateResolver("SampleTemplates");
Console.WriteLine($"\n📁 本地可用模板: {string.Join(", ", resolver.ListTemplates())}");

// 6. 创建渲染引擎
var renderer = new ReportRenderer(resolver);

// 7. 准备测试数据
Console.WriteLine("\n📊 准备测试数据...");
var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["orders"] = new()
    {
        new() { ["orderNo"] = "ORD-2024-001", ["customer"] = "张三科技", ["orderDate"] = new DateTime(2024, 3, 15), ["totalAmount"] = 12580.50m, ["status"] = "已确认" },
        new() { ["orderNo"] = "ORD-2024-002", ["customer"] = "李四贸易", ["orderDate"] = new DateTime(2024, 3, 18), ["totalAmount"] = 8960.00m, ["status"] = "待确认" },
        new() { ["orderNo"] = "ORD-2024-003", ["customer"] = "王五工业", ["orderDate"] = new DateTime(2024, 3, 20), ["totalAmount"] = 35200.75m, ["status"] = "已发货" },
    },
    ["order_items"] = new()
    {
        new() { ["productName"] = "A型传感器", ["quantity"] = 50, ["unitPrice"] = 128.00m, ["subtotal"] = 6400.00m },
        new() { ["productName"] = "B型传感器", ["quantity"] = 30, ["unitPrice"] = 206.00m, ["subtotal"] = 6180.00m },
        new() { ["productName"] = "C型传感器", ["quantity"] = 10, ["unitPrice"] = 318.50m, ["subtotal"] = 3185.00m },
        new() { ["productName"] = "D型传感器", ["quantity"] = 20, ["unitPrice"] = 175.00m, ["subtotal"] = 3500.00m },
        new() { ["productName"] = "E型传感器", ["quantity"] = 15, ["unitPrice"] = 250.00m, ["subtotal"] = 3750.00m },
    }
};

Console.WriteLine($"   orders: {dataSources["orders"].Count} 条");
Console.WriteLine($"   order_items: {dataSources["order_items"].Count} 条");

// 8. 渲染报表
Console.WriteLine("\n⚙️  开始渲染...");
var renderedReport = await renderer.RenderAsync(mainTemplate, dataSources);

Console.WriteLine($"   渲染完成!");
Console.WriteLine($"   总页数: {renderedReport.Pages.Count}");
Console.WriteLine($"   页面尺寸: {renderedReport.PageWidth}x{renderedReport.PageHeight}mm");

foreach (var (i, page) in renderedReport.Pages.Select((p, idx) => (idx + 1, p)))
{
    var textElements = page.Elements.OfType<RenderedTextElement>().ToList();
    Console.WriteLine($"\n   📃 第 {page.PageNumber}/{page.TotalPages} 页 (共 {page.Elements.Count} 个元素):");
    foreach (var elem in textElements.Take(8))
    {
        Console.WriteLine($"      [{elem.Id}] ({elem.X:F0},{elem.Y:F0}) \"{elem.Text}\"  字体:{elem.Font.Family} {elem.Font.Size}pt");
    }
    if (textElements.Count > 8)
    {
        Console.WriteLine($"      ... 还有 {textElements.Count - 8} 个文本元素");
    }
}

// 9. 测试表达式引擎
Console.WriteLine("\n🧪 测试表达式引擎:");
var exprEngine = new ReportEngine.Core.Data.ExpressionEngine();

var testDs = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["orders"] = dataSources["orders"],
};

var testContext = new ReportEngine.Core.Data.RenderContext
{
    CurrentPage = 3,
    TotalPages = 15,
    CurrentRowNumber = 7,
    CurrentRow = new() { ["name"] = "测试公司", ["amount"] = 9999.99m },
    DataSourceName = "orders",
};
testContext.DataSources["orders"] = dataSources["orders"];

var testCases = new[]
{
    ("系统变量-日期", "{{REPORT_DATE}}"),
    ("系统变量-页码", "第{{PAGE}}页"),
    ("字段引用", "{{currentRow.name}}"),
    ("金额格式化", "¥{{currentRow.amount}}"),
    ("聚合-SUM", "总金额: {{SUM(orders.totalAmount)}}"),
    ("聚合-COUNT", "订单数: {{COUNT(orders.orderNo)}}"),
    ("混合文本", "报表: {{REPORT_DATE}} | 第{{PAGE}}/{{TOTAL_PAGES}}页"),
};

foreach (var (name, expr) in testCases)
{
    testContext.FieldFormat = expr.Contains("amount") ? "currency" : null;
    var result = exprEngine.Evaluate(expr, testContext);
    Console.WriteLine($"   {name}: \"{expr}\" → \"{result}\"");
}

// 10. 序列化测试（验证模板可正确往返）
Console.WriteLine("\n💾 模板序列化测试...");
var serialized = parser.Serialize(mainTemplate);
var deserialized = parser.Parse(serialized);
Console.WriteLine($"   原始模板 bands: {mainTemplate.Bands.Count}");
Console.WriteLine($"   反序列化 bands: {deserialized.Bands.Count}");
Console.WriteLine($"   往返一致: {mainTemplate.Bands.Count == deserialized.Bands.Count} ✓");

// 11. PDF 导出（PdfSharpCore）
Console.WriteLine("\n📑 PDF 导出测试 (PdfSharpCore)...");
var pdfExporter = new PdfSharpExporter();
var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);
var pdfPath = Path.Combine(outputDir, "sales_order.pdf");
pdfExporter.ExportToFile(renderedReport, pdfPath);
var pdfInfo = new FileInfo(pdfPath);
Console.WriteLine($"   导出成功: {pdfPath}");
Console.WriteLine($"   文件大小: {pdfInfo.Length / 1024.0:F1} KB");

// 12. Excel 导出（ClosedXML）
Console.WriteLine("\n📊 Excel 导出测试 (ClosedXML)...");
var excelExporter = new ClosedXmlExporter();
var excelPath = Path.Combine(outputDir, "sales_order.xlsx");
excelExporter.ExportToFile(renderedReport, excelPath);
var excelInfo = new FileInfo(excelPath);
Console.WriteLine($"   导出成功: {excelPath}");
Console.WriteLine($"   文件大小: {excelInfo.Length / 1024.0:F1} KB");

Console.WriteLine("\n✅ 测试完成!");
