using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportElement 基类属性测试 (GroupId/Locked/Rotation/Opacity/VisibleExpression/ConditionalFormats)
/// </summary>
public class ReportElementBaseExtraTests
{
    // ============== GroupId ==============

    [Fact]
    public void GroupId_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.GroupId);
    }

    [Fact]
    public void GroupId_Set_Works()
    {
        var el = new TextElement { GroupId = "grp1" };
        Assert.Equal("grp1", el.GroupId);
    }

    [Fact]
    public void GroupId_CanBeCleared()
    {
        var el = new TextElement { GroupId = "grp1" };
        el.GroupId = null;
        Assert.Null(el.GroupId);
    }

    // ============== Locked ==============

    [Fact]
    public void Locked_FalseByDefault()
    {
        var el = new TextElement();
        Assert.False(el.Locked);
    }

    [Fact]
    public void Locked_SetTrue_Works()
    {
        var el = new TextElement { Locked = true };
        Assert.True(el.Locked);
    }

    [Fact]
    public void Locked_SetFalse_Works()
    {
        var el = new TextElement { Locked = true };
        el.Locked = false;
        Assert.False(el.Locked);
    }

    // ============== Rotation ==============

    [Fact]
    public void Rotation_ZeroByDefault()
    {
        var el = new TextElement();
        Assert.Equal(0, el.Rotation);
    }

    [Fact]
    public void Rotation_Set90_Works()
    {
        var el = new TextElement { Rotation = 90 };
        Assert.Equal(90, el.Rotation);
    }

    [Fact]
    public void Rotation_Set180_Works()
    {
        var el = new TextElement { Rotation = 180 };
        Assert.Equal(180, el.Rotation);
    }

    [Fact]
    public void Rotation_Set270_Works()
    {
        var el = new TextElement { Rotation = 270 };
        Assert.Equal(270, el.Rotation);
    }

    [Fact]
    public void Rotation_SetDecimal_Works()
    {
        var el = new TextElement { Rotation = 45.5 };
        Assert.Equal(45.5, el.Rotation);
    }

    // ============== Opacity ==============

    [Fact]
    public void Opacity_DefaultIs1()
    {
        var el = new TextElement();
        Assert.Equal(1.0, el.Opacity);
    }

    [Fact]
    public void Opacity_SetHalf_Works()
    {
        var el = new TextElement { Opacity = 0.5 };
        Assert.Equal(0.5, el.Opacity);
    }

    [Fact]
    public void Opacity_SetZero_Works()
    {
        var el = new TextElement { Opacity = 0 };
        Assert.Equal(0, el.Opacity);
    }

    [Fact]
    public void Opacity_SetOne_Works()
    {
        var el = new TextElement { Opacity = 0.3 };
        el.Opacity = 1.0;
        Assert.Equal(1.0, el.Opacity);
    }

    // ============== VisibleExpression ==============

    [Fact]
    public void VisibleExpression_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.VisibleExpression);
    }

    [Fact]
    public void VisibleExpression_Set_Works()
    {
        var el = new TextElement { VisibleExpression = "[ShowDetail] = true" };
        Assert.Equal("[ShowDetail] = true", el.VisibleExpression);
    }

    [Fact]
    public void VisibleExpression_CanBeCleared()
    {
        var el = new TextElement { VisibleExpression = "[x] > 0" };
        el.VisibleExpression = null;
        Assert.Null(el.VisibleExpression);
    }

    // ============== ConditionalFormats ==============

    [Fact]
    public void ConditionalFormats_EmptyByDefault()
    {
        var el = new TextElement();
        Assert.Empty(el.ConditionalFormats);
    }

    [Fact]
    public void ConditionalFormats_AddRule_Works()
    {
        var el = new TextElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[Amount] > 1000", BackgroundColor = "#FF0000" });
        Assert.Single(el.ConditionalFormats);
    }

    [Fact]
    public void ConditionalFormats_AddMultiple_Works()
    {
        var el = new TextElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[x] > 0" });
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[x] < 0" });
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[x] = 0" });
        Assert.Equal(3, el.ConditionalFormats.Count);
    }

    [Fact]
    public void ConditionalFormats_Clear_Works()
    {
        var el = new TextElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule());
        el.ConditionalFormats.Clear();
        Assert.Empty(el.ConditionalFormats);
    }

    // ============== 跨类型继承 ==============

    [Fact]
    public void ImageElement_InheritsBaseProperties()
    {
        var el = new ImageElement { Locked = true, Rotation = 45, Opacity = 0.8 };
        Assert.True(el.Locked);
        Assert.Equal(45, el.Rotation);
        Assert.Equal(0.8, el.Opacity);
    }

    [Fact]
    public void LineElement_InheritsBaseProperties()
    {
        var el = new LineElement { GroupId = "g1", Locked = true };
        Assert.Equal("g1", el.GroupId);
        Assert.True(el.Locked);
    }

    [Fact]
    public void ShapeElement_InheritsBaseProperties()
    {
        var el = new ShapeElement { Rotation = 30, Opacity = 0.7 };
        Assert.Equal(30, el.Rotation);
        Assert.Equal(0.7, el.Opacity);
    }

    [Fact]
    public void BarcodeElement_InheritsBaseProperties()
    {
        var el = new BarcodeElement { Locked = true, GroupId = "bc1" };
        Assert.True(el.Locked);
        Assert.Equal("bc1", el.GroupId);
    }

    [Fact]
    public void ChartElement_InheritsBaseProperties()
    {
        var el = new ChartElement { Rotation = 15, VisibleExpression = "[ShowChart]" };
        Assert.Equal(15, el.Rotation);
        Assert.Equal("[ShowChart]", el.VisibleExpression);
    }

    [Fact]
    public void TableElement_InheritsBaseProperties()
    {
        var el = new TableElement { Locked = true, Opacity = 0.9 };
        Assert.True(el.Locked);
        Assert.Equal(0.9, el.Opacity);
    }

    [Fact]
    public void CrossTabElement_InheritsBaseProperties()
    {
        var el = new CrossTabElement { GroupId = "ct1", ConditionalFormats = { new ConditionalFormatRule() } };
        Assert.Equal("ct1", el.GroupId);
        Assert.Single(el.ConditionalFormats);
    }

    [Fact]
    public void SubReportElement_InheritsBaseProperties()
    {
        var el = new SubReportElement { Locked = true, Rotation = 0, Opacity = 1.0 };
        Assert.True(el.Locked);
        Assert.Equal(0, el.Rotation);
        Assert.Equal(1.0, el.Opacity);
    }
}

