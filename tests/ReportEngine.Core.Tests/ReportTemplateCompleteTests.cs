using System;
using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportTemplate 完整字段测试：
///   - ReportTemplate 完整字段（Version/Page/DataSources/Bands/Author/Description/CreatedAt/ModifiedAt/Parameters）
///   - 字段组合行为
/// </summary>
public class ReportTemplateCompleteTests
{
    [Fact]
    public void ReportTemplate_Defaults()
    {
        var t = new ReportTemplate();
        Assert.Equal("1.0", t.Version);
        Assert.NotNull(t.Page);
        Assert.NotNull(t.DataSources);
        Assert.Empty(t.DataSources);
        Assert.NotNull(t.Bands);
        Assert.Empty(t.Bands);
        Assert.Null(t.Author);
        Assert.Null(t.Description);
        Assert.NotNull(t.Parameters);
        Assert.Empty(t.Parameters);
        // CreatedAt/ModifiedAt 是 DateTime.Now，接近当前时间
        Assert.True((DateTime.Now - t.CreatedAt).TotalSeconds < 1);
        Assert.True((DateTime.Now - t.ModifiedAt).TotalSeconds < 1);
    }

    [Fact]
    public void ReportTemplate_AllSetters()
    {
        var now = DateTime.Now;
        var t = new ReportTemplate
        {
            Version = "2.0",
            Page = new PageInfo { Width = 100, Height = 150 },
            Author = "张三",
            Description = "测试模板",
            CreatedAt = now.AddDays(-1),
            ModifiedAt = now,
        };
        t.DataSources.Add(new DataSourceDef { Name = "ds1" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20 });
        t.Parameters.Add(new TemplateParam { Name = "p1", Type = "string" });

        Assert.Equal("2.0", t.Version);
        Assert.Equal(100, t.Page.Width);
        Assert.Equal("张三", t.Author);
        Assert.Equal("测试模板", t.Description);
        Assert.Equal(now.AddDays(-1), t.CreatedAt);
        Assert.Equal(now, t.ModifiedAt);
        Assert.Single(t.DataSources);
        Assert.Single(t.Bands);
        Assert.Single(t.Parameters);
    }

    [Fact]
    public void ReportTemplate_Version_CanBeEmpty()
    {
        var t = new ReportTemplate { Version = "" };
        Assert.Equal("", t.Version);
    }

    [Fact]
    public void ReportTemplate_Version_CanBeAnyString()
    {
        var t = new ReportTemplate { Version = "3.5.2-beta" };
        Assert.Equal("3.5.2-beta", t.Version);
    }

    [Fact]
    public void ReportTemplate_Page_CanBeReplaced()
    {
        var t = new ReportTemplate();
        t.Page = new PageInfo { Width = 50, Height = 75 };
        Assert.Equal(50, t.Page.Width);
        Assert.Equal(75, t.Page.Height);
    }

    [Fact]
    public void ReportTemplate_Author_CanBeEmpty()
    {
        var t = new ReportTemplate { Author = "" };
        Assert.Equal("", t.Author);
    }

    [Fact]
    public void ReportTemplate_Author_CanBeLongText()
    {
        var t = new ReportTemplate { Author = "这是一个很长的作者名字用于测试" };
        Assert.Contains("作者", t.Author);
    }

    [Fact]
    public void ReportTemplate_Description_CanBeEmpty()
    {
        var t = new ReportTemplate { Description = "" };
        Assert.Equal("", t.Description);
    }

    [Fact]
    public void ReportTemplate_Description_CanBeLongText()
    {
        var t = new ReportTemplate { Description = "这是一个很长的描述用于测试模板的功能和用途" };
        Assert.Contains("描述", t.Description);
    }

    [Fact]
    public void ReportTemplate_CreatedAt_CanBePast()
    {
        var past = new DateTime(2020, 1, 1);
        var t = new ReportTemplate { CreatedAt = past };
        Assert.Equal(past, t.CreatedAt);
    }

    [Fact]
    public void ReportTemplate_ModifiedAt_CanBeFuture()
    {
        var future = DateTime.Now.AddDays(1);
        var t = new ReportTemplate { ModifiedAt = future };
        Assert.Equal(future, t.ModifiedAt);
    }

    [Fact]
    public void ReportTemplate_DataSources_CanAddMultiple()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "ds1" });
        t.DataSources.Add(new DataSourceDef { Name = "ds2" });
        t.DataSources.Add(new DataSourceDef { Name = "ds3" });
        Assert.Equal(3, t.DataSources.Count);
    }

    [Fact]
    public void ReportTemplate_Bands_CanAddMultiple()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Header, Height = 10 });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20 });
        t.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });
        Assert.Equal(3, t.Bands.Count);
    }

    [Fact]
    public void ReportTemplate_Parameters_CanAddMultiple()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "p1", Type = "string" });
        t.Parameters.Add(new TemplateParam { Name = "p2", Type = "number" });
        Assert.Equal(2, t.Parameters.Count);
    }

    [Fact]
    public void ReportTemplate_AllLists_CanBeCleared()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef());
        t.Bands.Add(new Band());
        t.Parameters.Add(new TemplateParam());

        t.DataSources.Clear();
        t.Bands.Clear();
        t.Parameters.Clear();

        Assert.Empty(t.DataSources);
        Assert.Empty(t.Bands);
        Assert.Empty(t.Parameters);
    }
}
