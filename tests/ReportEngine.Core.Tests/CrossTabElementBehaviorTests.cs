using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// CrossTabElement 行为测试：
///   - 默认值
///   - 数据源
///   - 行字段/列字段
///   - 度量
///   - 合计显示
///   - 字体/内边距/边框
/// </summary>
public class CrossTabElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new CrossTabElement();

        Assert.Equal("", el.DataSource);
        Assert.NotNull(el.RowFields);
        Assert.Empty(el.RowFields);
        Assert.NotNull(el.ColumnFields);
        Assert.Empty(el.ColumnFields);
        Assert.NotNull(el.Measures);
        Assert.Empty(el.Measures);
        Assert.True(el.ShowRowTotal);
        Assert.True(el.ShowColumnTotal);
        Assert.NotNull(el.CellFont);
        Assert.NotNull(el.HeaderFont);
        Assert.True(el.HeaderFont.Bold);
        Assert.Equal(1.0, el.CellPadding);
        Assert.Equal(0.3, el.BorderWidth);
        Assert.Equal("#000000", el.BorderColor);
    }

    // ============== DataSource ==============

    [Fact]
    public void DataSource_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.Equal("", el.DataSource);
    }

    [Fact]
    public void DataSource_Set_Works()
    {
        var el = new CrossTabElement { DataSource = "salesData" };
        Assert.Equal("salesData", el.DataSource);
    }

    // ============== RowFields ==============

    [Fact]
    public void RowFields_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.Empty(el.RowFields);
    }

    [Fact]
    public void RowFields_Add_Works()
    {
        var el = new CrossTabElement();
        el.RowFields.Add("region");
        Assert.Single(el.RowFields);
    }

    [Fact]
    public void RowFields_AddMultiple_Works()
    {
        var el = new CrossTabElement();
        el.RowFields.Add("region");
        el.RowFields.Add("city");
        Assert.Equal(2, el.RowFields.Count);
    }

    // ============== ColumnFields ==============

    [Fact]
    public void ColumnFields_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.Empty(el.ColumnFields);
    }

    [Fact]
    public void ColumnFields_Add_Works()
    {
        var el = new CrossTabElement();
        el.ColumnFields.Add("product");
        Assert.Single(el.ColumnFields);
    }

    [Fact]
    public void ColumnFields_AddMultiple_Works()
    {
        var el = new CrossTabElement();
        el.ColumnFields.Add("year");
        el.ColumnFields.Add("quarter");
        Assert.Equal(2, el.ColumnFields.Count);
    }

    // ============== Measures ==============

    [Fact]
    public void Measures_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.Empty(el.Measures);
    }

    [Fact]
    public void Measures_Add_Works()
    {
        var el = new CrossTabElement();
        el.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        Assert.Single(el.Measures);
    }

    [Fact]
    public void Measures_AddMultiple_Works()
    {
        var el = new CrossTabElement();
        el.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        el.Measures.Add(new CrossTabMeasure { Field = "quantity", Aggregate = "Sum" });
        Assert.Equal(2, el.Measures.Count);
    }

    // ============== ShowRowTotal ==============

    [Fact]
    public void ShowRowTotal_TrueByDefault()
    {
        var el = new CrossTabElement();
        Assert.True(el.ShowRowTotal);
    }

    [Fact]
    public void ShowRowTotal_SetFalse_Works()
    {
        var el = new CrossTabElement { ShowRowTotal = false };
        Assert.False(el.ShowRowTotal);
    }

    // ============== ShowColumnTotal ==============

    [Fact]
    public void ShowColumnTotal_TrueByDefault()
    {
        var el = new CrossTabElement();
        Assert.True(el.ShowColumnTotal);
    }

    [Fact]
    public void ShowColumnTotal_SetFalse_Works()
    {
        var el = new CrossTabElement { ShowColumnTotal = false };
        Assert.False(el.ShowColumnTotal);
    }

    // ============== CellFont ==============

    [Fact]
    public void CellFont_NotNull_ByDefault()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.CellFont);
    }

    [Fact]
    public void CellFont_DefaultValues_AreCorrect()
    {
        var el = new CrossTabElement();
        Assert.Equal("SimSun", el.CellFont.Family);
        Assert.Equal(10, el.CellFont.Size);
        Assert.False(el.CellFont.Bold);
    }

    // ============== HeaderFont ==============

    [Fact]
    public void HeaderFont_NotNull_ByDefault()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.HeaderFont);
    }

    [Fact]
    public void HeaderFont_DefaultIsBold()
    {
        var el = new CrossTabElement();
        Assert.True(el.HeaderFont.Bold);
    }

    // ============== CellPadding ==============

    [Fact]
    public void CellPadding_DefaultIs1()
    {
        var el = new CrossTabElement();
        Assert.Equal(1.0, el.CellPadding);
    }

    [Fact]
    public void CellPadding_Set_Works()
    {
        var el = new CrossTabElement { CellPadding = 2.0 };
        Assert.Equal(2.0, el.CellPadding);
    }

    // ============== BorderWidth ==============

    [Fact]
    public void BorderWidth_DefaultIs03()
    {
        var el = new CrossTabElement();
        Assert.Equal(0.3, el.BorderWidth);
    }

    [Fact]
    public void BorderWidth_Set_Works()
    {
        var el = new CrossTabElement { BorderWidth = 0.5 };
        Assert.Equal(0.5, el.BorderWidth);
    }

    // ============== BorderColor ==============

    [Fact]
    public void BorderColor_DefaultIsBlack()
    {
        var el = new CrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void BorderColor_Set_Works()
    {
        var el = new CrossTabElement { BorderColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", el.BorderColor);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void CrossTabElement_SimplePivot_Works()
    {
        var el = new CrossTabElement
        {
            DataSource = "salesData",
            ShowRowTotal = true,
            ShowColumnTotal = true
        };
        el.RowFields.Add("region");
        el.ColumnFields.Add("product");
        el.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });

        Assert.Equal("salesData", el.DataSource);
        Assert.Single(el.RowFields);
        Assert.Single(el.ColumnFields);
        Assert.Single(el.Measures);
    }

    [Fact]
    public void CrossTabElement_MultiLevelPivot_Works()
    {
        var el = new CrossTabElement { DataSource = "data" };
        el.RowFields.Add("year");
        el.RowFields.Add("quarter");
        el.RowFields.Add("month");
        el.ColumnFields.Add("region");
        el.Measures.Add(new CrossTabMeasure { Field = "revenue", Aggregate = "Sum" });
        el.Measures.Add(new CrossTabMeasure { Field = "cost", Aggregate = "Sum" });

        Assert.Equal(3, el.RowFields.Count);
        Assert.Equal(2, el.Measures.Count);
    }

    [Fact]
    public void CrossTabElement_NoTotals_Works()
    {
        var el = new CrossTabElement
        {
            DataSource = "data",
            ShowRowTotal = false,
            ShowColumnTotal = false
        };

        Assert.False(el.ShowRowTotal);
        Assert.False(el.ShowColumnTotal);
    }

    [Fact]
    public void CrossTabElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 100 };
        band.Elements.Add(new CrossTabElement
        {
            DataSource = "pivotData",
            X = 10,
            Y = 10,
            Width = 180,
            Height = 80
        });

        Assert.Single(band.Elements);
        var crosstab = Assert.IsType<CrossTabElement>(band.Elements[0]);
        Assert.Equal("pivotData", crosstab.DataSource);
    }
}

