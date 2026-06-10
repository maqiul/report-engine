using System;
using System.Windows;

namespace ReportEngine.Designer.Wpf
{
    public class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.Run(new MainWindow());
        }
    }
}
