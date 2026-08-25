using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PoliceImageToolkit.Models;
using TaskStatus = PoliceImageToolkit.Models.TaskStatus;

namespace PoliceImageToolkit.Services;

public class ImageService : IImageService
{
    public async Task ConvertAsync(ImageTaskItem item, ImageConvertOptions options, CancellationToken ct = default)
    {
        item.Status = TaskStatus.Processing;
        item.StatusMessage = "轉檔處理中...";
        item.Progress = 10;
        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(item.FilePath))
                {
                    throw new FileNotFoundException("找不到來源檔案", item.FilePath);
                }

                if (!Directory.Exists(options.OutputDirectory))
                {
                    Directory.CreateDirectory(options.OutputDirectory);
                }

                string baseName = Path.GetFileNameWithoutExtension(item.FilePath);
                string ext = options.TargetFormat.ToLowerInvariant().TrimStart('.');
                if (ext == "jpeg") ext = "jpg";
                string outPath = Path.Combine(options.OutputDirectory, $"{baseName}.{ext}");

                // 避免覆寫若同名
                int counter = 1;
                while (File.Exists(outPath))
                {
                    outPath = Path.Combine(options.OutputDirectory, $"{baseName}_{counter}.{ext}");
                    counter++;
                }

                ct.ThrowIfCancellationRequested();
                item.Progress = 30;

                // 讀取影像至記憶體
                using var fs = new FileStream(item.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var decoder = BitmapDecoder.Create(
                    fs,
                    BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
                    BitmapCacheOption.OnLoad
                );

                if (decoder.Frames.Count == 0)
                {
                    throw new InvalidOperationException("無法解碼此影像之影格。");
                }

                BitmapFrame frame = decoder.Frames[0];
                BitmapSource processedBitmap = frame;
                item.Progress = 50;

                // 處理 Exif 旋轉方向
                if (options.AutoOrient && frame.Metadata is BitmapMetadata metadata)
                {
                    int orientation = GetExifOrientation(metadata);
                    processedBitmap = ApplyOrientation(processedBitmap, orientation);
                }

                // 處理等比例縮放 (若設定 MaxDimension)
                if (options.MaxDimension > 0)
                {
                    int origW = processedBitmap.PixelWidth;
                    int origH = processedBitmap.PixelHeight;
                    int maxSide = Math.Max(origW, origH);
                    if (maxSide > options.MaxDimension)
                    {
                        double scale = (double)options.MaxDimension / maxSide;
                        processedBitmap = new TransformedBitmap(processedBitmap, new ScaleTransform(scale, scale));
                    }
                }

                item.Progress = 75;
                ct.ThrowIfCancellationRequested();

                // 根據目標格式選擇編碼器
                BitmapEncoder encoder = ext switch
                {
                    "png" => new PngBitmapEncoder(),
                    "bmp" => new BmpBitmapEncoder(),
                    "tif" or "tiff" => new TiffBitmapEncoder(),
                    _ => new JpegBitmapEncoder
                    {
                        QualityLevel = Math.Clamp(options.JpgQuality, 10, 100)
                    }
                };

                encoder.Frames.Add(BitmapFrame.Create(processedBitmap));

                using (var outFs = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    encoder.Save(outFs);
                }

                sw.Stop();
                var outInfo = new FileInfo(outPath);

                item.OutputPath = outPath;
                item.OutputSizeBytes = outInfo.Length;
                item.Elapsed = sw.Elapsed;
                item.Progress = 100;
                item.Status = TaskStatus.Success;
                item.StatusMessage = $"完成 ({sw.ElapsedMilliseconds} ms)";
            }
            catch (OperationCanceledException)
            {
                item.Status = TaskStatus.Failed;
                item.StatusMessage = "已取消";
            }
            catch (Exception ex)
            {
                sw.Stop();
                item.Status = TaskStatus.Failed;
                item.StatusMessage = $"失敗: {ex.Message}";
            }
        }, ct);
    }

    public Task<string> ExtractExifSummaryAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.None);
                if (decoder.Frames.Count > 0 && decoder.Frames[0].Metadata is BitmapMetadata metadata)
                {
                    string camera = metadata.CameraModel ?? "未知型號";
                    string date = metadata.DateTaken ?? "無拍攝時間";
                    return $"相機: {camera} | 拍攝時間: {date}";
                }
            }
            catch
            {
                // 忽略非標準中繼資料錯誤
            }
            return "無 Exif 中繼資料";
        });
    }

    private static int GetExifOrientation(BitmapMetadata metadata)
    {
        const string query = "/app1/ifd/{ushort=274}";
        if (metadata.ContainsQuery(query))
        {
            object val = metadata.GetQuery(query);
            if (val is ushort u) return u;
            if (val is int i) return i;
        }
        return 1;
    }

    private static BitmapSource ApplyOrientation(BitmapSource source, int orientation)
    {
        Transform transform = orientation switch
        {
            2 => new ScaleTransform(-1, 1), // Flip Horizontal
            3 => new RotateTransform(180),  // Rotate 180
            4 => new ScaleTransform(1, -1), // Flip Vertical
            5 => new TransformGroup
            {
                Children = { new RotateTransform(90), new ScaleTransform(-1, 1) }
            },
            6 => new RotateTransform(90),   // Rotate 90 CW
            7 => new TransformGroup
            {
                Children = { new RotateTransform(270), new ScaleTransform(-1, 1) }
            },
            8 => new RotateTransform(270),  // Rotate 270 CW
            _ => Transform.Identity
        };

        if (transform == Transform.Identity)
        {
            return source;
        }

        return new TransformedBitmap(source, transform);
    }
}
