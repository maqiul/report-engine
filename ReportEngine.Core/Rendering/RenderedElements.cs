using System.Collections.Generic;

namespace ReportEngine.Core.Rendering;

public class RenderedTextElement : RenderedElement
{
    public string Text { get; set; } = "";
    public FontDef Font { get; set; } = new FontDef();
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    public string? Hyperlink { get; set; }
}

public class RenderedImageElement : RenderedElement
{
    public string Source { get; set; } = "";
}

public class RenderedLineElement : RenderedElement
{
    public LineDirection Direction { get; set; }
    public double LineWidth { get; set; }
    public string LineColor { get; set; } = "#000000";
}

public class RenderedShapeElement : RenderedElement
{
    public ShapeType Shape { get; set; }
    public double BorderRadius { get; set; }
    public string FillColor { get; set; } = "#FFFFFF";
}

public class RenderedBarcodeElement : RenderedElement
{
    public string Value { get; set; } = "";
    public BarcodeFormat Format { get; set; }
    public string ForeColor { get; set; } = "#000000";
    public string BackColor { get; set; } = "#FFFFFF";
    public bool ShowText { get; set; } = true;
}

public class RenderedTableElement : RenderedElement
{
    public int RowCount { get; set; }
    public int ColCount { get; set; }
    public List<double> ColumnWidths { get; set; } = new List<double>();
    public List<double> RowHeights { get; set; } = new List<double>();
    public List<RenderedTableCell> Cells { get; set; } = new List<RenderedTableCell>();
    public double BorderWidth { get; set; }
    public string BorderColor { get; set; } = "#000000";
}

public class RenderedTableCell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColSpan { get; set; } = 1;
    public string Text { get; set; } = "";
    public FontDef Font { get; set; } = new FontDef();
    public TextAlignment Alignment { get; set; } = TextAlignment.Center;
    public string? BackgroundColor { get; set; }
}

/// <summary>
/// 交叉表渲染结果 — 展开后的表格
/// </summary>
public class RenderedCrossTabElement : RenderedElement
{
    public int RowCount { get; set; }
    public int ColCount { get; set; }
    public List<double> ColumnWidths { get; set; } = new List<double>();
    public List<double> RowHeights { get; set; } = new List<double>();
    public List<RenderedTableCell> Cells { get; set; } = new List<RenderedTableCell>();
    public double BorderWidth { get; set; }
    public string BorderColor { get; set; } = "#000000";
}