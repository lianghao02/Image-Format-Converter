using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public class VideoService : IVideoService
{
    public async Task<SnapshotResult> SaveFrameSnapshotAsync(
        BitmapSource sourceFrame,
        TimeSpan currentPosition,
        string videoFilePath,
        VideoSnapshotConfig config)
    {
        return await Task.Run(() =>
        {
            string? reservedOutputPath = null;
            try
            {
            string outDir = string.IsNullOrWhiteSpace(config.OutputDirectory)
                ? Path.GetDirectoryName(videoFilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                : config.OutputDirectory;

            string videoName = Path.GetFileNameWithoutExtension(videoFilePath);

            if (config.AutoCreateSubfolder)
            {
                outDir = Path.Combine(outDir, $"{videoName}_Snapshots");
            }

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            BitmapSource processedBitmap = sourceFrame;

            // 1. 烙印時間戳浮水印 (若開啟)
            if (config.AddTimestampOverlay)
            {
                string title = string.IsNullOrWhiteSpace(config.CasePrefix)
                    ? Path.GetFileName(videoFilePath)
                    : $"{config.CasePrefix} - {Path.GetFileName(videoFilePath)}";

                processedBitmap = RenderFrameWithTimestamp(processedBitmap, currentPosition, title, config.IncludeMilliseconds);
            }

            // 2. 檔名構造
            string safeCasePrefix = SanitizeFileNameSegment(config.CasePrefix);
            string prefix = string.IsNullOrWhiteSpace(safeCasePrefix)
                ? config.Prefix
                : $"{safeCasePrefix}_{config.Prefix}";

            string timeStr = config.IncludeMilliseconds
                ? $"{(int)currentPosition.TotalHours:00}-{currentPosition.Minutes:00}-{currentPosition.Seconds:00}_{currentPosition.Milliseconds:000}"
                : $"{(int)currentPosition.TotalHours:00}-{currentPosition.Minutes:00}-{currentPosition.Seconds:00}";

            string ext = config.OutputFormat.ToLowerInvariant() == "jpg" ? "jpg" : "png";
            // 3. 編碼並存檔
            BitmapEncoder encoder = ext == "jpg"
                ? new JpegBitmapEncoder { QualityLevel = Math.Clamp(config.JpgQuality, 10, 100) }
                : new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(processedBitmap));

            using (var fs = CreateUniqueOutputFile(outDir, prefix, timeStr, ext, out reservedOutputPath))
            {
                encoder.Save(fs);
            }

            var fi = new FileInfo(reservedOutputPath);

            // 產生輕量縮圖供 UI 對照顯示 (縮小至寬度 400px)
            BitmapSource thumb = processedBitmap;
            if (processedBitmap.PixelWidth > 400)
            {
                double scale = 400.0 / processedBitmap.PixelWidth;
                var scaled = new TransformedBitmap(processedBitmap, new ScaleTransform(scale, scale));
                scaled.Freeze();
                thumb = scaled;
            }

            return new SnapshotResult(
                reservedOutputPath,
                currentPosition,
                processedBitmap.PixelWidth,
                processedBitmap.PixelHeight,
                fi.Length,
                thumb
            );
            }
            catch
            {
                DeletePartialOutput(reservedOutputPath);
                throw;
            }
        });
    }

    public BitmapSource RenderFrameWithTimestamp(
        BitmapSource frame,
        TimeSpan timestamp,
        string videoTitle,
        bool includeMs)
    {
        int width = frame.PixelWidth;
        int height = frame.PixelHeight;

        var visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            // 繪製原始影格
            dc.DrawImage(frame, new Rect(0, 0, width, height));

            // 計算字體與浮水印尺寸 (依影像寬度動態縮放)
            double fontSize = Math.Clamp(width * 0.024, 14, 42);
            var typeface = new Typeface(new FontFamily("Segoe UI, Microsoft JhengHei"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

            string timeText = includeMs
                ? $"⏱ {(int)timestamp.TotalHours:00}:{timestamp.Minutes:00}:{timestamp.Seconds:00}.{timestamp.Milliseconds:000}"
                : $"⏱ {(int)timestamp.TotalHours:00}:{timestamp.Minutes:00}:{timestamp.Seconds:00}";

            var ftTime = new FormattedText(
                timeText,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.White,
                1.0
            );

            var ftTitle = new FormattedText(
                videoTitle,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize * 0.75,
                new SolidColorBrush(Color.FromRgb(220, 225, 235)),
                1.0
            );

            double padding = fontSize * 0.5;
            double barHeight = ftTime.Height + padding * 2;
            double barY = height - barHeight;

            // 繪製半透明深色底條 (現代深邃黑藍底條)
            var bgBrush = new SolidColorBrush(Color.FromArgb(175, 10, 18, 30));
            dc.DrawRectangle(bgBrush, null, new Rect(0, barY, width, barHeight));

            // 繪製時間戳記與檔名
            dc.DrawText(ftTime, new Point(padding, barY + padding));
            dc.DrawText(ftTitle, new Point(width - ftTitle.Width - padding, barY + padding + (ftTime.Height - ftTitle.Height) / 2));
        }

        var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(visual);
        renderBitmap.Freeze();
        return renderBitmap;
    }

    private static FileStream CreateUniqueOutputFile(string outputDirectory, string prefix, string timeStamp, string extension, out string outputPath)
    {
        for (int counter = 0; ; counter++)
        {
            string suffix = counter == 0 ? string.Empty : $"_{counter}";
            string candidate = Path.Combine(outputDirectory, $"{prefix}{timeStamp}{suffix}.{extension}");
            try
            {
                var stream = new FileStream(candidate, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                outputPath = candidate;
                return stream;
            }
            catch (IOException) when (File.Exists(candidate))
            {
                // 同名截圖已存在或被其他工作搶先建立，改用下一個流水號。
            }
        }
    }

    private static string SanitizeFileNameSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars()
            .Append(Path.DirectorySeparatorChar)
            .Append(Path.AltDirectorySeparatorChar)
            .ToHashSet();
        return new string(value.Trim().Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
    }

    private static void DeletePartialOutput(string? outputPath)
    {
        if (string.IsNullOrEmpty(outputPath)) return;

        try
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
        catch
        {
            // 保留原始例外，讓呼叫端回報實際存檔失敗原因。
        }
    }
}
