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
private void InsertElement(ReportElement el)
{
    if (_template == null) return;
    var band = _selectedBand ?? _template.Bands.FirstOrDefault(b => b.Type == BandType.Detail) ?? _template.Bands.FirstOrDefault();
    if (band == null) return;
    PushUndo();
    band.Elements.Add(el);
    _selectedElement = el;
    _selectedBand = band;
    MarkDirty();
    RefreshUI();
}

private void InsertElementAt(ReportElement el, Band band, double mmX, double mmY)
{
    el.X = Math.Max(0, Math.Round(mmX * 2) / 2);
    el.Y = Math.Max(0, Math.Round(mmY * 2) / 2);
    PushUndo();
    band.Elements.Add(el);
    _selectedElement = el;
    _selectedBand = band;
    _selectedElements.Clear();
    _selectedElements.Add(el);
    MarkDirty();
    RefreshUI();
}

private void DeleteBand(Band band)
{
    if (_template == null) return;
    PushUndo();
    _template.Bands.Remove(band);
    BandDeleter.ClearSelection(ref _selectedBand, ref _selectedElement, _selectedElements);
    MarkDirty();
    RefreshUI();
}

private void AddBand(BandType type, double height)
{
    if (_template == null) return;
    PushUndo();
    _template.Bands.Add(new Band { Type = type, Height = height });
    MarkDirty();
    RefreshUI();
}

private void AlignElements(string mode)
{
    var targets = _selectedElements.Count > 1 ? _selectedElements : new List<ReportElement>();
    if (!ElementAligner.Align(targets, mode))
    {
        _statusText.Text = "请选中多个元素后再对齐";
        return;
    }
    PushUndo();
    MarkDirty();
    RefreshUI();
}

private void MoveElementOrder(string direction)
{
    if (_selectedElement == null || _selectedBand == null) return;
    if (!ZOrderHelper.Move(_selectedBand.Elements, _selectedElement, direction)) return;
    PushUndo();
    MarkDirty();
    RefreshUI();
}

    }
}
