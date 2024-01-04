using System;
using ReactiveUI;
using System.Reactive;
using XelerateAvalonia.Views;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;

namespace XelerateAvalonia.ViewModels
{
    public class HomePageViewModel : ViewModelBase, IRoutableViewModel
    {
        // Variables Declaration
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = "Home";

        private readonly ISessionContext _sessionContext;

        

        // Properties to bind to in the view
        public string ProjectName => _sessionContext.ProjectName;
        public string ProjectPath => _sessionContext.ProjectPath;

        public DateTime DateCreated => _sessionContext.CreatedIn;

        public ReactiveCommand<Unit, IRoutableViewModel> GoImport { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoImage { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoPlotting { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoDatabase { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoFileCreation { get; }

        public RoutingState Router { get; } = new RoutingState();

        // View Navigation
        public HomePageViewModel(IScreen screen, ISessionContext sessionContext)
        {
            HostScreen = screen;
            _sessionContext = sessionContext;
           

            this.RaisePropertyChanged(nameof(ProjectName));
            this.RaisePropertyChanged(nameof(ProjectPath));
            this.RaisePropertyChanged(nameof(DateCreated));

            // Define and initialize the GoImport command in the constructor
            GoImport = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new ImportPageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoImage command in the constructor
            GoImage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new ImagePageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoPlotting command in the constructor
            GoPlotting = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new PlottingPageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoDatabase command in the constructor
            GoDatabase = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new DatabasePageViewModel(HostScreen, sessionContext));
                }
            );
            // Define and initialize the GoSettings command in the constructor
            GoSettings = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new SettingsPageViewModel(HostScreen, sessionContext));
                }
            );

            // Define and initialize the GoFileCreation command in the constructor
            GoFileCreation = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    // Pass the existing session context to the new view model
                    return HostScreen.Router.NavigateAndReset.Execute(new FileCreationPageViewModel(HostScreen, SessionContext));
                }
            );
        }
    }
}
