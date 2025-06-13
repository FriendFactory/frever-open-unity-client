using Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class ProfileExitState : ExitStateBase<IExitContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly PageManager _pageManager;

        public override ScenarioState Type => ScenarioState.ProfileExit;
        
        public ProfileExitState(ILevelManager levelManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _pageManager = pageManager;
        }

        public override void Run()
        {
            _levelManager.UnloadAllAssets();
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(), false);
        }
    }
}