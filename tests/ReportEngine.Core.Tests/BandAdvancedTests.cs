using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Band 高级属性测试
/// </summary>
public class BandAdvancedTests
{
    // ============== RepeatOnNewPage ==============

    [Fact]
    public void RepeatOnNewPage_DefaultIsFalse()
    {
        var band = new Band();
        Assert.False(band.RepeatOnNewPage);
    }

    [Fact]
    public void RepeatOnNewPage_SetTrue_Works()
    {
        var band = new Band { RepeatOnNewPage = true };
        Assert.True(band.RepeatOnNewPage);
    }

    // ============== DataSource ==============

    [Fact]
    public void DataSource_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.DataSource);
    }

    [Fact]
    public void DataSource_Set_Works()
    {
        var band = new Band { DataSource = "orders" };
        Assert.Equal("orders", band.DataSource);
    }

    // ============== Group ==============

    [Fact]
    public void Group_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.Group);
    }

    [Fact]
    public void Group_Set_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Group = new GroupDef { Expression = "region" }
        };
        Assert.NotNull(band.Group);
        Assert.Equal("region", band.Group.Expression);
    }

    [Fact]
    public void Group_WithKeepTogether_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Group = new GroupDef { Expression = "category", KeepTogether = true }
        };
        Assert.True(band.Group.KeepTogether);
    }

    // ============== MultiColumn ==============

    [Fact]
    public void MultiColumn_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.MultiColumn);
    }

    [Fact]
    public void MultiColumn_Set_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            MultiColumn = new MultiColumnConfig { ColumnCount = 3, ColumnSpacing = 5 }
        };
        Assert.NotNull(band.MultiColumn);
        Assert.Equal(3, band.MultiColumn.ColumnCount);
        Assert.Equal(5, band.MultiColumn.ColumnSpacing);
    }

    [Fact]
    public void MultiColumn_VerticalDirection_Works()
    {
        var band = new Band
        {
            MultiColumn = new MultiColumnConfig { Direction = "Vertical" }
        };
        Assert.Equal("Vertical", band.MultiColumn.Direction);
    }

    // ============== SubBands ==============

    [Fact]
    public void SubBands_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.SubBands);
    }

    [Fact]
    public void SubBands_Set_Works()
    {
        var band = new Band
        {
            Type = BandType.Header,
            SubBands = new List<Band>
            {
                new Band { Type = BandType.Header, Height = 20 },
                new Band { Type = BandType.Header, Height = 30 }
            }
        };
        Assert.NotNull(band.SubBands);
        Assert.Equal(2, band.SubBands.Count);
    }

    [Fact]
    public void SubBands_NestedStructure_Works()
    {
        var parent = new Band
        {
            Type = BandType.Header,
            Height = 50,
            SubBands = new List<Band>()
        };
        parent.SubBands.Add(new Band { Type = BandType.Header, Height = 25 });
        parent.SubBands.Add(new Band { Type = BandType.Header, Height = 25 });

        Assert.Equal(2, parent.SubBands.Count);
        Assert.Equal(25, parent.SubBands[0].Height);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void Band_DetailWithDataSource_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            Height = 30,
            DataSource = "orderItems",
            RepeatOnNewPage = false
        };
        band.Elements.Add(new TextElement { Text = "{{currentRow.productName}}" });

        Assert.Equal(BandType.Detail, band.Type);
        Assert.Equal("orderItems", band.DataSource);
        Assert.Single(band.Elements);
    }

    [Fact]
    public void Band_GroupHeaderWithGroup_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Height = 25,
            Group = new GroupDef { Expression = "department", KeepTogether = true }
        };
        band.Elements.Add(new TextElement { Text = "{{currentRow.department}}" });

        Assert.Equal(BandType.GroupHeader, band.Type);
        Assert.NotNull(band.Group);
        Assert.Equal("department", band.Group.Expression);
        Assert.True(band.Group.KeepTogether);
    }

    [Fact]
    public void Band_DetailWithMultiColumn_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            Height = 40,
            MultiColumn = new MultiColumnConfig
            {
                ColumnCount = 4,
                ColumnSpacing = 3,
                Direction = "Horizontal"
            }
        };

        Assert.Equal(4, band.MultiColumn.ColumnCount);
        Assert.Equal("Horizontal", band.MultiColumn.Direction);
    }

    [Fact]
    public void Band_HeaderWithSubBands_Works()
    {
        var header = new Band
        {
            Type = BandType.Header,
            Height = 60,
            SubBands = new List<Band>
            {
                new Band { Type = BandType.Header, Height = 30 },
                new Band { Type = BandType.Header, Height = 30 }
            }
        };

        Assert.Equal(2, header.SubBands.Count);
        Assert.All(header.SubBands, sb => Assert.Equal(30, sb.Height));
    }
}
