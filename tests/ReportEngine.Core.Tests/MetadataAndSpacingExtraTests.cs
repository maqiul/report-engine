using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportTemplate Author/Description 属性测试
/// </summary>
public class ReportTemplateMetadata2Tests
{
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
        var t = new ReportTemplate { Author = "老马" };
        Assert.Equal("老马", t.Author);
    }

    [Fact]
    public void Author_SetEnglish_Works()
    {
        var t = new ReportTemplate { Author = "John Doe" };
        Assert.Equal("John Doe", t.Author);
    }

    [Fact]
    public void Author_CanBeCleared()
    {
        var t = new ReportTemplate { Author = "test" };
        t.Author = null;
        Assert.Null(t.Author);
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
        var t = new ReportTemplate { Description = "月度销售报表模板" };
        Assert.Equal("月度销售报表模板", t.Description);
    }

    [Fact]
    public void Description_SetLong_Works()
    {
        var desc = "这是一个复杂的报表模板，包含多个数据源、分组和子报表。";
        var t = new ReportTemplate { Description = desc };
        Assert.Equal(desc, t.Description);
    }

    [Fact]
    public void Description_CanBeCleared()
    {
        var t = new ReportTemplate { Description = "test" };
        t.Description = null;
        Assert.Null(t.Description);
    }

    // ============== 综合 ==============

    [Fact]
    public void ReportTemplate_WithMetadata_Works()
    {
        var t = new ReportTemplate
        {
            Version = "2.0",
            Author = "报表组",
            Description = "客户对账单模板"
        };
        t.Bands.Add(new Band { Type = BandType.Detail });

        Assert.Equal("2.0", t.Version);
        Assert.Equal("报表组", t.Author);
        Assert.Equal("客户对账单模板", t.Description);
    }
}

/// <summary>
/// MultiUpConfig HSpacing/VSpacing/Count 测试
/// </summary>
public class MultiUpConfigExtraTests
{
    // ============== HSpacing ==============

    [Fact]
    public void HSpacing_DefaultIsZero()
    {
        var c = new MultiUpConfig();
        Assert.Equal(0, c.HSpacing);
    }

    [Fact]
    public void HSpacing_Set_Works()
    {
        var c = new MultiUpConfig { HSpacing = 5 };
        Assert.Equal(5, c.HSpacing);
    }

    [Fact]
    public void HSpacing_SetDecimal_Works()
    {
        var c = new MultiUpConfig { HSpacing = 2.5 };
        Assert.Equal(2.5, c.HSpacing);
    }

    // ============== VSpacing ==============

    [Fact]
    public void VSpacing_DefaultIsZero()
    {
        var c = new MultiUpConfig();
        Assert.Equal(0, c.VSpacing);
    }

    [Fact]
    public void VSpacing_Set_Works()
    {
        var c = new MultiUpConfig { VSpacing = 10 };
        Assert.Equal(10, c.VSpacing);
    }

    [Fact]
    public void VSpacing_SetDecimal_Works()
    {
        var c = new MultiUpConfig { VSpacing = 3.5 };
        Assert.Equal(3.5, c.VSpacing);
    }

    // ============== Count (computed) ==============

    [Fact]
    public void Count_DefaultIs4()
    {
        var c = new MultiUpConfig();
        Assert.Equal(4, c.Count); // 2 * 2
    }

    [Fact]
    public void Count_3x3_Is9()
    {
        var c = new MultiUpConfig { Rows = 3, Columns = 3 };
        Assert.Equal(9, c.Count);
    }

    [Fact]
    public void Count_1x4_Is4()
    {
        var c = new MultiUpConfig { Rows = 1, Columns = 4 };
        Assert.Equal(4, c.Count);
    }

    [Fact]
    public void Count_4x1_Is4()
    {
        var c = new MultiUpConfig { Rows = 4, Columns = 1 };
        Assert.Equal(4, c.Count);
    }

    [Fact]
    public void Count_2x3_Is6()
    {
        var c = new MultiUpConfig { Rows = 2, Columns = 3 };
        Assert.Equal(6, c.Count);
    }

    [Fact]
    public void Count_ChangesWithRowsColumns()
    {
        var c = new MultiUpConfig { Rows = 2, Columns = 2 };
        Assert.Equal(4, c.Count);
        c.Rows = 3;
        Assert.Equal(6, c.Count);
        c.Columns = 4;
        Assert.Equal(12, c.Count);
    }

    // ============== 综合 ==============

    [Fact]
    public void MultiUpConfig_FullSetup_Works()
    {
        var c = new MultiUpConfig
        {
            Rows = 3,
            Columns = 2,
            HSpacing = 5,
            VSpacing = 3,
            Direction = "Vertical"
        };

        Assert.Equal(3, c.Rows);
        Assert.Equal(2, c.Columns);
        Assert.Equal(5, c.HSpacing);
        Assert.Equal(3, c.VSpacing);
        Assert.Equal("Vertical", c.Direction);
        Assert.Equal(6, c.Count);
    }
}

