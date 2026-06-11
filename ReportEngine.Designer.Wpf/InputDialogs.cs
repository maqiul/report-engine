using System.Windows;
using System.Windows.Controls;

namespace ReportEngine.Designer.Wpf;

/// <summary>通用单行文本输入对话框 (从 MainWindow.InputDialog 抽离)。</summary>
public class InputDialog : Window
{
    public string Result { get; private set; } = "";

    public InputDialog(string title, string prompt, string defaultText = "")
    {
        Title = title;
        Width = 360;
        Height = 160;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var sp = new StackPanel { Margin = new Thickness(12) };
        sp.Children.Add(new TextBlock
        {
            Text = prompt,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 6),
        });

        var tb = new TextBox
        {
            Text = defaultText,
            FontSize = 12,
            Padding = new Thickness(4, 3, 4, 3),
        };
        tb.SelectAll();
        tb.Focus();
        sp.Children.Add(tb);

        var btnRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
        };
        var btnOk = new Button
        {
            Content = "确定",
            Width = 60,
            Height = 26,
            Margin = new Thickness(0, 0, 8, 0),
            IsDefault = true,
        };
        var btnCancel = new Button
        {
            Content = "取消",
            Width = 60,
            Height = 26,
            IsCancel = true,
        };
        btnOk.Click += (_, __) => { Result = tb.Text; DialogResult = true; };
        btnCancel.Click += (_, __) => DialogResult = false;
        btnRow.Children.Add(btnOk);
        btnRow.Children.Add(btnCancel);
        sp.Children.Add(btnRow);

        Content = sp;
    }
}

/// <summary>字段定义输入对话框 (从 MainWindow.FieldInputDialog 抽离)。</summary>
public class FieldInputDialog : Window
{
    public string FieldName { get; private set; } = "";
    public string FieldType { get; private set; } = "string";
    public string? FieldFormat { get; private set; }

    public FieldInputDialog(string title, string name = "", string type = "string", string? format = null)
    {
        Title = title;
        Width = 380;
        Height = 260;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var sp = new StackPanel { Margin = new Thickness(12) };

        sp.Children.Add(new TextBlock
        {
            Text = "字段名:",
            FontSize = 11,
            Margin = new Thickness(0, 0, 0, 2),
        });
        var nameBox = new TextBox
        {
            Text = name,
            FontSize = 12,
            Padding = new Thickness(4, 3, 4, 3),
        };
        sp.Children.Add(nameBox);

        sp.Children.Add(new TextBlock
        {
            Text = "类型:",
            FontSize = 11,
            Margin = new Thickness(0, 6, 0, 2),
        });
        var typeCombo = new ComboBox
        {
            FontSize = 12,
            Padding = new Thickness(4, 3, 4, 3),
        };
        foreach (var t in new[] { "string", "int", "decimal", "double", "DateTime", "bool" })
            typeCombo.Items.Add(t);
        typeCombo.SelectedItem = type;
        sp.Children.Add(typeCombo);

        sp.Children.Add(new TextBlock
        {
            Text = "格式 (可选):",
            FontSize = 11,
            Margin = new Thickness(0, 6, 0, 2),
        });
        var fmtBox = new TextBox
        {
            Text = format ?? "",
            FontSize = 12,
            Padding = new Thickness(4, 3, 4, 3),
        };
        sp.Children.Add(fmtBox);

        var btnRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
        };
        var btnOk = new Button
        {
            Content = "确定",
            Width = 60,
            Height = 26,
            Margin = new Thickness(0, 0, 8, 0),
            IsDefault = true,
        };
        var btnCancel = new Button
        {
            Content = "取消",
            Width = 60,
            Height = 26,
            IsCancel = true,
        };
        btnOk.Click += (_, __) =>
        {
            FieldName = nameBox.Text;
            FieldType = (string?)typeCombo.SelectedItem ?? "string";
            FieldFormat = string.IsNullOrWhiteSpace(fmtBox.Text) ? null : fmtBox.Text;
            DialogResult = true;
        };
        btnCancel.Click += (_, __) => DialogResult = false;
        btnRow.Children.Add(btnOk);
        btnRow.Children.Add(btnCancel);
        sp.Children.Add(btnRow);

        Content = sp;
    }
}