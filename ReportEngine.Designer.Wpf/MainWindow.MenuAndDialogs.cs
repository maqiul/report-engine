using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.UiFactory;
using static ReportEngine.Designer.Wpf.ElementFactory;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
private Menu BuildMenu()
{
    var menu = new Menu();
    var file = new MenuItem { Header = "文件(_F)" };
    file.Items.Add(MakeMenuItem("新建(_N)", "Ctrl+N", NewTemplate));
    file.Items.Add(MakeMenuItem("打开(_O)...", "Ctrl+O", OpenTemplate));
    file.Items.Add(new Separator());
    file.Items.Add(MakeMenuItem("保存(_S)", "Ctrl+S", () => SaveTemplate(false)));
    file.Items.Add(MakeMenuItem("另存为(_A)...", null, () => SaveTemplate(true)));
    file.Items.Add(new Separator());
    file.Items.Add(MakeMenuItem("页面设置(_P)...", null, ShowPageSetupDialog));
    file.Items.Add(new Separator());
    file.Items.Add(MakeMenuItem("导出PDF(_D)...", null, ExportPdf));
    file.Items.Add(MakeMenuItem("导出Excel(_E)...", null, ExportExcel));
    file.Items.Add(MakeMenuItem("导出图片(_I)...", null, ExportPng));
    file.Items.Add(MakeMenuItem("批量导出PDF+Excel(_B)...", null, ExportBatch));
    file.Items.Add(new Separator());
    file.Items.Add(MakeMenuItem("数据源(_S)...", null, OnShowDataSourceClicked));
    file.Items.Add(MakeMenuItem("数据绑定向导(_B)...", null, OnShowDataBindingWizardClicked));
    file.Items.Add(MakeMenuItem("模板参数(_P)...", null, OnShowTemplateParamsClicked));
    file.Items.Add(MakeMenuItem("加载预览数据(_L)...", null, OnLoadPreviewDataClicked));
    file.Items.Add(new Separator());
    BuildRecentFilesMenu(file);
    file.Items.Add(new Separator());
    file.Items.Add(MakeMenuItem("退出(_X)", "Alt+F4", Close));
    menu.Items.Add(file);

    var edit = new MenuItem { Header = "编辑(_E)" };
    edit.Items.Add(MakeMenuItem("撤销(_Z)", "Ctrl+Z", Undo));
    edit.Items.Add(MakeMenuItem("重做(_Y)", "Ctrl+Y", Redo));
    edit.Items.Add(new Separator());
    edit.Items.Add(MakeMenuItem("剪切(_X)", "Ctrl+X", CutSelected));
    edit.Items.Add(MakeMenuItem("复制(_C)", "Ctrl+C", CopySelected));
    edit.Items.Add(MakeMenuItem("粘贴(_V)", "Ctrl+V", PasteElement));
    edit.Items.Add(new Separator());
    edit.Items.Add(MakeMenuItem("删除", "Delete", DeleteSelected));
    edit.Items.Add(MakeMenuItem("全选", "Ctrl+A", SelectAll));
    menu.Items.Add(edit);

    var insert = new MenuItem { Header = "插入(_I)" };
    // 部件框子菜单
    var ctrlMenu = new MenuItem { Header = "部件框(_C)" };
    ctrlMenu.Items.Add(MakeMenuItem("静态框(_T)", null, () => InsertElement(NewText())));
    ctrlMenu.Items.Add(MakeMenuItem("字段框(_F)", null, () => InsertElement(NewFieldBox())));
    ctrlMenu.Items.Add(MakeMenuItem("统计框(_U)", null, () => InsertElement(NewSummaryBox())));
    ctrlMenu.Items.Add(MakeMenuItem("系统变量框(_V)", null, () => InsertElement(NewSysVarBox())));
    ctrlMenu.Items.Add(new Separator());
    ctrlMenu.Items.Add(MakeMenuItem("线段(_L)", null, () => InsertElement(NewLine())));
    ctrlMenu.Items.Add(MakeMenuItem("图形框(_S)", null, () => InsertElement(NewShape())));
    ctrlMenu.Items.Add(MakeMenuItem("图象框(_P)", null, () => InsertElement(NewImage())));
    ctrlMenu.Items.Add(MakeMenuItem("条形码&二维码(_B)", null, () => InsertElement(NewBarcode())));
    ctrlMenu.Items.Add(MakeMenuItem("表格(_G)", null, () => InsertElement(NewTable())));
    ctrlMenu.Items.Add(MakeMenuItem("交叉表", null, () => InsertElement(NewCrossTab())));
    ctrlMenu.Items.Add(MakeMenuItem("图表(_H)", null, () => InsertElement(NewChart())));
    ctrlMenu.Items.Add(MakeMenuItem("子报表(_E)", null, () => InsertElement(NewSubReport())));
    insert.Items.Add(ctrlMenu);
    // 报表节子菜单
    var sectionMenu = new MenuItem { Header = "报表节(_S)" };
    sectionMenu.Items.Add(MakeMenuItem("报表头", null, () => AddBand(BandType.ReportHeader, 20)));
    sectionMenu.Items.Add(MakeMenuItem("页眉", null, () => AddBand(BandType.Header, 15)));
    sectionMenu.Items.Add(MakeMenuItem("分组头", null, () => AddBand(BandType.GroupHeader, 12)));
    sectionMenu.Items.Add(MakeMenuItem("明细", null, () => AddBand(BandType.Detail, 10)));
    sectionMenu.Items.Add(MakeMenuItem("分组尾", null, () => AddBand(BandType.GroupFooter, 10)));
    sectionMenu.Items.Add(MakeMenuItem("页脚", null, () => AddBand(BandType.Footer, 10)));
    sectionMenu.Items.Add(MakeMenuItem("报表尾", null, () => AddBand(BandType.ReportFooter, 10)));
    insert.Items.Add(sectionMenu);
    insert.Items.Add(new Separator());
    insert.Items.Add(MakeMenuItem("页面设置(_P)...", null, ShowPageSetupDialog));
    menu.Items.Add(insert);

    var view = new MenuItem { Header = "视图(_V)" };
    view.Items.Add(MakeMenuItem("放大", "Ctrl++", () => { _zoomSlider.Value = Math.Min(400, _zoomSlider.Value + 25); }));
    view.Items.Add(MakeMenuItem("缩小", "Ctrl+-", () => { _zoomSlider.Value = Math.Max(25, _zoomSlider.Value - 25); }));
    view.Items.Add(MakeMenuItem("100%", null, () => { _zoomSlider.Value = 100; }));
    view.Items.Add(new Separator());
    var gridItem = new MenuItem { Header = "显示网格线(_G)", IsCheckable = true, IsChecked = _showGrid };
    gridItem.Click += (_, __) => { _showGrid = gridItem.IsChecked; _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand); };
    view.Items.Add(gridItem);
    view.Items.Add(MakeMenuItem("网格设置...", null, ShowGridSettingsDialog));
    view.Items.Add(new Separator());
    var miClearGuides = new MenuItem { Header = "清除所有参考线" };
    miClearGuides.Click += (_, __) => { _hGuides.Clear(); _vGuides.Clear(); _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand); _statusText.Text = "已清除所有参考线"; };
    view.Items.Add(miClearGuides);
    view.Items.Add(new Separator());
    var snapItem = new MenuItem { Header = "吸附对齐(_S)", IsCheckable = true, IsChecked = _snapEnabled };
    snapItem.Click += (_, __) => { _snapEnabled = snapItem.IsChecked; };
    view.Items.Add(snapItem);
    menu.Items.Add(view);

    var format = new MenuItem { Header = "格式(_O)" };
    format.Items.Add(MakeMenuItem("左对齐", null, () => AlignElements("left")));
    format.Items.Add(MakeMenuItem("右对齐", null, () => AlignElements("right")));
    format.Items.Add(MakeMenuItem("顶端对齐", null, () => AlignElements("top")));
    format.Items.Add(MakeMenuItem("底端对齐", null, () => AlignElements("bottom")));
    format.Items.Add(new Separator());
    format.Items.Add(MakeMenuItem("水平居中", null, () => AlignElements("hcenter")));
    format.Items.Add(MakeMenuItem("垂直居中", null, () => AlignElements("vcenter")));
    format.Items.Add(new Separator());
    format.Items.Add(MakeMenuItem("等宽", null, () => AlignElements("samewidth")));
    format.Items.Add(MakeMenuItem("等高", null, () => AlignElements("sameheight")));
    format.Items.Add(new Separator());
    format.Items.Add(MakeMenuItem("置于顶层", null, () => MoveElementOrder("front")));
    format.Items.Add(MakeMenuItem("置于底层", null, () => MoveElementOrder("back")));
    format.Items.Add(MakeMenuItem("上移一层", null, () => MoveElementOrder("up")));
    format.Items.Add(MakeMenuItem("下移一层", null, () => MoveElementOrder("down")));
    menu.Items.Add(format);

    var help = new MenuItem { Header = "帮助(_H)" };
    help.Items.Add(MakeMenuItem("快捷键...", "F1", () => ShortcutsDialog.Show(this)));
    help.Items.Add(MakeMenuItem("关于...", null, () => AboutDialog.Show(this)));
    menu.Items.Add(help);

    return menu;
}

