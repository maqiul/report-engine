using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// Margin 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class MarginCompleteTests
{
    [Fact]
    public void Margin_Top_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Top);
    }

    [Fact]
    public void Margin_Bottom_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Bottom);
    }

    [Fact]
    public void Margin_Left_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Left);
    }

    [Fact]
    public void Margin_Right_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Right);
    }

    [Fact]
    public void Margin_SetCustomValues()
    {
        var m = new Margin { Top = 5, Bottom = 15, Left = 20, Right = 25 };
        Assert.Equal(5, m.Top);
        Assert.Equal(15, m.Bottom);
        Assert.Equal(20, m.Left);
        Assert.Equal(25, m.Right);
    }

    [Fact]
    public void Margin_SetZero()
    {
        var m = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 };
        Assert.Equal(0, m.Top);
        Assert.Equal(0, m.Bottom);
        Assert.Equal(0, m.Left);
        Assert.Equal(0, m.Right);
    }

    [Fact]
    public void Margin_SetLargeValues()
    {
        var m = new Margin { Top = 100, Bottom = 100, Left = 100, Right = 100 };
        Assert.Equal(100, m.Top);
        Assert.Equal(100, m.Bottom);
        Assert.Equal(100, m.Left);
        Assert.Equal(100, m.Right);
    }

    [Fact]
    public void Margin_SetDecimalValues()
    {
        var m = new Margin { Top = 10.5, Bottom = 15.7, Left = 20.3, Right = 25.9 };
        Assert.Equal(10.5, m.Top);
        Assert.Equal(15.7, m.Bottom);
        Assert.Equal(20.3, m.Left);
        Assert.Equal(25.9, m.Right);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PageInfo 额外属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class PageInfoExtra3Tests
{
    [Fact]
    public void PageInfo_Unit_DefaultMM()
    {
        var p = new PageInfo();
        Assert.Equal("mm", p.Unit);
    }

    [Fact]
    public void PageInfo_Unit_SetInch()
    {
        var p = new PageInfo { Unit = "inch" };
        Assert.Equal("inch", p.Unit);
    }

    [Fact]
    public void PageInfo_BackgroundColor_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundColor_SetValue()
    {
        var p = new PageInfo { BackgroundColor = "#FFFFFF" };
        Assert.Equal("#FFFFFF", p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundImage_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_BackgroundImage_SetValue()
    {
        var p = new PageInfo { BackgroundImage = "bg.png" };
        Assert.Equal("bg.png", p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_Watermark_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.Watermark);
    }

    [Fact]
    public void PageInfo_Watermark_SetValue()
    {
        var p = new PageInfo { Watermark = "CONFIDENTIAL" };
        Assert.Equal("CONFIDENTIAL", p.Watermark);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TemplateParam 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class TemplateParamComplete3Tests
{
    [Fact]
    public void TemplateParam_Name_DefaultEmpty()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.Name);
    }

    [Fact]
    public void TemplateParam_Name_SetValue()
    {
        var p = new TemplateParam { Name = "startDate" };
        Assert.Equal("startDate", p.Name);
    }

    [Fact]
    public void TemplateParam_Type_DefaultString()
    {
        var p = new TemplateParam();
        Assert.Equal("string", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_SetNumber()
    {
        var p = new TemplateParam { Type = "number" };
        Assert.Equal("number", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_SetDate()
    {
        var p = new TemplateParam { Type = "date" };
        Assert.Equal("date", p.Type);
    }

    [Fact]
    public void TemplateParam_DefaultValue_DefaultEmpty()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_DefaultValue_SetValue()
    {
        var p = new TemplateParam { DefaultValue = "2026-01-01" };
        Assert.Equal("2026-01-01", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_Label_DefaultNull()
    {
        var p = new TemplateParam();
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_Label_SetValue()
    {
        var p = new TemplateParam { Label = "开始日期" };
        Assert.Equal("开始日期", p.Label);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ReportTemplate 额外属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ReportTemplateExtra2Tests
{
    [Fact]
    public void ReportTemplate_Author_DefaultNull()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Author);
    }

    [Fact]
    public void ReportTemplate_Author_SetValue()
    {
        var t = new ReportTemplate { Author = "张三" };
        Assert.Equal("张三", t.Author);
    }

    [Fact]
    public void ReportTemplate_Description_DefaultNull()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Description);
    }

    [Fact]
    public void ReportTemplate_Description_SetValue()
    {
        var t = new ReportTemplate { Description = "月度销售统计" };
        Assert.Equal("月度销售统计", t.Description);
    }

    [Fact]
    public void ReportTemplate_Parameters_DefaultEmpty()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.Parameters);
        Assert.Empty(t.Parameters);
    }

    [Fact]
    public void ReportTemplate_Parameters_AddItem()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "startDate" });
        Assert.Single(t.Parameters);
        Assert.Equal("startDate", t.Parameters[0].Name);
    }
}
