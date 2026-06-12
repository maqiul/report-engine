using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 左侧工具箱面板构造 - 把 BuildLeftPanel 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildLeftPanel() (49 行)。
///
/// 12 个"插入元素"按钮 + 7 个"插入区域"按钮，click handler 通过 19 个 callback 传出。
/// </summary>
internal static class LeftToolBoxBuilder
{
    public static DockPanel Build(
        Action onInsertText,
        Action onInsertFieldBox,
        Action onInsertSummaryBox,
        Action onInsertSysVarBox,
        Action onInsertLine,
        Action onInsertShape,
        Action onInsertImage,
        Action onInsertBarcode,
        Action onInsertTable,
        Action onInsertCrossTab,
        Action onInsertChart,
        Action onInsertSubReport,
        Action onAddHeader,
        Action onAddDetail,
        Action onAddFooter,
        Action onAddGroupHeader,
        Action onAddGroupFooter,
        Action onAddReportHeader,
        Action onAddReportFooter)
    {
        var panel = new DockPanel { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };

        // 标题
        var toolHeader = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            Padding = new Thickness(6, 3, 6, 3),
            Child = new TextBlock { Text = "插入元素", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, FontSize = 12 },
        };
        DockPanel.SetDock(toolHeader, Dock.Top);
        panel.Children.Add(toolHeader);

        // 插入工具按钮
        var toolbox = new StackPanel { Margin = new Thickness(4) };
        AddToolboxBtn(toolbox, "📝 静态框", onInsertText, "Text");
        AddToolboxBtn(toolbox, "📊 字段框", onInsertFieldBox, "Field");
        AddToolboxBtn(toolbox, "Σ 统计框", onInsertSummaryBox, "Summary");
        AddToolboxBtn(toolbox, "@ 系统变量框", onInsertSysVarBox, "SysVar");
        AddToolboxBtn(toolbox, "📏 线段", onInsertLine, "Line");
        AddToolboxBtn(toolbox, "▬ 图形框", onInsertShape, "Shape");
        AddToolboxBtn(toolbox, "🖼 图象框", onInsertImage, "Image");
        AddToolboxBtn(toolbox, "▦ 条形码&二维码", onInsertBarcode, "Barcode");
        AddToolboxBtn(toolbox, "▤ 表格", onInsertTable, "Table");
        AddToolboxBtn(toolbox, "⊞ 交叉表", onInsertCrossTab, "CrossTab");
        AddToolboxBtn(toolbox, "📈 图表", onInsertChart, "Chart");
        AddToolboxBtn(toolbox, "📄 子报表", onInsertSubReport, "SubReport");

        // 区域插入
        toolbox.Children.Add(new Border { Height = 8 });
        toolbox.Children.Add(new TextBlock { Text = "插入区域", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, Margin = new Thickness(0, 4, 0, 4), FontSize = 11 });
        AddToolboxBtn(toolbox, "▤ 页眉", onAddHeader);
        AddToolboxBtn(toolbox, "▤ 明细", onAddDetail);
        AddToolboxBtn(toolbox, "▤ 页脚", onAddFooter);
        AddToolboxBtn(toolbox, "▤ 分组头", onAddGroupHeader);
        AddToolboxBtn(toolbox, "▤ 分组尾", onAddGroupFooter);
        AddToolboxBtn(toolbox, "▤ 报表头", onAddReportHeader);
        AddToolboxBtn(toolbox, "▤ 报表尾", onAddReportFooter);

        var toolScroll = new ScrollViewer { Content = toolbox, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        panel.Children.Add(toolScroll);

        return panel;
    }
}