using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// MultiColumnConfig 完整字段测试：
///   - MultiColumnConfig 完整字段（ColumnCount/ColumnSpacing/Direction）
///   - 字段组合行为
/// </summary>
public class MultiColumnConfigCompleteTests
{
    [Fact]
    public void MultiColumnConfig_Defaults()
    {
        var m = new MultiColumnConfig();
        Assert.Equal(2, m.ColumnCount);
        Assert.Equal(5, m.ColumnSpacing);
        Assert.Equal("Horizontal", m.Direction);
    }

    [Fact]
    public void MultiColumnConfig_AllSetters()
    {
        var m = new MultiColumnConfig
        {
            ColumnCount = 3,
            ColumnSpacing = 10.5,
            Direction = "Vertical",
        };
        Assert.Equal(3, m.ColumnCount);
        Assert.Equal(10.5, m.ColumnSpacing);
        Assert.Equal("Vertical", m.Direction);
    }

    [Fact]
    public void MultiColumnConfig_ColumnCount_CanBeOne()
    {
        var m = new MultiColumnConfig { ColumnCount = 1 };
        Assert.Equal(1, m.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnCount_CanBeLarge()
    {
        var m = new MultiColumnConfig { ColumnCount = 10 };
        Assert.Equal(10, m.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnCount_CanBeZero()
    {
        var m = new MultiColumnConfig { ColumnCount = 0 };
        Assert.Equal(0, m.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_CanBeZero()
    {
        var m = new MultiColumnConfig { ColumnSpacing = 0 };
        Assert.Equal(0, m.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_CanBeDecimal()
    {
        var m = new MultiColumnConfig { ColumnSpacing = 2.5 };
        Assert.Equal(2.5, m.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_CanBeLarge()
    {
        var m = new MultiColumnConfig { ColumnSpacing = 50 };
        Assert.Equal(50, m.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_Direction_CanBeVertical()
    {
        var m = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", m.Direction);
    }

    [Fact]
    public void MultiColumnConfig_Direction_CanBeAnyString()
    {
        var m = new MultiColumnConfig { Direction = "Custom" };
        Assert.Equal("Custom", m.Direction);
    }

    [Fact]
    public void MultiColumnConfig_Direction_CanBeEmpty()
    {
        var m = new MultiColumnConfig { Direction = "" };
        Assert.Equal("", m.Direction);
    }

    [Fact]
    public void MultiColumnConfig_FullCombination()
    {
        var m = new MultiColumnConfig
        {
            ColumnCount = 4,
            ColumnSpacing = 8,
            Direction = "Vertical",
        };
        Assert.Equal(4, m.ColumnCount);
        Assert.Equal(8, m.ColumnSpacing);
        Assert.Equal("Vertical", m.Direction);
    }
}
