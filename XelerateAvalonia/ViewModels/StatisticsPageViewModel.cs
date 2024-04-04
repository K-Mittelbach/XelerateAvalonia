using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;
using XelerateAvalonia.Views;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Colormaps;
using DynamicData;
using System.Linq;
using Accord.Statistics.Analysis;
using Accord.Math;
using System.ComponentModel;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using SixLabors.ImageSharp.Formats.Png;
using static XelerateAvalonia.Auxiliaries.Clustering;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Xml;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace XelerateAvalonia.ViewModels
{
    public class StatisticsPageViewModel : ViewModelBase, IRoutableViewModel
    {
        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = "Statistical Analyses";

        public ReactiveCommand<Unit, IRoutableViewModel> GoImport { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoPlotting { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoDatabase { get; }

        public ReactiveCommand<Unit, Unit> CreateBiPlotCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateClusterPlotCommand { get; }

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
        private List<string> _clusteringMethods = new List<string>
        {
            "Hierarchical",
            "K-means",
            "DBSCAN"
        };

        public List<string> ClusteringMethods
        {
            get => _clusteringMethods;
            set => this.RaiseAndSetIfChanged(ref _clusteringMethods, value);
        }

        private string _selectedClusteringMethod;
        public string SelectedClusteringMethod
        {
            get => _selectedClusteringMethod;
            set => this.RaiseAndSetIfChanged(ref _selectedClusteringMethod, value);
        }

        private string _isNormalizationEnabledCluster;
        public string IsNormalizationEnabledCluster
        {
            get => _isNormalizationEnabledCluster;
            set => this.RaiseAndSetIfChanged(ref _isNormalizationEnabledCluster, value);
        }
        private Cluster _selectedClusterItem;
        public Cluster SelectedClusterItem
        {
            get => _selectedClusterItem;
            set => this.RaiseAndSetIfChanged(ref _selectedClusterItem, value);
        }

        private ObservableCollection<Cluster> _clusterList;
        public ObservableCollection<Cluster> ClusterList
        {
            get => _clusterList;
            set => this.RaiseAndSetIfChanged(ref _clusterList, value);
        }


        private string _numberOfClusters = "10";

        public string NumberOfClusters
        {
            get => _numberOfClusters;
            set => this.RaiseAndSetIfChanged(ref _numberOfClusters, value);
        }

        private ObservableCollection<NaturalElements> _elements;
        public ObservableCollection<NaturalElements> Elements
        {
            get => _elements;
            set => this.RaiseAndSetIfChanged(ref _elements, value);
        }
        // Event to notify that a new plot needs to be displayed
        public event EventHandler NewPlotRequested;

        private AvaPlot _currentPlot;
        public AvaPlot CurrentPlot
        {
            get => _currentPlot;
            private set => this.RaiseAndSetIfChanged(ref _currentPlot, value);
        }

        public RoutingState Router { get; } = new RoutingState();

        private readonly ISessionContext _sessionContext;

        public StatisticsPageViewModel(IScreen screen, ISessionContext sessionContext)
        {
            HostScreen = screen;
            _sessionContext = sessionContext;

            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            DataSets = new ObservableCollection<CoreMeta>();
            DataSets = DBAccess.GetAllCoreMetas(databasePath);

            ClusterList = new ObservableCollection<Cluster>();
            ClusterList = DBAccess.GetAllClusters(databasePath);
            

            // Initialize the CreatePlotCommand
            CreateBiPlotCommand = ReactiveCommand.Create(CreateBiplot);
            CreateClusterPlotCommand = ReactiveCommand.Create(CreateCluster);
            IsNormalizationEnabledCluster = "False";

            // Define and initialize the GoImport command in the constructor
            GoImport = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new ImportPageViewModel(HostScreen, sessionContext));
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

            // Whenever an item is selected update the ElementList
            this.WhenAnyValue(x => x.SelectedDataSetItem)
            .Where(selectedItem => selectedItem != null)
            .Subscribe(selectedItem =>
            {
                if (selectedItem != null)
                {
                    ObservableCollection<Cluster> clusterList = DBAccess.GetAllClusters(databasePath, selectedItem.Name.ToString());
                    ClusterList = clusterList;
                }
            });
        }

        // Logic for creating the Biplot 
        private void CreateBiplot()
        {
            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            // [0] DATA PREPARATION FOR PCA
            //  Read the data -> SelectedDataSetItem and read from db table by name
            DataTable BiPlotData = DBAccess.GetTableAsDataTable(SelectedDataSetItem.Name, databasePath);

            // Remove the first two columns containing position[mm] and DepthID
            BiPlotData.Columns.RemoveAt(0);
            BiPlotData.Columns.RemoveAt(0);

            // Remove columns with headers containing "inc", "coh", or "cps"
            var columnsToRemove = BiPlotData.Columns.Cast<DataColumn>()
                .Where(column => column.ColumnName.ToLower().Contains("inc") ||
                                 column.ColumnName.ToLower().Contains("coh") ||
                                 column.ColumnName.ToLower().Contains("cps"))
                .ToList();

            foreach (var column in columnsToRemove)
            {
                BiPlotData.Columns.Remove(column);
            }

            // Convert DataTable to double[][]
            double[][] dataMatrix = new double[BiPlotData.Rows.Count][];

            Parallel.For(0, BiPlotData.Rows.Count, row =>
            {
                dataMatrix[row] = new double[BiPlotData.Columns.Count];

                for (int col = 0; col < BiPlotData.Columns.Count; col++)
                {
                    object value = BiPlotData.Rows[row][col];
                    dataMatrix[row][col] = value == DBNull.Value ? 0 : Convert.ToDouble(value);
                }
            });

            // [1] DATA NORMALIZATION
            double[][] stdX = Auxiliaries.DataTransformations.MatStandardize(dataMatrix, out double[] means, out double[] stds);

            // [2] COMPUTING COVARIANCE MATRIX
            double[][] covarMat = Auxiliaries.DataTransformations.CovarMatrix(stdX,false);

            // [3] COMPUTING EIGENVALUES AND EIGENVECTORS
            double[] eigenVals;
            double[][] eigenVecs;

            Auxiliaries.DataTransformations.Eigen(covarMat, out eigenVals, out eigenVecs);

            int[] idxs = Auxiliaries.DataTransformations.ArgSort(eigenVals);  // Sorted indices of eigenvalues

            Array.Sort(eigenVals);
            Array.Reverse(eigenVals);

            eigenVecs = Auxiliaries.DataTransformations.MatExtractCols(eigenVecs, idxs); // Sort eigenvectors
            eigenVecs = Auxiliaries.DataTransformations.MatTranspose(eigenVecs);

            // [4] COMPUTING VARIANCE EXPLAINED
            double[] varExplained = Auxiliaries.DataTransformations.VarExplained(eigenVals);

            // [5] COMPUTING TRANSFORMED DATA
            double[][] transformed = Auxiliaries.DataTransformations.MatProduct(stdX, Auxiliaries.DataTransformations.MatTranspose(eigenVecs));

            // [6] Reconstructing for VALIDATION
            double[][] reconstructed = Auxiliaries.DataTransformations.MatProduct(transformed, eigenVecs);
            for (int i = 0; i < reconstructed.Length; ++i)
                for (int j = 0; j < reconstructed[0].Length; ++j)
                    reconstructed[i][j] = (reconstructed[i][j] * stds[j]) + means[j];


            // Create a new plot
            AvaPlot plt = new AvaPlot();

            // Plot vectors representing the original variables (eigenvectors)
            for (int i = 0; i < eigenVecs.Length; i++)
            {
                double x = eigenVecs[i][0]; // X coordinate of the eigenvector
                double y = eigenVecs[i][1]; // Y coordinate of the eigenvector

                // Plot each eigenvector as an arrow starting from the origin
                var arrow = plt.Plot.Add.Arrow(0, 0, x, y);
                arrow.ArrowheadLength = 10; // Customize arrowhead length
                arrow.ArrowheadWidth = 5; // Customize arrowhead width
                arrow.Color = Colors.LightBlue; // Customize arrow color

                // Extract element name from column name
                string elementName = BiPlotData.Columns[i].ColumnName;

                // Calculate text label coordinates
                double xText = x * 1.1; // Offset from arrow endpoint along x-axis
                double yText = y * 1.1; // Offset from arrow endpoint along y-axis

                // Add text label next to the arrow
                plt.Plot.Add.Text(elementName, xText, yText);
            }

            // Plot the transformed data points
            for (int i = 0; i < transformed.Length; i++)
            {
                double x = transformed[i][0]; // First principal component
                double y = transformed[i][1]; // Second principal component
                var fillColor = Colors.Black;

                if (SelectedClusterItem != null)
                {
                    int[] clusterID = SelectedClusterItem.ClusterID;
                    ScottPlot.Palettes.Category10 palette = new ScottPlot.Palettes.Category10();
                    int colorIndex = clusterID[i]; // Adjust the index as needed
                    fillColor = palette.GetColor(colorIndex);
                }

                // Create a plottable for each data point
                ScottPlot.Plottables.Marker dataPoint = new ScottPlot.Plottables.Marker()
                {
                    X = x,
                    Y = y,
                    Size = 2, // Adjust marker size as needed
                    Color = fillColor, // Customize marker color
                    Shape = MarkerShape.OpenCircle, // Customize marker shape
                };

                // Plot each data point
                plt.Plot.Add.Plottable(dataPoint);
            }


            // Customize the plot
            plt.Plot.Title("Biplot of Principal Components");
            plt.Plot.XLabel($"Principal Component 1 ({varExplained[0]:0.00%})");
            plt.Plot.YLabel($"Principal Component 2 ({varExplained[1]:0.00%})");
            plt.Width = 550;
            plt.Height = 500;

            // Set axis limits (optional)
            plt.Plot.Axes.SetLimitsX(-0.7, 0.7);
            plt.Plot.Axes.SetLimitsY(-0.7, 0.7);

            // Assign the new plot to CurrentPlot
            CurrentPlot = plt;

            // Raise event to notify View to display the new plot
            NewPlotRequested?.Invoke(this, EventArgs.Empty);
        }

        // Logic for creating the Clusterplot using Depth and Concentration data
        private void CreateCluster()
        {
            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            // Read the data -> SelectedDataSetItem and read from db table by name
            DataTable clusterData = DBAccess.GetTableAsDataTable(SelectedDataSetItem.Name, databasePath);

            // Remove the first column "position[mm]"
            clusterData.Columns.RemoveAt(0);

            // Cut DepthID column from dataset for later
            DataTable depth = new DataTable();
            depth.Columns.Add("DepthID", typeof(double)); // Assuming DepthID is of type double

            // Copy DepthID column values from clusterData to depth DataTable
            foreach (DataRow row in clusterData.Rows)
            {
                depth.Rows.Add(Convert.ToDouble(row["DepthID"])); // Assuming DepthID is the name of the column in clusterData
            }

            // Remove DepthID column from clusterData
            clusterData.Columns.Remove("DepthID");
     
            // Remove columns with headers containing "inc" or "coh"
            var columnsToRemove = clusterData.Columns.Cast<DataColumn>()
                .Where(column => column.ColumnName.ToLower().Contains("inc") || column.ColumnName.ToLower().Contains("coh"))
                .ToList();

            foreach (var column in columnsToRemove)
            {
                clusterData.Columns.Remove(column);
            }

            if (IsNormalizationEnabledCluster == "True")
            {
                // Min-Max normalization for each column
                foreach (DataColumn column in clusterData.Columns)
                {
                    double minValue = double.MaxValue;
                    double maxValue = double.MinValue;

                    // Find min and max values in the column
                    foreach (DataRow row in clusterData.Rows)
                    {
                        double value = Convert.ToDouble(row[column]);
                        minValue = Math.Min(minValue, value);
                        maxValue = Math.Max(maxValue, value);
                    }

                    // Apply min-max normalization to each value in the column
                    for (int i = 0; i < clusterData.Rows.Count; i++)
                    {
                        double value = Convert.ToDouble(clusterData.Rows[i][column]);
                        double normalizedValue = (value - minValue) / (maxValue - minValue);
                        clusterData.Rows[i][column] = normalizedValue;
                    }
                }
            }

            string ClusterNumber = NumberOfClusters;
            string clusteringMethod = SelectedClusteringMethod.ToString();

            switch (clusteringMethod)
            {
                case "K-means":
                    // K-Means clustering logic
                    PerformKMeansClustering(clusterData, depth, ClusterNumber);
                    break;
                case "Hierarchical":
                    // Hierarchical clustering logic
                    PerformHierarchicalClustering(clusterData, depth);
                    break;
                case "DBSCAN":
                    // Density-based clustering logic
                    PerformDensityBasedClustering(clusterData, depth);
                    break;
                default:
                    // Handle unknown clustering methods or provide a default behavior
                    break;
            }
            ClusterList = DBAccess.GetAllClusters(databasePath, SelectedDataSetItem.Name.ToString());
        }

        private void PerformKMeansClustering(DataTable data, DataTable depth, string ClusterNumber)
        {
            // Assuming data is not null and has rows
            double[][] dataArray = data.AsEnumerable()
                                       .Select(row => row.ItemArray.Select(item =>
                                            item == DBNull.Value ? 0.0 : Convert.ToDouble(item)).ToArray())
                                       .ToArray();


            // If normalizing is enabled 

            KMeans km = new KMeans(dataArray, int.Parse(NumberOfClusters));
            
            int[] clustering = km.clustering;

            // Create a new plot
            AvaPlot plt = new AvaPlot();

            ScottPlot.Palettes.Category10 palette = new();

            List<ScottPlot.Bar> bars = new List<ScottPlot.Bar>();

            // Starting iteration for the first depth
            int position = 0;

            // Define the first depth value
            double lastDepth = Convert.ToDouble(depth.Rows[0]["DepthID"]) / 1000.0;

            // Iterate through depths in the DataTable
            foreach (DataRow row in depth.Rows)
            {
                double depthValue = Convert.ToDouble(row["DepthID"]) / 1000.0;

                // Determine color 

                int colorIndex = clustering[position];
                var fillColor = palette.GetColor(colorIndex);

                // Create a stacked bar for the current depth
                ScottPlot.Bar bar = new ScottPlot.Bar()
                {
                    Position = 1,
                    ValueBase = lastDepth,
                    Value = depthValue,
                    FillColor = fillColor,
                    BorderColor= fillColor
                };

                bars.Add(bar);

                position++; // Move to the next position for the next depth
                lastDepth = Convert.ToDouble(row["DepthID"]) / 1000.0;
            }
            
            plt.Plot.Add.Bars(bars.ToArray());
            Tick[] ticks =
             {
                new(1, "Core"),
             };

            // Customize the plot
            plt.Plot.Title("Categorical (K-means)");
            plt.Plot.YLabel("Depth (m)");
            plt.Width = 350;
            plt.Height = 800;
            plt.Plot.HideGrid();
            plt.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            plt.Plot.Axes.Bottom.MajorTickStyle.Length = 0;

            plt.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#D3D3D3");
            plt.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            plt.Plot.Axes.Left.Label.ForeColor = Colors.Black;
            plt.Plot.Axes.Bottom.Label.ForeColor = Colors.Black;
            plt.Plot.Axes.Bottom.MajorTickStyle.Color = Colors.Black;
            plt.Plot.Axes.Left.MajorTickStyle.Color = Colors.Black;
            plt.Plot.Axes.Left.TickLabelStyle.ForeColor = Colors.Black;
            plt.Plot.Axes.Bottom.TickLabelStyle.ForeColor = Colors.Black;

            plt.Plot.Axes.Bottom.MinorTickStyle.Length = 5;
            plt.Plot.Axes.Bottom.MinorTickStyle.Width = 0.5f;
            plt.Plot.Axes.Bottom.MinorTickStyle.Color = Colors.Transparent;
            plt.Plot.Axes.Bottom.FrameLineStyle.Color = Colors.Transparent;

            plt.Plot.Axes.Left.MinorTickStyle.Length = 5;
            plt.Plot.Axes.Left.MinorTickStyle.Width = 0.5f;
            plt.Plot.Axes.Left.MinorTickStyle.Color = Colors.Transparent;
            plt.Plot.Axes.Left.FrameLineStyle.Color = Colors.Transparent;

            plt.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#E0E0E0");
            plt.Plot.Axes.Left.Label.FontSize = 11;

            // Assuming "DepthID" is the name of the column
            double minDepth = depth.AsEnumerable().Min(row => Convert.ToDouble(row["DepthID"]) / 1000.0);
            double maxDepth = depth.AsEnumerable().Max(row => Convert.ToDouble(row["DepthID"]) / 1000.0);

            // Set the limits of the Y-axis in the plot
            plt.Plot.Axes.SetLimitsY(minDepth, maxDepth);


            // -----  Saving the Cluster in the DB
            // Generate a unique ID
            UniqueId id = new UniqueId(Guid.NewGuid());

            plt.Plot.SavePng("XelerateClusterPlot.png", 350, 800);
            // Load the PNG image into a SixLabors Image variable
            Image<Rgba32> plot = SixLabors.ImageSharp.Image.Load<Rgba32>("XelerateClusterPlot.png");

            // Delete the PNG file
            File.Delete("XelerateClusterPlot.png");

            byte[] pltImage = Auxiliaries.ImageTransformations.GetBytesFromImage(plot);

            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            Cluster CurrentClusterPlot = new Cluster(SelectedDataSetItem.Name, id, pltImage, SelectedClusteringMethod, int.Parse(ClusterNumber), clustering);

            // Save created Cluster in db 
            DBAccess.SaveCluster(CurrentClusterPlot, false, databasePath);


            // Assign the new plot to CurrentPlot // Dendorg
            //CurrentPlot = plt;

            // Raise event to notify View to display the new plot
            //NewPlotRequested?.Invoke(this, EventArgs.Empty);

        }

        private void PerformHierarchicalClustering(DataTable data, DataTable depth)
        {

            
        }
    
        private void PerformDensityBasedClustering(DataTable data, DataTable depth)
        {
            

        }

        private double CalculateStandardDeviation(double[] values)
        {
            double average = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / values.Length);
        }
        
    }
}