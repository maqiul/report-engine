using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// CrossTabMeasure 完整字段测试：
///   - CrossTabMeasure 完整字段（Field/Aggregate/Format/Label）
///   - 字段组合行为
/// </summary>
public class CrossTabMeasureCompleteTests
{
    [Fact]
    public void CrossTabMeasure_Defaults()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("", m.Field);
        Assert.Equal("Sum", m.Aggregate);
        Assert.Null(m.Format);
        Assert.Null(m.Label);
    }

    [Fact]
    public void CrossTabMeasure_AllSetters()
    {
        var m = new CrossTabMeasure
        {
            Field = "amount",
            Aggregate = "Avg",
            Format = "N2",
            Label = "平均金额",
        };
        Assert.Equal("amount", m.Field);
        Assert.Equal("Avg", m.Aggregate);
        Assert.Equal("N2", m.Format);
        Assert.Equal("平均金额", m.Label);
    }

    [Fact]
    public void CrossTabMeasure_Field_CanBeEmpty()
    {
        var m = new CrossTabMeasure { Field = "" };
        Assert.Equal("", m.Field);
    }

    [Fact]
    public void CrossTabMeasure_Field_CanBeFieldName()
    {
        var m = new CrossTabMeasure { Field = "quantity" };
        Assert.Equal("quantity", m.Field);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_CanBeCount()
    {
        var m = new CrossTabMeasure { Aggregate = "Count" };
        Assert.Equal("Count", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_CanBeAvg()
    {
        var m = new CrossTabMeasure { Aggregate = "Avg" };
        Assert.Equal("Avg", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_CanBeMin()
    {
        var m = new CrossTabMeasure { Aggregate = "Min" };
        Assert.Equal("Min", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_CanBeMax()
    {
        var m = new CrossTabMeasure { Aggregate = "Max" };
        Assert.Equal("Max", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_CanBeAnyString()
    {
        var m = new CrossTabMeasure { Aggregate = "Custom" };
        Assert.Equal("Custom", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Format_CanBeNull()
    {
        var m = new CrossTabMeasure { Format = null };
        Assert.Null(m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Format_CanBeEmpty()
    {
        var m = new CrossTabMeasure { Format = "" };
        Assert.Equal("", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Format_CanBeN2()
    {
        var m = new CrossTabMeasure { Format = "N2" };
        Assert.Equal("N2", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Format_CanBeCurrency()
    {
        var m = new CrossTabMeasure { Format = "C" };
        Assert.Equal("C", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Format_CanBeCustom()
    {
        var m = new CrossTabMeasure { Format = "#,##0.00" };
        Assert.Equal("#,##0.00", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Label_CanBeNull()
    {
        var m = new CrossTabMeasure { Label = null };
        Assert.Null(m.Label);
    }

    [Fact]
    public void CrossTabMeasure_Label_CanBeEmpty()
    {
        var m = new CrossTabMeasure { Label = "" };
        Assert.Equal("", m.Label);
    }

    [Fact]
    public void CrossTabMeasure_Label_CanBeChinese()
    {
        var m = new CrossTabMeasure { Label = "总计" };
        Assert.Equal("总计", m.Label);
    }

    [Fact]
    public void CrossTabMeasure_FullCombination()
    {
        var m = new CrossTabMeasure
        {
            Field = "sales",
            Aggregate = "Max",
            Format = "N0",
            Label = "最大销售额",
        };
        Assert.Equal("sales", m.Field);
        Assert.Equal("Max", m.Aggregate);
        Assert.Equal("N0", m.Format);
        Assert.Equal("最大销售额", m.Label);
    }
}
