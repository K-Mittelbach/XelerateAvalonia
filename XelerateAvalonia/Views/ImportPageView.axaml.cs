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
            var topLevel = TopLevel.GetTopLevel(this);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open XLS or image files",
                AllowMultiple = false
            });

            if (files?.Count > 0)
            {
                var CurrentFileTextBlock = this.FindControl<TextBlock>("CurrentFile");
                var CurrentImageTextBlock = this.FindControl<TextBlock>("CurrentImage");

                string localPath = new Uri(files[0].Path.ToString()).LocalPath;

                // Check file extension to distinguish between Excel and image files
                string fileExtension = Path.GetExtension(localPath)?.ToLower();

                if (fileExtension == ".xlsx" || fileExtension == ".xls")
                {
                    // It's an Excel file
                    CurrentFileTextBlock.Text = localPath;
                    
                }
                else if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".jpeg" || fileExtension == ".tif")
                {
                    // It's an image file
                    CurrentImageTextBlock.Text = localPath;
                    
                }
                // If it's not an Excel or image file, do nothing
                
            }
        }


    }
}
