using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 页面设置弹窗：从 MainWindow 抽离的纯 WPF 弹窗工具。
/// 包含打印机选择 / 纸张类型 / 方向 / 边距 / 多联打印 (MultiUp) 设置。
/// 点击"确定"时直接修改传入的 template.Page 字段，然后调用 commit 回调
/// 让调用方统一处理 PushUndo + MarkDirty + RefreshUI。
/// </summary>
internal static class PageSetupDialog
{
    /// <summary>
    /// 弹出页面设置对话框。OK 后直接写回 template.Page 字段 + 调用 commit
    /// 回调；Cancel 则不做任何修改。
    /// </summary>
    /// <param name="owner">父窗口（居中和模态用）</param>
    /// <param name="template">要修改的模板（OK 时直接改 Page 字段）</param>
    /// <param name="commit">OK 后调用的回调（调用方做 PushUndo + MarkDirty + RefreshUI）</param>
    public static void Show(Window owner, ReportTemplate template, Action commit)
    {
        var dlg = new Window
        {
            Title = "页面设置",
            Width = 420,
            Height = 520,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
        };

        var stack = new StackPanel { Margin = new Thickness(16) };

        // ── 打印机选择 ──
        stack.Children.Add(new TextBlock { Text = "打印机", FontSize = 12, FontWeight = FontWeights.Bold });
        var printerCombo = new ComboBox { FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
        try
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                printerCombo.Items.Add(printer);
            var defPrinter = new System.Drawing.Printing.PrinterSettings().PrinterName;
            for (int i = 0; i < printerCombo.Items.Count; i++)
                if ((string)printerCombo.Items[i] == defPrinter) { printerCombo.SelectedIndex = i; break; }
        }
        catch { /* 无打印机环境 */ }
        stack.Children.Add(printerCombo);

        // 纸张类型预设
        var paperTypes = new[] { "A4 (210 × 297mm)", "A3 (297 × 420mm)", "A5 (148 × 210mm)", "B4 (250 × 353mm)", "B5 (176 × 250mm)", "Letter (216 × 279mm)", "Legal (216 × 356mm)", "(打印机默认纸张)", "自定义" };
        var paperSizes = new (double w, double h)[] { (210, 297), (297, 420), (148, 210), (250, 353), (176, 250), (216, 279), (216, 356), (0, 0), (0, 0) };
        // paperSizes[7] = 打印机默认纸张 (动态填充), paperSizes[8] = 自定义

        var paperCombo = new ComboBox { FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
        foreach (var pt in paperTypes) paperCombo.Items.Add(pt);

        var widthBox = new TextBox { Text = template.Page.Width.ToString(), FontSize = 12, Margin = new Thickness(0, 4, 0, 4), Padding = new Thickness(4, 2, 4, 2) };
        var heightBox = new TextBox { Text = template.Page.Height.ToString(), FontSize = 12, Margin = new Thickness(0, 4, 0, 8), Padding = new Thickness(4, 2, 4, 2) };

        var orientCombo = new ComboBox { FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
        orientCombo.Items.Add("纵向");
        orientCombo.Items.Add("横向");
        orientCombo.SelectedIndex = template.Page.Orientation == "landscape" ? 1 : 0;

        var topBox = new TextBox { Text = template.Page.Margin.Top.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var bottomBox = new TextBox { Text = template.Page.Margin.Bottom.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var leftBox = new TextBox { Text = template.Page.Margin.Left.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var rightBox = new TextBox { Text = template.Page.Margin.Right.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };

        // 尝试从默认打印机获取纸张尺寸
        void updatePrinterPaper()
        {
            try
            {
                var ps = new System.Drawing.Printing.PrinterSettings();
                if (ps.DefaultPageSettings.PaperSize != null && ps.DefaultPageSettings.PaperSize.Width > 0)
                {
                    // System.Drawing 用 0.01 英寸为单位，转 mm (1 inch = 25.4mm)
                    paperSizes[7] = (ps.DefaultPageSettings.PaperSize.Width / 100.0 * 25.4,
                                     ps.DefaultPageSettings.PaperSize.Height / 100.0 * 25.4);
                }
            }
            catch { }
        };
        updatePrinterPaper();

        void updateEditable()
        {
            bool custom = paperCombo.SelectedIndex == paperSizes.Length - 1;
            widthBox.IsReadOnly = !custom;
            heightBox.IsReadOnly = !custom;
            widthBox.Background = custom ? Brushes.White : new SolidColorBrush(Color.FromRgb(235, 235, 235));
            heightBox.Background = custom ? Brushes.White : new SolidColorBrush(Color.FromRgb(235, 235, 235));
        };

        paperCombo.SelectionChanged += (_, __) =>
        {
            int idx = paperCombo.SelectedIndex;
            if (idx == 7)
            {
                updatePrinterPaper();
                if (paperSizes[7].w > 0)
                {
                    bool landscape = orientCombo.SelectedIndex == 1;
                    widthBox.Text = (landscape ? paperSizes[7].h : paperSizes[7].w).ToString();
                    heightBox.Text = (landscape ? paperSizes[7].w : paperSizes[7].h).ToString();
                }
            }
            else if (idx >= 0 && idx < paperSizes.Length - 1)
            {
                bool landscape = orientCombo.SelectedIndex == 1;
                widthBox.Text = (landscape ? paperSizes[idx].h : paperSizes[idx].w).ToString();
                heightBox.Text = (landscape ? paperSizes[idx].w : paperSizes[idx].h).ToString();
            }
            updateEditable();
        };

        orientCombo.SelectionChanged += (_, __) =>
        {
            int idx = paperCombo.SelectedIndex;
            if (idx >= 0 && idx < paperSizes.Length - 1)
            {
                bool landscape = orientCombo.SelectedIndex == 1;
                widthBox.Text = (landscape ? paperSizes[idx].h : paperSizes[idx].w).ToString();
                heightBox.Text = (landscape ? paperSizes[idx].w : paperSizes[idx].h).ToString();
            }
        };

        updateEditable();

        stack.Children.Add(new TextBlock { Text = "纸张类型", FontSize = 12, FontWeight = FontWeights.Bold });
        stack.Children.Add(paperCombo);

        var sizeGrid = new Grid();
        sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var wPanel = new StackPanel();
        wPanel.Children.Add(new TextBlock { Text = "宽度(mm)", FontSize = 11 });
        wPanel.Children.Add(widthBox);
        Grid.SetColumn(wPanel, 0);
        sizeGrid.Children.Add(wPanel);
        var hPanel = new StackPanel();
        hPanel.Children.Add(new TextBlock { Text = "高度(mm)", FontSize = 11 });
        hPanel.Children.Add(heightBox);
        Grid.SetColumn(hPanel, 2);
        sizeGrid.Children.Add(hPanel);
        stack.Children.Add(sizeGrid);

        // 方向
        stack.Children.Add(new TextBlock { Text = "方向", FontSize = 12, FontWeight = FontWeights.Bold });
        stack.Children.Add(orientCombo);

        // 边距
        stack.Children.Add(new TextBlock { Text = "边距 (mm)", FontSize = 12, FontWeight = FontWeights.Bold });
        var marginGrid = new Grid();
        for (int i = 0; i < 4; i++)
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        if (marginGrid.ColumnDefinitions.Count > 1) { /* 4 列 */ }
        var mT = new StackPanel(); mT.Children.Add(new TextBlock { Text = "上", FontSize = 10 }); mT.Children.Add(topBox); Grid.SetColumn(mT, 0); marginGrid.Children.Add(mT);
        var mB = new StackPanel(); mB.Children.Add(new TextBlock { Text = "下", FontSize = 10 }); mB.Children.Add(bottomBox); Grid.SetColumn(mB, 1); marginGrid.Children.Add(mB);
        var mL = new StackPanel(); mL.Children.Add(new TextBlock { Text = "左", FontSize = 10 }); mL.Children.Add(leftBox); Grid.SetColumn(mL, 2); marginGrid.Children.Add(mL);
        var mR = new StackPanel(); mR.Children.Add(new TextBlock { Text = "右", FontSize = 10 }); mR.Children.Add(rightBox); Grid.SetColumn(mR, 3); marginGrid.Children.Add(mR);
        stack.Children.Add(marginGrid);

        // 多联打印
        var chkMultiUp = new CheckBox { Content = "启用多联打印 (一纸上印 N 份)", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 4) };
        chkMultiUp.IsChecked = template.Page.MultiUp != null;
        stack.Children.Add(chkMultiUp);

        var mu = template.Page.MultiUp;
        var muPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
        var muGrid = new Grid();
        for (int i = 0; i < 7; i++)
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var muRowsBox = new TextBox { Text = (mu?.Rows ?? 2).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var muColsBox = new TextBox { Text = (mu?.Columns ?? 2).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var muHSpBox = new TextBox { Text = (mu?.HSpacing ?? 0).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var muVSpBox = new TextBox { Text = (mu?.VSpacing ?? 0).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
        var pR = new StackPanel(); pR.Children.Add(new TextBlock { Text = "行数", FontSize = 10 }); pR.Children.Add(muRowsBox); Grid.SetColumn(pR, 0); muGrid.Children.Add(pR);
        var pC = new StackPanel(); pC.Children.Add(new TextBlock { Text = "列数", FontSize = 10 }); pC.Children.Add(muColsBox); Grid.SetColumn(pC, 1); muGrid.Children.Add(pC);
        var pH = new StackPanel(); pH.Children.Add(new TextBlock { Text = "水平间距", FontSize = 10 }); pH.Children.Add(muHSpBox); Grid.SetColumn(pH, 4); muGrid.Children.Add(pH);
        var pV = new StackPanel(); pV.Children.Add(new TextBlock { Text = "垂直间距", FontSize = 10 }); pV.Children.Add(muVSpBox); Grid.SetColumn(pV, 6); muGrid.Children.Add(pV);
        muPanel.Children.Add(muGrid);

        muPanel.Visibility = mu != null ? Visibility.Visible : Visibility.Collapsed;
        chkMultiUp.Checked += (_, __) => muPanel.Visibility = Visibility.Visible;
        chkMultiUp.Unchecked += (_, __) => muPanel.Visibility = Visibility.Collapsed;
        stack.Children.Add(muPanel);

        // 按钮
        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
        var btnOk = new Button { Content = "确定", Width = 80, Height = 28, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
        var btnCancel = new Button { Content = "取消", Width = 80, Height = 28, IsCancel = true };
        btnPanel.Children.Add(btnOk);
        btnPanel.Children.Add(btnCancel);
        stack.Children.Add(btnPanel);

        btnOk.Click += (_, __) =>
        {
            if (!double.TryParse(widthBox.Text, out var pw) || pw <= 0 ||
                !double.TryParse(heightBox.Text, out var ph) || ph <= 0)
            {
                MessageBox.Show("宽度和高度必须为正数", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            double.TryParse(topBox.Text, out var mt2);
            double.TryParse(bottomBox.Text, out var mb2);
            double.TryParse(leftBox.Text, out var ml2);
            double.TryParse(rightBox.Text, out var mr2);

            template.Page.Width = pw;
            template.Page.Height = ph;
            template.Page.Orientation = orientCombo.SelectedIndex == 1 ? "landscape" : "portrait";
            template.Page.Margin.Top = mt2;
            template.Page.Margin.Bottom = mb2;
            template.Page.Margin.Left = ml2;
            template.Page.Margin.Right = mr2;

            if (chkMultiUp.IsChecked == true)
            {
                int.TryParse(muRowsBox.Text, out var muR); if (muR < 1) muR = 1;
                int.TryParse(muColsBox.Text, out var muC); if (muC < 1) muC = 1;
                double.TryParse(muHSpBox.Text, out var muHS);
                double.TryParse(muVSpBox.Text, out var muVS);
                template.Page.MultiUp = new MultiUpConfig { Rows = muR, Columns = muC, HSpacing = muHS, VSpacing = muVS };
            }
            else
            {
                template.Page.MultiUp = null;
            }

            // 调用方负责 PushUndo / MarkDirty / RefreshUI
            commit();
            dlg.DialogResult = true;
        };

        dlg.Content = stack;
        dlg.ShowDialog();
    }
}
