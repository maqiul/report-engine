using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Margin 高级属性测试
/// </summary>
public class MarginAdvancedTests
{
    // ============== Top ==============

    [Fact]
    public void Top_DefaultIs10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Top);
    }

    [Fact]
    public void Top_Set_Works()
    {
        var m = new Margin { Top = 20 };
        Assert.Equal(20, m.Top);
    }

    [Fact]
    public void Top_SetZero_Works()
    {
        var m = new Margin { Top = 0 };
        Assert.Equal(0, m.Top);
    }

    // ============== Bottom ==============

    [Fact]
    public void Bottom_DefaultIs10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Bottom);
    }

    [Fact]
    public void Bottom_Set_Works()
    {
        var m = new Margin { Bottom = 15 };
        Assert.Equal(15, m.Bottom);
    }

    // ============== Left ==============

    [Fact]
    public void Left_DefaultIs10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Left);
    }

    [Fact]
    public void Left_Set_Works()
    {
        var m = new Margin { Left = 25 };
        Assert.Equal(25, m.Left);
    }

    // ============== Right ==============

    [Fact]
    public void Right_DefaultIs10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Right);
    }

    [Fact]
    public void Right_Set_Works()
    {
        var m = new Margin { Right = 5 };
        Assert.Equal(5, m.Right);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void Margin_UniformMargins_Works()
    {
        var m = new Margin { Top = 15, Bottom = 15, Left = 15, Right = 15 };
        Assert.Equal(15, m.Top);
        Assert.Equal(15, m.Bottom);
        Assert.Equal(15, m.Left);
        Assert.Equal(15, m.Right);
    }

    [Fact]
    public void Margin_AsymmetricMargins_Works()
    {
        var m = new Margin { Top = 20, Bottom = 10, Left = 15, Right = 25 };
        Assert.Equal(20, m.Top);
        Assert.Equal(10, m.Bottom);
        Assert.Equal(15, m.Left);
        Assert.Equal(25, m.Right);
    }

    [Fact]
    public void Margin_ZeroMargins_Works()
    {
        var m = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 };
        Assert.Equal(0, m.Top);
        Assert.Equal(0, m.Bottom);
        Assert.Equal(0, m.Left);
        Assert.Equal(0, m.Right);
    }

    [Fact]
    public void Margin_CanBeModified()
    {
        var m = new Margin();
        m.Top = 30;
        m.Bottom = 30;
        m.Left = 20;
        m.Right = 20;

        Assert.Equal(30, m.Top);
        Assert.Equal(30, m.Bottom);
        Assert.Equal(20, m.Left);
        Assert.Equal(20, m.Right);
    }
}

/// <summary>
/// Enum 值完整性测试
/// </summary>
public class EnumCompletenessTests
{
    // ============== BandType ==============

    [Fact]
    public void BandType_Has7Values()
    {
        var values = Enum.GetValues(typeof(BandType));
        Assert.Equal(7, values.Length);
    }

