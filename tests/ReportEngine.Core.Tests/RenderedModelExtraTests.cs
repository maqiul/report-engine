using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedReport FitToWidth/Scale 测试
/// </summary>
public class RenderedReportScaling2Tests
{
    private static RenderedReport MakeReport(double pageW = 210, double pageH = 297)
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = pageW,
            PageHeight = pageH
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 20, Width = 50, Height = 10, Text = "Test" });
        report.Pages.Add(page);
        return report;
    }

    // ============== FitToWidth ==============

    [Fact]
    public void FitToWidth_ScaleDown_Works()
    {
        var report = MakeReport(210, 297);
        report.FitToWidth(105); // 50% scale

        Assert.Equal(105, report.PageWidth);
        Assert.Equal(148.5, report.PageHeight, 1);
    }

    [Fact]
    public void FitToWidth_ScaleDown_ElementsScaled()
    {
        var report = MakeReport(210, 297);
        report.FitToWidth(105);

        var el = report.Pages[0].Elements[0];
        Assert.Equal(5, el.X, 1);
        Assert.Equal(10, el.Y, 1);
        Assert.Equal(25, el.Width, 1);
        Assert.Equal(5, el.Height, 1);
    }

    [Fact]
    public void FitToWidth_NoScaleNeeded_NoChange()
    {
        var report = MakeReport(210, 297);
        report.FitToWidth(300); // target > current

        Assert.Equal(210, report.PageWidth);
        Assert.Equal(297, report.PageHeight);
    }

    [Fact]
    public void FitToWidth_SameWidth_NoChange()
    {
        var report = MakeReport(210, 297);
        report.FitToWidth(210);

        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void FitToWidth_ZeroTarget_NoChange()
    {
        var report = MakeReport(210, 297);
        report.FitToWidth(0);

        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void FitToWidth_NegativeTarget_NoChange()
    {
        var report = MakeReport(210, 297);
        report.FitToWidth(-100);

        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void FitToWidth_ZeroPageWidth_NoChange()
    {
        var report = new RenderedReport { PageWidth = 0, PageHeight = 297 };
        report.FitToWidth(100);

        Assert.Equal(0, report.PageWidth);
    }

    // ============== Scale ==============

    [Fact]
    public void Scale_Double_Works()
    {
        var report = MakeReport(210, 297);
        report.Scale(2.0);

        Assert.Equal(420, report.PageWidth);
        Assert.Equal(594, report.PageHeight);
    }

    [Fact]
    public void Scale_Double_ElementsScaled()
    {
        var report = MakeReport(210, 297);
        report.Scale(2.0);

        var el = report.Pages[0].Elements[0];
        Assert.Equal(20, el.X);
        Assert.Equal(40, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(20, el.Height);
    }

    [Fact]
    public void Scale_Half_Works()
    {
        var report = MakeReport(210, 297);
        report.Scale(0.5);

        Assert.Equal(105, report.PageWidth);
        Assert.Equal(148.5, report.PageHeight, 1);
    }

    [Fact]
    public void Scale_One_NoChange()
    {
        var report = MakeReport(210, 297);
        report.Scale(1.0);

        Assert.Equal(210, report.PageWidth);
        Assert.Equal(297, report.PageHeight);
    }

    [Fact]
    public void Scale_NearOne_NoChange()
    {
        var report = MakeReport(210, 297);
        report.Scale(1.0005); // within 0.001 tolerance

        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void Scale_Zero_NoChange()
    {
        var report = MakeReport(210, 297);
        report.Scale(0);

        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void Scale_Negative_NoChange()
    {
        var report = MakeReport(210, 297);
        report.Scale(-1);

        Assert.Equal(210, report.PageWidth);
    }

    // ============== 综合 ==============

    [Fact]
    public void FitToWidth_ThenScale_CombinedEffect()
    {
        var report = MakeReport(200, 300);
        report.FitToWidth(100); // 50% scale
        report.Scale(2.0); // 200% scale

        Assert.Equal(200, report.PageWidth);
        Assert.Equal(300, report.PageHeight);
    }

    [Fact]
    public void Scale_MultiplePages_AllScaled()
    {
        var report = MakeReport(100, 100);
        report.Pages.Add(new RenderedPage());
        report.Pages[1].Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 20, Height = 20 });

        report.Scale(3.0);

        Assert.Equal(300, report.PageWidth);
        Assert.Equal(30, report.Pages[0].Elements[0].X);
        Assert.Equal(30, report.Pages[1].Elements[0].X);
    }
}

/// <summary>
/// RenderedPage 属性测试
/// </summary>
public class RenderedPageExtraTests
{
    [Fact]
    public void PageNumber_DefaultIsZero()
    {
        var page = new RenderedPage();
        Assert.Equal(0, page.PageNumber);
    }

    [Fact]
    public void PageNumber_Set_Works()
    {
        var page = new RenderedPage { PageNumber = 3 };
        Assert.Equal(3, page.PageNumber);
    }

    [Fact]
    public void TotalPages_DefaultIsZero()
    {
        var page = new RenderedPage();
        Assert.Equal(0, page.TotalPages);
    }

    [Fact]
    public void TotalPages_Set_Works()
    {
        var page = new RenderedPage { TotalPages = 10 };
        Assert.Equal(10, page.TotalPages);
    }

    [Fact]
    public void Elements_EmptyByDefault()
    {
        var page = new RenderedPage();
        Assert.Empty(page.Elements);
    }

    [Fact]
    public void Elements_AddElement_Works()
    {
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { Text = "Test" });
        Assert.Single(page.Elements);
    }

    [Fact]
    public void Elements_AddMultiple_Works()
    {
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { Text = "A" });
        page.Elements.Add(new RenderedImageElement { Source = "img.png" });
        page.Elements.Add(new RenderedLineElement());
        Assert.Equal(3, page.Elements.Count);
    }
}

/// <summary>
/// RenderedElement 基类属性测试
/// </summary>
public class RenderedElementBase2Tests
{
    [Fact]
    public void Id_EmptyByDefault()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Id);
    }

    [Fact]
    public void Id_Set_Works()
    {
        var el = new RenderedTextElement { Id = "elem1" };
        Assert.Equal("elem1", el.Id);
    }

    [Fact]
    public void Position_DefaultIsZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.X);
        Assert.Equal(0, el.Y);
    }

    [Fact]
    public void Position_Set_Works()
    {
        var el = new RenderedTextElement { X = 15.5, Y = 25.3 };
        Assert.Equal(15.5, el.X);
        Assert.Equal(25.3, el.Y);
    }

    [Fact]
    public void Size_DefaultIsZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Width);
        Assert.Equal(0, el.Height);
    }

    [Fact]
    public void Size_Set_Works()
    {
        var el = new RenderedTextElement { Width = 100, Height = 50 };
        Assert.Equal(100, el.Width);
        Assert.Equal(50, el.Height);
    }

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_Set_Works()
    {
        var el = new RenderedTextElement { BackgroundColor = "#FF0000" };
        Assert.Equal("#FF0000", el.BackgroundColor);
    }

    [Fact]
    public void Border_NullByDefault()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void Border_Set_Works()
    {
        var el = new RenderedTextElement { Border = new BorderDef { Width = 2 } };
        Assert.NotNull(el.Border);
        Assert.Equal(2, el.Border.Width);
    }
}

