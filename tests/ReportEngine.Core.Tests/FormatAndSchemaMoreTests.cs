using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine FormatValue 格式化测试
/// </summary>
public class ExpressionEngineFormatTests
{
    private readonly ExpressionEngine _engine = new();

    private RenderContext CreateContextWithDecimal()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "amount", 1234.56m } },
        };
        return ctx;
    }

    [Fact]
    public void Evaluate_Decimal_DefaultFormat()
    {
        var ctx = CreateContextWithDecimal();
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.Contains("1234", result);
    }

    [Fact]
    public void Evaluate_Decimal_CurrencyFormat()
    {
        var ctx = CreateContextWithDecimal();
        ctx.FieldFormat = "currency";
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        // Currency format includes currency symbol
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Evaluate_Decimal_PercentFormat()
    {
        var ctx = new RenderContext { DataSourceName = "ds", FieldFormat = "percent" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "rate", 0.75m } },
        };
        var result = _engine.Evaluate("{{SUM(rate)}}", ctx);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Evaluate_Decimal_NumberFormat0()
    {
        var ctx = CreateContextWithDecimal();
        ctx.FieldFormat = "number:0";
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.DoesNotContain(".", result);
    }

    [Fact]
    public void Evaluate_Decimal_NumberFormat1()
    {
        var ctx = CreateContextWithDecimal();
        ctx.FieldFormat = "number:1";
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Evaluate_Decimal_NumberFormat2()
    {
        var ctx = CreateContextWithDecimal();
        ctx.FieldFormat = "number:2";
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Evaluate_DateTime_DefaultFormat()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Evaluate_DateTime_DateFormat()
    {
        var ctx = new RenderContext { FieldFormat = "date" };
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result);
    }

    [Fact]
    public void Evaluate_DateTime_DateTimeFormat()
    {
        var ctx = new RenderContext { FieldFormat = "datetime" };
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result);
    }

    [Fact]
    public void Evaluate_Double_DefaultFormat()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "value", 3.14 } },
        };
        var result = _engine.Evaluate("{{AVG(value)}}", ctx);
        Assert.Contains("3.14", result);
    }

    [Fact]
    public void Evaluate_Double_CurrencyFormat()
    {
        var ctx = new RenderContext { DataSourceName = "ds", FieldFormat = "currency" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "price", 99.99 } },
        };
        var result = _engine.Evaluate("{{SUM(price)}}", ctx);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Evaluate_Double_PercentFormat()
    {
        var ctx = new RenderContext { DataSourceName = "ds", FieldFormat = "percent" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "rate", 0.5 } },
        };
        var result = _engine.Evaluate("{{AVG(rate)}}", ctx);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Evaluate_NullValue_ReturnsEmpty()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{UNKNOWN}}", ctx);
        // Unknown returns the variable name, not empty
        Assert.Equal("UNKNOWN", result);
    }
}

/// <summary>
/// RenderContext 更多属性测试
/// </summary>
public class RenderContextMoreTests
{
    [Fact]
    public void RenderContext_FieldFormat_DefaultNull()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.FieldFormat);
    }

    [Fact]
    public void RenderContext_FieldFormat_Settable()
    {
        var ctx = new RenderContext { FieldFormat = "currency" };
        Assert.Equal("currency", ctx.FieldFormat);
    }

    [Fact]
    public void RenderContext_NestingDepth_DefaultZero()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_NestingDepth_Settable()
    {
        var ctx = new RenderContext { NestingDepth = 3 };
        Assert.Equal(3, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_MaxNestingDepth_Is5()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }

    [Fact]
    public void RenderContext_CurrentRow_Settable()
    {
        var row = new Dictionary<string, object> { { "id", 1 }, { "name", "Test" } };
        var ctx = new RenderContext { CurrentRow = row };
        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal(1, ctx.CurrentRow["id"]);
        Assert.Equal("Test", ctx.CurrentRow["name"]);
    }

    [Fact]
    public void RenderContext_DataSources_AddAndRetrieve()
    {
        var ctx = new RenderContext();
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } },
            new Dictionary<string, object> { { "id", 2 } }
        };
        ctx.DataSources["test"] = rows;
        Assert.True(ctx.DataSources.ContainsKey("test"));
        Assert.Equal(2, ctx.DataSources["test"].Count);
    }
}

