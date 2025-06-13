using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class HomePageExitState : ExitStateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly PageManager _pageManager;

        public HomePageExitState(ILevelManager levelManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _pageManager = pageManager;
        }

        public override ScenarioState Type => ScenarioState.HomePageExit;
        
        public override void Run()
        {
            _levelManager.UnloadAllAssets();
            var args = new HomePageArgs();
            _pageManager.MoveNext(args);
        }
    }
}