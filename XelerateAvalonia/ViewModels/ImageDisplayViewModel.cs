using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Reactive;
using XelerateAvalonia.Auxiliaries;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;

namespace XelerateAvalonia.ViewModels
{
    public class ImageDisplayViewModel : ReactiveObject
    {
        private ImageCore _item;
        private string _databasePath;
        private double _totalRotationAngle = 0;
        private float _gammaValue = 1.0f;
        private SixLabors.ImageSharp.Image<Rgba32> _imageSharp;
        private string _fileName;
        private string _textBoxROIStart;
        private string _textBoxROIEnd;
        private string _pixelSize;
        private string _orientation;
        private string _crossMarginTop;
        private string _crossMarginBottom;

        public ReactiveCommand<Unit, Unit> SaveImageCommand { get; }

        public ReactiveCommand<Unit, Unit> ExitCommand { get; }



        public ImageDisplayViewModel(Avalonia.Controls.Image image, ImageCore item, string databasePath)
        {
            // Initialize properties
            Item = item;
            DatabasePath = databasePath;
            ImageControl = image;

        }

        // Properties
        public ImageCore Item
        {
            get => _item;
            set => this.RaiseAndSetIfChanged(ref _item, value);
        }

        public string DatabasePath
        {
            get => _databasePath;
            set => this.RaiseAndSetIfChanged(ref _databasePath, value);
        }

        public double TotalRotationAngle
        {
            get => _totalRotationAngle;
            set => this.RaiseAndSetIfChanged(ref _totalRotationAngle, value);
        }

        public float GammaValue
        {
            get => _gammaValue;
            set => this.RaiseAndSetIfChanged(ref _gammaValue, value);
        }

        public Avalonia.Controls.Image ImageControl { get; }

        // Bindable Properties
        public string FileNameText
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public string TextBoxROIStart
        {
            get => _textBoxROIStart;
            set => this.RaiseAndSetIfChanged(ref _textBoxROIStart, value);
        }

        public string TextBoxROIEnd
        {
            get => _textBoxROIEnd;
            set => this.RaiseAndSetIfChanged(ref _textBoxROIEnd, value);
        }

       

        public string OrientationText
        {
            get => _orientation;
            set => this.RaiseAndSetIfChanged(ref _orientation, value);
        }

        public string CrossMarginTopText
        {
            get => _crossMarginTop;
            set => this.RaiseAndSetIfChanged(ref _crossMarginTop, value);
        }

        public string CrossMarginBottomText
        {
            get => _crossMarginBottom;
            set => this.RaiseAndSetIfChanged(ref _crossMarginBottom, value);
        }
        public string PixelSizeText
        {
            get => _pixelSize;
            set => this.RaiseAndSetIfChanged(ref _pixelSize, value);
        }


        // Methods
        public void RotateImageControl()
        {
            TotalRotationAngle += 90;
            ImageControl.RenderTransform = new RotateTransform(TotalRotationAngle);
        }

        public void RotateImageSharp()
        {
            if (_imageSharp == null)
            {
                _imageSharp = ImageTransformations.LoadImageFromBytes(Item.Blob);
            }
            else
            {
                ImageTransformations.RotateImageSharp(_imageSharp);
            }
        }

        public void AdjustBrightness()
        {
            if (_imageSharp != null)
            {
                ImageTransformations.AdjustBrightness(_imageSharp, GammaValue);
            }
        }

        public void SaveImage()
        {
            // Gather input field values
            string name = "FileName.Text";
            string roiStart = "TextBoxROIStart.Text";
            string roiEnd = "TextBoxROIEnd.Text";
            string pixelSize = "PixelSize.Text";
            string orientation = "Orientation.Text";
            string marginLeft = "CrossMarginTop.Text";
            string marginRight = "CrossMarginBottom.Text";
            string FileType = Item.FileType;

            byte[] imageBytes = _imageSharp != null ? ImageTransformations.GetBytesFromImage(_imageSharp) : Item.Blob;

            // Create a new ImageCore object
            ImageCore newItem = new ImageCore(
                name,
                null,
                imageBytes,
                FileType,
                Item.Width,
                Item.Height,
                roiStart,
                roiEnd,
                pixelSize,
                orientation,
                marginRight,
                marginLeft,
                0,
                default,
                null,
                null
            );

            // Overwrite properties except for size and uploaded from the existing item
            newItem.Size = Item.Size;
            newItem.Uploaded = Item.Uploaded;

            // Save the updated item in the database
            DBAccess.SaveImage(newItem, true, DatabasePath);

            _imageSharp?.Dispose();
        }
    }
}