/// <summary>
/// TextElement 额外属性测试 (SummaryField/SystemVariable/Format/CanGrow/CanShrink/MaxLines/Hyperlink)
/// </summary>
public class TextElementExtraTests
{
    // ============== SummaryField ==============

    [Fact]
    public void SummaryField_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryField);
    }

    [Fact]
    public void SummaryField_Set_Works()
    {
        var el = new TextElement { SummaryField = "Amount" };
        Assert.Equal("Amount", el.SummaryField);
    }

    // ============== SystemVariable ==============

    [Fact]
    public void SystemVariable_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_PageNumber_Works()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal("PageNumber", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_TotalPages_Works()
    {
        var el = new TextElement { SystemVariable = "TotalPages" };
        Assert.Equal("TotalPages", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_PrintDate_Works()
    {
        var el = new TextElement { SystemVariable = "PrintDate" };
        Assert.Equal("PrintDate", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_PrintTime_Works()
    {
        var el = new TextElement { SystemVariable = "PrintTime" };
        Assert.Equal("PrintTime", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_ReportTitle_Works()
    {
        var el = new TextElement { SystemVariable = "ReportTitle" };
        Assert.Equal("ReportTitle", el.SystemVariable);
    }

    // ============== Format ==============

    [Fact]
    public void Format_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.Format);
    }

    [Fact]
    public void Format_Currency_Works()
    {
        var el = new TextElement { Format = "currency" };
        Assert.Equal("currency", el.Format);
    }

    [Fact]
    public void Format_Date_Works()
    {
        var el = new TextElement { Format = "date" };
        Assert.Equal("date", el.Format);
    }

    [Fact]
    public void Format_Percent_Works()
    {
        var el = new TextElement { Format = "percent" };
        Assert.Equal("percent", el.Format);
    }

    [Fact]
    public void Format_NumberWithDecimals_Works()
    {
        var el = new TextElement { Format = "number:2" };
        Assert.Equal("number:2", el.Format);
    }

    // ============== CanGrow ==============

    [Fact]
    public void CanGrow_FalseByDefault()
    {
        var el = new TextElement();
        Assert.False(el.CanGrow);
    }

    [Fact]
    public void CanGrow_SetTrue_Works()
    {
        var el = new TextElement { CanGrow = true };
        Assert.True(el.CanGrow);
    }

    // ============== CanShrink ==============

    [Fact]
    public void CanShrink_FalseByDefault()
    {
        var el = new TextElement();
        Assert.False(el.CanShrink);
    }

    [Fact]
    public void CanShrink_SetTrue_Works()
    {
        var el = new TextElement { CanShrink = true };
        Assert.True(el.CanShrink);
    }

    // ============== MaxLines ==============

    [Fact]
    public void MaxLines_ZeroByDefault()
    {
        var el = new TextElement();
        Assert.Equal(0, el.MaxLines);
    }

    [Fact]
    public void MaxLines_Set_Works()
    {
        var el = new TextElement { MaxLines = 3 };
        Assert.Equal(3, el.MaxLines);
    }

    [Fact]
    public void MaxLines_SetLarge_Works()
    {
        var el = new TextElement { MaxLines = 100 };
        Assert.Equal(100, el.MaxLines);
    }

    // ============== Hyperlink ==============

    [Fact]
    public void Hyperlink_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_SetUrl_Works()
    {
        var el = new TextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_SetExpression_Works()
    {
        var el = new TextElement { Hyperlink = "='https://example.com/' + [Id]" };
        Assert.Equal("='https://example.com/' + [Id]", el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_CanBeCleared()
    {
        var el = new TextElement { Hyperlink = "https://example.com" };
        el.Hyperlink = null;
        Assert.Null(el.Hyperlink);
    }

    // ============== BoxType 综合 ==============

    [Fact]
    public void BoxType_WithSystemVariable_ReturnsSysVar()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void BoxType_WithSummaryFunction_ReturnsSummary()
    {
        var el = new TextElement { SummaryFunction = "Sum", SummaryField = "Amount" };
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    [Fact]
    public void BoxType_WithDataField_ReturnsField()
    {
        var el = new TextElement { DataField = "CustomerName" };
        Assert.Equal(TextBoxType.Field, el.BoxType);
    }

    [Fact]
    public void BoxType_WithOnlyText_ReturnsStatic()
    {
        var el = new TextElement { Text = "Hello" };
        Assert.Equal(TextBoxType.Static, el.BoxType);
    }

    [Fact]
    public void BoxType_Priority_SysVarOverSummary()
    {
        var el = new TextElement { SystemVariable = "PageNumber", SummaryFunction = "Sum" };
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void BoxType_Priority_SummaryOverField()
    {
        var el = new TextElement { SummaryFunction = "Sum", DataField = "Amount" };
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    [Fact]
    public void BoxType_Priority_FieldOverStatic()
    {
        var el = new TextElement { DataField = "Name", Text = "static" };
        Assert.Equal(TextBoxType.Field, el.BoxType);
    }
}

/// <summary>
/// DataSourceDef 额外属性测试 (ConnectionString/Query)
/// </summary>
public class DataSourceDefExtraTests
{
    [Fact]
    public void ConnectionString_NullByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.ConnectionString);
    }

    [Fact]
    public void ConnectionString_Set_Works()
    {
        var ds = new DataSourceDef { ConnectionString = "Server=localhost;Database=test" };
        Assert.Equal("Server=localhost;Database=test", ds.ConnectionString);
    }

    [Fact]
    public void Query_NullByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.Query);
    }

    [Fact]
    public void Query_SetSql_Works()
    {
        var ds = new DataSourceDef { Query = "SELECT * FROM orders" };
        Assert.Equal("SELECT * FROM orders", ds.Query);
    }

    [Fact]
    public void Query_SetJsonPath_Works()
    {
        var ds = new DataSourceDef { Query = "$.data.items" };
        Assert.Equal("$.data.items", ds.Query);
    }

    [Fact]
    public void Type_SupportsJson()
    {
        var ds = new DataSourceDef { Type = "json" };
        Assert.Equal("json", ds.Type);
    }

    [Fact]
    public void Type_SupportsSql()
    {
        var ds = new DataSourceDef { Type = "sql" };
        Assert.Equal("sql", ds.Type);
    }

    [Fact]
    public void Type_SupportsCsv()
    {
        var ds = new DataSourceDef { Type = "csv" };
        Assert.Equal("csv", ds.Type);
    }

    [Fact]
    public void Type_SupportsApi()
    {
        var ds = new DataSourceDef { Type = "api" };
        Assert.Equal("api", ds.Type);
    }
}

/// <summary>
/// ConditionalFormatRule 完整属性测试
/// </summary>
public class ConditionalFormatRuleExtraTests
{
    [Fact]
    public void Expression_EmptyByDefault()
    {
        var r = new ConditionalFormatRule();
        Assert.Equal("", r.Expression);
    }

    [Fact]
    public void Expression_Set_Works()
    {
        var r = new ConditionalFormatRule { Expression = "[Amount] > 1000" };
        Assert.Equal("[Amount] > 1000", r.Expression);
    }

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_Set_Works()
    {
        var r = new ConditionalFormatRule { BackgroundColor = "#FF0000" };
        Assert.Equal("#FF0000", r.BackgroundColor);
    }

    [Fact]
    public void FontColor_NullByDefault()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.FontColor);
    }

    [Fact]
    public void FontColor_Set_Works()
    {
        var r = new ConditionalFormatRule { FontColor = "#00FF00" };
        Assert.Equal("#00FF00", r.FontColor);
    }

    [Fact]
    public void Bold_FalseByDefault()
    {
        var r = new ConditionalFormatRule();
        Assert.False(r.Bold);
    }

    [Fact]
    public void Bold_SetTrue_Works()
    {
        var r = new ConditionalFormatRule { Bold = true };
        Assert.True(r.Bold);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var r = new ConditionalFormatRule
        {
            Expression = "[Score] >= 90",
            BackgroundColor = "#00FF00",
            FontColor = "#000000",
            Bold = true
        };

        Assert.Equal("[Score] >= 90", r.Expression);
        Assert.Equal("#00FF00", r.BackgroundColor);
        Assert.Equal("#000000", r.FontColor);
        Assert.True(r.Bold);
    }
}

/// <summary>
/// ImageSizing/LineDirection/ShapeType 枚举完整性测试
/// </summary>
public class ElementEnumExtraTests
{
    // ============== ImageSizing ==============

    [Fact]
    public void ImageSizing_Stretch_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.Stretch };
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    [Fact]
    public void ImageSizing_FitProportional_IsDefault()
    {
        var el = new ImageElement();
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    [Fact]
    public void ImageSizing_Clip_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.Clip };
        Assert.Equal(ImageSizing.Clip, el.Sizing);
    }

    [Fact]
    public void ImageSizing_ActualSize_Works()
    {
        var el = new ImageElement { Sizing = ImageSizing.ActualSize };
        Assert.Equal(ImageSizing.ActualSize, el.Sizing);
    }

    [Fact]
    public void ImageSizing_Has4Values()
    {
        var values = Enum.GetValues(typeof(ImageSizing));
        Assert.Equal(4, values.Length);
    }

    // ============== LineDirection ==============

    [Fact]
    public void LineDirection_Horizontal_IsDefault()
    {
        var el = new LineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void LineDirection_Vertical_Works()
    {
        var el = new LineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void LineDirection_Diagonal_Works()
    {
        var el = new LineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, el.Direction);
    }

    [Fact]
    public void LineDirection_Has3Values()
    {
        var values = Enum.GetValues(typeof(LineDirection));
        Assert.Equal(3, values.Length);
    }

    // ============== ShapeType ==============

    [Fact]
    public void ShapeType_Rectangle_IsDefault()
    {
        var el = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void ShapeType_Ellipse_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void ShapeType_RoundedRect_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
    }

    [Fact]
    public void ShapeType_Triangle_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, el.Shape);
    }

    [Fact]
    public void ShapeType_Has4Values()
    {
        var values = Enum.GetValues(typeof(ShapeType));
        Assert.Equal(4, values.Length);
    }

    // ============== TextAlignment ==============

    [Fact]
    public void TextAlignment_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextAlignment));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void TextAlignment_Justify_Works()
    {
        var el = new TextElement { Alignment = TextAlignment.Justify };
        Assert.Equal(TextAlignment.Justify, el.Alignment);
    }

    // ============== TextBoxType ==============

    [Fact]
    public void TextBoxType_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextBoxType));
        Assert.Equal(4, values.Length);
    }

    // ============== BorderStyle ==============

    [Fact]
    public void BorderStyle_Has4Values()
    {
        var values = Enum.GetValues(typeof(BorderStyle));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void BorderStyle_None_Works()
    {
        var bd = new BorderDef { Style = BorderStyle.None };
        Assert.Equal(BorderStyle.None, bd.Style);
    }

    [Fact]
    public void BorderStyle_Dotted_Works()
    {
        var bd = new BorderDef { Style = BorderStyle.Dotted };
        Assert.Equal(BorderStyle.Dotted, bd.Style);
    }

    [Fact]
    public void BorderStyle_Dashed_Works()
    {
        var bd = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, bd.Style);
    }
}

