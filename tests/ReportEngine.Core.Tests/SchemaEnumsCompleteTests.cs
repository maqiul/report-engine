using System;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Schema 枚举完整测试：
///   - TextAlignment (4 值)
///   - ImageSizing (4 值)
///   - LineDirection (3 值)
///   - ShapeType (4 值)
/// </summary>
public class SchemaEnumsCompleteTests
{
    // ============== TextAlignment ==============

    [Fact]
    public void TextAlignment_Has4Values()
    {
        Assert.Equal(4, Enum.GetValues(typeof(TextAlignment)).Length);
    }

    [Fact]
    public void TextAlignment_HasLeft()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Left));
    }

    [Fact]
    public void TextAlignment_HasCenter()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Center));
    }

    [Fact]
    public void TextAlignment_HasRight()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Right));
    }

    [Fact]
    public void TextAlignment_HasJustify()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Justify));
    }

    [Fact]
    public void TextAlignment_DefaultIsLeft()
    {
        Assert.Equal(TextAlignment.Left, default(TextAlignment));
    }

    // ============== ImageSizing ==============

    [Fact]
    public void ImageSizing_Has4Values()
    {
        Assert.Equal(4, Enum.GetValues(typeof(ImageSizing)).Length);
    }

    [Fact]
    public void ImageSizing_HasStretch()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.Stretch));
    }

    [Fact]
    public void ImageSizing_HasFitProportional()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.FitProportional));
    }

    [Fact]
    public void ImageSizing_HasClip()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.Clip));
    }

    [Fact]
    public void ImageSizing_HasActualSize()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.ActualSize));
    }

    [Fact]
    public void ImageSizing_DefaultIsStretch()
    {
        Assert.Equal(ImageSizing.Stretch, default(ImageSizing));
    }

    // ============== LineDirection ==============

    [Fact]
    public void LineDirection_Has3Values()
    {
        Assert.Equal(3, Enum.GetValues(typeof(LineDirection)).Length);
    }

    [Fact]
    public void LineDirection_HasHorizontal()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Horizontal));
    }

    [Fact]
    public void LineDirection_HasVertical()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Vertical));
    }

    [Fact]
    public void LineDirection_HasDiagonal()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Diagonal));
    }

    [Fact]
    public void LineDirection_DefaultIsHorizontal()
    {
        Assert.Equal(LineDirection.Horizontal, default(LineDirection));
    }

    // ============== ShapeType ==============

    [Fact]
    public void ShapeType_Has4Values()
    {
        Assert.Equal(4, Enum.GetValues(typeof(ShapeType)).Length);
    }

    [Fact]
    public void ShapeType_HasRectangle()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Rectangle));
    }

    [Fact]
    public void ShapeType_HasEllipse()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Ellipse));
    }

    [Fact]
    public void ShapeType_HasRoundedRect()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.RoundedRect));
    }

    [Fact]
    public void ShapeType_HasTriangle()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Triangle));
    }

    [Fact]
    public void ShapeType_DefaultIsRectangle()
    {
        Assert.Equal(ShapeType.Rectangle, default(ShapeType));
    }

    // ============== TextBoxType ==============

    [Fact]
    public void TextBoxType_Has4Values()
    {
        Assert.Equal(4, Enum.GetValues(typeof(TextBoxType)).Length);
    }

    [Fact]
    public void TextBoxType_HasStatic()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.Static));
    }

    [Fact]
    public void TextBoxType_HasField()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.Field));
    }

    [Fact]
    public void TextBoxType_HasSummary()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.Summary));
    }

    [Fact]
    public void TextBoxType_HasSysVar()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.SysVar));
    }

    [Fact]
    public void TextBoxType_DefaultIsStatic()
    {
        Assert.Equal(TextBoxType.Static, default(TextBoxType));
    }
}
