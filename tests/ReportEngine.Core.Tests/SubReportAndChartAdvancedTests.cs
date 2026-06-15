using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// SubReportElement 高级属性测试
/// </summary>
public class SubReportElementAdvancedTests
{
    // ============== TemplateRef ==============

    [Fact]
    public void TemplateRef_EmptyByDefault()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.TemplateRef);
    }

    [Fact]
    public void TemplateRef_Set_Works()
    {
        var el = new SubReportElement { TemplateRef = "sub-report.rptx" };
        Assert.Equal("sub-report.rptx", el.TemplateRef);
    }

    [Fact]
    public void TemplateRef_SetPath_Works()
    {
        var el = new SubReportElement { TemplateRef = @"templates\detail.rptx" };
        Assert.Contains("detail.rptx", el.TemplateRef);
    }

    // ============== DataBinding ==============

    [Fact]
    public void DataBinding_NotNull()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding);
    }

    [Fact]
    public void DataBinding_Source_EmptyByDefault()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.DataBinding.Source);
    }

    [Fact]
    public void DataBinding_Source_Set_Works()
    {
        var el = new SubReportElement();
        el.DataBinding.Source = "orders";
        Assert.Equal("orders", el.DataBinding.Source);
    }

    [Fact]
    public void DataBinding_ParamMap_EmptyByDefault()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding.ParamMap);
        Assert.Empty(el.DataBinding.ParamMap);
    }

    [Fact]
    public void DataBinding_ParamMap_Add_Works()
    {
        var el = new SubReportElement();
        el.DataBinding.ParamMap["parentId"] = "Id";
        Assert.Single(el.DataBinding.ParamMap);
    }

    [Fact]
    public void DataBinding_ParamMap_AddMultiple_Works()
    {
        var el = new SubReportElement();
        el.DataBinding.ParamMap["parentId"] = "Id";
        el.DataBinding.ParamMap["region"] = "Region";
        Assert.Equal(2, el.DataBinding.ParamMap.Count);
    }

    // ============== HeightMode ==============

    [Fact]
    public void HeightMode_DefaultIsAuto()
    {
        var el = new SubReportElement();
        Assert.Equal("auto", el.HeightMode);
    }

    [Fact]
    public void HeightMode_SetFixed_Works()
    {
        var el = new SubReportElement { HeightMode = "fixed" };
        Assert.Equal("fixed", el.HeightMode);
    }

    // ============== RepeatPerRow ==============

    [Fact]
    public void RepeatPerRow_TrueByDefault()
    {
        var el = new SubReportElement();
        Assert.True(el.RepeatPerRow);
    }

    [Fact]
    public void RepeatPerRow_SetFalse_Works()
    {
        var el = new SubReportElement { RepeatPerRow = false };
        Assert.False(el.RepeatPerRow);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void SubReportElement_FullSetup_Works()
    {
        var el = new SubReportElement
        {
            TemplateRef = "detail.rptx",
            HeightMode = "auto",
            RepeatPerRow = true
        };
        el.DataBinding.Source = "orders";
        el.DataBinding.ParamMap["customerId"] = "Id";

        Assert.Equal("detail.rptx", el.TemplateRef);
        Assert.Equal("orders", el.DataBinding.Source);
        Assert.Single(el.DataBinding.ParamMap);
    }
}

/// <summary>
/// BarcodeElement 高级属性测试
/// </summary>
public class BarcodeElementAdvancedTests
{
    // ============== Value ==============

    [Fact]
    public void Value_EmptyByDefault()
    {
        var el = new BarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void Value_Set_Works()
    {
        var el = new BarcodeElement { Value = "ABC123" };
        Assert.Equal("ABC123", el.Value);
    }

    [Fact]
    public void Value_SetExpression_Works()
    {
        var el = new BarcodeElement { Value = "{{currentRow.OrderNo}}" };
        Assert.Contains("{{", el.Value);
    }

    // ============== Format ==============

    [Fact]
    public void Format_DefaultIsQRCode()
    {
        var el = new BarcodeElement();
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void Format_SetCode128_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void Format_SetCode39_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code39 };
        Assert.Equal(BarcodeFormat.Code39, el.Format);
    }

