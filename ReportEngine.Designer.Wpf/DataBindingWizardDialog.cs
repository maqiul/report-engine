using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 数据绑定向导 - 引导用户选择数据源+字段+目标属性, 生成 [dsName.fieldName] 表达式。
/// 等价抽离自 MainWindow.ShowDataBindingWizard()。
///
/// 副作用全部走 onBound 回调;MainWindow 负责 PushUndo / MarkDirty / RefreshUI。
/// </summary>
public static class DataBindingWizardDialog
{
    /// <summary>
    /// 显示向导。onBound 接收 (dsName, fieldName, propChoice, expression):
    ///   dsName/fieldName - 用户选择的字段
    ///   propChoice       - "文本内容 (Text)" 或 "可见性表达式 (VisibleExpression)"
    ///   expression       - 已生成的 "[dsName.fieldName]" 表达式串
    /// </summary>
    public static void Show(
        Window owner,
        ReportTemplate template,
        ReportElement selectedElement,
        Action<string, string, string, string> onBound)
    {
        var dlg = new Window
        {
            Title = "数据绑定向导",
            Width = 420,
            Height = 380,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };

        var sp = new StackPanel { Margin = new Thickness(12) };

        // 步骤1: 选择数据源
        sp.Children.Add(new TextBlock
        {
            Text = "步骤1: 选择数据源",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkBlue,
            Margin = new Thickness(0, 0, 0, 6),
        });
        var dsCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 12) };
        foreach (var ds in template.DataSources)
            dsCombo.Items.Add(ds.Name);
        dsCombo.SelectedIndex = 0;
        sp.Children.Add(dsCombo);

        // 步骤2: 选择字段
        sp.Children.Add(new TextBlock
        {
            Text = "步骤2: 选择要绑定的字段",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkBlue,
            Margin = new Thickness(0, 8, 0, 6),
        });
        var fieldList = new ListBox { Height = 150, Margin = new Thickness(0, 0, 0, 12) };
        Action refreshFields = () =>
        {
            fieldList.Items.Clear();
            var ds = template.DataSources.FirstOrDefault(d => d.Name == dsCombo.SelectedItem?.ToString());
            if (ds != null)
                foreach (var f in ds.Fields)
                    fieldList.Items.Add(f.Name);
        };
        dsCombo.SelectionChanged += (_, __) => refreshFields();
        refreshFields();
        sp.Children.Add(fieldList);

        // 步骤3: 绑定到元素的哪个属性
        sp.Children.Add(new TextBlock
        {
            Text = "步骤3: 绑定到元素的属性",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkBlue,
            Margin = new Thickness(0, 8, 0, 6),
        });
        var propCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 16) };
        propCombo.Items.Add("文本内容 (Text)");
        propCombo.Items.Add("可见性表达式 (VisibleExpression)");
        propCombo.SelectedIndex = 0;
        sp.Children.Add(propCombo);

        // 按钮
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        var btnCancel = new Button
        {
            Content = "取消",
            Width = 70,
            Height = 26,
            Margin = new Thickness(0, 0, 8, 0),
            IsCancel = true,
        };
        btnCancel.Click += (_, __) => dlg.Close();
        btnPanel.Children.Add(btnCancel);

        var btnBind = new Button
        {
            Content = "绑定",
            Width = 70,
            Height = 26,
            IsDefault = true,
        };
        btnBind.Click += (_, __) =>
        {
            var dsName = dsCombo.SelectedItem?.ToString();
            var fieldName = fieldList.SelectedItem?.ToString();
            var propChoice = propCombo.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(dsName) || string.IsNullOrEmpty(fieldName))
            {
                MessageBox.Show("请选择数据源和字段", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var expression = $"[{dsName}.{fieldName}]";
            onBound(dsName ?? "", fieldName ?? "", propChoice ?? "", expression);
            dlg.Close();
        };
        btnPanel.Children.Add(btnBind);
        sp.Children.Add(btnPanel);

        dlg.Content = sp;
        dlg.ShowDialog();
    }
}