/// <summary>
/// RenderedTextElement 属性测试
/// </summary>
public class RenderedTextElementExtraTests
{
    [Fact]
    public void Text_EmptyByDefault()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Text);
    }

    [Fact]
    public void Text_Set_Works()
    {
        var el = new RenderedTextElement { Text = "Hello World" };
        Assert.Equal("Hello World", el.Text);
    }

    [Fact]
    public void Font_DefaultIsNotNull()
    {
        var el = new RenderedTextElement();
        Assert.NotNull(el.Font);
    }

    [Fact]
    public void Alignment_DefaultIsLeft()
    {
        var el = new RenderedTextElement();
        Assert.Equal(TextAlignment.Left, el.Alignment);
    }

    [Fact]
    public void Alignment_SetCenter_Works()
    {
        var el = new RenderedTextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, el.Alignment);
    }

    [Fact]
    public void Alignment_SetRight_Works()
    {
        var el = new RenderedTextElement { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, el.Alignment);
    }

    [Fact]
    public void Hyperlink_NullByDefault()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_Set_Works()
    {
        var el = new RenderedTextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }
}

/// <summary>
/// RenderedImageElement 属性测试
/// </summary>
public class RenderedImageElementExtraTests
{
    [Fact]
    public void Source_EmptyByDefault()
    {
        var el = new RenderedImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void Source_Set_Works()
    {
        var el = new RenderedImageElement { Source = "logo.png" };
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void Source_SetUrl_Works()
    {
        var el = new RenderedImageElement { Source = "https://example.com/img.png" };
        Assert.Equal("https://example.com/img.png", el.Source);
    }
}

/// <summary>
/// RenderedLineElement 属性测试
/// </summary>
public class RenderedLineElementExtraTests
{
    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var el = new RenderedLineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var el = new RenderedLineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void LineWidth_DefaultIsZero()
    {
        var el = new RenderedLineElement();
        Assert.Equal(0, el.LineWidth);
    }

    [Fact]
    public void LineWidth_Set_Works()
    {
        var el = new RenderedLineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, el.LineWidth);
    }

    [Fact]
    public void LineColor_DefaultIsBlack()
    {
        var el = new RenderedLineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void LineColor_Set_Works()
    {
        var el = new RenderedLineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }
}

/// <summary>
/// RenderedShapeElement 属性测试
/// </summary>
public class RenderedShapeElementExtraTests
{
    [Fact]
    public void Shape_DefaultIsRectangle()
    {
        var el = new RenderedShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void Shape_SetEllipse_Works()
    {
        var el = new RenderedShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void BorderRadius_DefaultIsZero()
    {
        var el = new RenderedShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_Set_Works()
    {
        var el = new RenderedShapeElement { BorderRadius = 5 };
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void FillColor_DefaultIsWhite()
    {
        var el = new RenderedShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void FillColor_Set_Works()
    {
        var el = new RenderedShapeElement { FillColor = "#00FF00" };
        Assert.Equal("#00FF00", el.FillColor);
    }
}

/// <summary>
/// RenderedBarcodeElement 属性测试
/// </summary>
public class RenderedBarcodeElementExtraTests
{
    [Fact]
    public void Value_EmptyByDefault()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void Value_Set_Works()
    {
        var el = new RenderedBarcodeElement { Value = "123456789" };
        Assert.Equal("123456789", el.Value);
    }

    [Fact]
    public void Format_DefaultIsCode128()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void Format_SetQRCode_Works()
    {
        var el = new RenderedBarcodeElement { Format = BarcodeFormat.QRCode };
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void ForeColor_DefaultIsBlack()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void BackColor_DefaultIsWhite()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void ShowText_DefaultIsTrue()
    {
        var el = new RenderedBarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void ShowText_SetFalse_Works()
    {
        var el = new RenderedBarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }
}

/// <summary>
/// RenderedTableElement 属性测试
/// </summary>
public class RenderedTableElementExtraTests
{
    [Fact]
    public void RowCount_DefaultIsZero()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.RowCount);
    }

    [Fact]
    public void ColCount_DefaultIsZero()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.ColCount);
    }

    [Fact]
    public void ColumnWidths_EmptyByDefault()
    {
        var el = new RenderedTableElement();
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void RowHeights_EmptyByDefault()
    {
        var el = new RenderedTableElement();
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void Cells_EmptyByDefault()
    {
        var el = new RenderedTableElement();
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void BorderWidth_DefaultIsZero()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.BorderWidth);
    }

    [Fact]
    public void BorderColor_DefaultIsBlack()
    {
        var el = new RenderedTableElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var el = new RenderedTableElement
        {
            RowCount = 3,
            ColCount = 4,
            BorderWidth = 1,
            BorderColor = "#999999"
        };
        el.ColumnWidths.AddRange(new[] { 50.0, 60.0, 70.0, 80.0 });
        el.RowHeights.AddRange(new[] { 20.0, 25.0, 30.0 });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "A1" });

        Assert.Equal(3, el.RowCount);
        Assert.Equal(4, el.ColCount);
        Assert.Equal(4, el.ColumnWidths.Count);
        Assert.Equal(3, el.RowHeights.Count);
        Assert.Single(el.Cells);
    }
}

/// <summary>
/// RenderedTableCell 属性测试
/// </summary>
public class RenderedTableCellExtraTests
{
    [Fact]
    public void Row_DefaultIsZero()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(0, cell.Row);
    }

    [Fact]
    public void Col_DefaultIsZero()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(0, cell.Col);
    }

    [Fact]
    public void RowSpan_DefaultIs1()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(1, cell.RowSpan);
    }

    [Fact]
    public void ColSpan_DefaultIs1()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(1, cell.ColSpan);
    }

