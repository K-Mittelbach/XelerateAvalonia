using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XelerateAvalonia.Models;

namespace XelerateAvalonia.Auxiliaries
{
    public class ImageTransformations
    {
        // Rotate an Avalonia ImageControl (FrontEnd) or rotate a bitmap (BackEnd)
        public static void Rotate(Avalonia.Controls.Image image, SixLabors.ImageSharp.Image<Rgba32> imageSharp, ref double totalRotationAngle)
        {
            RotateImageControl(image, ref totalRotationAngle);
            RotateImageSharp(imageSharp);
        }

        public static void RotateImageControl(Avalonia.Controls.Image imageControl, ref double totalRotationAngle)
        {
            if (imageControl != null)
            {
                // Accumulate rotation angle
                totalRotationAngle += 90;

                // Rotate the existing Image control
                imageControl.RenderTransform = new RotateTransform(totalRotationAngle);
            }
        }
        public static void RotateImageSharp(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            image.Mutate(x => x.Rotate(RotateMode.Rotate90));
        }

        public static Image<Rgba32> LoadImageFromBytes(byte[] imageData)
        {
            // Load the image from the byte array
            using (var imageStream = new System.IO.MemoryStream(imageData))
            {
                return SixLabors.ImageSharp.Image.Load<Rgba32>(imageStream);
            }
        }

        public static byte[] GetBytesFromImage(Image<Rgba32> image)
        {
            using (var outputStream = new System.IO.MemoryStream())
            {
                image.Save(outputStream, new PngEncoder());
                return outputStream.ToArray();
            }
        }
        public static void AdjustBrightness(SixLabors.ImageSharp.Image<Rgba32> image, float factor)
        {
            image.Mutate(x => x.Brightness(factor));
        }

        public static Bitmap ConvertToBitmap(Image<Rgba32> imageSharp)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Save the ImageSharp image to a memory stream
                imageSharp.SaveAsBmp(memoryStream);

                // Reset the position to read from the beginning
                memoryStream.Position = 0;

                // Create a new Bitmap from the memory stream
                return new Bitmap(memoryStream);
            }
        }

        public static byte[] CaptureImageSlice(Grid sliderGridLeft, Grid sliderGridRight, Grid sliderGridStart, Grid sliderGridEnd, byte[] imageBytes)
        {
            // Load the image from the byte array to retrieve the original dimensions
            SixLabors.ImageSharp.Image<Rgba32> originalImage = LoadImageFromBytes(imageBytes);

            // Original image dimensions
            int originalWidth = originalImage.Width;
            int originalHeight = originalImage.Height;

            // Displayed image dimensions (assumed to be fixed at 480x780)
            int displayWidth = 175;
            int displayHeight = 780;

            // Calculate downscaling factors for width and height
            double widthDownscaleFactor = (double)originalWidth / displayWidth;
            double heightDownscaleFactor = (double)originalHeight / displayHeight;

            // Get slider margins (scaled to original image dimensions)
            double leftSliderMargin = sliderGridLeft.Margin.Left * widthDownscaleFactor;
            double rightSliderMargin = sliderGridRight.Margin.Left * widthDownscaleFactor;
            double topSliderMargin = sliderGridStart.Margin.Top * heightDownscaleFactor;
            double bottomSliderMargin = sliderGridEnd.Margin.Top * heightDownscaleFactor;

            // Ensure the sliders are within the bounds of the original image
            leftSliderMargin = Math.Max(leftSliderMargin, 0);
            rightSliderMargin = Math.Min(rightSliderMargin, originalWidth);
            topSliderMargin = Math.Max(topSliderMargin, 0);
            bottomSliderMargin = Math.Min(bottomSliderMargin, originalHeight);

            // Calculate slice dimensions
            int sliceWidth = (int)(originalWidth - (leftSliderMargin + rightSliderMargin));
            int sliceHeight = (int)(bottomSliderMargin - topSliderMargin);

            // Calculate the right boundary of the cropped area
            int cropRight = (int)Math.Round(Math.Abs(rightSliderMargin));
            int cropTop = (int)Math.Round(topSliderMargin);


            int cropLeft = (int)Math.Round(leftSliderMargin);

            // Crop the image to obtain the slice
            // WIP Error when image is smaller than cropped image?
            SixLabors.ImageSharp.Image<Rgba32> croppedImage = originalImage.Clone(x => x.Crop(new Rectangle(cropLeft, cropTop, sliceWidth, sliceHeight)));

            // Convert the cropped ImageSharp image to a byte array using GetBytesFromImage method
            return GetBytesFromImage(croppedImage);
        }

        public static byte[] CreateComposite(List<CoreSections> checkedCoreSections, double[] depth)
        {
            // Initialize variables
            List<Image<Rgba32>> images = new List<Image<Rgba32>>();
            int maxWidth = 0;
            int totalHeight = 0;
            int imageCount = 0;

            // Convert each byte array into an image and calculate total height
            foreach (CoreSections section in checkedCoreSections)
            {
                byte[] blobROI = section.BlobROI;

                if (blobROI != null)
                {
                    // Convert byte array to image
                    Image<Rgba32> image = LoadImageFromBytes(blobROI);

                    // Add the image to the list
                    images.Add(image);

                    // Update maximum width
                    maxWidth = Math.Max(maxWidth, image.Width);

                    // Update total height and image count
                    totalHeight += image.Height;
                    imageCount++;
                }
            }

            // Check if there's at least one section without BlobROI
            if (imageCount < checkedCoreSections.Count)
            {
                // Calculate the mean height of the images
                int meanHeight = totalHeight / imageCount;

                // Create the white rectangle with the mean height
                Image<Rgba32> whiteRectangle = new Image<Rgba32>(maxWidth > 0 ? maxWidth : 100, meanHeight);
                whiteRectangle.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.White));

                // Track the index where the white rectangle should be inserted
                int whiteRectangleIndex = 0;
                foreach (CoreSections section in checkedCoreSections)
                {
                    if (section.BlobROI == null)
                    {
                        break;
                    }
                    else
                    {
                        whiteRectangleIndex++;
                    }
                }

                // Insert the white rectangle at the appropriate position in the list
                images.Insert(whiteRectangleIndex, whiteRectangle);
            }

            // Reverse the order of images
            images.Reverse();

            // Recalculate total height
            totalHeight = images.Sum(image => image.Height);

            // Create composite image
            using (Image<Rgba32> compositeImage = new Image<Rgba32>(maxWidth, totalHeight))
            {
                int currentY = 0;

                // Draw each image or white rectangle onto the composite image
                foreach (Image<Rgba32> image in images)
                {
                    compositeImage.Mutate(ctx => ctx.DrawImage(image, new SixLabors.ImageSharp.Point(0, currentY), 1f));
                    currentY += image.Height;
                }

                // Convert composite image to byte array
                return GetBytesFromImage(compositeImage);
            }
        }
    }
}
