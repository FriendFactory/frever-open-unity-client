using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class PreviousPageExitState : ExitStateBase<IExitContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly PageManager _pageManager;

        public override ScenarioState Type => ScenarioState.PreviousPageExit;
        
        public PreviousPageExitState(ILevelManager levelManager, VideoManager videoManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _pageManager = pageManager;
        }

        public override void Run()
        {
            _levelManager.StopCurrentPlayMode();
            _levelManager.CancelLoadingCurrentAssets();
            _levelManager.UnloadAllAssets();
            _levelManager.CleanUp();

            if (_pageManager.CurrentPage.Id == Context.OpenedFromPage) 
            {
                return; //do not reload the page
            }
            
            _pageManager.MoveBackTo(Context.OpenedFromPage);
        }
    }
}