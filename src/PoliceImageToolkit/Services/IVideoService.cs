using System.Windows.Media.Imaging;
using PoliceImageToolkit.Models;

namespace PoliceImageToolkit.Services;

public record SnapshotResult(
    string FilePath,
    TimeSpan Timestamp,
    int Width,
    int Height,
    long SizeBytes,
    BitmapSource? Thumbnail = null
);

public interface IVideoService
{
    Task<SnapshotResult> SaveFrameSnapshotAsync(
        BitmapSource sourceFrame,
        TimeSpan currentPosition,
        string videoFilePath,
        VideoSnapshotConfig config
    );

    BitmapSource RenderFrameWithTimestamp(
        BitmapSource frame,
        TimeSpan timestamp,
        string videoTitle,
        bool includeMs
    );
}
