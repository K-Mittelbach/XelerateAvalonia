using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BitMiracle.LibTiff.Classic;
using ReactiveUI;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XelerateAvalonia.Services;
using XelerateAvalonia.Views;

namespace XelerateAvalonia.Models
{
    public class ImageCore
    {
        public string Name { get; set; }

        public UniqueId ID {get; set;}

        public byte[] Blob { get; set; }

        public byte[] BlobROI { get; set; }

        public string FileType { get; set; }

        public int CoreID { get; set; }

        public int SectionID { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public string ROIStart { get; set; }

        public string ROIEnd { get; set; }

        public string ImagePixelSize { get; set; }

        public string ImageOrientation { get; set; }

        public string ImageMarginLeft { get; set; }

        public string ImageMarginRight { get; set; }

        public float Size { get; set; }

        public DateOnly Uploaded { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteImageCommand { get; }

        public ReactiveCommand<Unit, Unit> DisplayImageCommand { get; }

        public ReactiveCommand<Unit, Unit> DisplayImageSettingsCommand { get; }


        // Reference to the parent collection (ImageList)
        private ObservableCollection<ImageCore> parentCollection;


        public ImageCore(string name, UniqueId id ,byte[] blob , byte[] blobROI, int coreID, int sectionID, string fileType, string width, string height, string ROIstart, string ROIend, string imagePixelSize, string imageOrientation, string imageMarginLeft, string imageMarginRight , float size, DateOnly uploaded, ObservableCollection<ImageCore> parentCollection,string databasePath)
        {
            Name = name;
            ID = id;
            Blob = blob;
            BlobROI = blobROI;
            CoreID = coreID;
            SectionID = sectionID;
            FileType = fileType;
            Width = width;
            Height = height;
            ROIStart = ROIstart;
            ROIEnd = ROIend;
            ImagePixelSize = imagePixelSize;
            ImageOrientation = imageOrientation;
            ImageMarginLeft = imageMarginLeft;
            ImageMarginRight = imageMarginRight;
            Size = size;
            Uploaded = uploaded;

            // Set the reference to the parent collection
            this.parentCollection = parentCollection;

            // Create the ReactiveCommand with a delegate
            DeleteImageCommand = ReactiveCommand.Create(() => DeleteImage(databasePath));

            DisplayImageCommand = ReactiveCommand.Create(() => DisplayImage(databasePath));

            DisplayImageSettingsCommand = ReactiveCommand.Create(() => DisplayImageSettings());
       
        }

        public void DeleteImage(string databasePath)
        {
            Console.WriteLine($"Deleting image: {Name}");
                        
            // Notify the parent collection to remove the current instance
            parentCollection.Remove(this);
            DBAccess.RemoveImage(this.Name, databasePath);

        }

        //Displaying a window with the image and adjustable ROI
        public void DisplayImage(string databasePath)
        {
            Console.WriteLine($"Displaying image: {Name}");

            // Create a new window
            Window window = new Window
            {
                Title = "Image ROI Calibration / Settings",
                Width = 915,
                Height = 900,
                Content = new ImageDisplay(CreateImageControl(),this,databasePath),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            // Show the window
            
            window.Show();
            
        }

       

        //Displaying a window with all properties that are adjustable for the user of the specific image
        public void DisplayImageSettings()
        {
            Console.WriteLine($"Displaying Settings for image: {Name}");

            // Create a new window
            Window window = new Window
            {
                Title = "Image Settings",
                Width = 600,
                Height = 700,
                Content = new ImageSettingsDisplay(this),
            };

            // Show the window

            window.Show();
        }
      
        private Image CreateImageControl()
        {
            // Create an Image control
            Image imageControl = new Image();

            // Load the image from the Blob byte array
            using (MemoryStream stream = new MemoryStream(Blob))
            {
                    // Process other image formats
                    Bitmap bitmap = new Bitmap(stream);
                    imageControl.Source = bitmap;
                
            }

            return imageControl;
        }

        // Legacy code, might be useful if .tif image is mandatory
        private Avalonia.Media.Imaging.Bitmap LoadTiffImage(Stream stream)
        {
            // open a TIFF stored in the stream
            using (var tifImg = Tiff.ClientOpen("in-memory", "r", stream, new TiffStream()))
            {
                // read the dimensions
                var width = tifImg.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                var height = tifImg.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                // create the SKBitmap
                var skBitmap = new SKBitmap();
                var info = new SKImageInfo(width, height);

                // create the buffer that will hold the pixels
                var raster = new int[width * height];

                // get a pointer to the buffer, and give it to the bitmap
                var ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);
                skBitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes, null, (addr, ctx) => ptr.Free(), null);

                // read the image into the memory buffer
                if (!tifImg.ReadRGBAImageOriented(width, height, raster, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT))
                {
                    // not a valid TIF image.
                    return null;
                }

                // swap the red and blue because SkiaSharp may differ from the tiff
                if (SKImageInfo.PlatformColorType == SKColorType.Bgra8888)
                {
                    SKSwizzle.SwapRedBlue(ptr.AddrOfPinnedObject(), raster.Length);
                }

                // Convert SKBitmap to System.Drawing.Bitmap
                var systemDrawingBitmap = new System.Drawing.Bitmap(width, height, PixelFormat.Format32bppArgb);
                var bitmapData = systemDrawingBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, systemDrawingBitmap.PixelFormat);

                // Convert int[] to byte[] before copying
                var byteBuffer = new byte[raster.Length * sizeof(int)];
                Buffer.BlockCopy(raster, 0, byteBuffer, 0, byteBuffer.Length);

                Marshal.Copy(byteBuffer, 0, bitmapData.Scan0, byteBuffer.Length);

                systemDrawingBitmap.UnlockBits(bitmapData);

                // Convert System.Drawing.Bitmap to Avalonia.Media.Imaging.Bitmap
                using (MemoryStream memory = new MemoryStream())
                {
                    systemDrawingBitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    return new Avalonia.Media.Imaging.Bitmap(memory);
                }
            }
        }


    }
}
