using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.VotingFeed.Transitions
{
    [UsedImplicitly]
    internal sealed class MoveBackToVotingFeedCharacterEditorTransition: TransitionBase<VotingFeedContext>, IResolvable
    {
        private readonly IAssetManager _assetManager;
        
        public override ScenarioState To => ScenarioState.VotingFeedCharacterEditor;

        public MoveBackToVotingFeedCharacterEditorTransition(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            _assetManager.UnloadAll();
            return base.OnRunning();
        }
    }
}