/// <summary>
/// ShapeElement BorderRadius 测试
/// </summary>
public class ShapeElementExtraTests
{
    [Fact]
    public void BorderRadius_ZeroByDefault()
    {
        var el = new ShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_Set_Works()
    {
        var el = new ShapeElement { BorderRadius = 5 };
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_SetDecimal_Works()
    {
        var el = new ShapeElement { BorderRadius = 2.5 };
        Assert.Equal(2.5, el.BorderRadius);
    }

    [Fact]
    public void FillColor_DefaultIsWhite()
    {
        var el = new ShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void FillColor_Set_Works()
    {
        var el = new ShapeElement { FillColor = "#FF0000" };
        Assert.Equal("#FF0000", el.FillColor);
    }

    [Fact]
    public void RoundedRect_WithBorderRadius_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.RoundedRect, BorderRadius = 8 };
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
        Assert.Equal(8, el.BorderRadius);
    }
}

/// <summary>
/// LineElement 额外属性测试
/// </summary>
public class LineElementExtraTests
{
    [Fact]
    public void LineWidth_DefaultIs1()
    {
        var el = new LineElement();
        Assert.Equal(1, el.LineWidth);
    }

    [Fact]
    public void LineWidth_Set_Works()
    {
        var el = new LineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, el.LineWidth);
    }

