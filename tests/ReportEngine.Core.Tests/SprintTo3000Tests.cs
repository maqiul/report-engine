using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 冲刺 3000 测试大关！补充遗漏的 Schema 属性
/// </summary>
public class SprintTo3000Tests
{
    // ============== ReportTemplate.CreatedAt ==============

    [Fact]
    public void CreatedAt_NearNow()
    {
        var before = DateTime.Now.AddSeconds(-2);
        var t = new ReportTemplate();
        var after = DateTime.Now.AddSeconds(2);
        Assert.True(t.CreatedAt >= before && t.CreatedAt <= after);
    }

    [Fact]
    public void CreatedAt_Set_Works()
    {
        var dt = new DateTime(2025, 1, 1);
        var t = new ReportTemplate { CreatedAt = dt };
        Assert.Equal(dt, t.CreatedAt);
    }

    // ============== ReportTemplate.ModifiedAt ==============

    [Fact]
    public void ModifiedAt_NearNow()
    {
        var before = DateTime.Now.AddSeconds(-2);
        var t = new ReportTemplate();
        var after = DateTime.Now.AddSeconds(2);
        Assert.True(t.ModifiedAt >= before && t.ModifiedAt <= after);
    }

    [Fact]
    public void ModifiedAt_Set_Works()
    {
        var dt = new DateTime(2025, 6, 15);
        var t = new ReportTemplate { ModifiedAt = dt };
        Assert.Equal(dt, t.ModifiedAt);
    }

    // ============== ReportTemplate.Parameters ==============

    [Fact]
    public void Parameters_EmptyByDefault()
    {
        var t = new ReportTemplate();
        Assert.Empty(t.Parameters);
    }

    [Fact]
    public void Parameters_AddParam_Works()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "StartDate", Type = "date" });
        Assert.Single(t.Parameters);
    }

    [Fact]
    public void Parameters_AddMultiple_Works()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "p1" });
        t.Parameters.Add(new TemplateParam { Name = "p2" });
        t.Parameters.Add(new TemplateParam { Name = "p3" });
        Assert.Equal(3, t.Parameters.Count);
    }

    // ============== TemplateParam.Label ==============

    [Fact]
    public void TemplateParam_Label_NullByDefault()
    {
        var p = new TemplateParam();
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_Label_Set_Works()
    {
        var p = new TemplateParam { Label = "开始日期" };
        Assert.Equal("开始日期", p.Label);
    }

    // ============== TemplateParam.Type ==============

    [Fact]
    public void TemplateParam_Type_DefaultIsString()
    {
        var p = new TemplateParam();
        Assert.Equal("string", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_Number_Works()
    {
        var p = new TemplateParam { Type = "number" };
        Assert.Equal("number", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_Date_Works()
    {
        var p = new TemplateParam { Type = "date" };
        Assert.Equal("date", p.Type);
    }

    // ============== PageInfo.BackgroundColor ==============

    [Fact]
    public void PageBackgroundColor_NullByDefault()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundColor);
    }

    [Fact]
    public void PageBackgroundColor_Set_Works()
    {
        var p = new PageInfo { BackgroundColor = "#F0F0F0" };
        Assert.Equal("#F0F0F0", p.BackgroundColor);
    }

    // ============== PageInfo.BackgroundImage ==============

    [Fact]
    public void PageBackgroundImage_NullByDefault()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundImage);
    }

    [Fact]
    public void PageBackgroundImage_Set_Works()
    {
        var p = new PageInfo { BackgroundImage = "bg.png" };
        Assert.Equal("bg.png", p.BackgroundImage);
    }

    // ============== PageInfo.Watermark ==============

    [Fact]
    public void PageWatermark_NullByDefault()
    {
        var p = new PageInfo();
        Assert.Null(p.Watermark);
    }

    [Fact]
    public void PageWatermark_Set_Works()
    {
        var p = new PageInfo { Watermark = "CONFIDENTIAL" };
        Assert.Equal("CONFIDENTIAL", p.Watermark);
    }

    // ============== PageInfo.Unit ==============

    [Fact]
    public void PageUnit_DefaultIsMm()
    {
        var p = new PageInfo();
        Assert.Equal("mm", p.Unit);
    }

    [Fact]
    public void PageUnit_SetInch_Works()
    {
        var p = new PageInfo { Unit = "inch" };
        Assert.Equal("inch", p.Unit);
    }

    // ============== PageInfo.Orientation ==============

    [Fact]
    public void PageOrientation_DefaultIsPortrait()
    {
        var p = new PageInfo();
        Assert.Equal("portrait", p.Orientation);
    }

    [Fact]
    public void PageOrientation_SetLandscape_Works()
    {
        var p = new PageInfo { Orientation = "landscape" };
        Assert.Equal("landscape", p.Orientation);
    }

    // ============== ReportElement.Id ==============

    [Fact]
    public void ElementId_NotEmpty()
    {
        var el = new TextElement();
        Assert.NotEmpty(el.Id);
    }

    [Fact]
    public void ElementId_Unique()
    {
        var el1 = new TextElement();
        var el2 = new TextElement();
        Assert.NotEqual(el1.Id, el2.Id);
    }

    [Fact]
    public void ElementId_Set_Works()
    {
        var el = new TextElement { Id = "custom-id" };
        Assert.Equal("custom-id", el.Id);
    }

    // ============== ReportElement.Name ==============

    [Fact]
    public void ElementName_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.Name);
    }

    [Fact]
    public void ElementName_Set_Works()
    {
        var el = new TextElement { Name = "titleLabel" };
        Assert.Equal("titleLabel", el.Name);
    }

    // ============== ReportElement.Position ==============

    [Fact]
    public void ElementPosition_SetXY()
    {
        var el = new TextElement { X = 10, Y = 20 };
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
    }

    [Fact]
    public void ElementSize_SetWidthHeight()
    {
        var el = new TextElement { Width = 100, Height = 30 };
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
    }

    // ============== ReportElement.BackgroundColor ==============

    [Fact]
    public void ElementBackgroundColor_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void ElementBackgroundColor_Set_Works()
    {
        var el = new TextElement { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", el.BackgroundColor);
    }

    // ============== ReportElement.Border ==============

    [Fact]
    public void ElementBorder_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void ElementBorder_Set_Works()
    {
        var el = new TextElement { Border = new BorderDef { Width = 2 } };
        Assert.Equal(2, el.Border.Width);
    }
}