internal void ShowPageSetupDialog()
{
    if (_template == null) return;
    PageSetupDialog.Show(this, _template, () => {
        PushUndo(); MarkDirty(); RefreshUI();
    });
}

private void OnLoadPreviewDataClicked()
{
    LoadPreviewDataDialog.Show(this,
        onLoaded: (parsed, fileName) =>
        {
            _previewData = parsed;
            _statusText.Text = "已加载预览数据: " + fileName + " (" + (_previewData?.Count ?? 0) + " 个字段)";
            if (_viewMode == "preview") _previewRenderer.Render(_template!, _zoom, _previewData);
        });
}

private void SearchElement()
{
    SearchElementDialog.Show(this, _template,
        onFound: (found, foundBand) =>
        {
            _selectedElement = found;
            _selectedBand = foundBand;
            _selectedElements.Clear();
            _selectedElements.Add(found);
            RefreshUI();
            _statusText.Text = "已找到: " + (found.Name ?? found.GetType().Name);
        },
        onNotFound: () => _statusText.Text = "未找到匹配的元素");
}

private void ShowGridSettingsDialog()
{
    GridSettingsDialog.Show(this, _gridSpacingMm,
        newSpacing =>
        {
            _gridSpacingMm = newSpacing;
            _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
        });
}

private void OnShowDataSourceClicked()
{
    DataSourceDialog.Show(this, _template, () => { PushUndo(); MarkDirty(); });
}

private void OnShowDataBindingWizardClicked()
{
    DataBindingWizardLauncher.Launch(this, _template, _selectedElement,
        setStatus: s => _statusText.Text = s,
        onCommit: (propChoice, expression) =>
        {
            if (_selectedElement == null) return;
            PushUndo();
            DataBindingApplier.Apply(_selectedElement, propChoice, expression);
            MarkDirty();
            RefreshUI();
            _statusText.Text = "已绑定: " + expression;
        });
}

private void OnShowTemplateParamsClicked()
{
    if (_template == null) return;
    TemplateParamsDialog.Show(this, _template,
        () => { PushUndo(); MarkDirty(); });
}

    }
}
