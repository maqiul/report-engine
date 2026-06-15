using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ImageElement 行为测试：
///   - 默认值
///   - Source 设置
///   - Sizing 模式
/// </summary>
public class ImageElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new ImageElement();

        Assert.Equal("", el.Source);
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    // ============== Source ==============

    [Fact]
    public void Source_EmptyByDefault()
    {
        var el = new ImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void Source_SetLocalPath_Works()
    {
        var el = new ImageElement { Source = @"C:\images\logo.png" };
        Assert.Equal(@"C:\images\logo.png", el.Source);
    }

    [Fact]
    public void Source_SetUrl_Works()
    {
        var el = new ImageElement { Source = "https://example.com/logo.png" };
        Assert.Equal("https://example.com/logo.png", el.Source);
    }

    [Fact]
    public void Source_SetBase64_Works()
    {
        var base64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        var el = new ImageElement { Source = base64 };
        Assert.StartsWith("data:image/png;base64,", el.Source);
    }

    [Fact]
    public void Source_SetRelativePath_Works()
    {
        var el = new ImageElement { Source = "./images/logo.png" };
        Assert.Equal("./images/logo.png", el.Source);
    }

    [Fact]
    public void Source_SetDataField_Works()
    {
        var el = new ImageElement { Source = "[LogoUrl]" };
        Assert.Equal("[LogoUrl]", el.Source);
    }

    [Fact]
    public void Source_CanBeChanged()
    {
        var el = new ImageElement { Source = "old.png" };
        el.Source = "new.png";
        Assert.Equal("new.png", el.Source);
    }

    // ============== Sizing ==============

    [Fact]
    public void Sizing_DefaultIsFitProportional()
    {
        var el = new ImageElement();
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    [Fact]
    public void Sizing_SetStretch_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.Stretch };
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    [Fact]
    public void Sizing_SetClip_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.Clip };
        Assert.Equal(ImageSizing.Clip, el.Sizing);
    }

    [Fact]
    public void Sizing_SetActualSize_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.ActualSize };
        Assert.Equal(ImageSizing.ActualSize, el.Sizing);
    }

    [Fact]
    public void Sizing_SetFitProportional_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.Stretch };
        el.Sizing = ImageSizing.FitProportional;
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    [Fact]
    public void Sizing_CanBeChanged()
    {
        var el = new ImageElement { Sizing = ImageSizing.FitProportional };
        el.Sizing = ImageSizing.Stretch;
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ImageElement_LogoImage_Works()
    {
        var el = new ImageElement
        {
            Source = @"C:\images\company-logo.png",
            Sizing = ImageSizing.FitProportional,
            X = 10,
            Y = 10,
            Width = 50,
            Height = 30
        };

        Assert.Equal(@"C:\images\company-logo.png", el.Source);
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
        Assert.Equal(10, el.X);
        Assert.Equal(50, el.Width);
    }

    [Fact]
    public void ImageElement_WebImage_Works()
    {
        var el = new ImageElement
        {
            Source = "https://example.com/photo.jpg",
            Sizing = ImageSizing.Stretch
        };

        Assert.StartsWith("https://", el.Source);
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    [Fact]
    public void ImageElement_Base64Image_Works()
    {
        var el = new ImageElement
        {
            Source = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD...",
            Sizing = ImageSizing.ActualSize
        };

        Assert.StartsWith("data:image/", el.Source);
    }

    [Fact]
    public void ImageElement_DynamicSource_Works()
    {
        var el = new ImageElement
        {
            Source = "[ProductImageUrl]",
            Sizing = ImageSizing.FitProportional
        };

        Assert.StartsWith("[", el.Source);
    }

    [Fact]
    public void ImageElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Header, Height = 50 };
        band.Elements.Add(new ImageElement
        {
            Source = "logo.png",
            X = 10,
            Y = 5,
            Width = 40,
            Height = 40,
            Sizing = ImageSizing.FitProportional
        });

        Assert.Single(band.Elements);
        var img = Assert.IsType<ImageElement>(band.Elements[0]);
        Assert.Equal("logo.png", img.Source);
    }

    [Fact]
    public void ImageElement_CanBeModified()
    {
        var el = new ImageElement { Source = "old.png", Sizing = ImageSizing.Stretch };
        
        el.Source = "new.png";
        el.Sizing = ImageSizing.Clip;
        
        Assert.Equal("new.png", el.Source);
        Assert.Equal(ImageSizing.Clip, el.Sizing);
    }
}
