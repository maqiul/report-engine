using System;
using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportTemplate 行为测试：
///   - 默认值
///   - DataSources 操作
///   - Bands 操作
///   - Parameters 操作
///   - Page 设置
///   - 元数据
/// </summary>
public class ReportTemplateBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var template = new ReportTemplate();

        Assert.Equal("1.0", template.Version);
        Assert.NotNull(template.Page);
        Assert.NotNull(template.DataSources);
        Assert.Empty(template.DataSources);
        Assert.NotNull(template.Bands);
        Assert.Empty(template.Bands);
        Assert.NotNull(template.Parameters);
        Assert.Empty(template.Parameters);
        Assert.Null(template.Author);
        Assert.Null(template.Description);
    }

    // ============== Version ==============

    [Fact]
    public void Version_DefaultIs1_0()
    {
        var template = new ReportTemplate();
        Assert.Equal("1.0", template.Version);
    }

    [Fact]
    public void Version_SetAndGet_Works()
    {
        var template = new ReportTemplate { Version = "2.0" };
        Assert.Equal("2.0", template.Version);
    }

    // ============== Page ==============

    [Fact]
    public void Page_NotNull_ByDefault()
    {
        var template = new ReportTemplate();
        Assert.NotNull(template.Page);
    }

    [Fact]
    public void Page_SetAndGet_Works()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 150, Height = 200 };
        Assert.Equal(150, template.Page.Width);
        Assert.Equal(200, template.Page.Height);
    }

    [Fact]
    public void Page_CanBeReplaced()
    {
        var template = new ReportTemplate();
        var newPage = new PageInfo { Width = 100, Height = 150, Orientation = "landscape" };
        template.Page = newPage;
        Assert.Same(newPage, template.Page);
    }

    // ============== DataSources ==============

    [Fact]
    public void DataSources_EmptyByDefault()
    {
        var template = new ReportTemplate();
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
        template.DataSources.Add(new DataSourceDef { Name = "products" });
        template.DataSources.Add(new DataSourceDef { Name = "customers" });
        Assert.Equal(3, template.DataSources.Count);
    }

    [Fact]
    public void DataSources_Remove_Works()
    {
        var template = new ReportTemplate();
        var ds = new DataSourceDef { Name = "orders" };
        template.DataSources.Add(ds);
        template.DataSources.Remove(ds);
        Assert.Empty(template.DataSources);
    }

    [Fact]
    public void DataSources_Clear_Works()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "a" });
        template.DataSources.Add(new DataSourceDef { Name = "b" });
        template.DataSources.Clear();
        Assert.Empty(template.DataSources);
    }

    // ============== Bands ==============

    [Fact]
    public void Bands_EmptyByDefault()
    {
        var template = new ReportTemplate();
        Assert.Empty(template.Bands);
    }

    [Fact]
    public void Bands_Add_Works()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
        Assert.Single(template.Bands);
    }

    [Fact]
    public void Bands_AddMultiple_Works()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 25 });
        template.Bands.Add(new Band { Type = BandType.Header, Height = 15 });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 30 });
        template.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });
        template.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 25 });
        Assert.Equal(5, template.Bands.Count);
    }

    [Fact]
    public void Bands_InsertAt_Works()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
        template.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });
        template.Bands.Insert(1, new Band { Type = BandType.Detail, Height = 30 });

        Assert.Equal(3, template.Bands.Count);
        Assert.Equal(BandType.Detail, template.Bands[1].Type);
    }

    [Fact]
    public void Bands_Remove_Works()
    {
        var template = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 20 };
        template.Bands.Add(band);
        template.Bands.Remove(band);
        Assert.Empty(template.Bands);
    }

    [Fact]
    public void Bands_Clear_Works()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header });
        template.Bands.Add(new Band { Type = BandType.Detail });
        template.Bands.Clear();
        Assert.Empty(template.Bands);
    }

    // ============== Parameters ==============

    [Fact]
    public void Parameters_EmptyByDefault()
    {
        var template = new ReportTemplate();
        Assert.Empty(template.Parameters);
    }

    [Fact]
    public void Parameters_Add_Works()
    {
        var template = new ReportTemplate();
        template.Parameters.Add(new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2024-01-01"
        });
        Assert.Single(template.Parameters);
    }

    [Fact]
    public void Parameters_AddMultiple_Works()
    {
        var template = new ReportTemplate();
        template.Parameters.Add(new TemplateParam { Name = "p1", Type = "string" });
        template.Parameters.Add(new TemplateParam { Name = "p2", Type = "number" });
        template.Parameters.Add(new TemplateParam { Name = "p3", Type = "date" });
        Assert.Equal(3, template.Parameters.Count);
    }

    [Fact]
    public void Parameters_Remove_Works()
    {
        var template = new ReportTemplate();
        var param = new TemplateParam { Name = "test" };
        template.Parameters.Add(param);
        template.Parameters.Remove(param);
        Assert.Empty(template.Parameters);
    }

    // ============== Author / Description ==============

    [Fact]
    public void Author_NullByDefault()
    {
        var template = new ReportTemplate();
        Assert.Null(template.Author);
    }

    [Fact]
    public void Author_SetAndGet_Works()
    {
        var template = new ReportTemplate { Author = "John Doe" };
        Assert.Equal("John Doe", template.Author);
    }

    [Fact]
    public void Description_NullByDefault()
    {
        var template = new ReportTemplate();
        Assert.Null(template.Description);
    }

    [Fact]
    public void Description_SetAndGet_Works()
    {
        var template = new ReportTemplate { Description = "Monthly sales report" };
        Assert.Equal("Monthly sales report", template.Description);
    }

    // ============== CreatedAt / ModifiedAt ==============

    [Fact]
    public void CreatedAt_IsRecent()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var template = new ReportTemplate();
        var after = DateTime.Now.AddSeconds(1);

        Assert.InRange(template.CreatedAt, before, after);
    }

    [Fact]
    public void ModifiedAt_IsRecent()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var template = new ReportTemplate();
        var after = DateTime.Now.AddSeconds(1);

        Assert.InRange(template.ModifiedAt, before, after);
    }

    [Fact]
    public void ModifiedAt_CanBeUpdated()
    {
        var template = new ReportTemplate();
        var original = template.ModifiedAt;

        System.Threading.Thread.Sleep(10);
        template.ModifiedAt = DateTime.Now;

        Assert.True(template.ModifiedAt >= original);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ReportTemplate_FullSetup_Works()
    {
        var template = new ReportTemplate
        {
            Version = "1.0",
            Author = "Test Author",
            Description = "Test Report"
        };

        template.Page = new PageInfo { Width = 210, Height = 297 };
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "orders" });
        template.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });
        template.Parameters.Add(new TemplateParam { Name = "title", Type = "string", DefaultValue = "Report" });

        Assert.Equal("1.0", template.Version);
        Assert.Equal("Test Author", template.Author);
        Assert.Equal("Test Report", template.Description);
        Assert.Equal(210, template.Page.Width);
        Assert.Single(template.DataSources);
        Assert.Equal(3, template.Bands.Count);
        Assert.Single(template.Parameters);
    }

    [Fact]
    public void ReportTemplate_ComplexTemplate_Works()
    {
        var template = new ReportTemplate();

        // 多数据源
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        template.DataSources.Add(new DataSourceDef { Name = "products" });

        // 多 Band 类型
        template.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 30 });
        template.Bands.Add(new Band { Type = BandType.Header, Height = 15 });
        template.Bands.Add(new Band
        {
            Type = BandType.GroupHeader,
            Height = 20,
            DataSource = "orders",
            Group = new GroupDef { Expression = "category" }
        });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 25, DataSource = "orders" });
        template.Bands.Add(new Band { Type = BandType.GroupFooter, Height = 15, DataSource = "orders" });
        template.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });
        template.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 30 });

        // 参数
        template.Parameters.Add(new TemplateParam { Name = "startDate", Type = "date" });
        template.Parameters.Add(new TemplateParam { Name = "endDate", Type = "date" });
        template.Parameters.Add(new TemplateParam { Name = "showDetails", Type = "boolean" });

        Assert.Equal(2, template.DataSources.Count);
        Assert.Equal(7, template.Bands.Count);
        Assert.Equal(3, template.Parameters.Count);
    }
}