/// <summary>
/// CrossTabMeasure 行为测试
/// </summary>
public class CrossTabMeasureBehaviorTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var m = new CrossTabMeasure();

        Assert.Equal("", m.Field);
        Assert.Equal("Sum", m.Aggregate);
        Assert.Null(m.Format);
    }

    [Fact]
    public void Field_SetAndGet_Works()
    {
        var m = new CrossTabMeasure { Field = "amount" };
        Assert.Equal("amount", m.Field);
    }

    [Fact]
    public void Aggregate_DefaultIsSum()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("Sum", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetCount_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Count" };
        Assert.Equal("Count", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetAvg_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Avg" };
        Assert.Equal("Avg", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetMin_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Min" };
        Assert.Equal("Min", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetMax_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Max" };
        Assert.Equal("Max", m.Aggregate);
    }

    [Fact]
    public void Format_NullByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Format);
    }

    [Fact]
    public void Format_Set_Works()
    {
        var m = new CrossTabMeasure { Format = "N2" };
        Assert.Equal("N2", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_FullSetup_Works()
    {
        var m = new CrossTabMeasure
        {
            Field = "revenue",
            Aggregate = "Sum",
            Format = "C2"
        };

        Assert.Equal("revenue", m.Field);
        Assert.Equal("Sum", m.Aggregate);
        Assert.Equal("C2", m.Format);
    }
}