    [Fact]
    public void Format_SetEAN13_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.EAN13 };
        Assert.Equal(BarcodeFormat.EAN13, el.Format);
    }

    [Fact]
    public void Format_SetEAN8_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.EAN8 };
        Assert.Equal(BarcodeFormat.EAN8, el.Format);
    }

    [Fact]
    public void Format_SetUPCA_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.UPC_A };
        Assert.Equal(BarcodeFormat.UPC_A, el.Format);
    }

    [Fact]
    public void Format_SetDataMatrix_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.DataMatrix };
        Assert.Equal(BarcodeFormat.DataMatrix, el.Format);
    }

    [Fact]
    public void Format_SetPDF417_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.PDF417 };
        Assert.Equal(BarcodeFormat.PDF417, el.Format);
    }

    // ============== ForeColor ==============

    [Fact]
    public void ForeColor_DefaultIsBlack()
    {
        var el = new BarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void ForeColor_Set_Works()
    {
        var el = new BarcodeElement { ForeColor = "#0000FF" };
        Assert.Equal("#0000FF", el.ForeColor);
    }

    // ============== BackColor ==============

    [Fact]
    public void BackColor_DefaultIsWhite()
    {
        var el = new BarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void BackColor_Set_Works()
    {
        var el = new BarcodeElement { BackColor = "#FFFFCC" };
        Assert.Equal("#FFFFCC", el.BackColor);
    }

    // ============== ShowText ==============

    [Fact]
    public void ShowText_TrueByDefault()
    {
        var el = new BarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void ShowText_SetFalse_Works()
    {
        var el = new BarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void BarcodeElement_QRCode_Works()
    {
        var el = new BarcodeElement
        {
            Value = "https://example.com",
            Format = BarcodeFormat.QRCode,
            Width = 30,
            Height = 30,
            ShowText = false
        };

        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.False(el.ShowText);
    }

    [Fact]
    public void BarcodeElement_Code128_Works()
    {
        var el = new BarcodeElement
        {
            Value = "{{currentRow.OrderNo}}",
            Format = BarcodeFormat.Code128,
            ShowText = true,
            Width = 60,
            Height = 20
        };

        Assert.Equal(BarcodeFormat.Code128, el.Format);
        Assert.True(el.ShowText);
    }
}

/// <summary>
/// ChartElement 高级属性测试
/// </summary>
public class ChartElementAdvancedTests
{
    // ============== ChartType ==============

    [Fact]
    public void ChartType_DefaultIsBar()
    {
        var el = new ChartElement();
        Assert.Equal(ChartType.Bar, el.ChartType);
    }

    [Fact]
    public void ChartType_SetLine_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Line };
        Assert.Equal(ChartType.Line, el.ChartType);
    }

    [Fact]
    public void ChartType_SetPie_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Pie };
        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    [Fact]
    public void ChartType_SetArea_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Area };
        Assert.Equal(ChartType.Area, el.ChartType);
    }

    [Fact]
    public void ChartType_SetScatter_Works()
    {
        var el = new ChartElement { ChartType = ChartType.Scatter };
        Assert.Equal(ChartType.Scatter, el.ChartType);
    }

    // ============== DataSource ==============

    [Fact]
    public void DataSource_EmptyByDefault()
    {
        var el = new ChartElement();
        Assert.Equal("", el.DataSource);
    }

    [Fact]
    public void DataSource_Set_Works()
    {
        var el = new ChartElement { DataSource = "salesData" };
        Assert.Equal("salesData", el.DataSource);
    }

    // ============== CategoryField ==============

    [Fact]
    public void CategoryField_EmptyByDefault()
    {
        var el = new ChartElement();
        Assert.Equal("", el.CategoryField);
    }

    [Fact]
    public void CategoryField_Set_Works()
    {
        var el = new ChartElement { CategoryField = "Month" };
        Assert.Equal("Month", el.CategoryField);
    }

    // ============== Series ==============

    [Fact]
    public void Series_EmptyByDefault()
    {
        var el = new ChartElement();
        Assert.NotNull(el.Series);
        Assert.Empty(el.Series);
    }

    [Fact]
    public void Series_Add_Works()
    {
        var el = new ChartElement();
        el.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "Amount" });
        Assert.Single(el.Series);
    }

    [Fact]
    public void Series_AddMultiple_Works()
    {
        var el = new ChartElement();
        el.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "Amount" });
        el.Series.Add(new ChartSeries { Name = "Cost", ValueField = "Cost" });
        Assert.Equal(2, el.Series.Count);
    }

    // ============== Title ==============

    [Fact]
    public void Title_NullByDefault()
    {
        var el = new ChartElement();
        Assert.Null(el.Title);
    }

    [Fact]
    public void Title_Set_Works()
    {
        var el = new ChartElement { Title = "Monthly Sales" };
        Assert.Equal("Monthly Sales", el.Title);
    }

    // ============== ChartSeries ==============

    [Fact]
    public void ChartSeries_Name_EmptyByDefault()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.Name);
    }

    [Fact]
    public void ChartSeries_ValueField_EmptyByDefault()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.ValueField);
    }

    [Fact]
    public void ChartSeries_Color_NullByDefault()
    {
        var s = new ChartSeries();
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_FullSetup_Works()
    {
        var s = new ChartSeries
        {
            Name = "Sales",
            ValueField = "TotalAmount",
            Color = "#FF5722"
        };

        Assert.Equal("Sales", s.Name);
        Assert.Equal("TotalAmount", s.ValueField);
        Assert.Equal("#FF5722", s.Color);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ChartElement_BarChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Bar,
            DataSource = "sales",
            CategoryField = "Month",
            Title = "Monthly Revenue"
        };
        el.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "Amount", Color = "#2196F3" });

        Assert.Equal(ChartType.Bar, el.ChartType);
        Assert.Single(el.Series);
    }

    [Fact]
    public void ChartElement_PieChart_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Pie,
            DataSource = "categories",
            CategoryField = "Category",
            Title = "Market Share"
        };
        el.Series.Add(new ChartSeries { ValueField = "Percentage" });

        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    [Fact]
    public void ChartElement_MultiSeries_Works()
    {
        var el = new ChartElement
        {
            ChartType = ChartType.Line,
            DataSource = "trends",
            CategoryField = "Quarter"
        };
        el.Series.Add(new ChartSeries { Name = "2023", ValueField = "Revenue2023" });
        el.Series.Add(new ChartSeries { Name = "2024", ValueField = "Revenue2024" });

        Assert.Equal(2, el.Series.Count);
    }
}

