using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PoliceImageToolkit.Models;
using PoliceImageToolkit.Services;

namespace PoliceImageToolkit.ViewModels;

public sealed class LongScreenshotSplitViewModel : ViewModelBase
{
    private const long MaximumDecodedPixels = 100_000_000;
    private const int MinimumRecommendedWidthPixels = 600;
    private readonly ILongScreenshotService _splitService;
    private BitmapSource? _sourceImage;
    private string _sourceFilePath = string.Empty;
    private string _sourceFileName = "尚未載入長截圖";
    private string _outputDirectory = string.Empty;
    private double _frameWidthCm = 8;
    private double _frameHeightCm = 17.5;
    private double _overlapMm = 5;
    private string _statusMessage = "載入手機長截圖後，設定未來 Word 圖框比例即可預覽分頁。";
    private bool _isExporting;
    private bool _hasLowResolutionWarning;

    public LongScreenshotSplitViewModel(ILongScreenshotService splitService)
    {
        _splitService = splitService;
        Pages = new ObservableCollection<LongScreenshotPage>();

        OpenImageCommand = new RelayCommand(_ => ExecuteOpenImage(), _ => !IsExporting);
        BrowseOutputDirectoryCommand = new RelayCommand(_ => ExecuteBrowseOutputDirectory(), _ => !IsExporting);
        OpenOutputDirectoryCommand = new RelayCommand(_ => ExecuteOpenOutputDirectory(), _ => Directory.Exists(ResolvedOutputDirectory));
        ExportAllCommand = new RelayCommand(async _ => await ExecuteExportAllAsync(), _ => SourceImage != null && Pages.Count > 0 && !IsExporting);
    }

    public event EventHandler? PreviewChanged;

    public ObservableCollection<LongScreenshotPage> Pages { get; }
    public BitmapSource? SourceImage
    {
        get => _sourceImage;
        private set => SetProperty(ref _sourceImage, value);
    }

    public string SourceFilePath
    {
        get => _sourceFilePath;
        private set => SetProperty(ref _sourceFilePath, value);
    }

    public string SourceFileName
    {
        get => _sourceFileName;
        private set => SetProperty(ref _sourceFileName, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            if (SetProperty(ref _outputDirectory, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public double FrameWidthCm
    {
        get => _frameWidthCm;
        set
        {
            if (SetProperty(ref _frameWidthCm, value)) RecalculatePages();
        }
    }

    public double FrameHeightCm
    {
        get => _frameHeightCm;
        set
        {
            if (SetProperty(ref _frameHeightCm, value)) RecalculatePages();
        }
    }

    public double OverlapMm
    {
        get => _overlapMm;
        set
        {
            if (SetProperty(ref _overlapMm, value)) RecalculatePages();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (SetProperty(ref _isExporting, value)) CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool HasLowResolutionWarning
    {
        get => _hasLowResolutionWarning;
        private set => SetProperty(ref _hasLowResolutionWarning, value);
    }

    public string LowResolutionWarningMessage => $"目前原圖寬度僅 {SourceImage?.PixelWidth ?? 0:N0} px，放入 Word 圖框時文字可能模糊。請改用原始長截圖；Telegram 請選「以檔案傳送」，不要以相片傳送。工具會保留原始像素裁切，不會自動放大製造細節。";

    public string SourceDimensions => SourceImage == null ? "-" : $"{SourceImage.PixelWidth:N0} × {SourceImage.PixelHeight:N0} px";
    public string PageSummary => Pages.Count == 0 ? "尚未計算分頁" : $"預計輸出 {Pages.Count} 張 PNG（僅原始像素裁切）";
    public string ResolvedOutputDirectory => !string.IsNullOrWhiteSpace(OutputDirectory)
        ? OutputDirectory
        : (string.IsNullOrWhiteSpace(SourceFilePath) ? string.Empty : Path.Combine(Path.GetDirectoryName(SourceFilePath)!, "Split"));

    public ICommand OpenImageCommand { get; }
    public ICommand BrowseOutputDirectoryCommand { get; }
    public ICommand OpenOutputDirectoryCommand { get; }
    public ICommand ExportAllCommand { get; }

    private void ExecuteOpenImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "長截圖 (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|所有檔案 (*.*)|*.*",
            Title = "選擇要分頁的手機長截圖"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadImage(dialog.FileName);
        }
    }

    public void LoadImage(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            if (decoder.Frames.Count == 0) throw new InvalidDataException("無法讀取此圖片。");

            var frame = decoder.Frames[0];
            long pixelCount = (long)frame.PixelWidth * frame.PixelHeight;
            if (pixelCount <= 0 || pixelCount > MaximumDecodedPixels)
            {
                throw new InvalidDataException($"圖片解析度 {frame.PixelWidth} × {frame.PixelHeight} 超過安全處理上限。");
            }

            frame.Freeze();
            SourceImage = frame;
            SourceFilePath = filePath;
            SourceFileName = Path.GetFileName(filePath);
            HasLowResolutionWarning = frame.PixelWidth < MinimumRecommendedWidthPixels;
            OnPropertyChanged(nameof(SourceDimensions));
            OnPropertyChanged(nameof(LowResolutionWarningMessage));
            OnPropertyChanged(nameof(ResolvedOutputDirectory));
            RecalculatePages();
            StatusMessage = HasLowResolutionWarning
                ? $"已載入 {SourceFileName}，但原圖寬度過低，請留意下方清晰度警告。"
                : $"已載入 {SourceFileName}（{SourceDimensions}）。可拖曳手機框或按 ↑／↓ 檢視內容。";
            PreviewChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"載入失敗：{ex.Message}";
        }
    }

    public void ShowDropMessage(string message)
    {
        StatusMessage = message;
    }

    private void RecalculatePages()
    {
        if (SourceImage == null || FrameWidthCm <= 0 || FrameHeightCm <= 0 || OverlapMm < 0) return;

        try
        {
            var pages = _splitService.CalculatePages(SourceImage.PixelWidth, SourceImage.PixelHeight, CreateOptions());
            Pages.Clear();
            foreach (var page in pages) Pages.Add(page);
            OnPropertyChanged(nameof(PageSummary));
            PreviewChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Pages.Clear();
            StatusMessage = $"分頁設定無效：{ex.Message}";
        }
        finally
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void ExecuteBrowseOutputDirectory()
    {
        var dialog = new OpenFolderDialog { Title = "選擇長截圖分頁輸出資料夾" };
        if (dialog.ShowDialog() == true) OutputDirectory = dialog.FolderName;
    }

    private void ExecuteOpenOutputDirectory()
    {
        if (!Directory.Exists(ResolvedOutputDirectory)) return;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = ResolvedOutputDirectory,
            UseShellExecute = true
        });
    }

    private async Task ExecuteExportAllAsync()
    {
        if (SourceImage == null || Pages.Count == 0) return;

        IsExporting = true;
        try
        {
            var paths = await _splitService.ExportPagesAsync(SourceImage, Pages.ToList(), SourceFilePath, CreateOptions());
            StatusMessage = $"已匯出 {paths.Count} 張 PNG：{ResolvedOutputDirectory}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"匯出失敗：{ex.Message}";
        }
        finally
        {
            IsExporting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private LongScreenshotSplitOptions CreateOptions() => new(FrameWidthCm, FrameHeightCm, OverlapMm, ResolvedOutputDirectory);
}
