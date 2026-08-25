using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public record ImageConvertOptions(
    string TargetFormat,
    int JpgQuality,
    string OutputDirectory,
    bool AutoOrient,
    int MaxDimension = 0
);

public interface IImageService
{
    Task ConvertAsync(ImageTaskItem item, ImageConvertOptions options, CancellationToken ct = default);
    Task<string> ExtractExifSummaryAsync(string filePath);
}