    [Fact]
    public void Text_EmptyByDefault()
    {
        var cell = new RenderedTableCell();
        Assert.Equal("", cell.Text);
    }

    [Fact]
    public void Font_DefaultIsNotNull()
    {
        var cell = new RenderedTableCell();
        Assert.NotNull(cell.Font);
    }

    [Fact]
    public void Alignment_DefaultIsCenter()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(TextAlignment.Center, cell.Alignment);
    }

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var cell = new RenderedTableCell();
        Assert.Null(cell.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_Set_Works()
    {
        var cell = new RenderedTableCell { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", cell.BackgroundColor);
    }

    [Fact]
    public void MergedCell_Works()
    {
        var cell = new RenderedTableCell { Row = 0, Col = 0, RowSpan = 2, ColSpan = 3, Text = "Merged" };
        Assert.Equal(2, cell.RowSpan);
        Assert.Equal(3, cell.ColSpan);
    }
}

/// <summary>
/// RenderedCrossTabElement 属性测试
/// </summary>
public class RenderedCrossTabElementExtraTests
{
    [Fact]
    public void RowCount_DefaultIsZero()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.RowCount);
    }

    [Fact]
    public void ColCount_DefaultIsZero()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.ColCount);
    }

    [Fact]
    public void ColumnWidths_EmptyByDefault()
    {
        var el = new RenderedCrossTabElement();
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void RowHeights_EmptyByDefault()
    {
        var el = new RenderedCrossTabElement();
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void Cells_EmptyByDefault()
    {
        var el = new RenderedCrossTabElement();
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void BorderWidth_DefaultIsZero()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.BorderWidth);
    }

    [Fact]
    public void BorderColor_DefaultIsBlack()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var el = new RenderedCrossTabElement
        {
            RowCount = 5,
            ColCount = 3,
            BorderWidth = 0.5
        };
        el.ColumnWidths.AddRange(new[] { 40.0, 50.0, 60.0 });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "Header" });

        Assert.Equal(5, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.Equal(3, el.ColumnWidths.Count);
        Assert.Single(el.Cells);
    }
}