/// <summary>
/// TableElement 高级属性测试
/// </summary>
public class TableElementAdvancedPropertyTests
{
    // ============== RowCount ==============

    [Fact]
    public void RowCount_DefaultIs3()
    {
        var el = new TableElement();
        Assert.Equal(3, el.RowCount);
    }

    [Fact]
    public void RowCount_Set_Works()
    {
        var el = new TableElement { RowCount = 10 };
        Assert.Equal(10, el.RowCount);
    }

    // ============== ColCount ==============

    [Fact]
    public void ColCount_DefaultIs3()
    {
        var el = new TableElement();
        Assert.Equal(3, el.ColCount);
    }

    [Fact]
    public void ColCount_Set_Works()
    {
        var el = new TableElement { ColCount = 5 };
        Assert.Equal(5, el.ColCount);
    }

    // ============== ColumnWidths ==============

    [Fact]
    public void ColumnWidths_EmptyByDefault()
    {
        var el = new TableElement();
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void ColumnWidths_Add_Works()
    {
        var el = new TableElement();
        el.ColumnWidths.Add(30);
        el.ColumnWidths.Add(50);
        el.ColumnWidths.Add(30);
        Assert.Equal(3, el.ColumnWidths.Count);
    }

    // ============== RowHeights ==============

    [Fact]
    public void RowHeights_EmptyByDefault()
    {
        var el = new TableElement();
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void RowHeights_Add_Works()
    {
        var el = new TableElement();
        el.RowHeights.Add(10);
        el.RowHeights.Add(8);
        Assert.Equal(2, el.RowHeights.Count);
    }

    // ============== Cells ==============

    [Fact]
    public void Cells_EmptyByDefault()
    {
        var el = new TableElement();
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void Cells_Add_Works()
    {
        var el = new TableElement();
        el.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "Header" });
        Assert.Single(el.Cells);
    }

    // ============== BorderWidth ==============

    [Fact]
    public void BorderWidth_DefaultIs03()
    {
        var el = new TableElement();
        Assert.Equal(0.3, el.BorderWidth);
    }

    [Fact]
    public void BorderWidth_Set_Works()
    {
        var el = new TableElement { BorderWidth = 1.0 };
        Assert.Equal(1.0, el.BorderWidth);
    }

    // ============== BorderColor ==============

    [Fact]
    public void BorderColor_DefaultIsBlack()
    {
        var el = new TableElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void BorderColor_Set_Works()
    {
        var el = new TableElement { BorderColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", el.BorderColor);
    }

    // ============== TableCell ==============

    [Fact]
    public void TableCell_Row_DefaultIsZero()
    {
        var cell = new TableCell();
        Assert.Equal(0, cell.Row);
    }

    [Fact]
    public void TableCell_Col_DefaultIsZero()
    {
        var cell = new TableCell();
        Assert.Equal(0, cell.Col);
    }

    [Fact]
    public void TableCell_RowSpan_DefaultIs1()
    {
        var cell = new TableCell();
        Assert.Equal(1, cell.RowSpan);
    }

    [Fact]
    public void TableCell_ColSpan_DefaultIs1()
    {
        var cell = new TableCell();
        Assert.Equal(1, cell.ColSpan);
    }

    [Fact]
    public void TableCell_Text_EmptyByDefault()
    {
        var cell = new TableCell();
        Assert.Equal("", cell.Text);
    }

    [Fact]
    public void TableCell_Font_NotNull()
    {
        var cell = new TableCell();
        Assert.NotNull(cell.Font);
    }

    [Fact]
    public void TableCell_Alignment_DefaultIsCenter()
    {
        var cell = new TableCell();
        Assert.Equal(TextAlignment.Center, cell.Alignment);
    }

    [Fact]
    public void TableCell_BackgroundColor_NullByDefault()
    {
        var cell = new TableCell();
        Assert.Null(cell.BackgroundColor);
    }

    [Fact]
    public void TableCell_MergedCell_Works()
    {
        var cell = new TableCell
        {
            Row = 0,
            Col = 0,
            RowSpan = 2,
            ColSpan = 3,
            Text = "Merged Header"
        };

        Assert.Equal(2, cell.RowSpan);
        Assert.Equal(3, cell.ColSpan);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void TableElement_SimpleTable_Works()
    {
        var el = new TableElement
        {
            RowCount = 4,
            ColCount = 3,
            ColumnWidths = { 40, 60, 40 }
        };

        Assert.Equal(4, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.Equal(3, el.ColumnWidths.Count);
    }
}

/// <summary>
/// CrossTabElement 高级属性测试
/// </summary>
public class CrossTabElementAdvancedTests
{
    // ============== DataSource ==============

    [Fact]
    public void DataSource_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.Equal("", el.DataSource);
    }

    [Fact]
    public void DataSource_Set_Works()
    {
        var el = new CrossTabElement { DataSource = "salesData" };
        Assert.Equal("salesData", el.DataSource);
    }

    // ============== RowFields ==============

    [Fact]
    public void RowFields_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.RowFields);
        Assert.Empty(el.RowFields);
    }

    [Fact]
    public void RowFields_Add_Works()
    {
        var el = new CrossTabElement();
        el.RowFields.Add("Region");
        Assert.Single(el.RowFields);
    }

    // ============== ColumnFields ==============

    [Fact]
    public void ColumnFields_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.ColumnFields);
        Assert.Empty(el.ColumnFields);
    }

