using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reactive;
using ReactiveUI;
using XelerateAvalonia.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Splat;
using XelerateAvalonia.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Xml;
using System.Reflection.Metadata;
using System.Reactive.Linq;
using ExcelDataReader;
using System.Text;
using System.Linq;

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

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = "File Import";

        private string _currentFileName;
        public string CurrentFileName
        {
            get => _currentFileName;
            set => this.RaiseAndSetIfChanged(ref _currentFileName, value);
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

            FileList = new ObservableCollection<CoreMeta>();

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
                // File does not exist, handle accordingly (throw exception, return a default value, etc.)
                // For now, returning -1 to indicate an invalid file path
                return -1;
            }
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
                DateTime currentDateTime = DateTime.Now;

                // Create a new CoreMeta instance
                CoreMeta newEntry = new CoreMeta(Corename, ID, DeviceName, InputSource, MeasuredTime, Voltage, Current, size, currentDateTime);

                // Add the new entry to the FileList or perform any other required operations
                FileList.Add(newEntry);
            }
        }


        public object[] MetaCoreReader(DataSet dataset, float size)
        {
            // Extracting Corename from the first row and first column
            string Corename = dataset.Tables[0].Rows[0][0].ToString(); // Assuming "Corename" is in the first row and first column

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

            var encodingUTF8 = System.Text.Encoding.UTF8;

            // Get file size from CurrentFileName
            long sizeInBytes = GetFileSize(CurrentFileName);

            // Convert bytes to kilobytes (if needed)
            float sizeInKb = sizeInBytes / 1024f;

            // Create a list to store the extracted row values
            List<object[]> allRowValues = new List<object[]>();

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
                    var result = reader.AsDataSet();

                    // Extract the data using MetaCoreReader and store row values
                    for (int rowIdx = 2; rowIdx < result.Tables[0].Rows.Count; rowIdx++)
                    {
                        // Extract values from each row using MetaCoreReader
                        object[] rowValues = MetaCoreReader(result, sizeInKb);

                        // Store the row values in the list
                        allRowValues.Add(rowValues);
                    }
                    ProgressValue = "65";
                    // Remove the first two rows from the dataset
                    result.Tables[0].Rows.RemoveAt(0);
                    result.Tables[0].Rows.RemoveAt(0);
                }
            }

            // After processing the entire file, create CoreMeta entry only once
            MetaCoreCreater(allRowValues);

            ProgressValue = "85";

            // Upload both the meta data and dataset into db

            // CONNECTION TO DB via "SessionContext.project path + database.db"

            //1 check if metadataTable already exists --> add new row entry

            //2 add new table with dataset

            
            // Wait for one second
            await Task.Delay(1000);

            ProgressValue = "0";
            ProgressBarBackground = "#1D1D1D";
        }


    }
}