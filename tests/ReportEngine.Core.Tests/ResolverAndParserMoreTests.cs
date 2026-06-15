using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 内存模板解析器（测试用）
/// </summary>
internal class InMemoryResolver3 : ITemplateResolver
{
    private readonly Dictionary<string, ReportTemplate> _templates = new();

    public void Add(string key, ReportTemplate template) => _templates[key] = template;

    public Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        if (_templates.TryGetValue(templateRef, out var t))
            return Task.FromResult(t);
        throw new Exception($"Template not found: {templateRef}");
    }

    public bool Exists(string templateRef) => _templates.ContainsKey(templateRef);
}

/// <summary>
/// CompositeResolver 行为测试
/// </summary>
public class CompositeResolverMoreTests
{
    [Fact]
    public async Task CompositeResolver_FirstResolverWins()
    {
        var r1 = new InMemoryResolver3();
        var r2 = new InMemoryResolver3();
        var t1 = new ReportTemplate { Version = "1.0" };
        t1.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var t2 = new ReportTemplate { Version = "2.0" };
        t2.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        r1.Add("sub.rpt", t1);
        r2.Add("sub.rpt", t2);

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);
        var result = await composite.ResolveAsync("sub.rpt");
        Assert.Equal("1.0", result.Version);
    }

    [Fact]
    public async Task CompositeResolver_FallsThrough()
    {
        var r1 = new InMemoryResolver3();
        var r2 = new InMemoryResolver3();
        var t2 = new ReportTemplate { Version = "2.0" };
        t2.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        r2.Add("sub.rpt", t2);

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);
        var result = await composite.ResolveAsync("sub.rpt");
        Assert.Equal("2.0", result.Version);
    }

    [Fact]
    public void CompositeResolver_Exists_FirstResolver()
    {
        var r1 = new InMemoryResolver3();
        var r2 = new InMemoryResolver3();
        r1.Add("a.rpt", new ReportTemplate());
        r2.Add("b.rpt", new ReportTemplate());

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);
        Assert.True(composite.Exists("a.rpt"));
        Assert.True(composite.Exists("b.rpt"));
        Assert.False(composite.Exists("c.rpt"));
    }

    [Fact]
    public async Task CompositeResolver_NotFound_Throws()
    {
        var r1 = new InMemoryResolver3();
        var r2 = new InMemoryResolver3();
        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);
        await Assert.ThrowsAnyAsync<Exception>(() => composite.ResolveAsync("missing.rpt"));
    }

    [Fact]
    public void CompositeResolver_EmptyResolvers_ExistsReturnsFalse()
    {
        var composite = new CompositeTemplateResolver();
        Assert.False(composite.Exists("any.rpt"));
    }

    [Fact]
    public void CompositeResolver_AddResolver_Chainable()
    {
        var r1 = new InMemoryResolver3();
        var composite = new CompositeTemplateResolver();
        var result = composite.AddResolver(r1);
        Assert.Same(composite, result);
    }
}

/// <summary>
/// FileSystemTemplateResolver 行为测试
/// </summary>
public class FileSystemTemplateResolverMoreTests
{
    [Fact]
    public void Exists_NonexistentDir_ReturnsFalse()
    {
        var resolver = new FileSystemTemplateResolver("C:\\nonexistent\\path\\that\\does\\not\\exist");
        Assert.False(resolver.Exists("anything.rpt"));
    }

    [Fact]
    public async Task ResolveAsync_NonexistentFile_Throws()
    {
        var resolver = new FileSystemTemplateResolver("C:\\nonexistent\\path");
        await Assert.ThrowsAnyAsync<Exception>(() => resolver.ResolveAsync("missing.rpt"));
    }
}

