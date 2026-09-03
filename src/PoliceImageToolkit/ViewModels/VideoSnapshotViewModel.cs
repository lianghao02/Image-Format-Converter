using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PoliceImageToolkit.Models;
using PoliceImageToolkit.Services;

namespace PoliceImageToolkit.ViewModels;

public class VideoSnapshotViewModel : ViewModelBase
{
    private readonly IVideoService _videoService;
    private readonly IOutputIndexService _outputIndexService;

    private string _videoPath = string.Empty;
    private string _videoTitle = "尚未載入影片";
    private TimeSpan _duration = TimeSpan.Zero;
    private TimeSpan _currentPosition = TimeSpan.Zero;
    private bool _isPlaying = false;
    private bool _hasVideoLoaded = false;
    private string _statusMessage = "請拖曳手機影片 (MOV/MP4) 或點擊「開啟影片」";

    // 快照設定
    private string _outputDirectory = string.Empty;
    private string _outputFormat = "PNG";
    private int _jpgQuality = 95;
    private bool _addTimestampOverlay = false;
    private bool _includeMilliseconds = true;
    private bool _autoCreateSubfolder = true;
    private string _casePrefix = string.Empty;

    // 上頁末行對照圖
    private SnapshotResult? _lastCapturedSnapshot;
    private bool _isCapturing = false;

    public VideoSnapshotViewModel(IVideoService videoService, IOutputIndexService outputIndexService)
    {
        _videoService = videoService;
        _outputIndexService = outputIndexService;
        Snapshots = new ObservableCollection<SnapshotResult>();

        OpenVideoCommand = new RelayCommand(_ => ExecuteOpenVideo());
        PlayPauseCommand = new RelayCommand(_ => ExecutePlayPause(), _ => HasVideoLoaded);
        StepForwardCommand = new RelayCommand(_ => StepTime(0.1), _ => HasVideoLoaded);
        StepBackwardCommand = new RelayCommand(_ => StepTime(-0.1), _ => HasVideoLoaded);
        StepForward1sCommand = new RelayCommand(_ => StepTime(1.0), _ => HasVideoLoaded);
        StepBackward1sCommand = new RelayCommand(_ => StepTime(-1.0), _ => HasVideoLoaded);
        SeekCommand = new RelayCommand(ExecuteSeek, _ => HasVideoLoaded);
        BrowseOutputDirectoryCommand = new RelayCommand(_ => ExecuteBrowseOutputDirectory());
        OpenOutputFolderCommand = new RelayCommand(_ => ExecuteOpenOutputFolder());
        ClearSnapshotsCommand = new RelayCommand(_ => ExecuteClearSnapshots(), _ => Snapshots.Count > 0);
        DeleteSnapshotFilesCommand = new RelayCommand(async _ => await ExecuteDeleteSnapshotFilesAsync(), _ => Snapshots.Count > 0);
        UndoLastCaptureCommand = new RelayCommand(async _ => await ExecuteUndoLastCaptureAsync(), _ => Snapshots.Count > 0);
        CaptureSnapshotCommand = new RelayCommand(async _ => await CaptureSnapshotAsync(), _ => HasVideoLoaded && !_isCapturing);
    }

    public ObservableCollection<SnapshotResult> Snapshots { get; }

    public event Action? RequestPlay;
    public event Action? RequestPause;
    public event Action<TimeSpan>? RequestSeek;
    public event Func<BitmapSource?>? RequestCurrentFrame;
    public event Action? RequestSnapshotVisualFeedback; // 截圖視覺閃爍回饋

    public string VideoPath
    {
        get => _videoPath;
        set => SetProperty(ref _videoPath, value);
    }

