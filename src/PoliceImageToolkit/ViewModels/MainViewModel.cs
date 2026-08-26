using PoliceImageToolkit.Services;

namespace PoliceImageToolkit.ViewModels;

public class MainViewModel : ViewModelBase
{
    private int _selectedTabIndex = 0;

    public MainViewModel()
    {
        IImageService imageService = new ImageService();
        IVideoService videoService = new VideoService();
        ILongScreenshotService longScreenshotService = new LongScreenshotService();

        ImageConverter = new ImageConverterViewModel(imageService);
        VideoSnapshot = new VideoSnapshotViewModel(videoService);
        LongScreenshotSplit = new LongScreenshotSplitViewModel(longScreenshotService);
    }

    public string AppTitle => "警務影像轉檔與手機影片截圖工具箱 (Police-Image-Toolkit v11.0.0)";

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public ImageConverterViewModel ImageConverter { get; }
    public VideoSnapshotViewModel VideoSnapshot { get; }
    public LongScreenshotSplitViewModel LongScreenshotSplit { get; }
}
