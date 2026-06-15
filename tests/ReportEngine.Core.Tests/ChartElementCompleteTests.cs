using System;
using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ChartElement / ChartSeries / ChartType 完整字段测试：
///   - ChartElement 完整字段（ChartType/DataSource/CategoryField/Series/Title）
///   - ChartSeries 完整字段（Name/ValueField/Color）
///   - ChartType 枚举 5 值
/// </summary>
public class ChartElementCompleteTests
{
    // ============== ChartElement ==============

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
    public void ChartElement_AllSetters()
    {
        var c = new ChartElement
        {
            ChartType = ChartType.Line,
            DataSource = "sales",
            CategoryField = "month",
            Title = "月度销售趋势",
        };
        c.Series.Add(new ChartSeries { Name = "收入", ValueField = "revenue" });
        c.Series.Add(new ChartSeries { Name = "支出", ValueField = "expense", Color = "#FF0000" });

        Assert.Equal(ChartType.Line, c.ChartType);
        Assert.Equal("sales", c.DataSource);
        Assert.Equal("month", c.CategoryField);
        Assert.Equal("月度销售趋势", c.Title);
        Assert.Equal(2, c.Series.Count);
    }

    [Fact]
    public void ChartElement_ChartType_DefaultBar()
    {
        var c = new ChartElement();
        Assert.Equal(ChartType.Bar, c.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_CanBeLine()
    {
        var c = new ChartElement { ChartType = ChartType.Line };
        Assert.Equal(ChartType.Line, c.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_CanBePie()
    {
        var c = new ChartElement { ChartType = ChartType.Pie };
        Assert.Equal(ChartType.Pie, c.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_CanBeArea()
    {
        var c = new ChartElement { ChartType = ChartType.Area };
        Assert.Equal(ChartType.Area, c.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_CanBeScatter()
    {
        var c = new ChartElement { ChartType = ChartType.Scatter };
        Assert.Equal(ChartType.Scatter, c.ChartType);
    }

    [Fact]
    public void ChartElement_DataSource_CanBeEmpty()
    {
        var c = new ChartElement { DataSource = "" };
        Assert.Equal("", c.DataSource);
    }

    [Fact]
    public void ChartElement_DataSource_CanBeSet()
    {
        var c = new ChartElement { DataSource = "orders" };
        Assert.Equal("orders", c.DataSource);
    }

    [Fact]
    public void ChartElement_CategoryField_CanBeEmpty()
    {
        var c = new ChartElement { CategoryField = "" };
        Assert.Equal("", c.CategoryField);
    }

    [Fact]
    public void ChartElement_CategoryField_CanBeSet()
    {
        var c = new ChartElement { CategoryField = "product" };
        Assert.Equal("product", c.CategoryField);
    }

    [Fact]
    public void ChartElement_Series_CanBeEmpty()
    {
        var c = new ChartElement();
        Assert.Empty(c.Series);
    }

    [Fact]
    public void ChartElement_Series_CanAddOne()
    {
        var c = new ChartElement();
        c.Series.Add(new ChartSeries { Name = "s1", ValueField = "v1" });
        Assert.Single(c.Series);
    }

    [Fact]
    public void ChartElement_Series_CanAddMultiple()
    {
        var c = new ChartElement();
        c.Series.Add(new ChartSeries { Name = "s1", ValueField = "v1" });
        c.Series.Add(new ChartSeries { Name = "s2", ValueField = "v2" });
        c.Series.Add(new ChartSeries { Name = "s3", ValueField = "v3" });
        Assert.Equal(3, c.Series.Count);
    }

    [Fact]
    public void ChartElement_Title_CanBeNull()
    {
        var c = new ChartElement { Title = null };
        Assert.Null(c.Title);
    }

    [Fact]
    public void ChartElement_Title_CanBeEmpty()
    {
        var c = new ChartElement { Title = "" };
        Assert.Equal("", c.Title);
    }

    [Fact]
    public void ChartElement_Title_CanBeChinese()
    {
        var c = new ChartElement { Title = "销售报表" };
        Assert.Equal("销售报表", c.Title);
    }

    // ============== ChartSeries ==============

    [Fact]
    public void ChartSeries_Defaults()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.Name);
        Assert.Equal("", s.ValueField);
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_AllSetters()
    {
        var s = new ChartSeries
        {
            Name = "收入",
            ValueField = "revenue",
            Color = "#00FF00",
        };
        Assert.Equal("收入", s.Name);
        Assert.Equal("revenue", s.ValueField);
        Assert.Equal("#00FF00", s.Color);
    }

    [Fact]
    public void ChartSeries_Name_CanBeEmpty()
    {
        var s = new ChartSeries { Name = "" };
        Assert.Equal("", s.Name);
    }

    [Fact]
    public void ChartSeries_ValueField_CanBeEmpty()
    {
        var s = new ChartSeries { ValueField = "" };
        Assert.Equal("", s.ValueField);
    }

    [Fact]
    public void ChartSeries_Color_CanBeNull()
    {
        var s = new ChartSeries { Color = null };
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_Color_CanBeHex()
    {
        var s = new ChartSeries { Color = "#123456" };
        Assert.Equal("#123456", s.Color);
    }

    [Fact]
    public void ChartSeries_Color_CanBeHex8()
    {
        var s = new ChartSeries { Color = "#80FF0000" };
        Assert.Equal("#80FF0000", s.Color);
    }

    // ============== ChartType ==============

    [Fact]
    public void ChartType_Has5Values()
    {
        Assert.Equal(5, Enum.GetValues(typeof(ChartType)).Length);
    }

    [Fact]
    public void ChartType_HasBar()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Bar));
    }

    [Fact]
    public void ChartType_HasLine()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Line));
    }

    [Fact]
    public void ChartType_HasPie()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Pie));
    }

    [Fact]
    public void ChartType_HasArea()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Area));
    }

    [Fact]
    public void ChartType_HasScatter()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Scatter));
    }

    [Fact]
    public void ChartType_DefaultIsBar()
    {
        Assert.Equal(ChartType.Bar, default(ChartType));
    }

    [Fact]
    public void ChartElement_FullCombination()
    {
        var c = new ChartElement
        {
            ChartType = ChartType.Pie,
            DataSource = "categories",
            CategoryField = "name",
            Title = "分类占比",
        };
        c.Series.Add(new ChartSeries { Name = "占比", ValueField = "percentage", Color = "#4488CC" });

        Assert.Equal(ChartType.Pie, c.ChartType);
        Assert.Equal("categories", c.DataSource);
        Assert.Equal("name", c.CategoryField);
        Assert.Equal("分类占比", c.Title);
        Assert.Single(c.Series);
        Assert.Equal("#4488CC", c.Series[0].Color);
    }
}
