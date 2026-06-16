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
private void SetupKeyBindings()
{
    InputBindings.Add(new KeyBinding(new RelayCmd(NewTemplate), Key.N, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(OpenTemplate), Key.O, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(() => SaveTemplate(false)), Key.S, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(Undo), Key.Z, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(Redo), Key.Y, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(CutSelected), Key.X, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(CopySelected), Key.C, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(PasteElement), Key.V, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(DeleteSelected), Key.Delete, ModifierKeys.None));
    InputBindings.Add(new KeyBinding(new RelayCmd(SelectAll), Key.A, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(DuplicateSelected), Key.D, ModifierKeys.Control));
    InputBindings.Add(new KeyBinding(new RelayCmd(() => ShortcutsDialog.Show(this)), Key.F1, ModifierKeys.None));
    InputBindings.Add(new KeyBinding(new RelayCmd(SearchElement), Key.F, ModifierKeys.Control));

    // 方向键微调
    PreviewKeyDown += OnPreviewKeyDown;
}

private void OnPreviewKeyDown(object sender, KeyEventArgs e)
{
    KeyboardInputRouter.Route(
        e, _template, _selectedBand, _selectedElement, _selectedElements,
        onTabSelected: (band, next) =>
        {
            _selectedElement = next;
            _selectedElements.Clear();
            _selectedElements.Add(_selectedElement);
            _selectedBand = band;
            RefreshUI();
        },
        onNudge: (dx, dy) => NudgeSelected(dx, dy));
}

private void NudgeSelected(double dx, double dy)
{
    PushUndo();
    ElementNudger.Nudge(NudgeRunner.ResolveTargets(_selectedElements, _selectedElement), dx, dy);
    MarkDirty();
    RefreshUI();
}

private void SwitchView(string mode, Border tabDesign, Border tabPreview)
{
    _viewMode = mode;
    ViewSwitcher.Switch(
        mode: mode,
        tabDesign: tabDesign,
        tabPreview: tabPreview,
        scrollViewer: _scrollViewer,
        hRuler: _hRuler,
        vRuler: _vRuler,
        previewScrollViewer: _previewScrollViewer,
        onPreviewRender: () => _previewRenderer.Render(_template!, _zoom, _previewData));
}

    }
}