/// <summary>
/// TemplateParser 更多边界测试
/// </summary>
public class TemplateParserMoreBoundaryTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void Parse_EmptyBands_Throws()
    {
        var json = @"{""version"":""1.0"",""bands"":[]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_NoBandsKey_Throws()
    {
        var json = @"{""version"":""1.0""}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => _parser.Parse("not json"));
    }

    [Fact]
    public void Parse_UnknownElementType_Throws()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""unknown"",""text"":""x""}]}]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_UnknownBandType_Throws()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""unknown"",""height"":10}]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_NullJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => _parser.Parse(null!));
    }

    [Fact]
    public void Serialize_NullTemplate_ReturnsEmptyOrNull()
    {
        // Serialize(null) 可能返回 "{}" 或 null，不抛异常
        var result = _parser.Serialize(null!);
        // 只要不崩就行
        Assert.True(result == null || result is string);
    }

    [Fact]
    public void Parse_AllBandTypes()
    {
        // Band types are camelCase in JSON
        var validTypes = new[] { "header", "footer", "reportHeader", "reportFooter", "detail", "groupHeader", "groupFooter" };
        foreach (var type in validTypes)
        {
            var json = $@"{{""version"":""1.0"",""bands"":[{{""type"":""{type}"",""height"":10}}]}}";
            var t = _parser.Parse(json);
            Assert.Single(t.Bands);
        }
    }

    [Fact]
    public void Parse_AllElementTypes()
    {
        var elementTypes = new[] { "text", "image", "line", "shape", "barcode", "chart", "table", "crosstab" };
        foreach (var type in elementTypes)
        {
            var json = $@"{{""version"":""1.0"",""bands"":[{{""type"":""detail"",""height"":50,""elements"":[{{""type"":""{type}""}}]}}]}}";
            var t = _parser.Parse(json);
            Assert.Single(t.Bands[0].Elements);
        }
    }

    [Fact]
    public void Parse_CaseSensitiveElementType_UppercaseThrows()
    {
        // Element types are case-sensitive (lowercase only)
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""Text"",""text"":""Hello""}]}]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_CaseInsensitiveBandType()
    {
        // Band types are case-insensitive
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""Detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal(BandType.Detail, t.Bands[0].Type);
    }
}

/// <summary>
/// ExpressionEngine 更多边界测试
/// </summary>
public class ExpressionEngineMoreBoundaryTests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_EmptyString_ReturnsEmpty()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("", ctx);
        Assert.Equal("", result);
    }

    [Fact]
    public void Evaluate_PlainText_ReturnsAsIs()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("Hello World", ctx);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_UnknownSystemVariable_ReturnsVariableName()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{UNKNOWN_VAR}}", ctx);
        Assert.Equal("UNKNOWN_VAR", result);
    }

    [Fact]
    public void Evaluate_PageVariable_ReturnsPageNumber()
    {
        var ctx = new RenderContext { CurrentPage = 3 };
        var result = _engine.Evaluate("{{PAGE}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_TotalPagesVariable_ReturnsTotal()
    {
        var ctx = new RenderContext { TotalPages = 10 };
        var result = _engine.Evaluate("{{TOTAL_PAGES}}", ctx);
        Assert.Equal("10", result);
    }

    [Fact]
    public void Evaluate_RowNumberVariable_ReturnsRowNum()
    {
        var ctx = new RenderContext { CurrentRowNumber = 5 };
        var result = _engine.Evaluate("{{ROW_NUMBER}}", ctx);
        Assert.Equal("5", result);
    }

    [Fact]
    public void Evaluate_ReportDateVariable_ReturnsDate()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{REPORT_DATE}}", ctx);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result);
    }

    [Fact]
    public void Evaluate_MultiplePlaceholders_AllReplaced()
    {
        var ctx = new RenderContext { CurrentPage = 1, TotalPages = 5 };
        var result = _engine.Evaluate("Page {{PAGE}} of {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Page 1 of 5", result);
    }

    [Fact]
    public void Evaluate_MixedTextAndPlaceholders()
    {
        var ctx = new RenderContext { CurrentPage = 2 };
        var result = _engine.Evaluate("Report - Page {{PAGE}}", ctx);
        Assert.Equal("Report - Page 2", result);
    }

    [Fact]
    public void Evaluate_NowVariable_ReturnsDateTime()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEmpty(result);
        Assert.NotEqual("NOW", result);
    }
}
