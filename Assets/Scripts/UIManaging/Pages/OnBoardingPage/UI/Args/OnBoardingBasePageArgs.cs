using System;
using Navigation.Core;
using UIManaging.Localization;

namespace UiManaging.Pages.OnBoardingPage.UI.Args
{
    public abstract class OnBoardingBasePageArgs : PageArgs
    {
        public string TitleText;
        public string DescriptionText;
        public Action OnContinueButtonClick;
        public Action RequestMoveBack;
        public Action DisplayStart;
        public Action<int , int> RequestMoveNext;
        public Func<bool> InputCheckerFunction;
        public bool ShowBackButton = true;
        public OnboardingServerErrorLocalization ErrorLocalization;
    }
}