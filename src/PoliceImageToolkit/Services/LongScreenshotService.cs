using System.IO;
using System.Windows.Media.Imaging;
using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public sealed class LongScreenshotService : ILongScreenshotService
{
    public IReadOnlyList<LongScreenshotPage> CalculatePages(int sourceWidth, int sourceHeight, LongScreenshotSplitOptions options)
    {
        ValidateOptions(options);
        if (sourceWidth <= 0 || sourceHeight <= 0)
        {
            return [];
        }

        int pageHeight = Math.Max(1, (int)Math.Round(sourceWidth * options.FrameHeightCm / options.FrameWidthCm));
        int overlapPixels = Math.Clamp(
            (int)Math.Round(sourceWidth * (options.OverlapMm / 10d) / options.FrameWidthCm),
            0,
            pageHeight - 1);

        var pages = new List<LongScreenshotPage>();
        int sourceY = 0;
        while (sourceY < sourceHeight)
        {
            int pixelHeight = Math.Min(pageHeight, sourceHeight - sourceY);
            bool isFirstPage = pages.Count == 0;
            bool isLastPage = sourceY + pixelHeight >= sourceHeight;
            pages.Add(new LongScreenshotPage(
                pages.Count + 1,
                sourceY,
                pixelHeight,
                isFirstPage ? 0 : overlapPixels,
                isLastPage ? 0 : overlapPixels));

            if (isLastPage) break;
            sourceY += pageHeight - overlapPixels;
        }

        return pages;
    }

    public async Task<IReadOnlyList<string>> ExportPagesAsync(
        BitmapSource source,
        IReadOnlyList<LongScreenshotPage> pages,
        string sourceFilePath,
        LongScreenshotSplitOptions options,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions(options);
        if (pages.Count == 0) return [];

        // 呼叫端位於 WPF 介面執行緒；先建立可跨執行緒使用的凍結點陣副本，
        // 再交給背景工作逐頁裁切，避免 Dispatcher 物件存取例外。
        var exportSource = new CachedBitmap(
            source,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);
        exportSource.Freeze();

        return await Task.Run(() =>
        {
            Directory.CreateDirectory(options.OutputDirectory);
            string baseName = Path.GetFileNameWithoutExtension(sourceFilePath);
            var outputPaths = new List<string>(pages.Count);

            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string outputPath = ReserveOutputPath(options.OutputDirectory, baseName, page.PageNumber, pages.Count);
                try
                {
                    var cropped = new CroppedBitmap(exportSource, new System.Windows.Int32Rect(0, page.SourceY, exportSource.PixelWidth, page.PixelHeight));
                    cropped.Freeze();

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(cropped));
                    using var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    encoder.Save(output);
                    outputPaths.Add(outputPath);
                }
                catch
                {
                    TryDelete(outputPath);
                    throw;
                }
            }

            return (IReadOnlyList<string>)outputPaths;
        }, cancellationToken);
    }

    private static string ReserveOutputPath(string outputDirectory, string baseName, int pageNumber, int totalPages)
    {
        string pageSuffix = $"_{pageNumber:000}of{totalPages:000}";
        for (int counter = 0; ; counter++)
        {
            string collisionSuffix = counter == 0 ? string.Empty : $"_{counter}";
            string candidate = Path.Combine(outputDirectory, $"{baseName}{pageSuffix}{collisionSuffix}.png");
            if (!File.Exists(candidate)) return candidate;
        }
    }

    private static void ValidateOptions(LongScreenshotSplitOptions options)
    {
        if (options.FrameWidthCm <= 0 || options.FrameHeightCm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "圖框寬高必須大於 0。");
        }

        if (options.OverlapMm < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "重疊高度不可小於 0。");
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // 保留原始例外，讓呼叫端回報真正的匯出失敗原因。
        }
    }
}
