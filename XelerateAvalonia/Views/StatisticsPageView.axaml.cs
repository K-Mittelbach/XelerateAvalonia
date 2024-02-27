using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using XelerateAvalonia.ViewModels;

namespace XelerateAvalonia.Views
{
    public partial class StatisticsPageView : ReactiveUserControl<StatisticsPageViewModel>
    {
        public StatisticsPageView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
