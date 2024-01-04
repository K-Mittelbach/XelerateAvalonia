using System;
using System.Reactive;
using ReactiveUI;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;

namespace XelerateAvalonia.ViewModels
{
    public class StartPageViewModel : ViewModelBase, IRoutableViewModel
    {
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; } = "Start";

        public RoutingState Router { get; } = new RoutingState();

        public ReactiveCommand<Unit, IRoutableViewModel> GoFileCreation { get; }

        private readonly ISessionContext _sessionContext;

        
        public StartPageViewModel(IScreen screen, ISessionContext sessionContext)
        {
            HostScreen = screen;
            _sessionContext = sessionContext;
            


            // Define and initialize the GoFileCreation command in the constructor
            GoFileCreation = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    // Pass the existing session context to the new view model
                    return HostScreen.Router.NavigateAndReset.Execute(new FileCreationPageViewModel(HostScreen, sessionContext));
                }
            );
        }
    }
}