/// <summary>
/// RenderContext 额外属性测试（NestingDepth/MaxNestingDepth 边界）
/// </summary>
public class RenderContextExtra2Tests
{
    [Fact]
    public void NestingDepth_Increment_Works()
    {
        var ctx = new RenderContext();
        ctx.NestingDepth++;
        Assert.Equal(1, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_AtMax_Works()
    {
        var ctx = new RenderContext { NestingDepth = RenderContext.MaxNestingDepth };
        Assert.Equal(5, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_ExceedMax_StillWorks()
    {
        var ctx = new RenderContext { NestingDepth = 10 };
        Assert.Equal(10, ctx.NestingDepth);
    }

    [Fact]
    public void DataSources_ContainsKey_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSources["ds1"] = new List<Dictionary<string, object>>();
        Assert.True(ctx.DataSources.ContainsKey("ds1"));
        Assert.False(ctx.DataSources.ContainsKey("ds2"));
    }

    [Fact]
    public void DataSources_Remove_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSources["ds1"] = new List<Dictionary<string, object>>();
        ctx.DataSources.Remove("ds1");
        Assert.False(ctx.DataSources.ContainsKey("ds1"));
    }

    [Fact]
    public void CurrentRow_MultipleFields_Works()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object>
        {
            ["id"] = 1,
            ["name"] = "test",
            ["amount"] = 99.9m,
            ["active"] = true
        };

        Assert.Equal(1, ctx.CurrentRow["id"]);
        Assert.Equal("test", ctx.CurrentRow["name"]);
        Assert.Equal(99.9m, ctx.CurrentRow["amount"]);
        Assert.Equal(true, ctx.CurrentRow["active"]);
    }

    [Fact]
    public void CurrentRow_Overwrite_Works()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { ["x"] = 1 };
        ctx.CurrentRow = new Dictionary<string, object> { ["y"] = 2 };

        Assert.False(ctx.CurrentRow.ContainsKey("x"));
        Assert.True(ctx.CurrentRow.ContainsKey("y"));
    }

    [Fact]
    public void PageWidth_SetDecimal_Works()
    {
        var ctx = new RenderContext { PageWidth = 210.5 };
        Assert.Equal(210.5, ctx.PageWidth);
    }

    [Fact]
    public void PageHeight_SetDecimal_Works()
    {
        var ctx = new RenderContext { PageHeight = 297.3 };
        Assert.Equal(297.3, ctx.PageHeight);
    }
}

/// <summary>
/// TemplateParser Serialize 格式测试
/// </summary>
public class TemplateParserSerializeTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void Serialize_ReturnsValidJson()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail });
        var json = _parser.Serialize(t);
        Assert.NotEmpty(json);
        Assert.Contains("{", json);
    }

    [Fact]
    public void Serialize_ContainsVersion()
    {
        var t = new ReportTemplate { Version = "3.0" };
        t.Bands.Add(new Band { Type = BandType.Detail });
        var json = _parser.Serialize(t);
        Assert.Contains("3.0", json);
    }

    [Fact]
    public void Serialize_ContainsBandType()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.ReportHeader });
        var json = _parser.Serialize(t);
        Assert.Contains("reportHeader", json);
    }

    [Fact]
    public void Serialize_TextElement_ContainsType()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new TextElement { Text = "Hello" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        Assert.Contains("\"type\": \"text\"", json);
    }

    [Fact]
    public void Serialize_ImageElement_ContainsType()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new ImageElement { Source = "logo.png" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        Assert.Contains("\"type\": \"image\"", json);
    }

    [Fact]
    public void Serialize_NullFields_Omitted()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail });
        var json = _parser.Serialize(t);
        // NullValueHandling.Ignore 应该省略 null 字段
        Assert.DoesNotContain("\"author\"", json);
    }

    [Fact]
    public void Serialize_Author_Included()
    {
        var t = new ReportTemplate { Author = "TestUser" };
        t.Bands.Add(new Band { Type = BandType.Detail });
        var json = _parser.Serialize(t);
        Assert.Contains("TestUser", json);
    }

    [Fact]
    public void Serialize_RoundTrip_PreservesAllBandTypes()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Header });
        t.Bands.Add(new Band { Type = BandType.Footer });
        t.Bands.Add(new Band { Type = BandType.ReportHeader });
        t.Bands.Add(new Band { Type = BandType.ReportFooter });
        t.Bands.Add(new Band { Type = BandType.Detail });
        t.Bands.Add(new Band { Type = BandType.GroupHeader });
        t.Bands.Add(new Band { Type = BandType.GroupFooter });

        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);

        Assert.Equal(7, parsed.Bands.Count);
        Assert.Equal(BandType.Header, parsed.Bands[0].Type);
        Assert.Equal(BandType.GroupFooter, parsed.Bands[6].Type);
    }

    [Fact]
    public void Serialize_EnumValues_CamelCase()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new TextElement { Alignment = TextAlignment.Center });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        Assert.Contains("center", json);
    }
}
