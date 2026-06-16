using System;
using System.Windows.Input;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
        private class RelayCmd : ICommand
        {
            private readonly Action _exec;
            public RelayCmd(Action exec) { _exec = exec; }
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _exec();
            public event EventHandler? CanExecuteChanged { add { } remove { } }
        }
    }
}
