using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot.Avalonia;
using XelerateAvalonia.Models;
using System.Collections.Generic;
using ScottPlot;
using XelerateAvalonia.Auxiliaries;
using System.Linq;
using Avalonia.Media.Imaging;
using System.IO;

namespace XelerateAvalonia.Views
{
    public partial class PlotDisplay : UserControl
    {
        public PlotDisplay(double[] Depth, Dictionary<string, double[]> Elements, byte[]? SelectedImageItem, Cluster? SelectedClusterItem, bool IsDarkModeEnabled)
        {
            InitializeComponent();

            Avalonia.Controls.Image _imageControl = this.FindControl<Avalonia.Controls.Image>("ImageControl");

            Avalonia.Controls.Image _imageCluster = new Avalonia.Controls.Image();
            _imageCluster.Name = "ImageCluster";
            _imageCluster.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            _imageCluster.Margin = new Avalonia.Thickness(0, 0, 0, 0);
          
            Avalonia.Controls.Grid AvaPlotsGrid = this.FindControl<Avalonia.Controls.Grid>("AvaPlotsGrid");
            
            // Create a list to store AvaPlot instances
            List<AvaPlot> avaPlots = new List<AvaPlot>();

            // Convert depth from millimeters to meters
            Depth = Depth.Select(d => d / 1000.0).ToArray();

            if (SelectedImageItem != null)
            {
                _imageControl.Source = ImageTransformations.ConvertToBitmap(ImageTransformations.LoadImageFromBytes(SelectedImageItem));
                // Create a new AvaPlot object
                AvaPlot avaImagePlot = new AvaPlot();

                //add a rectangle by specifying points
                avaImagePlot.Plot.Add.Rectangle(Depth.Min(), Depth.Max(), Depth.Min(), Depth.Max());
                avaImagePlot.Plot.Title("Cr");
                avaImagePlot.Width = 350;
                avaImagePlot.Height = 800;
                avaImagePlot.Plot.HideGrid();
                avaImagePlot.Plot.YLabel("Depth [m]");
                avaImagePlot.Plot.Axes.AutoScaler.InvertedY = true;
                avaImagePlot.Plot.Axes.SetLimitsY(Depth.Min(), Depth.Max());

                // Apply dark mode or standard mode
                if (IsDarkModeEnabled == true)
                {
                    ApplyDarkMode(avaImagePlot);
                }
                else
                {
                    ApplyStandardMode(avaImagePlot);
                }

                avaImagePlot.Plot.Axes.Title.Label.ForeColor = Colors.Transparent;
                avaImagePlot.Plot.Axes.SetLimitsX(0, 1);
                avaPlots.Add(avaImagePlot);
            }
            

            // Create AvaPlot objects dynamically and add them to the avaPlots list
            foreach (var element in Elements)
            {
                string elementName = element.Key;
                double[] Concentration = element.Value;

                // Create a new AvaPlot object
                AvaPlot avaPlot = new AvaPlot();
                avaPlot.Plot.YLabel("Depth [m]");
                avaPlot.Plot.Title(elementName);
                var sig1 = avaPlot.Plot.Add.SignalXY(Depth, Concentration);
                sig1.Label = elementName;
                avaPlot.Plot.Axes.SetLimitsX(Concentration.Min(), Concentration.Max());
                avaPlot.Plot.Axes.SetLimitsY(Depth.Min(), Depth.Max());
                sig1.Data.Rotated = true;
                avaPlot.Plot.HideGrid();

                // Apply dark mode or standard mode
                if (IsDarkModeEnabled == true)
                {
                    ApplyDarkMode(avaPlot);
                }
                else
                {
                    ApplyStandardMode(avaPlot);
                }

                // Add the AvaPlot object to the avaPlots list
                avaPlots.Add(avaPlot);
            }
            int columnOffset = SelectedClusterItem != null ? 2 : 1; // Adjust the column offset
            // Add the ImageCluster 
            if (SelectedClusterItem != null)
            {
                _imageCluster.Source = ImageTransformations.ConvertToBitmap(ImageTransformations.LoadImageFromBytes(SelectedClusterItem.ClusterPlot));
                AvaPlotsGrid.Children.Add(_imageCluster);
                Grid.SetRow(_imageCluster, columnOffset-1); // Set the row to 0
                Grid.SetColumn(_imageCluster, columnOffset-1); // Set the column to 0
            }

            // Access each AvaPlot instance by its index in the avaPlots list
            for (int i = 0; i < avaPlots.Count; i++)
            {
                AvaPlot avaPlot = avaPlots[i];
                if (SelectedImageItem != null && i == 0)
                {
                    Grid.SetColumn(avaPlot, i);
                }
                else
                {
                    Grid.SetColumn(avaPlot, i + columnOffset);
                }
                Grid.SetRow(avaPlot, 0);
                AvaPlotsGrid.Children.Add(avaPlot);
                avaPlot.Width = 350;
                avaPlot.Height = 800;
                
                
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void ApplyDarkMode(AvaPlot avaPlot)
        {
            // Apply dark mode to the specific AvaPlot or its elements
            
            avaPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#111619");
            avaPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#0b3049");
            avaPlot.Plot.Axes.Title.Label.ForeColor = Colors.White;
            avaPlot.Plot.Axes.Left.Label.ForeColor = Colors.White;
            avaPlot.Plot.Axes.Bottom.Label.ForeColor = Colors.White;
            avaPlot.Plot.Axes.Bottom.MajorTickStyle.Color = Colors.White;
            avaPlot.Plot.Axes.Left.MajorTickStyle.Color = Colors.White;
            avaPlot.Plot.Axes.Left.TickLabelStyle.ForeColor = Colors.White;
            avaPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = Colors.White;

            avaPlot.Plot.Axes.Bottom.MinorTickStyle.Length = 5;
            avaPlot.Plot.Axes.Bottom.MinorTickStyle.Width = 0.5f;
            avaPlot.Plot.Axes.Bottom.MinorTickStyle.Color = Colors.Transparent;
            avaPlot.Plot.Axes.Bottom.FrameLineStyle.Color = Colors.Transparent;

            avaPlot.Plot.Axes.Left.MinorTickStyle.Length = 5;
            avaPlot.Plot.Axes.Left.MinorTickStyle.Width = 0.5f;
            avaPlot.Plot.Axes.Left.MinorTickStyle.Color = Colors.Transparent;
            avaPlot.Plot.Axes.Left.FrameLineStyle.Color = Colors.Transparent;

            avaPlot.Plot.Grid.MajorLineColor = Color.FromHex("#0e3d54");
            avaPlot.Plot.Axes.Left.Label.FontSize = 11;
           
        }

        private void ApplyStandardMode(AvaPlot avaPlot)
        {
            // Apply standard mode to the specific AvaPlot or its elements
            avaPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#D3D3D3");
            avaPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            avaPlot.Plot.Axes.Left.Label.ForeColor = Colors.Black;
            avaPlot.Plot.Axes.Bottom.Label.ForeColor = Colors.Black;
            avaPlot.Plot.Axes.Bottom.MajorTickStyle.Color = Colors.Black;
            avaPlot.Plot.Axes.Left.MajorTickStyle.Color = Colors.Black;
            avaPlot.Plot.Axes.Left.TickLabelStyle.ForeColor = Colors.Black;
            avaPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = Colors.Black;

            avaPlot.Plot.Axes.Bottom.MinorTickStyle.Length = 5;
            avaPlot.Plot.Axes.Bottom.MinorTickStyle.Width = 0.5f;
            avaPlot.Plot.Axes.Bottom.MinorTickStyle.Color = Colors.Transparent;
            avaPlot.Plot.Axes.Bottom.FrameLineStyle.Color = Colors.Transparent;

            avaPlot.Plot.Axes.Left.MinorTickStyle.Length = 5;
            avaPlot.Plot.Axes.Left.MinorTickStyle.Width = 0.5f;
            avaPlot.Plot.Axes.Left.MinorTickStyle.Color = Colors.Transparent;
            avaPlot.Plot.Axes.Left.FrameLineStyle.Color = Colors.Transparent;

            avaPlot.Plot.Grid.MajorLineColor = Color.FromHex("#E0E0E0");
            avaPlot.Plot.Axes.Left.Label.FontSize = 11;
           
        }
       
    }
}
