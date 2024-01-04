using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using XelerateAvalonia.ViewModels;
using XelerateAvalonia.Services;

namespace XelerateAvalonia.Views
{
    public partial class FileCreationPageView : ReactiveUserControl<FileCreationPageViewModel>
    {
        public FileCreationPageView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
           
        }


        // Open a  Xelerate Project and initalize a connection to a SQlite Database file if existing 
        public async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
        {
            var button = (sender as Button)!;
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open a Xelerate project folder",
                AllowMultiple = false
            });

            if (folder?.Count > 0)
            {
                // Access the TextBox by its name
                var locationTextBox = this.FindControl<TextBox>("Location");

                string localPath = new Uri(folder[0].Path.ToString()).LocalPath;

                // Set the text value of the TextBox to the selected folder path
                locationTextBox.Text = localPath;

            }
        }

        // Create a new Xelerate Project and initalize a connection to a SQlite Database file 

        public async void CreateProjectFolderAsync(object sender, RoutedEventArgs args)
        {
            var button = (sender as Button)!;
            var projectNameTextBox = this.FindControl<TextBox>("ProjectName");
            var locationTextBox = this.FindControl<TextBox>("Location");

            string projectName = string.IsNullOrEmpty(projectNameTextBox.Text) ? projectNameTextBox.Text : projectNameTextBox.Text.Trim();
            string location = string.IsNullOrEmpty(locationTextBox.Text) ? locationTextBox.Text : locationTextBox.Text.Trim();

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(location))
            {
                Console.WriteLine("Please enter both project name and location.");
                return;
            }

            string projectFolderPath = System.IO.Path.Combine(location, projectName);
            string databasePath = System.IO.Path.Combine(projectFolderPath, "database.db");

            try
            {
                if (!System.IO.Directory.Exists(projectFolderPath))
                {
                    System.IO.Directory.CreateDirectory(projectFolderPath);
                    Console.WriteLine($"Project folder created at: {projectFolderPath}");

                    // Create a connection to the SQLite database using the full path
                    var connectionFactory = new ConnectionFactory(databasePath);
                }
                else
                {
                    Console.WriteLine($"Project folder already exists at: {projectFolderPath}");
                }
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine($"Error creating project folder: {ex.Message}");
            }
        }

    }
}