    [Fact]
    public void BandType_ContainsHeader()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.Header));
    }

    [Fact]
    public void BandType_ContainsFooter()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.Footer));
    }

    [Fact]
    public void BandType_ContainsDetail()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.Detail));
    }

    [Fact]
    public void BandType_ContainsReportHeader()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.ReportHeader));
    }

    [Fact]
    public void BandType_ContainsReportFooter()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.ReportFooter));
    }

    [Fact]
    public void BandType_ContainsGroupHeader()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.GroupHeader));
    }

    [Fact]
    public void BandType_ContainsGroupFooter()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.GroupFooter));
    }

    // ============== ChartType ==============

    [Fact]
    public void ChartType_Has5Values()
    {
        var values = Enum.GetValues(typeof(ChartType));
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void ChartType_ContainsBar()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Bar));
    }

    [Fact]
    public void ChartType_ContainsLine()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Line));
    }

    [Fact]
    public void ChartType_ContainsPie()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Pie));
    }

    [Fact]
    public void ChartType_ContainsArea()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Area));
    }

    [Fact]
    public void ChartType_ContainsScatter()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Scatter));
    }

    // ============== BarcodeFormat ==============

    [Fact]
    public void BarcodeFormat_Has8Values()
    {
        var values = Enum.GetValues(typeof(BarcodeFormat));
        Assert.Equal(8, values.Length);
    }

    [Fact]
    public void BarcodeFormat_ContainsCode128()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code128));
    }

    [Fact]
    public void BarcodeFormat_ContainsCode39()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code39));
    }

    [Fact]
    public void BarcodeFormat_ContainsEAN13()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN13));
    }

    [Fact]
    public void BarcodeFormat_ContainsQRCode()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.QRCode));
    }

    [Fact]
    public void BarcodeFormat_ContainsDataMatrix()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.DataMatrix));
    }

    [Fact]
    public void BarcodeFormat_ContainsPDF417()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.PDF417));
    }

    // ============== TextAlignment ==============

    [Fact]
    public void TextAlignment_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextAlignment));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void TextAlignment_ContainsLeft()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Left));
    }

    [Fact]
    public void TextAlignment_ContainsCenter()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Center));
    }

    [Fact]
    public void TextAlignment_ContainsRight()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Right));
    }

    [Fact]
    public void TextAlignment_ContainsJustify()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Justify));
    }

    // ============== TextBoxType ==============

    [Fact]
    public void TextBoxType_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextBoxType));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void TextBoxType_ContainsStatic()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.Static));
    }

    [Fact]
    public void TextBoxType_ContainsField()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.Field));
    }

    [Fact]
    public void TextBoxType_ContainsSummary()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.Summary));
    }

    [Fact]
    public void TextBoxType_ContainsSysVar()
    {
        Assert.True(Enum.IsDefined(typeof(TextBoxType), TextBoxType.SysVar));
    }

    // ============== BorderStyle ==============

    [Fact]
    public void BorderStyle_Has4Values()
    {
        var values = Enum.GetValues(typeof(BorderStyle));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void BorderStyle_ContainsSolid()
    {
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.Solid));
    }

    [Fact]
    public void BorderStyle_ContainsDashed()
    {
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.Dashed));
    }

    [Fact]
    public void BorderStyle_ContainsDotted()
    {
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.Dotted));
    }

    [Fact]
    public void BorderStyle_ContainsNone()
    {
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.None));
    }

    // ============== ImageSizing ==============

    [Fact]
    public void ImageSizing_Has4Values()
    {
        var values = Enum.GetValues(typeof(ImageSizing));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ImageSizing_ContainsStretch()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.Stretch));
    }

    [Fact]
    public void ImageSizing_ContainsFitProportional()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.FitProportional));
    }

    [Fact]
    public void ImageSizing_ContainsClip()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.Clip));
    }

    [Fact]
    public void ImageSizing_ContainsActualSize()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.ActualSize));
    }

    // ============== LineDirection ==============

    [Fact]
    public void LineDirection_Has3Values()
    {
        var values = Enum.GetValues(typeof(LineDirection));
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void LineDirection_ContainsHorizontal()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Horizontal));
    }

    [Fact]
    public void LineDirection_ContainsVertical()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Vertical));
    }

    [Fact]
    public void LineDirection_ContainsDiagonal()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Diagonal));
    }

    // ============== ShapeType ==============

    [Fact]
    public void ShapeType_Has4Values()
    {
        var values = Enum.GetValues(typeof(ShapeType));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ShapeType_ContainsRectangle()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Rectangle));
    }

    [Fact]
    public void ShapeType_ContainsEllipse()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Ellipse));
    }

    [Fact]
    public void ShapeType_ContainsRoundedRect()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.RoundedRect));
    }

    [Fact]
    public void ShapeType_ContainsTriangle()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Triangle));
    }
}

/// <summary>
/// PageInfo 扩展属性测试（BackgroundColor/BackgroundImage/Watermark/Unit/MultiUp）
/// </summary>
public class PageInfoExtraTests
{
    // ============== Unit ==============

    [Fact]
    public void Unit_DefaultIsMm()
    {
        var page = new PageInfo();
        Assert.Equal("mm", page.Unit);
    }

    [Fact]
    public void Unit_SetCm_Works()
    {
        var page = new PageInfo { Unit = "cm" };
        Assert.Equal("cm", page.Unit);
    }

    [Fact]
    public void Unit_SetInch_Works()
    {
        var page = new PageInfo { Unit = "inch" };
        Assert.Equal("inch", page.Unit);
    }

    // ============== BackgroundColor ==============

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_Set_Works()
    {
        var page = new PageInfo { BackgroundColor = "#F5F5F5" };
        Assert.Equal("#F5F5F5", page.BackgroundColor);
    }

    // ============== BackgroundImage ==============

