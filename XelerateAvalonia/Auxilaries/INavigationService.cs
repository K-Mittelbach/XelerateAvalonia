using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using XelerateAvalonia.ViewModels;

namespace XelerateAvalonia.Auxilaries
{
    public interface INavigationService
    {
        Subject<ViewModelBase> OnNavigation { get; }


        Task Navigate(string navigationContext);


    }
}
