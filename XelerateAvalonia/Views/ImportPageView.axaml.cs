using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Collections.Generic;
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
                AllowMultiple = true // Allow multiple files to be selected
            });

            if (files?.Count > 0)
            {
                var CurrentFileTextBlock = this.FindControl<TextBlock>("CurrentFile");
                var CurrentImageTextBlock = this.FindControl<TextBlock>("CurrentImage");

                string fileExtensions = ".xlsx,.xls,.jpg,.png,.jpeg,.tif"; // Define the allowed file extensions
                List<string> excelFiles = new List<string>();
                List<string> imageFiles = new List<string>();

                foreach (var file in files)
                {
                    string localPath = new Uri(file.Path.ToString()).LocalPath;
                    string fileExtension = Path.GetExtension(localPath)?.ToLower();

                    if (fileExtensions.Contains(fileExtension))
                    {
                        if (fileExtension == ".xlsx" || fileExtension == ".xls")
                        {
                            excelFiles.Add(localPath);
                        }
                        else if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".jpeg" || fileExtension == ".tif")
                        {
                            imageFiles.Add(localPath);
                        }
                    }
                }

                // Update CurrentFileTextBlock only if no image files are imported
                if (imageFiles.Count == 0)
                {
                    CurrentFileTextBlock.Text = string.Join(", ", excelFiles);
                }
                CurrentImageTextBlock.Text = string.Join(", ", imageFiles);
            }
        }



    }
}
