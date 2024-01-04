using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;
using XelerateAvalonia.Models;
using XelerateAvalonia.Services;

namespace XelerateAvalonia.ViewModels
{
	public class FileCreationPageViewModel :  ViewModelBase, IRoutableViewModel
	{
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; } = "Create a new project";

        public string LocationPath { get; set; } = " ";

        public string ProjectName { get; set; } = " ";

       
        public ReactiveCommand<Unit, IRoutableViewModel> GoStart { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> CreatedProject { get; }

        private ISessionContext _sessionContext;

       

        public FileCreationPageViewModel(IScreen screen, ISessionContext sessionContext)
        {
            HostScreen = screen; 
            _sessionContext = sessionContext;
           

            // Define and initialize the GoImport command in the constructor
            GoStart = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return HostScreen.Router.NavigateAndReset.Execute(new StartPageViewModel(HostScreen, sessionContext));
                }
            );

            CreatedProject = ReactiveCommand.CreateFromObservable(
               () =>
               {     // Update the session context with the provided values
                   _sessionContext.ProjectName = ProjectName;
                   _sessionContext.ProjectPath = LocationPath;
                   _sessionContext.CreatedIn = DateTime.Now;
                   return HostScreen.Router.NavigateAndReset.Execute(new HomePageViewModel(HostScreen,sessionContext));
                  
               }
           );
           
        }

    }
}