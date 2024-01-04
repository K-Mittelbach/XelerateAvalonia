using System;
using System.ComponentModel;

namespace XelerateAvalonia.Models
{
    public class SessionContext : INotifyPropertyChanged, ISessionContext
    {
        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        private string _projectPath;
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private int _numberOfDatasets;
        public int NumberOfDatasets
        {
            get => _numberOfDatasets;
            set
            {
                if (_numberOfDatasets != value)
                {
                    _numberOfDatasets = value;
                    OnPropertyChanged(nameof(NumberOfDatasets));
                }
            }
        }

        private int _numberOfImages;
        public int NumberOfImages
        {
            get => _numberOfImages;
            set
            {
                if (_numberOfImages != value)
                {
                    _numberOfImages = value;
                    OnPropertyChanged(nameof(NumberOfImages));
                }
            }
        }

        private DateTime _createdIn;

        public DateTime CreatedIn
        {
            get => _createdIn;
            set
            {
                if (_createdIn != value)
                {
                    _createdIn = value;
                    OnPropertyChanged(nameof(CreatedIn));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
