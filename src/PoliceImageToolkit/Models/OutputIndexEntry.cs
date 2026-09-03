namespace PoliceImageToolkit.Models;

/// <summary>
/// 輸出資料夾內的最小追溯資訊；只保存檔名與必要技術資料，不保存絕對路徑或案件資料。
/// </summary>
public sealed record OutputIndexEntry(
    string OutputFile,
    string SourceFile,
    string SourceType,
    string? MediaTimestamp,
    int Width,
    int Height);
