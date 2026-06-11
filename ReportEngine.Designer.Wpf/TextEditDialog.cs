using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 文本元素就地编辑弹窗 - 双击 TextElement 弹出文本框。
/// 等价抽离自 MainWindow.OnCanvasMouseDoubleClick()。
/// </summary>
public static class TextEditDialog
{
    /// <summary>
    /// 弹出文本编辑框, 用户点确定后调用 onCommit 写回 (接收新文本)。
    /// 返回 true 表示用户确认, false 表示取消/关闭。
    /// </summary>
    public static bool Show(
        Window owner,
        TextElement target,
        Action<string> onCommit)
    {
        var input = new Window
        {
            Title = "编辑文本",
            Width = 320,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };
        var tb = new TextBox
        {
            Text = target.Text ?? "",
            FontSize = 13,
            Margin = new Thickness(12),
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        var btnOk = new Button
        {
            Content = "确定",
            Width = 60,
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 12, 8),
            IsDefault = true,
        };
        btnOk.Click += (_, __) => { onCommit(tb.Text); input.DialogResult = true; };
        var sp = new StackPanel();
        sp.Children.Add(tb);
        sp.Children.Add(btnOk);
        input.Content = sp;
        return input.ShowDialog() == true;
    }
}