using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Pages.OnBoardingPage.UI.Pages;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class FollowContactsState: StateBase<OnboardingContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public ITransition MoveNext;
        
        public override ScenarioState Type => ScenarioState.OnboardingContactsFollow;
        public override ITransition[] Transitions => new[] { MoveNext }.RemoveNulls();

        public FollowContactsState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var args = new OnboardingContactsPageArgs(OnContinueButtonClick);
            
            _pageManager.MoveNext(args);
        }
        
        private async void OnContinueButtonClick()
        {
            await MoveNext.Run();
        }
    }
}