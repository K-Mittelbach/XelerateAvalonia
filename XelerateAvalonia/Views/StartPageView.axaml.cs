using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.IO;
using XelerateAvalonia.ViewModels;

namespace XelerateAvalonia.Views
{
    public partial class StartPageView: ReactiveUserControl<StartPageViewModel>
    {
        public StartPageView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }

        public async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
        {
            var button = (sender as Button)!;
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open a Xelerate project folder",
                AllowMultiple = false
            });

        }

    }
}
