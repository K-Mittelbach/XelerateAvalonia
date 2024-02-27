using DynamicData;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;
using XelerateAvalonia.Views;

namespace XelerateAvalonia.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        
        public RoutingState Router { get; } = new RoutingState();

        public ISessionContext SessionContext { get; set; } = new SessionContext(); // Initialize SessionContext

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        // The command that navigates a user to first view model.
        public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoStatistics { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoImport { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoPlotting { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoDatabase { get; }




        public MainWindowViewModel()
        {
            
            
            var startingViewModel = new StartPageViewModel(this, SessionContext);
            Router.Navigate.Execute(startingViewModel);

            GoHome = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Home Page View");
                    return Router.Navigate.Execute(new HomePageViewModel(this,SessionContext));
                }
            );

            GoStatistics = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Statastics View");
                    return Router.Navigate.Execute(new StatisticsPageViewModel(this, SessionContext));
                }
            );

            GoImport = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Import View");
                    return Router.Navigate.Execute(new ImportPageViewModel(this, SessionContext));
                }
            );

            GoSettings = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Settings View");
                    return Router.Navigate.Execute(new SettingsPageViewModel(this, SessionContext));
                }
            );

            GoDatabase = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Database View");
                    return Router.Navigate.Execute(new DatabasePageViewModel(this, SessionContext));
                }
            );

            GoPlotting = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Plotting View");
                    return Router.Navigate.Execute(new PlottingPageViewModel(this, SessionContext));
                }
            );



        }


    }
}
