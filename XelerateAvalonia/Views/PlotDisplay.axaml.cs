using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot.Avalonia;
using XelerateAvalonia.Models;
using System.Collections.Generic;

namespace XelerateAvalonia.Views
{
    public partial class PlotDisplay : UserControl
    {
        public PlotDisplay(double [] Depth, Dictionary<string, double[]> Elements)
        {
            InitializeComponent();

            // Get a reference to the AvaPlot control
            AvaPlot avaPlot1 = this.Find<AvaPlot>("AvaPlot1");

            // Iterate over each element in the Elements dictionary
            foreach (var element in Elements)
            {
                // Get the element name and its corresponding data array
                string elementName = element.Key;
                double[] Concentration = element.Value; // Assuming dataY represents the position values

                // Add a scatter plot for the current element
                avaPlot1.Plot.Title(elementName);
                avaPlot1.Plot.YLabel("Depth [mm]");
                avaPlot1.Plot.Add.Scatter(Concentration,Depth);
            }

            
            // Refresh the plot to reflect the changes
            avaPlot1.Refresh();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