    [Fact]
    public void LineColor_DefaultIsBlack()
    {
        var el = new LineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void LineColor_Set_Works()
    {
        var el = new LineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void DiagonalLine_FullSetup()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Diagonal,
            LineWidth = 1.5,
            LineColor = "#0000FF"
        };
        Assert.Equal(LineDirection.Diagonal, el.Direction);
        Assert.Equal(1.5, el.LineWidth);
        Assert.Equal("#0000FF", el.LineColor);
    }
}

/// <summary>
/// SubReportDataBinding ParamMap 测试
/// </summary>
public class SubReportDataBindingExtraTests
{
    [Fact]
    public void ParamMap_EmptyByDefault()
    {
        var db = new SubReportDataBinding();
        Assert.Empty(db.ParamMap);
    }

    [Fact]
    public void ParamMap_AddMapping_Works()
    {
        var db = new SubReportDataBinding();
        db.ParamMap["customerId"] = "CustomerId";
        Assert.Single(db.ParamMap);
        Assert.Equal("CustomerId", db.ParamMap["customerId"]);
    }

    [Fact]
    public void ParamMap_AddMultiple_Works()
    {
        var db = new SubReportDataBinding();
        db.ParamMap["p1"] = "Field1";
        db.ParamMap["p2"] = "Field2";
        db.ParamMap["p3"] = "Field3";
        Assert.Equal(3, db.ParamMap.Count);
    }

