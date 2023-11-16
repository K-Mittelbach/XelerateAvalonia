using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using XelerateAvalonia.ViewModels;
using XelerateAvalonia.Views;
using XelerateAvalonia.Auxilaries;

namespace XelerateAvalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var viewModel = Program.ServiceLocator.ServiceProvider.GetService<ViewModelBase>("MainWindowViewModel");

                desktop.MainWindow = new MainWindow()
                {
                    DataContext = viewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}