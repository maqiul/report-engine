using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// CrossTabElement 完整字段测试：
///   - CrossTabElement 完整字段（DataSource/RowFields/ColumnFields/Measures/ShowRowTotal/ShowColumnTotal/CellFont/HeaderFont/CellPadding/BorderWidth/BorderColor）
///   - 字段组合行为
/// </summary>
public class CrossTabElementCompleteTests
{
    [Fact]
    public void CrossTabElement_Defaults()
    {
        var c = new CrossTabElement();
        Assert.Equal("", c.DataSource);
        Assert.NotNull(c.RowFields);
        Assert.Empty(c.RowFields);
        Assert.NotNull(c.ColumnFields);
        Assert.Empty(c.ColumnFields);
        Assert.NotNull(c.Measures);
        Assert.Empty(c.Measures);
        Assert.True(c.ShowRowTotal);
        Assert.True(c.ShowColumnTotal);
        Assert.NotNull(c.CellFont);
        Assert.NotNull(c.HeaderFont);
        Assert.True(c.HeaderFont.Bold);
        Assert.Equal(1.0, c.CellPadding);
        Assert.Equal(0.3, c.BorderWidth);
        Assert.Equal("#000000", c.BorderColor);
    }

    [Fact]
    public void CrossTabElement_AllSetters()
    {
        var c = new CrossTabElement
        {
            DataSource = "sales",
            ShowRowTotal = false,
            ShowColumnTotal = false,
            CellPadding = 2.0,
            BorderWidth = 0.5,
            BorderColor = "#333333",
        };
        c.RowFields.Add("region");
        c.ColumnFields.Add("product");
        c.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        c.CellFont = new FontDef { Size = 11 };
        c.HeaderFont = new FontDef { Bold = true, Size = 12 };

        Assert.Equal("sales", c.DataSource);
        Assert.Single(c.RowFields);
        Assert.Single(c.ColumnFields);
        Assert.Single(c.Measures);
        Assert.False(c.ShowRowTotal);
        Assert.False(c.ShowColumnTotal);
        Assert.Equal(11, c.CellFont.Size);
        Assert.True(c.HeaderFont.Bold);
        Assert.Equal(2.0, c.CellPadding);
        Assert.Equal(0.5, c.BorderWidth);
        Assert.Equal("#333333", c.BorderColor);
    }

    [Fact]
    public void CrossTabElement_DataSource_CanBeEmpty()
    {
        var c = new CrossTabElement { DataSource = "" };
        Assert.Equal("", c.DataSource);
    }

    [Fact]
    public void CrossTabElement_DataSource_CanBeSet()
    {
        var c = new CrossTabElement { DataSource = "orders" };
        Assert.Equal("orders", c.DataSource);
    }

    [Fact]
    public void CrossTabElement_RowFields_CanBeEmpty()
    {
        var c = new CrossTabElement();
        Assert.Empty(c.RowFields);
    }

    [Fact]
    public void CrossTabElement_RowFields_CanAddMultiple()
    {
        var c = new CrossTabElement();
        c.RowFields.Add("region");
        c.RowFields.Add("city");
        Assert.Equal(2, c.RowFields.Count);
    }

    [Fact]
    public void CrossTabElement_ColumnFields_CanBeEmpty()
    {
        var c = new CrossTabElement();
        Assert.Empty(c.ColumnFields);
    }

    [Fact]
    public void CrossTabElement_ColumnFields_CanAddMultiple()
    {
        var c = new CrossTabElement();
        c.ColumnFields.Add("year");
        c.ColumnFields.Add("quarter");
        Assert.Equal(2, c.ColumnFields.Count);
    }

    [Fact]
    public void CrossTabElement_Measures_CanBeEmpty()
    {
        var c = new CrossTabElement();
        Assert.Empty(c.Measures);
    }

    [Fact]
    public void CrossTabElement_Measures_CanAddMultiple()
    {
        var c = new CrossTabElement();
        c.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        c.Measures.Add(new CrossTabMeasure { Field = "count", Aggregate = "Count" });
        Assert.Equal(2, c.Measures.Count);
    }

    [Fact]
    public void CrossTabElement_ShowRowTotal_DefaultTrue()
    {
        var c = new CrossTabElement();
        Assert.True(c.ShowRowTotal);
    }

    [Fact]
    public void CrossTabElement_ShowRowTotal_CanBeFalse()
    {
        var c = new CrossTabElement { ShowRowTotal = false };
        Assert.False(c.ShowRowTotal);
    }

    [Fact]
    public void CrossTabElement_ShowColumnTotal_DefaultTrue()
    {
        var c = new CrossTabElement();
        Assert.True(c.ShowColumnTotal);
    }

    [Fact]
    public void CrossTabElement_ShowColumnTotal_CanBeFalse()
    {
        var c = new CrossTabElement { ShowColumnTotal = false };
        Assert.False(c.ShowColumnTotal);
    }

    [Fact]
    public void CrossTabElement_CellFont_DefaultNotNull()
    {
        var c = new CrossTabElement();
        Assert.NotNull(c.CellFont);
        Assert.Equal("SimSun", c.CellFont.Family);
    }

    [Fact]
    public void CrossTabElement_HeaderFont_DefaultBold()
    {
        var c = new CrossTabElement();
        Assert.NotNull(c.HeaderFont);
        Assert.True(c.HeaderFont.Bold);
    }

    [Fact]
    public void CrossTabElement_CellPadding_Default1()
    {
        var c = new CrossTabElement();
        Assert.Equal(1.0, c.CellPadding);
    }

    [Fact]
    public void CrossTabElement_CellPadding_CanBeZero()
    {
        var c = new CrossTabElement { CellPadding = 0 };
        Assert.Equal(0, c.CellPadding);
    }

    [Fact]
    public void CrossTabElement_CellPadding_CanBeDecimal()
    {
        var c = new CrossTabElement { CellPadding = 2.5 };
        Assert.Equal(2.5, c.CellPadding);
    }

    [Fact]
    public void CrossTabElement_BorderWidth_Default03()
    {
        var c = new CrossTabElement();
        Assert.Equal(0.3, c.BorderWidth);
    }

    [Fact]
    public void CrossTabElement_BorderColor_DefaultBlack()
    {
        var c = new CrossTabElement();
        Assert.Equal("#000000", c.BorderColor);
    }

    [Fact]
    public void CrossTabElement_FullCombination()
    {
        var c = new CrossTabElement
        {
            DataSource = "inventory",
            ShowRowTotal = true,
            ShowColumnTotal = false,
            CellPadding = 1.5,
            BorderWidth = 0.4,
            BorderColor = "#666666",
        };
        c.RowFields.Add("warehouse");
        c.ColumnFields.Add("category");
        c.Measures.Add(new CrossTabMeasure { Field = "quantity", Aggregate = "Sum" });

        Assert.Equal("inventory", c.DataSource);
        Assert.Single(c.RowFields);
        Assert.Single(c.ColumnFields);
        Assert.Single(c.Measures);
        Assert.True(c.ShowRowTotal);
        Assert.False(c.ShowColumnTotal);
        Assert.Equal(1.5, c.CellPadding);
    }
}
