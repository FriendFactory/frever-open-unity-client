using Common;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Localization;
using UiManaging.Pages.OnBoardingPage.UI.Args;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingBirthdayState : StateBase<ISignupContext>, IResolvable
    {
        public ITransition MoveNext;
        public ITransition MoveBack;
        
        private readonly PageManager _pageManager;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly ISignUpService _signUpService;
        private readonly OnBoardingLocalization _localization;

        public OnboardingBirthdayState(PageManager pageManager, AmplitudeManager amplitudeManager, ISignUpService signUpService, 
            OnBoardingLocalization localization)
        {
            _pageManager = pageManager;
            _amplitudeManager = amplitudeManager;
            _signUpService = signUpService;
            _localization = localization;
        }
        
        public override ScenarioState Type => ScenarioState.SignUpBirthday;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext }.RemoveNulls();

        
        public override void Run()
        {
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.ENTER_AGE);
            
            var args = new OnBoardingScrollSelectorPageArgs
            {
                TitleText = _localization.BirthdateTitle,
                DescriptionText = string.Format(_localization.BirthdateDescription, 
                                                Constants.TERMS_OF_USE_LINK, 
                                                Constants.Onboarding.HIGHLIGHT_COLOR_STRING, 
                                                Constants.PRIVACY_POLICY_LINK, 
                                                Constants.Onboarding.HIGHLIGHT_COLOR_STRING),
                RequestMoveNext = OnMoveNextRequested,
                RequestMoveBack = OnMoveBackRequested,
            };
            _pageManager.MoveNext(args);
        }

        private async void OnMoveNextRequested(int month, int year)
        {
            var age = _signUpService.SetBirthdayAndCalculateAge(month, year);
            await _amplitudeManager.SetupAmplitude(age);
            await MoveNext.Run();
        }

        private async void OnMoveBackRequested()
        {
            await MoveBack.Run();
        }
    }
}