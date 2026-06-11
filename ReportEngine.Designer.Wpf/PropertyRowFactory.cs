using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf
{
    /// <summary>
    /// 属性面板行构建 context。
    /// 封装 8 个 AddProp* 工厂方法 + 3 个 state 字段 (CurrentExpander / PropRowIndex / PropertyStack)。
    /// 每次 UpdatePropertyPanel 调用创建一次, using 自动 Dispose 重置 state。
    ///
    /// 从 MainWindow.AddProp* 系列抽离, 严格等价 (无行为变更, 无 bug 修复)。
    /// </summary>
    internal sealed class PropertyRowContext : IDisposable
    {
        public StackPanel PropertyStack { get; }
        public Expander? CurrentExpander { get; private set; }
        public int PropRowIndex { get; private set; }

        public PropertyRowContext(StackPanel propertyStack)
        {
            PropertyStack = propertyStack;
        }

        public void Dispose()
        {
            // 重置 per-call state, 防止下次 UpdatePropertyPanel 串味
            CurrentExpander = null;
            PropRowIndex = 0;
        }

        // ============================== Section ==============================

        public void AddSection(string title)
        {
            var content = new StackPanel();
            var expander = new Expander
            {
                Header = title,
                IsExpanded = true,
                Content = content,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 100, 140)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            PropertyStack.Children.Add(expander);
            CurrentExpander = expander;
        }

        private void AddToCurrentSection(UIElement element)
        {
            // 交替行背景色
            if (element is Grid g)
            {
                g.Background = PropRowIndex % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(245, 245, 245));
                PropRowIndex++;
            }
            if (CurrentExpander?.Content is StackPanel sp)
                sp.Children.Add(element);
            else
                PropertyStack.Children.Add(element);
        }

        // ============================== Rows ==============================

        public void AddLabel(string label, string value)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var vb = new TextBlock { Text = value, Foreground = Brushes.Black, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0) };
            // 分隔线
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(vb, 1);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(vb);
            AddToCurrentSection(grid);
        }

        public void AddEditor(Window owner, string label, string value, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var tb = new TextBox
            {
                Text = value, FontSize = 11,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.GotFocus += (_, __) => { tb.Background = Brushes.White; tb.BorderThickness = new Thickness(1); tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200)); };
            tb.LostFocus += (_, __) => { tb.Background = Brushes.Transparent; tb.BorderThickness = new Thickness(0); onCommit(tb.Text); };
            tb.KeyDown += (_, args) => { if (args.Key == Key.Enter) onCommit(tb.Text); };
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(tb, 1);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(tb);
            AddToCurrentSection(grid);
        }

        public void AddExpr(Window owner, string label, string value, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var tb = new TextBox
            {
                Text = value, FontSize = 11,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.GotFocus += (_, __) => { tb.Background = Brushes.White; tb.BorderThickness = new Thickness(1); tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200)); };
            tb.LostFocus += (_, __) => { tb.Background = Brushes.Transparent; tb.BorderThickness = new Thickness(0); onCommit(tb.Text); };
            tb.KeyDown += (_, args) => { if (args.Key == Key.Enter) onCommit(tb.Text); };
            // 表达式按钮
            var exprBtn = new Button
            {
                Content = "🧮", FontSize = 11, Width = 22, Height = 18,
                Padding = new Thickness(0), Background = Brushes.Transparent,
                BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                ToolTip = "打开表达式编辑器",
            };
            exprBtn.Click += (_, __) => ExpressionEditorDialog.Show(owner, tb.Text, v => { tb.Text = v; onCommit(v); });
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(tb, 1); Grid.SetColumn(exprBtn, 2);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(tb); grid.Children.Add(exprBtn);
            AddToCurrentSection(grid);
        }

        public void AddColor(Window owner, string label, string value, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var tb = new TextBox
            {
                Text = value ?? "", FontSize = 11,
                Background = Brushes.Transparent, Foreground = Brushes.Black,
                BorderThickness = new Thickness(0), Padding = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.GotFocus += (_, __) => { tb.Background = Brushes.White; tb.BorderThickness = new Thickness(1); tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200)); };
            tb.LostFocus += (_, __) => { tb.Background = Brushes.Transparent; tb.BorderThickness = new Thickness(0); onCommit(tb.Text); };
            tb.KeyDown += (_, args) => { if (args.Key == Key.Enter) onCommit(tb.Text); };

            // 颜色预览块 + 点击弹出选色器
            var colorPreview = new Border { Width = 16, Height = 16, CornerRadius = new CornerRadius(2), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Margin = new Thickness(2), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
            colorPreview.Background = BrushParser.Parse(value ?? "", Brushes.Transparent);
            colorPreview.MouseLeftButtonDown += (_, __) =>
            {
                var picked = ColorPickerDialog.Show(owner, value ?? "");
                if (picked != null)
                {
                    tb.Text = picked;
                    colorPreview.Background = BrushParser.Parse(picked, Brushes.Transparent);
                    onCommit(picked);
                }
            };

            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(tb, 1); Grid.SetColumn(colorPreview, 2);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(tb); grid.Children.Add(colorPreview);
            AddToCurrentSection(grid);
        }

        public void AddBool(string label, bool value, Action<bool> onCommit)
        {
            AddCombo(label, new[] { "是", "否" }, value ? "是" : "否", v => onCommit(v == "是"));
        }

        public void AddCombo(string label, string[] options, string current, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var cb = new ComboBox
            {
                FontSize = 11,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(2, 0, 2, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            foreach (var opt in options) cb.Items.Add(opt);
            cb.SelectedItem = current;
            cb.SelectionChanged += (_, __) => { if (cb.SelectedItem is string s) onCommit(s); };
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(cb, 1);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(cb);
            AddToCurrentSection(grid);
        }

        public void AddFontRow(Window owner, TextElement t)
        {
            string fontDesc = t.Font.Family + "(" + t.Font.Size + ")";
            if (t.Font.Bold) fontDesc += ",粗体";
            if (t.Font.Italic) fontDesc += ",斜体";

            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            var lb = new TextBlock { Text = "字体", Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var vb = new TextBlock { Text = fontDesc, Foreground = Brushes.Black, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
            var btn = new Button { Content = "...", Width = 22, Height = 18, FontSize = 10, Padding = new Thickness(0), Background = Brushes.Transparent, BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
            btn.Click += (_, __) => FontDialog.Show(owner, t);
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(vb, 1); Grid.SetColumn(btn, 2);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(vb); grid.Children.Add(btn);
            AddToCurrentSection(grid);
        }
    }
}
