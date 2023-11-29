using DynamicData;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;
using XelerateAvalonia.Views;

namespace XelerateAvalonia.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        
        public RoutingState Router { get; } = new RoutingState();

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        // The command that navigates a user to first view model.
        public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoImage { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoImport { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoPlotting { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoDatabase { get; }




        public MainWindowViewModel()
        {
            var startingViewModel = new HomePageViewModel(this);
            Router.Navigate.Execute(startingViewModel);

            GoHome = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Home Page View");
                    return Router.Navigate.Execute(new HomePageViewModel(this));
                }
            );

            GoImage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Image View");
                    return Router.Navigate.Execute(new ImagePageViewModel(this));
                }
            );

            GoImport = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Import View");
                    return Router.Navigate.Execute(new ImportPageViewModel(this));
                }
            );

            // Add similar Console.WriteLine statements for other navigation commands...

            GoSettings = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Settings View");
                    return Router.Navigate.Execute(new SettingsPageViewModel(this));
                }
            );

            GoDatabase = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Database View");
                    return Router.Navigate.Execute(new DatabasePageViewModel(this));
                }
            );

            GoPlotting = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    Console.WriteLine("Navigating to Plotting View");
                    return Router.Navigate.Execute(new PlottingPageViewModel(this));
                }
            );



        }


    }
}
