using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// MultiColumnConfig 行为测试
/// </summary>
public class MultiColumnConfigBehaviorTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var config = new MultiColumnConfig();

        Assert.Equal(2, config.ColumnCount);
        Assert.Equal(5, config.ColumnSpacing);
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void ColumnCount_DefaultIs2()
    {
        var config = new MultiColumnConfig();
        Assert.Equal(2, config.ColumnCount);
    }

    [Fact]
    public void ColumnCount_Set_Works()
    {
        var config = new MultiColumnConfig { ColumnCount = 3 };
        Assert.Equal(3, config.ColumnCount);
    }

    [Fact]
    public void ColumnSpacing_DefaultIs5()
    {
        var config = new MultiColumnConfig();
        Assert.Equal(5, config.ColumnSpacing);
    }

    [Fact]
    public void ColumnSpacing_Set_Works()
    {
        var config = new MultiColumnConfig { ColumnSpacing = 10 };
        Assert.Equal(10, config.ColumnSpacing);
    }

    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var config = new MultiColumnConfig();
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var config = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", config.Direction);
    }

    [Fact]
    public void MultiColumnConfig_LabelPrint_Works()
    {
        var config = new MultiColumnConfig
        {
            ColumnCount = 4,
            ColumnSpacing = 3,
            Direction = "Horizontal"
        };

        Assert.Equal(4, config.ColumnCount);
        Assert.Equal(3, config.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_VerticalLayout_Works()
    {
        var config = new MultiColumnConfig
        {
            ColumnCount = 2,
            Direction = "Vertical"
        };

        Assert.Equal("Vertical", config.Direction);
    }
}

/// <summary>
/// TemplateParseException 行为测试
/// </summary>
public class TemplateParseExceptionBehaviorTests
{
    [Fact]
    public void Constructor_WithMessage_Works()
    {
        var ex = new TemplateParseException("Invalid template");
        Assert.Equal("Invalid template", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInner_Works()
    {
        var inner = new Exception("Inner error");
        var ex = new TemplateParseException("Outer error", inner);
        Assert.Equal("Outer error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void TemplateParseException_IsException()
    {
        var ex = new TemplateParseException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

/// <summary>
/// TemplateNotFoundException 行为测试
/// </summary>
public class TemplateNotFoundExceptionBehaviorTests
{
    [Fact]
    public void Constructor_WithTemplateRefAndSearchPath_Works()
    {
        var ex = new TemplateNotFoundException("report.xml", "C:\\Templates");
        Assert.Equal("report.xml", ex.TemplateRef);
        Assert.Equal("C:\\Templates", ex.SearchPath);
        Assert.Contains("report.xml", ex.Message);
        Assert.Contains("C:\\Templates", ex.Message);
    }

    [Fact]
    public void TemplateNotFoundException_Properties_Works()
    {
        var ex = new TemplateNotFoundException("header.xml", "/app/templates");
        Assert.Equal("header.xml", ex.TemplateRef);
        Assert.Equal("/app/templates", ex.SearchPath);
    }

    [Fact]
    public void TemplateNotFoundException_IsException()
    {
        var ex = new TemplateNotFoundException("test", "path");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
