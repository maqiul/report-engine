using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine 模板替换边界测试
/// </summary>
public class ExpressionEngineBoundary2Tests
{
    private readonly ExpressionEngine _engine = new();

    private RenderContext MakeContext(int page = 1, int totalPages = 1)
    {
        return new RenderContext
        {
            CurrentPage = page,
            TotalPages = totalPages,
            CurrentRow = new Dictionary<string, object>()
        };
    }

    // ============== 空/边界输入 ==============

    [Fact]
    public void Evaluate_EmptyString_ReturnsEmpty()
    {
        var result = _engine.Evaluate("", MakeContext());
        Assert.Equal("", result);
    }

    [Fact]
    public void Evaluate_NoPlaceholders_ReturnsOriginal()
    {
        var result = _engine.Evaluate("Hello World", MakeContext());
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_WhitespaceOnly_ReturnsWhitespace()
    {
        var result = _engine.Evaluate("   ", MakeContext());
        Assert.Equal("   ", result);
    }

    // ============== 系统变量边界 ==============

    [Fact]
    public void Evaluate_PageNumber_Zero()
    {
        var ctx = MakeContext(0, 0);
        var result = _engine.Evaluate("{{PAGE}}", ctx);
        Assert.Equal("0", result);
    }

    [Fact]
    public void Evaluate_PageNumber_LargeNumber()
    {
        var ctx = MakeContext(9999, 9999);
        var result = _engine.Evaluate("{{PAGE}}", ctx);
        Assert.Equal("9999", result);
    }

    [Fact]
    public void Evaluate_TotalPages_MatchesPage()
    {
        var ctx = MakeContext(5, 10);
        Assert.Equal("5", _engine.Evaluate("{{PAGE}}", ctx));
        Assert.Equal("10", _engine.Evaluate("{{TOTAL_PAGES}}", ctx));
    }

    // ============== 大小写不敏感 ==============

    [Fact]
    public void Evaluate_SystemVar_CaseInsensitive_Lower()
    {
        var ctx = MakeContext(3, 10);
        var result = _engine.Evaluate("{{page}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_SystemVar_CaseInsensitive_Mixed()
    {
        var ctx = MakeContext(3, 10);
        var result = _engine.Evaluate("{{Page}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_SystemVar_CaseInsensitive_Upper()
    {
        var ctx = MakeContext(3, 10);
        var result = _engine.Evaluate("{{PAGE}}", ctx);
        Assert.Equal("3", result);
    }

    // ============== 空格处理 ==============

    [Fact]
    public void Evaluate_WhitespaceInExpression_Trimmed()
    {
        var ctx = MakeContext(5, 10);
        var result = _engine.Evaluate("{{ PAGE }}", ctx);
        Assert.Equal("5", result);
    }

    [Fact]
    public void Evaluate_LeadingTrailingSpaces_Trimmed()
    {
        var ctx = MakeContext(5, 10);
        var result = _engine.Evaluate("{{  PAGE  }}", ctx);
        Assert.Equal("5", result);
    }

    // ============== 多占位符 ==============

    [Fact]
    public void Evaluate_MultiplePlaceholders_AllReplaced()
    {
        var ctx = MakeContext(3, 10);
        var result = _engine.Evaluate("Page {{PAGE}} of {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Page 3 of 10", result);
    }

    [Fact]
    public void Evaluate_SamePlaceholderMultipleTimes_AllReplaced()
    {
        var ctx = MakeContext(5, 10);
        var result = _engine.Evaluate("{{PAGE}}-{{PAGE}}-{{PAGE}}", ctx);
        Assert.Equal("5-5-5", result);
    }

    // ============== 未匹配占位符 ==============

    [Fact]
    public void Evaluate_UnknownPlaceholder_ReturnsStrippedName()
    {
        var ctx = MakeContext();
        var result = _engine.Evaluate("{{UNKNOWN}}", ctx);
        Assert.Equal("UNKNOWN", result);
    }

    [Fact]
    public void Evaluate_Mixed_KnownAndUnknown()
    {
        var ctx = MakeContext(3, 10);
        var result = _engine.Evaluate("{{PAGE}} - {{UNKNOWN}}", ctx);
        Assert.Equal("3 - UNKNOWN", result);
    }

    // ============== 特殊字符 ==============

    [Fact]
    public void Evaluate_ChineseText_Works()
    {
        var ctx = MakeContext(1, 5);
        var result = _engine.Evaluate("第{{PAGE}}页，共{{TOTAL_PAGES}}页", ctx);
        Assert.Equal("第1页，共5页", result);
    }

    [Fact]
    public void Evaluate_SpecialChars_Works()
    {
        var ctx = MakeContext(1, 1);
        var result = _engine.Evaluate("Price: $100 ({{PAGE}})", ctx);
        Assert.Equal("Price: $100 (1)", result);
    }

    // ============== CurrentRow 字段 ==============

    [Fact]
    public void Evaluate_CurrentRow_StringField()
    {
        var ctx = MakeContext();
        ctx.CurrentRow["name"] = "Alice";
        var result = _engine.Evaluate("{{currentRow.name}}", ctx);
        Assert.Equal("Alice", result);
    }

    [Fact]
    public void Evaluate_CurrentRow_IntField()
    {
        var ctx = MakeContext();
        ctx.CurrentRow["age"] = 30;
        var result = _engine.Evaluate("{{currentRow.age}}", ctx);
        Assert.Equal("30", result);
    }

    [Fact]
    public void Evaluate_CurrentRow_DoubleField()
    {
        var ctx = MakeContext();
        ctx.CurrentRow["price"] = 99.99;
        var result = _engine.Evaluate("{{currentRow.price}}", ctx);
        Assert.Equal("99.99", result);
    }

    [Fact]
    public void Evaluate_CurrentRow_BoolField()
    {
        var ctx = MakeContext();
        ctx.CurrentRow["active"] = true;
        var result = _engine.Evaluate("{{currentRow.active}}", ctx);
        Assert.Equal("True", result);
    }

    [Fact]
    public void Evaluate_CurrentRow_MissingField_ReturnsOriginal()
    {
        var ctx = MakeContext();
        var result = _engine.Evaluate("{{currentRow.missing}}", ctx);
        Assert.Equal("{{currentRow.missing}}", result);
    }

    [Fact]
    public void Evaluate_CurrentRow_NullRow_ReturnsOriginal()
    {
        var ctx = MakeContext();
        ctx.CurrentRow = null!;
        var result = _engine.Evaluate("{{currentRow.name}}", ctx);
        Assert.Equal("{{currentRow.name}}", result);
    }

    // ============== 简单字段引用 ==============

    [Fact]
    public void Evaluate_SimpleField_Works()
    {
        var ctx = MakeContext();
        ctx.CurrentRow["name"] = "Bob";
        var result = _engine.Evaluate("{{name}}", ctx);
        Assert.Equal("Bob", result);
    }

    // ============== NOW 变量 ==============

    [Fact]
    public void Evaluate_NOW_ReturnsDateTime()
    {
        var ctx = MakeContext();
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEqual("{{NOW}}", result);
        Assert.NotEmpty(result);
    }

    // ============== REPORT_DATE 变量 ==============

    [Fact]
    public void Evaluate_REPORT_DATE_ReturnsFormattedDate()
    {
        var ctx = MakeContext();
        var result = _engine.Evaluate("{{REPORT_DATE}}", ctx);
        Assert.NotEqual("{{REPORT_DATE}}", result);
        Assert.NotEmpty(result);
    }

    // ============== ROW_NUMBER 变量 ==============

    [Fact]
    public void Evaluate_ROW_NUMBER_ReturnsRowNumber()
    {
        var ctx = MakeContext();
        ctx.CurrentRowNumber = 5;
        var result = _engine.Evaluate("{{ROW_NUMBER}}", ctx);
        Assert.Equal("5", result);
    }

    [Fact]
    public void Evaluate_ROW_NUMBER_DefaultIsZero()
    {
        var ctx = MakeContext();
        var result = _engine.Evaluate("{{ROW_NUMBER}}", ctx);
        Assert.Equal("0", result);
    }
}

/// <summary>
/// FontDef 完整属性测试 (改名避免冲突)
/// </summary>
public class FontDefComplete2Tests
{
    [Fact]
    public void Family_DefaultIsSimSun()
    {
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
    }

    [Fact]
    public void Family_Set_Works()
    {
        var f = new FontDef { Family = "Arial" };
        Assert.Equal("Arial", f.Family);
    }

    [Fact]
    public void Size_DefaultIs10()
    {
        var f = new FontDef();
        Assert.Equal(10, f.Size);
    }

    [Fact]
    public void Size_Set_Works()
    {
        var f = new FontDef { Size = 14 };
        Assert.Equal(14, f.Size);
    }

    [Fact]
    public void Bold_FalseByDefault()
    {
        var f = new FontDef();
        Assert.False(f.Bold);
    }

    [Fact]
    public void Bold_SetTrue_Works()
    {
        var f = new FontDef { Bold = true };
        Assert.True(f.Bold);
    }

    [Fact]
    public void Italic_FalseByDefault()
    {
        var f = new FontDef();
        Assert.False(f.Italic);
    }

    [Fact]
    public void Italic_SetTrue_Works()
    {
        var f = new FontDef { Italic = true };
        Assert.True(f.Italic);
    }

    [Fact]
    public void Underline_FalseByDefault()
    {
        var f = new FontDef();
        Assert.False(f.Underline);
    }

    [Fact]
    public void Underline_SetTrue_Works()
    {
        var f = new FontDef { Underline = true };
        Assert.True(f.Underline);
    }

    [Fact]
    public void Color_NullByDefault()
    {
        var f = new FontDef();
        Assert.Null(f.Color);
    }

    [Fact]
    public void Color_Set_Works()
    {
        var f = new FontDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", f.Color);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var f = new FontDef
        {
            Family = "Microsoft YaHei",
            Size = 12,
            Bold = true,
            Italic = true,
            Underline = true,
            Color = "#0000FF"
        };

        Assert.Equal("Microsoft YaHei", f.Family);
        Assert.Equal(12, f.Size);
        Assert.True(f.Bold);
        Assert.True(f.Italic);
        Assert.True(f.Underline);
        Assert.Equal("#0000FF", f.Color);
    }
}
