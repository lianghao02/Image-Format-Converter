using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using PoliceImageToolkit.Models;
using PoliceImageToolkit.Services;
using TaskStatus = PoliceImageToolkit.Models.TaskStatus;

namespace PoliceImageToolkit.ViewModels;

public class ImageConverterViewModel : ViewModelBase
{
    private readonly IImageService _imageService;
    private CancellationTokenSource? _cts;

    private string _targetFormat = "JPG";
    private int _jpgQuality = 90;
    private bool _autoOrient = true;
    private int _maxDimension = 0;
    private string _outputDirectory = string.Empty;
    private bool _useCustomOutputDirectory = false;
    private bool _isProcessing = false;
    private double _overallProgress = 0;
    private string _summaryText = "請拖曳圖片或點擊「新增檔案」開始";

    public ImageConverterViewModel(IImageService imageService)
    {
        _imageService = imageService;
        Tasks = new ObservableCollection<ImageTaskItem>();

        AddFilesCommand = new RelayCommand(_ => ExecuteAddFiles(), _ => !IsProcessing);
        RemoveSelectedCommand = new RelayCommand(ExecuteRemoveSelected, _ => !IsProcessing);
        ClearAllCommand = new RelayCommand(_ => ExecuteClearAll(), _ => !IsProcessing && Tasks.Count > 0);
        StartConvertCommand = new RelayCommand(async _ => await ExecuteStartConvertAsync(), _ => !IsProcessing && Tasks.Count > 0);
        CancelConvertCommand = new RelayCommand(_ => ExecuteCancelConvert(), _ => IsProcessing);
        BrowseOutputDirectoryCommand = new RelayCommand(_ => ExecuteBrowseOutputDirectory());
        OpenOutputDirectoryCommand = new RelayCommand(_ => ExecuteOpenOutputDirectory());
    }

    public ObservableCollection<ImageTaskItem> Tasks { get; }

    public string[] AvailableFormats { get; } = ["JPG", "PNG", "BMP", "TIFF"];

    public string TargetFormat
    {
        get => _targetFormat;
        set => SetProperty(ref _targetFormat, value);
    }

    public int JpgQuality
    {
        get => _jpgQuality;
        set => SetProperty(ref _jpgQuality, value);
    }

    public bool AutoOrient
    {
        get => _autoOrient;
        set => SetProperty(ref _autoOrient, value);
    }

    public int MaxDimension
    {
        get => _maxDimension;
        set => SetProperty(ref _maxDimension, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    public bool UseCustomOutputDirectory
    {
        get => _useCustomOutputDirectory;
        set => SetProperty(ref _useCustomOutputDirectory, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (SetProperty(ref _isProcessing, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public double OverallProgress
    {
        get => _overallProgress;
        set => SetProperty(ref _overallProgress, value);
    }

    public string SummaryText
    {
        get => _summaryText;
        set => SetProperty(ref _summaryText, value);
    }

    public ICommand AddFilesCommand { get; }
    public ICommand RemoveSelectedCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand StartConvertCommand { get; }
    public ICommand CancelConvertCommand { get; }
    public ICommand BrowseOutputDirectoryCommand { get; }
    public ICommand OpenOutputDirectoryCommand { get; }

    public void AddFiles(IEnumerable<string> filePaths)
    {
        int added = 0;
        foreach (var path in filePaths)
        {
            if (!File.Exists(path)) continue;

            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext is ".jpg" or ".jpeg" or ".png" or ".heic" or ".heif" or ".webp" or ".bmp" or ".tif" or ".tiff" or ".gif")
            {
                // 避免重複
                if (Tasks.Any(t => t.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var fi = new FileInfo(path);
                Tasks.Add(new ImageTaskItem
                {
                    FilePath = path,
                    FileName = fi.Name,
                    SourceFormat = ext.TrimStart('.').ToUpperInvariant(),
                    SourceSizeBytes = fi.Length
                });
                added++;
            }
        }

        UpdateSummary();
    }

    private void ExecuteAddFiles()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "支援影像格式 (*.jpg;*.jpeg;*.png;*.heic;*.heif;*.webp;*.bmp;*.tiff)|*.jpg;*.jpeg;*.png;*.heic;*.heif;*.webp;*.bmp;*.tif;*.tiff|所有檔案 (*.*)|*.*",
            Title = "選擇要轉檔的手機圖片"
        };

        if (dialog.ShowDialog() == true)
        {
            AddFiles(dialog.FileNames);
        }
    }

    private void ExecuteRemoveSelected(object? parameter)
    {
        if (parameter is ImageTaskItem item)
        {
            Tasks.Remove(item);
            UpdateSummary();
        }
    }

    private void ExecuteClearAll()
    {
        Tasks.Clear();
        OverallProgress = 0;
        UpdateSummary();
    }

    private async Task ExecuteStartConvertAsync()
    {
        if (Tasks.Count == 0 || IsProcessing) return;

        IsProcessing = true;
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        int total = Tasks.Count;
        int completed = 0;
        OverallProgress = 0;
        SummaryText = $"正在批次轉檔中 (0 / {total})...";

        var sw = Stopwatch.StartNew();

        try
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(Tasks, parallelOptions, async (task, token) =>
            {
                string targetDir = UseCustomOutputDirectory && !string.IsNullOrWhiteSpace(OutputDirectory)
                    ? OutputDirectory
                    : Path.Combine(Path.GetDirectoryName(task.FilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");

                var options = new ImageConvertOptions(
                    TargetFormat,
                    JpgQuality,
                    targetDir,
                    AutoOrient,
                    MaxDimension
                );

                await _imageService.ConvertAsync(task, options, token);

                int done = Interlocked.Increment(ref completed);
                double pct = (double)done / total * 100;

                App.Current.Dispatcher.Invoke(() =>
                {
                    OverallProgress = pct;
                    SummaryText = $"正在批次轉檔中 ({done} / {total}) - {pct:0}%";
                });
            });

            sw.Stop();
            int successCount = Tasks.Count(t => t.Status == TaskStatus.Success);
            SummaryText = $"轉檔完成！成功: {successCount} 筆，失敗: {total - successCount} 筆 (總耗時: {sw.Elapsed.TotalSeconds:0.##} 秒)";
        }
        catch (OperationCanceledException)
        {
            SummaryText = "已取消轉檔作業。";
        }
        catch (Exception ex)
        {
            SummaryText = $"轉檔過程發生例外: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void ExecuteCancelConvert()
    {
        _cts?.Cancel();
    }

    private void ExecuteBrowseOutputDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "選擇輸出資料夾"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = dialog.FolderName;
            UseCustomOutputDirectory = true;
        }
    }

    private void ExecuteOpenOutputDirectory()
    {
        string dir = UseCustomOutputDirectory && !string.IsNullOrWhiteSpace(OutputDirectory)
            ? OutputDirectory
            : (Tasks.FirstOrDefault(t => !string.IsNullOrEmpty(t.OutputPath))?.OutputPath is { } p ? Path.GetDirectoryName(p) : null)
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

    private void UpdateSummary()
    {
        if (Tasks.Count == 0)
        {
            SummaryText = "請拖曳圖片或點擊「新增檔案」開始";
        }
        else
        {
            long totalBytes = Tasks.Sum(t => t.SourceSizeBytes);
            SummaryText = $"已載入 {Tasks.Count} 張圖片 (合計 {FormatBytes(totalBytes)})";
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {suffixes[order]}";
    }
}
