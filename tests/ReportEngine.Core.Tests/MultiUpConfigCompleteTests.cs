using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// MultiUpConfig 完整字段测试：
///   - MultiUpConfig 完整字段（Rows/Columns/HSpacing/VSpacing/Direction/Count）
///   - Count 计算行为
/// </summary>
public class MultiUpConfigCompleteTests
{
    [Fact]
    public void MultiUpConfig_Defaults()
    {
        var m = new MultiUpConfig();
        Assert.Equal(2, m.Rows);
        Assert.Equal(2, m.Columns);
        Assert.Equal(0, m.HSpacing);
        Assert.Equal(0, m.VSpacing);
        Assert.Equal("Horizontal", m.Direction);
        Assert.Equal(4, m.Count);
    }

    [Fact]
    public void MultiUpConfig_AllSetters()
    {
        var m = new MultiUpConfig
        {
            Rows = 3,
            Columns = 4,
            HSpacing = 5.5,
            VSpacing = 3.2,
            Direction = "Vertical",
        };
        Assert.Equal(3, m.Rows);
        Assert.Equal(4, m.Columns);
        Assert.Equal(5.5, m.HSpacing);
        Assert.Equal(3.2, m.VSpacing);
        Assert.Equal("Vertical", m.Direction);
        Assert.Equal(12, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_CalculatedFromRowsAndColumns()
    {
        var m = new MultiUpConfig { Rows = 5, Columns = 3 };
        Assert.Equal(15, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_UpdatesWhenRowsChanges()
    {
        var m = new MultiUpConfig { Rows = 2, Columns = 3 };
        Assert.Equal(6, m.Count);
        m.Rows = 4;
        Assert.Equal(12, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_UpdatesWhenColumnsChanges()
    {
        var m = new MultiUpConfig { Rows = 2, Columns = 3 };
        Assert.Equal(6, m.Count);
        m.Columns = 5;
        Assert.Equal(10, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Rows_CanBeOne()
    {
        var m = new MultiUpConfig { Rows = 1, Columns = 4 };
        Assert.Equal(4, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Columns_CanBeOne()
    {
        var m = new MultiUpConfig { Rows = 4, Columns = 1 };
        Assert.Equal(4, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Rows_CanBeZero()
    {
        var m = new MultiUpConfig { Rows = 0, Columns = 4 };
        Assert.Equal(0, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Columns_CanBeZero()
    {
        var m = new MultiUpConfig { Rows = 4, Columns = 0 };
        Assert.Equal(0, m.Count);
    }

    [Fact]
    public void MultiUpConfig_HSpacing_CanBeDecimal()
    {
        var m = new MultiUpConfig { HSpacing = 2.5 };
        Assert.Equal(2.5, m.HSpacing);
    }

    [Fact]
    public void MultiUpConfig_VSpacing_CanBeDecimal()
    {
        var m = new MultiUpConfig { VSpacing = 1.8 };
        Assert.Equal(1.8, m.VSpacing);
    }

    [Fact]
    public void MultiUpConfig_Direction_CanBeVertical()
    {
        var m = new MultiUpConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", m.Direction);
    }

    [Fact]
    public void MultiUpConfig_Direction_CanBeAnyString()
    {
        var m = new MultiUpConfig { Direction = "Custom" };
        Assert.Equal("Custom", m.Direction);
    }

    [Fact]
    public void MultiUpConfig_LargeGrid()
    {
        var m = new MultiUpConfig { Rows = 10, Columns = 10 };
        Assert.Equal(100, m.Count);
    }
}
