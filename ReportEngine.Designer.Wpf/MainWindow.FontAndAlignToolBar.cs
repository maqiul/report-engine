using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.Win32;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Pdf;
using ReportEngine.Export.Excel;
using static ReportEngine.Designer.Wpf.UiFactory;
using static ReportEngine.Designer.Wpf.ElementFactory;
using static ReportEngine.Designer.Wpf.ElementIcons;
using static ReportEngine.Designer.Wpf.BandStyle;
using static ReportEngine.Designer.Wpf.PreviewJsonParser;
using static ReportEngine.Designer.Wpf.ExportDataBuilder;
using static ReportEngine.Designer.Wpf.EnumCnMap;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
private ToolBarTray BuildFontToolBar()
{
    return FontToolBarBuilder.Build(
        onFontFamilyChanged: ApplyFontFamily,
        onFontSizeChanged: ApplyFontSize,
        onToggleBold: ToggleFontBold,
        onToggleItalic: ToggleFontItalic,
        onToggleUnderline: ToggleFontUnderline,
        onAlignLeft: () => SetAlignment(ReportEngine.Core.TextAlignment.Left),
        onAlignCenter: () => SetAlignment(ReportEngine.Core.TextAlignment.Center),
        onAlignRight: () => SetAlignment(ReportEngine.Core.TextAlignment.Right),
        out _fontFamilyCombo,
        out _fontSizeCombo);
}

private void ApplyFontFamily()
{
    if (_selectedElement is TextElement t && !string.IsNullOrEmpty(_fontFamilyCombo.Text))
    { PushUndo(); t.Font.Family = _fontFamilyCombo.Text; MarkDirty(); RefreshUI(); }
}

private void ApplyFontSize()
{
    if (_selectedElement is TextElement t && double.TryParse(_fontSizeCombo.Text, out var sz) && sz > 0)
    { PushUndo(); t.Font.Size = sz; MarkDirty(); RefreshUI(); }
}

private void ToggleFontBold()
{
    if (_selectedElement is TextElement t) { PushUndo(); t.Font.Bold = !t.Font.Bold; MarkDirty(); RefreshUI(); }
}

private void ToggleFontItalic()
{
    if (_selectedElement is TextElement t) { PushUndo(); t.Font.Italic = !t.Font.Italic; MarkDirty(); RefreshUI(); }
}

private void ToggleFontUnderline()
{
    if (_selectedElement is TextElement t) { PushUndo(); t.Font.Underline = !t.Font.Underline; MarkDirty(); RefreshUI(); }
}

private void SetAlignment(ReportEngine.Core.TextAlignment a)
{
    if (_selectedElement is TextElement t) { PushUndo(); t.Alignment = a; MarkDirty(); RefreshUI(); }
}

private ToolBarTray BuildAlignToolBar()
{
    return AlignToolBarBuilder.Build(
        onAlignLeft: () => AlignElements("left"),
        onAlignRight: () => AlignElements("right"),
        onAlignTop: () => AlignElements("top"),
        onAlignBottom: () => AlignElements("bottom"),
        onAlignHCenter: () => AlignElements("hcenter"),
        onAlignVCenter: () => AlignElements("vcenter"),
        onSameWidth: () => AlignElements("samewidth"),
        onSameHeight: () => AlignElements("sameheight"),
        onBringToFront: () => MoveElementOrder("front"),
        onSendToBack: () => MoveElementOrder("back"),
        onFormatPainter: StartFormatPainter,
        onToggleLock: ToggleLockSelected,
        onGroup: GroupSelected,
        onUngroup: UngroupSelected);
}

private void ToggleLockSelected()
{
    var targets = _selectedElements.Count > 0 ? _selectedElements : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : new List<ReportElement>());
    if (targets.Count == 0) { _statusText.Text = "请先选中元素"; return; }
    PushUndo();
    bool newState = LockStateToggler.Toggle(targets);
    MarkDirty();
    RefreshUI();
    _statusText.Text = newState ? "已锁定 " + targets.Count + " 个元素" : "已解锁 " + targets.Count + " 个元素";
}

private void GroupSelected()
{
    var targets = _selectedElements.Count > 1 ? _selectedElements : (_selectedElements.Count == 0 && _selectedElement != null ? new List<ReportElement> { _selectedElement } : _selectedElements);
    if (targets.Count < 2) { _statusText.Text = "请选中2个以上元素再组合"; return; }
    PushUndo();
    ElementGrouper.Group(targets);
    MarkDirty();
    RefreshUI();
    _statusText.Text = "已组合 " + targets.Count + " 个元素";
}

private void UngroupSelected()
{
    var targets = _selectedElements.Count > 0 ? _selectedElements : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : new List<ReportElement>());
    if (targets.Count == 0 || targets.All(e => e.GroupId == null)) { _statusText.Text = "选中的元素未分组"; return; }
    PushUndo();
    ElementGrouper.Ungroup(targets);
    MarkDirty();
    RefreshUI();
    _statusText.Text = "已取消组合";
}

private void StartFormatPainter()
{
    var src = _selectedElement;
    if (src == null) { _statusText.Text = "请先选中一个元素作为样式源"; return; }
    _formatPainterSource = src;
    _formatPainterActive = true;
    _statusText.Text = "格式刷已激活，请点击目标元素应用样式";
}

private void ApplyFormatToTarget(ReportElement target)
{
    if (_formatPainterSource == null || !_formatPainterActive) return;
    PushUndo();
    FormatPainterApplier.Apply(_formatPainterSource, target);
    MarkDirty();
    RefreshUI();
    _statusText.Text = "已应用格式刷";
}

private void StopFormatPainter()
{
    FormatPainterState.Reset(ref _formatPainterActive, ref _formatPainterSource);
}

    }
}
