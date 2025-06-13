using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Pages.OnBoardingPage;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class SignInOverlayState : StateBase<ISignupContext>, IResolvable
    {
        [Inject] private PopupManager _popupManager;
        [Inject] private PageManager _pageManager;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private AmplitudeManager _amplitudeManager;
        
        public ITransition MoveBack;
        public ITransition MoveNext;
        public ITransition MoveNextNoVerification;
        public ITransition SignUp;
        
        public override ScenarioState Type => ScenarioState.SignInOverlay;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext, MoveNextNoVerification, SignUp }.RemoveNulls();
        
        public override void Run()
        {
            var config = new LoginPopupConfiguration(OnComplete);

            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.LOGIN_PAGE);
        }

        private async void OnComplete(LoginPopupResult result)
        {
            switch (result)
            {
                case LoginPopupResult.Next:
                    _pageManager.PageDisplayed += OnPageDisplayed;
                    await MoveNext.Run();
                    break;
                case LoginPopupResult.NextNoVerification:
                    _pageManager.PageDisplayed += OnPageDisplayed;
                    await MoveNextNoVerification.Run();
                    break;
                case LoginPopupResult.Close:
                    _popupManager.ClosePopupByType(PopupType.Login);
                    await MoveBack.Run();
                    break;
                case LoginPopupResult.Signup:
                    _pageManager.PageDisplayed += OnPageDisplayed;
                    await SignUp.Run();
                    break;
            } 
            
            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                _popupManager.ClosePopupByType(PopupType.Login);
            }
        }
    }
}