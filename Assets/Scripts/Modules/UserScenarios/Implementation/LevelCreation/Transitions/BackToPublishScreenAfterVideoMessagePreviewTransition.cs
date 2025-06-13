using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class BackToPublishScreenAfterVideoMessagePreviewTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        
        public override ScenarioState To => ScenarioState.Publish;

        public BackToPublishScreenAfterVideoMessagePreviewTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        protected override Task UpdateContext()
        {
            //nothing
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            _levelManager.DeactivateAllAssets();
            return base.OnRunning();
        }
    }
}