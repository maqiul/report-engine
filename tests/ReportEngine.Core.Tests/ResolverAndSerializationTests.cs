using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ITemplateResolver 缓存行为测试（通过 FileSystemTemplateResolver）
/// </summary>
public class TemplateResolverCacheTests
{
    [Fact]
    public async Task ResolveAsync_CachesResult()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var parser = new TemplateParser();
            var t = new ReportTemplate { Version = "1.0" };
            t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
            var json = parser.Serialize(t);
            File.WriteAllText(Path.Combine(tempDir, "sub.rptx"), json);

            var resolver = new FileSystemTemplateResolver(tempDir);
            var result1 = await resolver.ResolveAsync("sub.rptx");
            var result2 = await resolver.ResolveAsync("sub.rptx");
            Assert.Same(result1, result2);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ResolveAsync_NotFound_ThrowsTemplateNotFoundException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var resolver = new FileSystemTemplateResolver(tempDir);
            await Assert.ThrowsAsync<TemplateNotFoundException>(() => resolver.ResolveAsync("missing.rptx"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Exists_Found_ReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var parser = new TemplateParser();
            var t = new ReportTemplate();
            t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
            var json = parser.Serialize(t);
            File.WriteAllText(Path.Combine(tempDir, "sub.rptx"), json);

            var resolver = new FileSystemTemplateResolver(tempDir);
            Assert.True(resolver.Exists("sub.rptx"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Exists_NotFound_ReturnsFalse()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var resolver = new FileSystemTemplateResolver(tempDir);
            Assert.False(resolver.Exists("missing.rptx"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

/// <summary>
/// TemplateNotFoundException 测试
/// </summary>
public class TemplateNotFoundExceptionTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var ex = new TemplateNotFoundException("test.rpt", "/templates");
        Assert.Equal("test.rpt", ex.TemplateRef);
        Assert.Equal("/templates", ex.SearchPath);
    }

    [Fact]
    public void Message_ContainsTemplateRef()
    {
        var ex = new TemplateNotFoundException("test.rpt", "/templates");
        Assert.Contains("test.rpt", ex.Message);
    }

    [Fact]
    public void Message_ContainsSearchPath()
    {
        var ex = new TemplateNotFoundException("test.rpt", "/templates");
        Assert.Contains("/templates", ex.Message);
    }

    [Fact]
    public void InheritsFromException()
    {
        var ex = new TemplateNotFoundException("test.rpt", "/templates");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

/// <summary>
/// 内存模板解析器（测试用）
/// </summary>
internal class InMemoryResolver4 : ITemplateResolver
{
    private readonly Dictionary<string, ReportTemplate> _templates = new();

    public void Add(string key, ReportTemplate template) => _templates[key] = template;

    public Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        if (_templates.TryGetValue(templateRef, out var t))
            return Task.FromResult(t);
        throw new TemplateNotFoundException(templateRef, "memory");
    }

    public bool Exists(string templateRef) => _templates.ContainsKey(templateRef);
}

/// <summary>
/// CompositeTemplateResolver 更多行为测试
/// </summary>
public class CompositeResolverBehaviorTests
{
    [Fact]
    public async Task ResolveAsync_NotFound_ThrowsTemplateNotFoundException()
    {
        var r1 = new InMemoryResolver4();
        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        var ex = await Assert.ThrowsAsync<TemplateNotFoundException>(() => composite.ResolveAsync("missing.rpt"));
        Assert.Contains("missing.rpt", ex.Message);
    }

    [Fact]
    public async Task ResolveAsync_MultipleResolvers_SecondWins()
    {
        var r1 = new InMemoryResolver4();
        var r2 = new InMemoryResolver4();
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
    public async Task ResolveAsync_ThreeResolvers_ThirdWins()
    {
        var r1 = new InMemoryResolver4();
        var r2 = new InMemoryResolver4();
        var r3 = new InMemoryResolver4();
        var t3 = new ReportTemplate { Version = "3.0" };
        t3.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        r3.Add("sub.rpt", t3);

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);
        composite.AddResolver(r3);
        var result = await composite.ResolveAsync("sub.rpt");
        Assert.Equal("3.0", result.Version);
    }

    [Fact]
    public void Exists_EmptyComposite_ReturnsFalse()
    {
        var composite = new CompositeTemplateResolver();
        Assert.False(composite.Exists("any.rpt"));
    }

    [Fact]
    public void Exists_WithMultipleResolvers_AnyExists()
    {
        var r1 = new InMemoryResolver4();
        var r2 = new InMemoryResolver4();
        r2.Add("b.rpt", new ReportTemplate());

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);
        Assert.True(composite.Exists("b.rpt"));
        Assert.False(composite.Exists("a.rpt"));
    }
}

/// <summary>
/// TemplateParser 序列化格式测试
/// </summary>
public class TemplateParserSerializationFormatTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void Serialize_OmitsNullFields()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        // Author is null by default, should not appear
        Assert.DoesNotContain("\"author\"", json);
        Assert.DoesNotContain("\"description\"", json);
    }

    [Fact]
    public void Serialize_IncludesNonEmptyAuthor()
    {
        var t = new ReportTemplate { Author = "Test" };
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("\"author\"", json);
        Assert.Contains("Test", json);
    }

    [Fact]
    public void Serialize_EnumAsCamelCase()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.GroupHeader, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("groupHeader", json);
    }

    [Fact]
    public void Serialize_BandType_Detail()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("\"detail\"", json);
    }

    [Fact]
    public void Serialize_BandType_ReportHeader()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("reportHeader", json);
    }

