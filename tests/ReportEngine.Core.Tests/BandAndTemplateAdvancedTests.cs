using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Band 高级属性测试
/// </summary>
public class BandAdvancedPropertyTests
{
    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsHeader()
    {
        var band = new Band();
        Assert.Equal(BandType.Header, band.Type);
    }

    [Fact]
    public void Type_SetHeader_Works()
    {
        var band = new Band { Type = BandType.Header };
        Assert.Equal(BandType.Header, band.Type);
    }

    [Fact]
    public void Type_SetFooter_Works()
    {
        var band = new Band { Type = BandType.Footer };
        Assert.Equal(BandType.Footer, band.Type);
    }

    [Fact]
    public void Type_SetReportHeader_Works()
    {
        var band = new Band { Type = BandType.ReportHeader };
        Assert.Equal(BandType.ReportHeader, band.Type);
    }

    [Fact]
    public void Type_SetReportFooter_Works()
    {
        var band = new Band { Type = BandType.ReportFooter };
        Assert.Equal(BandType.ReportFooter, band.Type);
    }

    [Fact]
    public void Type_SetGroupHeader_Works()
    {
        var band = new Band { Type = BandType.GroupHeader };
        Assert.Equal(BandType.GroupHeader, band.Type);
    }

    [Fact]
    public void Type_SetGroupFooter_Works()
    {
        var band = new Band { Type = BandType.GroupFooter };
        Assert.Equal(BandType.GroupFooter, band.Type);
    }

    [Fact]
    public void Type_CanBeChanged()
    {
        var band = new Band { Type = BandType.Detail };
        band.Type = BandType.Header;
        Assert.Equal(BandType.Header, band.Type);
    }

    // ============== Height ==============

    [Fact]
    public void Height_DefaultIsZero()
    {
        var band = new Band();
        Assert.Equal(0, band.Height);
    }

    [Fact]
    public void Height_Set_Works()
    {
        var band = new Band { Height = 20 };
        Assert.Equal(20, band.Height);
    }

    [Fact]
    public void Height_SetSmall_Works()
    {
        var band = new Band { Height = 5.5 };
        Assert.Equal(5.5, band.Height);
    }

    [Fact]
    public void Height_SetLarge_Works()
    {
        var band = new Band { Height = 100 };
        Assert.Equal(100, band.Height);
    }

    [Fact]
    public void Height_CanBeChanged()
    {
        var band = new Band { Height = 10 };
        band.Height = 30;
        Assert.Equal(30, band.Height);
    }

    // ============== RepeatOnNewPage ==============

    [Fact]
    public void RepeatOnNewPage_FalseByDefault()
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

    [Fact]
    public void RepeatOnNewPage_CanBeToggled()
    {
        var band = new Band { RepeatOnNewPage = true };
        band.RepeatOnNewPage = false;
        Assert.False(band.RepeatOnNewPage);
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

    [Fact]
    public void DataSource_CanBeCleared()
    {
        var band = new Band { DataSource = "ds1" };
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
    public void Group_Set_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Group = new GroupDef { Expression = "[Category]" }
        };
        Assert.NotNull(band.Group);
        Assert.Equal("[Category]", band.Group.Expression);
    }

    [Fact]
    public void Group_CanBeCleared()
    {
        var band = new Band { Group = new GroupDef { Expression = "[A]" } };
        band.Group = null;
        Assert.Null(band.Group);
    }

    // ============== Elements ==============

    [Fact]
    public void Elements_EmptyByDefault()
    {
        var band = new Band();
        Assert.NotNull(band.Elements);
        Assert.Empty(band.Elements);
    }

