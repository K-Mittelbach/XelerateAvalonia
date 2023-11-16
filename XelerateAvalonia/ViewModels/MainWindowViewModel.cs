using DynamicData;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;
using XelerateAvalonia.Auxilaries;
using XelerateAvalonia.Views;

namespace XelerateAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        
        private ViewModelBase _navigationContent;
        private INavigationService _navigationService;
        public ICommand HomeNavigationCommand { get; }
        public ICommand ImportNavigationCommand { get; }
        public ICommand PlottingNavigationCommand { get; }
        public ICommand ImageNavigationCommand { get; }
        public ICommand DatabaseNavigationCommand { get; }

        public ICommand SettingsNavigationCommand { get; }

        public ViewModelBase NavigationContent
        {
            get => this._navigationContent;
            set => this.RaiseAndSetIfChanged(ref this._navigationContent, value);
        }

        public MainWindowViewModel(INavigationService navigationService)
        {

            this._navigationService = navigationService;

            this.HomeNavigationCommand = ReactiveCommand.Create(this.OnHomeNavigationClick);
            this.ImportNavigationCommand = ReactiveCommand.Create(this.OnImportNavigationClick);
            this.PlottingNavigationCommand = ReactiveCommand.Create(this.OnPlottingNavigationClick);
            this.ImageNavigationCommand = ReactiveCommand.Create(this.OnImageNavigationClick);
            this.DatabaseNavigationCommand = ReactiveCommand.Create(this.OnDatabaseNavigationClick);
            this.SettingsNavigationCommand = ReactiveCommand.Create(this.OnSettingsNavigationClick);

            this._navigationService
               .OnNavigation
               .Subscribe(x => this.NavigationContent = x);

            this.OnHomeNavigationClick();
        }

        private void OnHomeNavigationClick()
        {
            this._navigationService.Navigate("HomePageViewModel");
        }

        private void OnImportNavigationClick()
        {
            Console.WriteLine("did it");
            this._navigationService.Navigate("ImportPageViewModel");
        }

        private void OnPlottingNavigationClick()
        {
            this._navigationService.Navigate("PlottingPageViewModel");
        }

        private void OnImageNavigationClick()
        {
            this._navigationService.Navigate("ImagePageViewModel");
        }
        private void OnDatabaseNavigationClick()
        {
            this._navigationService.Navigate("DatabasePageViewModel");
        }

        private void OnSettingsNavigationClick()
        {
            this._navigationService.Navigate("SettingsPageViewModel");
        }





    }
}
