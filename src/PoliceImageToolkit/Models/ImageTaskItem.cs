using PoliceImageToolkit.ViewModels;

namespace PoliceImageToolkit.Models;

public enum TaskStatus
{
    Pending,
    Processing,
    Success,
    Failed
}

public class ImageTaskItem : ViewModelBase
{
    private TaskStatus _status = TaskStatus.Pending;
    private string _statusMessage = "等待中";
    private double _progress = 0;
    private string _outputPath = string.Empty;
    private long _outputSizeBytes = 0;
    private int _outputWidth;
    private int _outputHeight;
    private TimeSpan _elapsed = TimeSpan.Zero;

    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public required string SourceFormat { get; init; }
    public required long SourceSizeBytes { get; init; }

    public TaskStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public string OutputPath
    {
        get => _outputPath;
        set => SetProperty(ref _outputPath, value);
    }

    public long OutputSizeBytes
    {
        get => _outputSizeBytes;
        set => SetProperty(ref _outputSizeBytes, value);
    }

    public int OutputWidth
    {
        get => _outputWidth;
        set => SetProperty(ref _outputWidth, value);
    }

    public int OutputHeight
    {
        get => _outputHeight;
        set => SetProperty(ref _outputHeight, value);
    }

    public TimeSpan Elapsed
    {
        get => _elapsed;
        set => SetProperty(ref _elapsed, value);
    }

    public string SourceSizeFormatted => FormatBytes(SourceSizeBytes);
    public string OutputSizeFormatted => OutputSizeBytes > 0 ? FormatBytes(OutputSizeBytes) : "-";

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
