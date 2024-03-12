using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ScottPlot.Avalonia;
using System;
using System.Collections.Generic;
using XelerateAvalonia.ViewModels;

namespace XelerateAvalonia.Views
{
    public partial class StatisticsPageView : ReactiveUserControl<StatisticsPageViewModel>
    {
        public StatisticsPageView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                // Get ViewModel
                var viewModel = (StatisticsPageViewModel)this.DataContext;

                // Subscribe to NewPlotRequested event
                // Subscribe to NewPlotRequested event
                viewModel.NewPlotRequested += (sender, args) =>
                {
                    // Find the Grid in the Visual Tree
                    var plottingGrid = this.FindControl<Grid>("PlottingGrid");

                    // Clear any existing plot in the Grid
                    plottingGrid.Children.Clear();

                    // Add the new plot to the Grid
                    plottingGrid.Children.Add(viewModel.CurrentPlot);
                };
            });
        }
        private Grid PlottingArea => this.FindControl<Grid>("PlottingArea");

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }



    }

}
