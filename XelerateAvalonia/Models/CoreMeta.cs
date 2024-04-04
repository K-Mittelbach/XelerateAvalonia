using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XelerateAvalonia.Services;
using XelerateAvalonia.Views;

namespace XelerateAvalonia.Models
{
    public class CoreMeta
    {
        public string Name { get; set; }

        public UniqueId ID {get; set;}

        public string DeviceUsed { get; set; }

        public string InputSource { get; set; }

        public float MeasuredTime { get; set; }

        public float Voltage { get; set; }

        public float Current { get; set; }

        public float Size { get; set; }

        public DateOnly Uploaded { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteFileCommand { get; }

        public ReactiveCommand<Unit, Unit> DisplayElementSelectionCommand { get; }

        // Reference to the parent collection (ImageList)
        private ObservableCollection<CoreMeta> parentCollection;



        // Defining the Meta data for each dataset -> should be loaded from database
        public CoreMeta(string name, UniqueId id , string deviceUsed, string inputSource, float measuredTime, float voltage, float current, float size, DateOnly uploaded, ObservableCollection<CoreMeta> parentCollection, string databasePath)
        {
            Name = name;
            ID = id;
            DeviceUsed = deviceUsed;
            InputSource = inputSource;
            MeasuredTime = measuredTime;
            Voltage = voltage;
            Current = current;
            Size = size;
            Uploaded = uploaded;

            // Set the reference to the parent collection
            this.parentCollection = parentCollection;

            // Create the ReactiveCommand with a delegate
            DeleteFileCommand = ReactiveCommand.Create(() => DeleteFile(databasePath));

            DisplayElementSelectionCommand = ReactiveCommand.Create(() => DisplayElementSelection(databasePath));

        }

        public void DeleteFile(string databasePath)
        {
            // Notify the parent collection to remove the current instance
            parentCollection.Remove(this);

            // Replace spaces and hyphens with underscores
            string validCoreName = this.Name.Replace(" ", "_").Replace("-", "_");

            // Check if the validCoreName contains numbers
            if (validCoreName.Any(char.IsDigit))
            {
                // If it contains digits, add "Core_" prefix if not already present
                if (!validCoreName.StartsWith("Core_"))
                {
                    validCoreName = "Core_" + validCoreName;
                }
            }

            // Remove the Core meta data from the database
            DBAccess.RemoveCoreMeta(validCoreName, databasePath);
        }

        //Displaying a window with the image and adjustable ROI
        public void DisplayElementSelection(string databasePath)
        {
      
            Window window = new Window
            {
                Title = "Element selection: " + this.Name,
                Width = 550,
                Height = 400,
                Content = new ElementSelectionDisplay(databasePath,this.Name),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            // Show the window
            window.Show();

        }

    }
}
