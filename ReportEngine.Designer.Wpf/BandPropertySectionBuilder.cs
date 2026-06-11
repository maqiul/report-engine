using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.BandStyle;
using static ReportEngine.Designer.Wpf.EnumCnMap;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Band 属性面板构造 - 把 UpdatePropertyListCore 中的"Band 选中"分支 (16 行) 抽离。
/// 等价抽离自 MainWindow.UpdatePropertyListCore() 内部 if (_selectedBand != null && _selectedElement == null) 块。
/// </summary>
internal static class BandPropertySectionBuilder
{
    public static void Build(PropertyRowContext ctx, Band band, MainWindow owner)
    {
        ctx.AddSection("设计");
        ctx.AddLabel("类型", Name(band.Type));
        ctx.AddLabel("标识", band.Type.ToString());

        ctx.AddSection("外观");
        ctx.AddEditor(owner, "高度(mm)", band.Height.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { owner.PushUndo(); band.Height = d; owner.MarkDirty(); owner.RefreshUI(); } });

        ctx.AddSection("行为");
        ctx.AddBool("重复每页", band.RepeatOnNewPage, v => { owner.PushUndo(); band.RepeatOnNewPage = v; owner.MarkDirty(); });

        ctx.AddSection("其它");
        ctx.AddEditor(owner, "数据源", band.DataSource ?? "", v => { owner.PushUndo(); band.DataSource = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); });
    }
}