using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingAnyOtherWayExitState : ExitStateBase<IExitContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public override ScenarioState Type => ScenarioState.OnboardingAnyOtherWayExit;
        
        public OnboardingAnyOtherWayExitState(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }
        
        public override void Run()
        {
            _levelManager.UnloadAllAssets();
        }
    }
}