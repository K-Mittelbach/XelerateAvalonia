using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using XelerateAvalonia.ViewModels;

namespace XelerateAvalonia.Views
{
    public partial class PlottingPageView : ReactiveUserControl<PlottingPageViewModel>
    {
        public PlottingPageView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }

    }
}
