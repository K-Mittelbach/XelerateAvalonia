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
using System.Text.RegularExpressions;

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
                long sizeInBytes = GetFileSize(CurrentFileName);
                float sizeInMb = (float)Math.Round(((sizeInBytes / 1024f) / 1000), 2);

                List<object[]> allRowValues = new List<object[]>();

                DataSet CoreDataSet;

                using (var stream = File.Open(CurrentFileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
                    {
                        FallbackEncoding = Encoding.GetEncoding(1252)
                    }))
                    {
                        CoreDataSet = reader.AsDataSet();

                        for (int rowIdx = 2; rowIdx < CoreDataSet.Tables[0].Rows.Count; rowIdx++)
                        {
                            object[] rowValues = MetaCoreReader(CoreDataSet, sizeInMb);
                            allRowValues.Add(rowValues);
                        }
                        ProgressValue = "50";
                    }
                }

                // Retrieve column headers
                DataRow headersRow = CoreDataSet.Tables[0].Rows[2];
                int numColumns = headersRow.ItemArray.Length;
                List<string> elementNames = new List<string>();

                // Initialize a list of abbreviations of all occurring natural elements
                List<string> naturalElementAbbreviations = new List<string>
                {
                    "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca", "Ti", "V", "Cr", "Mn",
                    "Fe", "Ni", "Cu", "Zn", "Ga", "As", "Br", "Rb", "Sr", "Y", "Zr", "Ag",
                    "Ba", "Ce", "Pr", "Nd", "Sm", "Eu", "Gd", "Tb", "Dy", "Ta", "W", "Co", "Pb"
                };


                // Retrieve all element names with their respective STD in their column and their amount of zeros as a Zero % value
                List<string> elements = new List<string>();
                List<string> elementsSTD = new List<string>();
                List<string> elementsZeroSum = new List<string>();

                for (int i = 0; i < numColumns; i++)
                {
                    // Retrieve the column name
                    string columnHeader = headersRow[i].ToString();

                    // Check if the column header exactly matches any natural element abbreviation
                    if (naturalElementAbbreviations.Contains(columnHeader))
                    {
                        // Get all values in the column starting from the third row
                        var columnValues = new List<double>();
                        foreach (DataRow row in CoreDataSet.Tables[0].Rows)
                        {
                            double value;
                            if (double.TryParse(row[i].ToString(), out value))
                            {
                                columnValues.Add(value);
                            }
                        }

                        // Calculate standard deviation and percentage of zeros for the corresponding column
                        double stdDev = CalculateStandardDeviation(columnValues);
                        double zeroPercentage = CalculateZeroPercentage(columnValues);

                        // Add the element name, standard deviation, and zero percentage to their respective lists
                        elements.Add(columnHeader);
                        elementsSTD.Add(stdDev.ToString());
                        elementsZeroSum.Add(zeroPercentage.ToString());
                    }
                }

                // Fill in missing elements with null values
                List<string> allElements = new List<string>
                    {
                        "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca", "Ti", "V", "Cr", "Mn",
                        "Fe", "Ni", "Cu", "Zn", "Ga", "As", "Br", "Rb", "Sr", "Y", "Zr", "Ag",
                        "Ba", "Ce", "Pr", "Nd", "Sm", "Eu", "Gd", "Tb", "Dy", "Ta", "W", "Co", "Pb"
                    };

                foreach (string element in allElements)
                {
                    if (!elements.Contains(element))
                    {
                        elements.Add(element);
                        elementsSTD.Add(null);
                        elementsZeroSum.Add(null);
                    }
                }

                string databaseFileName = "database.db";
                string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);
                object[] firstRowValues = allRowValues[0];
                
                string Core = (string)firstRowValues[0];
                string validCoreName = Core.Replace("-", "_");

                DBAccess.SaveDataset(CoreDataSet, validCoreName, databasePath);

                // After processing the entire file, create CoreMeta entry only once
                MetaCoreCreater(allRowValues);

                if (allRowValues.Count > 0)
                {
                    firstRowValues = allRowValues[0];
                    UniqueId ID = new UniqueId(Guid.NewGuid());
                    string DeviceName = (string)firstRowValues[1];
                    string InputSource = (string)firstRowValues[2];
                    float MeasuredTime = (float)firstRowValues[3];
                    float Voltage = (float)firstRowValues[4];
                    float Current = (float)firstRowValues[5];
                    float size = (float)firstRowValues[6];
                    DateOnly currentDateTime = DateOnly.FromDateTime(DateTime.Now);

                    CoreMeta newEntry = new CoreMeta(validCoreName, ID, DeviceName, InputSource, MeasuredTime, Voltage, Current, size, currentDateTime);

                    DBAccess.SaveCoreMeta(newEntry, false, databasePath, elements, elementsSTD, elementsZeroSum);
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

            await Task.Run(() =>
            {
                try
                {
                    string[] imageNames = CurrentImageName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); // Split the concatenated string into individual file paths
                    imageNames = imageNames.Select(imageName => imageName.Trim()).ToArray();
                    foreach (string imageName in imageNames)
                    {
                        // 1. Import the image and translate it into a byte array
                        byte[] imageFile = ReadImageFile(imageName.Trim()); // Trim to remove any leading or trailing whitespace

                        // Initial ROI Selection 
                        byte[] imageROI = imageFile;

                        string name = Path.GetFileNameWithoutExtension(imageName);

                        // Retrieve CoreID and Section ID by filename
                        int coreID, sectionID;

                        // Regular expression to match all numbers in the filename (Section ID)
                        var sectionMatches = Regex.Matches(name, @"\d+");

                        // If there are matches, get the last number as sectionID, else default to 1
                        sectionID = sectionMatches.Count > 0 ? int.Parse(sectionMatches[sectionMatches.Count - 1].Value) : 1;

                        // If there's more than one match, get the number before the last one as coreID
                        coreID = sectionMatches.Count > 1 ? int.Parse(sectionMatches[sectionMatches.Count - 2].Value) : 1;

                        string imageType = Path.GetExtension(imageName);

                        UniqueId id = new UniqueId(Guid.NewGuid().ToString());

                        // Load the image directly from the file path
                        using (Image<Rgba32> image = Image.Load<Rgba32>(imageName.Trim())) // Trim to remove any leading or trailing whitespace
                        {
                            // Get the width and height
                            int width = image.Width;
                            int height = image.Height;

                            string imageWidth = width.ToString();
                            string imageHeight = height.ToString();
                            string imagePixelSize = $"{width}x{height}";
                            string imageOrientation = width > height ? "Horizontal" : "Vertical";

                            if (imageOrientation == "Horizontal")
                            {
                                ImageTransformations.RotateImageSharp(image);
                                imageOrientation = "Vertical";
                                imageFile = ImageTransformations.GetBytesFromImage(image);
                            }

                            string imageROIStart = "0";
                            string imageROIEnd = "810";

                            string imageMarginRight = "0";
                            string imageMarginLeft = "0";

                            long sizeInBytes = GetFileSize(imageName);
                            float sizeInMb = (float)Math.Round(((sizeInBytes / 1024f) / 1000), 2);
                            DateOnly uploaded = DateOnly.FromDateTime(DateTime.Now);

                            string databaseFileName = "database.db";
                            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

                            // 3. Create a new Image entry
                            ImageCore newEntryImage = new ImageCore(name, id, imageFile, imageROI, coreID, sectionID, imageType, imageWidth, imageHeight, imageROIStart, imageROIEnd, imagePixelSize, imageOrientation, imageMarginRight, imageMarginLeft, sizeInMb, uploaded, ImageList, databasePath);

                            // 4. Save the Image entry to the database
                            DBAccess.SaveImage(newEntryImage, false, databasePath);

                            // Add the new entry to the FileList or perform any other required operations
                            ImageList.Add(newEntryImage);
                            UploadedFileCount = DBAccess.GetUploadedFileCounts(databasePath);
                        }
                    }
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

        public double CalculateStandardDeviation(IEnumerable<double> values)
        {
            double mean = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - mean) * (val - mean)).Sum();
            double variance = sumOfSquaresOfDifferences / values.Count();
            double standardDeviation = Math.Sqrt(variance);
            return Math.Round(standardDeviation, 2); // Round to two decimal places
        }

        public double CalculateZeroPercentage(IEnumerable<double> values)
        {
            int totalValues = values.Count();
            int zeroCount = values.Count(val => val == 0);
            double zeroPercentage = (double)zeroCount / totalValues * 100.0;
            return zeroPercentage;
        }


    }
}