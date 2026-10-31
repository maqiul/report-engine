using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 元素旋转 + 长文本自动换行撑高 测试
/// </summary>
public class RotationAndLongTextTests
{
    // ============ TextWrapper 换行算法 ============

    [Fact]
    public void TextWrapper_FontMm_ConvertsPointsToMillimeters()
    {
        // 12pt = 12 * 25.4/72 = 4.2333mm
        Assert.Equal(4.2333, TextWrapper.FontMm(12), 3);
    }

    [Fact]
    public void TextWrapper_LineHeight_IsFontTimes1_2()
    {
        Assert.Equal(TextWrapper.FontMm(10) * 1.2, TextWrapper.LineHeightMm(10), 6);
    }

    [Fact]
    public void TextWrapper_Wrap_SplitsChineseTextByWidth()
    {
        // 宽度 20mm，字号 10pt(3.5278mm) → 每行约 5.67em ≈ 5 个全角字
        var lines = TextWrapper.Wrap("一二三四五六七八九十", 20, 10);
        Assert.True(lines.Count > 1, $"应换行成多行，实际 {lines.Count} 行");
        Assert.Equal("一二三四五六七八九十", string.Concat(lines));
    }

    [Fact]
    public void TextWrapper_Wrap_EnglishTextUsesHalfWidthChars()
    {
        // 半角字符宽度 0.5em → 同样宽度能放更多字符
        var lines = TextWrapper.Wrap("abcdefghijklmnop", 20, 10);
        Assert.True(lines.Count > 1);
        Assert.Equal("abcdefghijklmnop", string.Concat(lines));
    }

    [Fact]
    public void TextWrapper_Wrap_KeepsExplicitNewlines()
    {
        var lines = TextWrapper.Wrap("第一行\n第二行", 100, 10);
        Assert.Equal(2, lines.Count);
        Assert.Equal("第一行", lines[0]);
        Assert.Equal("第二行", lines[1]);
    }

    [Fact]
    public void TextWrapper_Wrap_MaxLines_TruncatesWithEllipsis()
    {
        var lines = TextWrapper.Wrap("一二三四五六七八九十十一十二", 20, 10, maxLines: 2);
        Assert.Equal(2, lines.Count);
        Assert.EndsWith("…", lines[1]);
    }

    [Fact]
    public void TextWrapper_Wrap_EmptyText_ReturnsSingleEmptyLine()
    {
        var lines = TextWrapper.Wrap("", 50, 10);
        Assert.Single(lines);
        Assert.Equal("", lines[0]);
    }

    [Fact]
    public void TextWrapper_IsFullWidth_DetectsCjk()
    {
        Assert.True(TextWrapper.IsFullWidth('中'));
        Assert.True(TextWrapper.IsFullWidth('，'));
        Assert.False(TextWrapper.IsFullWidth('a'));
        Assert.False(TextWrapper.IsFullWidth('1'));
    }

    // ============ 渲染器：旋转透传 ============

    private static ReportRenderer NewRenderer() => new ReportRenderer(new NoopResolver());

    [Fact]
    public async Task Render_TextElement_RotationPassedThrough()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "旋转", X = 0, Y = 0, Width = 40, Height = 8, Rotation = 30 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>> { new() { ["x"] = 1 } }
        };

        var rendered = await NewRenderer().RenderAsync(t, ds);
        var el = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedTextElement>().First();
        Assert.Equal(30, el.Rotation);
    }

    [Fact]
    public async Task Render_ImageElement_RotationPassedThrough()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new ImageElement { Source = "a.png", X = 0, Y = 0, Width = 10, Height = 10, Rotation = 45 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>> { new() { ["x"] = 1 } }
        };

        var rendered = await NewRenderer().RenderAsync(t, ds);
        var el = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedImageElement>().First();
        Assert.Equal(45, el.Rotation);
    }

    [Fact]
    public async Task Render_ShapeElement_RotationPassedThrough()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new ShapeElement { Shape = ShapeType.Rectangle, X = 0, Y = 0, Width = 10, Height = 10, Rotation = -15 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>> { new() { ["x"] = 1 } }
        };

        var rendered = await NewRenderer().RenderAsync(t, ds);
        var el = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedShapeElement>().First();
        Assert.Equal(-15, el.Rotation);
    }

    // ============ 渲染器：长文本换行 ============

    [Fact]
    public async Task Render_LongText_ProducesMultipleLines()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "这是一段很长的中文文本需要自动换行处理",
            X = 0, Y = 0, Width = 25, Height = 8,
            Font = new FontDef { Size = 10 },
            CanGrow = true
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>> { new() { ["x"] = 1 } }
        };

        var rendered = await NewRenderer().RenderAsync(t, ds);
        var el = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedTextElement>().First();

        Assert.True(el.Lines.Count > 1, $"应换行成多行，实际 {el.Lines.Count}");
        Assert.Equal("这是一段很长的中文文本需要自动换行处理", string.Concat(el.Lines));
        Assert.True(el.LineHeight > 0);
        // canGrow → 撑高 = 行数 * 行高
        Assert.Equal(el.Lines.Count * el.LineHeight, el.GrownHeight, 6);
    }

    [Fact]
    public async Task Render_LongText_NoCanGrow_KeepsOriginalHeight()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "这是一段很长的中文文本需要自动换行处理",
            X = 0, Y = 0, Width = 25, Height = 8,
            Font = new FontDef { Size = 10 },
            CanGrow = false
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>> { new() { ["x"] = 1 } }
        };

        var rendered = await NewRenderer().RenderAsync(t, ds);
        var el = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedTextElement>().First();

        Assert.True(el.Lines.Count > 1);
        Assert.Equal(0, el.GrownHeight); // 未撑高
        Assert.Equal(8, el.Height);      // 原高度不变
    }

    [Fact]
    public async Task Render_LongText_MaxLines_TruncatesLines()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "这是一段很长的中文文本需要自动换行处理",
            X = 0, Y = 0, Width = 25, Height = 8,
            Font = new FontDef { Size = 10 },
            CanGrow = true,
            MaxLines = 2
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>> { new() { ["x"] = 1 } }
        };

        var rendered = await NewRenderer().RenderAsync(t, ds);
        var el = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedTextElement>().First();

        Assert.Equal(2, el.Lines.Count);
        Assert.EndsWith("…", el.Lines[1]);
    }

    private sealed class NoopResolver : SubReports.ITemplateResolver
    {
        public bool Exists(string templateRef) => false;
        public System.Threading.Tasks.Task<ReportTemplate> ResolveAsync(string templateRef)
            => throw new System.NotSupportedException();
    }
}
