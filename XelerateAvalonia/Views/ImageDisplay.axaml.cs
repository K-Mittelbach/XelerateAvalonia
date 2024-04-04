using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;
using XelerateAvalonia.Auxiliaries;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;
using XelerateAvalonia.ViewModels;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing.Imaging;
using BitMiracle.LibTiff.Classic;
using SkiaSharp;
using System.Runtime.InteropServices;
using System;
using Avalonia.Controls.Primitives;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace XelerateAvalonia.Views
{
    public partial class ImageDisplay : UserControl
    {
        private Grid _sliderGridStart;
        private Grid _sliderGridEnd;

        private Grid _sliderGridLeft;
        private Grid _sliderGridRight;

        private Avalonia.Controls.Image _imageControl;

        private double startDragY;
        private double endDragY;
        private double rightDragY;
        private double leftDragY;

        private double totalRotationAngle = 0;
        private double GammaValue;

        private string _appyForAllChecked;

        private SixLabors.ImageSharp.Image<Rgba32> ImageSharp;

        public ImageCore Item { get; private set; }

        public string DatabasePath { get; private set; }

        public ImageDisplay(Avalonia.Controls.Image image, ImageCore item, string databasePath)
        {
            InitializeComponent(image, item);
            Item = item;
            DatabasePath = databasePath;
        }

        private void InitializeComponent(Avalonia.Controls.Image image, ImageCore item)
        {
            AvaloniaXamlLoader.Load(this);

            FileName = this.FindControl<TextBox>("FileName");
            TextBoxROITop= this.FindControl<TextBox>("TextBoxROITop");
            TextBoxROIBottom = this.FindControl<TextBox>("TextBoxROIBottom");
            TextBoxROILeft = this.FindControl<TextBox>("TextBoxROILeft");
            TextBoxROIRight = this.FindControl<TextBox>("TextBoxROIRight");
            PixelSize = this.FindControl<TextBox>("PixelSize");
            ApplyForAll = this.FindControl<ToggleSwitch>("ApplyForAll");
            CoreID = this.FindControl<TextBox>("CoreID");
            SectionID = this.FindControl<TextBox>("SectionID");

            var brightnessSlider = this.FindControl<Slider>("GammaSlider");
            brightnessSlider.ValueChanged += OnBrightnessSliderValueChanged;

            if (item != null)
            {
                FileName.Text = item.Name?.ToString();
                TextBoxROITop.Text = item.ROIStart?.ToString();
                TextBoxROIBottom.Text = item.ROIEnd?.ToString();
                TextBoxROILeft.Text = item.ImageMarginLeft?.ToString();
                TextBoxROIRight.Text = item.ImageMarginRight?.ToString();
                PixelSize.Text = item.ImagePixelSize?.ToString();
                CoreID.Text = item.CoreID.ToString();
                SectionID.Text = item.SectionID.ToString();
            }

            

            _sliderGridStart = this.FindControl<Grid>("SliderGridStart");
            _sliderGridEnd = this.FindControl<Grid>("SliderGridEnd");
            _sliderGridLeft = this.FindControl<Grid>("SliderGridLeft");
            _sliderGridRight = this.FindControl<Grid>("SliderGridRight");

            _imageControl = this.FindControl<Avalonia.Controls.Image>("ImageControl");

            var rotateButton = this.FindControl<Button>("RotateButton");
            rotateButton.Click += OnRotateButtonClick;

            // Set the content of the UserControl to the provided Image control
            _imageControl.Source = image.Source;

            SetSliderPositions(item);

            // Attach drag events to the sliders
            _sliderGridStart.PointerPressed += SliderROIStart_OnPointerPressed;
            _sliderGridStart.PointerMoved += SliderROIStart_OnPointerMoved;
            _sliderGridEnd.PointerPressed += SliderROIEnd_OnPointerPressed;
            _sliderGridEnd.PointerMoved += SliderROIEnd_OnPointerMoved;
            _sliderGridLeft.PointerPressed += SliderROILeft_OnPointerPressed;
            _sliderGridLeft.PointerMoved += SliderROILeft_OnPointerMoved;
            _sliderGridRight.PointerPressed += SliderROIRight_OnPointerPressed;
            _sliderGridRight.PointerMoved += SliderROIRight_OnPointerMoved;

        }

        private void OnRotateButtonClick(object sender, RoutedEventArgs e)
        {
            var imageControl = this.FindControl<Avalonia.Controls.Image>("ImageControl");
            ImageTransformations.RotateImageControl(imageControl, ref totalRotationAngle);

            if (ImageSharp == null)
            {
                // Load image from byte array
                ImageSharp = ImageTransformations.LoadImageFromBytes(Item.Blob);
            }

            if (totalRotationAngle != 0)
            {
                // Rotate the existing ImageSharp object
                ImageTransformations.RotateImageSharp(ImageSharp);
            }
        }

        private void OnBrightnessSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var imageControl = this.FindControl<Avalonia.Controls.Image>("ImageControl");
            
            
                // Load image from byte array
                ImageSharp = ImageTransformations.LoadImageFromBytes(Item.Blob); 

             
                var slider = (Slider)sender;
                GammaValue = (float)slider.Value;
                ImageTransformations.AdjustBrightness(ImageSharp, (float)GammaValue);

               
                var newBitmap = ImageTransformations.ConvertToBitmap(ImageSharp);
                imageControl.Source = newBitmap;    
            
        }

        // WIP - might need to shift this to the viewModel
        private void SetSliderPositions(ImageCore item)
        {
            
            double verticalPositionStart = double.Parse(item.ROIStart);
            double verticalPositionEnd = double.Parse(item.ROIEnd);

            double verticalPositionLeft = double.Parse(item.ImageMarginLeft);
            double verticalPositionRight = double.Parse(item.ImageMarginRight);

            // Set the Y-coordinates of the Grid controls
            _sliderGridStart.Margin = new Thickness(0, verticalPositionStart, 0, 0);
            _sliderGridEnd.Margin = new Thickness(0, verticalPositionEnd, 0, 0);
            _sliderGridLeft.Margin = new Thickness(verticalPositionRight, 0, 0, 0);
            _sliderGridRight.Margin = new Thickness(verticalPositionLeft, 0, 0, 0);
        }

        private void SliderROIStart_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            startDragY = e.GetPosition(_sliderGridStart).Y;
        }

        private void SliderROIStart_OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(_sliderGridStart).Properties.IsLeftButtonPressed)
            {
                double newY = e.GetPosition(_sliderGridStart.Parent as Control).Y - startDragY;
                _sliderGridStart.Margin = new Thickness(0, newY, 0, 0);
            }
        }
        private void SliderROILeft_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            leftDragY = e.GetPosition(_sliderGridLeft).X - _sliderGridLeft.Margin.Left;
        }

        private void SliderROILeft_OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(_sliderGridLeft).Properties.IsLeftButtonPressed)
            {
                double newX = e.GetPosition(_sliderGridLeft.Parent as Control).X - leftDragY;

                // Update the horizontal position of the slider within the parent control
                _sliderGridLeft.Margin = new Thickness(newX, 0, 0, 0);
            }
        }

        private void SliderROIRight_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            rightDragY = e.GetPosition(_sliderGridRight).X - _sliderGridRight.Margin.Left;
        }

        private void SliderROIRight_OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(_sliderGridRight).Properties.IsLeftButtonPressed)
            {
                double newX = e.GetPosition(_sliderGridRight.Parent as Control).X - rightDragY;

                // Update the horizontal position of the slider within the parent control
                _sliderGridRight.Margin = new Thickness(newX, 0, 0, 0);
            }
        }
        private void SliderROIEnd_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            endDragY = e.GetPosition(_sliderGridEnd).Y;
        }

        private void SliderROIEnd_OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(_sliderGridEnd).Properties.IsLeftButtonPressed)
            {
                double newY = e.GetPosition(_sliderGridEnd.Parent as Control).Y - endDragY;
                _sliderGridEnd.Margin = new Thickness(0, newY, 0, 0);
            }
        }

        private void OnExitButtonClick(object sender, RoutedEventArgs e)
        {
            // Get the parent window of the UserControl
            var window = this.VisualRoot as Window;

            // Close the window
            window.Close();
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            // Gather input field values
            string name = FileName.Text;
            string roiStart = TextBoxROITop.Text;
            string roiEnd = TextBoxROIBottom.Text;
            string roiLeft = TextBoxROILeft.Text;
            string roiRight = TextBoxROIRight.Text;
            string pixelSize = PixelSize.Text;
            string orientation = "Vertical";
            string coreID = CoreID.Text;
            string sectionID = SectionID.Text;
            string FileType = Item.FileType;


            byte[] imageBytes;
            if (ImageSharp == null)
            {
                imageBytes = Item.Blob;
            }
            else
            {
                // Get the image bytes from the ImageControl
                imageBytes = ImageTransformations.GetBytesFromImage(ImageSharp);
            }


            // IMAGE SLICE SELECTION METHOD CALL

            //Change for integration of ROI Selection from User

            byte[] croppedImageBytes = ImageTransformations.CaptureImageSlice(_sliderGridLeft, _sliderGridRight, _sliderGridStart, _sliderGridEnd, imageBytes);

            // Check if ApplyForAll toggle button is checked
            if (ApplyForAll.IsChecked == true)
            {
                // Get all images whose names start with FileName.Text
                ObservableCollection<ImageCore> images = new ObservableCollection<ImageCore>(
                    DBAccess.GetAllImages(DatabasePath)
                    .Where(image => string.Concat(image.Name.TakeWhile(c => !char.IsDigit(c))) == string.Concat(FileName.Text.TakeWhile(c => !char.IsDigit(c))))
                );

                foreach (var image in images)
                {
                    // Set ROI values for each image
                    image.ROIStart = roiStart;
                    image.ROIEnd = roiEnd;
                    image.ImageMarginLeft = roiLeft;
                    image.ImageMarginRight = roiRight;

                    // Capture image slice using the same ROI values
                    byte[] otherCroppedImageBytes = ImageTransformations.CaptureImageSlice(_sliderGridLeft, _sliderGridRight, _sliderGridStart, _sliderGridEnd, image.Blob);

                    // Update the image's cropped bytes
                    image.BlobROI = otherCroppedImageBytes;

                    // Save the updated image to the database
                    DBAccess.SaveImage(image, true, DatabasePath);
                }
            }

            // Create a new ImageCore object with the gathered values and updated image bytes
            ImageCore newItem = new ImageCore(
                name,
                null, 
                imageBytes,
                croppedImageBytes,
                Int32.Parse(coreID),
                Int32.Parse(sectionID),
                FileType, 
                Item.Width, 
                Item.Height,
                roiStart,
                roiEnd,
                pixelSize,
                orientation,
                roiLeft,
                roiRight,
                0, // Placeholder for size, not updated here
                default, // Placeholder for uploaded, not updated here
                null, // Placeholder for images collection, not updated here
                null // Placeholder for database path, not updated here
            );

            // Overwrite properties except for size and uploaded from the existing item
            newItem.Size = Item.Size; // Keep the original size
            newItem.Uploaded = Item.Uploaded; // Keep the original uploaded date

            // Save the updated item in the database
            DBAccess.SaveImage(newItem, true, DatabasePath);

            // Change the ROI Settings for all other images with the given slices and set the cropedImageBytes !(if ApplyButton isChecked ="true") 

            ImageSharp?.Dispose();
            // Get the parent window of the UserControl
            var window = this.VisualRoot as Window;

            window.Close();

      
        }

        //private bool IsTiffImage(byte[] imageBytes)
        //{
        //    // Check if the imageBytes represent a TIFF image
        //    return imageBytes.Length > 1 && (imageBytes[0] == 'I' && imageBytes[1] == 'I' || imageBytes[0] == 'M' && imageBytes[1] == 'M');
        //}
    }
}
