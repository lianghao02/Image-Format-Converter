using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PoliceImageToolkit.ViewModels;

namespace PoliceImageToolkit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 若當前焦點在 TextBox，允許正常文字輸入，不攔截按鍵
        if (Keyboard.FocusedElement is TextBox)
        {
            return;
        }

        // 僅在影片截圖分頁時啟用快捷鍵
        if (TabVideo.IsChecked == true && ViewModel?.VideoSnapshot != null)
        {
            var vm = ViewModel.VideoSnapshot;

            switch (e.Key)
            {
                case Key.Space:
                    if (vm.CaptureSnapshotCommand.CanExecute(null))
                    {
                        vm.CaptureSnapshotCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                case Key.A:
                    if (vm.StepBackwardCommand.CanExecute(null))
                    {
                        vm.StepBackwardCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Right:
                case Key.D:
                    if (vm.StepForwardCommand.CanExecute(null))
                    {
                        vm.StepForwardCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Up:
                case Key.W:
                    if (vm.StepForward1sCommand.CanExecute(null))
                    {
                        vm.StepForward1sCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Down:
                case Key.S:
                    if (vm.StepBackward1sCommand.CanExecute(null))
                    {
                        vm.StepBackward1sCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Z:
                    if (vm.UndoLastCaptureCommand.CanExecute(null))
                    {
                        vm.UndoLastCaptureCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.P:
                case Key.Return:
                    if (vm.PlayPauseCommand.CanExecute(null))
                    {
                        vm.PlayPauseCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
            }
        }
    }

    private void TabImage_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewImageConverter != null && ViewVideoSnapshot != null && ViewLongScreenshotSplit != null)
        {
            ViewImageConverter.Visibility = Visibility.Visible;
            ViewVideoSnapshot.Visibility = Visibility.Collapsed;
            ViewLongScreenshotSplit.Visibility = Visibility.Collapsed;
        }
    }

    private void TabVideo_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewImageConverter != null && ViewVideoSnapshot != null && ViewLongScreenshotSplit != null)
        {
            ViewImageConverter.Visibility = Visibility.Collapsed;
            ViewVideoSnapshot.Visibility = Visibility.Visible;
            ViewLongScreenshotSplit.Visibility = Visibility.Collapsed;
        }
    }

    private void TabLongScreenshot_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewImageConverter != null && ViewVideoSnapshot != null && ViewLongScreenshotSplit != null)
        {
            ViewImageConverter.Visibility = Visibility.Collapsed;
            ViewVideoSnapshot.Visibility = Visibility.Collapsed;
            ViewLongScreenshotSplit.Visibility = Visibility.Visible;
        }
    }
}
