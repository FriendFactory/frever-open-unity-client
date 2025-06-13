using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class LevelToPostRecordEditorTransition : TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public override ScenarioState To => ScenarioState.PostRecordEditor;

        public LevelToPostRecordEditorTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        protected override Task UpdateContext()
        {
            Context.LevelData = _levelManager.CurrentLevel;
            Context.LevelEditor.DraftEventData = default;
            Context.PostRecordEditor.IsPreviewMode = false;
            Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = 1;
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            var targetEvent =
                Context.LevelData.GetEventBySequenceNumber(Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber);
            _levelManager.UnloadNotUsedByEventsAssets(targetEvent);
            return base.OnRunning();
        }
    }
}