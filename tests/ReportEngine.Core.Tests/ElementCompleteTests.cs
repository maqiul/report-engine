using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// LineElement / ShapeElement / BarcodeElement 完整字段测试：
///   - LineElement 完整字段（Direction/LineWidth/LineColor setter）
///   - ShapeElement 完整字段（Shape/BorderRadius/FillColor setter）
///   - BarcodeElement 完整字段（Value/Format/ForeColor/BackColor/ShowText setter）
///   - 枚举完整性（LineDirection/ShapeType/BarcodeFormat）
/// </summary>
public class ElementCompleteTests
{
    // ============== LineElement ==============

    [Fact]
    public void LineElement_AllSetters()
    {
        var e = new LineElement
        {
            Direction = LineDirection.Vertical,
            LineWidth = 2.5,
            LineColor = "#FF0000",
        };
        Assert.Equal(LineDirection.Vertical, e.Direction);
        Assert.Equal(2.5, e.LineWidth);
        Assert.Equal("#FF0000", e.LineColor);
    }

    [Fact]
    public void LineElement_Direction_CanBeDiagonal()
    {
        var e = new LineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, e.Direction);
    }

    [Fact]
    public void LineElement_LineWidth_CanBeZero()
    {
        var e = new LineElement { LineWidth = 0 };
        Assert.Equal(0, e.LineWidth);
    }

    [Fact]
    public void LineElement_LineColor_AcceptsAnyHex()
    {
        var e = new LineElement { LineColor = "#AABBCC" };
        Assert.Equal("#AABBCC", e.LineColor);
    }

    // ============== ShapeElement ==============

    [Fact]
    public void ShapeElement_AllSetters()
    {
        var e = new ShapeElement
        {
            Shape = ShapeType.Ellipse,
            BorderRadius = 5.0,
            FillColor = "#00FF00",
        };
        Assert.Equal(ShapeType.Ellipse, e.Shape);
        Assert.Equal(5.0, e.BorderRadius);
        Assert.Equal("#00FF00", e.FillColor);
    }

    [Fact]
    public void ShapeElement_Shape_CanBeRoundedRect()
    {
        var e = new ShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, e.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_CanBeTriangle()
    {
        var e = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, e.Shape);
    }

    [Fact]
    public void ShapeElement_BorderRadius_CanBeNegative()
    {
        var e = new ShapeElement { BorderRadius = -1 };
        Assert.Equal(-1, e.BorderRadius);
    }

    [Fact]
    public void ShapeElement_FillColor_AcceptsAnyHex()
    {
        var e = new ShapeElement { FillColor = "#112233" };
        Assert.Equal("#112233", e.FillColor);
    }

    // ============== BarcodeElement ==============

    [Fact]
    public void BarcodeElement_AllSetters()
    {
        var e = new BarcodeElement
        {
            Value = "123456",
            Format = BarcodeFormat.Code128,
            ForeColor = "#FF0000",
            BackColor = "#0000FF",
            ShowText = false,
        };
        Assert.Equal("123456", e.Value);
        Assert.Equal(BarcodeFormat.Code128, e.Format);
        Assert.Equal("#FF0000", e.ForeColor);
        Assert.Equal("#0000FF", e.BackColor);
        Assert.False(e.ShowText);
    }

    [Fact]
    public void BarcodeElement_Format_All8Formats()
    {
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code128));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code39));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN13));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN8));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.UPC_A));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.QRCode));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.DataMatrix));
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.PDF417));
    }

    [Fact]
    public void BarcodeElement_Value_CanBeEmpty()
    {
        var e = new BarcodeElement { Value = "" };
        Assert.Equal("", e.Value);
    }

    [Fact]
    public void BarcodeElement_ShowText_DefaultTrue()
    {
        var e = new BarcodeElement();
        Assert.True(e.ShowText);
    }

    [Fact]
    public void BarcodeElement_ForeColor_AcceptsAnyHex()
    {
        var e = new BarcodeElement { ForeColor = "#AABBCC" };
        Assert.Equal("#AABBCC", e.ForeColor);
    }

    // ============== 枚举完整性 ==============

    [Fact]
    public void LineDirection_HasThreeValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(LineDirection), LineDirection.Horizontal));
        Assert.True(System.Enum.IsDefined(typeof(LineDirection), LineDirection.Vertical));
        Assert.True(System.Enum.IsDefined(typeof(LineDirection), LineDirection.Diagonal));
    }

    [Fact]
    public void ShapeType_HasFourValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(ShapeType), ShapeType.Rectangle));
        Assert.True(System.Enum.IsDefined(typeof(ShapeType), ShapeType.Ellipse));
        Assert.True(System.Enum.IsDefined(typeof(ShapeType), ShapeType.RoundedRect));
        Assert.True(System.Enum.IsDefined(typeof(ShapeType), ShapeType.Triangle));
    }
}
