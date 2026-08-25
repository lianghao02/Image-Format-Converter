using System.Windows;
using System.Windows.Controls;
using PoliceImageToolkit.ViewModels;

namespace PoliceImageToolkit.Views;

public partial class ImageConverterView : UserControl
{
    public ImageConverterView()
    {
        InitializeComponent();
    }

    private void UserControl_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void UserControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                if (DataContext is ImageConverterViewModel vm)
                {
                    vm.AddFiles(files);
                }
            }
        }
    }
}
