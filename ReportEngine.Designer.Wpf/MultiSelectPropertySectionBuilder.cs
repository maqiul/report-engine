using System.Linq;
using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.ElementFactory;
using static ReportEngine.Designer.Wpf.EnumCnMap;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 多选属性面板构造 - 把 UpdateMultiSelectProperties 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.UpdateMultiSelectProperties() (55 行)。
///
/// 模式与 PropertySectionBuilder 一致：使用 PropertyRowContext 上下文，
/// 通过 owner 调 MainWindow 的 PushUndo / MarkDirty / RefreshUI 等副作用。
/// </summary>
internal static class MultiSelectPropertySectionBuilder
{
    public static void Build(PropertyRowContext ctx, MainWindow owner)
    {
        var targets = owner.SelectedElements;

        ctx.AddSection("批量编辑 (" + targets.Count + " 个元素)");
        ctx.AddLabel("数量", targets.Count.ToString());

        // 通用属性
        ctx.AddSection("位置尺寸");
        ctx.AddEditor(owner, "宽(mm)", "", v => { if (double.TryParse(v, out var d) && d > 0) { owner.PushUndo(); foreach (var el in targets) el.Width = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "高(mm)", "", v => { if (double.TryParse(v, out var d) && d > 0) { owner.PushUndo(); foreach (var el in targets) el.Height = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "X(mm)", "", v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); foreach (var el in targets) el.X = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddEditor(owner, "Y(mm)", "", v => { if (double.TryParse(v, out var d)) { owner.PushUndo(); foreach (var el in targets) el.Y = d; owner.MarkDirty(); owner.RefreshUI(); } });

        ctx.AddSection("设计");
        bool allVisible = targets.All(e => e.Visible);
        ctx.AddBool("可见", allVisible, v => { owner.PushUndo(); foreach (var el in targets) el.Visible = v; owner.MarkDirty(); });
        bool allLocked = targets.All(e => e.Locked);
        ctx.AddBool("锁定", allLocked, v => { owner.PushUndo(); foreach (var el in targets) el.Locked = v; owner.MarkDirty(); owner.RefreshUI(); });

        ctx.AddSection("外观");
        ctx.AddColor(owner, "背景色", "", v => { owner.PushUndo(); foreach (var el in targets) el.BackgroundColor = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });

        // 批量边框编辑
        ctx.AddSection("边框");
        ctx.AddEditor(owner, "边框宽", "", v => { if (double.TryParse(v, out var d) && d >= 0) { owner.PushUndo(); foreach (var el in targets) EnsureBorder(el).Width = d; owner.MarkDirty(); owner.RefreshUI(); } });
        ctx.AddColor(owner, "边框色", "", v => { owner.PushUndo(); foreach (var el in targets) EnsureBorder(el).Color = string.IsNullOrEmpty(v) ? "#000000" : v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("上边框", true, v => { owner.PushUndo(); foreach (var el in targets) EnsureBorder(el).Top = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("下边框", true, v => { owner.PushUndo(); foreach (var el in targets) EnsureBorder(el).Bottom = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("左边框", true, v => { owner.PushUndo(); foreach (var el in targets) EnsureBorder(el).Left = v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddBool("右边框", true, v => { owner.PushUndo(); foreach (var el in targets) EnsureBorder(el).Right = v; owner.MarkDirty(); owner.RefreshUI(); });

        // 如果所有选中元素都是 TextElement，显示文本共有属性
        if (targets.All(e => e is TextElement))
        {
            var texts = targets.Cast<TextElement>().ToList();
            ctx.AddSection("字体");
            string commonFamily = texts.Select(t => t.Font.Family).Distinct().Count() == 1 ? texts[0].Font.Family : "";
            ctx.AddEditor(owner, "字体", commonFamily, v => { if (!string.IsNullOrEmpty(v)) { owner.PushUndo(); foreach (var t in texts) t.Font.Family = v; owner.MarkDirty(); owner.RefreshUI(); } });
            string commonSize = texts.Select(t => t.Font.Size).Distinct().Count() == 1 ? texts[0].Font.Size.ToString() : "";
            ctx.AddEditor(owner, "字号", commonSize, v => { if (double.TryParse(v, out var sz) && sz > 0) { owner.PushUndo(); foreach (var t in texts) t.Font.Size = sz; owner.MarkDirty(); owner.RefreshUI(); } });
            bool allBold = texts.All(t => t.Font.Bold);
            ctx.AddBool("加粗", allBold, v => { owner.PushUndo(); foreach (var t in texts) t.Font.Bold = v; owner.MarkDirty(); owner.RefreshUI(); });
            bool allItalic = texts.All(t => t.Font.Italic);
            ctx.AddBool("斜体", allItalic, v => { owner.PushUndo(); foreach (var t in texts) t.Font.Italic = v; owner.MarkDirty(); owner.RefreshUI(); });
            ctx.AddColor(owner, "字体色", "", v => { owner.PushUndo(); foreach (var t in texts) t.Font.Color = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });
            ctx.AddCombo("对齐", new[] { "左对齐", "居中", "右对齐", "两端对齐" }, "",
                v => { var a = CNToAlign(v); owner.PushUndo(); foreach (var t in texts) t.Alignment = a; owner.MarkDirty(); owner.RefreshUI(); });
        }
    }
}