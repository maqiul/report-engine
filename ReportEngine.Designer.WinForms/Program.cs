using System;
using System.Windows.Forms;

namespace ReportEngine.Designer.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
#if NET8_0_OR_GREATER
            ApplicationConfiguration.Initialize();
#else
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Application.Run(new MainForm());
        }
    }
}
