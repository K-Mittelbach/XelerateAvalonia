using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reactive;
using ReactiveUI;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;

using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Xml;

using System.Reactive.Linq;
using ExcelDataReader;
using System.Text;
using System.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using System.Collections.Specialized;
using SixLabors.ImageSharp.Formats.Png;
using XelerateAvalonia.Auxiliaries;

namespace XelerateAvalonia.ViewModels
{
    public class ImportPageViewModel : ViewModelBase, IRoutableViewModel
    {
        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        private ObservableCollection<CoreMeta> _fileList;
        public ObservableCollection<CoreMeta> FileList
        {
            get => _fileList;
            set => this.RaiseAndSetIfChanged(ref _fileList, value);
        }
        private ObservableCollection<ImageCore> _imageList;
        public ObservableCollection<ImageCore> ImageList
        {
            get => _imageList;
            set => this.RaiseAndSetIfChanged(ref _imageList, value);
        }

        
        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = "File Import";

        private string _currentFileName;
        public string CurrentFileName
        {
            get => _currentFileName;
            set => this.RaiseAndSetIfChanged(ref _currentFileName, value);
        }

        private string _uploadedFileCount;
        public string UploadedFileCount
        {
            get => _uploadedFileCount   ;
            set => this.RaiseAndSetIfChanged(ref _uploadedFileCount, value);
        }

        private string _currentImageName;
        public string CurrentImageName
        {
            get => _currentImageName;
            set => this.RaiseAndSetIfChanged(ref _currentImageName, value);
        }

        private string _progressBarBackground = "#1D1D1D";
        public string ProgressBarBackground
        {
            get => _progressBarBackground;
            set => this.RaiseAndSetIfChanged(ref _progressBarBackground, value);
        }

        private string _progressValue;
        public string ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        
        public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoImage { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoPlotting { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoDatabase { get; }

       
        public RoutingState Router { get; } = new RoutingState();

        private readonly ISessionContext _sessionContext;

        public ImportPageViewModel(IScreen screen, ISessionContext sessionContext)
        {
            HostScreen = screen;
            _sessionContext = sessionContext;

            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            // Load Cores from database

            FileList = new ObservableCollection<CoreMeta>();
            FileList = DBAccess.GetAllCoreMetas(databasePath);

            ImageList = new ObservableCollection<ImageCore>();
            ImageList = DBAccess.GetAllImages(databasePath);

            UploadedFileCount = DBAccess.GetUploadedFileCounts(databasePath);

            // Subscribe to CollectionChanged event of ImageList
            ImageList.CollectionChanged += OnImageListChanged;

            // Define and initialize the GoImport command in the constructor
            GoHome = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new HomePageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoImage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new ImagePageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoPlotting = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new PlottingPageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoDatabase = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new DatabasePageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoSettings = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new SettingsPageViewModel(HostScreen, sessionContext));
                }
            );


            this.WhenAnyValue(x => x.CurrentFileName)
               .Where(CurrentFileName => CurrentFileName != null)
               .Subscribe(_ => DatasetImport());

            this.WhenAnyValue(x => x.CurrentImageName)
               .Where(CurrentImageName => CurrentImageName != null)
               .Subscribe(_ => ImageImport());
        }

