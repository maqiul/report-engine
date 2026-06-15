using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParam 行为测试：
///   - 默认值
///   - 名称
///   - 类型
///   - 默认值
///   - 标签
/// </summary>
public class TemplateParamBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var param = new TemplateParam();

        Assert.Equal("", param.Name);
        Assert.Equal("string", param.Type);
        Assert.Equal("", param.DefaultValue);
        Assert.Null(param.Label);
    }

    // ============== Name ==============

    [Fact]
    public void Name_EmptyByDefault()
    {
        var param = new TemplateParam();
        Assert.Equal("", param.Name);
    }

    [Fact]
    public void Name_SetAndGet_Works()
    {
        var param = new TemplateParam { Name = "startDate" };
        Assert.Equal("startDate", param.Name);
    }

    [Fact]
    public void Name_ChineseCharacters_Works()
    {
        var param = new TemplateParam { Name = "开始日期" };
        Assert.Equal("开始日期", param.Name);
    }

    [Fact]
    public void Name_WithUnderscore_Works()
    {
        var param = new TemplateParam { Name = "start_date" };
        Assert.Equal("start_date", param.Name);
    }

    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsString()
    {
        var param = new TemplateParam();
        Assert.Equal("string", param.Type);
    }

    [Fact]
    public void Type_SetNumber_Works()
    {
        var param = new TemplateParam { Type = "number" };
        Assert.Equal("number", param.Type);
    }

    [Fact]
    public void Type_SetDate_Works()
    {
        var param = new TemplateParam { Type = "date" };
        Assert.Equal("date", param.Type);
    }

    [Fact]
    public void Type_SetBoolean_Works()
    {
        var param = new TemplateParam { Type = "boolean" };
        Assert.Equal("boolean", param.Type);
    }

    [Fact]
    public void Type_AnyString_Accepted()
    {
        var param = new TemplateParam { Type = "custom" };
        Assert.Equal("custom", param.Type);
    }

    // ============== DefaultValue ==============

    [Fact]
    public void DefaultValue_EmptyByDefault()
    {
        var param = new TemplateParam();
        Assert.Equal("", param.DefaultValue);
    }

    [Fact]
    public void DefaultValue_SetString_Works()
    {
        var param = new TemplateParam { DefaultValue = "hello" };
        Assert.Equal("hello", param.DefaultValue);
    }

    [Fact]
    public void DefaultValue_SetNumber_Works()
    {
        var param = new TemplateParam { DefaultValue = "100" };
        Assert.Equal("100", param.DefaultValue);
    }

    [Fact]
    public void DefaultValue_SetDate_Works()
    {
        var param = new TemplateParam { DefaultValue = "2024-01-01" };
        Assert.Equal("2024-01-01", param.DefaultValue);
    }

    [Fact]
    public void DefaultValue_SetBoolean_Works()
    {
        var param = new TemplateParam { DefaultValue = "true" };
        Assert.Equal("true", param.DefaultValue);
    }

    [Fact]
    public void DefaultValue_ChineseCharacters_Works()
    {
        var param = new TemplateParam { DefaultValue = "默认值" };
        Assert.Equal("默认值", param.DefaultValue);
    }

    // ============== Label ==============

    [Fact]
    public void Label_NullByDefault()
    {
        var param = new TemplateParam();
        Assert.Null(param.Label);
    }

    [Fact]
    public void Label_SetEnglish_Works()
    {
        var param = new TemplateParam { Label = "Start Date" };
        Assert.Equal("Start Date", param.Label);
    }

    [Fact]
    public void Label_SetChinese_Works()
    {
        var param = new TemplateParam { Label = "开始日期" };
        Assert.Equal("开始日期", param.Label);
    }

    [Fact]
    public void Label_CanBeCleared()
    {
        var param = new TemplateParam { Label = "Start Date" };
        param.Label = null;
        Assert.Null(param.Label);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void TemplateParam_StringParam_Works()
    {
        var param = new TemplateParam
        {
            Name = "title",
            Type = "string",
            DefaultValue = "报告标题",
            Label = "标题"
        };

        Assert.Equal("title", param.Name);
        Assert.Equal("string", param.Type);
        Assert.Equal("报告标题", param.DefaultValue);
        Assert.Equal("标题", param.Label);
    }

    [Fact]
    public void TemplateParam_NumberParam_Works()
    {
        var param = new TemplateParam
        {
            Name = "pageSize",
            Type = "number",
            DefaultValue = "20",
            Label = "每页条数"
        };

        Assert.Equal("pageSize", param.Name);
        Assert.Equal("number", param.Type);
        Assert.Equal("20", param.DefaultValue);
    }

    [Fact]
    public void TemplateParam_DateParam_Works()
    {
        var param = new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2024-01-01",
            Label = "开始日期"
        };

        Assert.Equal("startDate", param.Name);
        Assert.Equal("date", param.Type);
        Assert.Equal("2024-01-01", param.DefaultValue);
        Assert.Equal("开始日期", param.Label);
    }

    [Fact]
    public void TemplateParam_BooleanParam_Works()
    {
        var param = new TemplateParam
        {
            Name = "showDetails",
            Type = "boolean",
            DefaultValue = "true",
            Label = "显示详情"
        };

        Assert.Equal("boolean", param.Type);
        Assert.Equal("true", param.DefaultValue);
    }

    [Fact]
    public void TemplateParam_InReportTemplate_Works()
    {
        var template = new ReportTemplate();
        template.Parameters.Add(new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2024-01-01",
            Label = "开始日期"
        });
        template.Parameters.Add(new TemplateParam
        {
            Name = "endDate",
            Type = "date",
            DefaultValue = "2024-12-31",
            Label = "结束日期"
        });

        Assert.Equal(2, template.Parameters.Count);
        Assert.Equal("startDate", template.Parameters[0].Name);
        Assert.Equal("endDate", template.Parameters[1].Name);
    }

    [Fact]
    public void TemplateParam_CanBeModified()
    {
        var param = new TemplateParam { Name = "title", Type = "string" };
        
        param.Type = "number";
        param.DefaultValue = "100";
        param.Label = "数值参数";
        
        Assert.Equal("number", param.Type);
        Assert.Equal("100", param.DefaultValue);
        Assert.Equal("数值参数", param.Label);
    }
}
