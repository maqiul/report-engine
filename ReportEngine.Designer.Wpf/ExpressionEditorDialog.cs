using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf
{
    /// <summary>
    /// 表达式编辑器弹窗: 文本框 + 系统变量/聚合函数/字段引用快捷插入按钮
    /// (从 MainWindow.ShowExpressionEditor 抽离, 严格等价)
    /// </summary>
    internal static class ExpressionEditorDialog
    {
        public static void Show(Window owner, string currentValue, Action<string> onCommit)
        {
            var dlg = new Window
            {
                Title = "表达式编辑器", Width = 520, Height = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = owner,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                ResizeMode = ResizeMode.CanResize,
            };

            var mainPanel = new DockPanel { Margin = new Thickness(12) };

            // 顶部：表达式输入
            var topPanel = new StackPanel();
            topPanel.Children.Add(new TextBlock { Text = "表达式:", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            var exprBox = new TextBox { Text = currentValue, FontSize = 12, FontFamily = new FontFamily("Consolas, Courier New"), MinHeight = 28, Padding = new Thickness(4, 4, 4, 4), AcceptsReturn = true, MaxHeight = 60, TextWrapping = TextWrapping.Wrap };
            exprBox.SelectAll();
            topPanel.Children.Add(exprBox);
            DockPanel.SetDock(topPanel, Dock.Top);
            mainPanel.Children.Add(topPanel);

            // 下部：快捷插入区
            var contentGrid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 左侧：系统变量
            var sysPanel = new StackPanel();
            sysPanel.Children.Add(new TextBlock { Text = "▸ 系统变量:", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            foreach (var kv in new[] { ("{{PAGE}}", "当前页码"), ("{{TOTAL_PAGES}}", "总页数"), ("{{REPORT_DATE}}", "报表日期"), ("{{NOW}}", "当前时间"), ("{{ROW_NUMBER}}", "行号") })
                AddExprBtn(sysPanel, kv.Item1, kv.Item2, exprBox);
            Grid.SetColumn(sysPanel, 0);
            contentGrid.Children.Add(sysPanel);

            // 右侧：聚合函数
            var aggPanel = new StackPanel();
            aggPanel.Children.Add(new TextBlock { Text = "▸ 聚合函数:", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            foreach (var kv in new[] { ("{{SUM(field)}}", "求和"), ("{{COUNT(field)}}", "计数"), ("{{AVG(field)}}", "平均值"), ("{{MIN(field)}}", "最小值"), ("{{MAX(field)}}", "最大值") })
                AddExprBtn(aggPanel, kv.Item1, kv.Item2, exprBox);
            aggPanel.Children.Add(new TextBlock { Text = "\n▸ 字段引用:", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 4) });
            foreach (var kv in new[] { ("{{currentRow.fieldName}}", "当前行字段"), ("{{dsName.fieldName}}", "数据源字段") })
                AddExprBtn(aggPanel, kv.Item1, kv.Item2, exprBox);
            Grid.SetColumn(aggPanel, 2);
            contentGrid.Children.Add(aggPanel);

            mainPanel.Children.Add(contentGrid);

            // 底部提示
            var hint = new TextBlock { Text = "💡 提示: 用 {{ }} 包裹表达式，点击下方按钮快速插入模板", FontSize = 10, Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 0) };
            DockPanel.SetDock(hint, Dock.Bottom);
            mainPanel.Children.Add(hint);

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            var btnOk = new Button { Content = "确定", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            var btnCancel = new Button { Content = "取消", Width = 75, Height = 26, IsCancel = true };
            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnCancel);
            mainPanel.Children.Add(btnPanel);

            btnOk.Click += (_, __) => { onCommit(exprBox.Text); dlg.DialogResult = true; };

            dlg.Content = mainPanel;
            dlg.ShowDialog();
        }

        private static void AddExprBtn(StackPanel sp, string template, string desc, TextBox target)
        {
            var btn = new Button
            {
                Content = template + "  " + desc,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas, Courier New"),
                Padding = new Thickness(6, 3, 6, 3),
                Margin = new Thickness(0, 1, 0, 1),
                Background = new SolidColorBrush(Color.FromRgb(225, 225, 225)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                Foreground = Brushes.Black,
                Cursor = Cursors.Hand,
            };
            btn.Click += (_, __) =>
            {
                var p = template.Replace("field", "").Replace("fieldName", "").Replace("dsName", "");
                var idx = p.IndexOf("}}");
                var cursorPos = idx >= 0 ? idx : p.Length - 1;
                // 插入模板并选中变量名部分
                var selStart = target.SelectionStart;
                target.Text = target.Text.Insert(selStart, p);
                target.SelectionStart = selStart + cursorPos;
                target.SelectionLength = 0;
                target.Focus();
            };
            sp.Children.Add(btn);
        }
    }
}
