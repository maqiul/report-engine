using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 数据源管理对话框 - 增删改数据源及其字段定义。
/// 等价抽离自 MainWindow.ShowDataSourceDialog()。
///
/// 纯 UI 责任: 所有"数据变更"通过 onChanged 回调传出, 由调用方(MainWindow)负责:
///   - PushUndo() (记录历史)
///   - MarkDirty() (标脏文件)
/// </summary>
public static class DataSourceDialog
{
    /// <summary>显示数据源管理窗口。如果 template 为 null, 直接返回。</summary>
    public static void Show(Window owner, ReportTemplate? template, Action onChanged)
    {
        if (template == null) return;

        var dlg = new Window
        {
            Title = "数据源管理",
            Width = 600,
            Height = 480,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            ResizeMode = ResizeMode.CanResize,
        };

        var mainGrid = new Grid { Margin = new Thickness(12) };
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        // 左侧：数据源列表
        var leftPanel = new DockPanel();
        var leftHeader = new TextBlock
        {
            Text = "数据源列表:",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 4),
        };
        leftPanel.Children.Add(leftHeader);
        DockPanel.SetDock(leftHeader, Dock.Top);

        var dsList = new ListBox { FontSize = 12, MinHeight = 300 };
        RefreshDsList(dsList, null, template);
        leftPanel.Children.Add(dsList);

        var dsBtnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 6, 0, 0),
        };
        var btnAddDs = new Button { Content = "+ 新增", Width = 60, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
        var btnDelDs = new Button { Content = "- 删除", Width = 60, Height = 26 };
        dsBtnPanel.Children.Add(btnAddDs);
        dsBtnPanel.Children.Add(btnDelDs);
        DockPanel.SetDock(dsBtnPanel, Dock.Bottom);
        leftPanel.Children.Add(dsBtnPanel);
        Grid.SetColumn(leftPanel, 0);
        mainGrid.Children.Add(leftPanel);

        // 右侧：字段列表
        var rightPanel = new DockPanel();
        var fieldHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
        var fieldHeaderText = new TextBlock
        {
            Text = "字段列表:",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
        };
        fieldHeader.Children.Add(fieldHeaderText);
        var selectedDsLabel = new TextBlock
        {
            Text = "",
            FontSize = 11,
            Foreground = Brushes.DimGray,
            Margin = new Thickness(8, 0, 0, 0),
        };
        DockPanel.SetDock(selectedDsLabel, Dock.Right);
        fieldHeader.Children.Add(selectedDsLabel);
        DockPanel.SetDock(fieldHeader, Dock.Top);
        rightPanel.Children.Add(fieldHeader);

        var fieldList = new ListBox { FontSize = 12, MinHeight = 280 };
        rightPanel.Children.Add(fieldList);

        var fieldBtnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 6, 0, 0),
        };
        var btnAddField = new Button { Content = "+ 添加", Width = 60, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
        var btnEditField = new Button { Content = "✎ 编辑", Width = 60, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
        var btnDelField = new Button { Content = "- 删除", Width = 60, Height = 26 };
        fieldBtnPanel.Children.Add(btnAddField);
        fieldBtnPanel.Children.Add(btnEditField);
        fieldBtnPanel.Children.Add(btnDelField);
        DockPanel.SetDock(fieldBtnPanel, Dock.Bottom);
        rightPanel.Children.Add(fieldBtnPanel);
        Grid.SetColumn(rightPanel, 2);
        mainGrid.Children.Add(rightPanel);

        // 事件：选中数据源
        dsList.SelectionChanged += (_, __) =>
        {
            if (dsList.SelectedItem is DataSourceDef ds)
            {
                selectedDsLabel.Text = ds.Name;
                RefreshFieldList(fieldList, ds);
            }
        };

        // 新增数据源
        btnAddDs.Click += (_, __) =>
        {
            var input = new InputDialog("新增数据源", "请输入数据源名称:", "DataSource" + (template.DataSources.Count + 1))
            {
                Owner = dlg,
            };
            if (input.ShowDialog() == true)
            {
                onChanged();
                template.DataSources.Add(new DataSourceDef { Name = input.Result });
                RefreshDsList(dsList, template.DataSources[template.DataSources.Count - 1], template);
            }
        };

        // 删除数据源
        btnDelDs.Click += (_, __) =>
        {
            if (dsList.SelectedItem is DataSourceDef ds)
            {
                onChanged();
                template.DataSources.Remove(ds);
                RefreshDsList(dsList, null, template);
                fieldList.Items.Clear();
                selectedDsLabel.Text = "";
            }
        };

        // 添加字段
        btnAddField.Click += (_, __) =>
        {
            if (dsList.SelectedItem is DataSourceDef ds)
            {
                var input = new FieldInputDialog("添加字段") { Owner = dlg };
                if (input.ShowDialog() == true)
                {
                    onChanged();
                    ds.Fields.Add(new FieldDef
                    {
                        Name = input.FieldName,
                        Type = input.FieldType,
                        Format = input.FieldFormat,
                    });
                    RefreshFieldList(fieldList, ds);
                }
            }
        };

        // 编辑字段
        btnEditField.Click += (_, __) =>
        {
            if (dsList.SelectedItem is DataSourceDef ds && fieldList.SelectedItem is FieldDef fd)
            {
                var input = new FieldInputDialog("编辑字段", fd.Name, fd.Type, fd.Format) { Owner = dlg };
                if (input.ShowDialog() == true)
                {
                    onChanged();
                    fd.Name = input.FieldName;
                    fd.Type = input.FieldType;
                    fd.Format = input.FieldFormat;
                    RefreshFieldList(fieldList, ds);
                }
            }
        };

        // 删除字段
        btnDelField.Click += (_, __) =>
        {
            if (dsList.SelectedItem is DataSourceDef ds && fieldList.SelectedItem is FieldDef fd)
            {
                onChanged();
                ds.Fields.Remove(fd);
                RefreshFieldList(fieldList, ds);
            }
        };

        // 关闭按钮
        var bottomPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
        };
        var btnClose = new Button { Content = "关闭", Width = 75, Height = 26, IsCancel = true };
        btnClose.Click += (_, __) => dlg.DialogResult = true;
        bottomPanel.Children.Add(btnClose);
        mainGrid.Children.Add(bottomPanel);

        dlg.Content = mainGrid;
        dlg.ShowDialog();
    }

    private static void RefreshDsList(ListBox list, DataSourceDef? select, ReportTemplate template)
    {
        list.Items.Clear();
        foreach (var ds in template.DataSources)
            list.Items.Add(ds);
        if (select != null) list.SelectedItem = select;
    }

    private static void RefreshFieldList(ListBox list, DataSourceDef ds)
    {
        list.Items.Clear();
        foreach (var f in ds.Fields)
            list.Items.Add(f);
    }
}