    [Fact]
    public void Serialize_BandType_ReportFooter()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("reportFooter", json);
    }

    [Fact]
    public void Serialize_BandType_GroupFooter()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.GroupFooter, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("groupFooter", json);
    }

    [Fact]
    public void Serialize_TextAlignment_Right()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement { Text = "Right", Alignment = TextAlignment.Right });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("right", json);
    }

    [Fact]
    public void Serialize_BorderStyle_Dashed()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement
        {
            Text = "Bordered",
            Border = new BorderDef { Style = BorderStyle.Dashed }
        });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("dashed", json);
    }

    [Fact]
    public void Serialize_ImageSizing_Stretch()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new ImageElement { Source = "img.png", Sizing = ImageSizing.Stretch });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("stretch", json);
    }

    [Fact]
    public void Serialize_ChartType_Pie()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new ChartElement { ChartType = ChartType.Pie });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("pie", json);
    }

    [Fact]
    public void Serialize_BarcodeFormat_QRCode()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new BarcodeElement { Value = "test", Format = BarcodeFormat.QRCode });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("qrCode", json);
    }

    [Fact]
    public void Serialize_ShapeType_Ellipse()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new ShapeElement { Shape = ShapeType.Ellipse });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("ellipse", json);
    }

    [Fact]
    public void Serialize_LineDirection_Vertical()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new LineElement { Direction = LineDirection.Vertical });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("vertical", json);
    }
}

/// <summary>
/// ReportElement 基类属性完整测试
/// </summary>
public class ReportElementBaseFinalTests
{
    [Fact]
    public void ReportElement_Id_AutoGenerated()
    {
        var el = new TextElement();
        Assert.NotEmpty(el.Id);
    }

    [Fact]
    public void ReportElement_Id_UniquePerInstance()
    {
        var el1 = new TextElement();
        var el2 = new TextElement();
        Assert.NotEqual(el1.Id, el2.Id);
    }

    [Fact]
    public void ReportElement_Name_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Name);
    }

    [Fact]
    public void ReportElement_GroupId_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.GroupId);
    }

    [Fact]
    public void ReportElement_Visible_DefaultTrue()
    {
        var el = new TextElement();
        Assert.True(el.Visible);
    }

    [Fact]
    public void ReportElement_Locked_DefaultFalse()
    {
        var el = new TextElement();
        Assert.False(el.Locked);
    }

    [Fact]
    public void ReportElement_Rotation_DefaultZero()
    {
        var el = new TextElement();
        Assert.Equal(0, el.Rotation);
    }

    [Fact]
    public void ReportElement_Opacity_DefaultOne()
    {
        var el = new TextElement();
        Assert.Equal(1.0, el.Opacity);
    }

    [Fact]
    public void ReportElement_VisibleExpression_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.VisibleExpression);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_DefaultEmpty()
    {
        var el = new TextElement();
        Assert.NotNull(el.ConditionalFormats);
        Assert.Empty(el.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_BackgroundColor_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void ReportElement_Border_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void ReportElement_AllPropertiesSettable()
    {
        var el = new TextElement
        {
            Name = "test",
            GroupId = "group1",
            X = 10, Y = 20,
            Width = 100, Height = 30,
            Visible = false,
            Locked = true,
            Rotation = 45,
            Opacity = 0.5,
            BackgroundColor = "#FF0000",
            VisibleExpression = "[x]>0"
        };

        Assert.Equal("test", el.Name);
        Assert.Equal("group1", el.GroupId);
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
        Assert.False(el.Visible);
        Assert.True(el.Locked);
        Assert.Equal(45, el.Rotation);
        Assert.Equal(0.5, el.Opacity);
        Assert.Equal("#FF0000", el.BackgroundColor);
        Assert.Equal("[x]>0", el.VisibleExpression);
    }
}
