using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParam 完整字段测试：
///   - TemplateParam 完整字段（Name/Type/DefaultValue/Label）
///   - 字段组合行为
/// </summary>
public class TemplateParamCompleteTests
{
    [Fact]
    public void TemplateParam_Defaults()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.Name);
        Assert.Equal("string", p.Type);
        Assert.Equal("", p.DefaultValue);
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_AllSetters()
    {
        var p = new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2024-01-01",
            Label = "开始日期",
        };
        Assert.Equal("startDate", p.Name);
        Assert.Equal("date", p.Type);
        Assert.Equal("2024-01-01", p.DefaultValue);
        Assert.Equal("开始日期", p.Label);
    }

    [Fact]
    public void TemplateParam_Name_CanBeEmpty()
    {
        var p = new TemplateParam { Name = "" };
        Assert.Equal("", p.Name);
    }

    [Fact]
    public void TemplateParam_Name_CanBeAnyString()
    {
        var p = new TemplateParam { Name = "customParam" };
        Assert.Equal("customParam", p.Name);
    }

    [Fact]
    public void TemplateParam_Type_CanBeNumber()
    {
        var p = new TemplateParam { Type = "number" };
        Assert.Equal("number", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_CanBeDate()
    {
        var p = new TemplateParam { Type = "date" };
        Assert.Equal("date", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_CanBeAnyString()
    {
        var p = new TemplateParam { Type = "boolean" };
        Assert.Equal("boolean", p.Type);
    }

    [Fact]
    public void TemplateParam_DefaultValue_CanBeEmpty()
    {
        var p = new TemplateParam { DefaultValue = "" };
        Assert.Equal("", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_DefaultValue_CanBeNumber()
    {
        var p = new TemplateParam { DefaultValue = "123" };
        Assert.Equal("123", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_DefaultValue_CanBeDate()
    {
        var p = new TemplateParam { DefaultValue = "2024-12-31" };
        Assert.Equal("2024-12-31", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_Label_CanBeNull()
    {
        var p = new TemplateParam { Label = null };
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_Label_CanBeEmpty()
    {
        var p = new TemplateParam { Label = "" };
        Assert.Equal("", p.Label);
    }

    [Fact]
    public void TemplateParam_Label_CanBeChinese()
    {
        var p = new TemplateParam { Label = "参数名称" };
        Assert.Equal("参数名称", p.Label);
    }

    [Fact]
    public void TemplateParam_FullCombination()
    {
        var p = new TemplateParam
        {
            Name = "quantity",
            Type = "number",
            DefaultValue = "100",
            Label = "数量",
        };
        Assert.Equal("quantity", p.Name);
        Assert.Equal("number", p.Type);
        Assert.Equal("100", p.DefaultValue);
        Assert.Equal("数量", p.Label);
    }
}
