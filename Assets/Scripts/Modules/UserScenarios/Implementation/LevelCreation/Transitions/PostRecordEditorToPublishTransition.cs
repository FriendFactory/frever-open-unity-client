using System.Threading.Tasks;
using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal class PostRecordEditorToPublishTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly IBridge _bridge;

        public PostRecordEditorToPublishTransition(ILevelManager levelManager, IBridge bridge)
        {
            _levelManager = levelManager;
            _bridge = bridge;
        }
        
        public override ScenarioState To => ScenarioState.Publish;

        protected override Task UpdateContext()
        {
            Context.PostRecordEditor.ShowPageLoadingPopup = false;
            Context.LevelData = _levelManager.CurrentLevel;
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            var firstEvent = _levelManager.CurrentLevel.GetFirstEvent();
            _levelManager.UnloadNotUsedByEventsAssets(firstEvent);
            if(Context.SocialActionId != null) _bridge.MarkActionAsComplete(Context.RecommendationId, Context.SocialActionId.Value);
            _levelManager.DeactivateAllAssets();
            return base.OnRunning();
        }
    }
}