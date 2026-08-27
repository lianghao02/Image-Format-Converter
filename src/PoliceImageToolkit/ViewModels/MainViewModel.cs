using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using PoliceImageToolkit.Services;

namespace PoliceImageToolkit.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;
    private int _selectedTabIndex = 0;
    private string _currentVersionDisplay = "v11.2.0";
    private bool _isCheckingUpdate = false;

    public MainViewModel() : this(new ImageService(), new VideoService(), new LongScreenshotService(), new UpdateService())
    {
    }

    public MainViewModel(
        IImageService imageService,
        IVideoService videoService,
        ILongScreenshotService longScreenshotService,
        IUpdateService updateService)
    {
        _updateService = updateService;

        ImageConverter = new ImageConverterViewModel(imageService);
        VideoSnapshot = new VideoSnapshotViewModel(videoService);
        LongScreenshotSplit = new LongScreenshotSplitViewModel(longScreenshotService);

        CurrentVersionDisplay = _updateService.GetInstalledVersion();
        CheckUpdateCommand = new RelayCommand(async _ => await ExecuteCheckUpdateAsync(), _ => !IsCheckingUpdate);
    }

    public string AppTitle => $"警務影像轉檔與手機影片截圖工具箱 (Police-Image-Toolkit {CurrentVersionDisplay})";

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public string CurrentVersionDisplay
    {
        get => _currentVersionDisplay;
        set
        {
            if (SetProperty(ref _currentVersionDisplay, value))
            {
                OnPropertyChanged(nameof(AppTitle));
            }
        }
    }

    public bool IsCheckingUpdate
    {
        get => _isCheckingUpdate;
        set
        {
            if (SetProperty(ref _isCheckingUpdate, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ImageConverterViewModel ImageConverter { get; }
    public VideoSnapshotViewModel VideoSnapshot { get; }
    public LongScreenshotSplitViewModel LongScreenshotSplit { get; }
    public ICommand CheckUpdateCommand { get; }

    private async Task ExecuteCheckUpdateAsync()
    {
        if (IsCheckingUpdate) return;

        var consent = MessageBox.Show(
            "此操作會連線至 GitHub Release 查詢最新版本與更新說明。\n\n" +
            "不會上傳影片、影像、案號或任何本機檔案；程式平時不會自動檢查更新。\n\n" +
            "是否繼續？",
            "檢查更新前確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (consent != MessageBoxResult.Yes) return;

        IsCheckingUpdate = true;
        try
        {
            var result = await _updateService.CheckForUpdateAsync();

            switch (result.Status)
            {
                case UpdateStatus.Latest:
                    MessageBox.Show(
                        $"目前已是最新版本！\n\n當前版本：{result.CurrentVersion}",
                        "檢查更新",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;

                case UpdateStatus.UpdateAvailable:
                    string notes = string.IsNullOrWhiteSpace(result.ReleaseNotes) ? "（無詳細說明）" : result.ReleaseNotes.Trim();
                    // 若更新說明太長，適度截斷保持對話框精簡
                    if (notes.Length > 400)
                    {
                        notes = notes[..400] + "...\n(更多詳情請參閱 Release 頁面)";
                    }

                    string prompt = $"發現新版本：{result.LatestVersion}（目前版本：{result.CurrentVersion}）\n\n" +
                                   $"標題：{result.ReleaseTitle}\n\n" +
                                   $"更新重點：\n{notes}\n\n" +
                                   $"是否立即前往 GitHub 下載新版本？";

                    var choice = MessageBox.Show(
                        prompt,
                        "發現新版本 - 檢查更新",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (choice == MessageBoxResult.Yes)
                    {
                        string targetUrl = !string.IsNullOrWhiteSpace(result.DownloadUrl)
                            ? result.DownloadUrl
                            : result.ReleaseUrl;

                        Process.Start(new ProcessStartInfo(targetUrl) { UseShellExecute = true });
                    }
                    break;

                case UpdateStatus.NetworkError:
                    MessageBox.Show(
                        $"無法連線至 GitHub 伺服器檢查更新。\n\n詳細原因：{result.ErrorMessage}\n\n若處於公務內網封閉環境，請確認外網連線後再試。",
                        "檢查更新失敗",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;

                case UpdateStatus.Failed:
                default:
                    MessageBox.Show(
                        $"檢查更新時發生錯誤。\n\n詳細原因：{result.ErrorMessage}",
                        "檢查更新失敗",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"執行更新檢查時發生未預期例外：\n{ex.Message}",
                "檢查更新失敗",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }
}
