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
private bool ConfirmDiscard()
{
    if (!_dirty) return true;
    var r = MessageBox.Show("模板已修改，是否保存？", "确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
    if (r == MessageBoxResult.Cancel) return false;
    if (r == MessageBoxResult.Yes) SaveTemplate(false);
    return true;
}

private void AddRecentFile(string path)
{
    RecentFileStore.Add(_recentFiles, path, RecentFilesPath);
}

private void LoadRecentFiles()
{
    RecentFileLoader.Load(_recentFiles, RecentFilesPath, RecentFileStore.MaxCount);
}

private void BuildRecentFilesMenu(MenuItem parent)
{
    RecentFilesMenuBuilder.Build(parent, _recentFiles, OpenFileDirect);
}

private void OpenFileDirect(string path)
{
    if (!ConfirmDiscard()) return;
    try
    {
        var json = File.ReadAllText(path);
        _template = _parser.Parse(json);
        _currentFilePath = path;
        _dirty = false;
        _undoStack.Clear(); _redoStack.Clear();
        _selectedElement = null; _selectedBand = null;
        RefreshUI();
        _statusText.Text = "已打开: " + System.IO.Path.GetFileName(path);
        AddRecentFile(path);
    }
    catch (Exception ex)
    {
        MessageBox.Show("打开失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        _recentFiles.Remove(path);
    }
}

    }
}