    [Fact]
    public void ParamMap_Overwrite_Works()
    {
        var db = new SubReportDataBinding();
        db.ParamMap["p1"] = "Field1";
        db.ParamMap["p1"] = "Field2";
        Assert.Equal("Field2", db.ParamMap["p1"]);
    }

    [Fact]
    public void ParamMap_Remove_Works()
    {
        var db = new SubReportDataBinding();
        db.ParamMap["p1"] = "Field1";
        db.ParamMap.Remove("p1");
        Assert.Empty(db.ParamMap);
    }

    [Fact]
    public void Source_EmptyByDefault()
    {
        var db = new SubReportDataBinding();
        Assert.Equal("", db.Source);
    }

    [Fact]
    public void Source_Set_Works()
    {
        var db = new SubReportDataBinding { Source = "orders" };
        Assert.Equal("orders", db.Source);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var db = new SubReportDataBinding { Source = "details" };
        db.ParamMap["orderId"] = "Id";
        db.ParamMap["region"] = "RegionCode";

        Assert.Equal("details", db.Source);
        Assert.Equal(2, db.ParamMap.Count);
        Assert.Equal("Id", db.ParamMap["orderId"]);
    }
}

/// <summary>
/// CrossTabMeasure 额外属性测试 (Label/Format)
/// </summary>
public class CrossTabMeasureExtraTests
{
    [Fact]
    public void Label_NullByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Label);
    }

    [Fact]
    public void Label_Set_Works()
    {
        var m = new CrossTabMeasure { Label = "Total Sales" };
        Assert.Equal("Total Sales", m.Label);
    }

    [Fact]
    public void Format_NullByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Format);
    }

    [Fact]
    public void Format_SetCurrency_Works()
    {
        var m = new CrossTabMeasure { Format = "currency" };
        Assert.Equal("currency", m.Format);
    }

    [Fact]
    public void Format_SetNumber_Works()
    {
        var m = new CrossTabMeasure { Format = "number:2" };
        Assert.Equal("number:2", m.Format);
    }

    [Fact]
    public void Field_EmptyByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("", m.Field);
    }

    [Fact]
    public void Field_Set_Works()
    {
        var m = new CrossTabMeasure { Field = "Amount" };
        Assert.Equal("Amount", m.Field);
    }

    [Fact]
    public void Aggregate_DefaultIsSum()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("Sum", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetAvg_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Avg" };
        Assert.Equal("Avg", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetCount_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Count" };
        Assert.Equal("Count", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetMin_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Min" };
        Assert.Equal("Min", m.Aggregate);
    }

    [Fact]
    public void Aggregate_SetMax_Works()
    {
        var m = new CrossTabMeasure { Aggregate = "Max" };
        Assert.Equal("Max", m.Aggregate);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var m = new CrossTabMeasure
        {
            Field = "Revenue",
            Aggregate = "Sum",
            Label = "Total Revenue",
            Format = "currency"
        };

        Assert.Equal("Revenue", m.Field);
        Assert.Equal("Sum", m.Aggregate);
        Assert.Equal("Total Revenue", m.Label);
        Assert.Equal("currency", m.Format);
    }
}

/// <summary>
/// MultiColumnConfig 完整属性测试
/// </summary>
public class MultiColumnConfigExtraTests
{
    [Fact]
    public void ColumnCount_DefaultIs2()
    {
        var mc = new MultiColumnConfig();
        Assert.Equal(2, mc.ColumnCount);
    }

    [Fact]
    public void ColumnCount_Set_Works()
    {
        var mc = new MultiColumnConfig { ColumnCount = 3 };
        Assert.Equal(3, mc.ColumnCount);
    }

    [Fact]
    public void ColumnSpacing_DefaultIs5()
    {
        var mc = new MultiColumnConfig();
        Assert.Equal(5, mc.ColumnSpacing);
    }

    [Fact]
    public void ColumnSpacing_Set_Works()
    {
        var mc = new MultiColumnConfig { ColumnSpacing = 10 };
        Assert.Equal(10, mc.ColumnSpacing);
    }

    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var mc = new MultiColumnConfig();
        Assert.Equal("Horizontal", mc.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var mc = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", mc.Direction);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var mc = new MultiColumnConfig
        {
            ColumnCount = 4,
            ColumnSpacing = 8,
            Direction = "Vertical"
        };

        Assert.Equal(4, mc.ColumnCount);
        Assert.Equal(8, mc.ColumnSpacing);
        Assert.Equal("Vertical", mc.Direction);
    }
}
