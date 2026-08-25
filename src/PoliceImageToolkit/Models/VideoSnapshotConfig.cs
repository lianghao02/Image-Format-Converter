namespace PoliceImageToolkit.Models;

public class VideoSnapshotConfig
{
    public string OutputDirectory { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = "PNG"; // PNG, JPG
    public int JpgQuality { get; set; } = 95;
    public bool AddTimestampOverlay { get; set; } = false; // 預設關閉時間戳浮水印，保持畫面純淨
    public bool IncludeMilliseconds { get; set; } = true;
    public bool AutoCreateSubfolder { get; set; } = true;
    public string Prefix { get; set; } = "SNAP_";
    public string CasePrefix { get; set; } = string.Empty;
}
