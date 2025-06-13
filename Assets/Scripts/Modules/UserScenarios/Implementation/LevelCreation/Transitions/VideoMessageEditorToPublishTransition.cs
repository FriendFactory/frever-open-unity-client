using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class VideoMessageEditorToPublishTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public VideoMessageEditorToPublishTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public override ScenarioState To => ScenarioState.Publish;
        
        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            _levelManager.StopCurrentPlayMode();
            _levelManager.CleanUp();
            _levelManager.DeactivateAllAssets();
            return base.OnRunning();
        }
    }
}