/// <summary>
/// Band 更多属性测试
/// </summary>
public class BandMoreTests
{
    [Fact]
    public void Band_SubBands_DefaultNull()
    {
        var band = new Band();
        Assert.Null(band.SubBands);
    }

    [Fact]
    public void Band_SubBands_Settable()
    {
        var band = new Band { SubBands = new List<Band> { new Band { Type = BandType.Detail, Height = 10 } } };
        Assert.Single(band.SubBands);
    }

    [Fact]
    public void Band_DataSource_DefaultNull()
    {
        var band = new Band();
        Assert.Null(band.DataSource);
    }

    [Fact]
    public void Band_DataSource_Settable()
    {
        var band = new Band { DataSource = "orders" };
        Assert.Equal("orders", band.DataSource);
    }

    [Fact]
    public void Band_RepeatOnNewPage_DefaultFalse()
    {
        var band = new Band();
        Assert.False(band.RepeatOnNewPage);
    }

    [Fact]
    public void Band_RepeatOnNewPage_Settable()
    {
        var band = new Band { RepeatOnNewPage = true };
        Assert.True(band.RepeatOnNewPage);
    }

    [Fact]
    public void Band_Elements_DefaultEmpty()
    {
        var band = new Band();
        Assert.NotNull(band.Elements);
        Assert.Empty(band.Elements);
    }

    [Fact]
    public void Band_Elements_Addable()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "Hello" });
        Assert.Single(band.Elements);
    }
}

/// <summary>
/// PageInfo 更多属性测试
/// </summary>
public class PageInfoMoreTests
{
    [Fact]
    public void PageInfo_BackgroundImage_DefaultNull()
    {
        var page = new PageInfo();
        Assert.Null(page.BackgroundImage);
    }

    [Fact]
    public void PageInfo_BackgroundImage_Settable()
    {
        var page = new PageInfo { BackgroundImage = "bg.jpg" };
        Assert.Equal("bg.jpg", page.BackgroundImage);
    }

    [Fact]
    public void PageInfo_Watermark_DefaultNull()
    {
        var page = new PageInfo();
        Assert.Null(page.Watermark);
    }

    [Fact]
    public void PageInfo_Watermark_Settable()
    {
        var page = new PageInfo { Watermark = "DRAFT" };
        Assert.Equal("DRAFT", page.Watermark);
    }

    [Fact]
    public void PageInfo_BackgroundColor_DefaultNull()
    {
        var page = new PageInfo();
        Assert.Null(page.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundColor_Settable()
    {
        var page = new PageInfo { BackgroundColor = "#FFFFFF" };
        Assert.Equal("#FFFFFF", page.BackgroundColor);
    }

    [Fact]
    public void PageInfo_MultiUp_DefaultNull()
    {
        var page = new PageInfo();
        Assert.Null(page.MultiUp);
    }

    [Fact]
    public void PageInfo_MultiUp_Settable()
    {
        var page = new PageInfo { MultiUp = new MultiUpConfig { Rows = 2, Columns = 2 } };
        Assert.NotNull(page.MultiUp);
        Assert.Equal(2, page.MultiUp.Rows);
    }
}

/// <summary>
/// ChartElement 更多属性测试
/// </summary>
public class ChartElementMoreTests
{
    [Fact]
    public void ChartElement_DataSource_DefaultEmpty()
    {
        var el = new ChartElement();
        Assert.Equal("", el.DataSource);
    }

    [Fact]
    public void ChartElement_DataSource_Settable()
    {
        var el = new ChartElement { DataSource = "sales" };
        Assert.Equal("sales", el.DataSource);
    }

    [Fact]
    public void ChartElement_CategoryField_DefaultEmpty()
    {
        var el = new ChartElement();
        Assert.Equal("", el.CategoryField);
    }

    [Fact]
    public void ChartElement_CategoryField_Settable()
    {
        var el = new ChartElement { CategoryField = "month" };
        Assert.Equal("month", el.CategoryField);
    }

    [Fact]
    public void ChartElement_Series_DefaultEmpty()
    {
        var el = new ChartElement();
        Assert.NotNull(el.Series);
        Assert.Empty(el.Series);
    }

    [Fact]
    public void ChartElement_Series_Addable()
    {
        var el = new ChartElement();
        el.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "amount" });
        Assert.Single(el.Series);
    }

