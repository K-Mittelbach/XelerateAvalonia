using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
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

        // WIP -> MUST FOLLOW MVVM PATTERN IN FUTURE
        // Dialog Service Implementation 
        public async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
        {
            var button = (sender as Button)!;
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open XLS files",
                AllowMultiple = false
            });

            if (files?.Count > 0)
            {
                // Access the TextBox by its name
                var CurrentFileTextBlock = this.FindControl<TextBlock>("CurrentFile");
                // LOOP all files
                string localPath = new Uri(files[0].Path.ToString()).LocalPath;

                //  Binding to Property in ViewModel neccessary
                
                CurrentFileTextBlock.Text = localPath;
                
            }
        }


    }
}