    [Fact]
    public void ColumnFields_Add_Works()
    {
        var el = new CrossTabElement();
        el.ColumnFields.Add("Quarter");
        Assert.Single(el.ColumnFields);
    }

    // ============== Measures ==============

    [Fact]
    public void Measures_EmptyByDefault()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.Measures);
        Assert.Empty(el.Measures);
    }

    [Fact]
    public void Measures_Add_Works()
    {
        var el = new CrossTabElement();
        el.Measures.Add(new CrossTabMeasure { Field = "Amount", Aggregate = "Sum" });
        Assert.Single(el.Measures);
    }

    // ============== ShowRowTotal ==============

    [Fact]
    public void ShowRowTotal_TrueByDefault()
    {
        var el = new CrossTabElement();
        Assert.True(el.ShowRowTotal);
    }

    [Fact]
    public void ShowRowTotal_SetFalse_Works()
    {
        var el = new CrossTabElement { ShowRowTotal = false };
        Assert.False(el.ShowRowTotal);
    }

    // ============== ShowColumnTotal ==============

    [Fact]
    public void ShowColumnTotal_TrueByDefault()
    {
        var el = new CrossTabElement();
        Assert.True(el.ShowColumnTotal);
    }

    [Fact]
    public void ShowColumnTotal_SetFalse_Works()
    {
        var el = new CrossTabElement { ShowColumnTotal = false };
        Assert.False(el.ShowColumnTotal);
    }

    // ============== CellPadding ==============

    [Fact]
    public void CellPadding_DefaultIs1()
    {
        var el = new CrossTabElement();
        Assert.Equal(1.0, el.CellPadding);
    }

    [Fact]
    public void CellPadding_Set_Works()
    {
        var el = new CrossTabElement { CellPadding = 2.5 };
        Assert.Equal(2.5, el.CellPadding);
    }

    // ============== BorderWidth ==============

    [Fact]
    public void BorderWidth_DefaultIs03()
    {
        var el = new CrossTabElement();
        Assert.Equal(0.3, el.BorderWidth);
    }

    // ============== BorderColor ==============

    [Fact]
    public void BorderColor_DefaultIsBlack()
    {
        var el = new CrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    // ============== CellFont ==============

    [Fact]
    public void CellFont_NotNull()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.CellFont);
    }

    // ============== HeaderFont ==============

    [Fact]
    public void HeaderFont_NotNull()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.HeaderFont);
    }

    [Fact]
    public void HeaderFont_DefaultIsBold()
    {
        var el = new CrossTabElement();
        Assert.True(el.HeaderFont.Bold);
    }

    // ============== CrossTabMeasure ==============

    [Fact]
    public void CrossTabMeasure_Field_EmptyByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("", m.Field);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_DefaultIsSum()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("Sum", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Format_NullByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Label_NullByDefault()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Label);
    }

    [Fact]
    public void CrossTabMeasure_FullSetup_Works()
    {
        var m = new CrossTabMeasure
        {
            Field = "Revenue",
            Aggregate = "Avg",
            Format = "N2",
            Label = "Average Revenue"
        };

        Assert.Equal("Revenue", m.Field);
        Assert.Equal("Avg", m.Aggregate);
        Assert.Equal("N2", m.Format);
        Assert.Equal("Average Revenue", m.Label);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void CrossTabElement_SalesPivot_Works()
    {
        var el = new CrossTabElement
        {
            DataSource = "sales",
            ShowRowTotal = true,
            ShowColumnTotal = true
        };
        el.RowFields.Add("Region");
        el.ColumnFields.Add("Quarter");
        el.Measures.Add(new CrossTabMeasure { Field = "Amount", Aggregate = "Sum", Format = "N0" });

        Assert.Single(el.RowFields);
        Assert.Single(el.ColumnFields);
        Assert.Single(el.Measures);
    }
}

