using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// MultiUpConfig 行为测试：
///   - 默认值
///   - 行列数
///   - 间距
///   - 方向
///   - Count 计算
/// </summary>
public class MultiUpConfigBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var config = new MultiUpConfig();

        Assert.Equal(2, config.Rows);
        Assert.Equal(2, config.Columns);
        Assert.Equal(0, config.HSpacing);
        Assert.Equal(0, config.VSpacing);
        Assert.Equal("Horizontal", config.Direction);
    }

    // ============== Rows ==============

    [Fact]
    public void Rows_DefaultIs1()
    {
        var config = new MultiUpConfig();
        Assert.Equal(2, config.Rows);
    }

    [Fact]
    public void Rows_Set2_Works()
    {
        var config = new MultiUpConfig { Rows = 2 };
        Assert.Equal(2, config.Rows);
    }

    [Fact]
    public void Rows_Set3_Works()
    {
        var config = new MultiUpConfig { Rows = 3 };
        Assert.Equal(3, config.Rows);
    }

    [Fact]
    public void Rows_SetLarge_Works()
    {
        var config = new MultiUpConfig { Rows = 10 };
        Assert.Equal(10, config.Rows);
    }

    // ============== Columns ==============

    [Fact]
    public void Columns_DefaultIs1()
    {
        var config = new MultiUpConfig();
        Assert.Equal(2, config.Columns);
    }

    [Fact]
    public void Columns_Set2_Works()
    {
        var config = new MultiUpConfig { Columns = 2 };
        Assert.Equal(2, config.Columns);
    }

    [Fact]
    public void Columns_Set3_Works()
    {
        var config = new MultiUpConfig { Columns = 3 };
        Assert.Equal(3, config.Columns);
    }

    [Fact]
    public void Columns_SetLarge_Works()
    {
        var config = new MultiUpConfig { Columns = 10 };
        Assert.Equal(10, config.Columns);
    }

    // ============== HSpacing ==============

    [Fact]
    public void HSpacing_DefaultIs0()
    {
        var config = new MultiUpConfig();
        Assert.Equal(0, config.HSpacing);
    }

    [Fact]
    public void HSpacing_SetSmall_Works()
    {
        var config = new MultiUpConfig { HSpacing = 2 };
        Assert.Equal(2, config.HSpacing);
    }

    [Fact]
    public void HSpacing_SetLarge_Works()
    {
        var config = new MultiUpConfig { HSpacing = 10 };
        Assert.Equal(10, config.HSpacing);
    }

    [Fact]
    public void HSpacing_SetDecimal_Works()
    {
        var config = new MultiUpConfig { HSpacing = 5.5 };
        Assert.Equal(5.5, config.HSpacing);
    }

    // ============== VSpacing ==============

    [Fact]
    public void VSpacing_DefaultIs0()
    {
        var config = new MultiUpConfig();
        Assert.Equal(0, config.VSpacing);
    }

    [Fact]
    public void VSpacing_SetSmall_Works()
    {
        var config = new MultiUpConfig { VSpacing = 2 };
        Assert.Equal(2, config.VSpacing);
    }

    [Fact]
    public void VSpacing_SetLarge_Works()
    {
        var config = new MultiUpConfig { VSpacing = 10 };
        Assert.Equal(10, config.VSpacing);
    }

    [Fact]
    public void VSpacing_SetDecimal_Works()
    {
        var config = new MultiUpConfig { VSpacing = 5.5 };
        Assert.Equal(5.5, config.VSpacing);
    }

    // ============== Direction ==============

    [Fact]
    public void Direction_NullByDefault()
    {
        var config = new MultiUpConfig();
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void Direction_SetHorizontal_Works()
    {
        var config = new MultiUpConfig { Direction = "Horizontal" };
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var config = new MultiUpConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", config.Direction);
    }

    [Fact]
    public void Direction_AnyString_Accepted()
    {
        var config = new MultiUpConfig { Direction = "Custom" };
        Assert.Equal("Custom", config.Direction);
    }

    [Fact]
    public void Direction_CanBeCleared()
    {
        var config = new MultiUpConfig { Direction = "Horizontal" };
        config.Direction = null;
        Assert.Null(config.Direction);
    }

    // ============== Count 计算 ==============

    [Fact]
    public void Count_1x1_Returns1()
    {
        var config = new MultiUpConfig { Rows = 1, Columns = 1 };
        Assert.Equal(1, config.Count);
    }

    [Fact]
    public void Count_2x2_Returns4()
    {
        var config = new MultiUpConfig { Rows = 2, Columns = 2 };
        Assert.Equal(4, config.Count);
    }

    [Fact]
    public void Count_2x3_Returns6()
    {
        var config = new MultiUpConfig { Rows = 2, Columns = 3 };
        Assert.Equal(6, config.Count);
    }

    [Fact]
    public void Count_3x3_Returns9()
    {
        var config = new MultiUpConfig { Rows = 3, Columns = 3 };
        Assert.Equal(9, config.Count);
    }

    [Fact]
    public void Count_7x3_Returns21()
    {
        var config = new MultiUpConfig { Rows = 7, Columns = 3 };
        Assert.Equal(21, config.Count);
    }

    [Fact]
    public void Count_10x10_Returns100()
    {
        var config = new MultiUpConfig { Rows = 10, Columns = 10 };
        Assert.Equal(100, config.Count);
    }

    [Fact]
    public void Count_Asymmetric_Works()
    {
        var config = new MultiUpConfig { Rows = 4, Columns = 5 };
        Assert.Equal(20, config.Count);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void MultiUpConfig_FullSetup_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 3,
            Columns = 2,
            HSpacing = 5,
            VSpacing = 10,
            Direction = "Horizontal"
        };

        Assert.Equal(3, config.Rows);
        Assert.Equal(2, config.Columns);
        Assert.Equal(5, config.HSpacing);
        Assert.Equal(10, config.VSpacing);
        Assert.Equal("Horizontal", config.Direction);
        Assert.Equal(6, config.Count);
    }

    [Fact]
    public void MultiUpConfig_LabelPrinting_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 7,
            Columns = 3,
            HSpacing = 2,
            VSpacing = 2
        };

        Assert.Equal(21, config.Count);
    }

    [Fact]
    public void MultiUpConfig_BusinessCards_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 2,
            Columns = 5,
            HSpacing = 0,
            VSpacing = 0
        };

        Assert.Equal(10, config.Count);
    }

    [Fact]
    public void MultiUpConfig_SingleCopy_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 1,
            Columns = 1,
            HSpacing = 0,
            VSpacing = 0
        };

        Assert.Equal(1, config.Count);
    }

    [Fact]
    public void MultiUpConfig_CanBeModified()
    {
        var config = new MultiUpConfig { Rows = 2, Columns = 2 };
        
        config.Rows = 3;
        config.Columns = 3;
        config.HSpacing = 5;
        config.VSpacing = 5;
        config.Direction = "Vertical";
        
        Assert.Equal(3, config.Rows);
        Assert.Equal(3, config.Columns);
        Assert.Equal(5, config.HSpacing);
        Assert.Equal(5, config.VSpacing);
        Assert.Equal("Vertical", config.Direction);
        Assert.Equal(9, config.Count);
    }

    [Fact]
    public void MultiUpConfig_WithPageInfo_Works()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            MultiUp = new MultiUpConfig
            {
                Rows = 2,
                Columns = 2,
                HSpacing = 5,
                VSpacing = 5,
                Direction = "Horizontal"
            }
        };

        Assert.NotNull(page.MultiUp);
        Assert.Equal(4, page.MultiUp.Count);
        Assert.Equal("Horizontal", page.MultiUp.Direction);
    }
}
