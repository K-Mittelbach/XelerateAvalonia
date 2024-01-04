using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XelerateAvalonia.Models
{
    public interface ISessionContext : INotifyPropertyChanged
    {
        string ProjectName { get; set; }

        string ProjectPath { get; set; }

        int NumberOfDatasets { get; set; }

        int NumberOfImages { get; set; }

        DateTime CreatedIn { get; set; }
    }
}
