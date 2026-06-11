using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 模板参数管理对话框 - 增删 TemplateParam (Name / Label / DefaultValue / Type)。
/// 等价抽离自 MainWindow.ShowTemplateParamsDialog()。
///
/// 任何变更 (新增/删除参数) 都会触发 onChanged 回调;MainWindow 负责 PushUndo / MarkDirty。
/// </summary>
public static class TemplateParamsDialog
{
    public static void Show(Window owner, ReportTemplate template, Action onChanged)
    {
        var dlg = new Window
        {
            Title = "模板参数管理",
            Width = 450,
            Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };

        var sp = new StackPanel { Margin = new Thickness(12) };

        sp.Children.Add(new TextBlock
        {
            Text = "模板参数列表（用于导出时变量替换）",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkBlue,
            Margin = new Thickness(0, 0, 0, 6),
        });

        var paramList = new ListBox { Height = 180, Margin = new Thickness(0, 0, 0, 8) };
        Action refreshList = () =>
        {
            paramList.Items.Clear();
            foreach (var p in template.Parameters)
                paramList.Items.Add($"{p.Label ?? p.Name} ({p.Name}) = {p.DefaultValue}");
        };
        refreshList();
        sp.Children.Add(paramList);

        // 添加/删除按钮
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 8),
        };
        var btnAdd = new Button { Content = "+ 添加", Width = 70, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
        var btnDel = new Button { Content = "- 删除", Width = 70, Height = 26 };
        btnPanel.Children.Add(btnAdd);
        btnPanel.Children.Add(btnDel);
        sp.Children.Add(btnPanel);

        // 添加参数
        btnAdd.Click += (_, __) =>
        {
            var inputDlg = new Window
            {
                Title = "添加参数",
                Width = 300,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = dlg,
                ResizeMode = ResizeMode.NoResize,
            };
            var inputSp = new StackPanel { Margin = new Thickness(12) };
            inputSp.Children.Add(new TextBlock { Text = "参数名称:", Margin = new Thickness(0, 0, 0, 4) });
            var tbName = new TextBox { Margin = new Thickness(0, 0, 0, 8) };
            inputSp.Children.Add(tbName);
            inputSp.Children.Add(new TextBlock { Text = "显示名称:", Margin = new Thickness(0, 0, 0, 4) });
            var tbLabel = new TextBox { Margin = new Thickness(0, 0, 0, 8) };
            inputSp.Children.Add(tbLabel);
            inputSp.Children.Add(new TextBlock { Text = "默认值:", Margin = new Thickness(0, 0, 0, 4) });
            var tbDefault = new TextBox { Margin = new Thickness(0, 0, 0, 12) };
            inputSp.Children.Add(tbDefault);
            var btnOk = new Button
            {
                Content = "确定",
                Width = 70,
                Height = 26,
                HorizontalAlignment = HorizontalAlignment.Right,
                IsDefault = true,
            };
            btnOk.Click += (_, ___) =>
            {
                if (!string.IsNullOrWhiteSpace(tbName.Text))
                {
                    template.Parameters.Add(new TemplateParam
                    {
                        Name = tbName.Text.Trim(),
                        Label = string.IsNullOrWhiteSpace(tbLabel.Text) ? null : tbLabel.Text.Trim(),
                        DefaultValue = tbDefault.Text,
                        Type = "string",
                    });
                    refreshList();
                    onChanged();
                    inputDlg.Close();
                }
            };
            inputSp.Children.Add(btnOk);
            inputDlg.Content = inputSp;
            inputDlg.ShowDialog();
        };

        // 删除参数
        btnDel.Click += (_, __) =>
        {
            if (paramList.SelectedIndex >= 0 && paramList.SelectedIndex < template.Parameters.Count)
            {
                template.Parameters.RemoveAt(paramList.SelectedIndex);
                refreshList();
                onChanged();
            }
        };

        // 关闭按钮
        var btnClose = new Button
        {
            Content = "关闭",
            Width = 70,
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Right,
            IsCancel = true,
        };
        btnClose.Click += (_, __) => dlg.Close();
        sp.Children.Add(btnClose);

        dlg.Content = sp;
        dlg.ShowDialog();
    }
}