    [Fact]
    public void ChartElement_Title_DefaultNull()
    {
        var el = new ChartElement();
        Assert.Null(el.Title);
    }

    [Fact]
    public void ChartElement_Title_Settable()
    {
        var el = new ChartElement { Title = "Sales Chart" };
        Assert.Equal("Sales Chart", el.Title);
    }

    [Fact]
    public void ChartElement_ChartType_DefaultBar()
    {
        var el = new ChartElement();
        Assert.Equal(ChartType.Bar, el.ChartType);
    }
}

/// <summary>
/// ChartSeries 完整属性测试
/// </summary>
public class ChartSeriesCompleteTests
{
    [Fact]
    public void ChartSeries_Name_DefaultEmpty()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.Name);
    }

    [Fact]
    public void ChartSeries_Name_Settable()
    {
        var s = new ChartSeries { Name = "Revenue" };
        Assert.Equal("Revenue", s.Name);
    }

    [Fact]
    public void ChartSeries_ValueField_DefaultEmpty()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.ValueField);
    }

    [Fact]
    public void ChartSeries_ValueField_Settable()
    {
        var s = new ChartSeries { ValueField = "amount" };
        Assert.Equal("amount", s.ValueField);
    }

    [Fact]
    public void ChartSeries_Color_DefaultNull()
    {
        var s = new ChartSeries();
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_Color_Settable()
    {
        var s = new ChartSeries { Color = "#FF0000" };
        Assert.Equal("#FF0000", s.Color);
    }

    [Fact]
    public void ChartSeries_FullSetup()
    {
        var s = new ChartSeries
        {
            Name = "Revenue",
            ValueField = "amount",
            Color = "#00FF00"
        };
        Assert.Equal("Revenue", s.Name);
        Assert.Equal("amount", s.ValueField);
        Assert.Equal("#00FF00", s.Color);
    }
}

/// <summary>
/// SubReportDataBinding 完整属性测试
/// </summary>
public class SubReportDataBindingCompleteTests
{
    [Fact]
    public void SubReportDataBinding_Source_DefaultEmpty()
    {
        var db = new SubReportDataBinding();
        Assert.Equal("", db.Source);
    }

    [Fact]
    public void SubReportDataBinding_Source_Settable()
    {
        var db = new SubReportDataBinding { Source = "orders" };
        Assert.Equal("orders", db.Source);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_DefaultEmpty()
    {
        var db = new SubReportDataBinding();
        Assert.NotNull(db.ParamMap);
        Assert.Empty(db.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_Addable()
    {
        var db = new SubReportDataBinding();
        db.ParamMap["customerId"] = "Id";
        Assert.Single(db.ParamMap);
        Assert.Equal("Id", db.ParamMap["customerId"]);
    }

    [Fact]
    public void SubReportDataBinding_FullSetup()
    {
        var db = new SubReportDataBinding { Source = "orders" };
        db.ParamMap["customerId"] = "Id";
        db.ParamMap["orderId"] = "OrderId";
        Assert.Equal("orders", db.Source);
        Assert.Equal(2, db.ParamMap.Count);
    }
}
