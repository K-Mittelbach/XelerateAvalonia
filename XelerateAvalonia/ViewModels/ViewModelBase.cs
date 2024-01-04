using ReactiveUI;
using XelerateAvalonia.Models;

namespace XelerateAvalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        protected ISessionContext SessionContext { get; } = new SessionContext();

        // Method to update session context properties
        protected void UpdateSessionContext(string projectName, string projectPath)
        {
            SessionContext.ProjectName = projectName;
            SessionContext.ProjectPath = projectPath;
        }

    }
}