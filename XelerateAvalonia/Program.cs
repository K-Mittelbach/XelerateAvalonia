using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Projektanker;
using XelerateAvalonia.ViewModels;
using XelerateAvalonia.Views;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Splat;
using System.Reflection;
using ReactiveUI;

namespace XelerateAvalonia

{
    class Program
    {
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {                    

            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            IconProvider.Current
            .Register<FontAwesomeIconProvider>();

            return AppBuilder.Configure<App>()
                 .UsePlatformDetect()
                 .LogToTrace()
                 .UseReactiveUI();

        }

    }
}