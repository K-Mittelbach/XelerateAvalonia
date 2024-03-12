using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot.Avalonia;
using XelerateAvalonia.Models;
using System.Collections.Generic;
using ScottPlot;
using XelerateAvalonia.Auxiliaries;
using System.Linq;

namespace XelerateAvalonia.Views
{
    public partial class PlotDisplay : UserControl
    {
        public PlotDisplay(double[] Depth, Dictionary<string, double[]> Elements, ImageCore SelectedImageitem, bool IsDarkModeEnabled)
        {
            InitializeComponent();

            Avalonia.Controls.Image _imageControl = this.FindControl<Avalonia.Controls.Image>("ImageControl");
            
            Avalonia.Controls.Grid AvaPlotsGrid = this.FindControl<Avalonia.Controls.Grid>("AvaPlotsGrid");
            
            // Create a list to store AvaPlot instances
            List<AvaPlot> avaPlots = new List<AvaPlot>();

            // Convert depth from millimeters to meters
            Depth = Depth.Select(d => d / 1000.0).ToArray();

            // Create a new AvaPlot object
            AvaPlot avaImagePlot = new AvaPlot();
            // add a rectangle by specifying points
            avaImagePlot.Plot.Add.Rectangle(Depth.Min(), Depth.Max(), Depth.Min(), Depth.Max());
            avaImagePlot.Plot.Title("Cr");
            avaImagePlot.Width = 350;
            avaImagePlot.Height = 800;
            avaImagePlot.Plot.HideGrid();
            avaImagePlot.Plot.YLabel("Depth [mm]");
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

            // Create AvaPlot objects dynamically and add them to the avaPlots list
            foreach (var element in Elements)
            {
                string elementName = element.Key;
                double[] Concentration = element.Value;

                // Create a new AvaPlot object
                AvaPlot avaPlot = new AvaPlot();
                avaPlot.Plot.YLabel("Depth [mm]");
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
            
            // Now you can access each AvaPlot instance by its index in the avaPlots list
            for (int i = 0; i < avaPlots.Count; i++)
            {
                AvaPlot avaPlot = avaPlots[i];
                Grid.SetColumn(avaPlot, i);
                Grid.SetRow(avaPlot, 0);
                AvaPlotsGrid.Children.Add(avaPlot);
                avaPlot.Width = 350; 
                avaPlot.Height = 800;
                
            }
            _imageControl.Source = ImageTransformations.ConvertToBitmap(ImageTransformations.LoadImageFromBytes(SelectedImageitem.BlobROI));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void ApplyDarkMode(AvaPlot avaPlot)
        {
            // Apply dark mode to the specific AvaPlot or its elements
            avaPlot.Plot.Style.Background(figure: Color.FromHex("#111619"), data: Color.FromHex("#0b3049"));
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

            avaPlot.Plot.Style.ColorGrids(Color.FromHex("#0e3d54"));
            avaPlot.Plot.Axes.Left.Label.FontSize = 11;
           
        }

        private void ApplyStandardMode(AvaPlot avaPlot)
        {
            // Apply standard mode to the specific AvaPlot or its elements
            avaPlot.Plot.Style.Background(figure: Color.FromHex("#D3D3D3"), data: Color.FromHex("#FFFFFF"));
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

            avaPlot.Plot.Style.ColorGrids(Color.FromHex("#E0E0E0"));
            avaPlot.Plot.Axes.Left.Label.FontSize = 11;
           
        }

    }
}
