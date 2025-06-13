using Extensions;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    internal sealed class FollowStarCreatorsState : StateBase<OnboardingContext>, IResolvable
    {
        private readonly PageManager _pageManager;

        public ITransition MoveNext;


        public FollowStarCreatorsState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override ScenarioState Type => ScenarioState.StarCreatorFollow;
        public override ITransition[] Transitions => new[] { MoveNext }.RemoveNulls();

        public override void Run()
        {
            var args = new OnboardingStarCreatorsPageArgs
            {
                OnContinueButtonClick = OnContinueButtonClick,
            };
            _pageManager.MoveNext(args);
        }
        
        private async void OnContinueButtonClick()
        {
            await MoveNext.Run();
        }
    }
}