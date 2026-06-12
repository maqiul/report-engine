using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Canvas 鼠标按下处理 - 把 OnCanvasMouseDown 元素/框选分支抽离。
/// 等价抽离自 MainWindow.OnCanvasMouseDown() line 833-880 (47 行)。
///
/// 3 种 hit 结果:
///   1. element 命中: Ctrl+Click 切换多选; 否则单选并启动 MoveElement
///   2. band 命中: 清选中, 设 _selectedBand
///   3. 空白: 启动框选
///
/// 格式刷优先: 命中元素 + 格式刷激活时只调用 onApplyFormat + onStopPainter, 跳过其他
///
/// 副作用走 callback + out:
///   - onStartMove: 启动 MoveElement (PushUndo + CaptureMouse + 记起始坐标)
///   - onStartMarquee: 启动框选 (CaptureMouse + 记 _dragStart)
///   - onApplyFormat / onStopPainter: 格式刷
///   - out band/element/selected: 写出命中
/// </summary>
internal static class CanvasMouseDownHandler
{
    public static void Handle(
        UIElement canvas,
        Point pos,
        ReportTemplate template,
        double zoom,
        double canvasPadding,
        double pixelsPerMm,
        List<ReportElement> selectedElements,
        bool formatPainterActive,
        Action onApplyFormat,
        Action onStopPainter,
        Action<ReportElement> onStartMove,
        Action onStartMarquee,
        out Band? hitBand,
        out ReportElement? hitElement)
    {
        var (band, element) = HitTester.Hit(pos, template, zoom, canvasPadding, pixelsPerMm);
        hitBand = band;
        hitElement = element;

        // 格式刷模式：点击元素应用格式
        if (element != null && formatPainterActive)
        {
            onApplyFormat();
            onStopPainter();
            return;
        }

        if (element != null)
        {
            // Ctrl+Click 多选
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (selectedElements.Contains(element))
                    selectedElements.Remove(element);
                else
                    selectedElements.Add(element);
            }
            else
            {
                selectedElements.Clear();
                selectedElements.Add(element);
                if (!element.Locked)
                    onStartMove(element);
            }
        }
        else if (band != null)
        {
            selectedElements.Clear();
        }
        else
        {
            // 空白区域开始框选
            selectedElements.Clear();
            onStartMarquee();
        }
    }
}
