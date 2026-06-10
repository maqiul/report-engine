using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素工厂：从工具箱拖出 / 菜单新建时使用的预置元素模板。
/// 从 MainWindow.cs 抽出，纯静态无副作用。
/// </summary>
internal static class ElementFactory
{
    /// <summary>确保元素有 Border 定义（不存在则创建空的）</summary>
    public static BorderDef EnsureBorder(ReportElement el)
    {
        if (el.Border == null) el.Border = new BorderDef();
        return el.Border;
    }

    public static TextElement NewText() => new TextElement { X = 5, Y = 2, Width = 40, Height = 6, Text = "文本" };
    public static TextElement NewFieldBox() => new TextElement { X = 5, Y = 2, Width = 40, Height = 6, DataField = "FieldName" };
    public static TextElement NewSummaryBox() => new TextElement { X = 5, Y = 2, Width = 40, Height = 6, SummaryFunction = "Sum", SummaryField = "Amount" };
    public static TextElement NewSysVarBox() => new TextElement { X = 5, Y = 2, Width = 40, Height = 6, SystemVariable = "PageNumber" };
    public static LineElement NewLine() => new LineElement { X = 5, Y = 2, Width = 50, Height = 1 };
    public static ImageElement NewImage() => new ImageElement { X = 5, Y = 2, Width = 30, Height = 20 };
    public static ShapeElement NewShape() => new ShapeElement { X = 5, Y = 2, Width = 30, Height = 15 };
    public static SubReportElement NewSubReport() => new SubReportElement { X = 5, Y = 2, Width = 60, Height = 30 };
    public static BarcodeElement NewBarcode() => new BarcodeElement { X = 5, Y = 2, Width = 25, Height = 25 };
    public static TableElement NewTable() => new TableElement { X = 5, Y = 2, Width = 80, Height = 30 };
    public static CrossTabElement NewCrossTab() => new CrossTabElement { X = 5, Y = 2, Width = 80, Height = 40 };
    public static ChartElement NewChart() => new ChartElement
    {
        X = 5,
        Y = 2,
        Width = 60,
        Height = 40,
        Title = "图表",
        ChartType = ChartType.Bar,
        CategoryField = "Category",
        Series = { new ChartSeries { Name = "系列1", ValueField = "Value" } },
    };
}