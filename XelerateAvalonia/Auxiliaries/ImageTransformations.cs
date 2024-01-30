using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

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

    }
}
