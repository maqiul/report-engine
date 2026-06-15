using System.Collections.Generic;
using System.Linq;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ChartElement / ChartType / ChartSeries 行为测试：
///   - ChartElement 默认值（ChartType=Bar, DataSource="", CategoryField="", Series=空）
///   - ChartType 枚举值（Bar/Line/Pie/Area/Scatter）
///   - ChartSeries 默认值（Name="", ValueField="", Color=null）
///   - ChartSeries 多个互不影响
///   - Series 增删改
/// </summary>
public class ChartElementTests
{
    [Fact]
    public void ChartElement_Defaults()
    {
        var c = new ChartElement();
        Assert.Equal(ChartType.Bar, c.ChartType);
        Assert.Equal("", c.DataSource);
        Assert.Equal("", c.CategoryField);
        Assert.NotNull(c.Series);
        Assert.Empty(c.Series);
        Assert.Null(c.Title);
    }

    [Fact]
    public void ChartType_HasFiveValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(ChartType), ChartType.Bar));
        Assert.True(System.Enum.IsDefined(typeof(ChartType), ChartType.Line));
        Assert.True(System.Enum.IsDefined(typeof(ChartType), ChartType.Pie));
        Assert.True(System.Enum.IsDefined(typeof(ChartType), ChartType.Area));
        Assert.True(System.Enum.IsDefined(typeof(ChartType), ChartType.Scatter));
    }

    [Fact]
    public void ChartSeries_Defaults()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.Name);
        Assert.Equal("", s.ValueField);
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartElement_ChartType_CanBeChanged()
    {
        var c = new ChartElement();
        c.ChartType = ChartType.Pie;
        Assert.Equal(ChartType.Pie, c.ChartType);
    }

    [Fact]
    public void ChartElement_DataSource_CanBeSet()
    {
        var c = new ChartElement { DataSource = "orders" };
        Assert.Equal("orders", c.DataSource);
    }

    [Fact]
    public void ChartElement_CategoryField_CanBeSet()
    {
        var c = new ChartElement { CategoryField = "month" };
        Assert.Equal("month", c.CategoryField);
    }

    [Fact]
    public void ChartElement_Title_CanBeSet()
    {
        var c = new ChartElement { Title = "Sales 2025" };
        Assert.Equal("Sales 2025", c.Title);
    }

    [Fact]
    public void ChartElement_Series_AddAndRead()
    {
        var c = new ChartElement();
        c.Series.Add(new ChartSeries { Name = "Sales", ValueField = "amount" });
        c.Series.Add(new ChartSeries { Name = "Cost", ValueField = "cost", Color = "#FF0000" });
        Assert.Equal(2, c.Series.Count);
        Assert.Equal("Sales", c.Series[0].Name);
        Assert.Equal("amount", c.Series[0].ValueField);
        Assert.Null(c.Series[0].Color);
        Assert.Equal("#FF0000", c.Series[1].Color);
    }

    [Fact]
    public void ChartElement_Series_RemoveAt()
    {
        var c = new ChartElement();
        c.Series.Add(new ChartSeries { Name = "A" });
        c.Series.Add(new ChartSeries { Name = "B" });
        c.Series.RemoveAt(0);
        Assert.Single(c.Series);
        Assert.Equal("B", c.Series[0].Name);
    }

    [Fact]
    public void ChartElement_InheritsReportElementDefaults()
    {
        var c = new ChartElement();
        Assert.NotNull(c.Id);
        Assert.NotEmpty(c.Id);
        Assert.True(c.Visible);
    }

    [Fact]
    public void ChartSeries_Color_DefaultsToNull()
    {
        var s = new ChartSeries();
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_Color_CanBeSetToHexString()
    {
        var s = new ChartSeries { Color = "#00FF00" };
        Assert.Equal("#00FF00", s.Color);
    }

    [Fact]
    public void ChartElement_Series_Linq_Query()
    {
        var c = new ChartElement();
        c.Series.Add(new ChartSeries { Name = "A" });
        c.Series.Add(new ChartSeries { Name = "B" });
        c.Series.Add(new ChartSeries { Name = "C" });
        var names = c.Series.Select(s => s.Name).ToList();
        Assert.Equal(new[] { "A", "B", "C" }, names);
    }
}