/// <summary>
/// MultiColumnConfig 高级属性测试
/// </summary>
public class MultiColumnConfigAdvancedTests
{
    // ============== ColumnCount ==============

    [Fact]
    public void ColumnCount_DefaultIs2()
    {
        var config = new MultiColumnConfig();
        Assert.Equal(2, config.ColumnCount);
    }

    [Fact]
    public void ColumnCount_Set_Works()
    {
        var config = new MultiColumnConfig { ColumnCount = 3 };
        Assert.Equal(3, config.ColumnCount);
    }

    // ============== ColumnSpacing ==============

    [Fact]
    public void ColumnSpacing_DefaultIs5()
    {
        var config = new MultiColumnConfig();
        Assert.Equal(5, config.ColumnSpacing);
    }

    [Fact]
    public void ColumnSpacing_Set_Works()
    {
        var config = new MultiColumnConfig { ColumnSpacing = 10 };
        Assert.Equal(10, config.ColumnSpacing);
    }

    // ============== Direction ==============

    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var config = new MultiColumnConfig();
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var config = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", config.Direction);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void MultiColumnConfig_LabelPrint_Works()
    {
        var config = new MultiColumnConfig
        {
            ColumnCount = 3,
            ColumnSpacing = 5,
            Direction = "Horizontal"
        };

        Assert.Equal(3, config.ColumnCount);
        Assert.Equal(5, config.ColumnSpacing);
    }
}
