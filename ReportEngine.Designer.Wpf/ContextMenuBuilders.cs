using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 右键菜单"插入"子菜单构造 - 抽出 13 种元素类型菜单项。
/// 等价抽离自 MainWindow.OnCanvasRightClick() "插入" 子菜单 (15 行)。
/// </summary>
internal static class InsertElementMenuBuilder
{
    public static MenuItem Build(Action<ReportElement> onInsert,
                                 Func<ReportElement> staticText,
                                 Func<ReportElement> field,
                                 Func<ReportElement> summary,
                                 Func<ReportElement> sysVar,
                                 Func<ReportElement> line,
                                 Func<ReportElement> shape,
                                 Func<ReportElement> image,
                                 Func<ReportElement> barcode,
                                 Func<ReportElement> table,
                                 Func<ReportElement> crossTab,
                                 Func<ReportElement> chart,
                                 Func<ReportElement> subReport)
    {
        var mi = new MenuItem { Header = "插入" };
        mi.Items.Add(Make("静态框", () => onInsert(staticText())));
        mi.Items.Add(Make("字段框", () => onInsert(field())));
        mi.Items.Add(Make("统计框", () => onInsert(summary())));
        mi.Items.Add(Make("系统变量框", () => onInsert(sysVar())));
        mi.Items.Add(new Separator());
        mi.Items.Add(Make("线段", () => onInsert(line())));
        mi.Items.Add(Make("图形框", () => onInsert(shape())));
        mi.Items.Add(Make("图象框", () => onInsert(image())));
        mi.Items.Add(Make("条形码&二维码", () => onInsert(barcode())));
        mi.Items.Add(Make("表格", () => onInsert(table())));
        mi.Items.Add(Make("交叉表", () => onInsert(crossTab())));
        mi.Items.Add(Make("图表", () => onInsert(chart())));
        mi.Items.Add(Make("子报表", () => onInsert(subReport())));
        return mi;
    }

    private static MenuItem Make(string header, Action onClick)
    {
        var mi = new MenuItem { Header = header };
        mi.Click += (_, __) => onClick();
        return mi;
    }
}

/// <summary>
/// 右键菜单"插入区域"子菜单构造 - 抽出 3 种 band 类型。
/// 等价抽离自 MainWindow.OnCanvasRightClick() "插入区域" 子菜单 (5 行)。
/// </summary>
internal static class InsertBandMenuBuilder
{
    public static MenuItem Build(Action<BandType, double> onAddBand)
    {
        var mi = new MenuItem { Header = "插入区域" };
        mi.Items.Add(Make("页眉", () => onAddBand(BandType.Header, 15)));
        mi.Items.Add(Make("明细", () => onAddBand(BandType.Detail, 10)));
        mi.Items.Add(Make("页脚", () => onAddBand(BandType.Footer, 10)));
        mi.Items.Add(Make("分组头", () => onAddBand(BandType.GroupHeader, 12)));
        mi.Items.Add(Make("分组尾", () => onAddBand(BandType.GroupFooter, 10)));
        mi.Items.Add(Make("报表头", () => onAddBand(BandType.ReportHeader, 20)));
        mi.Items.Add(Make("报表尾", () => onAddBand(BandType.ReportFooter, 10)));
        return mi;
    }

    private static MenuItem Make(string header, Action onClick)
    {
        var mi = new MenuItem { Header = header };
        mi.Click += (_, __) => onClick();
        return mi;
    }
}
