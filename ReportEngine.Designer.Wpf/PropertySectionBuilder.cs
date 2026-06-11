using System;
using System.Linq;
using static ReportEngine.Designer.Wpf.EnumCnMap;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 属性面板 section 构建器 - 把 UpdatePropertyListCore 中的元素级属性段集中管理。
/// 等价抽离自 MainWindow.UpdatePropertyListCore() 中 `_selectedElement != null` 分支的 ~170 行。
///
/// 设计：通过 owner 调 MainWindow 的 internal PushUndo/MarkDirty/RefreshUI 副作用方法。
/// </summary>
internal static class PropertySectionBuilder
{
    /// <summary>
    /// 选中单元素时构建其属性面板 - 设计/外观/边框/类型特定/位置/行为/其它。
    /// </summary>
    public static void BuildElementProperties(
        PropertyRowContext ctx,
        ReportElement el,
        MainWindow owner)
    {
        // === 设计 ===
        ctx.AddSection("设计");
        ctx.AddLabel("类型", ElementTypeName(el));
        ctx.AddLabel("标识", el.Id ?? "");
        ctx.AddEditor(owner, "名称", el.Name ?? "", v => { owner.PushUndo(); el.Name = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("可见", el.Visible, v => { owner.PushUndo(); el.Visible = v; owner.MarkDirty(); });
        ctx.AddBool("锁定", el.Locked, v => { owner.PushUndo(); el.Locked = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddEditor(owner, "旋转(°)", el.Rotation.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); el.Rotation = d % 360; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "透明度", el.Opacity.ToString("F2"), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); el.Opacity = Math.Max(0, Math.Min(1, d)); owner.MarkDirty(); owner.RefreshUI(); } });

        // === 外观 ===
        ctx.AddSection("外观");
        ctx.AddColor(owner, "背景色", el.BackgroundColor ?? "", v => { owner.PushUndo(); el.BackgroundColor = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });

        // 边框属性
        ctx.AddSection("边框");
        var border = el.Border ?? new BorderDef();
        ctx.AddEditor(owner, "边框宽", border.Width.ToString(), v =>
        {
            if (double.TryParse(v, out var d) && d >= 0) { owner.PushUndo(); ElementFactory.EnsureBorder(el).Width = d; owner.MarkDirty(); owner.RefreshUI(); }
        });
        ctx.AddColor(owner, "边框色", border.Color ?? "#000000", v =>
        {
            owner.PushUndo();
            ElementFactory.EnsureBorder(el).Color = string.IsNullOrEmpty(v) ? "#000000" : v;
            owner.MarkDirty(); owner.RefreshUI();
        });
        ctx.AddCombo("边框样式", new[] { "Solid", "Dashed", "Dotted", "None" }, border.Style.ToString(), v =>
        {
            owner.PushUndo();
            if (Enum.TryParse<BorderStyle>(v, out var bs)) ElementFactory.EnsureBorder(el).Style = bs;
            owner.MarkDirty(); owner.RefreshUI();
        });
        ctx.AddBool("上边框", border.Top, v => { owner.PushUndo(); ElementFactory.EnsureBorder(el).Top = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("下边框", border.Bottom, v => { owner.PushUndo(); ElementFactory.EnsureBorder(el).Bottom = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("左边框", border.Left, v => { owner.PushUndo(); ElementFactory.EnsureBorder(el).Left = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("右边框", border.Right, v => { owner.PushUndo(); ElementFactory.EnsureBorder(el).Right = v; owner.MarkDirty(); owner.RefreshUI(); });

        switch (el)
        {
            case TextElement t:
                ctx.AddFontRow(owner, t);
                ctx.AddColor(owner, "字体色", t.Font.Color ?? "", v => { owner.PushUndo(); t.Font.Color = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });
                ctx.AddCombo("对齐", new[] { "左对齐", "居中", "右对齐", "两端对齐" }, AlignToCN(t.Alignment.ToString()), v => { var a = CNToAlign(v); owner.PushUndo(); t.Alignment = a; owner.MarkDirty(); owner.RefreshUI(); });
                break;
            case LineElement l:
                ctx.AddEditor(owner, "线宽", l.LineWidth.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); l.LineWidth = d; owner.MarkDirty(); owner.RefreshUI(); } });
                ctx.AddColor(owner, "线色", l.LineColor, v => { owner.PushUndo(); l.LineColor = v; owner.MarkDirty(); owner.RefreshUI(); });
                break;
            case ShapeElement s:
                ctx.AddColor(owner, "填充色", s.FillColor, v => { owner.PushUndo(); s.FillColor = v; owner.MarkDirty(); owner.RefreshUI(); });
                ctx.AddEditor(owner, "圆角", s.BorderRadius.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); s.BorderRadius = d; owner.MarkDirty(); owner.RefreshUI(); } });
                break;
            case BarcodeElement bc:
                ctx.AddColor(owner, "前景色", bc.ForeColor, v => { owner.PushUndo(); bc.ForeColor = v; owner.MarkDirty(); });
                break;
            case TableElement tbl:
                ctx.AddEditor(owner, "边框宽", tbl.BorderWidth.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); tbl.BorderWidth = d; owner.MarkDirty(); } });
                ctx.AddColor(owner, "边框色", tbl.BorderColor, v => { owner.PushUndo(); tbl.BorderColor = v; owner.MarkDirty(); });
                break;
        }

        // === 位置尺寸 ===
        ctx.AddEditor(owner, "X(mm)", el.X.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); el.X = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "Y(mm)", el.Y.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); el.Y = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "宽(mm)", el.Width.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { owner.PushUndo(); el.Width = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "高(mm)", el.Height.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { owner.PushUndo(); el.Height = d; owner.MarkDirty(); owner.RefreshUI(); } });

        // === 行为 ===
        ctx.AddSection("行为");
        switch (el)
        {
            case TextElement t:
                ctx.AddBool("自动增高", t.CanGrow, v => { owner.PushUndo(); t.CanGrow = v; owner.MarkDirty(); });
                break;
            case LineElement l:
                ctx.AddCombo("方向", new[] { "水平", "垂直", "对角线" }, DirToCN(l.Direction.ToString()), v => { owner.PushUndo(); l.Direction = CNToDir(v); owner.MarkDirty(); owner.RefreshUI(); });
                break;
            case ShapeElement s:
                ctx.AddCombo("形状", new[] { "矩形", "椭圆", "圆角矩形", "三角形" }, ShapeToCN(s.Shape.ToString()), v => { owner.PushUndo(); s.Shape = CNToShape(v); owner.MarkDirty(); owner.RefreshUI(); });
                break;
            case ImageElement img:
                ctx.AddCombo("缩放", new[] { "拉伸", "等比缩放", "裁剪", "原始尺寸" }, SizingToCN(img.Sizing.ToString()), v => { owner.PushUndo(); img.Sizing = CNToSizing(v); owner.MarkDirty(); });
                break;
            case SubReportElement sr:
                ctx.AddBool("每行重复", sr.RepeatPerRow, v => { owner.PushUndo(); sr.RepeatPerRow = v; owner.MarkDirty(); });
                break;
            case BarcodeElement bc:
                ctx.AddCombo("格式", new[] { "Code128", "Code39", "EAN13", "EAN8", "UPC_A", "二维码(QR)", "DataMatrix", "PDF417" }, BcFmtToCN(bc.Format.ToString()), v => { owner.PushUndo(); bc.Format = CNToBcFmt(v); owner.MarkDirty(); owner.RefreshUI(); });
                ctx.AddBool("显示文字", bc.ShowText, v => { owner.PushUndo(); bc.ShowText = v; owner.MarkDirty(); });
                break;
            case TableElement tbl:
                ctx.AddEditor(owner, "行数", tbl.RowCount.ToString(), v => { if (int.TryParse(v, out var i) && i > 0) { owner.PushUndo(); tbl.RowCount = i; owner.MarkDirty(); owner.RefreshUI(); } });
                ctx.AddEditor(owner, "列数", tbl.ColCount.ToString(), v => { if (int.TryParse(v, out var i) && i > 0) { owner.PushUndo(); tbl.ColCount = i; owner.MarkDirty(); owner.RefreshUI(); } });
                break;
            case CrossTabElement ct:
                ctx.AddBool("行合计", ct.ShowRowTotal, v => { owner.PushUndo(); ct.ShowRowTotal = v; owner.MarkDirty(); });
                ctx.AddBool("列合计", ct.ShowColumnTotal, v => { owner.PushUndo(); ct.ShowColumnTotal = v; owner.MarkDirty(); });
                break;
            case ChartElement ch:
                ctx.AddCombo("图表类型", new[] { "柱状图", "折线图", "饼图", "面积图", "散点图" },
                    ChartTypeCN(ch.ChartType), v => { owner.PushUndo(); ch.ChartType = CNToChartType(v); owner.MarkDirty(); owner.RefreshUI(); });
                break;
        }

        // === 其它 ===
        ctx.AddSection("其它");
        switch (el)
        {
            case TextElement t:
                ctx.AddCombo("框类型", new[] { "静态框", "字段框", "统计框", "系统变量框" },
                    BoxTypeToCN(t.BoxType), v => {
                        owner.PushUndo();
                        switch (v) {
                            case "字段框": t.DataField = t.DataField ?? "FieldName"; t.SummaryFunction = null; t.SystemVariable = null; break;
                            case "统计框": t.SummaryFunction = t.SummaryFunction ?? "Sum"; t.SummaryField = t.SummaryField ?? "Amount"; t.DataField = null; t.SystemVariable = null; break;
                            case "系统变量框": t.SystemVariable = t.SystemVariable ?? "PageNumber"; t.DataField = null; t.SummaryFunction = null; break;
                            default: t.DataField = null; t.SummaryFunction = null; t.SystemVariable = null; break;
                        }
                        owner.MarkDirty(); owner.RefreshUI();
                    });
                if (t.BoxType == TextBoxType.Static)
                    ctx.AddEditor(owner, "文本", t.Text, v => { owner.PushUndo(); t.Text = v; owner.MarkDirty(); owner.RefreshUI(); });
                if (t.BoxType == TextBoxType.Field)
                    ctx.AddExpr(owner, "绑定字段", t.DataField ?? "", v => { owner.PushUndo(); t.DataField = v; owner.MarkDirty(); owner.RefreshUI(); });
                if (t.BoxType == TextBoxType.Summary)
                {
                    ctx.AddCombo("统计函数", new[] { "Sum", "Count", "Avg", "Max", "Min" },
                        t.SummaryFunction ?? "Sum", v => { owner.PushUndo(); t.SummaryFunction = v; owner.MarkDirty(); owner.RefreshUI(); });
                    ctx.AddEditor(owner, "统计字段", t.SummaryField ?? "", v => { owner.PushUndo(); t.SummaryField = v; owner.MarkDirty(); owner.RefreshUI(); });
                }
                if (t.BoxType == TextBoxType.SysVar)
                    ctx.AddCombo("系统变量", new[] { "PageNumber", "TotalPages", "PrintDate", "PrintTime", "ReportTitle" },
                        t.SystemVariable ?? "PageNumber", v => { owner.PushUndo(); t.SystemVariable = v; owner.MarkDirty(); owner.RefreshUI(); });
                ctx.AddExpr(owner, "格式", t.Format ?? "", v => { owner.PushUndo(); t.Format = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); });
                ctx.AddExpr(owner, "超链接", t.Hyperlink ?? "", v => { owner.PushUndo(); t.Hyperlink = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); });
                break;
            case ImageElement img:
                ctx.AddEditor(owner, "图像源", img.Source, v => { owner.PushUndo(); img.Source = v; owner.MarkDirty(); });
                break;
            case SubReportElement sr:
                ctx.AddExpr(owner, "模板引用", sr.TemplateRef, v => { owner.PushUndo(); sr.TemplateRef = v; owner.MarkDirty(); owner.RefreshUI(); });
                ctx.AddExpr(owner, "数据源", sr.DataBinding.Source, v => { owner.PushUndo(); sr.DataBinding.Source = v; owner.MarkDirty(); });
                break;
            case BarcodeElement bc:
                ctx.AddExpr(owner, "内容", bc.Value, v => { owner.PushUndo(); bc.Value = v; owner.MarkDirty(); owner.RefreshUI(); });
                break;
            case CrossTabElement ct:
                ctx.AddExpr(owner, "数据源", ct.DataSource, v => { owner.PushUndo(); ct.DataSource = v; owner.MarkDirty(); owner.RefreshUI(); });
                ctx.AddExpr(owner, "行字段", string.Join(",", ct.RowFields), v => { owner.PushUndo(); ct.RowFields = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(); owner.MarkDirty(); });
                ctx.AddExpr(owner, "列字段", string.Join(",", ct.ColumnFields), v => { owner.PushUndo(); ct.ColumnFields = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(); owner.MarkDirty(); });
                ctx.AddEditor(owner, "边框宽", ct.BorderWidth.ToString(), v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); ct.BorderWidth = d; owner.MarkDirty(); } });
                break;
            case ChartElement ch:
                ctx.AddExpr(owner, "标题", ch.Title ?? "", v => { owner.PushUndo(); ch.Title = v; owner.MarkDirty(); });
                ctx.AddExpr(owner, "数据源", ch.DataSource, v => { owner.PushUndo(); ch.DataSource = v; owner.MarkDirty(); });
                ctx.AddExpr(owner, "分类字段", ch.CategoryField, v => { owner.PushUndo(); ch.CategoryField = v; owner.MarkDirty(); });
                break;
        }
    }
}