        // Method to get the file size
        public long GetFileSize(string filePath)
        {
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Create a FileInfo object using the file path
                FileInfo fileInfo = new FileInfo(filePath);

                // Get the file size in bytes
                long fileSizeInBytes = fileInfo.Length;

                // Return the file size
                return fileSizeInBytes;
            }
            else
            {
                //  returning -1 to indicate an invalid file path
                return -1;
            }
        }
        private void OnImageListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Update UploadedFileCount whenever ImageList changes
            UploadedFileCount = "(" + (ImageList.Count + FileList.Count).ToString() +")";
        }

        // Method to create CoreMeta entry only once
        public void MetaCoreCreater(List<object[]> allRowValues)
        {
            // Extract values from the first set of row values
            object[] firstRowValues = allRowValues.FirstOrDefault();

            if (firstRowValues != null)
            {
                // Extract values from the first row
                string Corename = (string)firstRowValues[0];
                string DeviceName = (string)firstRowValues[1];
                string InputSource = (string)firstRowValues[2];
                float MeasuredTime = (float)firstRowValues[3];
                float Voltage = (float)firstRowValues[4];
                float Current = (float)firstRowValues[5];
                float size = (float)firstRowValues[6];

                // Generate a unique ID
                UniqueId ID = new UniqueId(Guid.NewGuid());

                // Get current DateTime
                DateOnly uploaded = DateOnly.FromDateTime(DateTime.Now);

                // Create a new CoreMeta instance
                CoreMeta newEntry = new CoreMeta(Corename, ID, DeviceName, InputSource, MeasuredTime, Voltage, Current, size, uploaded);

                // Add the new entry to the FileList or perform any other required operations
                FileList.Add(newEntry);
            }
        }


        public object[] MetaCoreReader(DataSet dataset, float size)
        {

            // ------- WIP!
            // Get Settings from document.txt



            string Corename = dataset.Tables[0].Rows[0][0].ToString(); // Assuming "Corename" is in the first row and first column

            // Remove "_results" if it exists in Corename
            Corename = Corename.Replace("_results", "");

            // Assuming the Settings row structure is consistent for Input Source, Measured Time, Voltage, and Current
            string settingsRow = dataset.Tables[0].Rows[1][0].ToString(); // Assuming Settings row is the second row and in the first column

            // Extracting Input Source, Measured Time, Voltage, and Current from the Settings row
            string[] settings = settingsRow.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Extract Input Source by removing "Input Source" and handling 0 values
            string InputSource = settings[1];

            // Extract Measured Time, Voltage, and Current, handling potential 0 values
            float MeasuredTime = float.TryParse(settings[1], out float time) ? time : 0;
            float Voltage = float.TryParse(settings[3], out float voltage) ? voltage : 0;
            float Current = float.TryParse(settings[5], out float current) ? current : 0;

            // Default DeviceName
            string DeviceName = "ITRAX";

            // Create an array containing the extracted values
            object[] rowValues = new object[] { Corename, DeviceName, InputSource, MeasuredTime, Voltage, Current, size };

            return rowValues;
        }


        // Method to handle dataset import
        public async void DatasetImport()
        {
            ProgressBarBackground = "#313131";
            ProgressValue = "30";

            await Task.Run(() =>
            {

                var encodingUTF8 = System.Text.Encoding.UTF8;

            // Get file size from CurrentFileName
            long sizeInBytes = GetFileSize(CurrentFileName);

            // Convert bytes to kilobytes (if needed)
            float sizeInMb = (float)Math.Round(((sizeInBytes / 1024f) / 1000), 2);

            // Create a list to store the extracted row values
            List<object[]> allRowValues = new List<object[]>();

            DataSet CoreDataSet;


            // Take CurrentFileName and import this ExcelFile into a dataSet
            using (var stream = File.Open(CurrentFileName, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
                {
                    FallbackEncoding = Encoding.GetEncoding(1252)  // Set the fallback encoding 
                }))
                {
                    // Read the entire content of the Excel file
                    CoreDataSet = reader.AsDataSet();

                    // Extract the data using MetaCoreReader and store row values
                    for (int rowIdx = 2; rowIdx < CoreDataSet.Tables[0].Rows.Count; rowIdx++)
                    {
                        // Extract values from each row using MetaCoreReader
                        object[] rowValues = MetaCoreReader(CoreDataSet, sizeInMb);

                        // Store the row values in the list
                        allRowValues.Add(rowValues);
                    }
                    ProgressValue = "50";
                    
                }
            }

            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            string Core = Path.GetFileNameWithoutExtension(CurrentFileName);

            DBAccess.SaveDataset(CoreDataSet, Core, databasePath);


            // After processing the entire file, create CoreMeta entry only once
            MetaCoreCreater(allRowValues);

            ProgressValue = "65";


                // Convert the first row of allRowValues to a CoreMeta object
                if (allRowValues.Count > 0)
            {
                object[] firstRowValues = allRowValues[0];
                string Corename = (string)firstRowValues[0];
                UniqueId ID = new UniqueId(Guid.NewGuid());
                string DeviceName = (string)firstRowValues[1];
                string InputSource = (string)firstRowValues[2];
                float MeasuredTime = (float)firstRowValues[3];
                float Voltage = (float)firstRowValues[4];
                float Current = (float)firstRowValues[5];
                float size = (float)firstRowValues[6];
                DateOnly currentDateTime = DateOnly.FromDateTime(DateTime.Now);

                CoreMeta newEntry = new CoreMeta(Corename, ID, DeviceName, InputSource, MeasuredTime, Voltage, Current, size, currentDateTime);

                // Save CoreMeta object to the database only once
                DBAccess.SaveCoreMeta(newEntry, false, databasePath);
            }
                UploadedFileCount = DBAccess.GetUploadedFileCounts(databasePath);
                ProgressValue = "80";
                           
            });

            ProgressValue = "0";
            ProgressBarBackground = "#1D1D1D";
           
        }

        public async void ImageImport()
        {
            ProgressBarBackground = "#313131";
            ProgressValue = "30";
            string imageWidth;
            string imageHeight;
            string imagePixelSize;
            string imageOrientation;

            await Task.Run(() =>
            {
                try
                {
                    // 1. Import the image and translate it into a byte array
                    byte[] imageFile = ReadImageFile(CurrentImageName);

                    // 2. Get other necessary information
                    string name = Path.GetFileNameWithoutExtension(CurrentImageName);

                    string imageType = Path.GetExtension(CurrentImageName);

                    UniqueId id = new UniqueId(Guid.NewGuid().ToString());

                    // Load the image directly from the file path
                    using (Image<Rgba32> image = Image.Load<Rgba32>(CurrentImageName))
                    {
                        // Get the width and height
                        int width = image.Width;
                        int height = image.Height;

                        imageWidth = width.ToString();
                        imageHeight = height.ToString();

                                             
                        imagePixelSize = $"{width}x{height}";

                        imageOrientation = width > height ? "Horizontal" : "Vertical";

                        if (imageOrientation == "Horizontal")
                        {
                            ImageTransformations.RotateImageSharp(image);
                            imageOrientation = "Vertical";
                            imageFile = ImageTransformations.GetBytesFromImage(image);
                        }
                        
                        
                    }
                    

                    string imageROIStart = "0";
                    string imageROIEnd = imageHeight;
                                                           
 
                    string imageMarginRight ="0";
                    string imageMarginLeft = "0";

                    long sizeInBytes = GetFileSize(CurrentImageName);
                    float sizeInMb = (float)Math.Round(((sizeInBytes / 1024f) / 1000),2);
                    DateOnly uploaded = DateOnly.FromDateTime(DateTime.Now);

                    string databaseFileName = "database.db";
                    string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

                    // 3. Create a new Image entry
                    ImageCore newEntryImage = new ImageCore(name, id, imageFile, imageType, imageWidth,imageHeight,imageROIStart,imageROIEnd, imagePixelSize,imageOrientation,imageMarginRight,imageMarginLeft, sizeInMb, uploaded, ImageList, databasePath);

                    // 4. Save the Image entry to the database
                    DBAccess.SaveImage(newEntryImage, false, databasePath);

                     // Add the new entry to the FileList or perform any other required operations
                    ImageList.Add(newEntryImage);
                    UploadedFileCount = DBAccess.GetUploadedFileCounts(databasePath);

                }
                catch (Exception ex)
                {
                    // Handle exceptions, log errors, or display messages as needed
                    Console.WriteLine($"Error importing image: {ex.Message}");
                }
                finally
                {
                    // Reset progress values after the operation
                    ProgressValue = "0";
                    ProgressBarBackground = "#1D1D1D";
                }
            });
        }


        //Reading an image file and converting it into a byte array, converting everyting to a .png for easier usage
        private byte[] ReadImageFile(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(fileStream))
                {
                    // Convert the image to PNG format and return the byte array
                    using (MemoryStream pngMemoryStream = new MemoryStream())
                    {
                        image.Save(pngMemoryStream, new PngEncoder());
                        return pngMemoryStream.ToArray();
                    }
                }
            }
        }


    }
}