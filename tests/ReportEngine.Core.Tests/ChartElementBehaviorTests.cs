using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ChartElement 行为测试：
///   - 默认值
///   - 图表类型
///   - 数据源
///   - 分类字段
///   - 系列
///   - 标题
/// </summary>
public class ChartElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new ChartElement();

        Assert.Equal(ChartType.Bar, el.ChartType);
        Assert.Equal("", el.DataSource);
        Assert.Equal("", el.CategoryField);
        Assert.NotNull(el.Series);
        Assert.Empty(el.Series);
        Assert.Null(el.Title);
    }

    // ============== ChartType ==============

    [Fact]
    public void ChartType_DefaultIsBar()
    {
        var el = new ChartElement();
        Assert.Equal(ChartType.Bar, el.ChartType);
    }

    [Fact]
    public void ChartType_SetLine_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Line };
        Assert.Equal(ChartType.Line, el.ChartType);
    }

    [Fact]
    public void ChartType_SetPie_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Pie };
        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    [Fact]
    public void ChartType_SetArea_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Area };
        Assert.Equal(ChartType.Area, el.ChartType);
    }

    [Fact]
    public void ChartType_SetScatter_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Scatter };
        Assert.Equal(ChartType.Scatter, el.ChartType);
    }

    [Fact]
    public void ChartType_CanBeChanged()
    {
        var el = new ChartElement { ChartType = ChartType.Bar };
        el.ChartType = ChartType.Pie;
        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    // ============== DataSource ==============

    [Fact]
    public void DataSource_EmptyByDefault()
    {
        var el = new ChartElement();
        Assert.Equal("", el.DataSource);
    }

    [Fact]
    public void DataSource_SetAndGet_Works()
    {
        var el = new ChartElement { DataSource = "salesData" };
        Assert.Equal("salesData", el.DataSource);
    }

    [Fact]
    public void DataSource_CanBeChanged()
    {
        var el = new ChartElement { DataSource = "old" };
        el.DataSource = "new";
        Assert.Equal("new", el.DataSource);
    }

    // ============== CategoryField ==============

    [Fact]
    public void CategoryField_EmptyByDefault()
    {
        var el = new ChartElement();
        Assert.Equal("", el.CategoryField);
    }

    [Fact]
    public void CategoryField_SetAndGet_Works()
    {
        var el = new ChartElement { CategoryField = "month" };
        Assert.Equal("month", el.CategoryField);
    }

    [Fact]
    public void CategoryField_CanBeChanged()
    {
        var el = new ChartElement { CategoryField = "month" };
        el.CategoryField = "year";
        Assert.Equal("year", el.CategoryField);
    }

    // ============== Series ==============

    [Fact]
    public void Series_EmptyByDefault()
    {
        var el = new ChartElement();
        Assert.Empty(el.Series);
    }

    [Fact]
    public void Series_Add_Works()
    {
        var el = new ChartElement();
        el.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "amount" });
        Assert.Single(el.Series);
    }

    [Fact]
    public void Series_AddMultiple_Works()
    {
        var el = new ChartElement();
        el.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "revenue" });
        el.Series.Add(new ChartSeries { Name = "Cost", ValueField = "cost" });
        el.Series.Add(new ChartSeries { Name = "Profit", ValueField = "profit" });
        Assert.Equal(3, el.Series.Count);
    }

    [Fact]
    public void Series_Remove_Works()
    {
        var el = new ChartElement();
        var series = new ChartSeries { Name = "Revenue" };
        el.Series.Add(series);
        el.Series.Remove(series);
        Assert.Empty(el.Series);
    }

    [Fact]
    public void Series_Clear_Works()
    {
        var el = new ChartElement();
        el.Series.Add(new ChartSeries { Name = "A" });
        el.Series.Add(new ChartSeries { Name = "B" });
        el.Series.Clear();
        Assert.Empty(el.Series);
    }

    // ============== Title ==============

    [Fact]
    public void Title_NullByDefault()
    {
        var el = new ChartElement();
        Assert.Null(el.Title);
    }

    [Fact]
    public void Title_SetEnglish_Works()
    {
        var el = new ChartElement { Title = "Monthly Sales" };
        Assert.Equal("Monthly Sales", el.Title);
    }

    [Fact]
    public void Title_SetChinese_Works()
    {
        var el = new ChartElement { Title = "月度销售报表" };
        Assert.Equal("月度销售报表", el.Title);
    }

    [Fact]
    public void Title_CanBeCleared()
    {
        var el = new ChartElement { Title = "Sales" };
        el.Title = null;
        Assert.Null(el.Title);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ChartElement_BarChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Bar,
            DataSource = "salesData",
            CategoryField = "month",
            Title = "月度销售额"
        };
        el.Series.Add(new ChartSeries { Name = "销售额", ValueField = "amount", Color = "#4472C4" });

        Assert.Equal(ChartType.Bar, el.ChartType);
        Assert.Single(el.Series);
    }

    [Fact]
    public void ChartElement_LineChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Line,
            DataSource = "trendData",
            CategoryField = "date"
        };
        el.Series.Add(new ChartSeries { Name = "Temperature", ValueField = "temp", Color = "#FF0000" });
        el.Series.Add(new ChartSeries { Name = "Humidity", ValueField = "humidity", Color = "#0000FF" });

        Assert.Equal(ChartType.Line, el.ChartType);
        Assert.Equal(2, el.Series.Count);
    }

    [Fact]
    public void ChartElement_PieChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Pie,
            DataSource = "categoryData",
            CategoryField = "category",
            Title = "市场份额"
        };
        el.Series.Add(new ChartSeries { Name = "份额", ValueField = "percentage" });

        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    [Fact]
    public void ChartElement_AreaChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Area,
            DataSource = "stackedData",
            CategoryField = "quarter"
        };
        el.Series.Add(new ChartSeries { Name = "Product A", ValueField = "productA" });
        el.Series.Add(new ChartSeries { Name = "Product B", ValueField = "productB" });

        Assert.Equal(ChartType.Area, el.ChartType);
    }

    [Fact]
    public void ChartElement_ScatterChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Scatter,
            DataSource = "scatterData",
            CategoryField = "x"
        };
        el.Series.Add(new ChartSeries { Name = "Data Points", ValueField = "y" });

        Assert.Equal(ChartType.Scatter, el.ChartType);
    }

    [Fact]
    public void ChartElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 100 };
        band.Elements.Add(new ChartElement
        {
            ChartType = ChartType.Bar,
            DataSource = "data",
            CategoryField = "category",
            X = 10,
            Y = 10,
            Width = 180,
            Height = 80
        });

        Assert.Single(band.Elements);
        var chart = Assert.IsType<ChartElement>(band.Elements[0]);
        Assert.Equal(ChartType.Bar, chart.ChartType);
    }

    [Fact]
    public void ChartElement_CanBeModified()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Bar,
            DataSource = "old",
            Title = "Old Title"
        };
        
        el.ChartType = ChartType.Line;
        el.DataSource = "new";
        el.Title = "New Title";
        
        Assert.Equal(ChartType.Line, el.ChartType);
        Assert.Equal("new", el.DataSource);
        Assert.Equal("New Title", el.Title);
    }
}

