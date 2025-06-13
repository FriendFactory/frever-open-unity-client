using JetBrains.Annotations;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UiManaging.Pages.OnBoardingPage.UI.Args;

namespace Modules.UserScenarios.Implementation.Onboarding.Transitions
{
    [UsedImplicitly]
    internal sealed class SignUpExitState : ExitStateBase<ISignupContext>, IResolvable
    {
        private readonly ISignUpService _signUpService;
        private readonly PageManager _pageManager;

        public SignUpExitState(ISignUpService signUpService, PageManager pageManager)
        {
            _signUpService = signUpService;
            _pageManager = pageManager;
        }
        
        public override ScenarioState Type => ScenarioState.SignUpExitToLandingPage;
        public override void Run()
        {
            _signUpService.CleanUp();
            _pageManager.MoveNext(new OnBoardingPageArgs(), false);
        }
    }
}