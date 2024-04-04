using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;
using XelerateAvalonia.Models;
using System.IO;

namespace XelerateAvalonia.Views
{
    public partial class ElementSelectionDisplay : UserControl
    {
        private ItemsRepeater _itemsRepeater;
        private ObservableCollection<NaturalElements> _elements;
        private string _validCoreName;
        private List<string> _uncheckedElementNames = new List<string>();
        public string DatabasePath { get; private set; }

        public ElementSelectionDisplay(string databasePath, string dataSetName)
        {
            InitializeComponent();

            DatabasePath = databasePath;

            // Find the ItemsRepeater control in the UserControl
            _itemsRepeater = this.FindControl<ItemsRepeater>("ItemsRepeater");

            // Replace spaces and hyphens with underscores
            string validCoreName = dataSetName.Replace(" ", "_").Replace("-", "_");

            // Check if the validCoreName contains numbers
            if (validCoreName.Any(char.IsDigit))
            {
                // If it contains digits, add "Core_" prefix if not already present
                if (!validCoreName.StartsWith("Core_"))
                {
                    validCoreName = "Core_" + validCoreName;
                }
            }
            // Get the elements from the database
            ObservableCollection<NaturalElements> elements = DBAccess.GetAllElements(validCoreName, databasePath);

            // Set the IsChecked property of each element to "True"
            foreach (var element in elements)
            {
                element.IsChecked = "True";
            }

            // Set the ItemsSource of the ItemsRepeater to your elementList
            _itemsRepeater.ItemsSource = elements;
            _validCoreName = validCoreName;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void ApplySelection_Clicked(object sender, RoutedEventArgs args)
        {
            // Update _elements collection with user selections
            UpdateElementsSelection();

            // Apply the selection
            await ApplySelection();

            var window = this.VisualRoot as Window;

            window.Close();
        }

        private void UpdateElementsSelection()
        {
            // Get the items from the ItemsRepeater and update their selections
            foreach (var item in _itemsRepeater.ItemsSource.OfType<NaturalElements>())
            {
                // Update the IsChecked property based on the current selection status
                item.IsChecked = item.IsChecked == "True" ? "True" : "False";

                // If the item is checked, add its name to the list
                if (item.IsChecked == "False")
                {
                    _uncheckedElementNames.Add(item.Name);
                }
            }
        }

        private async Task ApplySelection()
        {
            // Update and delete columns based on the selected elements
            string dataSetName = _validCoreName;
            DBAccess.UpdateAndDeleteColumns(dataSetName, _uncheckedElementNames, DatabasePath);
        }
    }
}