/// <summary>
/// ChartSeries 行为测试
/// </summary>
public class ChartSeriesBehaviorTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var series = new ChartSeries();

        Assert.Equal("", series.Name);
        Assert.Equal("", series.ValueField);
        Assert.Null(series.Color);
    }

    [Fact]
    public void Name_SetAndGet_Works()
    {
        var series = new ChartSeries { Name = "Revenue" };
        Assert.Equal("Revenue", series.Name);
    }

    [Fact]
    public void ValueField_SetAndGet_Works()
    {
        var series = new ChartSeries { ValueField = "amount" };
        Assert.Equal("amount", series.ValueField);
    }

    [Fact]
    public void Color_SetAndGet_Works()
    {
        var series = new ChartSeries { Color = "#FF0000" };
        Assert.Equal("#FF0000", series.Color);
    }

    [Fact]
    public void Color_CanBeCleared()
    {
        var series = new ChartSeries { Color = "#FF0000" };
        series.Color = null;
        Assert.Null(series.Color);
    }

    [Fact]
    public void ChartSeries_FullSetup_Works()
    {
        var series = new ChartSeries
        {
            Name = "销售额",
            ValueField = "totalAmount",
            Color = "#4472C4"
        };

        Assert.Equal("销售额", series.Name);
        Assert.Equal("totalAmount", series.ValueField);
        Assert.Equal("#4472C4", series.Color);
    }
}
