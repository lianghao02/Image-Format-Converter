using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PoliceImageToolkit;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException("DispatcherUnhandledException", e.Exception);
        MessageBox.Show($"應用程式發生未預期錯誤：\n\n{e.Exception.Message}\n\n詳細資訊已記錄至 crash.log", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException("CurrentDomain_UnhandledException", ex);
        }
    }

    private static void LogException(string source, Exception ex)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
            string content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}]\n{ex}\n\n";
            File.AppendAllText(logPath, content);
        }
        catch
        {
            // 忽略寫入日誌失敗
        }
    }
}
