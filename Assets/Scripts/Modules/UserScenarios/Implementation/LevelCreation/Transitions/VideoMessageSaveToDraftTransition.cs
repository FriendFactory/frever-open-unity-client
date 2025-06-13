using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class VideoMessageSaveToDraftTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public VideoMessageSaveToDraftTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public override ScenarioState To => ScenarioState.ProfileExit;
        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            _levelManager.UnloadAllAssets();
            _levelManager.CleanUp();
            return base.OnRunning();
        }
    }
}