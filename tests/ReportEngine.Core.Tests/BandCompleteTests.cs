using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Band 完整字段测试：
///   - Band 完整字段（Type/Height/RepeatOnNewPage/DataSource/Group/Elements/MultiColumn/SubBands）
///   - 字段组合行为
/// </summary>
public class BandCompleteTests
{
    [Fact]
    public void Band_Defaults()
    {
        var b = new Band();
        Assert.Equal(BandType.Header, b.Type);
        Assert.Equal(0, b.Height);
        Assert.False(b.RepeatOnNewPage);
        Assert.Null(b.DataSource);
        Assert.Null(b.Group);
        Assert.NotNull(b.Elements);
        Assert.Empty(b.Elements);
        Assert.Null(b.MultiColumn);
        Assert.Null(b.SubBands);
    }

    [Fact]
    public void Band_AllSetters()
    {
        var b = new Band
        {
            Type = BandType.Detail,
            Height = 50.5,
            RepeatOnNewPage = true,
            DataSource = "orders",
            Group = new GroupDef { Expression = "category", KeepTogether = false },
            MultiColumn = new MultiColumnConfig { ColumnCount = 3 },
        };
        b.Elements.Add(new TextElement { Text = "test" });
        b.SubBands = new List<Band> { new Band { Type = BandType.Header, Height = 10 } };

        Assert.Equal(BandType.Detail, b.Type);
        Assert.Equal(50.5, b.Height);
        Assert.True(b.RepeatOnNewPage);
        Assert.Equal("orders", b.DataSource);
        Assert.NotNull(b.Group);
        Assert.Equal("category", b.Group.Expression);
        Assert.False(b.Group.KeepTogether);
        Assert.Single(b.Elements);
        Assert.NotNull(b.MultiColumn);
        Assert.Equal(3, b.MultiColumn.ColumnCount);
        Assert.NotNull(b.SubBands);
        Assert.Single(b.SubBands);
    }

    [Fact]
    public void Band_Type_CanBeAll7Values()
    {
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.Header));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.Footer));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.ReportHeader));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.ReportFooter));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.Detail));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.GroupHeader));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.GroupFooter));
    }

    [Fact]
    public void Band_Height_CanBeZero()
    {
        var b = new Band { Height = 0 };
        Assert.Equal(0, b.Height);
    }

    [Fact]
    public void Band_Height_CanBeNegative()
    {
        var b = new Band { Height = -10 };
        Assert.Equal(-10, b.Height);
    }

    [Fact]
    public void Band_Height_CanBeDecimal()
    {
        var b = new Band { Height = 12.5 };
        Assert.Equal(12.5, b.Height);
    }

    [Fact]
    public void Band_RepeatOnNewPage_DefaultFalse()
    {
        var b = new Band();
        Assert.False(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_RepeatOnNewPage_CanBeSet()
    {
        var b = new Band { RepeatOnNewPage = true };
        Assert.True(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_DataSource_CanBeEmpty()
    {
        var b = new Band { DataSource = "" };
        Assert.Equal("", b.DataSource);
    }

    [Fact]
    public void Band_DataSource_CanBeNull()
    {
        var b = new Band { DataSource = null };
        Assert.Null(b.DataSource);
    }

    [Fact]
    public void Band_Group_CanBeNull()
    {
        var b = new Band { Group = null };
        Assert.Null(b.Group);
    }

    [Fact]
    public void Band_Group_CanBeSet()
    {
        var b = new Band { Group = new GroupDef { Expression = "region" } };
        Assert.NotNull(b.Group);
        Assert.Equal("region", b.Group.Expression);
    }

    [Fact]
    public void Band_Elements_CanAddMultiple()
    {
        var b = new Band();
        b.Elements.Add(new TextElement { Text = "a" });
        b.Elements.Add(new ImageElement { Source = "img.png" });
        b.Elements.Add(new LineElement { LineWidth = 2 });
        Assert.Equal(3, b.Elements.Count);
    }

    [Fact]
    public void Band_Elements_CanBeCleared()
    {
        var b = new Band();
        b.Elements.Add(new TextElement());
        b.Elements.Clear();
        Assert.Empty(b.Elements);
    }

    [Fact]
    public void Band_MultiColumn_CanBeNull()
    {
        var b = new Band { MultiColumn = null };
        Assert.Null(b.MultiColumn);
    }

    [Fact]
    public void Band_MultiColumn_CanBeSet()
    {
        var b = new Band { MultiColumn = new MultiColumnConfig { ColumnCount = 4, ColumnSpacing = 5 } };
        Assert.NotNull(b.MultiColumn);
        Assert.Equal(4, b.MultiColumn.ColumnCount);
        Assert.Equal(5, b.MultiColumn.ColumnSpacing);
    }

    [Fact]
    public void Band_SubBands_CanBeNull()
    {
        var b = new Band { SubBands = null };
        Assert.Null(b.SubBands);
    }

    [Fact]
    public void Band_SubBands_CanAddMultiple()
    {
        var b = new Band();
        b.SubBands = new List<Band>
        {
            new Band { Type = BandType.Header, Height = 10 },
            new Band { Type = BandType.Detail, Height = 20 },
            new Band { Type = BandType.Footer, Height = 15 },
        };
        Assert.Equal(3, b.SubBands.Count);
    }

    [Fact]
    public void Band_SubBands_CanBeNested()
    {
        var b = new Band();
        b.SubBands = new List<Band>
        {
            new Band
            {
                Type = BandType.Header,
                SubBands = new List<Band>
                {
                    new Band { Type = BandType.Detail, Height = 5 },
                },
            },
        };
        Assert.NotNull(b.SubBands[0].SubBands);
        Assert.Single(b.SubBands[0].SubBands);
    }
}
