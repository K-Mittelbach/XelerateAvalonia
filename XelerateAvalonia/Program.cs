using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Projektanker;

using XelerateAvalonia.ViewModels;
using XelerateAvalonia.Auxilaries;
using XelerateAvalonia.Views;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace XelerateAvalonia

{
    class Program
    {
        public static IServiceLocator ServiceLocator = new ServiceLocator();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            ServiceLocator.ServiceCollection.AddSingleton<INavigationService, NavigationService>();
            

            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, MainWindowViewModel>("MainWindowViewModel");
            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, HomePageViewModel>("HomePageViewModel");
            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, ImportPageViewModel>("ImportPageViewModel");
            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, PlottingPageViewModel>("PlottingPageViewModel");
            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, DatabasePageViewModel>("DatabasePageViewModel");
            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, ImagePageViewModel>("ImagePageViewModel");
            ServiceLocator.ServiceCollection.AddScoped<ViewModelBase, SettingsPageViewModel>("SettingsPageViewModel");

          

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            IconProvider.Current
            .Register<FontAwesomeIconProvider>();
            
            return AppBuilder.Configure<App>()
                 .UsePlatformDetect()
                 .LogToTrace()
                 .UseReactiveUI();

        }
      
    }
}