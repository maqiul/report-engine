using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// Band 完整属性组合测试
// ─────────────────────────────────────────────────────────────────────────────

public class BandCompletePropsTests
{
    [Fact]
    public void Band_Type_DefaultHeader()
    {
        var b = new Band();
        Assert.Equal(BandType.Header, b.Type);
    }

    [Fact]
    public void Band_Height_DefaultZero()
    {
        var b = new Band();
        Assert.Equal(0, b.Height);
    }

    [Fact]
    public void Band_Height_SetValue()
    {
        var b = new Band { Height = 25.5 };
        Assert.Equal(25.5, b.Height);
    }

    [Fact]
    public void Band_RepeatOnNewPage_DefaultFalse()
    {
        var b = new Band();
        Assert.False(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_RepeatOnNewPage_SetTrue()
    {
        var b = new Band { RepeatOnNewPage = true };
        Assert.True(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_DataSource_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.DataSource);
    }

    [Fact]
    public void Band_DataSource_SetValue()
    {
        var b = new Band { DataSource = "orders" };
        Assert.Equal("orders", b.DataSource);
    }

    [Fact]
    public void Band_Group_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.Group);
    }

    [Fact]
    public void Band_Group_SetValue()
    {
        var b = new Band { Group = new GroupDef { Expression = "region" } };
        Assert.NotNull(b.Group);
        Assert.Equal("region", b.Group.Expression);
    }

    [Fact]
    public void Band_Elements_DefaultEmpty()
    {
        var b = new Band();
        Assert.NotNull(b.Elements);
        Assert.Empty(b.Elements);
    }

    [Fact]
    public void Band_MultiColumn_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.MultiColumn);
    }

    [Fact]
    public void Band_SubBands_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.SubBands);
    }

    [Fact]
    public void Band_AddElements()
    {
        var b = new Band();
        b.Elements.Add(new TextElement { Text = "Title" });
        b.Elements.Add(new LineElement());
        Assert.Equal(2, b.Elements.Count);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GroupDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class GroupDefComplete3Tests
{
    [Fact]
    public void GroupDef_Expression_DefaultEmpty()
    {
        var g = new GroupDef();
        Assert.Equal("", g.Expression);
    }

    [Fact]
    public void GroupDef_Expression_SetValue()
    {
        var g = new GroupDef { Expression = "category" };
        Assert.Equal("category", g.Expression);
    }

    [Fact]
    public void GroupDef_KeepTogether_DefaultTrue()
    {
        var g = new GroupDef();
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_KeepTogether_SetFalse()
    {
        var g = new GroupDef { KeepTogether = false };
        Assert.False(g.KeepTogether);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RenderedReport FitToWidth / Scale 更多测试
// ─────────────────────────────────────────────────────────────────────────────

public class RenderedReportScalingExtraTests
{
    [Fact]
    public void FitToWidth_ScalesElements()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { X = 100, Y = 50, Width = 80, Height = 20 });
        report.Pages.Add(page);

        report.FitToWidth(100); // scale = 0.5

        Assert.Equal(100, report.PageWidth);
        Assert.Equal(150, report.PageHeight);
        var el = (RenderedTextElement)report.Pages[0].Elements[0];
        Assert.Equal(50, el.X);
        Assert.Equal(25, el.Y);
        Assert.Equal(40, el.Width);
        Assert.Equal(10, el.Height);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenAlreadyFits()
    {
        var report = new RenderedReport { PageWidth = 100, PageHeight = 150 };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 50, Height = 20 });
        report.Pages.Add(page);

        report.FitToWidth(200); // scale >= 1.0, no scaling

        Assert.Equal(100, report.PageWidth); // unchanged
        var el = (RenderedTextElement)report.Pages[0].Elements[0];
        Assert.Equal(10, el.X); // unchanged
    }

    [Fact]
    public void FitToWidth_NoScale_WhenPageWidthZero()
    {
        var report = new RenderedReport { PageWidth = 0, PageHeight = 300 };
        report.Pages.Add(new RenderedPage());

        report.FitToWidth(100); // should not throw

        Assert.Equal(0, report.PageWidth); // unchanged
    }

    [Fact]
    public void Scale_ScalesAllElements()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { X = 40, Y = 60, Width = 100, Height = 30 });
        report.Pages.Add(page);

        report.Scale(0.5);

        Assert.Equal(100, report.PageWidth);
        Assert.Equal(150, report.PageHeight);
        var el = (RenderedTextElement)report.Pages[0].Elements[0];
        Assert.Equal(20, el.X);
        Assert.Equal(30, el.Y);
        Assert.Equal(50, el.Width);
        Assert.Equal(15, el.Height);
    }

    [Fact]
    public void Scale_NoScale_WhenFactorOne()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        report.Pages.Add(new RenderedPage());

        report.Scale(1.0);

        Assert.Equal(200, report.PageWidth); // unchanged
    }

    [Fact]
    public void Scale_NoScale_WhenFactorZero()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        report.Pages.Add(new RenderedPage());

        report.Scale(0);

        Assert.Equal(200, report.PageWidth); // unchanged
    }

    [Fact]
    public void Scale_NoScale_WhenFactorNegative()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        report.Pages.Add(new RenderedPage());

        report.Scale(-1);

        Assert.Equal(200, report.PageWidth); // unchanged
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine 字段引用测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineFieldRefTests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_CurrentRowField()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "name", "Alice" } };
        var result = _engine.Evaluate("Hello {{currentRow.name}}!", ctx);
        Assert.Equal("Hello Alice!", result);
    }

    [Fact]
    public void Evaluate_DataSourceField()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "city", "Ningbo" } }
        };
        ctx.CurrentRow = new Dictionary<string, object> { { "city", "Ningbo" } };
        var result = _engine.Evaluate("City: {{ds.city}}", ctx);
        Assert.Equal("City: Ningbo", result);
    }

    [Fact]
    public void Evaluate_UnknownField_ReturnsOriginal()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{unknown}}", ctx);
        Assert.Equal("unknown", result);
    }

    [Fact]
    public void Evaluate_NoPlaceholders_ReturnsOriginal()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("Hello World", ctx);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_MultiplePlaceholders()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object>
        {
            { "first", "John" },
            { "last", "Doe" }
        };
        var result = _engine.Evaluate("{{first}} {{last}}", ctx);
        Assert.Equal("John Doe", result);
    }
}
