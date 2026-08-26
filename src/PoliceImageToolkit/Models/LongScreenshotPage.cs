namespace PoliceImageToolkit.Models;

public sealed record LongScreenshotPage(
    int PageNumber,
    int SourceY,
    int PixelHeight,
    int OverlapTopPixels,
    int OverlapBottomPixels)
{
    public int SourceEndY => SourceY + PixelHeight;
    public string DisplayRange => $"Y {SourceY:N0}–{SourceEndY:N0} px";
    public string DisplayOverlap => $"上／下重疊：{OverlapTopPixels:N0}／{OverlapBottomPixels:N0} px";
}