    [Fact]
    public void Elements_Add_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "Hello" });
        Assert.Single(band.Elements);
    }

    [Fact]
    public void Elements_AddMultiple_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "A" });
        band.Elements.Add(new TextElement { Text = "B" });
        band.Elements.Add(new TextElement { Text = "C" });
        Assert.Equal(3, band.Elements.Count);
    }

    [Fact]
    public void Elements_AddMixedTypes_Works()
    {
        var band = new Band();
        band.Elements.Add(new TextElement { Text = "Title" });
        band.Elements.Add(new ImageElement { Source = "logo.png" });
        band.Elements.Add(new LineElement());
        Assert.Equal(3, band.Elements.Count);
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
        var band = new Band { MultiColumn = new MultiColumnConfig() };
        Assert.NotNull(band.MultiColumn);
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
            SubBands = new List<Band>
            {
                new Band { Type = BandType.Header, Height = 15 },
                new Band { Type = BandType.Header, Height = 15 }
            }
        };
        Assert.Equal(2, band.SubBands.Count);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void Band_HeaderBand_Works()
    {
        var band = new Band
        {
            Type = BandType.Header,
            Height = 30,
            RepeatOnNewPage = true
        };
        band.Elements.Add(new TextElement { Text = "Report Header" });

        Assert.Equal(BandType.Header, band.Type);
        Assert.Equal(30, band.Height);
        Assert.True(band.RepeatOnNewPage);
        Assert.Single(band.Elements);
    }

    [Fact]
    public void Band_DetailBand_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            Height = 20,
            DataSource = "orders"
        };
        band.Elements.Add(new TextElement { DataField = "Name" });
        band.Elements.Add(new TextElement { DataField = "Amount", Format = "currency" });

        Assert.Equal(BandType.Detail, band.Type);
        Assert.Equal("orders", band.DataSource);
        Assert.Equal(2, band.Elements.Count);
    }

    [Fact]
    public void Band_GroupHeaderWithGroup_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Height = 25,
            Group = new GroupDef
            {
                Expression = "[Category]",
                KeepTogether = true
            }
        };

        Assert.Equal(BandType.GroupHeader, band.Type);
        Assert.NotNull(band.Group);
        Assert.Equal("[Category]", band.Group.Expression);
        Assert.True(band.Group.KeepTogether);
    }

    [Fact]
    public void Band_ReportFooter_Works()
    {
        var band = new Band
        {
            Type = BandType.ReportFooter,
            Height = 40
        };
        band.Elements.Add(new TextElement { Text = "Grand Total" });
        band.Elements.Add(new TextElement { SummaryFunction = "Sum", SummaryField = "Amount" });

        Assert.Equal(BandType.ReportFooter, band.Type);
        Assert.Equal(2, band.Elements.Count);
    }

    [Fact]
    public void Band_FullSetup_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Height = 30,
            RepeatOnNewPage = true,
            DataSource = "mainData",
            Group = new GroupDef
            {
                Expression = "[Department]",
                KeepTogether = true
            }
        };
        band.Elements.Add(new TextElement { Text = "Department: {{currentRow.Department}}" });
        band.Elements.Add(new LineElement { Y = 28 });

        Assert.Equal(BandType.GroupHeader, band.Type);
        Assert.Equal(30, band.Height);
        Assert.True(band.RepeatOnNewPage);
        Assert.Equal("mainData", band.DataSource);
        Assert.NotNull(band.Group);
        Assert.Equal(2, band.Elements.Count);
    }
}

/// <summary>
/// ReportTemplate 高级属性测试
/// </summary>
public class ReportTemplateAdvancedPropertyTests
{
    // ============== Version ==============

    [Fact]
    public void Version_DefaultIs10()
    {
        var template = new ReportTemplate();
        Assert.Equal("1.0", template.Version);
    }

    [Fact]
    public void Version_Set_Works()
    {
        var template = new ReportTemplate { Version = "2.0" };
        Assert.Equal("2.0", template.Version);
    }

    [Fact]
    public void Version_CanBeChanged()
    {
        var template = new ReportTemplate { Version = "1.0" };
        template.Version = "3.0";
        Assert.Equal("3.0", template.Version);
    }

    // ============== CreatedAt ==============

    [Fact]
    public void CreatedAt_NotDefault()
    {
        var template = new ReportTemplate();
        Assert.NotEqual(default, template.CreatedAt);
    }

    [Fact]
    public void CreatedAt_Set_Works()
    {
        var dt = new DateTime(2024, 1, 1);
        var template = new ReportTemplate { CreatedAt = dt };
        Assert.Equal(dt, template.CreatedAt);
    }

    // ============== ModifiedAt ==============

    [Fact]
    public void ModifiedAt_NotDefault()
    {
        var template = new ReportTemplate();
        Assert.NotEqual(default, template.ModifiedAt);
    }

    [Fact]
    public void ModifiedAt_Set_Works()
    {
        var dt = new DateTime(2024, 6, 15);
        var template = new ReportTemplate { ModifiedAt = dt };
        Assert.Equal(dt, template.ModifiedAt);
    }

    // ============== Parameters ==============

    [Fact]
    public void Parameters_EmptyByDefault()
    {
        var template = new ReportTemplate();
        Assert.NotNull(template.Parameters);
        Assert.Empty(template.Parameters);
    }

