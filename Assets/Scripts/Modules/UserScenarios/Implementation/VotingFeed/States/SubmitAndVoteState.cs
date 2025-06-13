using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Core;
using TipsManagment;
using UIManaging.Pages.VotingFeed;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class SubmitAndVoteState : StateBase<VotingFeedContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public ITransition MoveNext;
        public ITransition MoveBack;
        public override ScenarioState Type => ScenarioState.SubmitAndVote;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public SubmitAndVoteState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var args = new SubmitAndVotePageArgs
            {
                MoveNext = OnMoveNext,
                MoveBack = OnMoveBack
            };
            
            _pageManager.MoveNext(args, false);
        }

        private async void OnMoveNext()
        {
            await MoveNext.Run();
        }

        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }
    }
}