using System.Windows.Media.Imaging;
using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public record LongScreenshotSplitOptions(
    double FrameWidthCm,
    double FrameHeightCm,
    double OverlapMm,
    string OutputDirectory);

public interface ILongScreenshotService
{
    IReadOnlyList<LongScreenshotPage> CalculatePages(int sourceWidth, int sourceHeight, LongScreenshotSplitOptions options);
    Task<IReadOnlyList<string>> ExportPagesAsync(
        BitmapSource source,
        IReadOnlyList<LongScreenshotPage> pages,
        string sourceFilePath,
        LongScreenshotSplitOptions options,
        CancellationToken cancellationToken = default);
}
