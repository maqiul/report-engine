using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Band 行为测试：
///   - 默认值
///   - SubBands 嵌套
///   - MultiColumn 多栏
///   - Group 分组
///   - Elements 操作
/// </summary>
public class BandBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var band = new Band();

        Assert.Equal(BandType.Header, band.Type);
        Assert.Equal(0, band.Height);
        Assert.Null(band.DataSource);
        Assert.Null(band.Group);
        Assert.Null(band.MultiColumn);
        Assert.Null(band.SubBands);
        Assert.NotNull(band.Elements);
        Assert.Empty(band.Elements);
    }

    // ============== Type ==============

    [Fact]
    public void Type_AllBandTypes_Accepted()
    {
        var band = new Band();

        band.Type = BandType.Header;
        Assert.Equal(BandType.Header, band.Type);

        band.Type = BandType.Footer;
        Assert.Equal(BandType.Footer, band.Type);

        band.Type = BandType.ReportHeader;
        Assert.Equal(BandType.ReportHeader, band.Type);

        band.Type = BandType.ReportFooter;
        Assert.Equal(BandType.ReportFooter, band.Type);

        band.Type = BandType.Detail;
        Assert.Equal(BandType.Detail, band.Type);

        band.Type = BandType.GroupHeader;
        Assert.Equal(BandType.GroupHeader, band.Type);

        band.Type = BandType.GroupFooter;
        Assert.Equal(BandType.GroupFooter, band.Type);
    }

    // ============== Height ==============

    [Fact]
    public void Height_SetAndGet_Works()
    {
        var band = new Band();
        band.Height = 25.5;
        Assert.Equal(25.5, band.Height);
    }

    [Fact]
    public void Height_Zero_IsValid()
    {
        var band = new Band { Height = 0 };
        Assert.Equal(0, band.Height);
    }

    [Fact]
    public void Height_LargeValue_IsValid()
    {
        var band = new Band { Height = 1000 };
        Assert.Equal(1000, band.Height);
    }

    // ============== DataSource ==============

    [Fact]
    public void DataSource_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.DataSource);
    }

    [Fact]
    public void DataSource_SetAndGet_Works()
    {
        var band = new Band { DataSource = "orders" };
        Assert.Equal("orders", band.DataSource);
    }

    [Fact]
    public void DataSource_CanBeCleared()
    {
        var band = new Band { DataSource = "orders" };
        band.DataSource = null;
        Assert.Null(band.DataSource);
    }

    // ============== Group ==============

    [Fact]
    public void Group_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.Group);
    }

    [Fact]
    public void Group_SetAndGet_Works()
    {
        var band = new Band
        {
            Group = new GroupDef { Expression = "category", KeepTogether = true }
        };
        Assert.NotNull(band.Group);
        Assert.Equal("category", band.Group.Expression);
        Assert.True(band.Group.KeepTogether);
    }

    [Fact]
    public void Group_CanBeReplaced()
    {
        var band = new Band
        {
            Group = new GroupDef { Expression = "old" }
        };
        band.Group = new GroupDef { Expression = "new" };
        Assert.Equal("new", band.Group.Expression);
    }

    // ============== MultiColumn ==============

    [Fact]
    public void MultiColumn_NullByDefault()
    {
        var band = new Band();
        Assert.Null(band.MultiColumn);
    }

    [Fact]
    public void MultiColumn_SetAndGet_Works()
    {
        var band = new Band
        {
            MultiColumn = new MultiColumnConfig
            {
                ColumnCount = 3,
                ColumnSpacing = 5,
                Direction = "Vertical"
            }
        };
        Assert.NotNull(band.MultiColumn);
        Assert.Equal(3, band.MultiColumn.ColumnCount);
        Assert.Equal(5, band.MultiColumn.ColumnSpacing);
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
    public void SubBands_SetAndGet_Works()
    {
        var band = new Band
        {
            SubBands = new List<Band>
            {
                new Band { Type = BandType.Header, Height = 10 },
                new Band { Type = BandType.Header, Height = 10 }
            }
        };
        Assert.NotNull(band.SubBands);
        Assert.Equal(2, band.SubBands.Count);
    }

    [Fact]
    public void SubBands_NestedSubBands_Works()
    {
        var band = new Band
        {
            SubBands = new List<Band>
            {
                new Band
                {
                    Type = BandType.Header,
                    Height = 20,
                    SubBands = new List<Band>
                    {
                        new Band { Type = BandType.Header, Height = 10 }
                    }
                }
            }
        };
        Assert.NotNull(band.SubBands[0].SubBands);
        Assert.Single(band.SubBands[0].SubBands!);
    }

    // ============== Elements ==============

    [Fact]
    public void Elements_EmptyByDefault()
    {
        var band = new Band();
        Assert.Empty(band.Elements);
    }

    [Fact]
    public void Elements_AddTextElement_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "Hello", X = 10, Y = 5 });
        Assert.Single(band.Elements);
        Assert.IsType<TextElement>(band.Elements[0]);
    }

    [Fact]
    public void Elements_AddMultipleElements_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "Title" });
        band.Elements.Add(new ImageElement { Source = "logo.png" });
        band.Elements.Add(new LineElement { LineWidth = 2 });
        Assert.Equal(3, band.Elements.Count);
    }

    [Fact]
    public void Elements_AddAllElementTypes_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement());
        band.Elements.Add(new ImageElement());
        band.Elements.Add(new LineElement());
        band.Elements.Add(new ShapeElement());
        band.Elements.Add(new BarcodeElement());
        band.Elements.Add(new TableElement());
        band.Elements.Add(new CrossTabElement());
        band.Elements.Add(new ChartElement());
        band.Elements.Add(new SubReportElement());
        Assert.Equal(9, band.Elements.Count);
    }

    [Fact]
    public void Elements_Remove_Works()
    {
        var band = new Band();
        var element = new TextElement { Text = "Remove me" };
        band.Elements.Add(element);
        Assert.Single(band.Elements);

        band.Elements.Remove(element);
        Assert.Empty(band.Elements);
    }

    [Fact]
    public void Elements_Clear_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement());
        band.Elements.Add(new ImageElement());
        Assert.Equal(2, band.Elements.Count);

        band.Elements.Clear();
        Assert.Empty(band.Elements);
    }

    [Fact]
    public void Elements_InsertAt_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "First" });
        band.Elements.Add(new TextElement { Text = "Third" });
        band.Elements.Insert(1, new TextElement { Text = "Second" });

        Assert.Equal(3, band.Elements.Count);
        Assert.Equal("Second", ((TextElement)band.Elements[1]).Text);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void Band_FullSetup_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            Height = 30,
            DataSource = "orders",
            Group = new GroupDef { Expression = "region" },
            MultiColumn = new MultiColumnConfig { ColumnCount = 2 }
        };

        band.Elements.Add(new TextElement { Text = "{{orderNo}}" });
        band.Elements.Add(new TextElement { Text = "{{amount}}" });

        Assert.Equal(BandType.Detail, band.Type);
        Assert.Equal(30, band.Height);
        Assert.Equal("orders", band.DataSource);
        Assert.NotNull(band.Group);
        Assert.NotNull(band.MultiColumn);
        Assert.Equal(2, band.Elements.Count);
    }

    [Fact]
    public void Band_GroupHeaderWithSubBands_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Height = 25,
            DataSource = "orders",
            Group = new GroupDef { Expression = "category", KeepTogether = true },
            SubBands = new List<Band>
            {
                new Band { Type = BandType.Header, Height = 10 }
            }
        };

        band.Elements.Add(new TextElement { Text = "Category: {{category}}" });

        Assert.Equal(BandType.GroupHeader, band.Type);
        Assert.NotNull(band.Group);
        Assert.NotNull(band.SubBands);
        Assert.Single(band.SubBands);
        Assert.Single(band.Elements);
    }
}