    [Fact]
    public void Parameters_Add_Works()
    {
        var template = new ReportTemplate();
        template.Parameters.Add(new TemplateParam { Name = "title", Type = "string" });
        Assert.Single(template.Parameters);
    }

    [Fact]
    public void Parameters_AddMultiple_Works()
    {
        var template = new ReportTemplate();
        template.Parameters.Add(new TemplateParam { Name = "title", Type = "string" });
        template.Parameters.Add(new TemplateParam { Name = "startDate", Type = "date" });
        template.Parameters.Add(new TemplateParam { Name = "maxRows", Type = "number" });
        Assert.Equal(3, template.Parameters.Count);
    }

    // ============== Page ==============

    [Fact]
    public void Page_NotNull()
    {
        var template = new ReportTemplate();
        Assert.NotNull(template.Page);
    }

    [Fact]
    public void Page_WidthDefaultIs210()
    {
        var template = new ReportTemplate();
        Assert.Equal(210, template.Page.Width);
    }

    [Fact]
    public void Page_HeightDefaultIs297()
    {
        var template = new ReportTemplate();
        Assert.Equal(297, template.Page.Height);
    }

    [Fact]
    public void Page_SetCustom_Works()
    {
        var template = new ReportTemplate
        {
            Page = new PageInfo
            {
                Width = 297,
                Height = 210,
                Orientation = "landscape"
            }
        };

        Assert.Equal(297, template.Page.Width);
        Assert.Equal(210, template.Page.Height);
        Assert.Equal("landscape", template.Page.Orientation);
    }

    // ============== DataSources ==============

    [Fact]
    public void DataSources_EmptyByDefault()
    {
        var template = new ReportTemplate();
        Assert.NotNull(template.DataSources);
        Assert.Empty(template.DataSources);
    }

    [Fact]
    public void DataSources_Add_Works()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        Assert.Single(template.DataSources);
    }

    [Fact]
    public void DataSources_AddMultiple_Works()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        template.DataSources.Add(new DataSourceDef { Name = "customers" });
        Assert.Equal(2, template.DataSources.Count);
    }

    // ============== Bands ==============

    [Fact]
    public void Bands_EmptyByDefault()
    {
        var template = new ReportTemplate();
        Assert.NotNull(template.Bands);
        Assert.Empty(template.Bands);
    }

    [Fact]
    public void Bands_Add_Works()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header, Height = 30 });
        Assert.Single(template.Bands);
    }

    [Fact]
    public void Bands_AddMultiple_Works()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header });
        template.Bands.Add(new Band { Type = BandType.Detail });
        template.Bands.Add(new Band { Type = BandType.Footer });
        Assert.Equal(3, template.Bands.Count);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ReportTemplate_MinimalSetup_Works()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "data" });
        template.Bands.Add(new Band { Type = BandType.Detail });

        Assert.Equal("1.0", template.Version);
        Assert.Single(template.DataSources);
        Assert.Single(template.Bands);
    }

    [Fact]
    public void ReportTemplate_WithParameters_Works()
    {
        var template = new ReportTemplate();
        template.Parameters.Add(new TemplateParam { Name = "title", Type = "string", DefaultValue = "Report" });
        template.Parameters.Add(new TemplateParam { Name = "date", Type = "date" });

        Assert.Equal(2, template.Parameters.Count);
        Assert.Equal("title", template.Parameters[0].Name);
    }

    [Fact]
    public void ReportTemplate_LandscapePage_Works()
    {
        var template = new ReportTemplate
        {
            Page = new PageInfo
            {
                Width = 297,
                Height = 210,
                Orientation = "landscape"
            }
        };

        Assert.Equal(297, template.Page.Width);
        Assert.Equal(210, template.Page.Height);
    }

    [Fact]
    public void ReportTemplate_FullSetup_Works()
    {
        var template = new ReportTemplate
        {
            Version = "2.0",
            Page = new PageInfo
            {
                Width = 210,
                Height = 297,
                Orientation = "portrait"
            }
        };

        template.Parameters.Add(new TemplateParam { Name = "title", Type = "string" });
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        template.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 30 });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 20 });
        template.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 25 });

        Assert.Equal("2.0", template.Version);
        Assert.Single(template.Parameters);
        Assert.Single(template.DataSources);
        Assert.Equal(3, template.Bands.Count);
    }
}
