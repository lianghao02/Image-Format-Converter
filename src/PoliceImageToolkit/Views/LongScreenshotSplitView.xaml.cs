using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PoliceImageToolkit.ViewModels;

namespace PoliceImageToolkit.Views;

public partial class LongScreenshotSplitView : UserControl
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg"
    };
    private bool _isDragging;
    private Point _dragStart;
    private double _scrollStart;

    public LongScreenshotSplitView()
    {
        InitializeComponent();
        DataContextChanged += LongScreenshotSplitView_DataContextChanged;
    }

    private LongScreenshotSplitViewModel? ViewModel => DataContext as LongScreenshotSplitViewModel;

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdatePreviewLayout();
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePreviewLayout();
    }

    private void LongScreenshotSplitView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LongScreenshotSplitViewModel oldViewModel)
        {
            oldViewModel.PreviewChanged -= ViewModel_PreviewChanged;
            oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        if (e.NewValue is LongScreenshotSplitViewModel newViewModel)
        {
            newViewModel.PreviewChanged += ViewModel_PreviewChanged;
            newViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        UpdatePreviewLayout();
    }

    private void ViewModel_PreviewChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(UpdatePreviewLayout);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LongScreenshotSplitViewModel.SourceImage))
        {
            Dispatcher.Invoke(UpdatePreviewLayout);
        }
    }

    private void UpdatePreviewLayout()
    {
        if (ViewModel?.SourceImage == null || LongImageScroll.ViewportWidth <= 0) return;

        double previewWidth = LongImageScroll.ViewportWidth;
        double scale = previewWidth / ViewModel.SourceImage.PixelWidth;
        double previewHeight = ViewModel.SourceImage.PixelHeight * scale;

        LongPreviewImage.Width = previewWidth;
        PreviewHost.Width = previewWidth;
        PreviewHost.Height = previewHeight;
        SplitOverlay.Width = previewWidth;
        SplitOverlay.Height = previewHeight;
        SplitOverlay.Children.Clear();

        foreach (var page in ViewModel.Pages.Where(page => page.OverlapTopPixels > 0))
        {
            double top = page.SourceY * scale;
            double height = page.OverlapTopPixels * scale;
            var overlapMask = new Rectangle
            {
                Width = previewWidth,
                Height = height,
                Fill = new SolidColorBrush(Color.FromArgb(105, 251, 191, 36)),
                Stroke = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection([3, 2])
            };
            Canvas.SetTop(overlapMask, top);
            SplitOverlay.Children.Add(overlapMask);
        }
    }

    private void LongImageScroll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _dragStart = e.GetPosition(LongImageScroll);
        _scrollStart = LongImageScroll.VerticalOffset;
        LongImageScroll.Focus();
        LongImageScroll.CaptureMouse();
        e.Handled = true;
    }

    private void LongImageScroll_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        double delta = e.GetPosition(LongImageScroll).Y - _dragStart.Y;
        LongImageScroll.ScrollToVerticalOffset(_scrollStart - delta);
    }

    private void LongImageScroll_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        LongImageScroll.ReleaseMouseCapture();
    }

    private void LongImageScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        LongImageScroll.ScrollToVerticalOffset(LongImageScroll.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private void LongImageScroll_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        double smallStep = 24;
        double pageStep = Math.Max(1, LongImageScroll.ViewportHeight - 24);
        switch (e.Key)
        {
            case Key.Up:
                LongImageScroll.ScrollToVerticalOffset(LongImageScroll.VerticalOffset - smallStep);
                break;
            case Key.Down:
                LongImageScroll.ScrollToVerticalOffset(LongImageScroll.VerticalOffset + smallStep);
                break;
            case Key.PageUp:
                LongImageScroll.ScrollToVerticalOffset(LongImageScroll.VerticalOffset - pageStep);
                break;
            case Key.PageDown:
                LongImageScroll.ScrollToVerticalOffset(LongImageScroll.VerticalOffset + pageStep);
                break;
            case Key.Home:
                LongImageScroll.ScrollToTop();
                break;
            case Key.End:
                LongImageScroll.ScrollToBottom();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void PreviewDrop_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = GetDroppedImagePath(e.Data) == null ? DragDropEffects.None : DragDropEffects.Copy;
        e.Handled = true;
    }

    private void PreviewDrop_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = GetDroppedImagePath(e.Data) == null ? DragDropEffects.None : DragDropEffects.Copy;
        e.Handled = true;
    }

    private void PreviewDrop_Drop(object sender, DragEventArgs e)
    {
        string? imagePath = GetDroppedImagePath(e.Data);
        if (imagePath == null)
        {
            ViewModel?.ShowDropMessage("請拖曳單一 PNG 或 JPG 長截圖。" );
            return;
        }

        ViewModel?.LoadImage(imagePath);
        e.Handled = true;
    }

    private static string? GetDroppedImagePath(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.FileDrop)) return null;
        if (data.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length != 1) return null;

        string path = paths[0];
        return File.Exists(path) && SupportedImageExtensions.Contains(System.IO.Path.GetExtension(path)) ? path : null;
    }
}
