using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportTemplate 元数据字段测试
/// </summary>
public class ReportTemplateMetadataTests
{
    // ============== Version ==============

    [Fact]
    public void Version_DefaultIs10()
    {
        var t = new ReportTemplate();
        Assert.Equal("1.0", t.Version);
    }

    [Fact]
    public void Version_Set_Works()
    {
        var t = new ReportTemplate { Version = "2.0" };
        Assert.Equal("2.0", t.Version);
    }

    // ============== Author ==============

    [Fact]
    public void Author_NullByDefault()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Author);
    }

    [Fact]
    public void Author_Set_Works()
    {
        var t = new ReportTemplate { Author = "John Doe" };
        Assert.Equal("John Doe", t.Author);
    }

    // ============== Description ==============

    [Fact]
    public void Description_NullByDefault()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Description);
    }

    [Fact]
    public void Description_Set_Works()
    {
        var t = new ReportTemplate { Description = "Monthly sales report" };
        Assert.Equal("Monthly sales report", t.Description);
    }

    // ============== CreatedAt ==============

    [Fact]
    public void CreatedAt_DefaultIsRecent()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var t = new ReportTemplate();
        var after = DateTime.Now.AddSeconds(1);
        
        Assert.InRange(t.CreatedAt, before, after);
    }

    [Fact]
    public void CreatedAt_Set_Works()
    {
        var date = new DateTime(2024, 1, 15, 10, 30, 0);
        var t = new ReportTemplate { CreatedAt = date };
        Assert.Equal(date, t.CreatedAt);
    }

    // ============== ModifiedAt ==============

    [Fact]
    public void ModifiedAt_DefaultIsRecent()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var t = new ReportTemplate();
        var after = DateTime.Now.AddSeconds(1);
        
        Assert.InRange(t.ModifiedAt, before, after);
    }

    [Fact]
    public void ModifiedAt_Set_Works()
    {
        var date = new DateTime(2024, 6, 20, 14, 0, 0);
        var t = new ReportTemplate { ModifiedAt = date };
        Assert.Equal(date, t.ModifiedAt);
    }

    // ============== Parameters ==============

    [Fact]
    public void Parameters_EmptyByDefault()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.Parameters);
        Assert.Empty(t.Parameters);
    }

    [Fact]
    public void Parameters_Add_Works()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "title", DefaultValue = "Report" });
        Assert.Single(t.Parameters);
    }

    [Fact]
    public void Parameters_AddMultiple_Works()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "title", DefaultValue = "Report" });
        t.Parameters.Add(new TemplateParam { Name = "author", DefaultValue = "Admin" });
        t.Parameters.Add(new TemplateParam { Name = "date", DefaultValue = "2024-01-01" });
        Assert.Equal(3, t.Parameters.Count);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ReportTemplate_FullMetadata_Works()
    {
        var t = new ReportTemplate
        {
            Version = "1.5",
            Author = "Report Team",
            Description = "Annual financial summary"
        };
        t.Parameters.Add(new TemplateParam { Name = "year", DefaultValue = "2024" });
        t.Parameters.Add(new TemplateParam { Name = "company", DefaultValue = "Acme Corp" });

        Assert.Equal("1.5", t.Version);
        Assert.Equal("Report Team", t.Author);
        Assert.Equal("Annual financial summary", t.Description);
        Assert.Equal(2, t.Parameters.Count);
    }

    [Fact]
    public void ReportTemplate_MetadataIndependentOfContent()
    {
        var t = new ReportTemplate { Author = "Test" };
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 50 });
        t.DataSources.Add(new DataSourceDef { Name = "ds" });

        Assert.Equal("Test", t.Author);
        Assert.Single(t.Bands);
        Assert.Single(t.DataSources);
    }
}
