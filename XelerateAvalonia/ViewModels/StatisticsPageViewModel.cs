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
            "Density-based"
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

        private string _numberOfClusters;
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
        }

        // Logic for creating the Biplot 
        private void CreateBiplot()
        {
            string databaseFileName = "database.db";
            string databasePath = Path.Combine(_sessionContext.ProjectPath, _sessionContext.ProjectName, databaseFileName);

            // Read the data -> SelectedDataSetItem and read from db table by name
            DataTable BiPlotData = DBAccess.GetTableAsDataTable(SelectedDataSetItem.Name, databasePath);

            // Remove the first two columns
            BiPlotData.Columns.RemoveAt(0);
            BiPlotData.Columns.RemoveAt(0);

            // Remove columns with headers containing "inc" or "coh"
            var columnsToRemove = BiPlotData.Columns.Cast<DataColumn>()
                .Where(column => column.ColumnName.ToLower().Contains("inc") || column.ColumnName.ToLower().Contains("coh"))
                .ToList();

            foreach (var column in columnsToRemove)
            {
                BiPlotData.Columns.Remove(column);
            }

            // Convert DataTable to array of arrays (matrix) column by column
            var dataMatrix = Enumerable.Range(0, BiPlotData.Columns.Count)
                .Select(colIndex => BiPlotData.AsEnumerable()
                    .Select(row => row[colIndex])
                    .Select(value => value == DBNull.Value ? 0 : Convert.ToDouble(value))
                    .ToArray())
                .ToArray();

            double[] standardDeviation = new double[dataMatrix[0].Length];
            double[] mean = new double[dataMatrix[0].Length];

            for (int i = 0; i < dataMatrix[0].Length; i++)
            {
                double[] columnValues = dataMatrix.Select(row => row[i]).ToArray();
                standardDeviation[i] = CalculateStandardDeviation(columnValues);
                mean[i] = columnValues.Average();
            }

            // Scaling the data with zero unit variance
            var dataAfterScalingMatrix = dataMatrix.Select(row =>
                row.Select((value, index) => (value - mean[index]) / standardDeviation[index]).ToArray()).ToArray();

            // Apply log2 transformation to each value in the matrix (dataAfterScalingMatrix)
            double[][] loggedDataMatrix = dataAfterScalingMatrix
                .Select(column => column.Select(value => Math.Log(value + 1, 2)).ToArray())
                .ToArray();

            // Create a Principal Component Analysis object
            PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true // This will normalize the data
            };

            // Fit the PCA model to your data
            pca.Learn(loggedDataMatrix);

            // Get the components (eigenvectors) and eigenvalues
            double[][] components = pca.ComponentVectors;

            // Perform dimensionality reduction (optional)
            double[][] transformedData = pca.Transform(loggedDataMatrix);

            // Create a new plot
            AvaPlot plt = new AvaPlot();

            // Plot vectors representing the original variables
            for (int i = 0; i < pca.ComponentVectors.Length; i++)
            {
                // Extract the ith principal component (eigenvector)
                double[] component = pca.ComponentVectors[i];

                // Scale the component for visualization
                double scale = Math.Sqrt(Math.Abs(pca.Eigenvalues[i]));

                // Scale the component vector
                double scaledComponentX = component[0] * scale;
                double scaledComponentY = component[1] * scale;

                // Add arrows representing original variables
                var arrow = plt.Plot.Add.Arrow(0, 0, scaledComponentX, scaledComponentY);
                arrow.ArrowheadLength = 10; // Customize arrowhead length
                arrow.ArrowheadWidth = 5; // Customize arrowhead width

                // Extract element name from column name
                string elementName = BiPlotData.Columns[i].ColumnName;

                // Calculate text label coordinates
                double xText = scaledComponentX * 1.1; // Offset from arrow endpoint along x-axis
                double yText = scaledComponentY * 1.1; // Offset from arrow endpoint along y-axis

                // Add text label next to the arrow
                plt.Plot.Add.Text(elementName, xText, yText);
            }

            // Customize the plot
            plt.Plot.Title("Biplot of Principal Components");
            plt.Plot.XLabel("Principal Component 1");
            plt.Plot.YLabel("Principal Component 2");
            plt.Width = 550;
            plt.Height = 500;

            plt.Plot.Axes.SetLimitsX(-0.005, 0.005);
            plt.Plot.Axes.SetLimitsY(-0.005, 0.005);

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
                case "Density-based":
                    // Density-based clustering logic
                    PerformDensityBasedClustering(clusterData, depth);
                    break;
                default:
                    // Handle unknown clustering methods or provide a default behavior
                    break;
            }
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
            double lastDepth = Convert.ToDouble(depth.Rows[0]["DepthID"]);

            // Iterate through depths in the DataTable
            foreach (DataRow row in depth.Rows)
            {
                double depthValue = Convert.ToDouble(row["DepthID"]);

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
                lastDepth = Convert.ToDouble(row["DepthID"]);
            }
            
            plt.Plot.Add.Bars(bars.ToArray());
            Tick[] ticks =
             {
                new(1, "Core"),
             };

            // Customize the plot
            plt.Plot.Title("Categorical (K-means)");
            plt.Plot.YLabel("Depth (mm)");
            plt.Width = 550;
            plt.Height = 550;
            plt.Plot.HideGrid();
            plt.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            plt.Plot.Axes.Bottom.MajorTickStyle.Length = 0;

            // Assuming "DepthID" is the name of the column
            double minDepth = depth.AsEnumerable().Min(row => Convert.ToDouble(row["DepthID"]));
            double maxDepth = depth.AsEnumerable().Max(row => Convert.ToDouble(row["DepthID"]));

            // Set the limits of the Y-axis in the plot
            plt.Plot.Axes.SetLimitsY(minDepth, maxDepth);
            
            // Assign the new plot to CurrentPlot
            CurrentPlot = plt;

            // Raise event to notify View to display the new plot
            NewPlotRequested?.Invoke(this, EventArgs.Empty);

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