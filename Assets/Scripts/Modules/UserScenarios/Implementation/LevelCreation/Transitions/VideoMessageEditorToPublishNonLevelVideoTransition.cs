using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class VideoMessageEditorToPublishNonLevelVideoTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public VideoMessageEditorToPublishNonLevelVideoTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }
        
        public override ScenarioState To => ScenarioState.PublishFromGallery;
        
        protected override Task UpdateContext()
        {
            _levelManager.UnloadAllAssets();
            return Task.CompletedTask;
        }
    }
}