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
        private string _coreID;
        private string _sectionID;
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

        public string CoreIDText
        {
            get => _coreID;
            set => this.RaiseAndSetIfChanged(ref _coreID, value);
        }

        public string SectionIDText
        {
            get => _sectionID;
            set => this.RaiseAndSetIfChanged(ref _sectionID, value);
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

       
    }
}
