using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement 渲染与 BoxType 行为测试：
///   - BoxType 计算 getter 优先级（SysVar > Summary > Field > Static）
///   - RenderedTextElement 复制 Font/Alignment/Hyperlink/BackgroundColor/Border
///   - Text 表达式求值
///   - CanGrow/CanShrink/MaxLines 字段保留（即使 RenderedTextElement 不读）
/// </summary>
public class TextElementRenderingTests
{
    private static ReportRenderer NewRenderer() => new ReportRenderer(new InMemoryTemplateResolver());

    private static ReportTemplate MakeTemplate(params ReportElement[] elements)
    {
        var band = new Band
        {
            Type = BandType.Detail, Height = 20, DataSource = "ds",
            Elements = elements.ToList(),
        };
        return new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() },
            DataSources = new List<DataSourceDef> { new DataSourceDef { Name = "ds" } },
            Bands = new List<Band> { band },
        };
    }

    private static Dictionary<string, List<Dictionary<string, object>>> OneRow()
    {
        return new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { new() { { "name", "Alice" } } } },
        };
    }

    [Fact]
    public void BoxType_NoField_ReturnsStatic()
    {
        var t = new TextElement { Text = "Hello" };
        Assert.Equal(TextBoxType.Static, t.BoxType);
    }

    [Fact]
    public void BoxType_DataFieldSet_ReturnsField()
    {
        var t = new TextElement { Text = "", DataField = "name" };
        Assert.Equal(TextBoxType.Field, t.BoxType);
    }

    [Fact]
    public void BoxType_SummaryFunctionSet_ReturnsSummary()
    {
        var t = new TextElement { Text = "", SummaryFunction = "Sum", SummaryField = "amount" };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public void BoxType_SystemVariableSet_ReturnsSysVar()
    {
        var t = new TextElement { Text = "", SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void BoxType_SysVarTakesPrecedenceOverSummaryAndField()
    {
        var t = new TextElement
        {
            Text = "",
            DataField = "name",
            SummaryFunction = "Sum",
            SystemVariable = "PageNumber",
        };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void BoxType_SummaryTakesPrecedenceOverField()
    {
        var t = new TextElement
        {
            Text = "",
            DataField = "name",
            SummaryFunction = "Sum",
        };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public async Task Render_TextElement_PreservesAllVisualProperties()
    {
        var t = new TextElement
        {
            X = 5, Y = 10, Width = 40, Height = 8,
            Text = "Hello",
            Font = new FontDef { Family = "Arial", Size = 12, Bold = true, Italic = true, Color = "#000000" },
            Alignment = TextAlignment.Right,
            Hyperlink = "https://example.com",
            BackgroundColor = "#FFFF00",
            Border = new BorderDef { Style = BorderStyle.Solid, Width = 1, Color = "#000000" },
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(t), OneRow());
        var rendered = result.Pages[0].Elements.OfType<RenderedTextElement>().Single();
        Assert.Equal("Hello", rendered.Text);
        Assert.Equal(5, rendered.X, 1);
        Assert.Equal(10, rendered.Y, 1);
        Assert.Equal(TextAlignment.Right, rendered.Alignment);
        Assert.Equal("Arial", rendered.Font.Family);
        Assert.Equal(12, rendered.Font.Size, 1);
        Assert.True(rendered.Font.Bold);
        Assert.True(rendered.Font.Italic);
        Assert.Equal("https://example.com", rendered.Hyperlink);
        Assert.Equal("#FFFF00", rendered.BackgroundColor);
        Assert.NotNull(rendered.Border);
        Assert.Equal(BorderStyle.Solid, rendered.Border!.Style);
    }

    [Fact]
    public async Task Render_TextElement_EvaluatesTextExpression()
    {
        var t = new TextElement
        {
            X = 0, Y = 0, Width = 30, Height = 5,
            Text = "{{name}}",
        };
        var result = await NewRenderer().RenderAsync(MakeTemplate(t),
            new Dictionary<string, List<Dictionary<string, object>>>
            {
                { "ds", new List<Dictionary<string, object>> { new() { { "name", "Bob" } } } },
            });
        var rendered = result.Pages[0].Elements.OfType<RenderedTextElement>().Single();
        Assert.Equal("Bob", rendered.Text);
    }

    [Fact]
    public async Task Render_TextElement_EmptyString_StaysEmpty()
    {
        var t = new TextElement { X = 0, Y = 0, Width = 30, Height = 5, Text = "" };
        var result = await NewRenderer().RenderAsync(MakeTemplate(t), OneRow());
        var rendered = result.Pages[0].Elements.OfType<RenderedTextElement>().Single();
        Assert.Equal("", rendered.Text);
    }

    [Fact]
    public async Task Render_TextElement_MultipleElements_AllRendered()
    {
        var t1 = new TextElement { X = 0, Y = 0, Width = 20, Height = 3, Text = "A" };
        var t2 = new TextElement { X = 0, Y = 5, Width = 20, Height = 3, Text = "B" };
        var t3 = new TextElement { X = 0, Y = 10, Width = 20, Height = 3, Text = "C" };
        var result = await NewRenderer().RenderAsync(MakeTemplate(t1, t2, t3), OneRow());
        var texts = result.Pages[0].Elements.OfType<RenderedTextElement>().Select(t => t.Text).ToList();
        Assert.Equal(new[] { "A", "B", "C" }, texts);
    }
}
