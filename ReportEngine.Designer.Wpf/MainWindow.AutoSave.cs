using System;
using System.IO;
using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
        private void SetupAutoSave()
        {
            _autoSaveTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            _autoSaveTimer.Tick += (_, __) =>
            {
                if (_dirty && _template != null)
                {
                    try
                    {
                        var json = _parser.Serialize(_template);
                        File.WriteAllText(AutoSavePath, json);
                    }
                    catch { }
                }
            };
            _autoSaveTimer.Start();
        }

        private void CheckAutoSaveRecovery()
        {
            if (!File.Exists(AutoSavePath)) return;
            try
            {
                var fi = new FileInfo(AutoSavePath);
                if (fi.Length < 10) return;
                var json = File.ReadAllText(AutoSavePath);
                var result = MessageBox.Show(this, "检测到未保存的自动备份（" + fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm") + "）。\n是否恢复？", "自动恢复", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _template = _parser.Parse(json);
                    _dirty = true;
                    RefreshUI();
                    _statusText.Text = "已从自动备份恢复";
                }
            }
            catch { }
        }

        private void ClearAutoSave()
        {
            try { if (File.Exists(AutoSavePath)) File.Delete(AutoSavePath); } catch { }
        }
    }
}
