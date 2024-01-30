using Avalonia.Controls;
using XelerateAvalonia.Models;

namespace XelerateAvalonia.Views
{
    public partial class ImageSettingsDisplay : UserControl
    {
        public ImageSettingsDisplay(ImageCore item)
        {
            InitializeComponent();
            FileName.Text = item.Name;
            ROIStart.Text = item.ROIStart;
            ROIEnd.Text = item.ROIEnd;
            PixelSize.Text = item.ImagePixelSize;
            Orientation.Text = item.ImageOrientation;
            CorssMarginTop.Text = item.ImageMarginLeft;
            CrossMarginBottom.Text = item.ImageMarginRight;
        }
    }
}
