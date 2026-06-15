using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 枚举值验证测试：确保所有枚举类型定义正确
/// </summary>
public class EnumValuesTests
{
    // ============== BandType ==============

    [Fact]
    public void BandType_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(BandType)).Cast<BandType>().ToList();
        
        Assert.Contains(BandType.Header, values);
        Assert.Contains(BandType.Footer, values);
        Assert.Contains(BandType.ReportHeader, values);
        Assert.Contains(BandType.ReportFooter, values);
        Assert.Contains(BandType.Detail, values);
        Assert.Contains(BandType.GroupHeader, values);
        Assert.Contains(BandType.GroupFooter, values);
        Assert.Equal(7, values.Count);
    }

    [Fact]
    public void BandType_Default_IsHeader()
    {
        var defaultVal = default(BandType);
        Assert.Equal(BandType.Header, defaultVal);
    }

    // ============== BorderStyle ==============

    [Fact]
    public void BorderStyle_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(BorderStyle)).Cast<BorderStyle>().ToList();
        
        Assert.Contains(BorderStyle.Solid, values);
        Assert.Contains(BorderStyle.Dashed, values);
        Assert.Contains(BorderStyle.Dotted, values);
        Assert.Contains(BorderStyle.None, values);
        Assert.Equal(4, values.Count);
    }

    [Fact]
    public void BorderStyle_Default_IsSolid()
    {
        var defaultVal = default(BorderStyle);
        Assert.Equal(BorderStyle.Solid, defaultVal);
    }

    // ============== TextBoxType ==============

    [Fact]
    public void TextBoxType_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(TextBoxType)).Cast<TextBoxType>().ToList();
        
        Assert.Contains(TextBoxType.Static, values);
        Assert.Contains(TextBoxType.Field, values);
        Assert.Contains(TextBoxType.Summary, values);
        Assert.Contains(TextBoxType.SysVar, values);
        Assert.Equal(4, values.Count);
    }

    // ============== TextAlignment ==============

    [Fact]
    public void TextAlignment_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(TextAlignment)).Cast<TextAlignment>().ToList();
        
        Assert.Contains(TextAlignment.Left, values);
        Assert.Contains(TextAlignment.Center, values);
        Assert.Contains(TextAlignment.Right, values);
        Assert.Contains(TextAlignment.Justify, values);
        Assert.Equal(4, values.Count);
    }

    [Fact]
    public void TextAlignment_Default_IsLeft()
    {
        var defaultVal = default(TextAlignment);
        Assert.Equal(TextAlignment.Left, defaultVal);
    }

    // ============== ImageSizing ==============

    [Fact]
    public void ImageSizing_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(ImageSizing)).Cast<ImageSizing>().ToList();
        
        Assert.Contains(ImageSizing.Stretch, values);
        Assert.Contains(ImageSizing.FitProportional, values);
        Assert.Contains(ImageSizing.Clip, values);
        Assert.Contains(ImageSizing.ActualSize, values);
        Assert.Equal(4, values.Count);
    }

    [Fact]
    public void ImageSizing_Default_IsFitProportional()
    {
        var defaultVal = default(ImageSizing);
        Assert.Equal(ImageSizing.Stretch, defaultVal);
    }

    // ============== LineDirection ==============

    [Fact]
    public void LineDirection_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(LineDirection)).Cast<LineDirection>().ToList();
        
        Assert.Contains(LineDirection.Horizontal, values);
        Assert.Contains(LineDirection.Vertical, values);
        Assert.Contains(LineDirection.Diagonal, values);
        Assert.Equal(3, values.Count);
    }

    [Fact]
    public void LineDirection_Default_IsHorizontal()
    {
        var defaultVal = default(LineDirection);
        Assert.Equal(LineDirection.Horizontal, defaultVal);
    }

    // ============== ShapeType ==============

    [Fact]
    public void ShapeType_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>().ToList();
        
        Assert.Contains(ShapeType.Rectangle, values);
        Assert.Contains(ShapeType.Ellipse, values);
        Assert.Contains(ShapeType.RoundedRect, values);
        Assert.Contains(ShapeType.Triangle, values);
        Assert.Equal(4, values.Count);
    }

    [Fact]
    public void ShapeType_Default_IsRectangle()
    {
        var defaultVal = default(ShapeType);
        Assert.Equal(ShapeType.Rectangle, defaultVal);
    }

    // ============== ChartType ==============

    [Fact]
    public void ChartType_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(ChartType)).Cast<ChartType>().ToList();
        
        Assert.Contains(ChartType.Bar, values);
        Assert.Contains(ChartType.Line, values);
        Assert.Contains(ChartType.Pie, values);
        Assert.Contains(ChartType.Area, values);
        Assert.Contains(ChartType.Scatter, values);
        Assert.Equal(5, values.Count);
    }

    [Fact]
    public void ChartType_Default_IsBar()
    {
        var defaultVal = default(ChartType);
        Assert.Equal(ChartType.Bar, defaultVal);
    }

    // ============== BarcodeFormat ==============

    [Fact]
    public void BarcodeFormat_HasExpectedValues()
    {
        var values = Enum.GetValues(typeof(BarcodeFormat)).Cast<BarcodeFormat>().ToList();
        
        Assert.Contains(BarcodeFormat.QRCode, values);
        Assert.Contains(BarcodeFormat.Code128, values);
        Assert.Contains(BarcodeFormat.Code39, values);
        Assert.Contains(BarcodeFormat.EAN13, values);
        Assert.Contains(BarcodeFormat.EAN8, values);
        Assert.Contains(BarcodeFormat.UPC_A, values);
        Assert.Contains(BarcodeFormat.DataMatrix, values);
        Assert.Contains(BarcodeFormat.PDF417, values);
        Assert.Equal(8, values.Count);
    }

    [Fact]
    public void BarcodeFormat_Default_IsCode128()
    {
        var defaultVal = default(BarcodeFormat);
        Assert.Equal(BarcodeFormat.Code128, defaultVal);
    }

    // ============== 枚举转换测试 ==============

    [Fact]
    public void BandType_CanParse_AllValues()
    {
        Assert.True(Enum.TryParse<BandType>("Header", out var v1) && v1 == BandType.Header);
        Assert.True(Enum.TryParse<BandType>("Footer", out var v2) && v2 == BandType.Footer);
        Assert.True(Enum.TryParse<BandType>("ReportHeader", out var v3) && v3 == BandType.ReportHeader);
        Assert.True(Enum.TryParse<BandType>("ReportFooter", out var v4) && v4 == BandType.ReportFooter);
        Assert.True(Enum.TryParse<BandType>("Detail", out var v5) && v5 == BandType.Detail);
        Assert.True(Enum.TryParse<BandType>("GroupHeader", out var v6) && v6 == BandType.GroupHeader);
        Assert.True(Enum.TryParse<BandType>("GroupFooter", out var v7) && v7 == BandType.GroupFooter);
    }

    [Fact]
    public void TextAlignment_CanParse_AllValues()
    {
        Assert.True(Enum.TryParse<TextAlignment>("Left", out var v1) && v1 == TextAlignment.Left);
        Assert.True(Enum.TryParse<TextAlignment>("Center", out var v2) && v2 == TextAlignment.Center);
        Assert.True(Enum.TryParse<TextAlignment>("Right", out var v3) && v3 == TextAlignment.Right);
        Assert.True(Enum.TryParse<TextAlignment>("Justify", out var v4) && v4 == TextAlignment.Justify);
    }

    [Fact]
    public void ShapeType_CanParse_AllValues()
    {
        Assert.True(Enum.TryParse<ShapeType>("Rectangle", out var v1) && v1 == ShapeType.Rectangle);
        Assert.True(Enum.TryParse<ShapeType>("Ellipse", out var v2) && v2 == ShapeType.Ellipse);
        Assert.True(Enum.TryParse<ShapeType>("RoundedRect", out var v3) && v3 == ShapeType.RoundedRect);
        Assert.True(Enum.TryParse<ShapeType>("Triangle", out var v4) && v4 == ShapeType.Triangle);
    }

    [Fact]
    public void ChartType_CanParse_AllValues()
    {
        Assert.True(Enum.TryParse<ChartType>("Bar", out var v1) && v1 == ChartType.Bar);
        Assert.True(Enum.TryParse<ChartType>("Line", out var v2) && v2 == ChartType.Line);
        Assert.True(Enum.TryParse<ChartType>("Pie", out var v3) && v3 == ChartType.Pie);
        Assert.True(Enum.TryParse<ChartType>("Area", out var v4) && v4 == ChartType.Area);
        Assert.True(Enum.TryParse<ChartType>("Scatter", out var v5) && v5 == ChartType.Scatter);
    }
}
