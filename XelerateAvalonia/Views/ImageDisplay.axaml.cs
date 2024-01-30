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

namespace XelerateAvalonia.Views
{
    public partial class ImageDisplay : UserControl
    {
        private Grid _sliderGridStart;
        private Grid _sliderGridEnd;
        private double startDragY;
        private double endDragY;
        private double totalRotationAngle = 0;
        private double GammaValue;

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
            TextBoxROIStart = this.FindControl<TextBox>("TextBoxROIStart");
            TextBoxROIEnd = this.FindControl<TextBox>("TextBoxROIEnd");
            PixelSize = this.FindControl<TextBox>("PixelSize");
            Orientation = this.FindControl<TextBox>("Orientation");
            CrossMarginTop = this.FindControl<TextBox>("CrossMarginTop");
            CrossMarginBottom = this.FindControl<TextBox>("CrossMarginBottom");

            var brightnessSlider = this.FindControl<Slider>("GammaSlider");
            brightnessSlider.ValueChanged += OnBrightnessSliderValueChanged;

            if (item != null)
            {
                FileName.Text = item.Name?.ToString();
                TextBoxROIStart.Text = item.ROIStart?.ToString();
                TextBoxROIEnd.Text = item.ROIEnd?.ToString();
                PixelSize.Text = item.ImagePixelSize?.ToString();
                Orientation.Text = item.ImageOrientation?.ToString();
                CrossMarginTop.Text = item.ImageMarginLeft?.ToString();
                CrossMarginBottom.Text = item.ImageMarginRight?.ToString();
            }

            

            _sliderGridStart = this.FindControl<Grid>("SliderGridStart");
            _sliderGridEnd = this.FindControl<Grid>("SliderGridEnd");

            var imageControl = this.FindControl<Avalonia.Controls.Image>("ImageControl");

            var rotateButton = this.FindControl<Button>("RotateButton");
            rotateButton.Click += OnRotateButtonClick;

            // Set the content of the UserControl to the provided Image control
            imageControl.Source = image.Source;

            SetSliderPositions(item);

            // Attach drag events to the sliders
            _sliderGridStart.PointerPressed += SliderROIStart_OnPointerPressed;
            _sliderGridStart.PointerMoved += SliderROIStart_OnPointerMoved;
            _sliderGridEnd.PointerPressed += SliderROIEnd_OnPointerPressed;
            _sliderGridEnd.PointerMoved += SliderROIEnd_OnPointerMoved;

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

            // Set the Y-coordinates of the Grid controls
            _sliderGridStart.Margin = new Thickness(0, verticalPositionStart, 0, 0);
            _sliderGridEnd.Margin = new Thickness(0, verticalPositionEnd, 0, 0);
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
            string roiStart = TextBoxROIStart.Text;
            string roiEnd = TextBoxROIEnd.Text;
            string pixelSize = PixelSize.Text;
            string orientation = Orientation.Text;
            string marginLeft = CrossMarginTop.Text;
            string marginRight = CrossMarginBottom.Text;
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


            // Create a new ImageCore object with the gathered values and updated image bytes
            ImageCore newItem = new ImageCore(
                name,
                null, // Assuming we don't have an ID for the new item yet
                imageBytes,
                FileType, // Placeholder for image type, not updated here
                Item.Width, // Placeholder for image width, not updated here
                Item.Height, // Placeholder for image height, not updated here
                roiStart,
                roiEnd,
                pixelSize,
                orientation,
                marginRight,
                marginLeft,
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

            ImageSharp?.Dispose();
            // Get the parent window of the UserControl
            var window = this.VisualRoot as Window;

            window.Close();

            //reload the ImportViewPage

            // Define and initialize the GoImport command in the constructor
            
        }


        //private bool IsTiffImage(byte[] imageBytes)
        //{
        //    // Check if the imageBytes represent a TIFF image
        //    return imageBytes.Length > 1 && (imageBytes[0] == 'I' && imageBytes[1] == 'I' || imageBytes[0] == 'M' && imageBytes[1] == 'M');
        //}



    }
}
