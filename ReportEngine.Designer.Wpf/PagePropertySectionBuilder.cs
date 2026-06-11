using System;
using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.EnumCnMap;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 页面级属性面板构造 - 把 UpdatePropertyListCore 中的"页面级"分支 (44 行) 抽离。
/// 等价抽离自 MainWindow.UpdatePropertyListCore() 内部 if (_selectedBand == null && _selectedElement == null) 块。
/// </summary>
internal static class PagePropertySectionBuilder
{
    public static void Build(PropertyRowContext ctx, ReportTemplate template, MainWindow owner)
    {
        ctx.AddSection("页面");
        ctx.AddLabel("纸张", template.Page.Width + " × " + template.Page.Height + " mm");
        ctx.AddLabel("方向", template.Page.Orientation == "landscape" ? "横向" : "纵向");
        ctx.AddLabel("边距", "上" + template.Page.Margin.Top + " 下" + template.Page.Margin.Bottom + " 左" + template.Page.Margin.Left + " 右" + template.Page.Margin.Right);
        ctx.AddColor(owner, "背景色", template.Page.BackgroundColor ?? "", v => { owner.PushUndo(); template.Page.BackgroundColor = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddEditor(owner, "背景图片", template.Page.BackgroundImage ?? "", v => { owner.PushUndo(); template.Page.BackgroundImage = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });
        ctx.AddEditor(owner, "水印文字", template.Page.Watermark ?? "", v => { owner.PushUndo(); template.Page.Watermark = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); owner.RefreshUI(); });

        // 多联打印 - 分组卡片式
        ctx.AddSection("多联打印");
        var muInfo = template.Page.MultiUp;
        if (muInfo != null)
        {
            owner.PropertyStack.Children.Add(PropertyCardFactory.CreateMultiUpEnabledCard(
                muInfo, template,
                onEdit: owner.ShowPageSetupDialog,
                onDirectionToggled: newDir =>
                {
                    owner.PushUndo();
                    muInfo.Direction = newDir;
                    owner.MarkDirty();
                    owner.RefreshUI();
                }));
        }
        else
        {
            owner.PropertyStack.Children.Add(PropertyCardFactory.CreateMultiUpDisabledCard(
                onEnable: owner.ShowPageSetupDialog));
        }

        ctx.AddSection("元数据");
        ctx.AddEditor(owner, "作者", template.Author ?? "", v => { owner.PushUndo(); template.Author = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); });
        ctx.AddEditor(owner, "描述", template.Description ?? "", v => { owner.PushUndo(); template.Description = string.IsNullOrEmpty(v) ? null : v; owner.MarkDirty(); });
        ctx.AddLabel("创建时间", template.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
        ctx.AddLabel("修改时间", template.ModifiedAt.ToString("yyyy-MM-dd HH:mm"));
    }
}