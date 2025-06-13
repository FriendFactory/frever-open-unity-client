using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation;

namespace Modules.UserScenarios.Implementation.VotingFeed.Transitions
{
    [UsedImplicitly]
    internal class PostRecordEditorToSubmitAndVoteTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public PostRecordEditorToSubmitAndVoteTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }
        public override ScenarioState To => Context.Task.IsAvailableForVoting ? ScenarioState.SubmitAndVote : ScenarioState.VotingDone;

        protected override Task UpdateContext()
        {
            Context.LevelData = _levelManager.CurrentLevel;
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            _levelManager.UnloadAllAssets();
            return base.OnRunning();
        }
    }
}