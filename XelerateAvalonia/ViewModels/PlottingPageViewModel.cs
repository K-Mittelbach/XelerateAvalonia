using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;
using XelerateAvalonia.Views;

namespace XelerateAvalonia.ViewModels
{
    public class PlottingPageViewModel : ViewModelBase, IRoutableViewModel
    {
        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = "Core Plotting";

        public ReactiveCommand<Unit, IRoutableViewModel> GoImport { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoStatistics { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoDatabase { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> ReloadPage { get; }

        public ReactiveCommand<Unit, Unit> OpenPlotWindow { get; }


        private bool _isDarkModeEnabled;
        public bool IsDarkModeEnabled
        {
            get => _isDarkModeEnabled;
            set => this.RaiseAndSetIfChanged(ref _isDarkModeEnabled, value);
        }

        public RoutingState Router { get; } = new RoutingState();

        private readonly ISessionContext _sessionContext;

        public DataTable PlotData;

        public double[] ImageROI;

        private ObservableCollection<CoreMeta> _dataSets;
        public ObservableCollection<CoreMeta> DataSets
        {
            get => _dataSets;
            set => this.RaiseAndSetIfChanged(ref _dataSets, value);
        }

        private CoreMeta _selectedDataSetItem;
        public CoreMeta SelectedDataSetItem
        {
            get => _selectedDataSetItem;
            set => this.RaiseAndSetIfChanged(ref _selectedDataSetItem, value);
        }

        private ImageCore _selectedImageItem;
        public ImageCore SelectedImageItem
        {
            get => _selectedImageItem;
            set => this.RaiseAndSetIfChanged(ref _selectedImageItem, value);
        }

        private ObservableCollection<ImageCore> _imageList;
        public ObservableCollection<ImageCore> ImageList
        {
            get => _imageList;
            set => this.RaiseAndSetIfChanged(ref _imageList, value);
        }

        private ObservableCollection<NaturalElements> _elementList;
        public ObservableCollection<NaturalElements> ElementList
        {
            get => _elementList;
            set => this.RaiseAndSetIfChanged(ref _elementList, value);
        }

        private ObservableCollection<CoreSections> _coreSections;
        public ObservableCollection<CoreSections> CoreSections
        {
            get => _coreSections;
            set => this.RaiseAndSetIfChanged(ref _coreSections, value);
        }

        public PlottingPageViewModel(IScreen screen, ISessionContext sessionContext)
        {
            HostScreen = screen;
            _sessionContext = sessionContext;

            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            // Load Cores from database

            DataSets = new ObservableCollection<CoreMeta>();
            DataSets = DBAccess.GetAllCoreMetas(databasePath);

            ImageList = new ObservableCollection<ImageCore>();
            ImageList = DBAccess.GetAllImages(databasePath);

            ElementList = new ObservableCollection<NaturalElements>();
            CoreSections = new ObservableCollection<CoreSections>();

            // Define and initialize the GoImport command in the constructor
            GoImport = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new ImportPageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            ReloadPage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new PlottingPageViewModel(HostScreen, sessionContext));
                }
            );

            // Define and initialize the GoImport command in the constructor
            GoHome = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new HomePageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoStatistics = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new StatisticsPageViewModel(HostScreen,sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoDatabase = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new DatabasePageViewModel(HostScreen,sessionContext));
                }
            );
            // Define and initialize the GoImport command in the constructor
            GoSettings = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new SettingsPageViewModel(HostScreen, sessionContext));
                }
            );

            // Define and initialize the OpenPlotWindow command in the constructor
            OpenPlotWindow = ReactiveCommand.Create(() =>
            {

                DataTable Plotting = null; // Initialize Plotting DataTable
                var elementArrays = new Dictionary<string, double[]>();  // Create a dictionary to hold the arrays for each selected element
                                                                         
                // Check for Selected CoreSections
                var checkedCoreSections = CoreSections.Where(cs => cs.IsChecked == "True").ToList();

                if (checkedCoreSections.Any())
                {
                    int startRow = int.Parse(checkedCoreSections.First().StartRow);
                    int endRow = int.Parse(checkedCoreSections.Last().EndRow);

                    // read the datarows for plotting
                    var rowsToPlot = PlotData.AsEnumerable().Skip(startRow - 1).Take(endRow - startRow + 1);

                    Plotting = rowsToPlot.CopyToDataTable();
                    // Now Plotting DataTable contains the rows from PlotData corresponding to the StartRow of the first element
                    // and the EndRow of the last element in checkedCoreSections.
                }

                // Extract the "DepthID" column from the Plotting DataTable
                var positionColumn = Plotting.AsEnumerable()
                   .Select(row => double.Parse(row["DepthID"].ToString()))
                   .ToArray();

                // Check if Plotting DataTable is initialized
                if (Plotting != null)
                {
                    // Check for Selected Elements
                    var checkedElements = ElementList.Where(element => element.IsChecked == "True").ToList();              
                
                    // Loop through all checked elements
                    foreach (var element in checkedElements)
                    {
                        // Extract the values for the current element from the Plotting DataTable
                        var elementValues = Plotting.AsEnumerable()
                            .Select(row => double.Parse(row[element.Name].ToString()))
                            .ToArray();

                        // Add the array of element values to the dictionary with the element's name as the key
                        elementArrays[element.Name] = elementValues;
                    }
                    
                }

                // Create a new window for displaying the plotted data
                Window window = new Window
                {
                    Title = "Core Plotting",
                    Width = 1500,
                    Height = 900,
                    Content = new PlotDisplay(positionColumn,elementArrays,SelectedImageItem,IsDarkModeEnabled),
                };

                // Show the window

                window.Show();
            });

            // Subscribe to the command to execute when the button is clicked
            OpenPlotWindow.Subscribe();

            // Whenever an item is selected update the ElementList
            this.WhenAnyValue(x => x.SelectedDataSetItem)
            .Where(selectedItem => selectedItem != null)
            .Subscribe(selectedItem =>
            {
                if (selectedItem != null)
                {
                    ObservableCollection<NaturalElements> elements = DBAccess.GetAllElements(selectedItem.Name.ToString(), databasePath);

                    ObservableCollection<CoreSections> coreSections = DBAccess.GetAllCoreSections(selectedItem.Name.ToString(), databasePath);
                    PlotData = DBAccess.GetTableAsDataTable(selectedItem.Name.ToString(), databasePath);

                    CoreSections = coreSections;
                    ElementList = elements;
                  
                }
            });
       
        }
    }
}