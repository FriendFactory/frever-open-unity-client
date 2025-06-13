using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Navigation.Core;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.Transitions
{
    [UsedImplicitly]
    internal sealed class SignInMoveBackTransition : TransitionBase<ICharacterCreationContext>, IResolvable
    {
        [Inject] private PageManager _pageManager;

        public override ScenarioState To => ScenarioState.SignInOverlay;
        
        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }
        
        protected override async Task OnRunning()
        {
            await base.OnRunning();
            
            var args = new OnBoardingPageArgs();
            var pageLoaded = false;
            
            _pageManager.PageDisplayed += OnPageDisplayed;
            _pageManager.MoveNext(args);

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                
                if (pageData.PageArgs != args)
                {
                    return;
                }

                pageLoaded = true;
            }

            while (!pageLoaded)
            {
                await Task.Delay(25);
            }
        }
    }
}