    public string VideoTitle
    {
        get => _videoTitle;
        set => SetProperty(ref _videoTitle, value);
    }

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(DurationSeconds));
                OnPropertyChanged(nameof(FormattedDuration));
            }
        }
    }

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        set
        {
            if (SetProperty(ref _currentPosition, value))
            {
                OnPropertyChanged(nameof(CurrentPositionSeconds));
                OnPropertyChanged(nameof(FormattedPosition));
            }
        }
    }

    public double DurationSeconds => Duration.TotalSeconds;
    public double CurrentPositionSeconds
    {
        get => CurrentPosition.TotalSeconds;
        set
        {
            if (Math.Abs(CurrentPosition.TotalSeconds - value) > 0.03)
            {
                var ts = TimeSpan.FromSeconds(Math.Clamp(value, 0, DurationSeconds));
                RequestSeek?.Invoke(ts);
            }
        }
    }

    public string FormattedPosition => $"{(int)CurrentPosition.TotalHours:00}:{CurrentPosition.Minutes:00}:{CurrentPosition.Seconds:00}.{CurrentPosition.Milliseconds:000}";
    public string FormattedDuration => $"{(int)Duration.TotalHours:00}:{Duration.Minutes:00}:{Duration.Seconds:00}";

    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    public bool HasVideoLoaded
    {
        get => _hasVideoLoaded;
        set
        {
            if (SetProperty(ref _hasVideoLoaded, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    public string OutputFormat
    {
        get => _outputFormat;
        set => SetProperty(ref _outputFormat, value);
    }

    public int JpgQuality
    {
        get => _jpgQuality;
        set => SetProperty(ref _jpgQuality, value);
    }

    public bool AddTimestampOverlay
    {
        get => _addTimestampOverlay;
        set => SetProperty(ref _addTimestampOverlay, value);
    }

    public bool IncludeMilliseconds
    {
        get => _includeMilliseconds;
        set => SetProperty(ref _includeMilliseconds, value);
    }

    public bool AutoCreateSubfolder
    {
        get => _autoCreateSubfolder;
        set => SetProperty(ref _autoCreateSubfolder, value);
    }

    public string CasePrefix
    {
        get => _casePrefix;
        set => SetProperty(ref _casePrefix, value);
    }

    public SnapshotResult? LastCapturedSnapshot
    {
        get => _lastCapturedSnapshot;
        set
        {
            if (SetProperty(ref _lastCapturedSnapshot, value))
            {
                OnPropertyChanged(nameof(HasLastCapturedSnapshot));
            }
        }
    }

    public bool HasLastCapturedSnapshot => LastCapturedSnapshot != null;

    public ICommand OpenVideoCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand StepForwardCommand { get; }
    public ICommand StepBackwardCommand { get; }
    public ICommand StepForward1sCommand { get; }
    public ICommand StepBackward1sCommand { get; }
    public ICommand SeekCommand { get; }
    public ICommand BrowseOutputDirectoryCommand { get; }
    public ICommand OpenOutputFolderCommand { get; }
    public ICommand ClearSnapshotsCommand { get; }
    public ICommand DeleteSnapshotFilesCommand { get; }
    public ICommand UndoLastCaptureCommand { get; }
    public ICommand CaptureSnapshotCommand { get; }

    public void LoadVideo(string path)
    {
        if (!File.Exists(path)) return;

        VideoPath = path;
        VideoTitle = Path.GetFileName(path);
        HasVideoLoaded = true;
        StatusMessage = $"已載入影片：{VideoTitle} (按空白鍵 Space 秒截，←/→ 鍵微調)";
    }

    private void ExecuteOpenVideo()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "手機影片 (*.mov;*.mp4;*.m4v;*.avi;*.mkv)|*.mov;*.mp4;*.m4v;*.avi;*.mkv|所有檔案 (*.*)|*.*",
            Title = "選擇手機影片 (支援 iPhone MOV / Android MP4)"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadVideo(dialog.FileName);
        }
    }

    public void ExecutePlayPause()
    {
        if (!HasVideoLoaded) return;

        if (IsPlaying)
        {
            RequestPause?.Invoke();
            IsPlaying = false;
        }
        else
        {
            RequestPlay?.Invoke();
            IsPlaying = true;
        }
    }

    public void StepTime(double deltaSeconds)
    {
        if (!HasVideoLoaded) return;

        if (IsPlaying)
        {
            ExecutePlayPause();
        }

        var next = CurrentPosition + TimeSpan.FromSeconds(deltaSeconds);
        if (next < TimeSpan.Zero) next = TimeSpan.Zero;
        if (next > Duration) next = Duration;

        RequestSeek?.Invoke(next);
    }

    private void ExecuteSeek(object? param)
    {
        if (param is double seconds)
        {
            RequestSeek?.Invoke(TimeSpan.FromSeconds(seconds));
        }
    }

    public async Task<SnapshotResult?> CaptureSnapshotAsync()
    {
        if (!HasVideoLoaded || RequestCurrentFrame == null || _isCapturing) return null;

        _isCapturing = true;
        try
        {
            // 1. 立即擷取當前影格緩衝區
            var frame = RequestCurrentFrame.Invoke();
            if (frame == null)
            {
                StatusMessage = "截圖失敗：未能擷取畫面緩衝區。";
                return null;
            }

            // 2. 觸發介面視覺快門閃爍回饋
            RequestSnapshotVisualFeedback?.Invoke();

            var config = new VideoSnapshotConfig
            {
                OutputDirectory = OutputDirectory,
                OutputFormat = OutputFormat,
                JpgQuality = JpgQuality,
                AddTimestampOverlay = AddTimestampOverlay,
                IncludeMilliseconds = IncludeMilliseconds,
                // 每部影片固定輸出至自己的資料夾，流水號才可從 001 穩定排序。
                AutoCreateSubfolder = true,
                CasePrefix = CasePrefix
            };

            TimeSpan capturePos = CurrentPosition;

            // 3. 背景非同步存檔，不阻礙後續移動與播放
            var res = await _videoService.SaveFrameSnapshotAsync(frame, capturePos, VideoPath, config);
            string indexStatus;
            try
            {
                await _outputIndexService.AppendEntriesAsync(Path.GetDirectoryName(res.FilePath)!,
                [new OutputIndexEntry(
                    Path.GetFileName(res.FilePath),
                    Path.GetFileName(VideoPath),
                    "video_snapshot",
                    res.Timestamp.ToString("hh\\:mm\\:ss\\.fff"),
                    res.Width,
                    res.Height)]);
                indexStatus = "；追溯索引已更新";
            }
            catch
            {
                indexStatus = "；追溯索引未更新（截圖已保留）";
            }

            // 4. 更新 UI 與「上頁末行對照圖」
            App.Current.Dispatcher.Invoke(() =>
            {
                Snapshots.Insert(0, res);
                LastCapturedSnapshot = res;
                StatusMessage = $"📸 已擷取證物：{Path.GetFileName(res.FilePath)} (⏱ {res.Timestamp:hh\\:mm\\:ss\\.fff}){indexStatus} - 可繼續按 → 移動或 Space 截圖";
            });

            return res;
        }
        catch (Exception ex)
        {
            StatusMessage = $"截圖失敗: {ex.Message}";
            return null;
        }
        finally
        {
            _isCapturing = false;
        }
    }

    public async Task ExecuteUndoLastCaptureAsync()
    {
        if (Snapshots.Count == 0) return;

        var last = Snapshots[0];

        try
        {
            if (File.Exists(last.FilePath))
            {
                File.Delete(last.FilePath);
            }

            if (File.Exists(last.FilePath))
            {
                StatusMessage = $"無法刪除截圖，檔案仍保留於：{last.FilePath}";
                return;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"無法刪除截圖，清單已保留：{ex.Message}";
            return;
        }

        string indexStatus = string.Empty;
        try
        {
            await _outputIndexService.RemoveEntriesAsync(Path.GetDirectoryName(last.FilePath)!, [Path.GetFileName(last.FilePath)]);
        }
        catch
        {
            indexStatus = "；追溯索引未更新";
        }

        Snapshots.RemoveAt(0);
        LastCapturedSnapshot = Snapshots.FirstOrDefault();
        StatusMessage = $"已復原/刪除上一頁截圖：{Path.GetFileName(last.FilePath)}{indexStatus}";
    }

    private void ExecuteClearSnapshots()
    {
        int count = Snapshots.Count;
        Snapshots.Clear();
        LastCapturedSnapshot = null;
        StatusMessage = $"已清空本次截圖清單（保留磁碟上的 {count} 個檔案）。";
    }

    private async Task ExecuteDeleteSnapshotFilesAsync()
    {
        int requestedCount = Snapshots.Count;
        var choice = System.Windows.MessageBox.Show(
            $"確定要刪除本次清單中的 {requestedCount} 個截圖檔案嗎？\n\n" +
            "此動作不可復原，只會刪除目前清單中的衍生截圖，不會刪除來源影片。",
            "刪除本次截圖檔案",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (choice != System.Windows.MessageBoxResult.Yes) return;

        int deletedCount = 0;
        int missingCount = 0;
        int failedCount = 0;
        var removable = new List<SnapshotResult>();

        foreach (var snapshot in Snapshots)
        {
            try
            {
                if (File.Exists(snapshot.FilePath))
                {
                    File.Delete(snapshot.FilePath);
                    deletedCount++;
                }
                else
                {
                    missingCount++;
                }

                removable.Add(snapshot);
            }
            catch
            {
                failedCount++;
            }
        }

        foreach (var snapshot in removable) Snapshots.Remove(snapshot);
        LastCapturedSnapshot = Snapshots.FirstOrDefault();

        string indexStatus = string.Empty;
        try
        {
            foreach (var group in removable.GroupBy(snapshot => Path.GetDirectoryName(snapshot.FilePath)!))
            {
                await _outputIndexService.RemoveEntriesAsync(group.Key, group.Select(snapshot => Path.GetFileName(snapshot.FilePath)));
            }
        }
        catch
        {
            indexStatus = "；追溯索引未更新";
        }

        StatusMessage = failedCount == 0
            ? $"已刪除 {deletedCount} 個截圖檔案；另有 {missingCount} 個檔案原本不存在。{indexStatus}"
            : $"已刪除 {deletedCount} 個截圖檔案；{failedCount} 個檔案無法刪除並保留在清單中。{indexStatus}";
    }

    private void ExecuteBrowseOutputDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "選擇截圖儲存資料夾"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = dialog.FolderName;
        }
    }

    private void ExecuteOpenOutputFolder()
    {
        string? capturedDirectory = Snapshots.FirstOrDefault() is { } snapshot
            ? Path.GetDirectoryName(snapshot.FilePath)
            : null;
        string? configuredSnapshotDirectory = HasVideoLoaded && !string.IsNullOrWhiteSpace(OutputDirectory)
            ? Path.Combine(OutputDirectory, $"{Path.GetFileNameWithoutExtension(VideoPath)}_Snapshots")
            : null;
        string dir = capturedDirectory
            ?? configuredSnapshotDirectory
            ?? (HasVideoLoaded ? Path.Combine(Path.GetDirectoryName(VideoPath) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(VideoPath)}_Snapshots") : null)
            ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if (Directory.Exists(dir))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
        }
    }
}
