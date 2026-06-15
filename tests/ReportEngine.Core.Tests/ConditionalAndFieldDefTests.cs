using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ConditionalFormatRule / FieldDef / BorderDef 行为测试：
///   - ConditionalFormatRule 默认值（Expression=""，颜色 null，Bold=false）
///   - ConditionalFormatRule 可改
///   - FieldDef 默认值（Type="string"，Format=null）
///   - FieldDef.ToString() 包含 Name/Type，Format 非空时也包含
///   - BorderDef 默认值（Width=1, Color="#000000", Style=Solid）
///   - BorderDef 四边默认 false
///   - BorderStyle 枚举 4 值
/// </summary>
public class ConditionalAndFieldDefTests
{
    [Fact]
    public void ConditionalFormatRule_Defaults()
    {
        var c = new ConditionalFormatRule();
        Assert.Equal("", c.Expression);
        Assert.Null(c.BackgroundColor);
        Assert.Null(c.FontColor);
        Assert.False(c.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_AllSetters()
    {
        var c = new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            BackgroundColor = "#FFFF00",
            FontColor = "#000000",
            Bold = true,
        };
        Assert.Equal("[Amount] > 1000", c.Expression);
        Assert.Equal("#FFFF00", c.BackgroundColor);
        Assert.Equal("#000000", c.FontColor);
        Assert.True(c.Bold);
    }

    [Fact]
    public void FieldDef_Defaults()
    {
        var f = new FieldDef();
        Assert.Equal("", f.Name);
        Assert.Equal("string", f.Type);
        Assert.Null(f.Format);
    }

    [Fact]
    public void FieldDef_ToString_ContainsNameAndType()
    {
        var f = new FieldDef { Name = "price", Type = "number" };
        var s = f.ToString();
        Assert.Contains("price", s);
        Assert.Contains("number", s);
    }

    [Fact]
    public void FieldDef_ToString_ContainsFormatWhenPresent()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "currency" };
        var s = f.ToString();
        Assert.Contains("currency", s);
    }

    [Fact]
    public void FieldDef_Type_AcceptsCustomString()
    {
        // Type 字段是 string，不是 enum —— 任意值都接受
        var f = new FieldDef { Type = "decimal" };
        Assert.Equal("decimal", f.Type);
    }

    [Fact]
    public void BorderDef_Defaults()
    {
        var b = new BorderDef();
        Assert.Equal(1, b.Width);
        Assert.Equal("#000000", b.Color);
        Assert.Equal(BorderStyle.Solid, b.Style);
        Assert.False(b.Top);
        Assert.False(b.Bottom);
        Assert.False(b.Left);
        Assert.False(b.Right);
    }

    [Fact]
    public void BorderDef_Sides_AllSet()
    {
        var b = new BorderDef { Top = true, Bottom = true, Left = true, Right = true };
        Assert.True(b.Top);
        Assert.True(b.Bottom);
        Assert.True(b.Left);
        Assert.True(b.Right);
    }

    [Fact]
    public void BorderDef_Style_CanBeChanged()
    {
        var b = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, b.Style);
    }

    [Fact]
    public void BorderStyle_HasFourValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(BorderStyle), BorderStyle.Solid));
        Assert.True(System.Enum.IsDefined(typeof(BorderStyle), BorderStyle.Dashed));
        Assert.True(System.Enum.IsDefined(typeof(BorderStyle), BorderStyle.Dotted));
        Assert.True(System.Enum.IsDefined(typeof(BorderStyle), BorderStyle.None));
    }

    [Fact]
    public void FieldDef_ToString_NoFormat_BracketsOnly()
    {
        var f = new FieldDef { Name = "id" };
        var s = f.ToString();
        Assert.Contains("id", s);
        Assert.Contains("string", s);
        Assert.DoesNotContain("(", s);
    }
}
