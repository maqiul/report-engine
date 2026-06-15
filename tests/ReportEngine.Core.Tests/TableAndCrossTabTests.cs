using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TableElement + CrossTabElement 渲染测试：
///   - Table: RowCount/ColCount, EqualSplit 行为, 单元格表达式
///   - CrossTab: 单行/单列字段, 多 Measures, ShowRowTotal/ShowColumnTotal
///   - VisibleExpression: 隐藏/显示
/// </summary>
public class TableAndCrossTabTests
{
    private static ReportRenderer NewRenderer() => new ReportRenderer(new InMemoryTemplateResolver());

    private static ReportTemplate MakeTemplate(params ReportElement[] elements)
    {
        var band = new Band
        {
            Type = BandType.Detail, Height = 50, DataSource = "ds",
            Elements = elements.ToList(),
        };
        return new ReportTemplate
        {
            Page = new PageInfo { Width = 200, Height = 200, Margin = new Margin() },
            DataSources = new List<DataSourceDef> { new DataSourceDef { Name = "ds" } },
            Bands = new List<Band> { band },
        };
    }

    private static Dictionary<string, List<Dictionary<string, object>>> OneRow()
    {
        return new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { new() { { "x", 1 }, { "y", 2 } } } },
        };
    }

    [Fact]
    public async Task Render_TableElement_EqualSplit_ColumnsAndRows()
    {
        var table = new TableElement
        {
            X = 0, Y = 0, Width = 30, Height = 20, RowCount = 2, ColCount = 3,
            Cells = new List<TableCell>
            {
                new() { Row = 0, Col = 0, Text = "A" },
                new() { Row = 0, Col = 1, Text = "B" },
                new() { Row = 1, Col = 0, Text = "C" },
            },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(table), OneRow());
        var t = result.Pages[0].Elements.OfType<RenderedTableElement>().Single();
        Assert.Equal(2, t.RowCount);
        Assert.Equal(3, t.ColCount);
        // EqualSplit(30, 3) = [10, 10, 10]
        Assert.Equal(3, t.ColumnWidths.Count);
        Assert.All(t.ColumnWidths, w => Assert.Equal(10, w, 1));
        // EqualSplit(20, 2) = [10, 10]
        Assert.Equal(2, t.RowHeights.Count);
        Assert.All(t.RowHeights, h => Assert.Equal(10, h, 1));
    }

    [Fact]
    public async Task Render_TableElement_CustomColumnWidths_OverrideEqualSplit()
    {
        var table = new TableElement
        {
            X = 0, Y = 0, Width = 30, Height = 20, RowCount = 1, ColCount = 3,
            ColumnWidths = new List<double> { 5, 10, 15 },
            Cells = new List<TableCell>(),
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(table), OneRow());
        var t = result.Pages[0].Elements.OfType<RenderedTableElement>().Single();
        Assert.Equal(new double[] { 5, 10, 15 }, t.ColumnWidths);
    }

    [Fact]
    public async Task Render_TableElement_CellText_EvaluatedAsExpression()
    {
        var table = new TableElement
        {
            X = 0, Y = 0, Width = 30, Height = 10, RowCount = 1, ColCount = 1,
            Cells = new List<TableCell> { new() { Row = 0, Col = 0, Text = "Hello" } },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(table), OneRow());
        var t = result.Pages[0].Elements.OfType<RenderedTableElement>().Single();
        Assert.Single(t.Cells);
        Assert.Equal("Hello", t.Cells[0].Text);
    }

    [Fact]
    public async Task Render_TableElement_PreservesRowSpanColSpan()
    {
        var table = new TableElement
        {
            X = 0, Y = 0, Width = 30, Height = 30, RowCount = 2, ColCount = 2,
            Cells = new List<TableCell>
            {
                new() { Row = 0, Col = 0, RowSpan = 2, ColSpan = 1, Text = "merged" },
                new() { Row = 0, Col = 1, Text = "right" },
            },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(table), OneRow());
        var t = result.Pages[0].Elements.OfType<RenderedTableElement>().Single();
        var merged = t.Cells.First(c => c.Text == "merged");
        Assert.Equal(2, merged.RowSpan);
        Assert.Equal(1, merged.ColSpan);
    }

    [Fact]
    public async Task Render_CrossTab_SingleMeasure_ProducesCells()
    {
        var ct = new CrossTabElement
        {
            X = 0, Y = 0, Width = 100, Height = 50,
            DataSource = "ds",
            RowFields = new List<string> { "category" },
            ColumnFields = new List<string> { "region" },
            Measures = new List<CrossTabMeasure> { new() { Field = "amount", Aggregate = "Sum" } },
            ShowRowTotal = true,
            ShowColumnTotal = true,
        };
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>
                {
                    new() { { "category", "A" }, { "region", "East" }, { "amount", 10 } },
                    new() { { "category", "A" }, { "region", "West" }, { "amount", 20 } },
                    new() { { "category", "B" }, { "region", "East" }, { "amount", 5 } },
                }
            },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(ct), data);
        var rendered = result.Pages[0].Elements.OfType<RenderedCrossTabElement>().FirstOrDefault();
        Assert.NotNull(rendered);
        Assert.True(rendered.RowCount > 0);
        Assert.True(rendered.ColCount > 0);
        Assert.NotEmpty(rendered.Cells);
    }

    [Fact]
    public async Task Render_CrossTab_ShowRowTotalFalse_NoTotalRow()
    {
        var ct = new CrossTabElement
        {
            X = 0, Y = 0, Width = 100, Height = 50,
            DataSource = "ds",
            RowFields = new List<string> { "cat" },
            ColumnFields = new List<string> { "reg" },
            Measures = new List<CrossTabMeasure> { new() { Field = "x", Aggregate = "Sum" } },
            ShowRowTotal = false,
            ShowColumnTotal = false,
        };
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { new() { { "cat", "A" }, { "reg", "X" }, { "x", 1 } } } },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(ct), data);
        var rendered = result.Pages[0].Elements.OfType<RenderedCrossTabElement>().FirstOrDefault();
        Assert.NotNull(rendered);
        // ShowRowTotal=false 时无 "合计" 行
        Assert.DoesNotContain(rendered.Cells, c => c.Text == "合计");
    }

    [Fact]
    public async Task Render_CrossTab_MultipleMeasures_ShowsMeasureLabels()
    {
        var ct = new CrossTabElement
        {
            X = 0, Y = 0, Width = 100, Height = 50,
            DataSource = "ds",
            RowFields = new List<string> { "cat" },
            ColumnFields = new List<string> { "reg" },
            Measures = new List<CrossTabMeasure>
            {
                new() { Field = "amt", Aggregate = "Sum", Label = "销售额" },
                new() { Field = "qty", Aggregate = "Sum", Label = "数量" },
            },
            ShowRowTotal = false,
            ShowColumnTotal = false,
        };
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>
                {
                    new() { { "cat", "A" }, { "reg", "X" }, { "amt", 100 }, { "qty", 5 } },
                }
            },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(ct), data);
        var rendered = result.Pages[0].Elements.OfType<RenderedCrossTabElement>().FirstOrDefault();
        Assert.NotNull(rendered);
        var labels = rendered.Cells.Select(c => c.Text).ToList();
        Assert.Contains(labels, l => l != null && l.Contains("销售额"));
        Assert.Contains(labels, l => l != null && l.Contains("数量"));
    }

    [Fact]
    public async Task Render_ElementWithVisibleExpression_True_Visible()
    {
        // VisibleExpression 当前实现是 Convert.ToBoolean(Evaluate)—— "true" 字符串能转 boolean
        var text = new TextElement
        {
            X = 0, Y = 0, Width = 30, Height = 5, Text = "OK",
            VisibleExpression = "true",
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(text), OneRow());
        var texts = result.Pages[0].Elements.OfType<RenderedTextElement>().Select(t => t.Text).ToList();
        Assert.Contains("OK", texts);
    }

    [Fact]
    public async Task Render_ElementWithoutVisibleExpression_AlwaysVisible()
    {
        var text = new TextElement { X = 0, Y = 0, Width = 30, Height = 5, Text = "DEFAULT" };
        var result = await NewRenderer().RenderAsync(MakeTemplate(text), OneRow());
        var texts = result.Pages[0].Elements.OfType<RenderedTextElement>().Select(t => t.Text).ToList();
        Assert.Contains("DEFAULT", texts);
    }
}
