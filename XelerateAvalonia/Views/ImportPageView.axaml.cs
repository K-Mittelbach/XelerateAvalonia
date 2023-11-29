using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using XelerateAvalonia.ViewModels;

namespace XelerateAvalonia.Views
{
    public partial class ImportPageView : ReactiveUserControl<ImportPageViewModel>
    { 
        public ImportPageView()
        {
           
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }

    }
}
