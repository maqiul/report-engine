using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ImageElement 完整字段测试：
///   - ImageElement 完整字段（Source/Sizing）
///   - 字段组合行为
/// </summary>
public class ImageElementCompleteTests
{
    [Fact]
    public void ImageElement_Defaults()
    {
        var i = new ImageElement();
        Assert.Equal("", i.Source);
        Assert.Equal(ImageSizing.FitProportional, i.Sizing);
    }

    [Fact]
    public void ImageElement_AllSetters()
    {
        var i = new ImageElement
        {
            Source = "logo.png",
            Sizing = ImageSizing.Stretch,
        };
        Assert.Equal("logo.png", i.Source);
        Assert.Equal(ImageSizing.Stretch, i.Sizing);
    }

    [Fact]
    public void ImageElement_Source_CanBeEmpty()
    {
        var i = new ImageElement { Source = "" };
        Assert.Equal("", i.Source);
    }

    [Fact]
    public void ImageElement_Source_CanBeFilePath()
    {
        var i = new ImageElement { Source = @"C:\images\logo.png" };
        Assert.Equal(@"C:\images\logo.png", i.Source);
    }

    [Fact]
    public void ImageElement_Source_CanBeUrl()
    {
        var i = new ImageElement { Source = "https://example.com/logo.png" };
        Assert.Equal("https://example.com/logo.png", i.Source);
    }

    [Fact]
    public void ImageElement_Source_CanBeBase64()
    {
        var i = new ImageElement { Source = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==" };
        Assert.StartsWith("data:image/", i.Source);
    }

    [Fact]
    public void ImageElement_Source_CanBeExpression()
    {
        var i = new ImageElement { Source = "{{currentRow.photo}}" };
        Assert.Equal("{{currentRow.photo}}", i.Source);
    }

    [Fact]
    public void ImageElement_Sizing_DefaultFitProportional()
    {
        var i = new ImageElement();
        Assert.Equal(ImageSizing.FitProportional, i.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_CanBeStretch()
    {
        var i = new ImageElement { Sizing = ImageSizing.Stretch };
        Assert.Equal(ImageSizing.Stretch, i.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_CanBeClip()
    {
        var i = new ImageElement { Sizing = ImageSizing.Clip };
        Assert.Equal(ImageSizing.Clip, i.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_CanBeActualSize()
    {
        var i = new ImageElement { Sizing = ImageSizing.ActualSize };
        Assert.Equal(ImageSizing.ActualSize, i.Sizing);
    }

    [Fact]
    public void ImageElement_FullCombination()
    {
        var i = new ImageElement
        {
            Source = "photo.jpg",
            Sizing = ImageSizing.Clip,
        };
        Assert.Equal("photo.jpg", i.Source);
        Assert.Equal(ImageSizing.Clip, i.Sizing);
    }
}