    [Fact]
    public void BackgroundImage_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.BackgroundImage);
    }

    [Fact]
    public void BackgroundImage_Set_Works()
    {
        var page = new PageInfo { BackgroundImage = "bg.png" };
        Assert.Equal("bg.png", page.BackgroundImage);
    }

    // ============== Watermark ==============

    [Fact]
    public void Watermark_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.Watermark);
    }

    [Fact]
    public void Watermark_Set_Works()
    {
        var page = new PageInfo { Watermark = "CONFIDENTIAL" };
        Assert.Equal("CONFIDENTIAL", page.Watermark);
    }

    [Fact]
    public void Watermark_SetChinese_Works()
    {
        var page = new PageInfo { Watermark = "机密文件" };
        Assert.Equal("机密文件", page.Watermark);
    }

    // ============== MultiUp ==============

    [Fact]
    public void MultiUp_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.MultiUp);
    }

    [Fact]
    public void MultiUp_Set_Works()
    {
        var page = new PageInfo
        {
            MultiUp = new MultiUpConfig { Rows = 2, Columns = 2 }
        };
        Assert.NotNull(page.MultiUp);
        Assert.Equal(2, page.MultiUp.Rows);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void PageInfo_WithWatermark_Works()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            Watermark = "DRAFT",
            BackgroundColor = "#FFFFFF"
        };

        Assert.Equal("DRAFT", page.Watermark);
        Assert.Equal("#FFFFFF", page.BackgroundColor);
    }

    [Fact]
    public void PageInfo_WithBackgroundImage_Works()
    {
        var page = new PageInfo
        {
            BackgroundImage = "letterhead.png",
            Watermark = null
        };

        Assert.Equal("letterhead.png", page.BackgroundImage);
        Assert.Null(page.Watermark);
    }

    [Fact]
    public void PageInfo_WithMultiUp_Works()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            MultiUp = new MultiUpConfig
            {
                Rows = 2,
                Columns = 2,
                Direction = "Horizontal"
            }
        };

        Assert.NotNull(page.MultiUp);
        Assert.Equal(2, page.MultiUp.Rows);
        Assert.Equal(2, page.MultiUp.Columns);
    }
}

/// <summary>
/// Band 额外属性测试（DataSource/RepeatOnNewPage/MultiColumn/SubBands）
/// </summary>
public class BandExtraTests
{
    [Fact]
    public void DataSource_SetNull_Works()
    {
        var band = new Band { DataSource = null };
        Assert.Null(band.DataSource);
    }

    [Fact]
    public void RepeatOnNewPage_True_Works()
    {
        var band = new Band { RepeatOnNewPage = true };
        Assert.True(band.RepeatOnNewPage);
    }

    [Fact]
    public void MultiColumn_Set_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            MultiColumn = new MultiColumnConfig { ColumnCount = 3 }
        };
        Assert.NotNull(band.MultiColumn);
        Assert.Equal(3, band.MultiColumn.ColumnCount);
    }

    [Fact]
    public void SubBands_AddNested_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            SubBands = new List<Band>
            {
                new Band { Type = BandType.Header, Height = 10 },
                new Band { Type = BandType.Header, Height = 10 }
            }
        };
        Assert.Equal(2, band.SubBands!.Count);
    }

    [Fact]
    public void SubBands_NestedBandsHaveElements()
    {
        var parent = new Band
        {
            Type = BandType.GroupHeader,
            SubBands = new List<Band>
            {
                new Band
                {
                    Type = BandType.Header,
                    Height = 15,
                    Elements = { new TextElement { Text = "Sub Header" } }
                }
            }
        };

        Assert.Single(parent.SubBands![0].Elements);
    }

    [Fact]
    public void Band_DetailWithDataSource_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            DataSource = "orderItems",
            Height = 20,
            RepeatOnNewPage = true
        };

        Assert.Equal("orderItems", band.DataSource);
        Assert.True(band.RepeatOnNewPage);
    }

    [Fact]
    public void Band_MultiColumnDetail_Works()
    {
        var band = new Band
        {
            Type = BandType.Detail,
            MultiColumn = new MultiColumnConfig
            {
                ColumnCount = 2,
                ColumnSpacing = 5,
                Direction = "Horizontal"
            }
        };

        Assert.NotNull(band.MultiColumn);
        Assert.Equal(2, band.MultiColumn.ColumnCount);
    }
}
